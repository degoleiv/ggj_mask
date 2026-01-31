using UnityEngine;
using UnityEditor;
using System;

public class UFTL_MenuItem_TransformConfigurationTools : EditorWindow
{
    #region VARIABLES
    string wantedEmptyName = "Empty Name";
    string wantedGroupName = "Group Name";
    GUIContent content = new GUIContent();
    #endregion

    #region TRANSFORM TOOLS WINDOW
    [MenuItem("Ultimate Forge Tools Lite/Tools/Transform Tools", false, 7)]
    public static void ShowWindow()
    {
        UFTL_MenuItem_TransformConfigurationTools window = GetWindow<UFTL_MenuItem_TransformConfigurationTools>(true, "Transform Tools");
        window.minSize = new Vector2(250, 325);
        window.maxSize = new Vector2(250, 325);
        window.Show();
    }

    void OnEnable()
    {
        content.text = "Transform Tools";
        titleContent = content;
    }

    void OnGUI()
    {
        var centerTitle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold,
            fontSize = 14
        };

        EditorGUILayout.BeginVertical();

        GUILayout.Space(10);

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        GUILayout.Label("Group and Ungroup", centerTitle);

        wantedGroupName = EditorGUILayout.TextField("", wantedGroupName);

        if (GUILayout.Button("Group"))
        {
            Group();
            wantedGroupName = string.Empty;
        }

        if (GUILayout.Button("Ungroup"))
        {
            UnGroup();
        }
        EditorGUILayout.EndVertical();

        GUILayout.Space(15);

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        GUILayout.Label("Transform Configuration", centerTitle);

        if (GUILayout.Button("Reset Transform"))
        {
            ApplyToSelection(transform =>
            {
                Undo.RecordObject(transform, "Reset Transform");
                transform.position = Vector3.zero;
                transform.rotation = Quaternion.identity;
                transform.localScale = Vector3.one;
            });
        }

        GUILayout.Space(10);

        if (GUILayout.Button("Reset Position"))
        {
            ApplyToSelection(transform =>
            {
                Undo.RecordObject(transform, "Reset Position");
                transform.position = Vector3.zero;
            });
        }

        if (GUILayout.Button("Reset Rotation"))
        {
            ApplyToSelection(transform =>
            {
                Undo.RecordObject(transform, "Reset Rotation");
                transform.rotation = Quaternion.identity;
            });
        }

        if (GUILayout.Button("Reset Scale"))
        {
            ApplyToSelection(transform =>
            {
                Undo.RecordObject(transform, "Reset Scale");
                transform.localScale = Vector3.one;
            });
        }

        EditorGUILayout.EndVertical();

        GUILayout.Space(15);

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        GUILayout.Label("Create Empty", centerTitle);

        wantedEmptyName = EditorGUILayout.TextField("", wantedEmptyName);

        if (GUILayout.Button("Create"))
        {
            CreateEmpty();
            wantedEmptyName = string.Empty;
        }

        EditorGUILayout.EndVertical();
        EditorGUILayout.EndVertical();
    }
    #endregion

    #region FUNCTIONS
    void ApplyToSelection(Action<Transform> action)
    {
        Transform[] selection = Selection.transforms;
        if (selection.Length == 0)
        {
            EditorUtility.DisplayDialog("Sin selección", "Selecciona al menos un GameObject.", "Ok");
            return;
        }

        foreach (var transform in selection)
        {
            action.Invoke(transform);
        }
    }

    void Group()
    {
        Transform[] selection = Selection.transforms;
        if (selection.Length == 0) return;

        Vector3 center = Vector3.zero;
        foreach (var t in selection) center += t.position;
        center /= selection.Length;

        string groupName = string.IsNullOrEmpty(wantedGroupName) ? "New_Group" : wantedGroupName + "_Group";
        GameObject groupObject = new GameObject(groupName);
        Undo.RegisterCreatedObjectUndo(groupObject, "Group Objects");
        groupObject.transform.position = center;

        if (Selection.activeTransform?.parent != null)
            groupObject.transform.SetParent(Selection.activeTransform.parent);

        foreach (var t in selection)
        {
            Undo.SetTransformParent(t, groupObject.transform, "Group Objects");
        }
    }

    void UnGroup()
    {
        Transform[] selection = Selection.transforms;
        if (selection.Length == 0) return;

        foreach (var t in selection)
        {
            Undo.SetTransformParent(t, null, "Ungroup Objects");
        }

        Transform parent = Selection.activeTransform?.parent;
        if (parent != null && parent.childCount == 0)
        {
            Undo.DestroyObjectImmediate(parent.gameObject);
        }
    }

    void CreateEmpty()
    {
        string EmptyName = string.Empty;
        EmptyName += wantedEmptyName;


        if (EmptyName.Length > 0)
        {
            Transform Empty = new GameObject(EmptyName).transform;
            Empty.position = Vector3.zero;
        }
        else
        {
            Transform Empty = new GameObject("EMPTY").transform;
            Empty.position = Vector3.zero;
        }
    }
    #endregion
}
