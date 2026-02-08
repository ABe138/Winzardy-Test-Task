using Unity.Entities;
using UnityEngine;

public struct PlayerTag : IComponentData { }

public struct CameraTarget : IComponentData
{
    public UnityObjectRef<Transform> CameraTransform;
}

public struct CoinsCollectedCounter : IComponentData
{
    public int Value;
}

public struct UpdateHealthUIFlag : IComponentData, IEnableableComponent { }
public struct UpdateCoinsUIFlag : IComponentData, IEnableableComponent { }

public class PlayerAuthoring : MonoBehaviour
{
    private class Baker : Baker<PlayerAuthoring>
    {
        public override void Bake(PlayerAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent<PlayerTag>(entity);
            AddComponent(entity, new CoinsCollectedCounter { Value = 0 });

            AddComponent<UpdateHealthUIFlag>(entity);
            SetComponentEnabled<UpdateHealthUIFlag>(entity, true);

            AddComponent<UpdateCoinsUIFlag>(entity);
            SetComponentEnabled<UpdateCoinsUIFlag>(entity, true);
        }
    }
}
