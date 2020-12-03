using System;
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

        static HierarchyKeeper()
        {
            EditorSceneManager.sceneOpened += OnSceneOpened;
            SceneManager.sceneLoaded += OnSceneLoaded;
            EditorSceneManager.sceneClosing += OnSceneClosing;
            SceneManager.sceneUnloaded += OnSceneUnloaded;
        }

        private static void OnSceneUnloaded(Scene scene)
        {
            StoreExpandedData(scene.path);
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode arg1)
        {
            RestoreExpandedData(scene.path);
        }

        private static void OnSceneClosing(Scene scene, bool removingscene)
        {
            StoreExpandedData(scene.path);
        }

        private static void OnSceneOpened(Scene scene, OpenSceneMode mode)
        {
            RestoreExpandedData(scene.path);
        }

        private static void RestoreExpandedData(string scenePath)
        {
            if (hierarchyWindow == null)
                return;

            if (!HierarchyKeeperTools.IsActive())
                return;
            
            if (!hierarchyData.TryGetSceneData(scenePath, out SceneHierarchyData sceneHierarchyData)) 
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
                GameObject path = GameObject.Find(expandedItemPath);
                if (path == null)
                    continue;
                
                setExpandedMethod.Invoke(sceneHierarchy, new object[] {path.GetInstanceID(), true});
            }
        }

        private static void StoreExpandedData(string targetScenePath)
        {
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
