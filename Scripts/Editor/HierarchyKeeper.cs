using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace BrunoMikoski.SceneHierarchyKeeper
{
    [InitializeOnLoad]
    public static class HierarchyKeeper
    {
        private const string HierarchyDataStorageKey = "_cachedHierarchyData_storage_key";
        private const string UnityEditorSceneHierarchyWindowTypeName = "UnityEditor.SceneHierarchyWindow";
        private const string ExpandTreeViewItemMethodName = "ExpandTreeViewItem";
        private const string GetExpandedIDsMethodName = "GetExpandedIDs";
        private const string SceneHierarchyPropertyName = "sceneHierarchy";

        private static Type cachedSceneHierarchyWindowType;
        private static Type SceneHierarchyWindowType
        {
            get
            {
                if (cachedSceneHierarchyWindowType == null)
                {
                    //https://github.com/Unity-Technologies/UnityCsReference/blob/master/Editor/Mono/SceneHierarchyWindow.cs
                    cachedSceneHierarchyWindowType = typeof(EditorWindow).Assembly.GetType(UnityEditorSceneHierarchyWindowTypeName);
                }
                return cachedSceneHierarchyWindowType;
            }
        }

        private static EditorWindow cachedHierarchyWindow;
        private static EditorWindow hierarchyWindow
        {
            get
            {
                if (cachedHierarchyWindow == null)
                    cachedHierarchyWindow = EditorWindow.GetWindow(SceneHierarchyWindowType);
                return cachedHierarchyWindow;
            }
        }

        private static HierarchyData cachedHierarchyData;
        private static HierarchyData hierarchyData
        {
            get
            {
                if (cachedHierarchyData == null)
                    cachedHierarchyData = LoadOrCreateData();
                return cachedHierarchyData;
            }
        }
        
        private static Dictionary<Transform, string> sceneItemsCache = new Dictionary<Transform, string>();
        private static Dictionary<string, GameObject> pathToGameObjectsCache = new Dictionary<string, GameObject>();

        static HierarchyKeeper()
        {
            EditorSceneManager.sceneOpened += OnSceneOpened;
            SceneManager.sceneLoaded += OnSceneLoaded;
            EditorSceneManager.sceneClosing += OnSceneClosing;
            SceneManager.sceneUnloaded += OnSceneUnloaded;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange playModeStateChanged)
        {
            if (playModeStateChanged == PlayModeStateChange.ExitingEditMode)
            {
                StoreCurrentSelection();
            }
        }

        private static void StoreCurrentSelection()
        {
            hierarchyData.selectionData.Clear();
            
            for (int i = 0; i < Selection.objects.Length; i++)
            {
                Object selected = Selection.objects[i];
                if (selected is GameObject selectedGameObject)
                {
                    hierarchyData.selectionData.Add(new SelectionData(selectedGameObject.transform.GetPath(),
                        selectedGameObject.scene.path));
                }
            }
            SaveData();
        }

        private static void OnSceneUnloaded(Scene scene)
        {
            StoreExpandedData(scene.path);
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode arg1)
        {
            RestoreExpandedData(scene);
        }

        private static void OnSceneClosing(Scene scene, bool removingscene)
        {
            StoreExpandedData(scene.path);
        }

        private static void OnSceneOpened(Scene scene, OpenSceneMode mode)
        {
            RestoreExpandedData(scene);
        }

        private static void RestoreExpandedData(Scene scene)
        {
            if (hierarchyWindow == null)
                return;

            if (!HierarchyKeeperTools.IsActive())
                return;
            
            if (!hierarchyData.TryGetSceneData(scene.path, out SceneHierarchyData sceneHierarchyData)) 
                return;
            
            object sceneHierarchy = SceneHierarchyWindowType.GetProperty(SceneHierarchyPropertyName).GetValue(hierarchyWindow);
            MethodInfo setExpandedMethod =
                sceneHierarchy.GetType().GetMethod(ExpandTreeViewItemMethodName, BindingFlags.Instance | BindingFlags.NonPublic);
            
            if (setExpandedMethod == null)
            {
                throw new Exception(
                    $"Could not find a method with name {ExpandTreeViewItemMethodName} on type {UnityEditorSceneHierarchyWindowTypeName}, maybe unity changed it? ");
            }
            
            for (int i = 0; i < sceneHierarchyData.itemsPath.Count; i++)
            {
                string expandedItemPath = sceneHierarchyData.itemsPath[i];
            
                if (!TryToFindBySceneRootObjects(scene, expandedItemPath, out GameObject gameObjectAtPath))
                    continue;

                setExpandedMethod.Invoke(sceneHierarchy, new object[] {gameObjectAtPath.GetInstanceID(), true});
            }

            RestoreSelectionData(scene);
        }

        private static void RestoreSelectionData(Scene scene)
        {
            List<Object> selection = new List<Object>();
            for (int i = 0; i < hierarchyData.selectionData.Count; i++)
            {
                SelectionData selectionData = hierarchyData.selectionData[i];
                if (!selectionData.scenePath.Equals(scene.path, StringComparison.OrdinalIgnoreCase))
                    continue;

                if (!TryToFindBySceneRootObjects(scene, selectionData.itemPath, out GameObject resultGameObject))
                    continue;

                selection.Add(resultGameObject);
            }

            if (selection.Count == 0)
                return;

            Selection.objects = selection.ToArray();
        }

        private static bool TryToFindBySceneRootObjects(Scene scene, string targetItemPath, out GameObject resultGameObject)
        {
            resultGameObject = GameObject.Find(targetItemPath);
            if (resultGameObject != null)
                return true;
            
            if (pathToGameObjectsCache.TryGetValue(targetItemPath, out resultGameObject))
            {
                if (resultGameObject != null)
                    return true;
                
                pathToGameObjectsCache.Remove(targetItemPath);
            }
            
            GameObject[] objects = scene.GetRootGameObjects();
            for (int i = 0; i < objects.Length; i++)
            {
                GameObject rootGameObject = objects[i];
                Transform[] allChild = rootGameObject.GetComponentsInChildren<Transform>(true);
                for (int j = 0; j < allChild.Length; j++)
                {
                    Transform transform = allChild[j];
                    if (!sceneItemsCache.TryGetValue(transform, out string itemPath))
                    {
                        itemPath = transform.GetPath();
                        sceneItemsCache.Add(transform, itemPath);
                    }
                    
                    if (itemPath.Equals(targetItemPath, StringComparison.OrdinalIgnoreCase))
                    {
                        resultGameObject = transform.gameObject;
                        pathToGameObjectsCache.Add(targetItemPath, resultGameObject);
                        return true;
                    }
                }
            }

            return false;
        }

        private static void StoreExpandedData(string targetScenePath)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                return;

            if (hierarchyWindow == null)
                return;
            
            if (!HierarchyKeeperTools.IsActive())
                return;
            
            MethodInfo getExpandedIDsMethod = SceneHierarchyWindowType.GetMethod(GetExpandedIDsMethodName, BindingFlags.NonPublic | BindingFlags.Instance);
            if (getExpandedIDsMethod == null)
            {
                throw new Exception(
                    $"Could not find a method with name {GetExpandedIDsMethodName} on type {UnityEditorSceneHierarchyWindowTypeName}, maybe unity changed it? ");
            }
            
            int[] result = (int[]) getExpandedIDsMethod.Invoke(hierarchyWindow, null);

            SceneHierarchyData sceneData = hierarchyData.GetOrAddSceneData(targetScenePath);
            sceneData.itemsPath.Clear();

            for (int i = 0; i < result.Length; i++)
            {
                int instanceID = result[i];
                Object targetObj = EditorUtility.InstanceIDToObject(instanceID);
                if (targetObj == null)
                    continue;

                if (targetObj is GameObject gameObject)
                {
                    string scenePath = gameObject.scene.path;

                    if (!string.Equals(targetScenePath, scenePath, StringComparison.OrdinalIgnoreCase))
                        continue;
                    
                    sceneData.itemsPath.Add(gameObject.transform.GetPath());
                }
            }
            SaveData();
        }
        
        private static string GetPath(this Transform transform)
        {
            if (transform.parent == null)
                return transform.name;
            
            return GetPath(transform.parent) + "/" + transform.name;
        }

        private static void SaveData()
        {
            string json = EditorJsonUtility.ToJson(hierarchyData);
            EditorPrefs.SetString($"{Application.productName}{HierarchyDataStorageKey}", json);
        }
        
        private static HierarchyData LoadOrCreateData()
        {
            HierarchyData instance = new HierarchyData();

            string hierarchyDataJson =
                EditorPrefs.GetString($"{Application.productName}{HierarchyDataStorageKey}", "");
            if (!string.IsNullOrEmpty(hierarchyDataJson))
            {
                instance = JsonUtility.FromJson<HierarchyData>(hierarchyDataJson);
            }

            return instance;
        }
    }
}
