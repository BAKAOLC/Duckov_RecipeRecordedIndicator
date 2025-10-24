using System;
using System.Collections.Generic;
using Duckov.Modding;
using UnityEngine;

namespace Duckov_RecipeRecordedIndicator
{
    public static class ModConfig
    {
        private static readonly Dictionary<string, ModConfigItem> ConfigValues = [];
        private static bool _registered;

        public static void RegisterConfigOptionsOnce()
        {
            if (_registered) return;

            RegisterConfigValue("ShowIndicatorOnLeft", "Show Indicator On Left Side", false);

            _registered = true;
        }

        public static void Initialize()
        {
            Uninitialize();

            RegisterConfigOptionsOnce();

            if (!ModConfigAPI.Initialize())
            {
                ModLogger.Log("ModConfig not found, waiting for it to load...");
                ModManager.OnModActivated += OnModLoaded;
                return;
            }

            ModConfigAPI.SafeAddOnOptionsChangedDelegate(OnOptionsChanged);
            RegisterConfigOptionsView();
            LoadConfig();

            ModLogger.Log("Config Initialized.");
        }

        public static void Uninitialize()
        {
            ModManager.OnModActivated -= OnModLoaded;

            ModConfigAPI.SafeRemoveOnOptionsChangedDelegate(OnOptionsChanged);
        }

        public static void ResetConfig()
        {
            foreach (var (key, configItem) in ConfigValues)
            {
                configItem.Value = configItem.DefaultValue;
                ModConfigAPI.SafeSave(Constant.ModId, key, configItem.DefaultValue);
            }

            ModLogger.Log("Config Reset to Default Values.");
        }

        public static void RegisterConfigValue<T>(string key, string name, T defaultValue)
        {
            ConfigValues.TryAdd(key, new(name, typeof(T), defaultValue));
        }

        public static bool GetConfigValue<T>(string key, out T? value, T? defaultValue = default)
        {
            if (ConfigValues.TryGetValue(key, out var configItem))
            {
                value = (T?)configItem.Value;
                return true;
            }

            value = defaultValue;
            return false;
        }

        private static void LoadConfig()
        {
            foreach (var (key, configItem) in ConfigValues)
            {
                var value = ModConfigAPI.SafeLoad(Constant.ModId, key, configItem.DefaultValue);
                if (value == null) continue;
                configItem.Value = value;

                ModLogger.Log($"Config Value Loaded: {key} = {value} (Default: {configItem.DefaultValue})");
            }

            ModLogger.Log("Config Loaded.");
        }

        private static void OnOptionsChanged(string optionName)
        {
            optionName = optionName.Replace($"{Constant.ModId}_", "");
            if (!ConfigValues.TryGetValue(optionName, out var configItem)) return;

            var value = ModConfigAPI.SafeLoad(Constant.ModId, optionName, configItem.DefaultValue);
            if (value == null) return;
            configItem.Value = value;

            ModLogger.Log($"Config Value Changed: {optionName} = {value}");
        }

        private static void RegisterConfigOptionsView()
        {
            foreach (var (key, configItem) in ConfigValues)
                switch (configItem.ConfigValueType)
                {
                    case { } t when t == typeof(int):
                        ModConfigAPI.SafeAddInputWithSlider(Constant.ModId, key, configItem.Name,
                            typeof(int), (int)(configItem.Value ?? 0), configItem.SliderRange);
                        break;
                    case { } t when t == typeof(float):
                        ModConfigAPI.SafeAddInputWithSlider(Constant.ModId, key, configItem.Name,
                            typeof(float), (float)(configItem.Value ?? 0f), configItem.SliderRange);
                        break;
                    case { } t when t == typeof(bool):
                        ModConfigAPI.SafeAddBoolDropdownList(Constant.ModId, key, configItem.Name,
                            (bool)(configItem.Value ?? false));
                        break;
                    case { } t when t == typeof(string):
                        ModConfigAPI.SafeAddInputWithSlider(Constant.ModId, key, configItem.Name,
                            typeof(string), (string)(configItem.Value ?? ""));
                        break;
                }
        }

        private static void OnModLoaded(ModInfo modInfo, Duckov.Modding.ModBehaviour modBehaviour)
        {
            if (modInfo.name != "ModConfig") return;

            ModLogger.Log("ModConfig loaded, attempting to initialize config.");
            if (!ModConfigAPI.Initialize()) return;

            ModConfigAPI.SafeAddOnOptionsChangedDelegate(OnOptionsChanged);
            RegisterConfigOptionsView();
            LoadConfig();

            ModManager.OnModActivated -= OnModLoaded;
        }

        private class ModConfigItem(
            string name,
            Type configValueType,
            object? defaultValue,
            object? value = null,
            Vector2? sliderRange = null)
        {
            public string Name { get; } = name;
            public Type ConfigValueType { get; } = configValueType;
            public object? DefaultValue { get; } = value ?? defaultValue;
            public object? Value { get; set; } = defaultValue;
            public Vector2? SliderRange { get; } = sliderRange;
        }
    }
}