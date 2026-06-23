using System;
using System.Collections.Generic;
using UnityEngine;

namespace RhythmGame
{
    /// <summary>
    /// 現在落下中のノーツをレーンごとにヒット時刻順で保持する。
    /// これにより、キー入力時にそのレーンで最も古い未判定ノーツを即座に取得できる。
    /// 判定ロジック（Perfect/Good/Miss）はStep2で追加。
    /// </summary>
    public class JudgementManager : MonoBehaviour
    {
        [SerializeField] private ChartPlayer chartPlayer;
        [SerializeField] private float perfectJudgement = 0.05f;
        [SerializeField] private float goodJudgement = 0.12f;
        [SerializeField] private float badJudgement = 0.2f;

        // レーンごとに1つのQueue。先頭は常に次に叩くべきノーツ。
        private Queue<ActiveNote>[] _laneQueues;

        public Action<int,JudgeType> OnNoteBeJudged;

        /// <summary>落下中のノーツと、その画面上のオブジェクトを紐付けた構造体。</summary>
        public readonly struct ActiveNote
        {
            public readonly Note Note;        // タイミングデータ（time, lane など）
            public readonly NodeObject Object; // 画面上の視覚ノーツ
            public readonly int UseId;         // 生成時に記録したObject.UseId

            public ActiveNote(Note note, NodeObject obj)
            {
                Note = note;
                Object = obj;
                UseId = obj != null ? obj.UseId : -1;
            }

            // 視覚オブジェクトがまだこのノーツのものであるのは、その後に別のノーツへ
            // 再利用されていない場合のみ（再利用されるとUseIdが変化している）。
            public bool IsObjectStillMine =>
                Object != null && Object.UseId == UseId;
        }

        public enum JudgeType
        {
            None,
            Perfect,
            Good,
            Bad,
            Miss
        }

        private void Awake()
        {
            // レーン数は譜面から決まる。読み込まれるまでは暫定で4とする。
            BuildQueues(4);
        }

        private void Update()
        {
            if (chartPlayer == null || _laneQueues == null) return;

            float now = chartPlayer.SongTime;

            // 入力されないままヒット窓を完全に過ぎたノーツはMiss。
            // 各Queueの先頭が最も古いので、そこから順に処理していく。
            for (int lane = 0; lane < _laneQueues.Length; lane++)
            {
                var queue = _laneQueues[lane];
                while (queue.Count > 0 &&
                       now - queue.Peek().Note.time > badJudgement)
                {
                    queue.Dequeue();
                    // 視覚ノーツはそのまま落下させる。NoteReturnLineを越えた時点で
                    // 自身でプールに返却される。
                    OnNoteBeJudged?.Invoke(lane, JudgeType.Miss);
                }
            }
        }

        private void BuildQueues(int laneCount)
        {
            _laneQueues = new Queue<ActiveNote>[laneCount];
            for (int i = 0; i < laneCount; i++)
            {
                _laneQueues[i] = new Queue<ActiveNote>();
            }
        }

        /// <summary>
        /// NoteSpawnerがノーツの視覚オブジェクトを生成した直後に呼ばれる。
        /// そのノーツを所属レーンの判定対象として登録する。
        /// </summary>
        public void Register(Note note, NodeObject obj)
        {
            if (note.lane < 0 || note.lane >= _laneQueues.Length)
            {
                Debug.LogWarning($"[Judge] note.lane {note.lane} out of range.");
                return;
            }
            _laneQueues[note.lane].Enqueue(new ActiveNote(note, obj));
        }

        /// <summary>
        /// | 判定 | 許容 ||---|---|
        ///| Perfect | ±0.05秒 |
        ///| Good    | ±0.12秒 |
        ///| Bad     | ±0.20秒 |
        ///| 窓外     | 無視（早押し）／ Update で見逃しMiss（遅れ）|
        /// </summary>
        /// <param name="lane"></param>
        /// <param name="time"></param>
        public void NoteJudgement(int lane, float time)
        {
            if (_laneQueues[lane].Count == 0)
            {
                // レーンが空：叩くべきノーツのない空打ち。無視する。
                return;
            }

            // まずPeekする。範囲内だと分かるまでノーツを消費しない。
            ActiveNote active = _laneQueues[lane].Peek();
            var diff = Mathf.Abs(active.Note.time - time);

            // ヒット窓の外：早押し・空打ちとして扱い、ノーツはQueueに残す。
            // 遅れノーツはここではなくUpdateでMissとして処理される。
            if (diff > badJudgement)
            {
                return;
            }

            JudgeType result;
            if (diff <= perfectJudgement) result = JudgeType.Perfect;
            else if (diff <= goodJudgement) result = JudgeType.Good;
            else result = JudgeType.Bad;

            // ノーツを消費：Queueから取り除き、画面上のオブジェクトを片付ける。
            // 視覚オブジェクトの返却は、まだこのノーツのものである場合のみ行う。
            // プールされたインスタンスが既に後続ノーツへ再利用されている場合
            // （例：元のノーツが先にリターンラインを越えた）、誤って返却すると
            // 同じレーンの別ノーツを消してしまうため。
            _laneQueues[lane].Dequeue();
            if (active.IsObjectStillMine) active.Object.ReturnToPool();

            OnNoteBeJudged?.Invoke(lane, result);
        }
    }
}
