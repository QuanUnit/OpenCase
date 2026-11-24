using System.Collections.Generic;
using System.Linq;
using Shared.Identifiers;
using UnityEditor;
using UnityEngine;
using EntityId = Shared.Identifiers.EntityId;

namespace Shared.Editor.Identifiers
{
    public abstract class EntityIdPropertyDrawer<TId> : PropertyDrawer where TId : EntityId
    {
        private const string ValuePropertyName = "_value";
        private const string NoneLabel = "<none>";
        private const string MixedLabel = "—";

        private readonly Color _unknownColor = new Color(1f, 0.55f, 0.55f);

        private HashSet<string> _knownValues;
        private Dictionary<string, int> _valueCounts;

        protected abstract IEnumerable<TId> GetIds();

        protected virtual string GetGroup(TId id)
        {
            return string.Empty;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var valueProperty = property.FindPropertyRelative(ValuePropertyName);

            EditorGUI.BeginProperty(position, label, property);

            var fieldRect = EditorGUI.PrefixLabel(position, label);
            var indentLevel = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            if (fieldInfo.IsDefined(typeof(EntityIdDefinitionAttribute), false) == true)
            {
                DrawDefinition(fieldRect, valueProperty);
            }
            else
            {
                DrawReference(fieldRect, property, valueProperty);
            }

            EditorGUI.indentLevel = indentLevel;

            EditorGUI.EndProperty();
        }

        private void DrawDefinition(Rect rect, SerializedProperty valueProperty)
        {
            var previousColor = GUI.color;

            if (valueProperty.hasMultipleDifferentValues == false && IsValidDefinition(valueProperty.stringValue) == false)
            {
                GUI.color = _unknownColor;
            }

            EditorGUI.showMixedValue = valueProperty.hasMultipleDifferentValues;
            EditorGUI.BeginChangeCheck();

            var value = EditorGUI.DelayedTextField(rect, valueProperty.stringValue);

            if (EditorGUI.EndChangeCheck() == true)
            {
                valueProperty.stringValue = value.Trim();
                _valueCounts = null;
            }

            EditorGUI.showMixedValue = false;
            GUI.color = previousColor;
        }

        private bool IsValidDefinition(string value)
        {
            if (string.IsNullOrEmpty(value) == true)
            {
                return false;
            }

            if (_valueCounts == null)
            {
                _valueCounts = GetIds()
                    .Where(id => id != null && id.IsEmpty == false)
                    .GroupBy(id => id.Value)
                    .ToDictionary(group => group.Key, group => group.Count());
            }

            return _valueCounts.TryGetValue(value, out var count) == false || count <= 1;
        }

        private void DrawReference(Rect rect, SerializedProperty property, SerializedProperty valueProperty)
        {
            var previousColor = GUI.color;

            if (valueProperty.hasMultipleDifferentValues == false && IsKnown(valueProperty.stringValue) == false)
            {
                GUI.color = _unknownColor;
            }

            var isClicked = EditorGUI.DropdownButton(rect, new GUIContent(GetDisplayValue(valueProperty)), FocusType.Keyboard);

            GUI.color = previousColor;

            if (isClicked == true)
            {
                ShowDropdown(rect, property);
            }
        }

        private void ShowDropdown(Rect rect, SerializedProperty property)
        {
            var ids = GetIds().Where(id => id != null && id.IsEmpty == false).Distinct().ToList();
            var options = ids.Select(id => new EntityIdOption(id.Value, GetGroup(id))).ToList();
            var serializedObject = property.serializedObject;
            var valuePath = property.propertyPath + "." + ValuePropertyName;

            _knownValues = new HashSet<string>(options.Select(option => option.Value));

            var dropdown = new EntityIdDropdown(typeof(TId).Name, options, value =>
            {
                serializedObject.Update();
                serializedObject.FindProperty(valuePath).stringValue = value;
                serializedObject.ApplyModifiedProperties();
            });

            dropdown.Show(rect);
        }

        private string GetDisplayValue(SerializedProperty valueProperty)
        {
            if (valueProperty.hasMultipleDifferentValues == true)
            {
                return MixedLabel;
            }

            if (string.IsNullOrEmpty(valueProperty.stringValue) == true)
            {
                return NoneLabel;
            }

            return valueProperty.stringValue;
        }

        private bool IsKnown(string value)
        {
            if (string.IsNullOrEmpty(value) == true)
            {
                return false;
            }

            if (_knownValues == null)
            {
                _knownValues = new HashSet<string>(GetIds().Where(id => id != null).Select(id => id.Value));
            }

            return _knownValues.Contains(value);
        }
    }
}
