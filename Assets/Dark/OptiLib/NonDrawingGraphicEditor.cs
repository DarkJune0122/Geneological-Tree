#if UNITY_EDITOR
using OptiLib.UI;
using UnityEditor;
using UnityEditor.UI;
using UnityEngine;

[CanEditMultipleObjects, CustomEditor(typeof(NonDrawingGraphic), false)]
public class NonDrawingGraphicEditor : GraphicEditor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(m_Script, new GUILayoutOption[0]);
        // skipping AppearanceControlsGUI
        RaycastControlsGUI();
        serializedObject.ApplyModifiedProperties();
    }
}
#endif