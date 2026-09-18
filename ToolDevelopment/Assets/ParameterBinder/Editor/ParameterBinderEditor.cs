using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

[CustomEditor(typeof(ParameterBinder))]
public class ParameterBinderEditor : Editor
{
    private VisualElement bindingContainer;

    private Label bindingsSummary;

    public override VisualElement CreateInspectorGUI()
    {
        var visualTree = FindAsset<VisualTreeAsset>("ParameterBinderEditor");

        var styleSheet = FindAsset<StyleSheet>("ParameterBinderEditor");

        if (visualTree == null)
            return new HelpBox(
                "ParameterBinderEditor.uxml が見つかりません。",
                HelpBoxMessageType.Error
            );

        var root = visualTree.CloneTree();

        if (styleSheet != null)
            root.styleSheets.Add(styleSheet);

        // Source
        var sourceField = root.Q<PropertyField>("source-field");
        var targetContainer = root.Q<VisualElement>("target-container");
        bindingContainer = root.Q<VisualElement>("binding-container");
        bindingsSummary = root.Q<Label>("bindings-summary");

        // Target
        SetupTargetDropdown(targetContainer);

        // 初回表示
        RefreshBindingList();

        // Sourceが変更されたらBindingsを更新
        sourceField.RegisterValueChangeCallback(evt =>
        {
            serializedObject.ApplyModifiedProperties();
            RefreshBindingList();
        });

        return root;
    }

    private T FindAsset<T>(string assetName) where T : UnityEngine.Object
    {
        string[] guids = AssetDatabase.FindAssets($"{assetName} t:{typeof(T).Name}");

        if (guids.Length == 0)
        {
            Debug.LogError($"{assetName} ({typeof(T).Name})が見つかりません。");

            return null;
        }

        string path = AssetDatabase.GUIDToAssetPath(guids[0]);

        return AssetDatabase.LoadAssetAtPath<T>(path);
    }

    private void SetupTargetDropdown(VisualElement root)
    {
        var binder = (ParameterBinder)target;
        var targetProperty = serializedObject.FindProperty("target");

        MonoBehaviour[] components =
            binder.GetComponents<MonoBehaviour>();

        var componentNames = new List<string>();
        var componentList = new List<MonoBehaviour>();

        componentNames.Add("None (MonoBehaviour)");
        componentList.Add(null);

        foreach (MonoBehaviour component in components)
        {
            // ParameterBinder自身は候補から除外
            if (component == binder) continue;

            componentNames.Add(component.GetType().Name);
            componentList.Add(component);
        }

        int currentIndex = 0;

        var currentTarget = targetProperty.objectReferenceValue as MonoBehaviour;

        if (currentTarget != null)
        {
            int foundIndex = componentList.IndexOf(currentTarget);

            if (foundIndex >= 0)
                currentIndex = foundIndex;
        }

        string currentValue = componentNames[currentIndex];

        var targetDropdown = new DropdownField(
            "Target Component",
            componentNames,
            currentValue
        );

        targetDropdown.RegisterValueChangedCallback(evt =>
        {
            int selectedIndex = componentNames.IndexOf(evt.newValue);

            if (selectedIndex < 0 || selectedIndex >= componentList.Count)
                return;

            serializedObject.Update();

            if (selectedIndex == 0)
                targetProperty.objectReferenceValue = null;
            else
                targetProperty.objectReferenceValue = componentList[selectedIndex];

            serializedObject.ApplyModifiedProperties();

            RefreshBindingList();
        });

        root.Add(targetDropdown);
    }

    private void RefreshBindingList()
    {
        bindingContainer.Clear();

        serializedObject.Update();

        var sourceProperty = serializedObject.FindProperty("source");
        var targetProperty = serializedObject.FindProperty("target");
        var bindingsProperty = serializedObject.FindProperty("bindings");

        var source = sourceProperty.objectReferenceValue as ScriptableObject;
        var targetComponent = targetProperty.objectReferenceValue as MonoBehaviour;

        if (source == null || targetComponent == null)
        {
            bindingsSummary.text = "0 / 0";

            var helpBox = new HelpBox(
                "SourceとTarget Componentを設定してください。",
                HelpBoxMessageType.Info
            );

            bindingContainer.Add(helpBox);
            return;
        }

        FieldInfo[] sourceFields = GetSourceFields(source);
        FieldInfo[] targetFields = GetTargetFields(targetComponent);

        int validCount = 0;

        for (int i = 0; i < bindingsProperty.arraySize; i++)
        {
            SerializedProperty bindingProperty = bindingsProperty.GetArrayElementAtIndex(i);

            SerializedProperty sourceNameProperty = bindingProperty.FindPropertyRelative("sourceFieldName");
            SerializedProperty targetNameProperty = bindingProperty.FindPropertyRelative("targetFieldName");

            if (IsBindingValid(sourceFields, targetFields, sourceNameProperty.stringValue, targetNameProperty.stringValue))
                validCount++;

            bindingContainer.Add(CreateBindingRow(sourceFields, targetFields, i));
        }

        bindingsSummary.text = $"{validCount} / {bindingsProperty.arraySize} Valid";

        CreateAddBindingButton(bindingsProperty);
    }

