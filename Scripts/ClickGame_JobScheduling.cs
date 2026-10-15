using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

/// <summary>
/// BallPhysicsJob の実行と結果の反映を管理するクラスの抜粋。
/// Update でジョブを登録し、LateUpdate で完了を待って結果を反映する。
/// 一時配列はジョブの完了後に解放する。
/// </summary>
public partial class BallSimulationManager : MonoBehaviour
{
    // 永続データ（NativeList は動的にサイズ変更が可能）
    private NativeList<float3> _positions;
    private NativeList<float3> _velocities;
    private NativeArray<float3> _pinPositions;

    // フレームごとに確保する一時データ（読み取り専用入力）
    private NativeArray<float3> _inputPositionsCopy;
    private NativeArray<float3> _inputVelocitiesCopy;
    private NativeArray<int> _pinBeHit;

    private JobHandle _jobHandle;
    private bool _jobScheduled;

    private void Update()
    {
        _jobScheduled = false;
        if (_positions.Length == 0) return;

        int count = _positions.Length;

        // このフレーム用の入力スナップショットを確保（Allocator.TempJob = 数フレーム以内に破棄）
        _inputPositionsCopy = new NativeArray<float3>(count, Allocator.TempJob);
        _inputVelocitiesCopy = new NativeArray<float3>(count, Allocator.TempJob);
        _pinBeHit = new NativeArray<int>(_pinPositions.Length, Allocator.TempJob);

        _inputPositionsCopy.CopyFrom(_positions.AsArray());
        _inputVelocitiesCopy.CopyFrom(_velocities.AsArray());

        // ジョブを構築し、64 要素ごとのバッチでワーカースレッドへ分散
        var job = new BallPhysicsJob
        {
            Positions = _positions.AsArray(),
            Velocities = _velocities.AsArray(),
            PinPositions = _pinPositions,
            PinBeHit = _pinBeHit,
            InputPositions = _inputPositionsCopy,
            InputVelocities = _inputVelocitiesCopy,
            DeltaTime = Time.deltaTime,
            Gravity = 9.80665f
            // … その他パラメータは省略
        };
        _jobHandle = job.Schedule(count, 64);
        _jobScheduled = true;
    }

    private void LateUpdate()
    {
        if (!_jobScheduled) return;

        // ジョブの完了後に一時配列を解放するよう、Dispose に依存関係を渡す。
        _inputPositionsCopy.Dispose(_jobHandle);
        _inputVelocitiesCopy.Dispose(_jobHandle);

        // LateUpdate でジョブの完了を待ってから結果を反映する
        _jobHandle.Complete();

        // 計算結果をメインスレッドで Transform へ反映する。
        for (int i = 0; i < _positions.Length; i++)
        {
            // _ballTransforms[i].position = _positions[i]; など
        }

        _pinBeHit.Dispose(_jobHandle);
    }

    private void OnDestroy()
    {
        // 永続データは明示的に破棄してメモリリークを防ぐ
        if (_positions.IsCreated) _positions.Dispose();
        if (_velocities.IsCreated) _velocities.Dispose();
        if (_pinPositions.IsCreated) _pinPositions.Dispose();
    }
}
