using Unity.Entities;
using UnityEngine;

public struct EnemyTag : IComponentData { }

public struct EnemyAttack : IComponentData
{
    public float AttackRange;
    public float AttackCooldown;
    public float TimeSinceLastAttack;
    public int Damage;
}

public struct ObstacleAvoidanceData : IComponentData
{
    public float DetectionDistance;
    public float SideRayAngle;
    public float AvoidanceStrength;
    public uint ObstacleLayerMask;
}

public class EnemyAuthoring : MonoBehaviour
{
    public float AttackRange = 1f;
    public float AttackCooldown = 1f;
    public int AttackDamage = 5;

    public float ObstacleDetectionDistance = 3f;
    public float SideRayAngle = 45f;
    public float AvoidanceStrength = 1f;
    public LayerMask ObstacleLayer;

    public GameObject DropPrefab;
    public float DropChance;

    private class Baker : Baker<EnemyAuthoring>
    {
        public override void Bake(EnemyAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent<EnemyTag>(entity);
            AddComponent(entity, new EnemyAttack
            {
                AttackRange = authoring.AttackRange,
                AttackCooldown = authoring.AttackCooldown,
                TimeSinceLastAttack = 0f,
                Damage = authoring.AttackDamage
            });
            AddComponent(entity, new ObstacleAvoidanceData
            {
                DetectionDistance = authoring.ObstacleDetectionDistance,
                SideRayAngle = authoring.SideRayAngle,
                AvoidanceStrength = authoring.AvoidanceStrength,
                ObstacleLayerMask = (uint)authoring.ObstacleLayer.value
            });
            if (authoring.DropPrefab != null) 
            {
                AddComponent(entity, new DropData
                {
                    DropChance = authoring.DropChance,
                    DropEntity = GetEntity(authoring.DropPrefab, TransformUsageFlags.Dynamic)
                });
            }
        }
    }
}
