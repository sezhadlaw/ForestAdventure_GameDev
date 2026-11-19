using UnityEngine;

[CreateAssetMenu(menuName = "Forest Adventure/Balance/ForestGuard")]
public class ForestGuardStats : ScriptableObject
{
    public float patrolSpeed;
    public float chaseSpeed;
    public float hitRange;
    public float loseTime;
    public float windUp;
    public float cooldown;
    public int damage;
}
