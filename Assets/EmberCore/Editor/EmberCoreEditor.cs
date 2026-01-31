using UnityEngine;
using UnityEditor;
using EmberVFX;
using System.Collections.Generic;
using System.Linq;
using System; // Required for Enum handling

namespace EmberVFX.EditorTools
{
    [CustomEditor(typeof(EmberCoreVFXManagement))]
    public class EmberCoreEditor : Editor
    {
        private SerializedProperty libraryProp;
        private static readonly string EmberGUID = "74393133621a82443849df0465b9032e";

        private Texture2D EmberTexture;
        private GUIStyle _addButtonStyle;
        private GUIStyle _boxStyle;
        private GUIStyle _headerStyle;
        private GUIStyle _subHeaderStyle;
        private GUIStyle _titleFoldoutStyle;

        // --- FILTER VARIABLES ---
        private bool _showFilters = false;
        private int _filterCategoryIndex = 0;
        private int _filterContextIndex = 0;
        private int _filterCostIndex = 0;
        private int _filterSystemIndex = 0;

        private void OnEnable()
        {
            libraryProp = serializedObject.FindProperty("library");

            if (EmberTexture == null)
            {
                string path = AssetDatabase.GUIDToAssetPath(EmberGUID);
                if (!string.IsNullOrEmpty(path))
                {
                    EmberTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                }
            }
        }

        private void InitStyles()
        {
            if (_addButtonStyle == null)
            {
                _addButtonStyle = new GUIStyle(GUI.skin.button)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = 13,
                    fixedHeight = 40,
                    fontStyle = FontStyle.Bold,
                    normal = { textColor = Color.white }
                };
            }

            if (_boxStyle == null)
            {
                _boxStyle = new GUIStyle(EditorStyles.helpBox)
                {
                    padding = new RectOffset(20, 10, 10, 10),
                    margin = new RectOffset(0, 0, 10, 0)
                };
            }

            if (_headerStyle == null)
            {
                _headerStyle = new GUIStyle(EditorStyles.boldLabel)
                {
                    fontSize = 14,
                    normal = { textColor = Color.white }
                };
            }

            if (_subHeaderStyle == null)
            {
                _subHeaderStyle = new GUIStyle(EditorStyles.label)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = 13,
                    fontStyle = FontStyle.Bold,
                    normal = { textColor = Color.gray } // Adjusted to standard color to ensure compilation
                };
            }

