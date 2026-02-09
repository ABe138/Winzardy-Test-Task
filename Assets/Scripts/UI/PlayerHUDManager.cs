using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerHUDManager : Singleton<PlayerHUDManager>
{
    protected override bool Persistent => false;

    [SerializeField] private TextMeshProUGUI _playerHealthText;
    [SerializeField] private TextMeshProUGUI _coinsCounterText;

    [SerializeField] private GameObject _gameOverScreen;
    [SerializeField] private Button _restartButton;

    protected override void Awake()
    {
        base.Awake();
        _restartButton.onClick.AddListener(Restart);
    }

    public void UpdatePlayerHealthText(int current, int max)
    {
        _playerHealthText.text = $"Health: {current}/{max}";
    }

    public void UpdateCoinsCollectedText(int value)
    {
        _coinsCounterText.text = $"Coins: {value}";
    }

    public void ShowGameOverScreen() 
    {
        StartCoroutine(DelayedShow());
    }

    private IEnumerator DelayedShow() 
    {
        yield return new WaitForSeconds(2f);
        _gameOverScreen.gameObject.SetActive(true);
    }

    private void Restart()
    {
        SceneManager.LoadScene(0);
    }
}
