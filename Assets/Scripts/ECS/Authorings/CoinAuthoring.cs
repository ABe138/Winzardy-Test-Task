using Unity.Entities;
using UnityEngine;

public struct CoinTag : IComponentData { }

public class CoinAuthoring : MonoBehaviour
{
    private class Baker : Baker<CoinAuthoring>
    {
        public override void Bake(CoinAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent<CoinTag>(entity);
            AddComponent<DestroyEntityFlag>(entity);
            SetComponentEnabled<DestroyEntityFlag>(entity, false);
        }
    }
}
