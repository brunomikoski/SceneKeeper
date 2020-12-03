using UnityEditor;

namespace BrunoMikoski.SceneHierarchyKeeper
{
    public static class HierarchyKeeperTools
    {
        private const string ToggleMenuKey = "Tools/Scene Hierarchy Keeper/Toggle Scene Hierarchy Keeper";
        private const string HierarchyKeeperEnabledKey = "HierarchyKeeperEnabled";

        [MenuItem (ToggleMenuKey)]
        private static void ToggleHierarchyKeeper()
        {
            EditorPrefs.SetBool(HierarchyKeeperEnabledKey, !IsActive());
        }
        
        [MenuItem (ToggleMenuKey, true)]
        private static bool ToggleHierarchyKeeperValidate()
        {
            bool isActive =  IsActive();
            Menu.SetChecked(ToggleMenuKey, isActive);
            return true;
        }

        internal static bool IsActive()
        {
            return EditorPrefs.GetBool(HierarchyKeeperEnabledKey, true);
        }
    }
}
