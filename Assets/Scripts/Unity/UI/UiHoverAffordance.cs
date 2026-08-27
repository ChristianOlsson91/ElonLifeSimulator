using ElonLifeSim.Core.Content;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ElonLifeSim.Unity.UI
{
    /// <summary>Brighter edge + slight scale on hover/select. Duration matches panel motion.</summary>
    public sealed class UiHoverAffordance : MonoBehaviour,
        IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
    {
        private RectTransform _rt;
        private Outline _outline;
        private Vector3 _rest = Vector3.one;

        private void Awake()
        {
            _rt = transform as RectTransform;
            if (_rt != null)
                _rest = _rt.localScale;
            _outline = GetComponent<Outline>();
        }

        public void OnPointerEnter(PointerEventData eventData) => SetHot(true);
        public void OnPointerExit(PointerEventData eventData) => SetHot(false);
        public void OnSelect(BaseEventData eventData) => SetHot(true);
        public void OnDeselect(BaseEventData eventData) => SetHot(false);

        private void SetHot(bool hot)
        {
            if (_rt != null)
                _rt.localScale = _rest * (hot ? UiStyleTokens.HoverScale : 1f);
            if (_outline != null)
            {
                _outline.effectColor = hot ? UiTheme.Accent : UiTheme.Border;
                _outline.effectDistance = hot ? new Vector2(2f, -2f) : new Vector2(1f, -1f);
            }
        }
    }
}
