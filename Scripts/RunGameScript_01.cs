/// <summary>
/// ゲームの状態を管理する
/// </summary>
public static class GameStateMachine
{
    private static CancellationTokenSource _tokenSource;
    private static CancellationToken _cancellationToken;
    private static BaseState CurrentState { get; set; }

    /// <summary>
    /// ステータスマシンを開始する
    /// </summary>
    public static void StartStateMachine()
    {
        _tokenSource = CancellationTokenSource.CreateLinkedTokenSource(Application.exitCancellationToken);
        _cancellationToken = _tokenSource.Token;
        RunStateMachineLoop().Forget();
    }

    /// <summary>
    /// ステータスマシンのLoop
    /// </summary>
    private static async UniTaskVoid RunStateMachineLoop()
    {
        //最初はInitialState
        CurrentState = StateFactory.CreateState(EGameState.Initial);
        BaseState nextState = CurrentState;

        try
        {
            while (true)
            {
                _cancellationToken.ThrowIfCancellationRequested();

                //Stateを切り替える
                nextState = await TransitionAndRunStateAsync(nextState);
                if (nextState == null)
                {
                    Debug.Log("GameOver!!");
                    break;
                }
            }
        }
        catch (OperationCanceledException)
        {
            Debug.Log("GameStateMachine is been canceled.");
        }
        catch (Exception ex)
        {
            Debug.LogError($"GameStateMachine Error: {ex.Message}");
        }
        finally
        {
            Debug.Log("GameStateMachine Stopped.");
            _tokenSource.Dispose();
            _tokenSource = null;
            SceneManager.LoadScene("Result");
        }
    }

    /// <summary>
    /// Stateを切り替える
    /// 前StateをonExitで抜けて
    /// 次のStateのOnEnterを呼ぶ
    /// RunStateで完成するを待つ
    /// </summary>
    /// <param name="nextState">次のState</param>
    /// <returns></returns>
    private static async UniTask<BaseState> TransitionAndRunStateAsync(BaseState nextState)
    {
        BaseState oldState = CurrentState;

        if (oldState != null && oldState != nextState)
        {
            try
            {
                await oldState.OnExit(_cancellationToken);
            }
            catch (OperationCanceledException)
            {
                Debug.Log($"State {oldState.GetType().Name} : OnExit is been canceled.");
            }
        }

        CurrentState = nextState;

        await CurrentState.OnEnter(_cancellationToken);

        // RunStateで完成するを待つ、完成すると次のStateを返す
        BaseState next = await CurrentState.RunState(_cancellationToken);

        return next;
    }

    private static BaseState _nextStateRequest;
    /// <summary>
    /// 外からのStateを切り替える
    /// </summary>
    /// <param name="nextState">替わるState</param>
    public static void SwitchState(EGameState nextState)
    {
        _nextStateRequest = StateFactory.CreateState(nextState);
        if (_nextStateRequest == null)
        {
            Debug.LogError("SwitchState failed.");
        }
        else
        {
            Debug.Log($"Switch State To : {_nextStateRequest.GetType().Name}");
        }
    }
    /// <summary>
    /// MainGameStateで毎フレームで検知する、もしrequestedStateはNullじゃないとStateを切り替わる
    /// </summary>
    /// <returns></returns>
    public static BaseState ConsumeStateRequest()
    {
        var requestedState = _nextStateRequest;
        _nextStateRequest = null;
        return requestedState;
    }
}