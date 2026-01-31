using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(UFTL_FolderStructureConfig))]
public class UFTL_FolderStructureConfigEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        UFTL_FolderStructureConfig config = (UFTL_FolderStructureConfig)target;

        EditorGUILayout.Space();

        if (GUILayout.Button("Create Folders in selected path"))
        {
            config.CreateStructure();
        }
    }
}

