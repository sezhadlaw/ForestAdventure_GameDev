
using UnityEngine;

public class PlatformPassenger : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.GetComponent<PlayerController>() == null)
            return;

        // Проверяем, что Player стоит сверху платформы
        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (contact.normal.y < -0.5f)
            {
                collision.transform.SetParent(transform, true);
                break;
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.GetComponent<PlayerController>() != null &&
            collision.transform.parent == transform)
        {
            collision.transform.SetParent(null, true);
        }
    }
}
