using UnityEngine;
using TMPro;

public class EnemyHealthView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _healthText;
    [SerializeField] private RectTransform _rectTransform;

    public RectTransform RectTransform => _rectTransform;

    public void SetHealth(int current, int max)
    {
        _healthText.text = $"{current}/{max}";
    }

    public void Show() => gameObject.SetActive(true);
}
