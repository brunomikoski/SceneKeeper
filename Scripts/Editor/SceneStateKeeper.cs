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
    public static class SceneStateKeeper
    {
        private const string HierarchyDataStorageKey = "_cachedHierarchyData_storage_key";
        private const string UnityEditorSceneHierarchyWindowTypeName = "UnityEditor.SceneHierarchyWindow";
        private const string ExpandTreeViewItemMethodName = "ExpandTreeViewItem";
        private const string GetExpandedIDsMethodName = "GetExpandedIDs";
        private const string SceneHierarchyPropertyName = "sceneHierarchy";
        private const int CHILD_LIST_CAPACITY = 512;

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
        private static EditorWindow HierarchyWindow
        {
            get
            {
                if (cachedHierarchyWindow == null)
                    cachedHierarchyWindow = EditorWindow.GetWindow(SceneHierarchyWindowType);
                return cachedHierarchyWindow;
            }
        }

        private static SceneData cachedSceneData;
        private static SceneData SceneData
        {
            get
            {
                if (cachedSceneData == null)
                    cachedSceneData = LoadOrCreateData();
                return cachedSceneData;
            }
        }
        
        private static Dictionary<Transform, string> sceneItemsCache = new Dictionary<Transform, string>();
        private static Dictionary<string, GameObject> pathToGameObjectsCache = new Dictionary<string, GameObject>();
        private static List<Transform> childListTransform = new List<Transform>(CHILD_LIST_CAPACITY);


        private static Dictionary<Scene, List<GameObject>> selectionHistory = new Dictionary<Scene, List<GameObject>>();
        private static HashSet<Scene> clearedSceneChangeLists = new HashSet<Scene>();

        static SceneStateKeeper()
        {
            EditorSceneManager.sceneOpened += OnSceneOpened;
            SceneManager.sceneLoaded += OnSceneLoaded;
            EditorSceneManager.sceneClosing += OnSceneClosing;
            SceneManager.sceneUnloaded += OnSceneUnloaded;
            Selection.selectionChanged += OnSelectionChanged;
        }

        private static void OnSelectionChanged()
        {
            if (!SceneKeeperTools.IsSelectionKeeperActive())
                return;
            
            if (HierarchyWindow == null)
                return;

            if (Selection.objects.Length == 0)
            {
                if (EditorWindow.focusedWindow == HierarchyWindow)
                {
                    for (int i = 0; i < SceneManager.sceneCount; i++)
                    {
                        Scene scene = SceneManager.GetSceneAt(i);
                        if (!selectionHistory.ContainsKey(scene))
                            continue;
                        
                        selectionHistory[scene].Clear();
                    }
                }
                return;
            }
            
            clearedSceneChangeLists.Clear();
            for (int i = 0; i < Selection.objects.Length; i++)
            {
                Object selectedObject = Selection.objects[i];
                if (selectedObject is GameObject selectedGameObject)
                {
                    Scene selectedGameObjectScene = selectedGameObject.scene;

                    if (!selectionHistory.ContainsKey(selectedGameObjectScene))
                    {
                        selectionHistory.Add(selectedGameObjectScene, new List<GameObject>());
                    }
                    else
                    {
                        if (!clearedSceneChangeLists.Contains(selectedGameObjectScene))
                        {
                            selectionHistory[selectedGameObjectScene].Clear();
                            clearedSceneChangeLists.Add(selectedGameObjectScene);
                        }
                    }

                    selectionHistory[selectedGameObjectScene].Add(selectedGameObject);
                }
            }
        }

        private static void OnSceneUnloaded(Scene scene)
        {
            StoreScenedData(scene);
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode arg1)
        {
            RestoreSceneData(scene);
        }

        private static void OnSceneClosing(Scene scene, bool removingscene)
        {
            StoreScenedData(scene);
        }

        private static void OnSceneOpened(Scene scene, OpenSceneMode mode)
        {
            RestoreSceneData(scene);
        }

        private static void RestoreSceneData(Scene scene)
        {
            if (HierarchyWindow == null)
                return;

            HashSet<string> alreadySelectedGameObjectPaths = new HashSet<string>();

            RestoreSelectionData(scene, ref alreadySelectedGameObjectPaths);
            RestoreHierarchyData(scene, ref alreadySelectedGameObjectPaths);
        }

        private static void RestoreHierarchyData(Scene scene, ref HashSet<string> alreadySelectedGameObjectPaths)
        {
            if (!SceneKeeperTools.IsHierarchyKeeperActive()) 
                return;

            if (!SceneData.TryGetSceneData(scene.path, out HierarchyData sceneHierarchyData)) 
                return;
            
            object sceneHierarchy = SceneHierarchyWindowType.GetProperty(SceneHierarchyPropertyName)
                .GetValue(HierarchyWindow);
            MethodInfo setExpandedMethod =
                sceneHierarchy.GetType().GetMethod(ExpandTreeViewItemMethodName,
                    BindingFlags.Instance | BindingFlags.NonPublic);

            if (setExpandedMethod == null)
            {
                throw new Exception(
                    $"Could not find a method with name {ExpandTreeViewItemMethodName} on type {UnityEditorSceneHierarchyWindowTypeName}, maybe unity changed it? ");
            }

            for (int i = 0; i < sceneHierarchyData.itemsPath.Count; i++)
            {
                string expandedItemPath = sceneHierarchyData.itemsPath[i];

                if (alreadySelectedGameObjectPaths.Contains(expandedItemPath))
                    continue;

                if (!TryToFindBySceneRootObjects(scene, expandedItemPath, out GameObject gameObjectAtPath))
                    continue;

                setExpandedMethod.Invoke(sceneHierarchy, new object[] {gameObjectAtPath.GetInstanceID(), true});
            }
        }

        private static void RestoreSelectionData(Scene scene, ref HashSet<string> alreadySelectedGameObjectPaths)
        {
            if (!SceneKeeperTools.IsSelectionKeeperActive()) 
                return;
            
            if (!SceneData.TryGetSceneSelectionData(scene.path, out SelectionData resultSelectionData)) 
                return;
            
            List<Object> selectedObjects = new List<Object>();
            for (int i = 0; i < resultSelectionData.itemPath.Count; i++)
            {
                string targetItemPath = resultSelectionData.itemPath[i];
                if (TryToFindBySceneRootObjects(scene, targetItemPath, out GameObject targetGameObject))
                {
                    selectedObjects.Add(targetGameObject);
                    alreadySelectedGameObjectPaths.Add(targetItemPath);
                }
            }

            if (selectedObjects.Count > 0)
                Selection.objects = selectedObjects.ToArray();
        }

        private static void StoreScenedData(Scene targetScene)
        {
            if (HierarchyWindow == null)
                return;

            HashSet<GameObject> alreadySelectedGameObjects = new HashSet<GameObject>();
            StoreSelectionData(targetScene, ref alreadySelectedGameObjects);
            StoreHierarchyData(targetScene, ref alreadySelectedGameObjects);
            
            SaveData();
        }

        private static void StoreHierarchyData(Scene targetScene, ref HashSet<GameObject> alreadySelectedGameObjects)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) 
                return;

            if (!SceneKeeperTools.IsHierarchyKeeperActive()) 
                return;
            
            MethodInfo getExpandedIDsMethod = SceneHierarchyWindowType.GetMethod(GetExpandedIDsMethodName,
                BindingFlags.NonPublic | BindingFlags.Instance);
            if (getExpandedIDsMethod == null)
            {
                throw new Exception(
                    $"Could not find a method with name {GetExpandedIDsMethodName} on type {UnityEditorSceneHierarchyWindowTypeName}, maybe unity changed it? ");
            }

            int[] result = (int[]) getExpandedIDsMethod.Invoke(HierarchyWindow, null);

            HierarchyData data = SceneData.GetOrAddSceneData(targetScene.path);
            data.itemsPath.Clear();

            for (int i = 0; i < result.Length; i++)
            {
                int instanceID = result[i];
                Object targetObj = EditorUtility.InstanceIDToObject(instanceID);
                if (targetObj == null)
                    continue;

                if (targetObj is GameObject gameObject)
                {
                    if (alreadySelectedGameObjects.Contains(gameObject))
                        continue;

                    string scenePath = gameObject.scene.path;

                    if (!string.Equals(targetScene.path, scenePath, StringComparison.OrdinalIgnoreCase))
                        continue;

                    data.itemsPath.Add(gameObject.transform.GetPath());
                }
            }
        }

        private static void StoreSelectionData(Scene targetScene, ref HashSet<GameObject> alreadySelectedGameObjects)
        {
            if (!SceneKeeperTools.IsSelectionKeeperActive()) 
                return;

            if (Application.isPlaying && SceneKeeperTools.IsIgnoringPlaytimeSelection())
                return;

            if (!TryGetLastValidSelectionForScene(targetScene, out List<GameObject> selectedGameObjects)) 
                return;
            
            SelectionData sceneSelectionData = SceneData.GetOrAddSceneSelectionData(targetScene.path);
            sceneSelectionData.itemPath.Clear();

            for (int i = 0; i < selectedGameObjects.Count; i++)
            {
                GameObject selectedGameObject = selectedGameObjects[i];
                if (selectedGameObject == null)
                    continue;

                sceneSelectionData.itemPath.Add(selectedGameObject.transform.GetPath());

                alreadySelectedGameObjects.Add(selectedGameObject);
            }
        }

        private static bool TryGetLastValidSelectionForScene(Scene targetScene, out List<GameObject> resultGameObjects)
        {
            return selectionHistory.TryGetValue(targetScene, out resultGameObjects);
        }

        private static bool TryToFindBySceneRootObjects(Scene scene, string targetItemPath, out GameObject resultGameObject)
        {
            if (pathToGameObjectsCache.TryGetValue(targetItemPath, out resultGameObject))
            {
                if (resultGameObject != null)
                    return true;
                
                pathToGameObjectsCache.Remove(targetItemPath);
            }
            
            resultGameObject = GameObject.Find(targetItemPath);
            if (resultGameObject != null)
            {
                pathToGameObjectsCache.Add(targetItemPath, resultGameObject);
                return true;
            }
            
            GameObject[] objects = scene.GetRootGameObjects();
            for (int i = 0; i < objects.Length; i++)
            {
                GameObject rootGameObject = objects[i];

                rootGameObject.GetComponentsInChildren(true, childListTransform);
                for (int j = 0; j < childListTransform.Count; j++)
                {
                    Transform transform = childListTransform[j];
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
        
        private static string GetPath(this Transform transform)
        {
            if (transform.parent == null)
                return transform.name;
            
            return GetPath(transform.parent) + "/" + transform.name;
        }

        private static void SaveData()
        {
            string json = EditorJsonUtility.ToJson(SceneData);
            EditorPrefs.SetString($"{Application.productName}{HierarchyDataStorageKey}", json);
        }
        
        private static SceneData LoadOrCreateData()
        {
            SceneData instance = new SceneData();

            string hierarchyDataJson =
                EditorPrefs.GetString($"{Application.productName}{HierarchyDataStorageKey}", "");
            if (!string.IsNullOrEmpty(hierarchyDataJson))
            {
                instance = JsonUtility.FromJson<SceneData>(hierarchyDataJson);
            }

            return instance;
        }
    } 
}
