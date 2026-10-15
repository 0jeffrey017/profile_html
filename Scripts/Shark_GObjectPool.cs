using UnityEngine;
using UnityEngine.Pool;

/// <summary>
/// 大量に出現・消滅する食べ物オブジェクトを使い回すためのオブジェクトプール。
/// Instantiate / Destroy の回数を減らすため、
/// Unity 標準の ObjectPool をラップし、生成時の初期化処理を一元化する。
/// </summary>
public class GObjectPool
{
    private readonly ObjectPool<GameObject> _pool;
    private readonly int _defaultNumber = 30; // プールの初期容量（オブジェクトの事前生成数ではない）
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

    /// <summary>オブジェクトを再利用または新規生成し、指定位置に配置する。</summary>
    public GameObject Get(Vector3 position)
    {
        GameObject go = _pool.Get();
        go.transform.position = position;
        return go;
    }

    /// <summary>使い終えたオブジェクトを返却する。保持数の上限を超えた分は破棄する。</summary>
    public void Return(GameObject g) => _pool.Release(g);

    /// <summary>
    /// 新規生成時のみ実行される初期化。レイヤー設定・当たり判定の付与・
    /// プールへの参照の設定をまとめる。
    /// </summary>
    private GameObject Initialization(GameObject prefab, int layerID)
    {
        var g = Object.Instantiate(prefab);
        g.layer = layerID;

        var controller = g.AddComponent<ObjectsController>();
        var collider = g.AddComponent<SphereCollider>();
        collider.isTrigger = true;

        // 使用後にプールへ戻れるよう、返却先の参照を渡す。
        controller.Setup(this, Random.Range(5, 12));
        g.transform.rotation = Quaternion.Euler(0, -90, 90);
        return g;
    }
}
