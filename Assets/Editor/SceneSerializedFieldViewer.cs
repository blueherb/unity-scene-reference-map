using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SceneSerializedFieldViewer.Editor
{
    public class SceneSerializedFieldViewer : EditorWindow
    {
        private const string HiddenPathsSessionKey = "SceneSerializedFieldViewer.HiddenPaths";
        private const string ClosedObjectPathsSessionKey = "SceneSerializedFieldViewer.ClosedObjectPaths";
        private const string ClosedComponentKeysSessionKey = "SceneSerializedFieldViewer.ClosedComponentKeys";

        private readonly List<SceneObjectEntry> m_entries = new List<SceneObjectEntry>();

        [SerializeField] private List<string> m_hiddenObjectPaths = new List<string>();
        [SerializeField] private List<string> m_closedObjectPaths = new List<string>();
        [SerializeField] private List<string> m_closedComponentKeys = new List<string>();

        private Vector2 m_scrollPosition;
        private string m_searchText = string.Empty;
        private bool m_includeInactive;
        private bool m_includeAllMonoBehaviours;
        private bool m_useEnglish;
        private bool m_showHiddenList = true;
        private SortMode m_sortMode;
        private GUIStyle m_boldFoldoutStyle;

        private enum SortMode
        {
            Hierarchy,
            GameObjectName,
            ScriptName
        }

        [MenuItem("Tools/Scene Serialized Field Viewer")]
        private static void OpenWindow()
        {
            SceneSerializedFieldViewer window = GetWindow<SceneSerializedFieldViewer>(Labels.WindowTitleKo);
            window.Refresh();
        }

        private void OnEnable()
        {
            LoadHiddenPaths();
            LoadFoldoutState();
            Refresh();
        }

        private void OnHierarchyChange()
        {
            Refresh();
            Repaint();
        }

        private void OnGUI()
        {
            EnsureStyles();
            DrawToolbar();

            EditorGUILayout.Space(4f);

            Scene activeScene = SceneManager.GetActiveScene();
            EditorGUILayout.LabelField(Text(Labels.ActiveSceneKo, Labels.ActiveSceneEn), activeScene.IsValid() ? activeScene.name : Text(Labels.NoneKo, Labels.NoneEn));
            EditorGUILayout.LabelField(Text(Labels.FoundObjectsKo, Labels.FoundObjectsEn), m_entries.Count.ToString());

            EditorGUILayout.Space(4f);

            DrawLegend();

            m_scrollPosition = EditorGUILayout.BeginScrollView(m_scrollPosition);

            foreach (SceneObjectEntry entry in m_entries)
            {
                DrawObjectEntry(entry);
            }

            EditorGUILayout.EndScrollView();
        }

        private void EnsureStyles()
        {
            if (m_boldFoldoutStyle != null)
            {
                return;
            }

            m_boldFoldoutStyle = new GUIStyle(EditorStyles.foldout)
            {
                fontStyle = FontStyle.Bold
            };
        }

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            if (GUILayout.Button(Text(Labels.RefreshKo, Labels.RefreshEn), EditorStyles.toolbarButton, GUILayout.Width(80f)))
            {
                Refresh();
            }

            EditorGUI.BeginChangeCheck();
            m_includeInactive = GUILayout.Toggle(m_includeInactive, Text(Labels.IncludeInactiveKo, Labels.IncludeInactiveEn), EditorStyles.toolbarButton, GUILayout.Width(110f));
            if (EditorGUI.EndChangeCheck())
            {
                Refresh();
            }

            EditorGUI.BeginChangeCheck();
            m_includeAllMonoBehaviours = GUILayout.Toggle(m_includeAllMonoBehaviours, Text(Labels.AllMonoBehavioursKo, Labels.AllMonoBehavioursEn), EditorStyles.toolbarButton, GUILayout.Width(145f));
            if (EditorGUI.EndChangeCheck())
            {
                Refresh();
            }

            EditorGUI.BeginChangeCheck();
            m_useEnglish = GUILayout.Toggle(m_useEnglish, m_useEnglish ? Labels.KoreanKo : Labels.EnglishEn, EditorStyles.toolbarButton, GUILayout.Width(70f));
            if (EditorGUI.EndChangeCheck())
            {
                Refresh();
            }

            GUILayout.Space(8f);
            GUILayout.Label(Text(Labels.SortKo, Labels.SortEn), GUILayout.Width(35f));

            EditorGUI.BeginChangeCheck();
            m_sortMode = (SortMode)EditorGUILayout.Popup((int)m_sortMode, SortLabels, EditorStyles.toolbarPopup, GUILayout.Width(100f));
            if (EditorGUI.EndChangeCheck())
            {
                Refresh();
            }

            GUILayout.Space(8f);
            GUILayout.Label(Text(Labels.SearchKo, Labels.SearchEn), GUILayout.Width(35f));

            EditorGUI.BeginChangeCheck();
            m_searchText = GUILayout.TextField(m_searchText, EditorStyles.toolbarSearchField);
            if (EditorGUI.EndChangeCheck())
            {
                Refresh();
            }

            if (GUILayout.Button(Text(Labels.ClearKo, Labels.ClearEn), EditorStyles.toolbarButton, GUILayout.Width(55f)))
            {
                m_searchText = string.Empty;
                Refresh();
                GUI.FocusControl(null);
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawLegend()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField(Text(Labels.LegendKo, Labels.LegendEn), EditorStyles.boldLabel);
            EditorGUILayout.LabelField(Text(Labels.BoldHelpKo, Labels.BoldHelpEn));
            EditorGUILayout.LabelField(Text(Labels.HiddenHelpKo, Labels.HiddenHelpEn));

            EditorGUILayout.BeginHorizontal();
            m_showHiddenList = EditorGUILayout.Foldout(
                m_showHiddenList,
                string.Format(Text(Labels.HiddenListKo, Labels.HiddenListEn), m_hiddenObjectPaths.Count),
                true);

            GUI.enabled = m_hiddenObjectPaths.Count > 0;
            if (GUILayout.Button(Text(Labels.ShowAllKo, Labels.ShowAllEn), GUILayout.Width(110f)))
            {
                m_hiddenObjectPaths.Clear();
                SaveHiddenPaths();
                Refresh();
                GUIUtility.ExitGUI();
            }
            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();

            if (m_showHiddenList)
            {
                DrawHiddenObjects();
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(4f);
        }

        private void DrawHiddenObjects()
        {
            if (m_hiddenObjectPaths.Count == 0)
            {
                EditorGUILayout.LabelField(Text(Labels.NoHiddenResultsKo, Labels.NoHiddenResultsEn));
                return;
            }

            List<string> removedPaths = new List<string>();

            foreach (string path in m_hiddenObjectPaths)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(path);

                if (GUILayout.Button(Text(Labels.ShowKo, Labels.ShowEn), GUILayout.Width(85f)))
                {
                    removedPaths.Add(path);
                }

                EditorGUILayout.EndHorizontal();
            }

            if (removedPaths.Count == 0)
            {
                return;
            }

            foreach (string path in removedPaths)
            {
                m_hiddenObjectPaths.Remove(path);
            }

            SaveHiddenPaths();
            Refresh();
            GUIUtility.ExitGUI();
        }

        private void DrawObjectEntry(SceneObjectEntry entry)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();

            bool isOpen = !m_closedObjectPaths.Contains(entry.Path);
            bool newOpen = EditorGUILayout.Foldout(
                isOpen,
                entry.Path,
                true,
                m_boldFoldoutStyle);

            if (newOpen != isOpen)
            {
                SetListState(m_closedObjectPaths, entry.Path, !newOpen);
                SaveFoldoutState();
            }

            if (GUILayout.Button(Text(Labels.HideKo, Labels.HideEn), GUILayout.Width(60f)))
            {
                AddHiddenPath(entry.Path);
                Refresh();
                GUIUtility.ExitGUI();
            }

            if (GUILayout.Button(Text(Labels.SelectKo, Labels.SelectEn), GUILayout.Width(60f)))
            {
                Selection.activeGameObject = entry.GameObject;
                EditorGUIUtility.PingObject(entry.GameObject);
            }

            EditorGUILayout.EndHorizontal();

            if (newOpen)
            {
                EditorGUI.indentLevel++;

                foreach (ComponentEntry componentEntry in entry.Components)
                {
                    DrawComponentEntry(componentEntry);
                }

                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawComponentEntry(ComponentEntry entry)
        {
            bool isOpen = !m_closedComponentKeys.Contains(entry.StateKey);
            bool newOpen = EditorGUILayout.Foldout(
                isOpen,
                entry.DisplayName,
                true);

            if (newOpen != isOpen)
            {
                SetListState(m_closedComponentKeys, entry.StateKey, !newOpen);
                SaveFoldoutState();
            }

            if (!newOpen)
            {
                return;
            }

            EditorGUI.indentLevel++;

            if (entry.Component == null)
            {
                EditorGUILayout.HelpBox(Text(Labels.MissingScriptKo, Labels.MissingScriptEn), MessageType.Warning);
                EditorGUI.indentLevel--;
                return;
            }

            SerializedObject serializedObject = new SerializedObject(entry.Component);
            serializedObject.Update();

            EditorGUI.BeginChangeCheck();

            SerializedProperty property = serializedObject.GetIterator();
            bool enterChildren = true;

            while (property.NextVisible(enterChildren))
            {
                enterChildren = false;

                if (property.propertyPath == "m_Script")
                {
                    continue;
                }

                EditorGUILayout.PropertyField(property, true);
            }

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(entry.Component, Labels.UndoEditSerializedField);

                if (serializedObject.ApplyModifiedProperties())
                {
                    MarkDirty(entry.Component);
                }
            }

            EditorGUI.indentLevel--;
        }

        private void Refresh()
        {
            m_entries.Clear();

            Scene activeScene = SceneManager.GetActiveScene();
            if (!activeScene.IsValid() || !activeScene.isLoaded)
            {
                return;
            }

            foreach (GameObject root in activeScene.GetRootGameObjects())
            {
                CollectGameObject(root.transform, root.name);
            }

            SortEntries();
        }

        private void CollectGameObject(Transform transform, string path)
        {
            GameObject gameObject = transform.gameObject;
            if (!m_hiddenObjectPaths.Contains(path) && (m_includeInactive || gameObject.activeInHierarchy))
            {
                List<ComponentEntry> componentEntries = CollectComponents(gameObject, path);

                if (componentEntries.Count > 0)
                {
                    m_entries.Add(new SceneObjectEntry(gameObject, path, componentEntries));
                }
            }

            for (int i = 0; i < transform.childCount; i++)
            {
                Transform child = transform.GetChild(i);
                CollectGameObject(child, $"{path}/{child.name}");
            }
        }

        private List<ComponentEntry> CollectComponents(GameObject gameObject, string path)
        {
            List<ComponentEntry> componentEntries = new List<ComponentEntry>();
            Component[] components = gameObject.GetComponents<Component>();

            for (int i = 0; i < components.Length; i++)
            {
                Component component = components[i];

                if (component == null)
                {
                    if (MatchesSearch(path, Labels.MissingScriptEn))
                    {
                        componentEntries.Add(ComponentEntry.Missing(gameObject.GetInstanceID(), i, path));
                    }

                    continue;
                }

                MonoBehaviour behaviour = component as MonoBehaviour;
                if (behaviour == null)
                {
                    continue;
                }

                if (!m_includeAllMonoBehaviours && !IsProjectScript(behaviour))
                {
                    continue;
                }

                string typeName = component.GetType().Name;

                if (!MatchesSearch(path, typeName))
                {
                    continue;
                }

                if (!HasVisibleSerializedField(component))
                {
                    continue;
                }

            componentEntries.Add(ComponentEntry.Valid(component, typeName, path));
            }

            return componentEntries;
        }

        private bool HasVisibleSerializedField(Component component)
        {
            SerializedObject serializedObject = new SerializedObject(component);
            SerializedProperty property = serializedObject.GetIterator();
            bool enterChildren = true;

            while (property.NextVisible(enterChildren))
            {
                enterChildren = false;

                if (property.propertyPath != "m_Script")
                {
                    return true;
                }
            }

            return false;
        }

        private void SortEntries()
        {
            if (m_sortMode == SortMode.Hierarchy)
            {
                return;
            }

            m_entries.Sort(CompareEntries);
        }

        private int CompareEntries(SceneObjectEntry a, SceneObjectEntry b)
        {
            if (m_sortMode == SortMode.GameObjectName)
            {
                int nameCompare = string.Compare(a.GameObject.name, b.GameObject.name, StringComparison.OrdinalIgnoreCase);
                return nameCompare != 0 ? nameCompare : string.Compare(a.Path, b.Path, StringComparison.OrdinalIgnoreCase);
            }

            string aScriptName = a.Components.Count > 0 ? a.Components[0].DisplayName : string.Empty;
            string bScriptName = b.Components.Count > 0 ? b.Components[0].DisplayName : string.Empty;
            int scriptCompare = string.Compare(aScriptName, bScriptName, StringComparison.OrdinalIgnoreCase);
            return scriptCompare != 0 ? scriptCompare : string.Compare(a.Path, b.Path, StringComparison.OrdinalIgnoreCase);
        }

        private bool MatchesSearch(string path, string typeName)
        {
            if (string.IsNullOrWhiteSpace(m_searchText))
            {
                return true;
            }

            string searchText = m_searchText.Trim();
            return path.Contains(searchText, StringComparison.OrdinalIgnoreCase)
                || typeName.Contains(searchText, StringComparison.OrdinalIgnoreCase);
        }

        private bool IsProjectScript(MonoBehaviour behaviour)
        {
            MonoScript script = MonoScript.FromMonoBehaviour(behaviour);
            if (script == null)
            {
                return false;
            }

            string assetPath = AssetDatabase.GetAssetPath(script);
            return !string.IsNullOrEmpty(assetPath) && assetPath.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase);
        }

        private void AddHiddenPath(string path)
        {
            if (m_hiddenObjectPaths.Contains(path))
            {
                return;
            }

            m_hiddenObjectPaths.Add(path);
            SaveHiddenPaths();
        }

        private void SetListState(List<string> list, string value, bool shouldContain)
        {
            if (shouldContain)
            {
                if (!list.Contains(value))
                {
                    list.Add(value);
                }

                return;
            }

            list.Remove(value);
        }

        private void LoadHiddenPaths()
        {
            string savedPaths = SessionState.GetString(HiddenPathsSessionKey, string.Empty);
            if (string.IsNullOrEmpty(savedPaths))
            {
                return;
            }

            m_hiddenObjectPaths = new List<string>(savedPaths.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries));
        }

        private void SaveHiddenPaths()
        {
            SessionState.SetString(HiddenPathsSessionKey, string.Join("\n", m_hiddenObjectPaths));
        }

        private void LoadFoldoutState()
        {
            m_closedObjectPaths = LoadSessionList(ClosedObjectPathsSessionKey);
            m_closedComponentKeys = LoadSessionList(ClosedComponentKeysSessionKey);
        }

        private void SaveFoldoutState()
        {
            SessionState.SetString(ClosedObjectPathsSessionKey, string.Join("\n", m_closedObjectPaths));
            SessionState.SetString(ClosedComponentKeysSessionKey, string.Join("\n", m_closedComponentKeys));
        }

        private List<string> LoadSessionList(string key)
        {
            string savedValues = SessionState.GetString(key, string.Empty);
            if (string.IsNullOrEmpty(savedValues))
            {
                return new List<string>();
            }

            return new List<string>(savedValues.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries));
        }

        private void MarkDirty(Component component)
        {
            if (component == null)
            {
                return;
            }

            if (EditorUtility.IsPersistent(component))
            {
                EditorUtility.SetDirty(component);
                return;
            }

            Scene scene = component.gameObject.scene;
            if (scene.IsValid() && scene.isLoaded)
            {
                EditorSceneManager.MarkSceneDirty(scene);
            }
        }

        private string[] SortLabels => m_useEnglish ? Labels.SortLabelsEn : Labels.SortLabelsKo;

        private string Text(string korean, string english)
        {
            return m_useEnglish ? english : korean;
        }

        private static class Labels
        {
            public const string WindowTitleKo = "직렬화 필드";
            public const string ActiveSceneKo = "활성 씬";
            public const string ActiveSceneEn = "Active Scene";
            public const string AllMonoBehavioursKo = "모든 MonoBehaviour";
            public const string AllMonoBehavioursEn = "All MonoBehaviours";
            public const string BoldHelpKo = "굵게 표시된 항목은 GameObject 결과입니다.";
            public const string BoldHelpEn = "Bold rows are GameObject results.";
            public const string ClearKo = "지우기";
            public const string ClearEn = "Clear";
            public const string EnglishEn = "English";
            public const string FoundObjectsKo = "검색된 오브젝트";
            public const string FoundObjectsEn = "Found Objects";
            public const string HiddenHelpKo = "숨김은 현재 창에서만 적용됩니다.";
            public const string HiddenHelpEn = "Hidden results are ignored only in this window.";
            public const string HiddenListKo = "숨김 목록 ({0})";
            public const string HiddenListEn = "Hidden List ({0})";
            public const string HideKo = "숨김";
            public const string HideEn = "Hide";
            public const string IncludeInactiveKo = "비활성 포함";
            public const string IncludeInactiveEn = "Include Inactive";
            public const string KoreanKo = "한국어";
            public const string LegendKo = "범례";
            public const string LegendEn = "Legend";
            public const string MissingScriptEn = "Missing Script";
            public const string MissingScriptKo = "스크립트가 누락되었습니다.";
            public const string NoHiddenResultsKo = "숨김 처리된 결과가 없습니다.";
            public const string NoHiddenResultsEn = "No hidden results.";
            public const string NoneKo = "없음";
            public const string NoneEn = "None";
            public const string RefreshKo = "새로고침";
            public const string RefreshEn = "Refresh";
            public const string SearchKo = "검색";
            public const string SearchEn = "Search";
            public const string SelectKo = "선택";
            public const string SelectEn = "Select";
            public const string ShowKo = "다시 표시";
            public const string ShowEn = "Show";
            public const string ShowAllKo = "모두 다시 표시";
            public const string ShowAllEn = "Show All";
            public const string SortKo = "정렬";
            public const string SortEn = "Sort";
            public const string UndoEditSerializedField = "Edit Serialized Field";

            public static readonly string[] SortLabelsKo =
            {
                "계층순",
                "이름순",
                "스크립트순"
            };

            public static readonly string[] SortLabelsEn =
            {
                "Hierarchy",
                "Object Name",
                "Script Name"
            };
        }

        private sealed class SceneObjectEntry
        {
            public SceneObjectEntry(GameObject gameObject, string path, List<ComponentEntry> components)
            {
                GameObject = gameObject;
                Path = path;
                Components = components;
            }

            public GameObject GameObject { get; }
            public string Path { get; }
            public List<ComponentEntry> Components { get; }
        }

        private sealed class ComponentEntry
        {
            private ComponentEntry(Component component, string displayName, string stateKey, int foldoutId)
            {
                Component = component;
                DisplayName = displayName;
                StateKey = stateKey;
                FoldoutId = foldoutId;
            }

            public Component Component { get; }
            public string DisplayName { get; }
            public string StateKey { get; }
            public int FoldoutId { get; }

            public static ComponentEntry Valid(Component component, string typeName, string path)
            {
                return new ComponentEntry(component, typeName, $"{path}/{typeName}", component.GetInstanceID());
            }

            public static ComponentEntry Missing(int gameObjectId, int componentIndex, string path)
            {
                string stateKey = $"{path}/{Labels.MissingScriptEn}_{componentIndex}";
                return new ComponentEntry(null, Labels.MissingScriptEn, stateKey, gameObjectId * 397 ^ componentIndex);
            }
        }
    }
}
