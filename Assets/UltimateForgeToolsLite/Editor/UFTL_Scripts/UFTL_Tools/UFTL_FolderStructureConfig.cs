using UnityEngine;
using UnityEditor;
using System.IO;

[CreateAssetMenu(fileName = "New Structure", menuName = "Ultimate Forge Tools Lite/Create Folder Structure")]
public class UFTL_FolderStructureConfig : ScriptableObject
{
    [Tooltip("List of folders to create. You can use nested routes like 'UI/Buttons'.")]
    public string[] folders;

#if UNITY_EDITOR
    [ContextMenu("Create Folders")]
    public void CreateStructure()
    {
        string basePath = GetSelectedPath();

        if (folders == null || folders.Length == 0)
        {
            Debug.LogWarning("No folders defined.");
            return;
        }

        foreach (string relativePath in folders)
        {
            string fullPath = Path.Combine(basePath, relativePath.Replace("/", Path.DirectorySeparatorChar.ToString()));
            if (!Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
                Debug.Log("Created folders: " + fullPath);
            }
            else
            {
                Debug.LogWarning("Already exists: " + fullPath);
            }
        }

        AssetDatabase.Refresh();
        Debug.Log("Folders created in: " + basePath);
    }

    private string GetSelectedPath()
    {
        foreach (Object obj in Selection.GetFiltered(typeof(Object), SelectionMode.Assets))
        {
            string path = AssetDatabase.GetAssetPath(obj);
            if (Directory.Exists(path))
                return path;
        }

        return "Assets";
    }
#endif
}

