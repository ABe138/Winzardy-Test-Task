using System.Collections.Generic;
using UnityEngine;

public class EnemyHealthViewManager : Singleton<EnemyHealthViewManager>
{
    protected override bool Persistent => false;

    [SerializeField] private EnemyHealthView _healthViewPrefab;
    [SerializeField] private RectTransform _healthViewsContainer;

    [SerializeField] private Camera _mainCamera;

    private Vector3 _worldOffset = new Vector3(0, 2f, 0);

    private Dictionary<int, EnemyHealthView> _activeElements = new();
    private int _nextId = -1;

    public int Register()
    {
        var id = _nextId++;
        var element = GetFromPool();
        element.Show();
        _activeElements[id] = element;
        return id;
    }

    public void Unregister(int id)
    {
        if (_activeElements.TryGetValue(id, out var element))
        {
            PoolingManager.Instance.Release(element);
            _activeElements.Remove(id);
        }
    }

    public void UpdateHealthView(int id, Vector3 worldPosition, int currentHP, int maxHP)
    {
        if (!_activeElements.TryGetValue(id, out var element)) return;

        var screenPos = _mainCamera.WorldToScreenPoint(worldPosition + _worldOffset);

        if (screenPos.z < 0)
        {
            Unregister(id);
            return;
        }

        element.Show();
        element.RectTransform.position = screenPos;
        element.SetHealth(currentHP, maxHP);
    }

    private EnemyHealthView GetFromPool()
    {
        var newHealthView = PoolingManager.Instance.Pool<EnemyHealthView>(_healthViewPrefab, _healthViewsContainer, Vector3.zero, Quaternion.identity);
        return newHealthView;
    }
}
