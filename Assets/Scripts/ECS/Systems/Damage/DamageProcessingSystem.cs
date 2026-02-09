using Unity.Burst;
using Unity.Entities;

[BurstCompile]
public partial struct DamageProcessingSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<IncomingDamage>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        foreach (var (hitPoints, damageBuffer, entity) in SystemAPI.Query<RefRW<CurrentHitPoints>, DynamicBuffer<IncomingDamage>>().WithDisabled<DestroyEntityFlag>().WithEntityAccess())
        {
            foreach (var damage in damageBuffer)
            {
                hitPoints.ValueRW.Value -= damage.Value;
            }
            damageBuffer.Clear();

            if (hitPoints.ValueRO.Value <= 0)
            {
                SystemAPI.SetComponentEnabled<DestroyEntityFlag>(entity, true);
            }

            if (SystemAPI.HasComponent<UpdateHealthUIFlag>(entity))
            {
                SystemAPI.SetComponentEnabled<UpdateHealthUIFlag>(entity, true);
            }
        }
    }
}
