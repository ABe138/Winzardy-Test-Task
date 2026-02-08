using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[BurstCompile]
[UpdateBefore(typeof(TransformSystemGroup))]
public partial struct TargetAcquisitionSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PlayerTag>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var playerEntity = SystemAPI.GetSingletonEntity<PlayerTag>();
        var playerPos = SystemAPI.GetComponentRO<LocalTransform>(playerEntity).ValueRO.Position;

        var minDistSq = float.MaxValue;
        var nearestEnemy = Entity.Null;
        var nearestPos = float3.zero;

        foreach (var (transform, entity) in SystemAPI.Query<RefRO<LocalTransform>>().WithAll<EnemyTag>().WithDisabled<DestroyEntityFlag>().WithEntityAccess())
        {
            var distSq = math.distancesq(playerPos, transform.ValueRO.Position);
            if (distSq < minDistSq)
            {
                minDistSq = distSq;
                nearestEnemy = entity;
                nearestPos = transform.ValueRO.Position;
            }
        }

        var isValid = nearestEnemy != Entity.Null;
        foreach (var target in SystemAPI.Query<RefRW<NearestEnemyTarget>>().WithAll<WeaponTag>())
        {
            target.ValueRW.TargetEntity = nearestEnemy;
            target.ValueRW.TargetPosition = nearestPos;
            target.ValueRW.DistanceSquared = minDistSq;
            target.ValueRW.IsValid = isValid;
        }
    }
}
