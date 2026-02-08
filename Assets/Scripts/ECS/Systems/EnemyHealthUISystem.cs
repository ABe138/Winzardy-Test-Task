using System.Collections.Generic;
using Unity.Entities;
using Unity.Transforms;

[UpdateInGroup(typeof(PresentationSystemGroup))]
public partial class EnemyHealthUISystem : SystemBase
{
    private Dictionary<Entity, int> _entityToUIId = new();
    private List<Entity> _toRemove = new();

    protected override void OnUpdate()
    {
        if (EnemyHealthViewManager.Instance == null) return;

        var manager = EnemyHealthViewManager.Instance;

        foreach (var (hp, maxHp, transform, entity) in
            SystemAPI.Query<RefRO<CurrentHitPoints>, RefRO<MaxHitPoints>, RefRO<LocalTransform>>()
                     .WithAll<EnemyTag>()
                     .WithDisabled<DestroyEntityFlag>()
                     .WithEntityAccess())
        {
            if (!_entityToUIId.TryGetValue(entity, out var id))
            {
                id = manager.Register();
                _entityToUIId[entity] = id;
            }

            manager.UpdateHealthView(id, transform.ValueRO.Position, hp.ValueRO.Value, maxHp.ValueRO.Value);
        }

        foreach (var (entity, id) in _entityToUIId)
        {
            if (!EntityManager.Exists(entity))
            {
                manager.Unregister(id);
                _toRemove.Add(entity);
            }
        }

        foreach (var entity in _toRemove) _entityToUIId.Remove(entity);

        _toRemove.Clear();
    }

    protected override void OnDestroy()
    {
        if (EnemyHealthViewManager.Instance != null)
        {
            foreach (var id in _entityToUIId.Values)
            {
                EnemyHealthViewManager.Instance.Unregister(id);
            }
        }
        _entityToUIId.Clear();
    }
}
