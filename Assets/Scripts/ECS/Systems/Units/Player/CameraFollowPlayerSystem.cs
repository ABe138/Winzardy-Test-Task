using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

public partial class CameraFollowPlayerSystem : SystemBase
{
    private Transform _target;

    protected override void OnUpdate()
    {
        if (_target == null)
        {
            var cameraTarget = Object.FindFirstObjectByType<CameraPlayerTarget>();
            if (cameraTarget == null) return;
            _target = cameraTarget.transform;
        }

        foreach (var transform in SystemAPI.Query<RefRO<LocalTransform>>().WithAll<PlayerTag>())
        {
            _target.position = transform.ValueRO.Position;
        }
    }
}