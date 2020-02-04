using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Tonari.Unity;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Tonari.UnityEditor
{
    public class GameModeSettingsWindow : EditorWindow
    {
        private GameModeSettingsObject settings;

        private Dictionary<string, GameMode> gameModeCache = new Dictionary<string, GameMode>();

        private GameInstance[] allGameInstanceTypes;
        private string[] allGameInstanceNames;
        private GameMode[] allGameModeTypes;
        private string[] allGameModeNames;

        private bool toggleFlag;

        [MenuItem("Window/GameMode/GameMode Settings")]
        private static void Create()
        {
            GetWindow<GameModeSettingsWindow>("GameModeSettings");
        }

        private void OnEnable()
        {
            Refresh();
        }

        private void Refresh()
        {
            var assetName = GameInstance.AssetName + ".asset";
            var assetRelativePath = $"Assets/{GameInstance.AssetRelativeDirectory}/{assetName}";
            var assetDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Assets", GameInstance.AssetRelativeDirectory);
            if (!File.Exists(Path.Combine(assetDirectory, assetName)))
            {
                if (!Directory.Exists(assetDirectory))
                {
                    Directory.CreateDirectory(assetDirectory);
                }

                settings = CreateInstance<GameModeSettingsObject>();
                settings.GameInstance = new DefaultGameInstance();
                settings.Settings = new GameModeSetting[0];

                AssetDatabase.CreateAsset(settings, assetRelativePath);
                Save();
            }
            else
            {
                settings = AssetDatabase.LoadAssetAtPath<GameModeSettingsObject>(assetRelativePath);
            }

            gameModeCache.Clear();
            gameModeCache = settings.Settings.ToDictionary(x => x.SceneName, x => x.GameMode);

            allGameInstanceTypes = Assembly.Load("Assembly-CSharp").GetTypes()
                .Where(x => !x.IsInterface && !x.IsAbstract)
                .Where(x => typeof(GameInstance).IsAssignableFrom(x))
                .Where(x => x != null)
                .Distinct()
                .OrderBy(x => x.Name)
                .Select(x => Activator.CreateInstance(x) as GameInstance)
                .ToArray();
            allGameInstanceNames = allGameInstanceTypes
                .Select(x => x.GetType().Name)
                .ToArray();

            allGameModeTypes = Assembly.Load("Assembly-CSharp").GetTypes()
                .Where(x => !x.IsInterface && !x.IsAbstract)
                .Where(x => typeof(GameMode).IsAssignableFrom(x))
                .Where(x => x != null)
                .Distinct()
                .OrderBy(x => x.Name)
                .Select(x => Activator.CreateInstance(x) as GameMode)
                .ToArray();
            allGameModeNames = allGameModeTypes
                .Select(x => x.GetType().Name)
                .ToArray();
        }

        private void Save()
        {
            settings.Settings = gameModeCache
                .Select(x => new GameModeSetting() { SceneName = x.Key, GameMode = x.Value })
                .ToArray();

            EditorUtility.SetDirty(settings);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private void OnGUI()
        {
            if (GUILayout.Button("Refresh"))
            {
                Refresh();
                return;
            }

            if (settings.GameInstance == null)
            {
                settings.GameInstance = new DefaultGameInstance();
                Save();
            }

            var index = allGameInstanceTypes
                .Select((x, i) => (x, i))
                .FirstOrDefault(x => x.x.GetType() == settings.GameInstance.GetType())
                .i;
            var next = EditorGUILayout.Popup("Game Instance", index, allGameInstanceNames);
            if (index != next)
            {
                settings.GameInstance = allGameInstanceTypes[next];
                Save();
            }

            DrawGameModeSetting(SceneManager.GetActiveScene());
        }

        private void DrawGameModeSetting(Scene scene)
        {
            if (!gameModeCache.ContainsKey(scene.name) || gameModeCache[scene.name] == null)
            {
                gameModeCache[scene.name] = new DefaultGameMode();
                Save();
            }

            var index = allGameModeTypes
                .Select((x, i) => (x, i))
                .FirstOrDefault(x => x.x.GetType() == gameModeCache[scene.name].GetType())
                .i;
            var next = EditorGUILayout.Popup(scene.name, index, allGameModeNames);

            if (index != next)
            {
                gameModeCache[scene.name] = allGameModeTypes[next];
                Save();
            }
        }
    }
}
