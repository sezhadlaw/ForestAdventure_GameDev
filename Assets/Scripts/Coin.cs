
using UnityEngine;

public class Coin : MonoBehaviour
{
    public bool IsCollected { get; private set; }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsCollected && other.GetComponentInParent<PlayerController>() != null)
        {
            IsCollected = true;
            SoundManager.Play(Sfx.Coin, 0.8f);
            if (LevelLoader.Instance != null) LevelLoader.Instance.AddCoin();
            CoinManager manager =
                FindFirstObjectByType<CoinManager>();

            if (manager != null)
            {
                manager.AddCoin();
            }

            Destroy(gameObject);
            if (LevelLoader.Instance != null && LevelLoader.Instance.BossDefeated) LevelLoader.Instance.TryComplete();
        }
    }
}
