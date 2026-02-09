using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[BurstCompile]
public partial struct EnemySpawnSystem : ISystem
{
    private Random _random;

    public void OnCreate(ref SystemState state)
    {
        _random = new Random((uint)System.Environment.TickCount);

        state.RequireForUpdate<EnemySpawnData>();
        state.RequireForUpdate<PlayerTag>();
        state.RequireForUpdate<BeginInitializationEntityCommandBufferSystem.Singleton>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var deltaTime = SystemAPI.Time.DeltaTime;

        var playerEntity = SystemAPI.GetSingletonEntity<PlayerTag>();
        var playerPos = SystemAPI.GetComponentRO<LocalTransform>(playerEntity).ValueRO.Position;

        var ecbSystem = SystemAPI.GetSingleton<BeginInitializationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSystem.CreateCommandBuffer(state.WorldUnmanaged);

        foreach (var spawnData in SystemAPI.Query<RefRW<EnemySpawnData>>())
        {
            spawnData.ValueRW.TimeSinceLastSpawn += deltaTime;

            if (spawnData.ValueRO.TimeSinceLastSpawn >= spawnData.ValueRO.SpawnCooldown)
            {
                spawnData.ValueRW.TimeSinceLastSpawn = 0f;

                var enemy = ecb.Instantiate(spawnData.ValueRO.SpawnEntity);

                var spawnPos = GetSpawnPositionOutsideScreen(playerPos + spawnData.ValueRO.SpawnOffset, spawnData.ValueRO.SpawnDistance, ref _random);
                ecb.SetComponent(enemy, LocalTransform.FromPosition(spawnPos));
            }
        }
    }

    private float3 GetSpawnPositionOutsideScreen(float3 spawnCenter, float spawnRadius, ref Random random)
    {
        var angle = random.NextFloat(0f, math.PI * 2f);
        var offset = new float3(math.cos(angle) * spawnRadius, 0f, math.sin(angle) * spawnRadius);
        return spawnCenter + offset;
    }
}