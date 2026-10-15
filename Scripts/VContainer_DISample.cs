using System;
using R3;
using VContainer;
using VContainer.Unity;

/*
 * VContainer DI Experiment（担当：個人開発 / アーキテクチャ実験）
 *
 * VContainer で依存関係を登録し、各クラスに必要な参照を渡す。
 * 各クラスがシングルトンを直接参照せず、コンストラクターで参照を受け取る構成を検証する。
 */

/// <summary>
/// 依存関係の登録を行うコンテナの設定クラス（VContainerのエントリポイント）。
/// 登録する型と生存期間、生成方法をここで指定する。
/// 各クラスは、必要な参照をコンテナから受け取る。
/// </summary>
public class GameLifeTimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        // このスコープで単一の Model を共有する（Singleton）
        builder.Register<MainGameModel>(Lifetime.Singleton);

        // シーン上の MonoBehaviour を解決対象として登録
        builder.RegisterComponentInHierarchy<BallSimulationManager>();
        builder.RegisterComponentInHierarchy<MainGameView>();
        builder.RegisterComponentInHierarchy<ClickView>();

        // Presenter をエントリポイントとして登録（Start/Dispose が自動で呼ばれる）
        builder.RegisterEntryPoint<MainGamePresenter>(Lifetime.Scoped);
    }
}

/// <summary>
/// Model と View を仲介する Presenter（MVP）。
/// 必要な参照はコンストラクターで受け取る。
/// IStartable / IDisposable で初期化と購読解除のタイミングを管理する。
/// </summary>
public class MainGamePresenter : IStartable, IDisposable
{
    // readonly で、受け取った参照の再代入を防ぐ。
    private readonly MainGameView _mainGameView;
    private readonly MainGameModel _mainGameModel;
    private readonly ClickView _clickView;
    private readonly BallSimulationManager _ballSimulationManager;

    private DisposableBag _disposableBag;

    // コンテナが各依存を自動で解決してこのコンストラクタへ渡す
    public MainGamePresenter(MainGameView view,
        BallSimulationManager ballSimulationManager,
        MainGameModel mainGameModel,
        ClickView clickView)
    {
        _mainGameView = view;
        _mainGameModel = mainGameModel;
        _ballSimulationManager = ballSimulationManager;
        _clickView = clickView;
    }

    // エントリポイントとしてコンテナ初期化後に自動で呼ばれる
    public void Start() => Bind();

    /// <summary>Model（R3のReactiveProperty）を購読し、変化をViewへ反映する。</summary>
    private void Bind()
    {
        _ballSimulationManager.SetBallsPerClick(_mainGameModel.BallsPerClick.CurrentValue);

        // 所持金が変わったら表示を更新（宣言的バインディング）
        _mainGameModel.Money
            .Subscribe(money => _mainGameView.SetMoneyText(money))
            .AddTo(ref _disposableBag);

        // 1秒ごとに自動でボールを生成
        Observable.Interval(TimeSpan.FromSeconds(1))
            .Subscribe(_ => _ballSimulationManager.HandleClickSpawn())
            .AddTo(ref _disposableBag);

        // View からの入力イベントを購読
        _clickView.OnClick += OnClick;
        _ballSimulationManager.OnGetMoney += OnGetMoney;
    }

    private void OnClick() => _ballSimulationManager.HandleClickSpawn();
    private void OnGetMoney(uint money) => _mainGameModel.Money.Value += money;

    // DisposableBag に登録した R3 の購読を解除する
    public void Dispose() => _disposableBag.Dispose();
}
