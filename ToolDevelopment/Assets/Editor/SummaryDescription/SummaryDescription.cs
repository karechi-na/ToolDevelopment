using UnityEditor;
using UnityEngine;
using System.IO;
using System.Text.RegularExpressions;

[CustomEditor(typeof(MonoBehaviour), true)]
public class SummaryDescriptionEditor : Editor
{
    public override void OnInspectorGUI()
    {
        Debug.Log($"OnInspectorGUI called for: {target.GetType().Name}");
        var summary = TryGetSummary((MonoBehaviour)target);
        Debug.Log($"summary target: {target.GetType().Name}");
        Debug.Log($"summary: {summary}");

        if (!string.IsNullOrWhiteSpace(summary))
        {
            var content = new GUIContent("ê‡ñæ", summary);
            EditorGUILayout.LabelField(content, EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(summary, MessageType.Info);
        }

        DrawDefaultInspector();
    }

    private static string TryGetSummary(MonoBehaviour behaviour)
    {
        var script = MonoScript.FromMonoBehaviour(behaviour);
        if (script == null) return null;

        var path = AssetDatabase.GetAssetPath(script);
        if (string.IsNullOrEmpty(path)) return null;

        var source = File.ReadAllText(path);

        var className = behaviour.GetType().Name;

        var pattern =
            @"///\s*<summary>\s*\r\n" +
            @"(?<summary>(?:\s*///.*\r?\n)+)" +
            @"\s*///\s*</summary>\s*\r?\n" +
            @"(?:\s*\[[\s\S]*?\]\s*)*" +
            @"\s*public\s+class\s" +
            Regex.Escape(className) +
            @"\b";

        var match = Regex.Match(source, pattern);

        if (!match.Success) return null;

        var summary = match.Groups["summary"].Value;

        summary = Regex.Replace(summary, @"^\s*///\s?", "", RegexOptions.Multiline);
        summary = Regex.Replace(summary, @"\r?\n", "\n");
        summary = summary.Trim();

        return summary;
    }
}