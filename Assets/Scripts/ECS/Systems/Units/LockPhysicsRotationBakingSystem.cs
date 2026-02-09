using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;

[WorldSystemFilter(WorldSystemFilterFlags.BakingSystem)]
[UpdateInGroup(typeof(PostBakingSystemGroup))]
[BurstCompile]
public partial struct LockPhysicsRotationBakingSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        foreach (var physicsMass in SystemAPI.Query<RefRW<PhysicsMass>>().WithAny<PlayerTag, EnemyTag>().WithOptions(EntityQueryOptions.IncludePrefab))
        {
            physicsMass.ValueRW.InverseInertia = float3.zero;   //fix for unwanted rotations after to collisions
        }
    }
}
