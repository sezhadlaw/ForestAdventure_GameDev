using UnityEngine;

[CreateAssetMenu(menuName = "Forest Adventure/Balance/ForestShooter")]
public class ForestShooterStats : ScriptableObject
{
    public float moveSpeed;
    public float preferredDistance;
    public float moveTime;
    public float aimTime;
    public float cooldown;
    public float shotSpeed;
    public float shotLife;
    public float shotScale;
    public int damage;
}
