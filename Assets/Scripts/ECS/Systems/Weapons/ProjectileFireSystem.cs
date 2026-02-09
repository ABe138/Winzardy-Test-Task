using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

[UpdateAfter(typeof(TargetAcquisitionSystem))]
[BurstCompile]
public partial struct ProjectileFireSystem : ISystem
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
        var playerPos = SystemAPI.GetComponentRO<LocalTransform>(playerEntity).ValueRO.Position;

        var deltaTime = SystemAPI.Time.DeltaTime;

        var ecbSystem = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSystem.CreateCommandBuffer(state.WorldUnmanaged);

        foreach (var (weaponData, config, target, weaponEntity) in SystemAPI.Query<RefRW<WeaponData>, RefRO<ProjectileWeaponConfig>, RefRO<NearestEnemyTarget>>().WithAll<WeaponTag>().WithEntityAccess())
        {
            weaponData.ValueRW.TimeSinceLastAttack += deltaTime;

            if (weaponData.ValueRO.TimeSinceLastAttack < weaponData.ValueRO.Cooldown)
                continue;

            if (!target.ValueRO.IsValid)
                continue;

            weaponData.ValueRW.TimeSinceLastAttack = 0f;

            var direction = math.normalize(target.ValueRO.TargetPosition - playerPos);
            var spawnPos = playerPos + SystemAPI.GetComponentRO<LocalTransform>(weaponEntity).ValueRO.Position;

            var projectileCount = config.ValueRO.ProjectileCount;
            var spreadAngleRad = math.radians(config.ValueRO.SpreadAngle);
            var startAngle = -spreadAngleRad * (projectileCount - 1) * 0.5f;

            for (int i = 0; i < projectileCount; i++)
            {
                var angle = startAngle + spreadAngleRad * i;
                var rotatedDir = math.mul(quaternion.AxisAngle(math.up(), angle), direction);

                var projectile = ecb.Instantiate(config.ValueRO.ProjectilePrefab);

                ecb.SetComponent(projectile, LocalTransform.FromPositionRotation(
                    spawnPos,
                    quaternion.LookRotationSafe(rotatedDir, math.up())
                ));

                ecb.SetComponent(projectile, new ProjectileData
                {
                    Damage = weaponData.ValueRO.Damage,
                    TimeRemaining = config.ValueRO.ProjectileLifetime
                });

                ecb.SetComponent(projectile, new PhysicsVelocity
                {
                    Linear = rotatedDir * config.ValueRO.ProjectileSpeed,
                    Angular = float3.zero
                });
            }
        }
    }
}
