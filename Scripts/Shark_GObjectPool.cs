using UnityEngine;
using UnityEngine.Pool;

/// <summary>
/// 大量に出現・消滅する食べ物オブジェクトを使い回すためのオブジェクトプール。
/// Instantiate / Destroy の頻発による GC スパイク（処理落ち）を防ぐため、
/// Unity 標準の ObjectPool をラップし、生成時の初期化処理を一元化する。
/// </summary>
public class GObjectPool
{
    private readonly ObjectPool<GameObject> _pool;
    private readonly int _defaultNumber = 30; // 事前確保数
    private readonly int _maxSize = 100;       // 上限（超過分は破棄）

    public GObjectPool(GameObject prefab, int layerID, int defaultNumber = 30, int maxSize = 100)
    {
        _defaultNumber = defaultNumber;
        _maxSize = maxSize;

        _pool = new ObjectPool<GameObject>(
            createFunc: () => Initialization(prefab, layerID), // 新規生成時のみ呼ばれる
            actionOnGet: g => g.SetActive(true),               // 取り出し時：再有効化
            actionOnRelease: g => g.SetActive(false),          // 返却時：非表示にして保持
            actionOnDestroy: Object.Destroy,                   // 上限超過時：破棄
            collectionCheck: false,
            defaultCapacity: _defaultNumber,
            maxSize: _maxSize);
    }

    /// <summary>指定位置にオブジェクトを取り出す（再利用 or 新規生成）。</summary>
    public GameObject Get(Vector3 position)
    {
        GameObject go = _pool.Get();
        go.transform.position = position;
        return go;
    }

    /// <summary>使い終えたオブジェクトをプールへ返却する（Destroy しない）。</summary>
    public void Return(GameObject g) => _pool.Release(g);

    /// <summary>
    /// 新規生成時のみ実行される初期化。レイヤー設定・当たり判定の付与・
    /// 自身（プール）への参照渡しをまとめ、生成箇所の責務を集約する。
    /// </summary>
    private GameObject Initialization(GameObject prefab, int layerID)
    {
        var g = Object.Instantiate(prefab);
        g.layer = layerID;

        var controller = g.AddComponent<ObjectsController>();
        var collider = g.AddComponent<SphereCollider>();
        collider.isTrigger = true;

        // 自身を渡すことで、各オブジェクトが「使い終わったら自分でプールへ戻る」ことを可能にする
        controller.Setup(this, Random.Range(5, 12));
        g.transform.rotation = Quaternion.Euler(0, -90, 90);
        return g;
    }
}
