/*
 * Crescent_Moon / マップ更新と月イベントの連携（抜粋）
 * DI・Addressables の初期化、フィールド、他のイベントハンドラーは省略。
 * 購読は AddTo でまとめて保持し、Dispose 時に解除する。
 */

private void Bind()
{
    // Model の変更を購読し、View の更新完了後に各システムへ通知。
    _mapGeneratorModel.CurrentMap
        .SubscribeAwait(async (v, ct) =>
        {
            await _mapViewer.HandleMapView(v);
            // マップ表示 → 経路再構築 → アイテム配置の順序を揃える。
            await _gameEventBroker.Publish(new MapNavMeshGenerationEvent(), ct);
            await _gameEventBroker.Publish(new MapItemGenerationEvent(v), ct);
            await _gameEventBroker.Publish(new MapSunFlowerGenerationEvent( _mapGeneratorModel.CurrentSunFlowerMap.CurrentValue), ct);
            await _gameEventBroker.Publish(new MapTempleExitGenerationEvent( _mapGeneratorModel.CurrentTempleExitMap.CurrentValue), ct);
            // 新しいマップ通知が来たら、前の購読処理のキャンセルを要求。
        }, AwaitOperation.Switch).AddTo(_Disposable);
    
    _gameEventBroker
        .Subscribe<SunFlowerCollectedEvent>(HandleSunFlowerCollectedEvent)
        .AddTo(_Disposable);
    
    _gameEventBroker
        .Subscribe<GameStartEvent>(HandleGameStartEvent)
        .AddTo(_Disposable);
    _gameEventBroker
        .Subscribe<GazingMoonEvent>(HandleGazingMoonEvent)
        .AddTo(_Disposable);
}

private async UniTask HandleGazingMoonEvent(GazingMoonEvent e, CancellationToken token)
{    
    await _gameEventBroker.Publish(new MapGenerationStartedEvent(), token);
    // 月イベントに含まれる現在位置を、新しいマップの生成基準にする。
    MapCell player = _mapGeneratorModel.GetMapCellByPosition(e.X,e.Y);
    await _mapGeneratorModel.StartGeneration(player);
}
