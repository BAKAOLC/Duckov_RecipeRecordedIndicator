using System;
using System.IO;
using System.Reflection;

namespace Duckov_RecipeRecordedIndicator
{
    public static class ModLoader
    {
        public static void Initialize()
        {
            Uninitialize();
            HarmonyLoader.OnReadyToPatch += OnReadyToPatch;
        }

        public static void Uninitialize()
        {
            HarmonyLoader.OnReadyToPatch -= OnReadyToPatch;
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

                ModLogger.Log("Applying Harmony Patches to Target Assembly...");

                HarmonyLoader.PatchAll(targetAssembly);

                ModLogger.Log("ModLoader finished applying patches.");
            }
            catch (Exception ex)
            {
                ModLogger.LogError($"Error loading target assembly or applying patches: {ex}");
            }
        }
    }
}