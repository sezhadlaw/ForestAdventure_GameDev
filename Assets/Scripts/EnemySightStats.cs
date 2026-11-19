using UnityEngine;

[CreateAssetMenu(menuName = "Forest Adventure/Balance/EnemySight")]
public class EnemySightStats : ScriptableObject
{
    public float viewRange;
    public float viewAngle;
    public float checkInterval;
}
