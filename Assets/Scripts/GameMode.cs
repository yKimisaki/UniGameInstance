using System;
using UnityEngine;

namespace Tonari.Unity
{
    [Serializable]
    public abstract class GameMode : IDisposable
    {
        public virtual void Initialize() { }
        public virtual void Dispose() { }
    }

    public static class GameObjectExtensions
    {
        public static TGameMode GetGameMode<TGameMode>(this GameObject gameObject) where TGameMode : GameMode
        {
            if (!gameObject.scene.IsValid())
            {
                return null;
            }

            return GameInstance.Current.GetGameMode(gameObject.scene) as TGameMode ?? null;
        }

        public static TGameInstance GetGameInstance<TGameInstance>(this GameObject gameObject) where TGameInstance : GameInstance
        {
            if (!gameObject.scene.IsValid())
            {
                return null;
            }

            return GameInstance.Current as TGameInstance ?? null;
        }
    }
}
