using UnityEngine;
using System.Collections.Generic;

public class CheckVariableDisplay : MonoBehaviour
{
    [Header("Check Variable Display")]

    [Header("int ---------------------------------------------------------")]
    [ShowIf,IntButton(0, 100)]
    public int intValue = 0;
    [IntButton(0, 100)]
    public int[] intArray = new int[] { };
    public List<int> intList = new List<int>();
    [Header("float -------------------------------------------------------")]
    [ShowIf, Range(0, 100)]public float floatValue = 0.0f;
    public float[] floatArray = new float[] { };
    public List<float> floatList = new List<float>();
    [Header("bool --------------------------------------------------------")]
    [ShowIf]public bool flag = false;
    public bool[] boolArray = new bool[] { };
    public List<bool> boolList = new List<bool>();
    [Header("string ------------------------------------------------------")]
    [ShowIf] public string message = "";
    public string[] stringArray = new string[] { };
    public List<string> stringList = new List<string>();
    [Header("Color -------------------------------------------------------")]
    [ShowIf]public Color color = Color.white;
    public Color[] colorArray = new Color[] { };
    public List<Color> colorList = new List<Color>();
    [Header("Vector2 -----------------------------------------------------")]
    [ShowIf] public Vector2 vector2D = Vector2.zero;
    public Vector2[] vector2DArray = new Vector2[] { };
    public List<Vector2> vector2DList = new List<Vector2>();
    [Header("Vector3 -----------------------------------------------------")]
    [ShowIf] public Vector3 position = Vector3.zero;
    public Vector3[] positionArray = new Vector3[] { };
    public List<Vector3> positionList = new List<Vector3>();
    [Header("GameObject --------------------------------------------------")]
    [ShowIf] public GameObject targetObject = null;
    public GameObject[] targetObjectArray = new GameObject[] { };
    public List<GameObject> targetObjectList = new List<GameObject>();
    [Header("Complex Types -----------------------------------------------")]
    [ShowIf] public List<int>[] intListArray = new List<int>[] { };
    [ShowIf]public Dictionary<string, int> stringIntDict = new Dictionary<string, int>();
}
