using UnityEngine;
using System.Collections.Generic;

public class PoolingManager : Singleton<PoolingManager>
{
    protected override bool Persistent => false;

    class PoolData
    {
        public Queue<MonoBehaviour> Free = new Queue<MonoBehaviour>();
        public MonoBehaviour Original;
        public Transform Container;
        public bool Expandable = true;
    }

    Dictionary<int, PoolData> _pools = new Dictionary<int, PoolData>();

    public T Pool<T>(T prefab, Transform parentTo, Vector3 position, Quaternion rotation) where T : MonoBehaviour
    {
        if (prefab == null) return null;
        int id = prefab.gameObject.GetInstanceID();
        if (!_pools.TryGetValue(id, out var pool))
        {
            pool = CreatePool(prefab, 0, true);
            _pools[id] = pool;
        }

        MonoBehaviour mb;
        if (pool.Free.Count > 0)
        {
            mb = pool.Free.Dequeue();
        }
        else if (pool.Expandable)
        {
            mb = Instantiate(pool.Original as T, position: position, rotation: rotation, parent: parentTo);
            MarkAsPooled(mb.gameObject, id);
        }
        else
        {
            return null;
        }

        mb.transform.SetParent(parentTo);
        mb.transform.localPosition = position;
        mb.transform.localRotation = rotation;
        mb.transform.localScale = Vector3.one;
        mb.gameObject.SetActive(true);
        return mb as T;
    }

    public void Release(MonoBehaviour instance)
    {
        if (instance == null) return;
        int prefabId = GetOriginalPrefabId(instance.gameObject);
        if (prefabId == 0 || !_pools.TryGetValue(prefabId, out var pool))
        {
            Destroy(instance.gameObject);
            return;
        }

        instance.gameObject.SetActive(false);
        instance.transform.SetParent(pool.Container, false);
        pool.Free.Enqueue(instance);
    }

    PoolData CreatePool(MonoBehaviour prefab, int initialSize, bool expandable)
    {
        var pool = new PoolData
        {
            Original = prefab,
            Container = new GameObject(prefab.name + "_Pool").transform,
            Expandable = expandable
        };
        pool.Container.SetParent(transform, false);

        for (int i = 0; i < initialSize; i++)
        {
            var mb = Instantiate(prefab, pool.Container);
            mb.gameObject.SetActive(false);
            pool.Free.Enqueue(mb);
            MarkAsPooled(mb.gameObject, prefab.gameObject.GetInstanceID());
        }

        return pool;
    }

    void MarkAsPooled(GameObject instance, int prefabId)
    {
        var tag = instance.GetComponent<PooledObjectTag>();
        if (tag == null) tag = instance.AddComponent<PooledObjectTag>();
        tag.PrefabId = prefabId;
    }

    int GetOriginalPrefabId(GameObject instance)
    {
        var tag = instance.GetComponent<PooledObjectTag>();
        return tag != null ? tag.PrefabId : 0;
    }

    public void ClearAllPools()
    {
        foreach (var kv in _pools)
        {
            var pool = kv.Value;
            if (pool.Container != null) Destroy(pool.Container.gameObject);
            while (pool.Free.Count > 0) Destroy(pool.Free.Dequeue());
        }
        _pools.Clear();
    }
}
