using System;
using System.Collections.Generic;
using UnityEditor;

[FilePath("ProjectSettings/ObjectTodoDatabase.asset", FilePathAttribute.Location.ProjectFolder)]
public class ObjectTodoDatabase : ScriptableSingleton<ObjectTodoDatabase>
{
    public List<ObjectTodoData> todoDataList = new();

    public static event Action DataChanged;

    public void SaveDatabase()
    {
        Save(true);

        DataChanged?.Invoke();
    }

	public bool TryDeleteIfCompleted(ObjectTodoData todoData)
	{
		if (todoData == null ||
			todoData.todos == null ||
			todoData.todos.Count == 0)
		{
			return false;
		}

		bool allCompleted =
			todoData.todos.TrueForAll(todo => todo.completed);

		if (!allCompleted)
		{
			return false;
		}

		bool delete =
			EditorUtility.DisplayDialog(
				"Object TODO",
				"すべてのTODOが完了しました。\nTODOリストを削除しますか？",
				"削除",
				"残す");

		if (!delete)
		{
			return false;
		}

		todoDataList.Remove(todoData);

		SaveDatabase();

		return true;
	}
}
