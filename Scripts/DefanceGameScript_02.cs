/// <summary>
/// EnemyのデータをScriptableObjectを所持する、Enemyの詳細変更を簡単にできる
/// </summary>
public class EnemyConfig : ScriptableObject
{
    [Header("表示属性")]
    [Tooltip("名前")] public EnemyType enemyName;
    [Tooltip("画像")] public Sprite enemyImage;
    [Tooltip("説明"), Multiline(6)] public string description;

    [Header("基本属性")]
    [Tooltip("資材コスト"), Range(0, 1000)] public int cost;
    [Tooltip("生成時間"), Range(0, 500)] public float spawnTime;
    [Tooltip("ドロップ資材"), Range(0, 10000)] public int dropMoney;
    [Tooltip("HP最大値"), Range(1, 10000)] public int maxHealth;
    [Tooltip("攻撃"), Range(0, 1000)] public int attackPower;
    [Tooltip("移動速度（m/s）"), Range(0.0f, 20.0f)] public float moveSpeed;
    [Tooltip("攻撃範囲（ｍ）"), Range(0.1f, 30.0f)] public float attackRange;
    [Tooltip("攻撃頻度（ｓ）"), Range(0.1f, 30.0f)] public float attackColdDownTime;
    [Tooltip("ダメージを受ける頻度（ｓ）"), Range(0.1f, 20.0f)] public float damageColdDownTime;

    [Header("プレハブ")]
    public GameObject prefab;

    [Header("弾の種類")]
    public BulletType bulletType;
    [Header("弾の音")]
    public SEType SeType;

    [Header("CanTakeDamageBy")]
    public DamageRelationData damageRelationData;

    [Header("EffectType")]
    [Tooltip("ダメージエフェクト")] public EffectType damageEffect;
    [Tooltip("攻撃エフェクト")] public EffectType attackEffect;
    [Tooltip("死亡エフェクト")] public EffectType deathEffect;

    [Header("MoveStrategy")]
    public MoveStrategy moveStrategy;
}