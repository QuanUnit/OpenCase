using System.Reflection;
using NaughtyAttributes.Editor;
using UnityEditor;
using UnityEngine;

namespace AdvancedLayoutGroups.Tools.Properties.Editor
{
    [CustomPropertyDrawer(typeof(NotifiedProperty<>))]
    public class NotifiedPropertyDrawer : PropertyDrawer
    {
        public sealed override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
        {
            SerializedProperty internalValueProperty = property.FindPropertyRelative("_value");

            EditorGUI.BeginProperty(rect, label, internalValueProperty);
            EditorGUI.PropertyField(rect, internalValueProperty, label, true);
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            SerializedProperty internalValueProperty = property.FindPropertyRelative("_value");
            return EditorGUI.GetPropertyHeight(internalValueProperty, includeChildren: true);
        }
    }
}