using Player;
using UI;
using UniRx;
using UnityEngine;

/// <summary>
/// MVP（Model-View-Presenter）パターンの Presenter 層。
/// Model（PlayerModel の GameContext）の値の変化を UniRx で購読し、
/// View（MainUIViewer）へ反映する。
/// Model と View を直接参照させず、UI の更新処理を Presenter にまとめる。
/// </summary>
public class MainUIPresenter : MonoBehaviour
{
    [SerializeField] private MainUIViewer mainUIViewer; // View
    [SerializeField] private PlayerModel playerModel;   // Model

    private void Start()
    {
        // Model の各 ReactiveProperty を購読し、変化があれば対応する View 更新を呼ぶ。
        // データの変更に合わせて画面を更新する。
        playerModel.GameContext.CoinCount.Subscribe(c => mainUIViewer.SetCoin(c));
        playerModel.GameContext.DistanceCount.Subscribe(d => mainUIViewer.SetDistance(d));
        playerModel.GameContext.DominationCount.Subscribe(d => mainUIViewer.SetDominationBar(d));
        playerModel.GameContext.LifeCount.Subscribe(n => mainUIViewer.SetLife(n));
        playerModel.GameContext.CurrentSpeed.Subscribe(s => mainUIViewer.SetSpeed(s));
    }
}
