using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using UnityEngine.AI;
using VContainer;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyView : MonoBehaviour, IDamageable
{
    private static readonly int Step = Shader.PropertyToID("_Step");
    [SerializeField] private float repathInterval = 0.2f;
    [SerializeField] private float stoppingDistance = 0.6f;

    [SerializeField] private float health = 3.0f;
    [SerializeField] private float attackRange = 3.0f;
    [SerializeField] private float attackCooldownTime = 1.0f;
    [SerializeField] private int attackDamage = 1;
    [SerializeField] private LayerMask playLayer;
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private int scoreValue = 100;

    private readonly Subject<int> _onEnemyDie = new Subject<int>();
    public Observable<int> OnEnemyDie => _onEnemyDie;

    private NavMeshAgent _agent;
    private Transform _target;
    private float _lastRepathTime = float.NegativeInfinity;
    private float _lastAttackTime = float.NegativeInfinity;

    private BulletPool _bulletPool;
    private Material _material;
    private bool _isDead;

    [Inject]
    public void Construct(PlayerView player, BulletPool bulletPool)
    {
        _target = player ? player.transform : null;
        _bulletPool = bulletPool;
    }

    private void Awake()
    {   
        _material = GetComponent<Renderer>().material;
        _agent = GetComponent<NavMeshAgent>();
        _agent.updateRotation = false;
        _agent.updateUpAxis = false;
        _agent.stoppingDistance = stoppingDistance;
    }

    private void Update()
    {
        if (_target == null || !_agent.isOnNavMesh) return;
        if(_isDead) return;
        Vector2 toTarget = _target.position - transform.position;

        if (CanAttack(toTarget))
        {
            _agent.isStopped = true;
            RotateTowards(toTarget);
            TryShoot(toTarget.normalized);
        }
        else
        {
            _agent.isStopped = false;
            if (Time.time - _lastRepathTime >= repathInterval)
            {
                _lastRepathTime = Time.time;
                _agent.SetDestination(_target.position);
            }
            RotateTowards(_agent.velocity);
        }
    }
    
    private bool CanAttack(Vector2 toTarget)
    {
        if (toTarget.sqrMagnitude > attackRange * attackRange) return false;

        // 壁も含めてRayを飛ばし、最初に当たったのがプレイヤーなら視線が通っている
        RaycastHit2D hit = Physics2D.Raycast(transform.position, toTarget.normalized, attackRange, playLayer | wallLayer);
        
        return hit.collider != null && hit.transform == _target;
    }

    private void TryShoot(Vector2 direction)
    {
        if (Time.time - _lastAttackTime < attackCooldownTime) return;
        _lastAttackTime = Time.time;
        LayerMask l = playLayer | wallLayer;
        _bulletPool.Fire(transform.position, direction, attackDamage, l);
    }

    private void RotateTowards(Vector2 direction)
    {
        if (direction.sqrMagnitude < 0.01f) return;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    public void TakeDamage(int damage)
    {
        if (health <= 0f) return; // 死亡処理中の多重ヒットで二重加算しない

        health -= damage;
        if (health <= 0f)
        {
            _onEnemyDie.OnNext(scoreValue);
            HandleDie().Forget();
        }
    }

    private async UniTaskVoid HandleDie()
    {   
        _isDead = true;
        float timer = 2.0f;
        float time = 0.0f;
        while (time <= timer)
        {   
            time += Time.deltaTime;
            var t = Mathf.Clamp01(time / timer);
            _material.SetFloat(Step, t);
            await UniTask.Yield();
        }
        Destroy(this.gameObject);
    }

    private void OnDestroy()
    {
        _onEnemyDie.Dispose();
    }
}
