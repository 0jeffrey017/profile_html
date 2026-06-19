using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using State.Enum;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace State
{
    /// <summary>
    /// ゲーム全体の進行を制御する有限ステートマシン（FSM）。
    /// 各状態（State）は OnEnter → RunState → OnExit のライフサイクルを持ち、
    /// UniTask により「状態の実行完了」を await で待つことで、複雑なゲーム進行を
    /// コールバックに頼らず直線的かつ堅牢に記述できる。
    /// CancellationToken でシーン破棄・アプリ終了時の安全な中断も保証する。
    /// </summary>
    public static class GameStateMachine
    {
        private static CancellationTokenSource _tokenSource;
        private static CancellationToken _cancellationToken;
        private static BaseState CurrentState { get; set; }

        /// <summary>ステートマシンを開始する。アプリ終了トークンと連動させる。</summary>
        public static void StartStateMachine()
        {
            _tokenSource = CancellationTokenSource.CreateLinkedTokenSource(Application.exitCancellationToken);
            _cancellationToken = _tokenSource.Token;
            RunStateMachineLoop().Forget();
        }

        /// <summary>状態遷移を回し続けるメインループ。次状態が null になれば終了。</summary>
        private static async UniTaskVoid RunStateMachineLoop()
        {
            CurrentState = StateFactory.CreateState(EGameState.Initial); // 初期状態
            BaseState nextState = CurrentState;

            try
            {
                while (true)
                {
                    _cancellationToken.ThrowIfCancellationRequested();

                    nextState = await TransitionAndRunStateAsync(nextState);
                    if (nextState == null) // ゲームオーバー
                    {
                        Debug.Log("GameOver!!");
                        break;
                    }
                }
            }
            catch (OperationCanceledException)
            {
                Debug.Log("GameStateMachine is canceled.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"GameStateMachine Error: {ex.Message}");
            }
            finally
            {
                // 終了処理は必ず通る finally に集約し、リソース解放とシーン遷移を保証
                _tokenSource.Dispose();
                _tokenSource = null;
                SceneManager.LoadScene("Result");
            }
        }

        /// <summary>
        /// 状態を切り替える：旧状態を OnExit で抜け、新状態を OnEnter →
        /// RunState の完了まで待ち、その戻り値を次状態として返す。
        /// </summary>
        private static async UniTask<BaseState> TransitionAndRunStateAsync(BaseState nextState)
        {
            BaseState oldState = CurrentState;

            if (oldState != null && oldState != nextState)
            {
                try { await oldState.OnExit(_cancellationToken); }
                catch (OperationCanceledException)
                {
                    Debug.Log($"State {oldState.GetType().Name} : OnExit canceled.");
                }
            }

            CurrentState = nextState;
            await CurrentState.OnEnter(_cancellationToken);

            // RunState が完了する（＝状態の役目が終わる）まで待ち、次状態を受け取る
            return await CurrentState.RunState(_cancellationToken);
        }

        private static BaseState _nextStateRequest;

        /// <summary>外部から状態遷移をリクエストする（例：イベント発火による割り込み）。</summary>
        public static void SwitchState(EGameState nextState)
        {
            _nextStateRequest = StateFactory.CreateState(nextState);
            if (_nextStateRequest == null) Debug.LogError("SwitchState failed.");
        }

        /// <summary>リクエストされた状態を1度だけ取り出す（MainGameStateが毎フレーム検知）。</summary>
        public static BaseState ConsumeStateRequest()
        {
            var requested = _nextStateRequest;
            _nextStateRequest = null;
            return requested;
        }
    }
}
