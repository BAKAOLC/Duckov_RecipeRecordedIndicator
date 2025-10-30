﻿using System;
using Duckov.UI;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Duckov_RecipeRecordedIndicator
{
    public static class RecordedIndicatorUI
    {
        public const string IndicatorObjectName = "CheckedRecordedIndicator";

        public static readonly Vector2 IndicatorSize = new(28f, 28f);

        public static readonly Vector2 IndicatorAnchorPositionOnLeft = new(5f, -5f);
        public static readonly Vector2 IndicatorAnchorMinOnLeft = new(0f, 1f);
        public static readonly Vector2 IndicatorAnchorMaxOnLeft = new(0f, 1f);
        public static readonly Vector2 IndicatorPivotOnLeft = new(0f, 1f);

        public static readonly Vector2 IndicatorAnchorPositionOnRight = new(-5f, -5f);
        public static readonly Vector2 IndicatorAnchorMinOnRight = new(1f, 1f);
        public static readonly Vector2 IndicatorAnchorMaxOnRight = new(1f, 1f);
        public static readonly Vector2 IndicatorPivotOnRight = new(1f, 1f);

        public static readonly Color RecordedIndicatorBgColor = new(0.2f, 0.8f, 0.2f, 1f);
        public static readonly Color RecordedIndicatorTextColor = new(1f, 1f, 1f, 1f);

        public static void AddOrUpdateIndicator(ItemDisplay itemDisplay, bool isRecorded)
        {
            if (itemDisplay == null) return;

            var indicatorObject = GetIndicator(itemDisplay);
            if (indicatorObject == null) return;

            indicatorObject.SetActive(isRecorded);
        }

        private static GameObject? GetIndicator(ItemDisplay itemDisplay)
        {
            if (itemDisplay == null) return null;

            var indicatorTransform = itemDisplay.transform.Find(IndicatorObjectName);
            if (indicatorTransform != null) return indicatorTransform.gameObject;

            if (!CreateIndicator(itemDisplay)) return null;

            indicatorTransform = itemDisplay.transform.Find(IndicatorObjectName);
            return indicatorTransform?.gameObject;
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
                if (textObject != null) return true;

                ModLogger.LogError("Failed to create text object");
                Object.Destroy(indicatorObject);
                return false;
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

            ModConfig.GetConfigValue<bool>("ShowIndicatorOnLeft", out var showOnLeft);

            ModLogger.Log($"Creating Recorded Indicator on {(showOnLeft ? "Left" : "Right")}");

            if (showOnLeft)
            {
                rectTransform.anchorMin = IndicatorAnchorMinOnLeft;
                rectTransform.anchorMax = IndicatorAnchorMaxOnLeft;
                rectTransform.pivot = IndicatorPivotOnLeft;
                rectTransform.anchoredPosition = IndicatorAnchorPositionOnLeft;
            }
            else
            {
                rectTransform.anchorMin = IndicatorAnchorMinOnRight;
                rectTransform.anchorMax = IndicatorAnchorMaxOnRight;
                rectTransform.pivot = IndicatorPivotOnRight;
                rectTransform.anchoredPosition = IndicatorAnchorPositionOnRight;
            }

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
    }
}