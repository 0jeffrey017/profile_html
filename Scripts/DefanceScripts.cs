/// <summary>
/// EnemyBaseを作成、詳しい攻撃や移動などStrategyパターンで決める
/// </summary>
public class EnemyBase : MonoBehaviour, IMoveable, IAttackable, IDamageable, IEntity, IDieable
{
    private EnemyConfig _config;
    public FactionType Faction => FactionType.Enemy;
    public void Move(Vector3 direction)
    {
        _isAttackTimerCanCount = false;
        var newPosition = _config.moveStrategy.Move(transform.position, direction, _config.moveSpeed);
        transform.position = newPosition;
    }
    public void Attack(GameObject target)
    {
        _isAttackTimerCanCount = true;
        if (_attackColdDownTimer <= _config.attackColdDownTime) return;
        _attackColdDownTimer -= _config.attackColdDownTime;

        //ObjectPoolを使って、ファクトリーを使って、enemyを生成する、大量のInstantiate,Destoryを避けることができる
        IFireable enemyBullet = BulletManager.Instance.SpawnBullet(_config.bulletType, transform.position, transform.rotation);
        
        OnAttack?.Invoke(_config.attackEffect);
        enemyBullet?.Fire(target, _config.attackPower);
        AudioManager.Instance.PlaySE(_config.SeType);
    }
}

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