/*
 * Crescent_Moon / 型付き非同期 EventBroker のコア処理（抜粋）
 * using・namespace・クラス定義は省略。
 * _events は Dictionary<Type, Delegate>、Subscription は Dispose で解除処理を実行。
 */

public async UniTask Publish<T>(T message,CancellationToken token) where T : IEvent
{
    // 型ごとに配信先を取得。発行側は購読者の具体的なクラスを知らなくてよい。
    if (!_events.TryGetValue(typeof(T), out Delegate eventDelegate))
    {
        Debug.LogWarning($"[EventBroker] No one is listening to {typeof(T).Name}");
        return;
    }

    Delegate[] invocationList = eventDelegate.GetInvocationList();
    if (invocationList.Length == 1)
    {
        await ((Func<T, CancellationToken, UniTask>)invocationList[0])(message, token);
        return;
    }

    var tasks = new UniTask[invocationList.Length];
    for (int i = 0; i < invocationList.Length; i++)
    {
        tasks[i] = ((Func<T, CancellationToken, UniTask>)invocationList[i])(message, token);
    }
    // 全ハンドラーの完了を待つため、呼び出し側も await で順序を制御できる。
    await UniTask.WhenAll(tasks);
}

public IDisposable Subscribe<T>(Func<T,CancellationToken,UniTask> action) where T : IEvent
{
    Type type = typeof(T);

    _events[type] = _events.TryGetValue(type, out var existing) ? Delegate.Combine(existing, action) : action;
    // 返した購読オブジェクトを Dispose すると、このハンドラーだけ解除する。
    return new Subscription(() =>
    {
        if (_events.TryGetValue(type, out Delegate current))
        {
            Delegate removed = Delegate.Remove(current, action);
            if (removed == null)
            {
                _events.Remove(type);
            }
            else
            {
                _events[type] = removed;
            }
        }
    });
}
