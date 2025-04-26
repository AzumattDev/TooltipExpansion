using UnityEngine;
using UnityEngine.UI;

namespace TooltipExpansion.CodeNShit.Monos;

public class TooltipSizeAdjuster : MonoBehaviour
{
    public float maxTooltipHeight = Screen.height;
    private ScrollRect _scrollRectComponent = null!;
    private RectTransform _scrollViewRT = null!;
    private RectTransform _contentRT = null!;

    private void Awake()
    {
        _scrollRectComponent = GetComponent<ScrollRect>();
        _scrollViewRT = GetComponent<RectTransform>();
        if (_scrollRectComponent != null)
            _contentRT = _scrollRectComponent.content;
    }

    private void Start()
    {
        AdjustSize();
    }

    public void AdjustSize()
    {
        if (_contentRT == null || _scrollViewRT == null)
            return;

        LayoutRebuilder.ForceRebuildLayoutImmediate(_contentRT);
        float contentHeight = _contentRT.rect.height;

        _scrollViewRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, contentHeight < maxTooltipHeight ? contentHeight : maxTooltipHeight);
    }
}