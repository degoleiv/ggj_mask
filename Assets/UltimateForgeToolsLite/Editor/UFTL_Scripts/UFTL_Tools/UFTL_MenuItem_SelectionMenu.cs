using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public static class UFTL_MenuItem_SelectionMenu 
{
    static List<GameObject> parents;
    [MenuItem("Ultimate Forge Tools Lite/Selection/Get Parent from selection", false, 1)]
    public static void GetMyParent()
    {
        parents = new List<GameObject>();
        foreach (GameObject obj in Selection.gameObjects)
        {
            parents.Add(obj.transform.parent.gameObject);
        }
        GameObject[] gos = parents.ToArray();
        Selection.objects = gos;
    }

    static List<GameObject> allChilds;
    [MenuItem("Ultimate Forge Tools Lite/Selection/Select Childs", false, 2)]
    public static void SelectAllChilds()
    {
        allChilds = new List<GameObject>();
        foreach (GameObject obj in Selection.gameObjects)
        {
            for (int i = 0; i < obj.transform.childCount; i++)
            {
                allChilds.Add(obj.transform.GetChild(i).gameObject);
            }
        }
        GameObject[] gos = allChilds.ToArray();
        Selection.objects = gos;
    }

    static List<GameObject> oddChilds;
    [MenuItem("Ultimate Forge Tools Lite/Selection/Select Odd Childs", false, 3)]
    public static void SelectOddChilds()
    {
        oddChilds = new List<GameObject>();
        foreach (GameObject obj in Selection.gameObjects)
        {
            for (int i = 0; i < obj.transform.childCount; i++)
            {
                if (i % 2 == 1)
                {
                    oddChilds.Add(obj.transform.GetChild(i).gameObject);
                }
            }
        }
        GameObject[] gos = oddChilds.ToArray();
        Selection.objects = gos;
    }

    static List<GameObject> evenChilds;
    [MenuItem("Ultimate Forge Tools Lite/Selection/Select Even Childs", false, 4)]
    public static void SelectEvenChilds()
    {
        evenChilds = new List<GameObject>();
        foreach (GameObject obj in Selection.gameObjects)
        {
            for (int i = 0; i < obj.transform.childCount; i++)
            {
                if (i % 2 == 0)
                {
                    evenChilds.Add(obj.transform.GetChild(i).gameObject);
                }
            }
        }
        GameObject[] gos = evenChilds.ToArray();
        Selection.objects = gos;
    }

}
