using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;

[BurstCompile]
public partial struct PickUpCoinSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<SimulationSingleton>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var newCollectJob = new CoinTriggerJob
        {
            CoinLookup = SystemAPI.GetComponentLookup<CoinTag>(true),
            CoinsCollectedLookup = SystemAPI.GetComponentLookup<CoinsCollectedCounter>(),
            DestroyEntityLookup = SystemAPI.GetComponentLookup<DestroyEntityFlag>(),
            UpdateCoinsUILookup = SystemAPI.GetComponentLookup<UpdateCoinsUIFlag>()
        };

        var simulationSingleton = SystemAPI.GetSingleton<SimulationSingleton>();
        state.Dependency = newCollectJob.Schedule(simulationSingleton, state.Dependency);
    }
}

[BurstCompile]
public struct CoinTriggerJob : ITriggerEventsJob
{
    [ReadOnly] public ComponentLookup<CoinTag> CoinLookup;

    public ComponentLookup<CoinsCollectedCounter> CoinsCollectedLookup;
    public ComponentLookup<DestroyEntityFlag> DestroyEntityLookup;
    public ComponentLookup<UpdateCoinsUIFlag> UpdateCoinsUILookup;

    public void Execute(TriggerEvent triggerEvent)
    {
        Entity coinEntity;
        Entity playerEntity;

        if (CoinLookup.HasComponent(triggerEvent.EntityA) && CoinsCollectedLookup.HasComponent(triggerEvent.EntityB))
        {
            coinEntity = triggerEvent.EntityA;
            playerEntity = triggerEvent.EntityB;
        }
        else if (CoinLookup.HasComponent(triggerEvent.EntityB) && CoinsCollectedLookup.HasComponent(triggerEvent.EntityA))
        {
            coinEntity = triggerEvent.EntityB;
            playerEntity = triggerEvent.EntityA;
        }
        else
        {
            return;
        }

        var coinsCollected = CoinsCollectedLookup[playerEntity];
        coinsCollected.Value += 1;
        CoinsCollectedLookup[playerEntity] = coinsCollected;

        UpdateCoinsUILookup.SetComponentEnabled(playerEntity, true);

        DestroyEntityLookup.SetComponentEnabled(coinEntity, true);
    }
}

[UpdateInGroup(typeof(PresentationSystemGroup))]
public partial struct UpdateCoinUISystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        foreach (var (coinsCounter, updateFlag) in SystemAPI.Query<CoinsCollectedCounter, EnabledRefRW<UpdateCoinsUIFlag>>())
        {
            PlayerHUDManager.Instance.UpdateCoinsCollectedText(coinsCounter.Value);
            updateFlag.ValueRW = false;
        }
    }
}
