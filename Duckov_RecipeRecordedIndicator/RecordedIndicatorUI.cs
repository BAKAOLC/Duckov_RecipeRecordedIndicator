using System;
using System.Collections.Generic;
using Duckov.UI;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Duckov_RecipeRecordedIndicator
{
    public static class RecordedIndicatorUI
    {
        public const string IndicatorObjectName = "CheckedRecordedIndicator";
        public const string OtherIndicatorObjectName = "RecordedIndicator";

        public static readonly Vector2 IndicatorAnchorPosition = new(-5f, -5f);
        public static readonly Vector2 IndicatorSize = new(28f, 28f);
        public static readonly Color RecordedIndicatorBgColor = new(0.2f, 0.8f, 0.2f, 1f);
        public static readonly Color RecordedIndicatorTextColor = new(1f, 1f, 1f, 1f);

        private static readonly HashSet<ItemDisplay> IndicatedDisplays = [];

        public static void AddIndicator(ItemDisplay itemDisplay)
        {
            if (itemDisplay == null || IndicatedDisplays.Contains(itemDisplay)) return;

            try
            {
                if (itemDisplay.transform.Find(OtherIndicatorObjectName) != null)
                {
                    ModLogger.LogWarning(
                        $"ItemDisplay {itemDisplay.name} has another Recorded Indicator present. Skipping addition of new indicator.");
                    return;
                }

                if (itemDisplay.transform.Find(IndicatorObjectName) != null)
                {
                    IndicatedDisplays.Add(itemDisplay);
                    return;
                }

                var success = CreateIndicator(itemDisplay);
                if (!success)
                {
                    ModLogger.LogError($"Failed to create Recorded Indicator for ItemDisplay: {itemDisplay.name}");
                    return;
                }

                IndicatedDisplays.Add(itemDisplay);
                ModLogger.Log($"Added Recorded Indicator to ItemDisplay: {itemDisplay.name}");
            }
            catch (Exception ex)
            {
                ModLogger.LogError($"Error Adding Recorded Indicator to ItemDisplay {itemDisplay.name}: {ex}");
            }
        }

        public static void RemoveIndicator(ItemDisplay itemDisplay)
        {
            if (itemDisplay == null) return;

            try
            {
                var transform = itemDisplay.transform.Find(IndicatorObjectName);
                if (transform != null)
                {
                    Object.Destroy(transform.gameObject);
                    ModLogger.Log($"Removed Recorded Indicator from ItemDisplay: {itemDisplay.name}");
                }

                IndicatedDisplays.Remove(itemDisplay);
            }
            catch (Exception ex)
            {
                ModLogger.LogError($"Error Removing Recorded Indicator from ItemDisplay {itemDisplay.name}: {ex}");
            }
        }

        private static bool CreateIndicator(ItemDisplay itemDisplay)
        {
            try
            {
                if (itemDisplay == null)
                {
                    ModLogger.LogError("ItemDisplay is null");
                    return false;
                }

                var indicatorObject = CreateIndicator_Main(itemDisplay);
                if (indicatorObject == null)
                {
                    ModLogger.LogError("Failed to create main indicator object");
                    return false;
                }

                var backgroundObject = CreateIndicator_Background(indicatorObject);
                if (backgroundObject == null)
                {
                    ModLogger.LogError("Failed to create background object");
                    Object.Destroy(indicatorObject);
                    return false;
                }

                var textObject = CreateIndicator_Text(indicatorObject);
                if (textObject == null)
                {
                    ModLogger.LogError("Failed to create text object");
                    Object.Destroy(indicatorObject);
                    return false;
                }

                ModLogger.Log("Successfully Created Recorded Indicator UI");
                return true;
            }
            catch (Exception ex)
            {
                ModLogger.LogError($"Error Creating Recorded Indicator UI: {ex}");
            }

            return false;
        }

        private static GameObject? CreateIndicator_Main(ItemDisplay itemDisplay)
        {
            var indicatorObject = new GameObject(IndicatorObjectName);
            indicatorObject.transform.SetParent(itemDisplay.transform, false);
            indicatorObject.transform.localScale = Vector3.one;
            var rectTransform = indicatorObject.AddComponent<RectTransform>();
            if (rectTransform == null)
            {
                ModLogger.LogError("Failed to add RectTransform to Indicator");
                Object.Destroy(indicatorObject);
                return null;
            }

            rectTransform.anchorMin = new(1f, 1f);
            rectTransform.anchorMax = new(1f, 1f);
            rectTransform.pivot = new(1f, 1f);
            rectTransform.anchoredPosition = IndicatorAnchorPosition;
            rectTransform.sizeDelta = IndicatorSize;

            return indicatorObject;
        }

        private static GameObject? CreateIndicator_Background(GameObject indicatorObject)
        {
            var backgroundObject = new GameObject("Background");
            backgroundObject.transform.SetParent(indicatorObject.transform, false);
            backgroundObject.transform.localScale = Vector3.one;

            var rectTransform = backgroundObject.AddComponent<RectTransform>();
            if (rectTransform == null)
            {
                ModLogger.LogError("Failed to add RectTransform to Background");
                Object.Destroy(backgroundObject);
                return null;
            }

            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;

            var textMesh = backgroundObject.AddComponent<TextMeshProUGUI>();
            if (textMesh == null)
            {
                ModLogger.LogError("Failed to add TextMeshProUGUI to Background");
                Object.Destroy(backgroundObject);
                return null;
            }

            textMesh.text = "●";
            textMesh.color = RecordedIndicatorBgColor;
            textMesh.fontSize = 32f;
            textMesh.alignment = TextAlignmentOptions.Center;

            return backgroundObject;
        }

        private static GameObject? CreateIndicator_Text(GameObject indicatorObject)
        {
            var textObject = new GameObject("Text");
            textObject.transform.SetParent(indicatorObject.transform, false);
            textObject.transform.localScale = Vector3.one;

            var rectTransform = textObject.AddComponent<RectTransform>();
            if (rectTransform == null)
            {
                ModLogger.LogError("Failed to add RectTransform to Text");
                Object.Destroy(textObject);
                return null;
            }

            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;

            var textMesh = textObject.AddComponent<TextMeshProUGUI>();
            if (textMesh == null)
            {
                ModLogger.LogError("Failed to add TextMeshProUGUI to Text");
                Object.Destroy(textObject);
                return null;
            }

            textMesh.text = "✓";
            textMesh.color = RecordedIndicatorTextColor;
            textMesh.fontSize = 20f;
            textMesh.alignment = TextAlignmentOptions.Center;

            return textObject;
        }

        public static void ClearAll()
        {
            try
            {
                IndicatedDisplays.Clear();
                ModLogger.Log("Recorded Indicator Clear All");
            }
            catch (Exception ex)
            {
                ModLogger.LogError($"Error Clearing Recorded Indicators: {ex}");
            }
        }
    }
}