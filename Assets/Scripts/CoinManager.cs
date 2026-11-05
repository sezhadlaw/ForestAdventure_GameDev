
using UnityEngine;
using TMPro;

public class CoinManager : MonoBehaviour
{
    public TextMeshProUGUI coinText;

    private int coinsCollected = 0;
    private int totalCoins;

    public static bool AllCoinsCollected
    {
        get
        {
            foreach (Coin coin in FindObjectsByType<Coin>(FindObjectsSortMode.None))
                if (!coin.IsCollected) return false;
            return true;
        }
    }

    void Awake()
    {
        totalCoins = FindObjectsByType<Coin>(
            FindObjectsSortMode.None
        ).Length;

        UpdateCoinText();
    }

    public void AddCoin()
    {
        coinsCollected++;
        UpdateCoinText();
    }

    void UpdateCoinText()
    {
        if (coinText == null) return;
        coinText.text = "Coins: " +
                        coinsCollected + "/" + totalCoins;
    }
}
