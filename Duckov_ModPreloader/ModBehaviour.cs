using System;

namespace Duckov_RecipeRecordedIndicator
{
    public class ModBehaviour : Duckov.Modding.ModBehaviour
    {
        public static ModBehaviour? Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
            ModLogger.Log($"{Constant.ModName} Loaded");
        }

        private void OnEnable()
        {
            ModLoader.Initialize();
            HarmonyLoader.Initialize();
        }

        private void OnDisable()
        {
            OnModDisabled?.Invoke();

            ModLoader.Uninitialize();
            HarmonyLoader.Uninitialize();
        }

        private void OnDestroy()
        {
            ModLoader.Uninitialize();
            HarmonyLoader.Uninitialize();

            Instance = null;
        }

        public event Action? OnModDisabled;
    }
}