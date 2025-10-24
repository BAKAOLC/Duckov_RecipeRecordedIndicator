﻿using System;
using System.IO;
using System.Reflection;

namespace Duckov_RecipeRecordedIndicator
{
    public static class ModLoader
    {
        private static Assembly? _loadedAssembly;

        public static void Initialize()
        {
            Uninitialize();
            HarmonyLoader.OnReadyToPatch += OnReadyToPatch;
        }

        public static void Uninitialize()
        {
            HarmonyLoader.OnReadyToPatch -= OnReadyToPatch;

            OnModDisabled();
        }

        private static void OnReadyToPatch()
        {
            var path = Path.GetDirectoryName(typeof(ModLoader).Assembly.Location);
            if (path == null)
            {
                ModLogger.LogError("Failed to get assembly directory.");
                return;
            }

            var targetAssemblyFile = Path.Combine(path, Constant.TargetAssemblyName);
            if (!File.Exists(targetAssemblyFile))
            {
                ModLogger.LogError($"Target assembly not found: {targetAssemblyFile}");
                return;
            }

            try
            {
                ModLogger.Log($"Loading Assembly from: {targetAssemblyFile}");

                var bytes = File.ReadAllBytes(targetAssemblyFile);
                var targetAssembly = Assembly.Load(bytes);
                _loadedAssembly = targetAssembly;

                ModLogger.Log("Applying Harmony Patches to Target Assembly...");

                HarmonyLoader.PatchAll(targetAssembly);

                ModLogger.Log("ModLoader finished applying patches.");

                ModLogger.Log("Invoking ModEntry.Initialize...");

                InvokeModEntryMethodInitialize();

                if (ModBehaviour.Instance != null)
                {
                    ModBehaviour.Instance.OnModDisabled -= OnModDisabled;
                    ModBehaviour.Instance.OnModDisabled += OnModDisabled;
                }

                ModLogger.Log("ModLoader initialization complete.");
            }
            catch (Exception ex)
            {
                ModLogger.LogError($"Error loading target assembly or applying patches: {ex}");
            }
        }

        private static void OnModDisabled()
        {
            if (_loadedAssembly == null) return;

            if (ModBehaviour.Instance != null) ModBehaviour.Instance.OnModDisabled -= OnModDisabled;

            ModLogger.Log("Uninitializing Mod...");

            InvokeModEntryMethodUninitialize();
            _loadedAssembly = null;

            ModLogger.Log("Mod uninitialization complete.");
        }

        private static void InvokeModEntryMethodInitialize()
        {
            InvokeModEntryMethod("Initialize");
        }

        private static void InvokeModEntryMethodUninitialize()
        {
            InvokeModEntryMethod("Uninitialize");
        }

        private static void InvokeModEntryMethod(string methodName)
        {
            if (_loadedAssembly == null)
            {
                ModLogger.LogError("Target assembly is not loaded. Cannot invoke ModEntry methods.");
                return;
            }

            var modEntryType = _loadedAssembly.GetType($"{Constant.ModId}.ModEntry");
            if (modEntryType == null)
            {
                ModLogger.LogError("ModEntry type not found in target assembly.");
                return;
            }

            var method = modEntryType.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static);
            if (method == null)
            {
                ModLogger.LogError($"ModEntry.{methodName} method not found.");
                return;
            }

            try
            {
                method.Invoke(null, null);
                ModLogger.Log($"ModEntry.{methodName} invoked successfully.");
            }
            catch (Exception ex)
            {
                ModLogger.LogError($"Error invoking ModEntry.{methodName}: {ex}");
            }
        }
    }
}