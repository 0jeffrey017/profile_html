/*
 * Crescent_Moon / 非同期 FSM のコア処理（抜粋）
 * 遷移要求の登録、フィールド、補助メソッド、using・namespace は省略。
 * TransitionRequest は完了・例外・キャンセルを待機者へ通知する要求オブジェクト。
 */

private async UniTask ProcessTransitionQueueAsync(CancellationToken token)
{
    try
    {
        // 要求を一つずつ処理し、非同期の状態遷移が重なるのを防ぐ。
        while (_transitionQueue.Count > 0)
        {
            token.ThrowIfCancellationRequested();

            TransitionRequest request = _transitionQueue.Dequeue();
            _activeRequest = request;
            if (_transitionQueue.Count == 0)
            {
                _lastQueuedRequest = null;
            }

            try
            {
                // 終了処理と開始処理が完了してから、待機者へ完了を通知。
                await TransitionTo(request.State, token);
                request.Complete();
            }
            catch (OperationCanceledException)
            {
                // 中断時は処理中・待機中の要求をまとめてキャンセル。
                request.Cancel(token);
                CancelPendingRequests(token);
                throw;
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                InvalidOperationException transitionException = new(
                    $"[GameStateMachine] Transition to {request.State} failed.",
                    exception);
                request.Fail(transitionException);
                CancelPendingRequests(token);
                throw transitionException;
            }
            finally
            {
                _activeRequest = null;
            }
        }
    }
    finally
    {
        _isProcessingQueue = false;

        if (_transitionQueue.Count > 0 && !token.IsCancellationRequested)
        {
            _isProcessingQueue = true;
            ProcessTransitionQueueAsync(token).ForgetSafely();
        }
    }
}

private async UniTask TransitionTo(EGameState nextState, CancellationToken token)
{
    // 遷移中はゲームプレイを停止し、旧状態の終了を待つ。
    _pauseService.SetPaused(true);

    if (_currentState != null)
    {
        await _currentState.OnExit(token);
    }

    _currentState = _stateFactory.Create(nextState);
    _currentStateType = nextState;

    // 新状態の初期化・演出が完了してからプレイ可能な状態だけ再開。
    await _currentState.OnEnter(token);
    _pauseService.SetPaused(nextState is not EGameState.MainGame and not EGameState.Tutorial);
}
