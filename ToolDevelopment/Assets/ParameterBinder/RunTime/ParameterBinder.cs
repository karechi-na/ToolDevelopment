using System.Reflection;
using System.Collections.Generic;
using UnityEngine;

public class ParameterBinder : MonoBehaviour
{
    [SerializeField] private ScriptableObject source;

    [SerializeField] private MonoBehaviour target;

    [SerializeField] private List<ParameterBinding> bindings = new();

    private void Awake()
    {
        Bind();
    }

    private void Bind()
    {
        if (source == null || target == null) return;

        BindToComponent(target);
    }

    private void BindToComponent(MonoBehaviour target)
    {
        System.Type sourceType = source.GetType();
        System.Type targetType = target.GetType();

        BindingFlags flags =
            BindingFlags.Instance |
            BindingFlags.Public |
            BindingFlags.NonPublic;

        foreach (ParameterBinding binding in bindings)
        {
            FieldInfo sourceField = sourceType.GetField(
                binding.sourceFieldName,
                flags
            );

            FieldInfo targetField = targetType.GetField(
                binding.targetFieldName,
                flags
            );

            // Field‚ª‘¶İ‚µ‚È‚¢
            if (sourceField == null || targetField == null) continue;

            // ParameterBind‚ª‚Â‚¢‚Ä‚¢‚È‚¢Field‚É‚Í‘‚«‚Ü‚¹‚È‚¢
            if (!System.Attribute.IsDefined(targetField, typeof(ParameterBindAttribute))) continue;

            // Œ^‚ªˆá‚¤ê‡‚à‘‚«‚Ü‚È‚¢
            if(sourceField.FieldType != targetField.FieldType) continue;

            object value = sourceField.GetValue(source);

            targetField.SetValue(target, value);
        }
    }
}