using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[InitializeOnLoad]
public class ObjectTodoInspector
{
	static ObjectTodoInspector()
	{
		Editor.finishedDefaultHeaderGUI += OnInspectorHeaderGUI;
		ObjectTodoDatabase.DataChanged += RepaintInspectors;
	}

	private static void OnInspectorHeaderGUI(Editor editor)
	{
		if (editor.target is not GameObject gameObject) return;

		string objectId =
			GlobalObjectId.GetGlobalObjectIdSlow(gameObject).ToString();

		ObjectTodoData todoData =
			ObjectTodoDatabase.instance.todoDataList.Find(
				data => data.objectId == objectId);

		if (todoData == null) return;
		if (todoData.todos == null) return;
		if (todoData.todos.Count == 0) return;

		EditorGUILayout.Space(8);

		EditorGUILayout.LabelField("Object TODO", EditorStyles.boldLabel);

		bool changed = false;

		foreach (TodoItem todo in todoData.todos)
		{
			bool newCompleted =
				EditorGUILayout.ToggleLeft(todo.text, todo.completed);

			if (newCompleted != todo.completed)
			{
				todo.completed = newCompleted;
				changed = true;
			}
		}

		if (changed)
		{
			ObjectTodoDatabase.instance.SaveDatabase();

			EditorApplication.delayCall += () =>
			{
				ObjectTodoDatabase.instance.TryDeleteIfCompleted(todoData);
			};
		}

		EditorGUILayout.Space(4);
	}

	private static void RepaintInspectors()
	{
		//foreach (EditorWindow window in Resources.FindObjectsOfTypeAll<EditorWindow>())
		//{
		//	if (window.GetType().Name == "InspectorWindow")
		//	{
		//		window.Repaint();
		//	}
		//}

		InternalEditorUtility.RepaintAllViews();
	}
}
