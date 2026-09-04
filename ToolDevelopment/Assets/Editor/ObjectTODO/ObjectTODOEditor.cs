using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;


/// <summary>
/// objectに紐づくTODOリストを管理するエディタウィンドウ
/// </summary>
public class ObjectTODOEditor : EditorWindow
{
    // 現在選択中のGameObjectに紐づくTODOデータ
    private ObjectTodoData currentTodoData;

    // UI要素
    // 選択中のGameObject名を表示するラベル
    private Label selectedObjectLabel;
    // TODOリストを表示するListView
    private ListView todoList;

    // TODO詳細を表示するコンテナ
    private VisualElement detailContainer;
    // TODOが未選択のときに表示するメッセージ
    private Label noSelectionLabel;

    // 登録済みのTODOデータを表示する折りたたみ
    private Foldout registeredFoldout;
    // 登録済みのTODOデータを表示するListView
    private ListView registeredList;

    /// <summary>
    /// メニューからウィンドウを開く
    /// </summary>
    [MenuItem("Tools/Object TODO")]
    public static void ShowWindow()
    {
        GetWindow<ObjectTODOEditor>("Object TODO");
    }

    #region イベント登録と解除
    /// <summary>
    /// イベント登録
    /// </summary>
    private void OnEnable()
    {
        Selection.selectionChanged += OnSelectionChanged;
        ObjectTodoDatabase.DataChanged += OnDatabaseChanged;
    }

    /// <summary>
    /// イベント解除
    /// </summary>
    private void OnDisable()
    {
        Selection.selectionChanged -= OnSelectionChanged;
        ObjectTodoDatabase.DataChanged -= OnDatabaseChanged;
    }
    #endregion

    /// <summary>
    /// GUIの構築
    /// </summary>
    public void CreateGUI()
    {
        #region uxml、cssファイルの読み込みと適用
        // UXMLファイルをVisualTreeAssetとして読み込む
        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
            "Assets/Editor/ObjectTodo/ObjectTODOEditor.uxml");

        // 読み込んだUXMLをもとにUIを生成し、rootVisualElementの子要素として追加
        visualTree.CloneTree(rootVisualElement);

        // スタイルシートを読み込む
        var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(
            "Assets/Editor/ObjectTodo/ObjectTODOEditor.uss");

        // スタイルシートを適用
        rootVisualElement.styleSheets.Add(styleSheet);
        #endregion

        #region UI要素の取得
        selectedObjectLabel =
            rootVisualElement.Q<Label>("selected-object-label");

        detailContainer =
            rootVisualElement.Q<VisualElement>("detail-container");

        noSelectionLabel =
            rootVisualElement.Q<Label>("no-selection-label");

        registeredFoldout =
            rootVisualElement.Q<Foldout>("registered-foldout");

        registeredList =
            rootVisualElement.Q<ListView>("registered-list");

        var todoField =
            rootVisualElement.Q<TextField>("todo-field");

        var addButton =
            rootVisualElement.Q<Button>("add-button");

        todoList =
            rootVisualElement.Q<ListView>("todo-list");
        #endregion

        SetupRegisteredList();

        RefreshSelectedObject();
        RefreshCurrentTodoData(todoList);
        RefreshDisplayMode();

