using TMPro;
using UnityEngine;

public class PlayerHUDManager : Singleton<PlayerHUDManager>
{
    protected override bool Persistent => false;

    [SerializeField] private TextMeshProUGUI _playerHealthText;
    [SerializeField] private TextMeshProUGUI _coinsCounterText;

    public void UpdatePlayerHealthText(int current, int max)
    {
        _playerHealthText.text = $"Health: {current}/{max}";
    }

    public void UpdateCoinsCollectedText(int value)
    {
        _coinsCounterText.text = $"Coins: {value}";
    }
}
