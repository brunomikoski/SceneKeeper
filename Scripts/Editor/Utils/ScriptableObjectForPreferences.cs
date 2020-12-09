using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace BrunoMikoski.SceneHierarchyKeeper
{
    /// <summary>
    /// From https://github.com/baba-s/UniScriptableObjectForPreferences/blob/master/Editor/ScriptableObjectForPreferences.cs
    /// </summary>
    public abstract class ScriptableObjectForPreferences<T> : ScriptableObject
        where T : ScriptableObjectForPreferences<T>
    {
        private static T instance;

        private static string ConfigName => typeof(T).Name;

        public static T GetInstance()
        {
            if (instance != null)
                return instance;

            instance = CreateInstance<T>();
            string json = EditorUserSettings.GetConfigValue(ConfigName);
            EditorJsonUtility.FromJsonOverwrite(json, instance);

            if (instance == null)
                instance = CreateInstance<T>();

            return instance;
        }

        public static SettingsProvider CreateSettingsProvider(string settingsProviderPath = null,
            Action<SerializedObject> onGUI = null, Action<SerializedObject> onGUIExtra = null)
        {
            if (settingsProviderPath == null)
            {
                settingsProviderPath = $"Preferences/{typeof(T).Name}";
            }

            T instance = GetInstance();
            SerializedObject serializedObject = new SerializedObject(instance);
            IEnumerable<string> keywords = SettingsProvider.GetSearchKeywordsFromSerializedObject(serializedObject);
            SettingsProvider provider = new SettingsProvider(settingsProviderPath, SettingsScope.User, keywords);

            provider.guiHandler += _ => OnGuiHandler(onGUI, onGUIExtra);

            return provider;
        }

        private static void OnGuiHandler(Action<SerializedObject> onGUI, Action<SerializedObject> onGUIExtra)
        {
            T instance = GetInstance();
            Editor editor = Editor.CreateEditor(instance);

            using (EditorGUI.ChangeCheckScope scope = new EditorGUI.ChangeCheckScope())
            {
                SerializedObject serializedObject = editor.serializedObject;

                serializedObject.Update();

                if (onGUI != null)
                {
                    onGUI(serializedObject);
                }
                else
                {
                    editor.DrawDefaultInspector();
                }

                onGUIExtra?.Invoke(serializedObject);

                if (!scope.changed)
                    return;


                serializedObject.ApplyModifiedProperties();

                string json = EditorJsonUtility.ToJson(editor.target);
                EditorUserSettings.SetConfigValue(ConfigName, json);
            }
        }
    }
}
