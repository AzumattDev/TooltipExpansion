using TMPro;
using TooltipExpansion.CodeNShit.Monos;
using UnityEngine;
using UnityEngine.UI;
using static TooltipExpansion.CodeNShit.TooltipExpansionPlugin;

namespace TooltipExpansion.CodeNShit;

public static class Functions
{
    public static void ConvertTextToScrollView(GameObject tooltipObject)
    {
        Transform tooltipTextTransform = Utils.FindChild(tooltipObject.transform, "Text");
        if (tooltipTextTransform == null)
        {
            TooltipExpansionPluginLogger.LogWarning("Tooltip does not contain a 'Text' object.");
            return;
        }

        TMP_Text tooltipTextComponent = tooltipTextTransform.GetComponent<TMP_Text>();
        if (tooltipTextComponent == null)
        {
            TooltipExpansionPluginLogger.LogWarning("Tooltip 'Text' object is missing TMP_Text component.");
            return;
        }

        ContentSizeFitter csf = tooltipTextComponent.gameObject.GetOrAddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // Force an update so we get a proper preferredHeight.
        tooltipTextComponent.ForceMeshUpdate();

        // Save the original parent (should be Bkg) and the original Text RectTransform.
        Transform originalParentTransform = tooltipTextTransform.parent;
        int originalSiblingIndex = tooltipTextTransform.GetSiblingIndex();
        RectTransform originalTextRectTransform = (tooltipTextTransform as RectTransform)!;

        GameObject scrollViewGO = GenScrollView(ref originalParentTransform, originalTextRectTransform, tooltipTextComponent);
        RectTransform viewportRT = GenViewPort(ref scrollViewGO, originalTextRectTransform);
        ScrollRect scrollRect = GenScrollRect(ref scrollViewGO, originalTextRectTransform, ref viewportRT);
        GenScrollBar(ref scrollViewGO, ref scrollRect);


        scrollRect.gameObject.AddComponent<ScrollWheelHandler>();
        scrollRect.gameObject.AddComponent<TooltipSizeAdjuster>();
    }


    private static GameObject GenScrollView(ref Transform originalParentTransform, RectTransform originalTextRectTransform, TMP_Text tooltipTextComponent)
    {
        GameObject scrollViewGO = new GameObject("TooltipScrollView", typeof(RectTransform), typeof(ScrollRect), typeof(Image));

        // Parent it to the same parent as the original Text and at the same sibling index.
        scrollViewGO.transform.SetParent(originalParentTransform, false);
        scrollViewGO.transform.SetSiblingIndex(originalTextRectTransform.GetSiblingIndex());
        RectTransform scrollViewRect = scrollViewGO.GetComponent<RectTransform>();

        // Copy the original Text’s anchors, pivot, and anchored position.
        scrollViewRect.anchorMin = originalTextRectTransform.anchorMin;
        scrollViewRect.anchorMax = originalTextRectTransform.anchorMax;
        scrollViewRect.pivot = originalTextRectTransform.pivot;
        scrollViewRect.anchoredPosition = originalTextRectTransform.anchoredPosition;

        tooltipTextComponent.ForceMeshUpdate();

        float wrapWidth = originalTextRectTransform.rect.width;
        Vector2 pref = tooltipTextComponent.GetPreferredValues(tooltipTextComponent.text, wrapWidth, 0f);
        float measuredHeight = pref.y;

        // Add a LayoutElement so that Bkg’s Vertical Layout Group reserves this space.
        LayoutElement layoutElem = scrollViewGO.AddComponent<LayoutElement>();
        float scrollbarWidth = InventoryGui.instance?.m_recipeListScroll?.handleRect.rect.width ?? 0f;

        layoutElem.preferredWidth = originalTextRectTransform.rect.width + scrollbarWidth + 5f;
        scrollViewRect.sizeDelta = new Vector2(originalTextRectTransform.rect.width + scrollbarWidth, 0f);

        if (tooltipTextComponent.preferredHeight > 460f && GuiScaler.m_largeGuiScale >= 1.11f && UITooltip.m_hovered != null && UITooltip.m_hovered.name == "JC_ItemBackground")
        {
            layoutElem.minHeight = 460f;
            layoutElem.preferredHeight = 460f;
        }
        else
        {
            layoutElem.preferredHeight = Mathf.Min(measuredHeight, Screen.height * 0.9f); // clamp if needed
            layoutElem.minHeight = Mathf.Min(measuredHeight, 460f);
        }


        Image img = scrollViewGO.GetComponent<Image>();
        img.color = new Color(0, 0, 0, 0.2f);
        img.raycastTarget = true;
        //LayoutRebuilder.ForceRebuildLayoutImmediate(scrollViewRect);
        //LayoutRebuilder.ForceRebuildLayoutImmediate(originalParentTransform as RectTransform);
        return scrollViewGO;
    }

    private static RectTransform GenViewPort(ref GameObject scrollRectGo, RectTransform? originalTextRectTransform)
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

    private static ScrollRect GenScrollRect(ref GameObject scrollRectGo, RectTransform originalTextRectTransform, ref RectTransform viewportRT)
    {
        ScrollRect scrollRect = scrollRectGo.GetComponent<ScrollRect>();
        scrollRect.viewport = viewportRT;

        // Reparent the original Text into the viewport.
        originalTextRectTransform.SetParent(viewportRT, false);
        scrollRect.content = originalTextRectTransform;
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;
        scrollRect.scrollSensitivity = 50;
        // Make scroll start at top
        scrollRect.verticalNormalizedPosition = 1f;
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
        scrollRect.onValueChanged.Invoke(new Vector2(0, 0));
    }

    private static T GetOrAddComponent<T>(this GameObject targetGameObject) where T : Component
    {
        T comp = targetGameObject.GetComponent<T>();
        if (comp == null)
            comp = targetGameObject.AddComponent<T>();
        return comp;
    }
}