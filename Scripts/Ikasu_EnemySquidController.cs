using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// ボス（巨大イカ）のAI制御。NavMeshAgent による移動を土台に、
/// プレイヤーとの「距離」に応じて 巡回 → 追跡 → 近・中・遠距離攻撃 を切り替える
/// 多段階の行動ロジックを実装。各攻撃はクールダウンで管理し、
/// 弾は物理計算で算出した放物線軌道でプレイヤーへ撃ち込む。
/// （担当：ボス戦のAI・戦闘ロジック全般）
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class EnemySquidController : MonoBehaviour
{
    private NavMeshAgent _agent;
    public Transform _player;
    public LayerMask WhatIsPlayer;
    public BattleSceneManager _battleSceneManager;

    // 視認・各攻撃の有効範囲（半径）
    public float _signtRange, _attackRange, _attackRange_02, _attackRange_03;
    public float _timeBetweenAttacks;
    public float _parabolaTime = 2f;

    public GameObject _squidAttack;       // 放物線で飛ばす弾
    private bool _alreadyAttacked;

    private void Awake() => _agent = GetComponent<NavMeshAgent>();

    private void Update()
    {
        // プレイヤーとの距離を各範囲の球で判定（近いほど強力な攻撃へ移行）
        bool inSight   = Physics.CheckSphere(transform.position, _signtRange, WhatIsPlayer);
        bool inAttack  = Physics.CheckSphere(transform.position, _attackRange, WhatIsPlayer);
        bool inAttack2 = Physics.CheckSphere(transform.position, _attackRange_02, WhatIsPlayer);
        bool inAttack3 = Physics.CheckSphere(transform.position, _attackRange_03, WhatIsPlayer);

        // --- 距離に応じた行動分岐（簡易ステートマシン） ---
        if (!inSight && !inAttack && !inAttack2)      Patroling();   // 圏外：巡回
        else if (inSight && !inAttack && !inAttack2)  ChasePlayer(); // 視認：追跡
        else if (inAttack && inSight && !inAttack2)   AttackPlayer(); // 中距離：放物線弾

        if (inAttack && inSight && inAttack2)         AttackPlayer02(); // 近距離：ノックバック

        // 遠距離攻撃は「残り時間が一定以下」のときのみ発動する条件付きギミック
        if (inAttack3 && _battleSceneManager.GetCountDownTime() < 0.6f)
            AttackPlayer03();
    }

    private void ChasePlayer() => _agent.SetDestination(_player.position);

    /// <summary>中距離攻撃：プレイヤーへ放物線を描く弾を発射する。</summary>
    private void AttackPlayer()
    {
        _agent.SetDestination(transform.position); // その場で停止
        transform.LookAt(_player);

        if (_alreadyAttacked) return; // クールダウン中は撃たない

        Rigidbody rb = Instantiate(_squidAttack, transform.position, transform.rotation)
                       .GetComponent<Rigidbody>();
        AudioManager.Instance.PlaySE(E_SEType.大砲1);

        Vector3 firePoint = transform.position + Vector3.up * 2f;
        Vector3 target = GetRandomPointInCircle(_player.position, 2f); // 着弾に揺らぎを与える
        ParabolaWithTime(rb, firePoint, target, _parabolaTime);

        _alreadyAttacked = true;
        Invoke(nameof(ResetAttack), _timeBetweenAttacks); // 一定時間後に再攻撃を許可
    }

    private void ResetAttack() => _alreadyAttacked = false;

    /// <summary>着弾点を円内にランダム分布させる（角度＋√で一様分布を実現）。</summary>
    private Vector3 GetRandomPointInCircle(Vector3 center, float radius)
    {
        float angle = Random.Range(0f, Mathf.PI * 2);
        float r = Mathf.Sqrt(Random.value) * radius; // √を取ることで中心に偏らせない
        return center + new Vector3(Mathf.Cos(angle) * r, 0f, Mathf.Sin(angle) * r);
    }

    /// <summary>
    /// 指定時間 totalTime で startPos から targetPos へ到達する放物線の初速を逆算し、
    /// Rigidbody にインパルスとして与える。
    /// 水平：v = 距離 / 時間、垂直：y = v0t − ½gt² を v0 について解く。
    /// </summary>
    public void ParabolaWithTime(Rigidbody rb, Vector3 startPos, Vector3 targetPos, float totalTime)
    {
        Vector3 displacement = targetPos - startPos;

        // 水平成分：等速とみなして初速を算出
        Vector3 displacementXZ = new Vector3(displacement.x, 0, displacement.z);
        Vector3 velocityXZ = displacementXZ / totalTime;

        // 垂直成分：重力を打ち消して目標高度に届く初速を逆算
        float gravity = Mathf.Abs(Physics.gravity.y);
        float velocityY = (displacement.y + 0.5f * gravity * totalTime * totalTime) / totalTime;

        Vector3 initialVelocity = velocityXZ + Vector3.up * velocityY;

        rb.linearVelocity = Vector3.zero;
        rb.AddForce(initialVelocity * 1.05f, ForceMode.Impulse);
    }

    // Patroling() / AttackPlayer02() / AttackPlayer03() は紙幅の都合で省略
    private void Patroling() { /* NavMeshでランダムな巡回点へ移動 */ }
    private void AttackPlayer02() { /* 近距離：周囲をノックバック */ }
    private void AttackPlayer03() { /* 遠距離：地面から触手を出現させる */ }
}
