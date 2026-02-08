using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateBefore(typeof(TransformSystemGroup))]
public partial struct PlayerSpawnSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<Respawn>();
        state.RequireForUpdate<PlayerSpawnData>();
    }

    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);

        foreach (var (spawnTag, spawnData, entity) in SystemAPI.Query<RefRW<Respawn>, RefRO<PlayerSpawnData>>().WithEntityAccess())
        {
            var player = ecb.Instantiate(spawnData.ValueRO.SpawnEntity);
            ecb.SetComponent(player, LocalTransform.FromPosition(float3.zero));
            ecb.SetComponentEnabled<Respawn>(entity, false);
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}