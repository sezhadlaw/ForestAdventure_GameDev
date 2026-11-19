using UnityEngine;

[CreateAssetMenu(menuName = "Forest Adventure/Balance/EnemyPatrol")]
public class EnemyPatrolStats : ScriptableObject
{
    public float moveSpeed;
    public float patrolDistance;
    public int damage;
}
