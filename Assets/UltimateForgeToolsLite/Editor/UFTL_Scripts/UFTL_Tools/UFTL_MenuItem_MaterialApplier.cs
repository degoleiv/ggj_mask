using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class UFTL_MenuItem_MaterialApplier : EditorWindow
{
    private Material selectedMaterial;

    [MenuItem("Ultimate Forge Tools Lite/Material Utilities/Material Applier", false, 6)]
    public static void Init()
    {
        UFTL_MenuItem_MaterialApplier window = GetWindow<UFTL_MenuItem_MaterialApplier>(true, "Material Applier");
        window.minSize = new Vector2(350, 100);
        window.maxSize = new Vector2(350, 100);
        window.Show();
    }

    private void OnGUI()
    {
        GUILayout.Label("Material Applier", EditorStyles.boldLabel);
        selectedMaterial = (Material)EditorGUILayout.ObjectField("Material", selectedMaterial, typeof(Material), false);

        if (GUILayout.Button("Apply to Selected Objects"))
        {
            ApplyMaterialToSelection();
        }
    }

    private void ApplyMaterialToSelection()
    {
        if (selectedMaterial == null)
        {
            EditorUtility.DisplayDialog("Error", "Please assign a material first.", "OK");
            return;
        }

        GameObject[] selectedObjects = Selection.gameObjects;

        if (selectedObjects.Length == 0)
        {
            EditorUtility.DisplayDialog("Error", "No objects selected.", "OK");
            return;
        }

        Undo.RegisterCompleteObjectUndo(selectedObjects, "Assign Material");

        foreach (GameObject obj in selectedObjects)
        {
            Renderer renderer = obj.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = selectedMaterial;
            }
        }

        Debug.Log($"Material '{selectedMaterial.name}' applied to {selectedObjects.Length} object(s).");
    }

}
