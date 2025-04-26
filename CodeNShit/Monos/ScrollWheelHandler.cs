using UnityEngine;
using UnityEngine.UI;
using static TooltipExpansion.CodeNShit.TooltipExpansionPlugin;

namespace TooltipExpansion.CodeNShit.Monos;

public class TooltipScrollInitializer : MonoBehaviour
{
    private bool _isInitialized;

    private void OnEnable()
    {
        if (_isInitialized) return;
        // Run the conversion now that the tooltip is active.
        Functions.ConvertTextToScrollView(gameObject);
        Canvas.ForceUpdateCanvases();
        _isInitialized = true;
    }
}

public class ScrollWheelHandler : MonoBehaviour
{
    private ScrollRect _scrollRect = null!;

    private void Awake()
    {
        _scrollRect = GetComponent<ScrollRect>();
        if (_scrollRect == null)
        {
            TooltipExpansionPluginLogger.LogWarning("ScrollWheelHandler: No ScrollRect found on " + gameObject.name);
        }
        else
        {
            _scrollRect.verticalNormalizedPosition = 1f;
        }
    }

    private void Update()
    {
        // Only process if this object is active.
        if (!gameObject.activeInHierarchy || _scrollRect == null)
            return;

        // Get scroll wheel input regardless of pointer location.
        float scrollDelta = Input.GetAxis("Mouse ScrollWheel");
        if (!(Mathf.Abs(scrollDelta) > float.Epsilon)) return;
        // Adjust the vertical scroll position.
        float newScrollPosition = _scrollRect.verticalNormalizedPosition + scrollDelta * 0.7f;
        _scrollRect.verticalNormalizedPosition = Mathf.Clamp01(newScrollPosition);
    }
}