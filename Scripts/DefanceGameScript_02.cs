using Enemy.Bullets;
using Enemy.Strategy;
using Enemy.Strategy.MoveStrategys;
using UnityEngine;

namespace Enemy.Enemy
{
    /// <summary>
    /// 敵1種類分のマスターデータを保持する ScriptableObject。
    /// HP・攻撃力・コスト等の数値に加え、移動方法（MoveStrategy）や弾種・エフェクトまで
    /// データとして外部化することで、プログラマーを介さずプランナーが
    /// Unity エディタ上で敵を新規作成・バランス調整できるワークフローを実現する。
    /// Range / Tooltip / Header で入力ミスを防ぎ、調整コストを下げている。
    /// </summary>
    [CreateAssetMenu(menuName = "CreateConfig/EnemyConfig", fileName = "EnemyConfig")]
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
        [Tooltip("攻撃範囲（m）"), Range(0.1f, 30.0f)] public float attackRange;
        [Tooltip("攻撃頻度（s）"), Range(0.1f, 30.0f)] public float attackColdDownTime;

        [Header("プレハブ")]
        public GameObject prefab;

        [Header("弾の種類")] public BulletType bulletType;

        // 移動の「戦略」もデータとして差し替え可能（直進・上下動など）
        [Header("MoveStrategy")] public MoveStrategy moveStrategy;
    }

    /// <summary>敵の種類（和風モチーフの妖怪たち）。</summary>
    public enum EnemyType
    {
        一ツ目小僧,
        轆轤首,
        鎌鼬,
        鬼,
        餓者髑髏
    }
}
