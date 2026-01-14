#if UNITY_EDITOR


using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PlayerBase), true)]
public class PlayerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        PlayerBase player = (PlayerBase)target;

        player.hp = EditorGUILayout.Slider("HP", player.hp, 0, 100);
        player.speed = EditorGUILayout.FloatField("Speed", player.speed);

        if (GUILayout.Button("ƒŠƒZƒbƒg"))
        {
            player.hp = 100;
            player.speed = 5.0f;
        }

        EditorUtility.SetDirty(player);
    }
}
#endif