        // ListViewの行を生成するmakeItemとbindItemを設定
        todoList.makeItem = () =>
        {
            // 行のVisualElementを生成
            var row = new VisualElement();
            row.AddToClassList("todo-row");

            // 行の中に表示するトグルと削除ボタンを生成
            var toggle = new Toggle();
            toggle.name = "todo-toggle";
            toggle.AddToClassList("todo-toggle");

            var deleteButton = new Button();
            deleteButton.name = "delete-button";
            deleteButton.text = "Delete";
            
            // トグルの値が変更されたときの処理を登録
            toggle.RegisterValueChangedCallback(evt =>
            {
                if (toggle.userData is not int index) return;
                if (currentTodoData == null) return;
                if (index < 0 || index >= currentTodoData.todos.Count) return;
                
                // TODOの完了状態を更新
                currentTodoData.todos[index].completed = evt.newValue;

                // 完了状態の更新後に、データベースを保存して、完了済みのTODOが全て完了した場合はデータを削除する
                ObjectTodoData targetData = currentTodoData;

                // データベースを保存
                ObjectTodoDatabase.instance.SaveDatabase();

                // 完了済みのTODOが全て完了した場合はデータを削除する
                EditorApplication.delayCall += () =>
                {
                    // 完了済みのTODOが全て完了した場合はデータを削除する
                    ObjectTodoDatabase.instance.TryDeleteIfCompleted(targetData);
                };
            });

            // 削除ボタンがクリックされたときの処理を登録
            deleteButton.clicked += () =>
            {
                if (deleteButton.userData is not int index) return;
                if (currentTodoData == null) return;
                if (index < 0 || index >= currentTodoData.todos.Count) return;
                
                // TODOを削除
                currentTodoData.todos.RemoveAt(index);

                // TODOが全て削除された場合は、ObjectTodoDataも削除する
                if (currentTodoData.todos.Count == 0)
                {
                    // データベースから削除
                    ObjectTodoDatabase.instance.todoDataList.Remove(currentTodoData);

                    currentTodoData = null;
                }
                
                // データベースを保存
                ObjectTodoDatabase.instance.SaveDatabase();
            };

            // 行にトグルと削除ボタンを追加
            row.Add(toggle);
            row.Add(deleteButton);

            return row;
        };

        todoList.bindItem = (element, index) =>
        {
            // 行の中のトグルと削除ボタンにデータをバインド
            var toggle = element.Q<Toggle>("todo-toggle");
            var deleteButton = element.Q<Button>("delete-button");

            // 現在のTODOデータから、index番目のTodoItemを取得
            TodoItem item = currentTodoData.todos[index];

            // トグルにTODOのテキストと完了状態を設定
            toggle.text = item.text;
            toggle.SetValueWithoutNotify(item.completed);

            // この行が現在何番目のデータを表示しているかを保持
            toggle.userData = index;
            deleteButton.userData = index;
        };

