using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Duckov_RecipeRecordedIndicator
{
    /// <summary>
    ///     ModConfig 安全接口封装类 - 提供不抛异常的静态接口
    ///     ModConfig Safe API Wrapper Class - Provides non-throwing static interfaces
    /// </summary>
    public static class ModConfigAPI
    {
        //Ensure this match the number of ModConfig.ModBehaviour.VERSION
        //这里确保版本号与ModConfig.ModBehaviour.VERSION匹配
        private const int ModConfigVersion = 1;

        private static readonly string Tag = $"ModConfig_v{ModConfigVersion}";

        private static Type? _modBehaviourType;
        private static Type? _optionsManagerType;
        public static bool IsInitialized;
        private static bool _versionChecked;
        private static bool _isVersionCompatible;

        /// <summary>
        ///     检查版本兼容性
        ///     Check version compatibility
        /// </summary>
        private static bool CheckVersionCompatibility()
        {
            if (_versionChecked)
                return _isVersionCompatible;

            try
            {
                // 尝试获取 ModConfig 的版本号
                // Try to get ModConfig version number
                var versionField = _modBehaviourType?.GetField("VERSION", BindingFlags.Public | BindingFlags.Static);
                if (versionField != null && versionField.FieldType == typeof(int))
                {
                    var modConfigVersion = (int)versionField.GetValue(null);
                    _isVersionCompatible = modConfigVersion == ModConfigVersion;

                    if (!_isVersionCompatible)
                    {
                        ModLogger.LogError($"[{Tag}] 版本不匹配！API版本: {ModConfigVersion}, ModConfig版本: {modConfigVersion}");
                        return false;
                    }

                    ModLogger.Log($"[{Tag}] 版本检查通过: {ModConfigVersion}");
                    _versionChecked = true;
                    return true;
                }

                // 如果找不到版本字段，发出警告但继续运行（向后兼容）
                // If version field not found, warn but continue (backward compatibility)
                ModLogger.LogWarning($"[{Tag}] 未找到版本信息字段，跳过版本检查");
                _isVersionCompatible = true;
                _versionChecked = true;
                return true;
            }
            catch (Exception ex)
            {
                ModLogger.LogError($"[{Tag}] 版本检查失败: {ex.Message}");
                _isVersionCompatible = false;
                _versionChecked = true;
                return false;
            }
        }

        /// <summary>
        ///     初始化 ModConfigAPI，检查必要的函数是否存在
        ///     Initialize ModConfigAPI, check if necessary functions exist
        /// </summary>
        public static bool Initialize()
        {
            try
            {
                if (IsInitialized)
                    return true;

                // 获取 ModBehaviour 类型
                // Get ModBehaviour type
                _modBehaviourType = FindTypeInAssemblies("ModConfig.ModBehaviour");
                if (_modBehaviourType == null)
                {
                    ModLogger.LogWarning($"[{Tag}] ModConfig.ModBehaviour 类型未找到，ModConfig 可能未加载");
                    return false;
                }

                // 获取 OptionsManager_Mod 类型
                // Get OptionsManager_Mod type
                _optionsManagerType = FindTypeInAssemblies("ModConfig.OptionsManager_Mod");
                if (_optionsManagerType == null)
                {
                    ModLogger.LogWarning($"[{Tag}] ModConfig.OptionsManager_Mod 类型未找到");
                    return false;
                }

                // 检查版本兼容性
                // Check version compatibility
                if (!CheckVersionCompatibility())
                {
                    ModLogger.LogWarning($"[{Tag}] ModConfig version mismatch!!!");
                    return false;
                }

                // 检查必要的静态方法是否存在
                // Check if necessary static methods exist
                string[] requiredMethods =
                [
                    "AddDropdownList",
                    "AddInputWithSlider",
                    "AddBoolDropdownList",
                    "AddOnOptionsChangedDelegate",
                    "RemoveOnOptionsChangedDelegate",
                ];

                foreach (var methodName in requiredMethods)
                {
                    var method = _modBehaviourType.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static);
                    if (method != null) continue;
                    ModLogger.LogError($"[{Tag}] 必要方法 {methodName} 未找到");
                    return false;
                }

                IsInitialized = true;
                ModLogger.Log($"[{Tag}] ModConfigAPI 初始化成功");
                return true;
            }
            catch (Exception ex)
            {
                ModLogger.LogError($"[{Tag}] 初始化失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        ///     在所有已加载的程序集中查找类型
        /// </summary>
        private static Type? FindTypeInAssemblies(string typeName)
        {
            try
            {
                // 获取当前域中的所有程序集
                Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

                foreach (var assembly in assemblies)
                    try
                    {
                        // 检查程序集名称是否包含 ModConfig
                        if (assembly.FullName.Contains("ModConfig"))
                            ModLogger.Log($"[{Tag}] 找到 ModConfig 相关程序集: {assembly.FullName}");

                        // 尝试在该程序集中查找类型
                        var type = assembly.GetType(typeName);
                        if (type == null) continue;
                        ModLogger.Log($"[{Tag}] 在程序集 {assembly.FullName} 中找到类型 {typeName}");
                        return type;
                    }
                    catch (Exception)
                    {
                        // 忽略单个程序集的查找错误
                    }

                // 记录所有已加载的程序集用于调试
                ModLogger.LogWarning($"[{Tag}] 在所有程序集中未找到类型 {typeName}，已加载程序集数量: {assemblies.Length}");
                foreach (var assembly in assemblies.Where(a => a.FullName.Contains("ModConfig")))
                    ModLogger.Log($"[{Tag}] ModConfig 相关程序集: {assembly.FullName}");

                return null;
            }
            catch (Exception ex)
            {
                ModLogger.LogError($"[{Tag}] 程序集扫描失败: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        ///     安全地添加选项变更事件委托
        ///     Safely add options changed event delegate
        /// </summary>
        /// <param name="action">事件处理委托，参数为变更的选项键名</param>
        /// <returns>是否成功添加</returns>
        public static bool SafeAddOnOptionsChangedDelegate(Action<string>? action)
        {
            if (!Initialize())
                return false;

            if (action == null)
            {
                ModLogger.LogWarning($"[{Tag}] 不能添加空的事件委托");
                return false;
            }

            try
            {
                var method = _modBehaviourType?.GetMethod("AddOnOptionsChangedDelegate",
                    BindingFlags.Public | BindingFlags.Static);
                method?.Invoke(null, [action]);

                ModLogger.Log($"[{Tag}] 成功添加选项变更事件委托");
                return true;
            }
            catch (Exception ex)
            {
                ModLogger.LogError($"[{Tag}] 添加选项变更事件委托失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        ///     安全地移除选项变更事件委托
        ///     Safely remove options changed event delegate
        /// </summary>
        /// <param name="action">要移除的事件处理委托</param>
        /// <returns>是否成功移除</returns>
        public static bool SafeRemoveOnOptionsChangedDelegate(Action<string>? action)
        {
            if (!Initialize())
                return false;

            if (action == null)
            {
                ModLogger.LogWarning($"[{Tag}] 不能移除空的事件委托");
                return false;
            }

            try
            {
                var method = _modBehaviourType?.GetMethod("RemoveOnOptionsChangedDelegate",
                    BindingFlags.Public | BindingFlags.Static);
                method?.Invoke(null, [action]);

                ModLogger.Log($"[{Tag}] 成功移除选项变更事件委托");
                return true;
            }
            catch (Exception ex)
            {
                ModLogger.LogError($"[{Tag}] 移除选项变更事件委托失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        ///     安全地添加下拉列表配置项
        ///     Safely add dropdown list configuration item
        /// </summary>
        public static bool SafeAddDropdownList(string modName, string key, string description,
            SortedDictionary<string, object> options, Type valueType, object defaultValue)
        {
            key = $"{modName}_{key}";

            if (!Initialize())
                return false;

            try
            {
                var method = _modBehaviourType?.GetMethod("AddDropdownList", BindingFlags.Public | BindingFlags.Static);
                method?.Invoke(null, [modName, key, description, options, valueType, defaultValue]);

                ModLogger.Log($"[{Tag}] 成功添加下拉列表: {modName}.{key}");
                return true;
            }
            catch (Exception ex)
            {
                ModLogger.LogError($"[{Tag}] 添加下拉列表失败 {modName}.{key}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        ///     安全地添加带滑条的输入框配置项
        ///     Safely add input box with slider configuration item
        /// </summary>
        public static bool SafeAddInputWithSlider(string modName, string key, string description, Type valueType,
            object defaultValue, Vector2? sliderRange = null)
        {
            key = $"{modName}_{key}";

            if (!Initialize())
                return false;

            try
            {
                var method =
                    _modBehaviourType?.GetMethod("AddInputWithSlider", BindingFlags.Public | BindingFlags.Static);

                // 处理可空参数
                // Handle nullable parameters
                var parameters = sliderRange.HasValue
                    ? new[] { modName, key, description, valueType, defaultValue, sliderRange.Value }
                    : new[] { modName, key, description, valueType, defaultValue, null };

                method?.Invoke(null, parameters);

                ModLogger.Log($"[{Tag}] 成功添加滑条输入框: {modName}.{key}");
                return true;
            }
            catch (Exception ex)
            {
                ModLogger.LogError($"[{Tag}] 添加滑条输入框失败 {modName}.{key}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        ///     安全地添加布尔下拉列表配置项
        ///     Safely add boolean dropdown list configuration item
        /// </summary>
        public static bool SafeAddBoolDropdownList(string modName, string key, string description, bool defaultValue)
        {
            key = $"{modName}_{key}";

            if (!Initialize())
                return false;

            try
            {
                var method =
                    _modBehaviourType?.GetMethod("AddBoolDropdownList", BindingFlags.Public | BindingFlags.Static);
                method?.Invoke(null, [modName, key, description, defaultValue]);

                ModLogger.Log($"[{Tag}] 成功添加布尔下拉列表: {modName}.{key}");
                return true;
            }
            catch (Exception ex)
            {
                ModLogger.LogError($"[{Tag}] 添加布尔下拉列表失败 {modName}.{key}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        ///     安全地加载配置值
        ///     Safely load configuration value
        /// </summary>
        /// <typeparam name="T">值的类型</typeparam>
        /// <param name="modName"></param>
        /// <param name="key">配置键</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns>加载的值或默认值</returns>
        public static T? SafeLoad<T>(string modName, string key, T? defaultValue = default)
        {
            key = $"{modName}_{key}";

            if (!Initialize())
                return defaultValue;

            if (string.IsNullOrEmpty(key))
            {
                ModLogger.LogWarning($"[{Tag}] 配置键不能为空");
                return defaultValue;
            }

            try
            {
                var loadMethod = _optionsManagerType?.GetMethod("Load", BindingFlags.Public | BindingFlags.Static);
                if (loadMethod == null)
                {
                    ModLogger.LogError($"[{Tag}] 未找到 OptionsManager_Mod.Load 方法");
                    return defaultValue;
                }

                // 获取泛型方法
                var genericLoadMethod = loadMethod.MakeGenericMethod(typeof(T));
                var result = genericLoadMethod.Invoke(null, [key, defaultValue]);

                ModLogger.Log($"[{Tag}] 成功加载配置: {key} = {result}");
                return (T)result;
            }
            catch (Exception ex)
            {
                ModLogger.LogError($"[{Tag}] 加载配置失败 {key}: {ex.Message}");
                return defaultValue;
            }
        }

        /// <summary>
        ///     安全地保存配置值
        ///     Safely save configuration value
        /// </summary>
        /// <typeparam name="T">值的类型</typeparam>
        /// <param name="modName"></param>
        /// <param name="key">配置键</param>
        /// <param name="value">要保存的值</param>
        /// <returns>是否保存成功</returns>
        public static bool SafeSave<T>(string modName, string key, T value)
        {
            key = $"{modName}_{key}";

            if (!Initialize())
                return false;

            if (string.IsNullOrEmpty(key))
            {
                ModLogger.LogWarning($"[{Tag}] 配置键不能为空");
                return false;
            }

            try
            {
                var saveMethod = _optionsManagerType?.GetMethod("Save", BindingFlags.Public | BindingFlags.Static);
                if (saveMethod == null)
                {
                    ModLogger.LogError($"[{Tag}] 未找到 OptionsManager_Mod.Save 方法");
                    return false;
                }

                // 获取泛型方法
                var genericSaveMethod = saveMethod.MakeGenericMethod(typeof(T));
                genericSaveMethod.Invoke(null, [key, value]);

                ModLogger.Log($"[{Tag}] 成功保存配置: {key} = {value}");
                return true;
            }
            catch (Exception ex)
            {
                ModLogger.LogError($"[{Tag}] 保存配置失败 {key}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        ///     检查 ModConfig 是否可用
        ///     Check if ModConfig is available
        /// </summary>
        public static bool IsAvailable()
        {
            return Initialize();
        }

        /// <summary>
        ///     获取 ModConfig 版本信息（如果存在）
        ///     Get ModConfig version information (if exists)
        /// </summary>
        public static string GetVersionInfo()
        {
            if (!Initialize())
                return "ModConfig 未加载 | ModConfig not loaded";

            try
            {
                // 尝试获取版本信息（如果 ModBehaviour 有相关字段或属性）
                // Try to get version information (if ModBehaviour has related fields or properties)
                var versionField = _modBehaviourType?.GetField("VERSION", BindingFlags.Public | BindingFlags.Static);
                if (versionField != null && versionField.FieldType == typeof(int))
                {
                    var modConfigVersion = (int)versionField.GetValue(null);
                    var compatibility = modConfigVersion == ModConfigVersion ? "兼容" : "不兼容";
                    return $"ModConfig v{modConfigVersion} (API v{ModConfigVersion}, {compatibility})";
                }

                var versionProperty =
                    _modBehaviourType?.GetProperty("VERSION", BindingFlags.Public | BindingFlags.Static);
                if (versionProperty == null)
                    return "ModConfig 已加载（版本信息不可用） | ModConfig loaded (version info unavailable)";
                var versionValue = versionProperty.GetValue(null);
                return versionValue?.ToString() ?? "未知版本 | Unknown version";
            }
            catch
            {
                return "ModConfig 已加载（版本检查失败） | ModConfig loaded (version check failed)";
            }
        }

        /// <summary>
        ///     检查版本兼容性
        ///     Check version compatibility
        /// </summary>
        public static bool IsVersionCompatible()
        {
            return Initialize() && _isVersionCompatible;
        }
    }
}