using System;
using System.Collections.Generic;
using UnityEngine;

namespace BrunoMikoski.SceneHierarchyKeeper
{
    [Serializable]
    internal class HierarchyData
    {
        [SerializeField]
        internal List<SceneHierarchyData> scenesHierarchy = new List<SceneHierarchyData>();

        [SerializeField]
        internal List<SelectionData> selectionData = new List<SelectionData>();
        
        public SceneHierarchyData GetOrAddSceneData(string scenePath)
        {
            if (TryGetSceneData(scenePath, out SceneHierarchyData resultData))
                return resultData;

            resultData = new SceneHierarchyData {scenePath = scenePath};
            scenesHierarchy.Add(resultData);
            return resultData;
        }

        public bool TryGetSceneData(string scenePath, out SceneHierarchyData resultSceneHierarchyData)
        {
            for (int i = 0; i < scenesHierarchy.Count; i++)
            {
                SceneHierarchyData sceneHierarchyData = scenesHierarchy[i];
                if (sceneHierarchyData.scenePath.Equals(scenePath, StringComparison.InvariantCulture))
                {
                    resultSceneHierarchyData = sceneHierarchyData;
                    return true;
                }
            }

            resultSceneHierarchyData = null;
            return false;
        }
    }

    [Serializable]
    internal class SelectionData
    {
        [SerializeField]
        internal string scenePath;
        [SerializeField]
        internal string itemPath;

        public SelectionData(string transformPath, string targetScenePath)
        {
            scenePath = targetScenePath;
            itemPath = targetScenePath;
        }
    }
            
    [Serializable]
    internal class SceneHierarchyData
    {
        [SerializeField]
        internal string scenePath;
        [SerializeField]
        internal List<string> itemsPath = new List<string>();
    }
}
