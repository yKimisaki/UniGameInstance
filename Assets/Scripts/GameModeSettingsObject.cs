using System;
using UnityEngine;

namespace Tonari.Unity
{

    [Serializable]
    public struct GameModeSetting
    {
        [SerializeField]
        public string SceneName;

        [SerializeReference]
        public GameMode GameMode;
    }

    public class GameModeSettingsObject : ScriptableObject
    {
        [SerializeReference]
        public GameInstance GameInstance;

        [SerializeField]
        public GameModeSetting[] Settings;
    }
}
