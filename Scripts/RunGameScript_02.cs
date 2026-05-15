public class MainUIPresenter : MonoBehaviour
{
    [SerializeField] private MainUIViewer mainUIViewer;
    [SerializeField] private PlayerModel playerModel;
    
    /// <summary>
    /// playerModel　と　mainUIViewer　の中間者
    /// </summary>
    private void Start()
    {   
        playerModel.GameContext.CoinCount.Subscribe(c => mainUIViewer.SetCoin(c));
        playerModel.GameContext.DistanceCount.Subscribe(d => mainUIViewer.SetDistance(d));
        playerModel.GameContext.DominationCount.Subscribe(d => mainUIViewer.SetDominationBar(d));
        playerModel.GameContext.LifeCount.Subscribe(n => mainUIViewer.SetLife(n));
        playerModel.GameContext.CurrentSpeed.Subscribe(s => mainUIViewer.SetSpeed(s));
    }
}