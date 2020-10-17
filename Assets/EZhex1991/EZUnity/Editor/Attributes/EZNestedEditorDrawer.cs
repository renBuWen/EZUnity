/* Author:          ezhex1991@outlook.com
 * CreateTime:      2019-07-18 15:41:57
 * Organization:    #ORGANIZATION#
 * Description:     
 */
using UnityEditor;
using UnityEngine;

namespace EZhex1991.EZUnity
{
    [CustomPropertyDrawer(typeof(EZNestedEditorAttribute))]
    public class EZNestedEditorDrawer : PropertyDrawer
    {
        private Editor nestedEditor;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            if (property.hasMultipleDifferentValues)
            {
                EditorGUI.PropertyField(position, property, label);
            }
            else
            {
                EditorGUI.PropertyField(position, property, label, true);
                if (property.objectReferenceValue != null)
                {
                    property.isExpanded = EditorGUI.Foldout(new Rect(position) { width = 0 }, property.isExpanded, GUIContent.none);
                    if (property.isExpanded)
                    {
                        EditorGUI.indentLevel++;
                        if (property.type == "PPtr<$Material>")
                        {
                            Editor.CreateCachedEditor(property.objectReferenceValue, typeof(MaterialEditor), ref nestedEditor);
                            (nestedEditor as MaterialEditor).PropertiesGUI();
                        }
                        else
                        {
                            Editor.CreateCachedEditor(property.objectReferenceValue, null, ref nestedEditor);
                            nestedEditor.OnInspectorGUI();
                        }
                        EditorGUI.indentLevel--;
                    }
                }
            }
            EditorGUI.EndProperty();
        }
    }

    public abstract class EZNestedPropertyDrawer : PropertyDrawer
    {
        protected bool initialized;
        protected SerializedObject serializedObject;

        protected abstract void GetSerializedProperties();
        protected abstract void OnNestedEditorGUI(Rect position, SerializedProperty property, GUIContent label);

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            if (property.hasMultipleDifferentValues)
            {
                EditorGUI.PropertyField(position, property, label);
            }
            else
            {
                EditorGUI.BeginChangeCheck();
                EditorGUI.PropertyField(position, property, label, true);
                if (EditorGUI.EndChangeCheck() || !initialized)
                {
                    if (property.objectReferenceValue == null)
                    {
                        serializedObject = null;
                    }
                    else
                    {
                        serializedObject = new SerializedObject(property.objectReferenceValue);
                        GetSerializedProperties();
                    }
                    initialized = true;
                }
                if (serializedObject != null)
                {
                    property.isExpanded = EditorGUI.Foldout(new Rect(position) { width = 0 }, property.isExpanded, GUIContent.none, false);
                    if (property.isExpanded)
                    {
                        serializedObject.Update();
                        EditorGUI.indentLevel++;
                        OnNestedEditorGUI(position, property, label);
                        EditorGUI.indentLevel--;
                        serializedObject.ApplyModifiedProperties();
                    }
                }
            }
            EditorGUI.EndProperty();
        }
    }
}