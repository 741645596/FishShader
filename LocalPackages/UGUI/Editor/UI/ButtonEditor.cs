using UnityEngine.UI;

namespace UnityEditor.UI
{
    [CustomEditor(typeof(Button), true)]
    [CanEditMultipleObjects]
    /// <summary>
    ///   Custom Editor for the Button Component.
    ///   Extend this class to write a custom editor for a component derived from Button.
    /// </summary>
    public class ButtonEditor : SelectableEditor
    {
        SerializedProperty m_OnClickProperty;
        SerializedProperty enableScale;

        protected override void OnEnable()
        {
            base.OnEnable();
            m_OnClickProperty = serializedObject.FindProperty("m_OnClick");
            enableScale = serializedObject.FindProperty("enableScale");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUILayout.Space();

            serializedObject.Update();
            EditorGUILayout.PropertyField(enableScale);
            EditorGUILayout.PropertyField(m_OnClickProperty);
            serializedObject.ApplyModifiedProperties();
        }
    }
}
