using System;
using R3;
using VContainer;
using VContainer.Unity;

/*
 * VContainer DI Experiment（担当：個人開発 / アーキテクチャ実験）
 *
 * DIコンテナ「VContainer」で依存関係の登録と注入を一元管理。
 * シングルトンへの直接参照をやめ、各クラスは「必要なものをコンストラクタで受け取る」だけに。
 * これにより結合度が下がり、テスト・再利用が容易な構成を実現する。
 */

/// <summary>
/// 依存関係の登録を行うコンテナの設定クラス（VContainerのエントリポイント）。
/// 「何を・どの生存期間で・どう生成するか」をここに集約することで、
/// 各クラスは生成や参照解決の責務から解放される。
/// </summary>
public class GameLifeTimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        // Model はアプリ全体で1つ（Singleton）
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
/// 依存物は全て「コンストラクタインジェクション」で受け取るため、newもFindも不要。
/// IStartable / IDisposable を実装し、初期化と後始末をライフサイクルに乗せる。
/// </summary>
public class MainGamePresenter : IStartable, IDisposable
{
    // readonly で注入後の差し替えを禁止し、依存の不変性を保証
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

    // 購読をまとめて破棄し、メモリリークを防ぐ
    public void Dispose() => _disposableBag.Dispose();
}
