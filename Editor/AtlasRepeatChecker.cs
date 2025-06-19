using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.U2D;

namespace CYi.Editor.AtlasRepeatChecker
{
    /// <summary>
    /// GUID-based tool for checking repeat images in atlases - supports multiple folders and atlases
    /// </summary>
    public class AtlasRepeatChecker : EditorWindow
    {
        private Vector2 scrollPosition;
        private Vector2 sourcesScrollPosition;
        private bool hasAnalysisResults = false;
        private string searchFilter = string.Empty;

        private static readonly Regex GuidRegex = new(ToolConstants.GUID_PATTERN, RegexOptions.Compiled);

        /// <summary>
        /// Check which atlas catch the same GUID
        /// </summary>
        private Dictionary<string, List<AtlasInfo>> guidToAtlasMap = new();

        private Dictionary<string, string> guidToAssetPathMap = new();
        private List<string> repeatGuids = new();
        private List<AtlasInfo> allAtlases = new();

        private HashSet<string> atlasFolders = new();
        private HashSet<SpriteAtlas> specificAtlases = new();
        private int TotalSourcesCount => atlasFolders.Count + specificAtlases.Count;

        [MenuItem(ToolConstants.MENU_ITEM_PATH)]
        public static void ShowWindow()
        {
            AtlasRepeatChecker window = GetWindow<AtlasRepeatChecker>(nameof(AtlasRepeatChecker));
            window.minSize = new Vector2(700, 600);
            window.Show();
        }

        private void OnGUI()
        {
            using (new EditorGUILayout.VerticalScope())
            {
                EditorGUILayout.HelpBox("This tool analyzes specified multiple folders and atlas files, checking for repeat internal GUIDs to find identical images used by multiple atlases", MessageType.Info);
                GUILayout.Space(10);

                DrawSourcesSelectionArea();

                GUILayout.Space(10);

                using (new EditorGUILayout.HorizontalScope())
                {
                    bool hasAnySource = TotalSourcesCount > 0;

                    using (new EditorGUI.DisabledScope(!hasAnySource))
                    {
                        if (GUILayout.Button("Analyze Atlas Guids", GUILayout.Height(30)))
                        {
                            AnalyzeAtlasGuids();
                        }
                    }

                    if (hasAnalysisResults && GUILayout.Button("Clear Results", GUILayout.Height(30)))
                    {
                        ClearResults();
                    }

                    using (new EditorGUI.DisabledScope(!hasAnySource))
                    {
                        if (GUILayout.Button("Clear All Sources", GUILayout.Height(30)))
                        {
                            ClearAllSources();
                        }
                    }
                }

                if (TotalSourcesCount == 0)
                {
                    EditorGUILayout.HelpBox("Add at least one folder or atlas source", MessageType.Warning);
                }

                GUILayout.Space(10);

                if (hasAnalysisResults)
                {
                    DisplayResults();
                }
            }
        }

        private void DrawSourcesSelectionArea()
        {
            var rect = EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.ExpandWidth(true));
            DrawDragDropArea(rect);
            DrawSources();
            EditorGUILayout.EndVertical();

