using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public struct WeaponTag : IComponentData { }

public struct WeaponData : IComponentData
{
    public float Damage;
    public float Cooldown;
    public float TimeSinceLastAttack;
}

public struct WeaponOwner : IComponentData
{
    public Entity Owner;
}

public struct NearestEnemyTarget : IComponentData
{
    public Entity TargetEntity;
    public float3 TargetPosition;
    public float DistanceSquared;
    public bool IsValid;
}

public struct ProjectileWeaponConfig : IComponentData
{
    public Entity ProjectilePrefab;
    public float ProjectileSpeed;
    public float ProjectileLifetime;
    public int ProjectileCount;
    public float SpreadAngle;
}

public struct AreaWeaponConfig : IComponentData
{
    public float Radius;
}

public class WeaponAuthoring : MonoBehaviour
{
    public float BaseDamage = 10f;
    public float Cooldown = 1f;

    public GameObject ProjectilePrefab;
    public float ProjectileSpeed = 15f;
    public float ProjectileLifetime = 2f;
    public int ProjectileCount = 1;
    public float SpreadAngle = 15f;

    private class Baker : Baker<WeaponAuthoring>
    {
        public override void Bake(WeaponAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent<WeaponTag>(entity);
            AddComponent(entity, new WeaponData
            {
                Damage = authoring.BaseDamage,
                Cooldown = authoring.Cooldown,
                TimeSinceLastAttack = authoring.Cooldown
            });
            AddComponent<WeaponOwner>(entity);
            AddComponent<NearestEnemyTarget>(entity);

            AddComponent(entity, new ProjectileWeaponConfig
            {
                ProjectilePrefab = GetEntity(authoring.ProjectilePrefab, TransformUsageFlags.Dynamic),
                ProjectileSpeed = authoring.ProjectileSpeed,
                ProjectileLifetime = authoring.ProjectileLifetime,
                ProjectileCount = authoring.ProjectileCount,
                SpreadAngle = authoring.SpreadAngle
            });
        }
    }
}
