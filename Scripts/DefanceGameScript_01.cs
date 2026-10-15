using System.Collections;
using Enemy.Bullets;
using Enemy.Interfaces;
using UnityEngine;

/*
 * 逢魔々城 — 弾の挙動を Strategy パターンで切り替える設計（担当：敵AI・武器攻撃システム）
 *
 * 飛び方と着弾時の処理を BulletStrategy に定義し、弾本体（BulletBase）から分ける。
 * プランナーは Unity エディターで Strategy アセットを差し替え、挙動を切り替えられる。
 */

namespace Enemy.Strategy.BulletStrategys
{
    /// <summary>
    /// 弾の挙動を定義する抽象クラス。発射時と衝突時の処理を派生クラスで実装する。
    /// </summary>
    public abstract class BulletStrategy : ScriptableObject
    {
        public virtual void OnInitialize(BulletBase bullet) { }
        public abstract void FireBullet(BulletBase bullet, GameObject target);
        public abstract void BulletTriggerEnter(BulletBase bullet, Collider2D collision);
    }

    /// <summary>
    /// 具体戦略の例：山なりの軌道でターゲットへ着弾する弾。
    /// sin 波で高さを与えることで山なりの軌道を表現する。
    /// </summary>
    [CreateAssetMenu(menuName = "BulletStrategy/BulletParabola")]
    public class BulletParabola : BulletStrategy
    {
        private readonly float _bulletHeight = 2.0f;
        private GameObject _target;

        public override void FireBullet(BulletBase bullet, GameObject target)
        {
            _target = target;
            bullet.StartCoroutine(FireToTarget(bullet, target));
        }

        public override void BulletTriggerEnter(BulletBase bullet, Collider2D collision)
        {
            if (collision.gameObject != _target) return;

            // 着弾相手が IDamageable ならダメージ処理を呼び、弾をプールへ返す。
            if (collision.gameObject.TryGetComponent<IDamageable>(out var damageable))
            {
                damageable.TakeDamage(bullet.Damage);
                BulletManager.Instance.RecycleBullet(bullet.Config.bulletType, bullet);
            }
        }

        /// <summary>開始点から着弾点まで Lerp で進めつつ、sin 波で高さを加える。</summary>
        private IEnumerator FireToTarget(BulletBase bullet, GameObject target)
        {
            var start = bullet.transform.position;
            var end = target.transform.position;
            var time = Vector3.Distance(start, end) / bullet.Config.speed; // 速度から所要時間を算出
            var elapsed = 0f;

            while (elapsed < time)
            {
                elapsed += Time.deltaTime;
                var t = Mathf.Clamp01(elapsed / time);

                var pos = Vector3.Lerp(start, end, t);
                pos.y += _bulletHeight * Mathf.Sin(Mathf.PI * t); // 0→頂点→0 の山なり軌道
                bullet.transform.position = pos;

                yield return null;
            }

            bullet.transform.position = end;
            BulletManager.Instance.RecycleBullet(bullet.Config.bulletType, bullet); // 使い回しでGC抑制
        }
    }
}
