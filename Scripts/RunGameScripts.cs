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

public abstract class BaseState
{
    public virtual UniTask OnEnter(CancellationToken token) => UniTask.CompletedTask;
    public abstract UniTask<BaseState> RunState(CancellationToken token);
    public virtual UniTask OnExit(CancellationToken token) => UniTask.CompletedTask;
}

public static class StateFactory
{
    private static GameContext _gameContext;
    private static GameResultData _gameResultData;
    private static GameSettingData _settingData;
    private static PlayerMovementController _playerMovementController;
    private static AnimationChange _animationChanger;
    private static ParticleSystem _speedParticleSystem;
    private static EventUiViewer _eventUiViewer;
    private static StageGenerator _stageGenerator;
    private static PlayerInput _playerInput;
    private static InvincibleManager _invincibleManager;
    private static PlayerEffectController _playerEffectController;

    public static void Initialize(GameContext gameContext,
        PlayerMovementController playerMovementController,
        ParticleSystem speedParticleSystem,
        EventUiViewer eventUiViewer,
        StageGenerator stageGenerator,
        PlayerInput playerInput,
        GameSettingData settingData,
        GameResultData gameResultData,
        AnimationChange animationChanger,
        InvincibleManager invincibleManager,
        PlayerEffectController playerEffectController)
    {
        _gameContext = gameContext;
        _playerMovementController = playerMovementController;
        _speedParticleSystem = speedParticleSystem;
        _eventUiViewer = eventUiViewer;
        _stageGenerator = stageGenerator;
        _playerInput = playerInput;
        _settingData = settingData;
        _gameResultData = gameResultData;
        _animationChanger = animationChanger;
        _invincibleManager = invincibleManager;
        _playerEffectController = playerEffectController;
    }

    public static BaseState CreateState(EGameState state)
    {
        return state switch
        {
            EGameState.Initial => new InitialState(),
            EGameState.Tutorial => new TutorialState(_gameContext, _eventUiViewer, _stageGenerator, _playerInput, _playerMovementController),
            EGameState.CountDown => new CountDownState(_eventUiViewer, _stageGenerator, _playerMovementController),
            EGameState.MainGame => new MainGameState(_gameContext),
            EGameState.Pause => new PauseState(_playerInput, _eventUiViewer),
            EGameState.EventBigEater => new EventBigEaterState(_gameContext, _eventUiViewer, _stageGenerator, _settingData),
            EGameState.Horse => new HorseState(_playerMovementController, _speedParticleSystem, _gameContext, _settingData, _eventUiViewer, _invincibleManager),
            EGameState.DarkWrestler => new EventDarkWrestlerState(_gameContext, _playerMovementController, _eventUiViewer, _playerInput, _settingData, _stageGenerator, _playerEffectController),
            EGameState.StartDarkWrestler => new StartDarkWrestlerState(_stageGenerator),
            EGameState.HelpPeople => new EventHelpPeopleState(_eventUiViewer, _gameContext, _settingData),
            EGameState.PanCake => new ItemPanCakeEffectState(_playerMovementController, _settingData, _eventUiViewer, _animationChanger, _gameContext),
            EGameState.GameOver => new GameOverState(_playerMovementController, _eventUiViewer, _gameContext, _gameResultData, _playerInput),
            EGameState.LifeCheckState => new LifeCheckState(_gameContext, _playerMovementController, _invincibleManager),
            _ => null
        };
    }
}