    private VisualElement CreateBindingRow(
        FieldInfo[] sourceFields, FieldInfo[] targetFields, int bindingIndex)
    {
        var row = new VisualElement();
        row.AddToClassList("binding-row");

        SerializedProperty bindingsProperty = serializedObject.FindProperty("bindings");
        SerializedProperty bindingProperty = bindingsProperty.GetArrayElementAtIndex(bindingIndex);

        SerializedProperty sourceNameProperty = bindingProperty.FindPropertyRelative("sourceFieldName");
        SerializedProperty targetNameProperty = bindingProperty.FindPropertyRelative("targetFieldName");

        var sourceNames = new List<string>
        {
            "None"
        };

        foreach (FieldInfo flied in sourceFields)
        {
            sourceNames.Add(flied.Name);
        }

        var targetNames = new List<string>
        {
            "None"
        };

        foreach (FieldInfo filed in targetFields)
        {
            targetNames.Add(filed.Name);
        }

        string currentSource = string.IsNullOrEmpty(sourceNameProperty.stringValue)
            ? "None"
            : sourceNameProperty.stringValue;
        string currentTarget = string.IsNullOrEmpty(targetNameProperty.stringValue)
            ? "None"
            : targetNameProperty.stringValue;

        var sourceDropdown = new DropdownField(sourceNames, currentSource);
        sourceDropdown.AddToClassList("binding-source");

        var arrowLabel = new Label("→");
        arrowLabel.AddToClassList("binding-arrow");

        var targetDropdown = new DropdownField(targetNames, currentTarget);
        targetDropdown.AddToClassList("binding-target");

        var statusLabel = new Label();
        statusLabel.AddToClassList("binding-status");

        sourceDropdown.RegisterValueChangedCallback(evt =>
        {
            serializedObject.Update();

            SerializedProperty currentBindings = serializedObject.FindProperty("bindings");

            if (bindingIndex < 0 || bindingIndex >= currentBindings.arraySize) return;

            SerializedProperty currentBinding = currentBindings.GetArrayElementAtIndex(bindingIndex);

            SerializedProperty currentSource = currentBinding.FindPropertyRelative("sourceFieldName");

            SerializedProperty currentTarget = currentBinding.FindPropertyRelative("targetFieldName");

            currentSource.stringValue = evt.newValue == "None" ? "" : evt.newValue;

            serializedObject.ApplyModifiedProperties();

            UpdateBindingStatus(statusLabel, sourceFields, targetFields, currentSource.stringValue, currentTarget.stringValue);
            RefreshBindingSummary(sourceFields, targetFields);
        });

        targetDropdown.RegisterValueChangedCallback(evt =>
        {
            serializedObject.Update();

            SerializedProperty currentBindings = serializedObject.FindProperty("bindings");

            if (bindingIndex < 0 || bindingIndex >= currentBindings.arraySize) return;

            SerializedProperty currentBinding = currentBindings.GetArrayElementAtIndex(bindingIndex);

            SerializedProperty currentSource = currentBinding.FindPropertyRelative("sourceFieldName");

            SerializedProperty currentTarget = currentBinding.FindPropertyRelative("targetFieldName");

            currentTarget.stringValue = evt.newValue == "None" ? "" : evt.newValue;

            serializedObject.ApplyModifiedProperties();

            UpdateBindingStatus(statusLabel, sourceFields, targetFields, currentSource.stringValue, currentTarget.stringValue);
            RefreshBindingSummary(sourceFields, targetFields);
        });

        var removeButton = new Button(() =>
        {
            serializedObject.Update();

            SerializedProperty currentBindingsProperty = serializedObject.FindProperty("bindings");

            if (currentBindingsProperty == null || !currentBindingsProperty.isArray) return;

            if (bindingIndex < 0 || bindingIndex >= currentBindingsProperty.arraySize) return;

            currentBindingsProperty.DeleteArrayElementAtIndex(bindingIndex);

            serializedObject.ApplyModifiedProperties();

            bindingContainer.schedule.Execute(() =>
            {
                RefreshBindingList();
            });
        });

        removeButton.text = " X ";
        removeButton.AddToClassList("binding-remove");

        row.Add(sourceDropdown);
        row.Add(arrowLabel);
        row.Add(targetDropdown);
        row.Add(statusLabel);
        row.Add(removeButton);

        return row;
    }

