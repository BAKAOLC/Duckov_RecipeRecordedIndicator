using System;
using System.Reflection;
using Duckov.Modding;

namespace Duckov_RecipeRecordedIndicator
{
    public static class HarmonyLoader
    {
        private static object? _harmonyInstance;
        private static bool _isInitialized;

        public static event Action? OnReadyToPatch;

        public static void Initialize()
        {
            if (_harmonyInstance != null) return;

            if (!LoadHarmony())
            {
                ModLogger.LogError("Failed to load Harmony.");
                RegisterModActivatedEvents();
                return;
            }

            ModLogger.Log("Harmony Initialized Successfully");
            ModLogger.Log("Triggering ReadyToPatch Event");
            ReadyToPatch();
        }

        public static void Uninitialize()
        {
            if (!UnpatchAll()) ModLogger.LogError("Failed to unpatch Harmony patches during uninitialization.");

            _harmonyInstance = null;
            _isInitialized = false;
        }

        public static bool PatchAll(Assembly assembly)
        {
            if (!_isInitialized)
            {
                ModLogger.LogError("Harmony is not initialized. Cannot apply patches.");
                return false;
            }

            try
            {
                var patchAllMethod = _harmonyInstance!.GetType().GetMethod("PatchAll", [typeof(Assembly)]);
                patchAllMethod!.Invoke(_harmonyInstance, [assembly]);
                ModLogger.Log("Harmony Patches Applied Successfully");
                return true;
            }
            catch (Exception ex)
            {
                ModLogger.LogError($"Error Applying Harmony Patches: {ex}");
            }

            return false;
        }

        private static bool UnpatchAll()
        {
            if (!_isInitialized)
            {
                ModLogger.LogError("Harmony is not initialized. Cannot remove patches.");
                return false;
            }

            if (_harmonyInstance == null)
            {
                ModLogger.LogError("Harmony instance is null. Cannot remove patches.");
                return false;
            }

            try
            {
                var unpatchAllMethod = _harmonyInstance.GetType().GetMethod("UnpatchAll", [typeof(string)]);
                unpatchAllMethod!.Invoke(_harmonyInstance, [Constant.HarmonyId]);
                ModLogger.Log("Harmony Patches Removed Successfully");
                return true;
            }
            catch (Exception ex)
            {
                ModLogger.LogError($"Error Removing Harmony Patches: {ex}");
            }

            return false;
        }

        private static void RegisterModActivatedEvents()
        {
            UnregisterModActivatedEvents();
            ModManager.OnModActivated += OnModActivated;
        }

        private static void UnregisterModActivatedEvents()
        {
            ModManager.OnModActivated -= OnModActivated;
        }

        private static void ReadyToPatch()
        {
            _isInitialized = true;
            OnReadyToPatch?.Invoke();
        }

        private static void OnModActivated(ModInfo modInfo, Duckov.Modding.ModBehaviour modBehaviour)
        {
            ModLogger.Log($"Mod Activated: {modInfo.name}. Attempting to initialize Harmony again.");

            if (!LoadHarmony()) return;

            ModLogger.Log("Harmony Initialized Successfully on Mod Activation");
            UnregisterModActivatedEvents();
            ReadyToPatch();
        }

        private static bool LoadHarmony()
        {
            try
            {
                var harmonyType = Type.GetType("HarmonyLib.Harmony, 0Harmony");
                if (harmonyType == null)
                {
                    ModLogger.LogError("HarmonyLib not found. Please ensure Harmony is installed.");
                    return false;
                }

                _harmonyInstance = Activator.CreateInstance(harmonyType, Constant.HarmonyId);
                return true;
            }
            catch (Exception ex)
            {
                ModLogger.LogError($"Error initializing Harmony: {ex}");
            }

            return false;
        }
    }
}