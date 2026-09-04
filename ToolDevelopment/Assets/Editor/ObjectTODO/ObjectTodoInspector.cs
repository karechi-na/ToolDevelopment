using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

/// <summary>
/// TODOをインスペクターに表示するためのクラス
/// </summary>
[InitializeOnLoad]
public class ObjectTodoInspector
{
	// コンストラクタ
	static ObjectTodoInspector()
	{
        // EditorのヘッダーGUIが描画されるときに呼ばれるイベントに登録
        Editor.finishedDefaultHeaderGUI += OnInspectorHeaderGUI;
        // ObjectTodoDatabaseのデータが変更されたときにインスペクターを再描画するためのイベントに登録
        ObjectTodoDatabase.DataChanged += RepaintInspectors;
	}

    /// <summary>
    /// インスペクターのヘッダーGUIが描画されるときに呼ばれるメソッド
    /// </summary>
    private static void OnInspectorHeaderGUI(Editor editor)
	{
        // 対象がGameObjectでない場合は処理を終了
        if (editor.target is not GameObject gameObject) return;

        // GameObjectのGlobalObjectIdを取得
        string objectId =
			GlobalObjectId.GetGlobalObjectIdSlow(gameObject).ToString();

        // ObjectTodoDatabaseから対象のGameObjectのTODOデータを取得
        ObjectTodoData todoData =
			ObjectTodoDatabase.instance.todoDataList.Find(
				data => data.objectId == objectId);

        // TODOデータが存在しない場合は処理を終了
        if (todoData == null) return;
		if (todoData.todos == null) return;
		if (todoData.todos.Count == 0) return;

		EditorGUILayout.Space(8);

        // TODOリストのヘッダーを表示
        EditorGUILayout.LabelField("Object TODO", EditorStyles.boldLabel);

        bool changed = false;

        // TODOリストの各項目を表示
        foreach (TodoItem todo in todoData.todos)
		{
            // TODO項目のテキストと完了状態を表示するトグルを作成
            bool newCompleted =
				EditorGUILayout.ToggleLeft(todo.text, todo.completed);

			if (newCompleted != todo.completed)
			{
				todo.completed = newCompleted;
				changed = true;
			}
		}

        // TODOデータが変更された場合はデータベースを保存し、完了したTODOがあれば削除する
        if (changed)
		{
            // データベースを保存
            ObjectTodoDatabase.instance.SaveDatabase();

            // 完了したTODOがあれば削除する
            EditorApplication.delayCall += () =>
			{
				ObjectTodoDatabase.instance.TryDeleteIfCompleted(todoData);
			};
		}

		EditorGUILayout.Space(4);
	}

    /// <summary>
    /// インスペクターを再描画するメソッド
    /// </summary>
    private static void RepaintInspectors()
	{
        // すべてのインスペクターを再描画
        InternalEditorUtility.RepaintAllViews();
	}
}
