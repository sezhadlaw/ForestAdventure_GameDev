using UnityEngine;

[CreateAssetMenu(menuName = "Forest Adventure/Balance/ForestCharger")]
public class ForestChargerStats : ScriptableObject
{
    public float triggerHeight;
    public float dashLength;
    public float dashSpeed;
    public float hitRange;
    public float cooldown;
    public float windUp;
    public int damage;
}
