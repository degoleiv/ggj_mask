using UnityEngine;
using UnityEditor;
using System;

public class UFTL_PackageStartScreen : EditorWindow
{
    private const string PrefShowAtStartup = "UFTL_ShowAtStartup";
    private static readonly string LogoGUID = "dfbde66f864856a40854d6ebf2b9317f";

    private Vector2 m_scrollPosition = Vector2.zero;
    private bool showAtStartup = true;
    private Texture2D logoTexture;

    [NonSerialized]
    private GUIStyle titleStyle = null;
    [NonSerialized]
    private GUIStyle titleStyle2 = null;
    [NonSerialized]
    private GUIStyle buttonStyle = null;

    [MenuItem("Ultimate Forge Tools Lite/Start Screen", false, 0)]
    public static void Init()
    {
        UFTL_PackageStartScreen window = GetWindow<UFTL_PackageStartScreen>(true, "Ultimate Forge Tools Lite Start Screen");
        window.minSize = new Vector2(550, 450);
        window.maxSize = new Vector2(550, 450);
        window.Show();
    }


    [InitializeOnLoadMethod]
    private static void ShowAtStartupCheck()
    {
        if (!SessionState.GetBool("UFTL_AlreadyOpened", false))
        {
            SessionState.SetBool("UFTL_AlreadyOpened", true);
            bool show = EditorPrefs.GetBool(PrefShowAtStartup, true);
            if (show)
                EditorApplication.delayCall += Init;
        }
    }

    private void OnEnable()
    {
        showAtStartup = EditorPrefs.GetBool(PrefShowAtStartup, true);

        if (logoTexture == null)
        {
            string path = AssetDatabase.GUIDToAssetPath(LogoGUID);
            if (!string.IsNullOrEmpty(path))
            {
                logoTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            }
        }
    }

    private void EnsureStyles()
    {
        if (titleStyle == null)
        {
            titleStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 14,
                alignment = TextAnchor.MiddleLeft
            };
        }

        if (titleStyle2 == null)
        {
            titleStyle2 = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 14,
                alignment = TextAnchor.MiddleCenter
            };
        }

        if (buttonStyle == null)
        {
            buttonStyle = new GUIStyle(GUI.skin.button)
            {
                alignment = TextAnchor.MiddleLeft,
                fontSize = 12,
                fixedHeight = 22,
                padding = new RectOffset(8, 8, 2, 2),
                margin = new RectOffset(4, 4, 2, 2)
            };
        }
    }

    private void OnGUI()
    {
        EnsureStyles();
        GUILayout.Space(10);
        
        EditorGUILayout.BeginHorizontal();
        {
            EditorGUILayout.BeginVertical(GUILayout.ExpandHeight(true));
            {
                GUILayout.Label("Welcome to Ultimate Forge Tools Lite", titleStyle);

                m_scrollPosition = EditorGUILayout.BeginScrollView(m_scrollPosition, GUILayout.ExpandHeight(true));
                {
                    EditorGUILayout.HelpBox(
                        "This package is a free version and can only be used for educational purposes.\n" +
                        "Ultimate Forge Tools is a suite of tools designed to improve the workflow in Unity.\n\n" +
                        "Access all utilities from the 'Ultimate Forge Tools Lite' menu.\n\n" +
                        "This free package includes the following tools:\n" +
                        "• Selection: Get parent from selection, Select Childs, Select Odd Childs, Select Even Childs\n" +
                        "• Unique Functions: Sort Children A-Z.\n" +
                        "• Material Utilities: Material Applier.\n" +
                        "• Tools: Transform Tools.\n" +
                        "• Folder Structure Creator." ,
                        MessageType.Info
                    );
                }
                EditorGUILayout.EndScrollView();
            }
            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(15);

        EditorGUILayout.BeginHorizontal();
        {
            EditorGUILayout.BeginVertical(GUILayout.ExpandHeight(true));
            {
                GUILayout.Label("License", titleStyle2);
                {
                    EditorGUILayout.HelpBox(
                        "© 2026 Black Nova Interactive. All rights reserved.\n\n" +
                        "Ultimate Forge Tools is proprietary software. Usage is permitted only for users with a valid license obtained directly from the author.\n" +
                        "Without prior written consent, it is prohibited to copy, distribute, modify, reverse engineer, resell, sublicense, or rent the software." ,
                        MessageType.Warning
                    );
                }
            }
            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.EndHorizontal();

        GUILayout.FlexibleSpace();

        GUILayout.Space(15);

        if (logoTexture != null)
        {
            float logoWidth = 200f;
            float ratio = (float)logoTexture.height / Mathf.Max(1f, logoTexture.width);
            float logoHeight = logoWidth * ratio;

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label(logoTexture, GUILayout.Width(logoWidth), GUILayout.Height(logoHeight));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }


        GUILayout.Space(5);
        EditorGUILayout.BeginHorizontal();
        {
            GUILayout.FlexibleSpace();
            bool newShow = EditorGUILayout.ToggleLeft("Show at StartUp", showAtStartup, GUILayout.Width(160));
            if (newShow != showAtStartup)
            {
                showAtStartup = newShow;
                EditorPrefs.SetBool(PrefShowAtStartup, showAtStartup);
            }
            GUILayout.FlexibleSpace();
        }
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(5);

        EditorGUILayout.BeginHorizontal("ProjectBrowserBottomBarBg", GUILayout.ExpandWidth(true), GUILayout.Height(25));
        {
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Close", GUILayout.Width(80)))
                Close();
        }
        EditorGUILayout.EndHorizontal();
    }
}
