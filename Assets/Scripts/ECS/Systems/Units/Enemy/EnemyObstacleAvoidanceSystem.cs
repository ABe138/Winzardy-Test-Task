using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

[UpdateAfter(typeof(EnemyFollowPlayerSystem))]
[BurstCompile]
public partial struct EnemyObstacleAvoidanceSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PhysicsWorldSingleton>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var physicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>();

        var job = new ObstacleAvoidanceJob
        {
            CollisionWorld = physicsWorld.CollisionWorld
        };

        state.Dependency = job.ScheduleParallel(state.Dependency);
    }
}


[WithAll(typeof(EnemyTag))]
[BurstCompile]
public partial struct ObstacleAvoidanceJob : IJobEntity
{
    [ReadOnly] public CollisionWorld CollisionWorld;

    private void Execute(
        ref MoveDirection direction,
        in LocalTransform transform,
        in ObstacleAvoidanceData avoidance,
        in Entity entity)
    {
        if (math.lengthsq(direction.Value) < 0.001f) return;

        var position = transform.Position;
        var forward = new float3(direction.Value.x, 0, direction.Value.y);
        var normalizedForward = math.normalize(forward);

        var rayOrigin = position + new float3(0, 0.5f, 0);

        var forwardBlocked = CastObstacleRay(rayOrigin, normalizedForward, avoidance.DetectionDistance, avoidance.ObstacleLayerMask, out float forwardFraction);
        if (!forwardBlocked) return;

        var angleRad = math.radians(avoidance.SideRayAngle);
        var rightDir = math.rotate(quaternion.AxisAngle(math.up(), angleRad), normalizedForward);
        var leftDir = math.rotate(quaternion.AxisAngle(math.up(), -angleRad), normalizedForward);

        CastObstacleRay(rayOrigin, rightDir, avoidance.DetectionDistance, avoidance.ObstacleLayerMask, out float rightFraction);
        CastObstacleRay(rayOrigin, leftDir, avoidance.DetectionDistance, avoidance.ObstacleLayerMask, out float leftFraction);

        var avoidanceDir = float3.zero;

        if (rightFraction > leftFraction)
        {
            avoidanceDir = rightDir;
        }
        else if (leftFraction > rightFraction)
        {
            avoidanceDir = leftDir;
        }
        else
        {
            var perpendicular = math.cross(normalizedForward, math.up());
            avoidanceDir = perpendicular;
        }

        var proximityFactor = 1f - forwardFraction;
        var blendFactor = avoidance.AvoidanceStrength * proximityFactor;

        var blendedDirection = math.lerp(normalizedForward, math.normalize(avoidanceDir), blendFactor);

        blendedDirection = math.normalize(blendedDirection);
        direction.Value = new float2(blendedDirection.x, blendedDirection.z);
    }

    private bool CastObstacleRay(
        float3 origin,
        float3 direction,
        float distance,
        uint obstacleLayerMask,
        out float hitFraction)
    {
        hitFraction = 1f;

        var rayInput = new RaycastInput
        {
            Start = origin,
            End = origin + direction * distance,
            Filter = new CollisionFilter
            {
                BelongsTo = ~0u,
                CollidesWith = obstacleLayerMask,
                GroupIndex = 0
            }
        };

        if (CollisionWorld.CastRay(rayInput, out RaycastHit hit))
        {
            hitFraction = hit.Fraction;
            return true;
        }

        return false;
    }
}
