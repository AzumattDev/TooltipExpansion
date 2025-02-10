using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using TooltipExpansion.CodeNShit.Monos;
using UnityEngine;
using UnityEngine.UI;

namespace TooltipExpansion.CodeNShit;

[BepInPlugin(ModGUID, ModName, ModVersion)]
public class TooltipExpansionPlugin : BaseUnityPlugin
{
    internal const string ModName = "TooltipExpansion";
    internal const string ModVersion = "1.0.1";
    internal const string Author = "Azumatt";
    private const string ModGUID = $"{Author}.{ModName}";
    internal static string ConnectionError = "";
    private readonly Harmony _harmony = new(ModGUID);
    public static readonly ManualLogSource TooltipExpansionPluginLogger = BepInEx.Logging.Logger.CreateLogSource(ModName);

    public void Awake()
    {
        Assembly assembly = Assembly.GetExecutingAssembly();
        _harmony.PatchAll(assembly);
    }
}

[HarmonyPatch(typeof(UITooltip), nameof(UITooltip.OnHoverStart))]
public class UITooltip_OnHoverStart_Patch
{
    static void Postfix(UITooltip __instance, GameObject go)
    {
        if (InventoryGui.instance == null || go == null || (go.name != "JC_ItemBackground" && !go.name.StartsWith("InventoryElement"))) return;
        if (UITooltip.m_tooltip != null && UITooltip.m_tooltip.GetComponent<TooltipScrollInitializer>() == null)
        {
            UITooltip.m_tooltip.AddComponent<TooltipScrollInitializer>();
        }
    }
}

[HarmonyPatch(typeof(Utils), nameof(Utils.ClampUIToScreen))]
public static class Utils_ClampUIToScreen_Patch
{
    static bool Prefix(RectTransform transform)
    {
        // If this transform (or one of its parents) has a ScrollRect,
        // assume it's our scrollable tooltip and skip clamping.
        if (transform.GetComponentInParent<ScrollRect>() != null)
        {
            return false; // Skip the original clamping.
        }

        // Otherwise, perform clamping in a clear, self‐documented way.
        Vector3[] corners = new Vector3[4];
        transform.GetWorldCorners(corners);
        // corners[0] = bottom left, corners[1] = top left,
        // corners[2] = top right, corners[3] = bottom right.

        float offsetX = 0f;
        float offsetY = 0f;

        // If the right edge is past the screen width, move left.
        if (corners[2].x > Screen.width)
            offsetX = Screen.width - corners[2].x;
        // If the left edge is before 0, move right.
        if (corners[0].x < 0)
            offsetX = -corners[0].x;
        // If the top edge is above the screen height, move down.
        if (corners[2].y > Screen.height)
            offsetY = Screen.height - corners[2].y;
        // If the bottom edge is below 0, move up.
        if (corners[0].y < 0)
            offsetY = -corners[0].y;

        // Apply the computed offset.
        transform.position += new Vector3(offsetX, offsetY, 0);

        // Skip the original method.
        return false;
    }
}