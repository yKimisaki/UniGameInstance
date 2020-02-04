using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Tonari.Unity
{
    [Serializable]
    public abstract class GameInstance : IDisposable
    {
        public const string AssetName = "GameModeSettings";
        public const string AssetRelativeDirectory = "Resources";

        private static GameModeSettingsObject settings;

        public static GameInstance Current { get; private set; }

        private ConcurrentDictionary<Scene, GameMode> gameModeCache;

        [RuntimeInitializeOnLoadMethod]
        private static void Launch()
        {
            var resourcesTrimed = AssetRelativeDirectory.StartsWith("Resources") ? AssetRelativeDirectory.Substring("Resources".Length) : AssetRelativeDirectory;
            settings = Resources.Load<GameModeSettingsObject>($"{(string.IsNullOrEmpty(resourcesTrimed) ? "" : resourcesTrimed + "/")}{AssetName}");

            InitializeInstance();
        }

        public static void ResetInstance()
        {
            Current.Dispose();
            InitializeInstance();
        }

        private static void InitializeInstance()
        {
            Current = DeepCopy(settings.GameInstance) as GameInstance;
            Current.Initialize();
        }

        public virtual void Dispose() { }

        public virtual void Initialize()
        {
            SceneManager.sceneLoaded += SceneLoaded;
            SceneManager.sceneUnloaded += SceneUnloaded;

            gameModeCache = new ConcurrentDictionary<Scene, GameMode>();

            SceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
            Application.quitting += () => SceneUnloaded(SceneManager.GetActiveScene());
        }

        private void SceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
        {
            if (loadSceneMode == LoadSceneMode.Single)
            {
                var gameMode = DeepCopy(settings.Settings.FirstOrDefault(x => x.SceneName == scene.name).GameMode) as GameMode;
                foreach (var x in scene.GetRootGameObjects())
                {
                    gameModeCache.TryAdd(scene, gameMode);
                }

                gameMode.Initialize();
            }
            else if (loadSceneMode == LoadSceneMode.Additive)
            {
                gameModeCache.TryAdd(scene, gameModeCache[SceneManager.GetActiveScene()]);
            }
        }

        private void SceneUnloaded(Scene scene)
        {
            if (!scene.isSubScene && gameModeCache.TryRemove(scene, out var gameMode))
            {
                gameMode.Dispose();
            }
        }

        public GameMode GetGameMode(Scene scene)
        {
            return gameModeCache.GetValueOrDefault(scene);
        }

        private static object DeepCopy(object original)
        {
            object result;
            var binaryFormatter = new BinaryFormatter();
            var memoryStream = new MemoryStream();

            try
            {
                binaryFormatter.Serialize(memoryStream, original);
                memoryStream.Position = 0;
                result = binaryFormatter.Deserialize(memoryStream);
            }
            finally
            {
                memoryStream.Close();
            }

            return result;
        }
    }

    public static class DictionaryExtensions
    {
        public static TValue GetValueOrDefault<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> source, TKey key, TValue defaultValue = default) => source.ContainsKey(key) ? source[key] : defaultValue;
    }
}
