using UnityEditor;
using UnityEngine;

namespace BrunoMikoski.SceneHierarchyKeeper
{
    public class SceneKeeperSettings : ScriptableObjectForPreferences<SceneKeeperSettings>
    {
        [SerializeField]
        private bool keepHierarchy = true;
        public bool KeepHierarchy => keepHierarchy;
        [SerializeField]
        private bool keepSelection = true;
        public bool KeepSelection => keepSelection;
        [SerializeField]
        private bool ignoreHierarchyAtRuntime = false;
        public bool IgnoreHierarchyAtRuntime => ignoreHierarchyAtRuntime;
        [SerializeField]
        private bool ignoreRuntimeSelection = true;
        public bool IgnoreRuntimeSelection => ignoreRuntimeSelection;

        
        [SettingsProvider]
        private static SettingsProvider SettingsProvider()
        {
            return CreateSettingsProvider
            (
                "Scene Keeper",
                OnSceneKeeperSettings
            );
        }

        private static void OnSceneKeeperSettings(SerializedObject obj)
        {
            using (EditorGUI.ChangeCheckScope scope = new EditorGUI.ChangeCheckScope())
            {
                EditorGUILayout.BeginHorizontal();
                SerializedProperty keepHierarchy = obj.FindProperty("keepHierarchy");
                EditorGUILayout.PropertyField(keepHierarchy);
                EditorGUILayout.EndHorizontal();

                EditorGUI.indentLevel++;
                if (keepHierarchy.boolValue)
                {
                    SerializedProperty keepHierarchyAtRuntime = obj.FindProperty("ignoreHierarchyAtRuntime");
                    EditorGUILayout.PropertyField(keepHierarchyAtRuntime,
                        new GUIContent("Ignore Runtime", "Hierarchy changes made in Play Mode will not be stored"));
                }

                EditorGUI.indentLevel--;

                SerializedProperty keepSelection = obj.FindProperty("keepSelection");
                EditorGUILayout.PropertyField(keepSelection);
                if (keepSelection.boolValue)
                {
                    EditorGUI.indentLevel++;
                    SerializedProperty keepSelectionAtRuntime = obj.FindProperty("ignoreRuntimeSelection");
                    EditorGUILayout.PropertyField(keepSelectionAtRuntime,
                        new GUIContent("Ignore Runtime", "Selection changes made in Play Mode will not be stored"));
                    EditorGUI.indentLevel--;
                }

                if (scope.changed)
                    obj.ApplyModifiedProperties();
            }

            using (new EditorGUI.DisabledScope(!SceneStateKeeper.HasData()))
            {
                if (GUILayout.Button("Clear Local Cache", EditorStyles.miniButton))
                {
                    SceneStateKeeper.ClearData();
                }
            }
        }
    }
}