        // addButtonがクリックされたときの処理を登録
        addButton.clicked += () =>
        {
            // TODOを追加
            AddTodo(todoField, todoList);
        };
    }

    /// <summary>
    /// 登録済みTODOリストのListViewをセットアップする
    /// </summary>
    private void SetupRegisteredList()
    {
        // ListViewのitemsSourceにデータベースのtodoDataListを設定
        registeredList.itemsSource = ObjectTodoDatabase.instance.todoDataList;

        // ListViewの行を生成するmakeItemとbindItemを設定
        // makeItemは行のVisualElementを生成する処理、bindItemは行にデータをバインドする処理
        registeredList.makeItem = () =>
        {
            // 行のVisualElementを生成
            var row = new VisualElement();
            row.AddToClassList("registered-row");

            // 行の中に表示するラベルやボタンを生成
            // 登録オブジェクト名を表示するラベル
            var nameLabel = new Label();
            nameLabel.name = "object-name";
            nameLabel.AddToClassList("registered-name");
            
            // TODOの進捗を表示するラベル
            var progressLabel = new Label();
            progressLabel.name = "todo-progress";
            progressLabel.AddToClassList("todo-progress");

            // TODOデータを削除するボタン
            var removeButton = new Button();
            removeButton.name = "remove-button";
            removeButton.text = "Remove";
            removeButton.AddToClassList("remove-button");

            // 削除ボタンがクリックされたときの処理
            removeButton.clicked += () =>
            {
                if (removeButton.userData is not ObjectTodoData data) return;

                // 削除確認ダイアログを表示
                bool remove = EditorUtility.DisplayDialog(
                    "Remove TODO",
                    $"「{data.objectName}」のTODOデータを削除しますか？",
                    "削除",
                    "キャンセル"
                );

                // 削除がキャンセルされた場合は処理を中断
                if (!remove) return;
                
                // データベースからTODOデータを削除
                ObjectTodoDatabase.instance.todoDataList.Remove(data);
                // データベースを保存
                ObjectTodoDatabase.instance.SaveDatabase();
            };

            // 行にラベルとボタンを追加
            row.Add(nameLabel);
            row.Add(progressLabel);
            row.Add(removeButton);

            return row;
        };

        registeredList.bindItem = (element, index) =>
        {
            // データベースからTODOデータを取得
            ObjectTodoData data =
                ObjectTodoDatabase.instance.todoDataList[index];

            // 行の中のラベルやボタンにデータをバインド
            var nameLabel = element.Q<Label>("object-name");
            var progressLabel = element.Q<Label>("todo-progress");
            var removeButton = element.Q<Button>("remove-button");
            
            // TODOデータに対応するGameObjectを取得
            GameObject gameObject = GetGameObject(data);

            // GameObjectが存在する場合と存在しない場合で表示を切り替える
            if (gameObject != null)
            {
                // GameObjectが存在する場合は、オブジェクト名とTODOの進捗を表示
                // 名前表示
                nameLabel.text = data.objectName;

                // 進捗表示
                int completedCount = data.todos.Count(todo => todo.completed);
                progressLabel.text = $"{completedCount}/{data.todos.Count}";

                // 削除ボタンは非表示にする
                removeButton.style.display = DisplayStyle.None;
            }
            else
            {
                // GameObjectが存在しない場合は、オブジェクト名をMissing Objectとして表示
                nameLabel.text = $"Missing Object({data.objectName})";

                // 進捗表示は空にする
                progressLabel.text = string.Empty;
                
                // 削除ボタンを表示する
                removeButton.style.display = DisplayStyle.Flex;
                removeButton.userData = data;
            }

        };

        // ListViewの選択変更イベントにコールバックを登録
        registeredList.selectionChanged += OnRegisteredObjectSelected;
    }

    /// <summary>
    /// TODO追加処理
    /// </summary>
    private void AddTodo(TextField todoField, ListView todoList)
    {
        // 選択中のGameObjectを取得
        GameObject selectedObject = Selection.activeGameObject;

        // nullチェック
        if (selectedObject == null)
        {
            Debug.LogWarning("GameObjectを選択してください");
            return;
        }

        // 入力されたTODOテキストを取得
        string text = todoField.value;

        // 空白文字のみのTODOは追加しない
        if (string.IsNullOrWhiteSpace(text)) return;

        // まだTODOデータが無ければ、
        // 最初のTODO追加時に登録する
        if (currentTodoData == null)
        {
            // 新しいObjectTodoDataを作成して登録
            currentTodoData = new ObjectTodoData
            {
                objectId = GetObjectId(selectedObject),
                objectName = selectedObject.name
            };

            // データベースに追加
            ObjectTodoDatabase.instance.todoDataList.Add(currentTodoData);

            // ListViewのitemsSourceを更新
            todoList.itemsSource = currentTodoData.todos;
        }

        // TODOを追加
        currentTodoData.todos.Add(new TodoItem(text));

        // データベースを保存
        ObjectTodoDatabase.instance.SaveDatabase();

        // ListViewを更新
        todoList.RefreshItems();

        // 入力フィールドをクリアしてフォーカスを戻す
        todoField.value = string.Empty;
        todoField.Focus();
    }

    #region 表示更新
    /// <summary>
    /// ウィンドウ表示形式切り替え処理
    /// </summary>
    private void RefreshDisplayMode()
    {
        // 選択中のGameObjectがあるかどうかを判定
        bool hasSelection = Selection.activeGameObject != null;

        // 選択中のGameObjectがある場合は詳細を表示し、ない場合は未選択メッセージを表示する
        detailContainer.style.display =
            hasSelection
                ? DisplayStyle.Flex
                : DisplayStyle.None;

        noSelectionLabel.style.display =
            hasSelection
                ? DisplayStyle.None
                : DisplayStyle.Flex;

        registeredFoldout.value = !hasSelection;
    }

    /// <summary>
    /// 選択中のGameObject名を更新する
    /// </summary>
    private void RefreshSelectedObject()
    {
        // 選択中のGameObjectを取得
        GameObject selectedObject = Selection.activeGameObject;

        // 選択中のGameObject名を更新
        // 選択されていない場合は「GameObjectが選択されていません」と表示
        selectedObjectLabel.text = selectedObject != null
            ? selectedObject.name
            : "GameObjectが選択されていません";
    }

    /// <summary>
    /// 現在のTodoDataを更新する
    /// </summary>
    private void RefreshCurrentTodoData(ListView todoList)
    {
        // 選択中のGameObjectを取得
        GameObject selectedObject = Selection.activeGameObject;

        // 選択オブジェクトのnullチェック
        if (selectedObject == null)
        {
            // 選択されていない場合はcurrentTodoDataをクリア
            currentTodoData = null;
            // ListViewのitemsSourceをクリアして更新
            todoList.itemsSource = null;
            // ListViewを更新
            todoList.RefreshItems();
            return;
        }
        
        // 選択中のGameObjectのIDを取得
        string objectId = GetObjectId(selectedObject);

        // データベースから該当するObjectTodoDataを検索
        currentTodoData =
            ObjectTodoDatabase.instance.todoDataList.Find(
                data => data.objectId == objectId);

        // 該当するObjectTodoDataが見つからない場合は、currentTodoDataをnullに設定
        if (currentTodoData == null)
        {
            todoList.itemsSource = null;
            todoList.RefreshItems();
            return;
        }

        // ListViewのitemsSourceを更新
        todoList.itemsSource = currentTodoData.todos;
        todoList.RefreshItems();
    }
    #endregion

    #region イベントコールバック
    private void OnSelectionChanged()
    {
        if (selectedObjectLabel == null ||
            todoList == null) return;

        RefreshSelectedObject();
        RefreshCurrentTodoData(todoList);
        RefreshDisplayMode();
    }

    private void OnDatabaseChanged()
    {
        if (todoList != null)
        {
            RefreshCurrentTodoData(todoList);
        }

        if (registeredList != null)
        {
            registeredList.itemsSource = ObjectTodoDatabase.instance.todoDataList;
            registeredList.RefreshItems();
        }
    }

    private void OnRegisteredObjectSelected(IEnumerable<object> selectedItems)
    {
        ObjectTodoData data =
            selectedItems.OfType<ObjectTodoData>().FirstOrDefault();

        if (data == null) return;

        GameObject gameObject = GetGameObject(data);

        if (gameObject == null) return;

        Selection.activeGameObject = gameObject;

        EditorGUIUtility.PingObject(gameObject);
    }
    #endregion

    #region GlobalObjectIdとGameObjectとの変換メソッド
    /// <summary>
    /// ObjectのGlobalObjectIdを取得
    /// </summary>
    private string GetObjectId(GameObject gameObject)
    {
        // GameObjectのGlobalObjectIdを取得する
        GlobalObjectId globalId = GlobalObjectId.GetGlobalObjectIdSlow(gameObject);

        // GlobalObjectIdを文字列に変換して返す
        return globalId.ToString();
    }

    /// <summary>
    /// GameObjectを取得する。見つからない場合はnullを返す。
    /// </summary>
    private GameObject GetGameObject(ObjectTodoData data)
    {
        // 引数で渡されたObjectTodoDataのobjectIdからGameObjectのGlobalObjectIdを取得する
        if (!GlobalObjectId.TryParse(data.objectId, out GlobalObjectId globalId)) return null;

        // GlobalObjectIdからObjectを取得する
        Object target =
            GlobalObjectId.GlobalObjectIdentifierToObjectSlow(globalId);

        // 取得したObjectをGameObjectにキャストする
        // もしキャストできなければnullを返す
        return target as GameObject;
    }
    #endregion
}
