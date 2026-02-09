using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Systems;

[UpdateInGroup(typeof(PhysicsSystemGroup))]
[UpdateAfter(typeof(PhysicsSimulationGroup))]
[BurstCompile]
public partial struct ProjectileTriggerSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<SimulationSingleton>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var projectileLookup = SystemAPI.GetComponentLookup<ProjectileData>(true);
        var enemyLookup = SystemAPI.GetComponentLookup<EnemyTag>(true);
        var destroyEntityLookup = SystemAPI.GetComponentLookup<DestroyEntityFlag>();
        var damageBufferLookup = SystemAPI.GetBufferLookup<IncomingDamage>();

        state.Dependency = new ProjectileTriggerJob
        {
            ProjectileLookup = projectileLookup,
            EnemyLookup = enemyLookup,
            DestroyEntityLookup = destroyEntityLookup,
            DamageBufferLookup = damageBufferLookup
        }.Schedule(SystemAPI.GetSingleton<SimulationSingleton>(), state.Dependency);

        state.Dependency.Complete();
    }
}

[BurstCompile]
public struct ProjectileTriggerJob : ITriggerEventsJob
{
    [ReadOnly] public ComponentLookup<ProjectileData> ProjectileLookup;
    [ReadOnly] public ComponentLookup<EnemyTag> EnemyLookup;

    public ComponentLookup<DestroyEntityFlag> DestroyEntityLookup;
    public BufferLookup<IncomingDamage> DamageBufferLookup;

    public void Execute(TriggerEvent triggerEvent)
    {
        var entityA = triggerEvent.EntityA;
        var entityB = triggerEvent.EntityB;

        var projectileEntity = default(Entity);
        var enemyEntity = default(Entity);

        if (ProjectileLookup.HasComponent(entityA) && EnemyLookup.HasComponent(entityB))
        {
            projectileEntity = entityA;
            enemyEntity = entityB;
        }
        else if (ProjectileLookup.HasComponent(entityB) && EnemyLookup.HasComponent(entityA))
        {
            projectileEntity = entityB;
            enemyEntity = entityA;
        }
        else
        {
            return;
        }

        if (!DestroyEntityLookup.HasComponent(enemyEntity) || DestroyEntityLookup.IsComponentEnabled(enemyEntity)) return;

        var projectileData = ProjectileLookup[projectileEntity];
        if (DamageBufferLookup.HasBuffer(enemyEntity))
        {
            var damageBuffer = DamageBufferLookup[enemyEntity];
            damageBuffer.Add(new IncomingDamage { Value = (int)projectileData.Damage });
        }

        DestroyEntityLookup.SetComponentEnabled(projectileEntity, true);
    }
}
