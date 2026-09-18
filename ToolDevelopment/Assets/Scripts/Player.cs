using UnityEngine;

public class Player : MonoBehaviour
{
    [ParameterBind]
    [SerializeField] private int HP;

    [ParameterBind]
    [SerializeField] private int mp;

    [ParameterBind]
    [SerializeField] private float speed;

    private void Start()
    {
        Debug.Log($"HP : {HP}");
        Debug.Log($"HP : {mp}");
    }
}
