using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class ObjectTODOEditor : EditorWindow
{

	private ObjectTodoData currentTodoData;

	private Label selectedObjectLabel;
	private ListView todoList;

	[MenuItem("Tools/Object TODO")]
	public static void ShowWindow()
	{
		GetWindow<ObjectTODOEditor>("Object TODO");
	}

	private void OnEnable()
	{
		Selection.selectionChanged += OnSelectionChanged;
		ObjectTodoDatabase.DataChanged += OnDatabaseChanged;
	}

	private void OnDisable()
	{
		Selection.selectionChanged -= OnSelectionChanged;
		ObjectTodoDatabase.DataChanged -= OnDatabaseChanged;
	}

	public void CreateGUI()
	{
		var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
			"Assets/Editor/ObjectTodo/ObjectTODOEditor.uxml");

		visualTree.CloneTree(rootVisualElement);

		var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(
			"Assets/Editor/ObjectTodo/ObjectTODOEditor.uss");

		rootVisualElement.styleSheets.Add(styleSheet);

		selectedObjectLabel =
			rootVisualElement.Q<Label>("selected-object-label");

		var todoField =
			rootVisualElement.Q<TextField>("todo-field");

		var addButton =
			rootVisualElement.Q<Button>("add-button");

		todoList =
			rootVisualElement.Q<ListView>("todo-list");

		RefreshSelectedObject();
		RefreshCurrentTodoData(todoList);

		todoList.makeItem = () =>
		{
			var row = new VisualElement();
			row.AddToClassList("todo-row");

			var toggle = new Toggle();
			toggle.name = "todo-toggle";
			toggle.AddToClassList("todo-toggle");

			var deleteButton = new Button();
			deleteButton.name = "delete-button";
			deleteButton.text = "Delete";

			toggle.RegisterValueChangedCallback(evt =>
			{
				if (toggle.userData is not int index) return;
				if (currentTodoData == null) return;
				if (index < 0 || index >= currentTodoData.todos.Count) return;

				currentTodoData.todos[index].completed = evt.newValue;

				ObjectTodoData targetData = currentTodoData;

				ObjectTodoDatabase.instance.SaveDatabase();

				EditorApplication.delayCall += () =>
				{
					ObjectTodoDatabase.instance.TryDeleteIfCompleted(targetData);
				};
			});

			deleteButton.clicked += () =>
			{
				if (deleteButton.userData is not int index) return;
				if (currentTodoData == null) return;
				if (index < 0 || index >= currentTodoData.todos.Count) return;

				currentTodoData.todos.RemoveAt(index);

				ObjectTodoDatabase.instance.SaveDatabase();

				todoList.RefreshItems();
			};

			row.Add(toggle);
			row.Add(deleteButton);

			return row;
		};

		todoList.bindItem = (element, index) =>
		{
			var toggle = element.Q<Toggle>("todo-toggle");
			var deleteButton = element.Q<Button>("delete-button");

			TodoItem item = currentTodoData.todos[index];

			toggle.text = item.text;
			toggle.SetValueWithoutNotify(item.completed);

			// この行が現在何番目のデータを表示しているかを保持
			toggle.userData = index;
			deleteButton.userData = index;
		};

		addButton.clicked += () =>
		{
			AddTodo(todoField, todoList);
		};
	}

	private void RefreshSelectedObject()
	{
		GameObject selectedObject = Selection.activeGameObject;

		selectedObjectLabel.text = selectedObject != null
			? selectedObject.name
			: "GameObjectが選択されていません";
	}

	private void AddTodo(TextField todoField, ListView todoList)
	{
		GameObject selectedObject = Selection.activeGameObject;

		if (selectedObject == null)
		{
			Debug.LogWarning("GameObjectを選択してください");
			return;
		}

		string text = todoField.value;

		if (string.IsNullOrWhiteSpace(text)) return;

		// まだTODOデータが無ければ、
		// 最初のTODO追加時に登録する
		if (currentTodoData == null)
		{
			currentTodoData = new ObjectTodoData
			{
				objectId = GetObjectId(selectedObject),
				objectName = selectedObject.name
			};

			ObjectTodoDatabase.instance.todoDataList.Add(currentTodoData);

			todoList.itemsSource = currentTodoData.todos;
		}

		currentTodoData.todos.Add(new TodoItem(text));

		ObjectTodoDatabase.instance.SaveDatabase();

		todoList.RefreshItems();

		todoField.value = string.Empty;
		todoField.Focus();
	}

	private void OnSelectionChanged()
	{
		if (selectedObjectLabel == null ||
			todoList == null) return;

		RefreshSelectedObject();
		RefreshCurrentTodoData(todoList);
	}

	private void OnDatabaseChanged()
	{
		if (todoList == null) return;

		RefreshCurrentTodoData(todoList);
	}

	private string GetObjectId(GameObject gameObject)
	{
		GlobalObjectId globalId = GlobalObjectId.GetGlobalObjectIdSlow(gameObject);

		return globalId.ToString();
	}

	private void RefreshCurrentTodoData(ListView todoList)
	{
		GameObject selectedObject = Selection.activeGameObject;

		if (selectedObject == null)
		{
			currentTodoData = null;
			todoList.itemsSource = null;
			todoList.RefreshItems();
			return;
		}

		string objectId = GetObjectId(selectedObject);

		currentTodoData =
			ObjectTodoDatabase.instance.todoDataList.Find(
				data => data.objectId == objectId);

		if (currentTodoData == null)
		{
			todoList.itemsSource = null;
			todoList.RefreshItems();
			return;
		}

		todoList.itemsSource = currentTodoData.todos;
		todoList.RefreshItems();
	}
}

