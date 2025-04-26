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
    internal const string ModVersion = "1.1.0";
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
    [HarmonyPriority(Priority.Last)]
    static void Postfix(UITooltip __instance, GameObject go)
    {
        AddCompIfNeeded(UITooltip.m_tooltip, go);
    }

    private static void AddCompIfNeeded(GameObject tooltipInstance, GameObject go)
    {
        if (InventoryGui.instance == null || go == null || (go.name != "JC_ItemBackground" && !go.name.StartsWith("InventoryElement"))) return;

        if (tooltipInstance != null && tooltipInstance.GetComponent<TooltipScrollInitializer>() == null)
        {
            tooltipInstance.AddComponent<TooltipScrollInitializer>();
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
        Vector3[] worldCorners = new Vector3[4];
        transform.GetWorldCorners(worldCorners);
        // corners[0] = bottom left, corners[1] = top left,
        // corners[2] = top right, corners[3] = bottom right.

        float horizontalOffset = 0f;
        float verticalOffset = 0f;

        // If the right edge is past the screen width, move left.
        if (worldCorners[2].x > Screen.width)
            horizontalOffset = Screen.width - worldCorners[2].x;
        // If the left edge is before 0, move right.
        if (worldCorners[0].x < 0)
            horizontalOffset = -worldCorners[0].x;
        // If the top edge is above the screen height, move down.
        if (worldCorners[2].y > Screen.height)
            verticalOffset = Screen.height - worldCorners[2].y;
        // If the bottom edge is below 0, move up.
        if (worldCorners[0].y < 0)
            verticalOffset = -worldCorners[0].y;

        // Apply the computed offset.
        transform.position += new Vector3(horizontalOffset, verticalOffset, 0);

        // Skip the original method.
        return false;
    }
}