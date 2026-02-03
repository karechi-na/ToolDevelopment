using UnityEngine;

public class SoCheckClass : MonoBehaviour
{
    [SerializeField] private SampleMasterData masterData;

    void Start()
    {
        int id = masterData.GetInt("HP");
        Debug.Log(id);

        float attack = masterData.GetFloat("Attack");
        Debug.Log (attack);
    }

}
