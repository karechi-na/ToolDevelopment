using UnityEngine;

[CreateAssetMenu(menuName = "Generated/EnemyData")]
public class EnemyData : ScriptableObject
{
   public string id;
   public string displayName;
   public int hp;
   public int attack;
   public bool isBoss;
}
