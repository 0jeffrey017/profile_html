using Player;
using UI;
using UniRx;
using UnityEngine;

/// <summary>
/// MVP（Model-View-Presenter）パターンの Presenter 層。
/// Model（PlayerModel の GameContext）の値の変化を UniRx で購読し、
/// View（MainUIViewer）へ反映するだけの「中間者」に徹する。
/// Model は View を知らず、View は Model を知らない。両者を Presenter が仲介することで
/// 結合度を極限まで下げ、UI 刷新やロジック変更が互いに波及しない設計を実現する。
/// </summary>
public class MainUIPresenter : MonoBehaviour
{
    [SerializeField] private MainUIViewer mainUIViewer; // View
    [SerializeField] private PlayerModel playerModel;   // Model

    private void Start()
    {
        // Model の各 ReactiveProperty を購読し、変化があれば対応する View 更新を呼ぶ。
        // 「データが変わったら自動で画面に反映される」宣言的なバインディング。
        playerModel.GameContext.CoinCount.Subscribe(c => mainUIViewer.SetCoin(c));
        playerModel.GameContext.DistanceCount.Subscribe(d => mainUIViewer.SetDistance(d));
        playerModel.GameContext.DominationCount.Subscribe(d => mainUIViewer.SetDominationBar(d));
        playerModel.GameContext.LifeCount.Subscribe(n => mainUIViewer.SetLife(n));
        playerModel.GameContext.CurrentSpeed.Subscribe(s => mainUIViewer.SetSpeed(s));
    }
}
