using UnityEngine;

[CreateAssetMenu(menuName = "Parameter/Player")]
public class PlayerParameter : ScriptableObject
{
    public int hp = 10;
    public int mp = 20;
    public float speed = 30;
}