            GUILayout.Space(5);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Add Folder"))
                {
                    string selectedFolder = EditorUtility.OpenFolderPanel("Select Atlas Folder", Application.dataPath, string.Empty);
                    AddAssetManually(atlasFolders, selectedFolder, (path) => (path));
                }
                if (GUILayout.Button("Add Atlas"))
                {
                    string selectedAtlasPath = EditorUtility.OpenFilePanel("Select Atlas", Application.dataPath, "spriteatlas");
                    AddAssetManually(specificAtlases, selectedAtlasPath, (path) => AssetDatabase.LoadAssetAtPath<SpriteAtlas>(path));
                }
            }
        }

        private void DrawDragDropArea(Rect dropRect)
        {
            HandleDragAndDrop(dropRect);

            EditorGUI.DrawRect(dropRect, new Color(0.4f, 0.4f, 0.4f, 0.4f));

            if (TotalSourcesCount == 0)
            {
                GUI.Label(dropRect, "Drag folders or atlases to here", EditorStyles.centeredGreyMiniLabel);
            }
        }

        private void HandleDragAndDrop(Rect dropRect)
        {
            Event currentEvent = Event.current;
            if (!dropRect.Contains(currentEvent.mousePosition))
                return;

            switch (currentEvent.type)
            {
                case EventType.DragUpdated:
                case EventType.DragPerform:

                    HashSet<string> draggedFolders = new();
                    HashSet<SpriteAtlas> draggedAtlases = new();

                    foreach (UnityEngine.Object draggedObject in DragAndDrop.objectReferences)
                    {
                        if (draggedObject == null)
                            continue;

                        string path = AssetDatabase.GetAssetPath(draggedObject);

                        if (string.IsNullOrEmpty(path))
                            continue;

                        if (AssetDatabase.IsValidFolder(path))
                        {
                            draggedFolders.Add(path);
                        }
                        else if (draggedObject is SpriteAtlas atlas)
                        {
                            draggedAtlases.Add(atlas);
                        }
                    }

                    bool isValidDrop = draggedFolders.Count > 0 || draggedAtlases.Count > 0;
                    DragAndDrop.visualMode = isValidDrop ? DragAndDropVisualMode.Copy : DragAndDropVisualMode.Rejected;
                    if (isValidDrop && currentEvent.type == EventType.DragPerform)
                    {
                        DragAndDrop.AcceptDrag();

                        foreach (string folder in draggedFolders)
                        {
                            atlasFolders.Add(folder);
                        }

                        foreach (var atlas in draggedAtlases)
                        {
                            specificAtlases.Add(atlas);
                        }

                        GUI.changed = true;

                        if (hasAnalysisResults)
                            ClearResults();
                    }

                    currentEvent.Use();
                    break;
            }
        }

        private void DrawSources()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("Sources", EditorStyles.boldLabel);
                EditorGUILayout.LabelField($"Atlas: {specificAtlases.Count} Folder: {atlasFolders.Count}", EditorStyles.miniLabel, GUILayout.Width(90));
            }

            sourcesScrollPosition = EditorGUILayout.BeginScrollView(sourcesScrollPosition, GUILayout.MaxHeight(100));

            int elementIndex = 0;
            DrawSourceSet(atlasFolders, "📁", typeof(DefaultAsset), (path) => AssetDatabase.LoadAssetAtPath<DefaultAsset>(path));
            DrawSourceSet(specificAtlases, "🖼️", typeof(SpriteAtlas), (atlas) => atlas);

            EditorGUILayout.EndScrollView();

            void DrawSourceSet<T>(HashSet<T> set, string icon, Type objectFieldType, Func<T, UnityEngine.Object> getDisplayObject)
            {
                foreach (var source in set.ToList())
                {
                    bool removed = false;

                    if (source == null || (source is string s && string.IsNullOrEmpty(s)))
                    {
                        set.Remove(source);
                        continue;
                    }

                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.LabelField($"Element {elementIndex}", GUILayout.Width(80));
                        EditorGUILayout.LabelField(icon, GUILayout.Width(20));

                        UnityEngine.Object displayObj = getDisplayObject.Invoke(source);

                        using (new EditorGUI.DisabledScope(true))
                        {
                            EditorGUILayout.ObjectField(displayObj, objectFieldType, false);
                        }

                        if (GUILayout.Button("×", GUILayout.Width(20)))
                        {
                            set.Remove(source);
                            if (hasAnalysisResults)
                                ClearResults();
                            removed = true;
                        }

                        SelectObjectButton(displayObj);
                    }
                    if (removed)
                        break;

                    elementIndex++;
                }
            }
        }

        private void SelectObjectButton(UnityEngine.Object displayObj)
        {
            if (GUILayout.Button("◎", GUILayout.Width(20)))
            {
                if (displayObj != null)
                {
                    Selection.activeObject = displayObj;
                    EditorGUIUtility.PingObject(displayObj);
                }
            }
        }

        private void AddAssetManually<T>(HashSet<T> set, string selectedPath, Func<string, T> converter)
        {
            if (string.IsNullOrEmpty(selectedPath))
            {
                return;
            }

            string relativePath = FileUtil.GetProjectRelativePath(selectedPath);
            if (!string.IsNullOrEmpty(relativePath))
            {
                T item = converter.Invoke(relativePath);
                if (item != null)
                {
                    set.Add(item);

                    if (hasAnalysisResults)
                        ClearResults();
                }
            }
            else
            {
                EditorUtility.DisplayDialog("Warning", "Only assets from the current Unity project can be added", "OK!");
            }
        }

        private void ClearAllSources()
        {
            atlasFolders.Clear();
            specificAtlases.Clear();
            if (hasAnalysisResults)
                ClearResults();
        }

        private void ClearResults()
        {
            guidToAtlasMap.Clear();
            guidToAssetPathMap.Clear();
            repeatGuids.Clear();
            allAtlases.Clear();
            hasAnalysisResults = false;
        }

        private void AnalyzeAtlasGuids()
        {
            ClearResults();

            HashSet<SpriteAtlas> atlasesToAnalyze = new(specificAtlases.Where(atlas => atlas != null));

            foreach (string folder in atlasFolders)
            {
                if (!Directory.Exists(folder))
                {
                    Debug.LogWarning($"Folder does not exist: {folder}");
                    continue;
                }

                string[] atlasGuids = AssetDatabase.FindAssets("t:SpriteAtlas", new string[] { folder });

                foreach (string guid in atlasGuids)
                {
                    string atlasPath = AssetDatabase.GUIDToAssetPath(guid);
                    SpriteAtlas atlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(atlasPath);
                    if (atlas != null)
                    {
                        atlasesToAnalyze.Add(atlas);
                    }
                }
            }

            if (atlasesToAnalyze.Count == 0)
            {
                EditorUtility.DisplayDialog(nameof(AtlasRepeatChecker), "No valid atlas files found!", "OK!");
                return;
            }

            EditorUtility.DisplayProgressBar("Analyzing Atlas GUID", " Analyzing atlas assets...", 0);

            try
            {
                int i = 0;
                HashSet<string> textureGuids = new();
                foreach (var atlas in atlasesToAnalyze)
                {
                    string atlasPath = AssetDatabase.GetAssetPath(atlas);

                    EditorUtility.DisplayProgressBar("Analyzing Atlas GUID", $"Analyzing: {atlas.name}", i / (float)atlasesToAnalyze.Count);

                    textureGuids.Clear();
                    ExtractGuidsFromAtlasFile(atlasPath, textureGuids);
                    AtlasInfo atlasInfo = new(atlas.name, atlasPath, atlas, new(textureGuids));

                    allAtlases.Add(atlasInfo);

                    foreach (string guid in textureGuids)
                    {
                        if (string.IsNullOrEmpty(guid))
                            continue;

                        if (!guidToAtlasMap.ContainsKey(guid))
                        {
                            guidToAtlasMap[guid] = new List<AtlasInfo>();
                        }

                        guidToAtlasMap[guid].Add(atlasInfo);

                        if (!guidToAssetPathMap.ContainsKey(guid))
                        {
                            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                            if (!string.IsNullOrEmpty(assetPath))
                            {
                                guidToAssetPathMap[guid] = assetPath;
                            }
                        }
                    }
                    i++;
                }

                foreach (var pair in guidToAtlasMap)
                {
                    if (pair.Value.Count > 1)
                    {
                        repeatGuids.Add(pair.Key);
                    }
                }

                hasAnalysisResults = true;
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        private void ExtractGuidsFromAtlasFile(string atlasPath, HashSet<string> guids)
        {
            try
            {
                string fileContent = File.ReadAllText(atlasPath);
                MatchCollection matches = GuidRegex.Matches(fileContent);

                foreach (Match match in matches)
                {
                    if (match.Groups.Count > 1)
                    {
                        string guid = match.Groups[1].Value;
                        string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                        if (!string.IsNullOrEmpty(assetPath))
                        {
                            Type type = AssetDatabase.GetMainAssetTypeAtPath(assetPath);
                            if (type == typeof(Texture2D) || type == typeof(Sprite))
                            {
                                guids.Add(guid);
                            }
                        }
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to load atlas file {atlasPath}: {e.Message}");
            }
        }

        private void DisplayResults()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("Search:", GUILayout.Width(50));
                searchFilter = EditorGUILayout.TextField(searchFilter);
                if (GUILayout.Button("Clear", GUILayout.Width(60)))
                {
                    searchFilter = string.Empty;
                    GUI.FocusControl(null);
                }
            }

            GUILayout.Space(10);

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField($"Atlas: {allAtlases.Count}", EditorStyles.boldLabel, GUILayout.ExpandWidth(true));
                EditorGUILayout.LabelField($"GUID: {guidToAtlasMap.Count}", EditorStyles.boldLabel, GUILayout.ExpandWidth(true));
                EditorGUILayout.LabelField($"Repeat GUID: {repeatGuids.Count}", EditorStyles.boldLabel, GUILayout.ExpandWidth(true));
            }

            GUILayout.Space(10);

            if (repeatGuids.Count == 0)
            {
                EditorGUILayout.HelpBox("No repeat images found! All images are used by only one atlas.", MessageType.Info);
                return;
            }

            EditorGUILayout.LabelField("Repeat Images (used by multiple atlases):", EditorStyles.boldLabel);
            EditorGUILayout.LabelField(string.Empty, GUI.skin.horizontalSlider);

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            List<string> filteredGuids = repeatGuids;
            if (!string.IsNullOrEmpty(searchFilter))
            {
                filteredGuids = repeatGuids.Where(guid =>
                {
                    string assetPath = guidToAssetPathMap.GetValueOrDefault(guid, string.Empty);
                    string assetName = Path.GetFileNameWithoutExtension(assetPath);
                    return assetName.IndexOf(searchFilter, StringComparison.OrdinalIgnoreCase) >= 0 ||
                           assetPath.IndexOf(searchFilter, StringComparison.OrdinalIgnoreCase) >= 0;
                }).ToList();
            }

            foreach (string guid in filteredGuids)
            {
                var rect = EditorGUILayout.BeginVertical("box");
                GUI.color = Color.white;

                string assetPath = guidToAssetPathMap.GetValueOrDefault(guid, "Missing path");
                string assetName = Path.GetFileNameWithoutExtension(assetPath);

                Texture2D texture = string.IsNullOrEmpty(assetPath)
                    ? null :
                    AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);

                using (new EditorGUILayout.HorizontalScope())
                {
                    var previewSize = ToolConstants.PREVIEW_SIZE;
                    if (texture != null)
                    {
                        bool isPress = GUILayout.Button(texture, GUILayout.Width(previewSize), GUILayout.Height(previewSize));
                        if (isPress)
                        {
                            Selection.activeObject = texture;
                            EditorGUIUtility.PingObject(texture);
                        }
                    }
                    else
                    {
                        GUILayout.Box("None", GUILayout.Width(previewSize), GUILayout.Height(previewSize));
                    }

                    using (new EditorGUILayout.VerticalScope())
                    {
                        EditorGUILayout.LabelField($"{assetName}", EditorStyles.boldLabel);

                        EditorGUILayout.LabelField($"Path: {assetPath}", EditorStyles.miniLabel);
                        EditorGUILayout.LabelField($"GUID: {guid}", EditorStyles.miniLabel);

                        EditorGUILayout.LabelField($"Used by the following {guidToAtlasMap[guid].Count} atlases:", EditorStyles.boldLabel);

                        foreach (var atlasInfo in guidToAtlasMap[guid])
                        {
                            EditorGUILayout.LabelField($"• {atlasInfo.Name}", EditorStyles.miniLabel);

                            using (new EditorGUILayout.HorizontalScope())
                            {
                                using (new EditorGUI.DisabledScope(true))
                                {
                                    EditorGUILayout.ObjectField(atlasInfo.Atlas, typeof(SpriteAtlas), true);
                                }
                                SelectObjectButton(atlasInfo.Atlas);
                            }
                        }
                    }
                }

                EditorGUILayout.EndVertical();

                if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
                {
                    if (texture != null)
                    {
                        Selection.activeObject = texture;
                        EditorGUIUtility.PingObject(texture);
                    }
                    Event.current.Use();
                }

                GUILayout.Space(5);
            }

            EditorGUILayout.EndScrollView();

            GUILayout.Space(10);

            if (repeatGuids.Count > 0)
            {
                EditorGUILayout.HelpBox($"Found {repeatGuids.Count} repeat images! It's recommended to check these images.", MessageType.Warning);
            }
        }

        [Serializable]
        private class AtlasInfo
        {
            [field: SerializeField] public string Name { get; }
            [field: SerializeField] public string Path { get; }
            [field: SerializeField] public SpriteAtlas Atlas { get; }
            [field: SerializeField] public List<string> TextureGuids { get; private set; }

            public AtlasInfo(string name, string path, SpriteAtlas atlas, List<string> textureGuids)
            {
                Name = name;
                Path = path;
                Atlas = atlas;
                TextureGuids = textureGuids;
            }
        }
    }
}