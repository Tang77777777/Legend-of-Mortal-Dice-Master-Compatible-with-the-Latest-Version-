using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.Mono;
using HarmonyLib;
using UnityEngine;
using System.Reflection;

namespace DiceMasterUiFix
{
    [BepInPlugin("DiceMaster.UiFix", "DiceMaster UI Fix", "1.0.0")]
    [BepInDependency("DiceMaster", BepInDependency.DependencyFlags.HardDependency)]
    public sealed class Plugin : BaseUnityPlugin
    {
        private static ConfigEntry<float> minimumScale;
        private static FieldInfo scaleFactorField;
        private static FieldInfo logWindowRectField;
        private static FieldInfo isInitializedField;
        private static FieldInfo headerStyleField;
        private static FieldInfo logEntryStyleField;
        private static FieldInfo descriptionStyleField;
        private static FieldInfo buttonStyleField;

        private void Awake()
        {
            minimumScale = Config.Bind(
                "UI",
                "MinimumScale",
                1.0f,
                "Minimum DiceMaster UI scale. Use 1.25 for larger text. The patch limits the window to the current screen.");

            var dicePluginType = AccessTools.TypeByName("DiceMaster.DicePlugin");
            if (dicePluginType == null)
            {
                Logger.LogError("DiceMaster.DicePlugin was not found.");
                return;
            }

            scaleFactorField = AccessTools.Field(dicePluginType, "scaleFactor");
            logWindowRectField = AccessTools.Field(dicePluginType, "logWindowRect");
            isInitializedField = AccessTools.Field(dicePluginType, "isInitialized");
            headerStyleField = AccessTools.Field(dicePluginType, "headerStyle");
            logEntryStyleField = AccessTools.Field(dicePluginType, "logEntryStyle");
            descriptionStyleField = AccessTools.Field(dicePluginType, "descriptionStyle");
            buttonStyleField = AccessTools.Field(dicePluginType, "buttonStyle");
            var onGui = AccessTools.Method(dicePluginType, "OnGUI");
            if (scaleFactorField == null || logWindowRectField == null || onGui == null)
            {
                Logger.LogError("DiceMaster UI fields or OnGUI method were not found.");
                return;
            }

            new Harmony("DiceMaster.UiFix").Patch(
                onGui,
                prefix: new HarmonyMethod(typeof(Plugin), nameof(PrepareUi)));
            Logger.LogInfo("DiceMaster UI scale fix loaded; minimum scale = " + minimumScale.Value.ToString("0.##"));
        }

        private static void PrepareUi(object __instance)
        {
            if (__instance == null)
                return;

            var currentScale = (float)scaleFactorField.GetValue(__instance);
            var requestedScale = Mathf.Max(0.5f, minimumScale.Value);
            var screenFitScale = Mathf.Min(Screen.width * 0.9f / 750f, Screen.height * 0.9f / 750f);
            var targetScale = Mathf.Min(requestedScale, Mathf.Max(0.5f, screenFitScale));

            if (Mathf.Abs(currentScale - targetScale) > 0.001f)
                scaleFactorField.SetValue(__instance, targetScale);
            var rect = (Rect)logWindowRectField.GetValue(__instance);
            rect.x = Mathf.Clamp(rect.x, 10f, Mathf.Max(10f, Screen.width - 750f * targetScale - 10f));
            rect.y = Mathf.Clamp(rect.y, 10f, Mathf.Max(10f, Screen.height - 750f * targetScale - 10f));
            rect.width = 750f * targetScale;
            rect.height = 750f * targetScale;
            logWindowRectField.SetValue(__instance, rect);

            // SetGUI uses the scale only once. If the plugin was initialized before
            // this patch ran, update its cached styles as well.
            if (isInitializedField != null && (bool)isInitializedField.GetValue(__instance))
            {
                SetFontSize(__instance, headerStyleField, 14f * targetScale);
                SetFontSize(__instance, logEntryStyleField, 16f * targetScale);
                SetFontSize(__instance, descriptionStyleField, 14f * targetScale);
                SetFontSize(__instance, buttonStyleField, 14f * targetScale);
                var buttonStyle = buttonStyleField?.GetValue(__instance) as GUIStyle;
                if (buttonStyle != null)
                    buttonStyle.fixedWidth = rect.width * 0.2f;
            }
        }

        private static void SetFontSize(object instance, FieldInfo field, float size)
        {
            var style = field?.GetValue(instance) as GUIStyle;
            if (style != null)
                style.fontSize = Mathf.Max(1, Mathf.RoundToInt(size));
        }
    }
}
