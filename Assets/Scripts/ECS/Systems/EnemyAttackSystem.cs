using Unity.Entities;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateAfter(typeof(EnemyFollowPlayerSystem))]
public partial struct EnemyAttackSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PlayerTag>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var playerEntity = SystemAPI.GetSingletonEntity<PlayerTag>();

        if (SystemAPI.IsComponentEnabled<DestroyEntityFlag>(playerEntity)) return;

        var playerTransform = SystemAPI.GetComponentRO<LocalTransform>(playerEntity);
        var playerDamageBuffer = SystemAPI.GetBuffer<IncomingDamage>(playerEntity);

        var deltaTime = SystemAPI.Time.DeltaTime;
        var playerPos = playerTransform.ValueRO.Position.xz;

        foreach (var (attack, transform) in
            SystemAPI.Query<RefRW<EnemyAttack>, RefRO<LocalTransform>>().WithAll<EnemyTag>())
        {
            attack.ValueRW.TimeSinceLastAttack += deltaTime;

            var enemyPos = transform.ValueRO.Position.xz;
            var distanceSq = math.lengthsq(playerPos - enemyPos);

            if (distanceSq <= attack.ValueRO.AttackRange * attack.ValueRO.AttackRange)
            {
                if (attack.ValueRO.TimeSinceLastAttack >= attack.ValueRO.AttackCooldown)
                {
                    playerDamageBuffer.Add(new IncomingDamage { Value = attack.ValueRO.Damage });
                    attack.ValueRW.TimeSinceLastAttack = 0f;
                }
            }
        }
    }
}
