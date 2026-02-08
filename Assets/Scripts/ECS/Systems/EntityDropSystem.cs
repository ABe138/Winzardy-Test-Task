using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public struct DropData : IComponentData
{
    public float DropChance;
    public Entity DropEntity;
}

[UpdateAfter(typeof(DamageProcessingSystem))]
[UpdateBefore(typeof(DestroyEntitySystem))]
[BurstCompile]
public partial struct EntityDropSystem : ISystem
{
    private Random _random;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<BeginInitializationEntityCommandBufferSystem.Singleton>();
        _random = Random.CreateFromIndex(state.GlobalSystemVersion);
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var beginEcbSystem = SystemAPI.GetSingleton<BeginInitializationEntityCommandBufferSystem.Singleton>();
        var beginEcb = beginEcbSystem.CreateCommandBuffer(state.WorldUnmanaged);

        foreach (var (_, dropData, entity) in SystemAPI.Query<DestroyEntityFlag, DropData>().WithEntityAccess())
        {
            if (_random.NextFloat() > dropData.DropChance) continue;

            var spawnPosition = SystemAPI.GetComponentRO<LocalTransform>(entity).ValueRO.Position;

            var dropEntity = beginEcb.Instantiate(dropData.DropEntity);
            beginEcb.SetComponent(dropEntity, LocalTransform.FromPosition(spawnPosition));
        }
    }
}