            if (_titleFoldoutStyle == null)
            {
                _titleFoldoutStyle = new GUIStyle(EditorStyles.foldout)
                {
                    fontSize = 13,
                    fontStyle = FontStyle.Bold
                };
            }
        }

        public override void OnInspectorGUI()
        {
            InitStyles();
            serializedObject.Update();

            EditorGUILayout.Space(10);
            if (EmberTexture != null)
            {
                float logoWidth = 350f;
                float ratio = (float)EmberTexture.height / Mathf.Max(1f, EmberTexture.width);
                float logoHeight = logoWidth * ratio;

                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.Label(EmberTexture, GUILayout.Width(logoWidth), GUILayout.Height(logoHeight));
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }
            EditorGUILayout.Space(5);

            Color defaultColor = GUI.backgroundColor;
            GUI.backgroundColor = Color.green;
            if (GUILayout.Button("+ Add New Effect", _addButtonStyle))
            {
                AddNewEffect();
            }
            GUI.backgroundColor = defaultColor;

            // --- FILTER SYSTEM ---
            DrawFilterSection();
            // ---------------------

            int indexToDelete = -1;

            for (int i = 0; i < libraryProp.arraySize; i++)
            {
                SerializedProperty item = libraryProp.GetArrayElementAtIndex(i);

                // CHECK FILTERS
                if (IsItemFiltered(item))
                {
                    if (DrawEffectCard(item, i))
                    {
                        indexToDelete = i;
                    }
                }
            }

            if (indexToDelete > -1)
            {
                libraryProp.DeleteArrayElementAtIndex(indexToDelete);
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawFilterSection()
        {
            EditorGUILayout.Space(10);
            _showFilters = EditorGUILayout.Foldout(_showFilters, "Filters & Sorting", true, EditorStyles.foldoutHeader);

            if (_showFilters)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                // Enum Options
                string[] categoryOptions = GetEnumOptionsWithAll<EffectCategory>();
                string[] costOptions = GetEnumOptionsWithAll<EffectCost>();
                string[] systemOptions = GetEnumOptionsWithAll<VFXSystemType>();

                // Dynamic Context Options
                EmberCoreVFXManagement manager = (EmberCoreVFXManagement)target;
                List<string> ctxList = new List<string>();
                ctxList.Add("All");
                ctxList.AddRange(manager.availableContexts);
                string[] contextOptions = ctxList.ToArray();

                // Row 1: Category & Context
                EditorGUILayout.BeginHorizontal();
                EditorGUIUtility.labelWidth = 90;
                _filterCategoryIndex = EditorGUILayout.Popup("Category", _filterCategoryIndex, categoryOptions);
                _filterContextIndex = EditorGUILayout.Popup("Context", _filterContextIndex, contextOptions);
                EditorGUILayout.EndHorizontal();

                // Row 2: Cost & System
                EditorGUILayout.BeginHorizontal();
                EditorGUIUtility.labelWidth = 90;
                _filterCostIndex = EditorGUILayout.Popup("Cost", _filterCostIndex, costOptions);
                _filterSystemIndex = EditorGUILayout.Popup("System", _filterSystemIndex, systemOptions);
                EditorGUILayout.EndHorizontal();

                // Reset Button
                if (GUILayout.Button("Reset Filters", EditorStyles.miniButton))
                {
                    _filterCategoryIndex = 0;
                    _filterContextIndex = 0;
                    _filterCostIndex = 0;
                    _filterSystemIndex = 0;
                    GUI.FocusControl(null);
                }

                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.Space(5);
        }

        private bool IsItemFiltered(SerializedProperty item)
        {
            // 1. Category Filter (0 = All)
            if (_filterCategoryIndex > 0)
            {
                SerializedProperty catProp = item.FindPropertyRelative("metadata").FindPropertyRelative("category");
                // Index - 1 because "All" is at 0, but Enum starts at 0
                if (catProp.enumValueIndex != _filterCategoryIndex - 1) return false;
            }

            // 2. Context Filter (0 = All)
            if (_filterContextIndex > 0)
            {
                EmberCoreVFXManagement manager = (EmberCoreVFXManagement)target;
                // Safety check
                if (_filterContextIndex - 1 < manager.availableContexts.Count)
                {
                    string selectedContext = manager.availableContexts[_filterContextIndex - 1];
                    SerializedProperty ctxProp = item.FindPropertyRelative("metadata").FindPropertyRelative("context");
                    if (ctxProp.stringValue != selectedContext) return false;
                }
            }

            // 3. Cost Filter (0 = All)
            if (_filterCostIndex > 0)
            {
                SerializedProperty costProp = item.FindPropertyRelative("metadata").FindPropertyRelative("performanceCost");
                if (costProp.enumValueIndex != _filterCostIndex - 1) return false;
            }

            // 4. System Type Filter (0 = All)
            if (_filterSystemIndex > 0)
            {
                SerializedProperty sysProp = item.FindPropertyRelative("systemType");
                if (sysProp.enumValueIndex != _filterSystemIndex - 1) return false;
            }

            return true;
        }

        private string[] GetEnumOptionsWithAll<T>() where T : Enum
        {
            List<string> options = new List<string>();
            options.Add("All");
            options.AddRange(Enum.GetNames(typeof(T)));
            return options.ToArray();
        }

        private void AddNewEffect()
        {
            libraryProp.InsertArrayElementAtIndex(libraryProp.arraySize);
            SerializedProperty newItem = libraryProp.GetArrayElementAtIndex(libraryProp.arraySize - 1);

            EmberCoreVFXManagement manager = (EmberCoreVFXManagement)target;

            // --- 1. Basic IDs ---
            newItem.FindPropertyRelative("effectName").stringValue = "New_Effect_" + (libraryProp.arraySize);
            newItem.FindPropertyRelative("id").stringValue = System.Guid.NewGuid().ToString().Substring(0, 8).ToUpper();

            // --- 2. Metadata Defaults ---
            SerializedProperty meta = newItem.FindPropertyRelative("metadata");
            meta.FindPropertyRelative("author").stringValue = System.Environment.UserName;

            // Context Logic
            if (manager.availableContexts.Count > 0)
                meta.FindPropertyRelative("context").stringValue = manager.availableContexts[0];
            else
                meta.FindPropertyRelative("context").stringValue = "";

            // --- 3. Value Defaults (FIX FOR 0 VALUES) ---
            newItem.FindPropertyRelative("volume").floatValue = 1f;
            newItem.FindPropertyRelative("colliderSize").vector3Value = Vector3.one;
            newItem.FindPropertyRelative("scaleDuration").floatValue = 1f;
            newItem.FindPropertyRelative("scaleCurve").animationCurveValue = AnimationCurve.Linear(0, 0, 1, 1);

            // --- 4. Pooling Defaults ---
            newItem.FindPropertyRelative("prewarmCount").intValue = 5;
            newItem.FindPropertyRelative("maxPoolSize").intValue = 50;

            newItem.isExpanded = true;
        }

        private bool DrawEffectCard(SerializedProperty item, int index)
        {
            bool deleteRequest = false;
            SerializedProperty nameProp = item.FindPropertyRelative("effectName");
            SerializedProperty idProp = item.FindPropertyRelative("id");

            SerializedProperty meta = item.FindPropertyRelative("metadata");
            SerializedProperty catProp = meta.FindPropertyRelative("category");
            SerializedProperty ctxProp = meta.FindPropertyRelative("context");
            SerializedProperty sysProp = item.FindPropertyRelative("systemType");

            string categoryName = catProp.enumDisplayNames[catProp.enumValueIndex];
            string contextName = string.IsNullOrEmpty(ctxProp.stringValue) ? "None" : ctxProp.stringValue;
            string systemName = sysProp.enumDisplayNames[sysProp.enumValueIndex];

            if (string.IsNullOrEmpty(idProp.stringValue))
                idProp.stringValue = System.Guid.NewGuid().ToString().Substring(0, 8).ToUpper();

            EditorGUILayout.BeginVertical(_boxStyle);
            EditorGUILayout.BeginHorizontal();

            string rawName = string.IsNullOrEmpty(nameProp.stringValue) ? "Unnamed Effect" : nameProp.stringValue;

            // --- UPDATED TITLE ---
            string displayTitle = $"{rawName} [{categoryName}] [{contextName}] [{systemName}]";

            item.isExpanded = EditorGUILayout.Foldout(item.isExpanded, displayTitle, true, _titleFoldoutStyle);

            GUILayout.FlexibleSpace();

            Color defaultBg = GUI.backgroundColor;
            GUI.backgroundColor = Color.red;
            if (GUILayout.Button("Remove"))
            {
                if (EditorUtility.DisplayDialog("Delete Effect", $"Delete '{rawName}'?", "Yes", "Cancel"))
                {
                    deleteRequest = true;
                }
            }
            GUI.backgroundColor = defaultBg;
            EditorGUILayout.EndHorizontal();

            if (item.isExpanded)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.Space(5);
                float originalLabelWidth = EditorGUIUtility.labelWidth;

                // --- IDENTITY ---
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                DrawSectionHeader("VFX MAIN INFO");
                EditorGUIUtility.labelWidth = 150;
                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.PropertyField(nameProp, new GUIContent("Effect Name"));
                    GUI.backgroundColor = Color.blueViolet;
                    if (GUILayout.Button("Copy", GUILayout.Width(45)))
                    {
                        EditorGUIUtility.systemCopyBuffer = nameProp.stringValue;
                        Debug.Log($"[EmberCore] Name '{nameProp.stringValue}' copied to clipboard.");
                    }
                    GUI.backgroundColor = defaultBg;
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                {
                    GUI.enabled = false;
                    EditorGUILayout.TextField("Effect ID", idProp.stringValue);
                    GUI.enabled = true;
                    GUI.backgroundColor = Color.blueViolet;
                    if (GUILayout.Button("Copy", GUILayout.Width(45)))
                    {
                        EditorGUIUtility.systemCopyBuffer = idProp.stringValue;
                        Debug.Log($"[EmberCore] ID '{idProp.stringValue}' copied to clipboard.");
                    }
                    GUI.backgroundColor = defaultBg;
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space(5);
                meta.isExpanded = EditorGUILayout.Foldout(meta.isExpanded, "Metadata Info", true);

                if (meta.isExpanded)
                {
                    EditorGUI.indentLevel++;

                    // Row 1
                    EditorGUILayout.BeginHorizontal();
                    EditorGUIUtility.labelWidth = 90;
                    EditorGUILayout.PropertyField(meta.FindPropertyRelative("author"), new GUIContent("Author"));
                    EditorGUIUtility.labelWidth = 50;
                    EditorGUILayout.PropertyField(meta.FindPropertyRelative("version"), new GUIContent("Ver"));
                    EditorGUIUtility.labelWidth = originalLabelWidth;
                    EditorGUILayout.EndHorizontal();

                    // Row 2
                    EditorGUILayout.BeginHorizontal();
                    EditorGUIUtility.labelWidth = 90;
                    EditorGUILayout.PropertyField(meta.FindPropertyRelative("category"), new GUIContent("Category"), GUILayout.Width(174));

                    // --- CUSTOM CONTEXT DROPDOWN + BUTTON ---
                    EmberCoreVFXManagement manager = (EmberCoreVFXManagement)target;
                    EditorGUIUtility.labelWidth = 80;

                    EditorGUILayout.BeginHorizontal();

                    // Validate Index
                    int ctxIndex = manager.availableContexts.IndexOf(ctxProp.stringValue);
                    if (ctxIndex == -1) ctxIndex = 0;

                    // Avoid empty dropdown error
                    if (manager.availableContexts.Count > 0)
                    {
                        int newIndex = EditorGUILayout.Popup("Context", ctxIndex, manager.availableContexts.ToArray());
                        if (newIndex >= 0 && newIndex < manager.availableContexts.Count)
                        {
                            ctxProp.stringValue = manager.availableContexts[newIndex];
                        }
                    }
                    else
                    {
                        EditorGUILayout.LabelField("Context", "No Contexts Available");
                    }

                    // Draw Edit Button
                    if (GUILayout.Button("+", GUILayout.Width(25), GUILayout.Height(18)))
                    {
                        PopupWindow.Show(new Rect(Event.current.mousePosition, Vector2.zero), new ContextPopup(manager));
                    }
                    EditorGUILayout.EndHorizontal();

                    EditorGUIUtility.labelWidth = originalLabelWidth;
                    EditorGUILayout.EndHorizontal();

                    // Row 3
                    EditorGUILayout.BeginHorizontal();
                    EditorGUIUtility.labelWidth = 90;
                    EditorGUILayout.PropertyField(meta.FindPropertyRelative("intensity"), new GUIContent("Intensity"));
                    EditorGUILayout.PropertyField(meta.FindPropertyRelative("performanceCost"), new GUIContent("Cost"));
                    EditorGUIUtility.labelWidth = originalLabelWidth;
                    EditorGUILayout.EndHorizontal();

                    SerializedProperty platProp = meta.FindPropertyRelative("supportedPlatforms");
                    platProp.intValue = (int)(EffectPlatform)EditorGUILayout.EnumFlagsField(new GUIContent("Platforms"), (EffectPlatform)platProp.intValue);

                    EditorGUI.indentLevel--;
                }

                EditorGUILayout.Space(5);
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(5);

                // --- VISUALS ---
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                DrawSectionHeader("VISUAL ASSETS");
                // sysProp fetched at top
                EditorGUIUtility.labelWidth = 150;
                EditorGUILayout.PropertyField(sysProp, new GUIContent("System Type"));
                if (sysProp.enumValueIndex == (int)VFXSystemType.Shuriken)
                    EditorGUILayout.PropertyField(item.FindPropertyRelative("shurikenPrefab"), new GUIContent("Particle Prefab"));
                else
                    EditorGUILayout.PropertyField(item.FindPropertyRelative("vfxGraphPrefab"), new GUIContent("VFX Graph Prefab"));

                EditorGUILayout.Space(5);
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(5);

                // --- AUDIO ---
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                DrawSectionHeader("AUDIO");
                // Mixer Group
                EditorGUILayout.PropertyField(item.FindPropertyRelative("mixerGroup"), new GUIContent("Output Mixer Group"));

                EditorGUILayout.PropertyField(item.FindPropertyRelative("clips"), new GUIContent("Audio Clips"), true);
                if (item.FindPropertyRelative("clips").arraySize > 0)
                {
                    EditorGUILayout.PropertyField(item.FindPropertyRelative("volume"));
                    EditorGUILayout.PropertyField(item.FindPropertyRelative("randomizePitch"));
                }
                EditorGUILayout.Space(5);
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(5);

                // --- PHYSICS ---
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                DrawSectionHeader("INTERACTION & PHYSICS");
                SerializedProperty colType = item.FindPropertyRelative("colliderType");
                EditorGUILayout.PropertyField(colType);

                if (colType.enumValueIndex != (int)EmberColliderType.None)
                {
                    EditorGUILayout.PropertyField(item.FindPropertyRelative("isTrigger"));
                    EditorGUILayout.PropertyField(item.FindPropertyRelative("colliderSize"));

                    SerializedProperty useScale = item.FindPropertyRelative("useScaling");
                    EditorGUILayout.PropertyField(useScale, new GUIContent("Animate Hitbox?"));
                    if (useScale.boolValue)
                    {
                        EditorGUILayout.PropertyField(item.FindPropertyRelative("scaleDuration"));
                        EditorGUILayout.PropertyField(item.FindPropertyRelative("scaleCurve"));
                    }
                }

                EditorGUILayout.Space(5);
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(5);

                // --- POOLING ---
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                DrawSectionHeader("OPTIMIZATION");
                EditorGUILayout.BeginHorizontal();
                EditorGUIUtility.labelWidth = 100;
                EditorGUILayout.PropertyField(item.FindPropertyRelative("prewarmCount"), new GUIContent("Pre-warm"));
                EditorGUILayout.PropertyField(item.FindPropertyRelative("maxPoolSize"), new GUIContent("Max Limit"));
                EditorGUIUtility.labelWidth = 100;
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.PropertyField(item.FindPropertyRelative("durationOverride"), new GUIContent("Force Duration", "0 = Auto"));

                EditorGUI.indentLevel--;
                EditorGUILayout.Space(5);
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(5);
            }

            EditorGUILayout.EndVertical();
            return deleteRequest;
        }

        private void DrawSectionHeader(string label)
        {
            EditorGUILayout.LabelField(label, _subHeaderStyle);
            Rect rect = EditorGUILayout.GetControlRect(false, 1);
            rect.height = 1;
            EditorGUI.DrawRect(rect, new Color(0.6f, 0.6f, 0.6f));
            EditorGUILayout.Space(5);
        }

        // --- POPUP WINDOW CLASS ---
        public class ContextPopup : PopupWindowContent
        {
            private EmberCoreVFXManagement _manager;
            private string _newEntry = "";

            public ContextPopup(EmberCoreVFXManagement manager)
            {
                _manager = manager;
            }

            public override Vector2 GetWindowSize()
            {
                return new Vector2(250, 300);
            }

            public override void OnGUI(Rect rect)
            {
                GUILayout.Label("Manage Contexts", EditorStyles.boldLabel);

                // ADD AREA
                GUILayout.BeginHorizontal();
                _newEntry = EditorGUILayout.TextField(_newEntry);
                if (GUILayout.Button("Add", GUILayout.Width(50)))
                {
                    if (!string.IsNullOrEmpty(_newEntry) && !_manager.availableContexts.Contains(_newEntry))
                    {
                        // UNDO SUPPORT
                        Undo.RecordObject(_manager, "Add Context");
                        _manager.availableContexts.Add(_newEntry);
                        EditorUtility.SetDirty(_manager); // Force save
                        _newEntry = "";
                    }
                }
                GUILayout.EndHorizontal();

                EditorGUILayout.Space(5);
                GUILayout.Label("Existing Contexts:", EditorStyles.miniLabel);

                // LIST AREA
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                for (int i = 0; i < _manager.availableContexts.Count; i++)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(_manager.availableContexts[i]);
                    if (GUILayout.Button("X", GUILayout.Width(25)))
                    {
                        // UNDO SUPPORT
                        Undo.RecordObject(_manager, "Remove Context");
                        _manager.availableContexts.RemoveAt(i);
                        EditorUtility.SetDirty(_manager); // Force save
                        break;
                    }
                    GUILayout.EndHorizontal();
                }
                EditorGUILayout.EndVertical();
            }
        }
    }
}