    private void CreateAddBindingButton(SerializedProperty bindingsProperty)
    {
        var addButton = new Button(() =>
        {
            serializedObject.Update();

            int newIndex = bindingsProperty.arraySize;

            bindingsProperty.InsertArrayElementAtIndex(newIndex);

            SerializedProperty newBinding = bindingsProperty.GetArrayElementAtIndex(newIndex);

            newBinding.FindPropertyRelative("sourceFieldName").stringValue = "";
            newBinding.FindPropertyRelative("targetFieldName").stringValue = "";

            serializedObject.ApplyModifiedProperties();

            RefreshBindingList();
        });

        addButton.text = "Add Binding";
        addButton.AddToClassList("add-binding-button");

        bindingContainer.Add(addButton);
    }

    private FieldInfo FindField(FieldInfo[] fields, string fieldName)
    {
        if (string.IsNullOrEmpty(fieldName)) return null;

        foreach (FieldInfo field in fields)
        {
            if (field.Name == fieldName) return field;
        }

        return null;
    }

    private void UpdateBindingStatus(Label statusLabel, FieldInfo[] sourceFields, FieldInfo[] targetFields, string sourceName, string targetName)
    {
        FieldInfo sourceField = FindField(sourceFields, sourceName);
        FieldInfo targetField = FindField(targetFields, targetName);

        if (sourceField == null || targetField == null)
        {
            statusLabel.text = "-";
            return;
        }

        if (sourceField.FieldType == targetField.FieldType)
        {
            statusLabel.text = "[OK]";
            return;
        }

        statusLabel.text = $"[ ! ] {GetTypeName(sourceField.FieldType)} → " + $"{GetTypeName(targetField.FieldType)}";
    }

    private bool IsBindingValid(FieldInfo[] sourceFields, FieldInfo[] targetFields, string sourceName, string targetName)
    {
        FieldInfo sourceField = FindField(sourceFields, sourceName);
        FieldInfo targetField = FindField(targetFields, targetName);

        if (sourceField == null || targetField == null) return false;

        return sourceField.FieldType == targetField.FieldType;
    }

    private void RefreshBindingSummary(FieldInfo[] sourceFields, FieldInfo[] targetFields)
    {
        serializedObject.Update();

        SerializedProperty bindingsProperty = serializedObject.FindProperty("bindings");

        int validCount = 0;

        for (int i = 0; i < bindingsProperty.arraySize; i++)
        {
            SerializedProperty bindingProperty = bindingsProperty.GetArrayElementAtIndex(i);

            string sourceName = bindingProperty.FindPropertyRelative("sourceFieldName").stringValue;
            string targetName = bindingProperty.FindPropertyRelative("targetFieldName").stringValue;

            if (IsBindingValid(sourceFields, targetFields, sourceName, targetName))
            {
                validCount++;
            }
        }

        bindingsSummary.text = $"{validCount} / {bindingsProperty.arraySize} Valid";
    }

    private FieldInfo[] GetSourceFields(ScriptableObject source)
    {
        return source.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
    }

    private FieldInfo[] GetTargetFields(MonoBehaviour target)
    {
        FieldInfo[] fields = target.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        var result = new List<FieldInfo>();

        foreach (FieldInfo field in fields)
        {
            if (Attribute.IsDefined(field, typeof(ParameterBindAttribute))) result.Add(field);
        }

        return result.ToArray();
    }

    private string GetTypeName(Type type)
    {
        if (type == typeof(int)) return "int";
        if (type == typeof(float)) return "float";
        if (type == typeof(bool)) return "bool";
        if (type == typeof(string)) return "string";
        if (type == typeof(double)) return "double";
        if (type == typeof(long)) return "long";

        return type.Name;
    }

}
