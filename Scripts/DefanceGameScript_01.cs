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