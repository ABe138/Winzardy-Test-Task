using Unity.Burst;
using Unity.Entities;

public partial struct DamageProcessingSystem : ISystem
{
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

[UpdateInGroup(typeof(PresentationSystemGroup))]
public partial struct UpdatePlayerHealthUISystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        foreach (var(currentHealth, maxHealth, updateFlag) in SystemAPI.Query<RefRO<CurrentHitPoints>, RefRO<MaxHitPoints>, EnabledRefRW<UpdateHealthUIFlag>>())
        {
            PlayerHUDManager.Instance.UpdatePlayerHealthText(currentHealth.ValueRO.Value, maxHealth.ValueRO.Value);
            updateFlag.ValueRW = false;
        }
    }
}
