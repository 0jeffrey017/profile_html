public class EnemyBase : MonoBehaviour, IMoveable, IAttackable, IDamageable, IEntity, IDieable
{
    private EnemyConfig _config;

    private int _currentHealth;
    private float _beDamageColdDownTimer;
    private float _attackColdDownTimer;
    private bool _isAttackTimerCanCount;
    public FactionType Faction => FactionType.Enemy;
    [field: NonSerialized] public event Action<int, int> OnHealthChanged;
    [field: NonSerialized] public event Action<EffectType> OnDamage;
    [field: NonSerialized] public event Action<EffectType> OnAttack;
    [field: NonSerialized] public event Action<EffectType> OnDeathEffect;
    [field: NonSerialized] public event Action OnDeath;
    [field: NonSerialized] public event Action<float, float> AttackTimer;
    [field: NonSerialized] public event Action OnDamageEffectInitialize;

    private void Update()
    {
        UpdateTimer();
        UpdateState();
    }

    private void UpdateState()
    {
        if (transform.position.x >= 20) OnDeath?.Invoke();
        if (IsTargetInRange(out var go))
        {
            if (go.activeInHierarchy)
            {
                Attack(go);
            }
        }
        else
        {
            Move(Vector3.right);
        }
    }

    private void UpdateTimer()
    {
        _beDamageColdDownTimer += Time.deltaTime;
        if (!_isAttackTimerCanCount) return;
        _attackColdDownTimer += Time.deltaTime;
        AttackTimer?.Invoke(_attackColdDownTimer, _config.attackColdDownTime);
    }

    public void Initialize(EnemyConfig config)
    {
        _config = config;
        _currentHealth = _config.maxHealth;
        _beDamageColdDownTimer = 0;
        _attackColdDownTimer = 0;
        _isAttackTimerCanCount = false;
        OnHealthChanged?.Invoke(_config.maxHealth, _config.maxHealth);
        _config.moveStrategy.Initialize(transform);
        OnDamageEffectInitialize?.Invoke();
    }

    public void TakeDamage(int damage) //TODO TakeDamage Strategy
    {
        if (_beDamageColdDownTimer <= _config.damageColdDownTime) return;
        _beDamageColdDownTimer -= _config.damageColdDownTime;

        _currentHealth -= damage;

        if (_currentHealth <= 0)
        {
            _currentHealth = 0;
            OnHealthChanged?.Invoke(_currentHealth, _config.maxHealth);
            StartCoroutine(Die());
            return;
        }
        //effectController update effect
        OnDamage?.Invoke(_config.damageEffect);
        //UIManager update HP
        OnHealthChanged?.Invoke(_currentHealth, _config.maxHealth);
    }

    public bool CanDamage(GameObject target)
    {
        if (target == gameObject) return false;
        if (!target.TryGetComponent<IEntity>(out var entity)) return false;
        return _config.damageRelationData.CanDamage(entity.Faction);
    }

    public IEnumerator Die()
    {
        yield return null;
        OnDeathEffect?.Invoke(_config.deathEffect);
        OnDeath?.Invoke();
        GameManager.Instance.CurrentMaterials += _config.dropMoney; //TODO ???
    }

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
        IFireable enemyBullet = BulletManager.Instance.SpawnBullet(_config.bulletType, transform.position, transform.rotation);
        OnAttack?.Invoke(_config.attackEffect);
        enemyBullet?.Fire(target, _config.attackPower);
        AudioManager.Instance.PlaySE(_config.SeType);
    }

    public bool IsTargetInRange(out GameObject go)
    {
        go = null;
        LayerMask l = 1 << LayerMask.NameToLayer("Target"); // TODO ??
        var hit = Physics2D.OverlapCircle(transform.position, _config.attackRange, l);
        if (hit == null) return false;
        go = hit.gameObject;
        if (go == gameObject) return false;
        return CanDamage(go);
    }
}


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

public class EnemyFactory
{
    private readonly ObjectPool<GameObject> _enemyPool;
    private readonly EnemyConfig _enemyConfig;

    public EnemyFactory(EnemyConfig enemyConfig, int defaultNumber, int maxSize)
    {
        _enemyConfig = enemyConfig;

        _enemyPool = new ObjectPool<GameObject>(
            createFunc: CreateEnemy,
            actionOnGet: OnGet,
            actionOnRelease: OnRelease,
            actionOnDestroy: OnDestroy,
            collectionCheck: false,
            defaultCapacity: defaultNumber,
            maxSize: maxSize
        );
    }

    private GameObject CreateEnemy()
    {
        var go = Object.Instantiate(_enemyConfig.prefab);
        return go;
    }

    private void OnGet(GameObject go)
    {
        go.SetActive(true);
        if (!go.TryGetComponent<EnemyBase>(out var enemyBase)) return;
        enemyBase.Initialize(_enemyConfig);

        void OnEnemyDeath() // 死亡したらpoolに返す
        {
            enemyBase.OnDeath -= OnEnemyDeath;
            _enemyPool.Release(go);
        }
        enemyBase.OnDeath += OnEnemyDeath;
    }

    private void OnRelease(GameObject go) => go.SetActive(false);
    private void OnDestroy(GameObject go) => Object.Destroy(go);
    public GameObject Get() => _enemyPool.Get();
}