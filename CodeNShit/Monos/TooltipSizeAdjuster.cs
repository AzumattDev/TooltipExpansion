using UnityEngine;
using UnityEngine.UI;

namespace TooltipExpansion.CodeNShit.Monos;

public class TooltipSizeAdjuster : MonoBehaviour
{
    public float maxHeight = 450f;
    private ScrollRect _scrollRect = null!;
    private RectTransform _scrollViewRT = null!;
    private RectTransform _contentRT = null!;

    private void Awake()
    {
        _scrollRect = GetComponent<ScrollRect>();
        _scrollViewRT = GetComponent<RectTransform>();
        if (_scrollRect != null)
            _contentRT = _scrollRect.content;
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

        _scrollViewRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, contentHeight < maxHeight ? contentHeight : maxHeight);
    }
}