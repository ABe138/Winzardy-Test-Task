using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateBefore(typeof(TransformSystemGroup))]
public partial struct EnemySpawnSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<EnemySpawnData>();
        state.RequireForUpdate<PlayerTag>();
    }

    public void OnUpdate(ref SystemState state)
    {
        var deltaTime = SystemAPI.Time.DeltaTime;

        var playerEntity = SystemAPI.GetSingletonEntity<PlayerTag>();
        var playerPos = SystemAPI.GetComponentRO<LocalTransform>(playerEntity).ValueRO.Position;

        var ecb = new EntityCommandBuffer(Allocator.Temp);

        foreach (var spawnData in SystemAPI.Query<RefRW<EnemySpawnData>>())
        {
            spawnData.ValueRW.TimeSinceLastSpawn += deltaTime;

            if (spawnData.ValueRO.TimeSinceLastSpawn >= spawnData.ValueRO.SpawnCooldown)
            {
                spawnData.ValueRW.TimeSinceLastSpawn = 0f;

                var enemy = ecb.Instantiate(spawnData.ValueRO.SpawnEntity);

                var spawnPos = GetSpawnPositionOutsideScreen(playerPos);
                ecb.SetComponent(enemy, LocalTransform.FromPosition(spawnPos));
            }
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }

    private float3 GetSpawnPositionOutsideScreen(float3 playerPos)
    {
        var minDistance = 15f;
        var maxDistance = 20f;

        var random = new Unity.Mathematics.Random((uint)System.DateTime.Now.Ticks);

        var angle = random.NextFloat(0f, math.PI * 2f);

        var distance = random.NextFloat(minDistance, maxDistance);

        var offset = new float3(math.cos(angle) * distance, 0f, math.sin(angle) * distance);

        return playerPos + offset;
    }
}