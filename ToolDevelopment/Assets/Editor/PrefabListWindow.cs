#if UNITY_EDITOR

using UnityEditor; // Unityエディタ専用のAPI（EditorWindow, MenuItem など）
using UnityEngine;

/// <summary>
/// プレハブを選択するためのエディタウィンドウ
/// </summary>
public class PrefabPickerWindow : EditorWindow
{
    // Unity上部メニューに「Tools/Prefab Picker」を追加する
    [MenuItem("Tools/Prefab Picker")]
    static void Open()
    {
        // このウィンドウを開く（すでに開いていれば前面に出す）
        GetWindow<PrefabPickerWindow>("Prefab Picker");
    }

    // エディタウィンドウが再描画されるたびに呼ばれる
    // ※ フレームごと、フォーカス切り替え時など何度も呼ばれる
    private void OnGUI()
    {
        // ボタンを表示し、押されたら true が返る
        if (GUILayout.Button("プレハブを検索"))
        {
            ///////////////////////////////////////////////////////////////////////
            // Unity標準の「Object Picker（オブジェクト選択ウィンドウ）」を開く
            //
            // 第1引数 : 初期選択オブジェクト（null = 何も選ばない）
            // 第2引数 : シーンオブジェクトを含めるか（false = Assetsのみ）
            // 第3引数 : 検索フィルタ（"" = 何も入れない）
            // 第4引数 : コントロールID（0でOK）
            ///////////////////////////////////////////////////////////////////////
            EditorGUIUtility.ShowObjectPicker<GameObject>(
                null,
                false,
                "",
                0
            );
        }

        //Object Picker で選択が変更されたときに飛んでくるイベント
        //（ユーザーが一覧からクリックした瞬間）
        if (Event.current.commandName == "ObjectSelectorUpdated")
        {
            // Pickerで現在選ばれているオブジェクトを取得
            var obj = EditorGUIUtility.GetObjectPickerObject() as GameObject;

            //obj が null ではない
            //かつ
            //選ばれた GameObject が「Prefabアセット」であるかをチェック
            if (obj != null &&
                PrefabUtility.GetPrefabAssetType(obj) != PrefabAssetType.NotAPrefab)
            {
                // ProjectビューでそのPrefabを選択状態にする
                Selection.activeObject = obj;

                // Projectビューでピン（ハイライト）表示する
                EditorGUIUtility.PingObject(obj);
            }
        }
    }
}
#endif