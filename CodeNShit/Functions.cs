using TMPro;
using TooltipExpansion.CodeNShit.Monos;
using UnityEngine;
using UnityEngine.UI;
using static TooltipExpansion.CodeNShit.TooltipExpansionPlugin;

namespace TooltipExpansion.CodeNShit;

public static class Functions
{
    public static void ConvertTextToScrollView(GameObject tooltip)
    {
        Transform textTransform = Utils.FindChild(tooltip.transform, "Text");
        if (textTransform == null)
        {
            TooltipExpansionPluginLogger.LogWarning("Tooltip does not contain a 'Text' object.");
            return;
        }

        TMP_Text textComponent = textTransform.GetComponent<TMP_Text>();
        if (textComponent == null)
        {
            TooltipExpansionPluginLogger.LogWarning("Tooltip 'Text' object is missing TMP_Text component.");
            return;
        }

        ContentSizeFitter csf = textComponent.gameObject.GetOrAddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // Force an update so we get a proper preferredHeight.
        textComponent.ForceMeshUpdate();

        // Save the original parent (should be Bkg) and the original Text RectTransform.
        Transform originalParent = textTransform.parent;
        int siblingIndex = textTransform.GetSiblingIndex();
        RectTransform originalTextRect = (textTransform as RectTransform)!;

        GameObject scrollViewGO = GenScrollView(ref originalParent, originalTextRect, textComponent);
        RectTransform viewportRT = GenViewPort(ref scrollViewGO, originalTextRect);
        ScrollRect scrollRect = GenScrollRect(ref scrollViewGO, originalTextRect, ref viewportRT);
        GenScrollBar(ref scrollViewGO, ref scrollRect);


        scrollRect.gameObject.AddComponent<ScrollWheelHandler>();
        scrollRect.gameObject.AddComponent<TooltipSizeAdjuster>();
    }


    private static GameObject GenScrollView(ref Transform originalParent, RectTransform originalTextRect, TMP_Text textComponent)
    {
        GameObject scrollViewGO = new GameObject("TooltipScrollView", typeof(RectTransform), typeof(ScrollRect), typeof(Image));

        // Parent it to the same parent as the original Text and at the same sibling index.
        scrollViewGO.transform.SetParent(originalParent, false);
        scrollViewGO.transform.SetSiblingIndex(originalTextRect.GetSiblingIndex());
        RectTransform scrollViewRect = scrollViewGO.GetComponent<RectTransform>();

        // Copy the original Text’s anchors, pivot, and anchored position.
        scrollViewRect.anchorMin = originalTextRect.anchorMin;
        scrollViewRect.anchorMax = originalTextRect.anchorMax;
        scrollViewRect.pivot = originalTextRect.pivot;
        scrollViewRect.anchoredPosition = originalTextRect.anchoredPosition;

        // Explicitly update the Text object's height.
        RectTransform textRT = textComponent.rectTransform;
        textRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, textComponent.preferredHeight);
        LayoutRebuilder.ForceRebuildLayoutImmediate(scrollViewRect);

        float fixedWidth = originalTextRect.rect.width;
        float fixedHeight = originalTextRect.rect.height;
        // If the original height is zero (as it is forced to 0 in the base), choose a default height.
        if (Mathf.Approximately(fixedHeight, 0f))
            fixedHeight = textComponent.preferredHeight; // you can adjust this value
        float width = fixedWidth + InventoryGui.instance.m_recipeListScroll.handleRect.rect.width;
        scrollViewRect.sizeDelta = new Vector2(width + 5, fixedHeight);

        // Add a LayoutElement so that Bkg’s Vertical Layout Group reserves this space.
        LayoutElement layoutElem = scrollViewGO.AddComponent<LayoutElement>();
        layoutElem.preferredWidth = width;
        if (textComponent.preferredHeight > 460f && GuiScaler.m_largeGuiScale >= 1.11f && UITooltip.m_hovered != null && UITooltip.m_hovered.name == "JC_ItemBackground")
        {
            layoutElem.minHeight = 460f;
            layoutElem.preferredHeight = 460f;
        }
        else
        {
            layoutElem.minHeight = fixedHeight;
            layoutElem.preferredHeight = textComponent.preferredHeight;
        }

        textComponent.ForceMeshUpdate();

        Image img = scrollViewGO.GetComponent<Image>();
        img.color = new Color(0, 0, 0, 0.2f);
        img.raycastTarget = true;
        return scrollViewGO;
    }

    private static RectTransform GenViewPort(ref GameObject scrollRectGo, RectTransform? originalTextRect)
    {
        GameObject viewport = new GameObject("TooltipViewport", typeof(RectTransform), typeof(RectMask2D));
        viewport.transform.SetParent(scrollRectGo.transform, false);
        RectTransform vrt = viewport.GetComponent<RectTransform>();

        // Set anchors to stretch fully.
        vrt.anchorMin = new Vector2(0, 0);
        vrt.anchorMax = new Vector2(1, 1);
        vrt.offsetMin = Vector2.zero;
        vrt.offsetMax = Vector2.zero;

        return vrt;
    }

    private static ScrollRect GenScrollRect(ref GameObject scrollRectGo, RectTransform originalTextRect, ref RectTransform viewportRT)
    {
        ScrollRect scrollRect = scrollRectGo.GetComponent<ScrollRect>();
        scrollRect.viewport = viewportRT;

        // Reparent the original Text into the viewport.
        originalTextRect.SetParent(viewportRT, false);
        scrollRect.content = originalTextRect;
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHide;
        scrollRect.scrollSensitivity = 40;
        scrollRect.inertia = true;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;
        scrollRect.onValueChanged.RemoveAllListeners();

        return scrollRect;
    }

    private static void GenScrollBar(ref GameObject scrollRectGo, ref ScrollRect scrollRect)
    {
        Scrollbar newScrollbar = Object.Instantiate(InventoryGui.instance.m_recipeListScroll, scrollRectGo.transform);
        newScrollbar.size = 0.4f;
        scrollRect.onValueChanged.AddListener(_ => newScrollbar.size = 0.4f);
        scrollRect.verticalScrollbar = newScrollbar;
    }

    private static T GetOrAddComponent<T>(this GameObject go) where T : Component
    {
        T comp = go.GetComponent<T>();
        if (comp == null)
            comp = go.AddComponent<T>();
        return comp;
    }
}