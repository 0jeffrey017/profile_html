using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;

/// <summary>
/// 数千個のボールの物理挙動（重力・ボール同士／ピン／壁との衝突）を
/// マルチスレッドで並列計算する Burst コンパイル済みジョブ。
/// IJobParallelFor により 1 ボール = 1 インデックスとして分散処理し、
/// [BurstCompile] でネイティブコードへ最適化することで 60FPS を維持する。
/// </summary>
[BurstCompile]
public struct BallPhysicsJob : IJobParallelFor
{
    // 書き込み対象：計算後の位置・速度・ピンの被弾数
    public NativeArray<float3> Positions;
    public NativeArray<float3> Velocities;
    public NativeArray<int> PinBeHit;

    // 読み取り専用データ（[ReadOnly] でスレッド間の安全性をコンパイラに保証させる）
    [ReadOnly] public NativeArray<float3> PinPositions;
    [ReadOnly] public NativeArray<float3> InputPositions;
    [ReadOnly] public NativeArray<float3> InputVelocities;

    [ReadOnly] public float Radius;
    [ReadOnly] public float PinRadius;
    [ReadOnly] public float3 BoxMin;
    [ReadOnly] public float3 BoxMax;
    [ReadOnly] public float DeltaTime;
    [ReadOnly] public float Gravity;

    public unsafe void Execute(int index)
    {
        float3 pos = InputPositions[index];
        float3 vel = InputVelocities[index];

        // --- 1. 重力を適用して仮の位置を更新 ---
        vel.y -= Gravity * DeltaTime;
        pos += vel * DeltaTime;

        float targetDist = Radius * 2f;
        float targetDistSq = targetDist * targetDist;

        // --- 2. ボール同士の衝突応答（運動量保存に基づくインパルス計算） ---
        for (int i = 0; i < InputPositions.Length; i++)
        {
            if (i == index) continue;

            float3 toOther = pos - InputPositions[i];
            float distSq = math.lengthsq(toOther);
            if (distSq >= targetDistSq) continue; // 接触していなければスキップ

            float dist = math.sqrt(distSq);
            float3 normal = dist > 0.001f ? -toOther / dist : new float3(0, 1, 0);

            float3 relativeVel = InputVelocities[i] - vel;
            float velAlongNormal = math.dot(relativeVel, normal);
            if (velAlongNormal >= 0) continue; // 離れていく場合は処理不要

            // 反発係数を加味したインパルスで速度を補正
            float restitution = 0.8f;
            float impulse = -(1f + restitution) * velAlongNormal / 2f;
            if (impulse < 0.05f) impulse = 0.1f;
            vel -= impulse * normal;

            // めり込み解消（位置の押し戻し）
            pos += normal * ((targetDist - dist) * 0.6f);
        }

        // --- 3. ピンとの衝突判定（被弾したピンの数をスレッドセーフに加算） ---
        float pinDist = PinRadius + Radius;
        float pinDistSq = pinDist * pinDist;
        for (int i = 0; i < PinPositions.Length; i++)
        {
            float3 toPin = PinPositions[i] - pos;
            float distSq = math.lengthsq(toPin);
            if (distSq >= pinDistSq) continue;

            float dist = math.sqrt(distSq);
            float3 normal = dist > 0.001f ? -toPin / dist : new float3(0, 1, 0);
            float velAlongNormal = math.dot(vel, normal);
            if (velAlongNormal >= 0) continue;

            float pinRestitution = 0.6f;
            vel -= (1f + pinRestitution) * velAlongNormal * normal;
            pos += normal * (pinDist - dist);

            // 複数スレッドから同じピンを同時更新しうるため Interlocked で原子的に加算
            System.Threading.Interlocked.Increment(ref ((int*)PinBeHit.GetUnsafePtr())[i]);
        }

        // --- 4. 箱（プレイ領域）の壁で跳ね返す ---
        float boundRestitution = 0.6f;
        if (pos.x - Radius < BoxMin.x) { pos.x = BoxMin.x + Radius; vel.x = math.abs(vel.x) * boundRestitution; }
        else if (pos.x + Radius > BoxMax.x) { pos.x = BoxMax.x - Radius; vel.x = -math.abs(vel.x) * boundRestitution; }

        if (pos.y - Radius < BoxMin.y) { pos.y = BoxMin.y + Radius; vel.y = math.abs(vel.y) * boundRestitution; }
        else if (pos.y + Radius > BoxMax.y) { pos.y = BoxMax.y - Radius; vel.y = -math.abs(vel.y) * boundRestitution; }

        if (pos.z - Radius < BoxMin.z) { pos.z = BoxMin.z + Radius; vel.z = math.abs(vel.z) * boundRestitution; }
        else if (pos.z + Radius > BoxMax.z) { pos.z = BoxMax.z - Radius; vel.z = -math.abs(vel.z) * boundRestitution; }

        // --- 5. 計算結果を書き戻す ---
        Positions[index] = pos;
        Velocities[index] = vel;
    }
}
