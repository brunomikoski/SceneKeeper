using UnityEditor;

namespace BrunoMikoski.SceneHierarchyKeeper
{
    public static class SceneKeeperTools
    {
        private const string ToggleSceneKeeperMenuKey = "Tools/Scene Keeper/Hierarchy/Keep Hierarchy";
        private const string ToggleSelectionKeeperMenuKey = "Tools/Scene Keeper/Selection/Keep Selection";
        private const string IgnorePlaytimeSelectionKeeperMenuKey = "Tools/Scene Keeper/Selection/Ignore Play Time Selection";
        private const string HierarchyKeeperEnabledKey = "HierarchyKeeperEnabled";
        private const string SelectionKeeperEnabledKey = "SelectionKeeperEnabled";
        private const string IgnorePlaytimeSelectionKeeperKey = "IgnorePlayTimeSelection";

        [MenuItem (ToggleSceneKeeperMenuKey)]
        private static void ToggleHierarchyKeeper()
        {
            EditorPrefs.SetBool(HierarchyKeeperEnabledKey, !IsHierarchyKeeperActive());
        }
        
        [MenuItem (ToggleSceneKeeperMenuKey, true)]
        private static bool ToggleHierarchyKeeperValidate()
        {
            bool isActive =  IsHierarchyKeeperActive();
            Menu.SetChecked(ToggleSceneKeeperMenuKey, isActive);
            return true;
        }

        internal static bool IsHierarchyKeeperActive()
        {
            return EditorPrefs.GetBool(HierarchyKeeperEnabledKey, true);
        }
        
        
        [MenuItem (ToggleSelectionKeeperMenuKey)]
        private static void ToggleSelectionKeeper()
        {
            EditorPrefs.SetBool(SelectionKeeperEnabledKey, !IsSelectionKeeperActive());
        }
        
        [MenuItem (ToggleSelectionKeeperMenuKey, true)]
        private static bool ToggleSelectionKeeperValidate()
        {
            bool isActive =  IsSelectionKeeperActive();
            Menu.SetChecked(ToggleSelectionKeeperMenuKey, isActive);
            return true;
        }

        internal static bool IsSelectionKeeperActive()
        {
            return EditorPrefs.GetBool(SelectionKeeperEnabledKey, true);
        }
        
        
        [MenuItem (IgnorePlaytimeSelectionKeeperMenuKey)]
        private static void ToggleIgnorePlaytimeSelectionKeeper()
        {
            EditorPrefs.SetBool(IgnorePlaytimeSelectionKeeperKey, !IsHierarchyKeeperActive());
        }
        
        [MenuItem (IgnorePlaytimeSelectionKeeperMenuKey, true)]
        private static bool ToggleIgnorePlaytimeSelectionKeeperValidate()
        {
            bool isActive =  IsIgnoringPlaytimeSelection();
            Menu.SetChecked(IgnorePlaytimeSelectionKeeperMenuKey, isActive);
            return IsSelectionKeeperActive();
        }

        internal static bool IsIgnoringPlaytimeSelection()
        {
            return EditorPrefs.GetBool(IgnorePlaytimeSelectionKeeperKey, true);
        }
    }
}
