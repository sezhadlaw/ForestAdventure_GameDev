
using UnityEngine;

public class Coin : MonoBehaviour
{
    public bool IsCollected { get; private set; }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsCollected && other.GetComponentInParent<PlayerController>() != null)
        {
            IsCollected = true;
            GameAudio.PlayCoin();
            CoinManager manager =
                FindFirstObjectByType<CoinManager>();

            if (manager != null)
            {
                manager.AddCoin();
            }

            Destroy(gameObject);
        }
    }
}
