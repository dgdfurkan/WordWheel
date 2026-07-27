using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Runtime.UI.Core
{
    /// <summary>
    /// Full-screen dark scrim shown behind overlay panels.
    /// Keeps exclusive panels (e.g. MainMenu background) at full opacity.
    /// </summary>
    public class UIOverlayScrim : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Image image;

        private Tween fadeTween;

        private const float ScrimAlpha = 240f / 255f;

        public static UIOverlayScrim Create(Transform canvasRoot)
        {
            GameObject scrimObject = new GameObject(
                "OverlayScrim",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image),
                typeof(CanvasGroup),
                typeof(UIOverlayScrim));

            scrimObject.transform.SetParent(canvasRoot, false);

            UIOverlayScrim scrim = scrimObject.GetComponent<UIOverlayScrim>();
            scrim.Configure();
            scrimObject.SetActive(false);
            return scrim;
        }

        private void Configure()
        {
            RectTransform rectTransform = transform as RectTransform;
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            rectTransform.localScale = Vector3.one;

            image = GetComponent<Image>();
            image.color = new Color(0f, 0f, 0f, ScrimAlpha);
            image.raycastTarget = true;

            canvasGroup = GetComponent<CanvasGroup>();
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = true;
        }

        public void Show(float duration = 0.25f)
        {
            fadeTween?.Kill();
            gameObject.SetActive(true);
            fadeTween = canvasGroup
                .DOFade(1f, duration)
                .SetUpdate(true);
        }

        public void Hide(float duration = 0.2f)
        {
            fadeTween?.Kill();

            if (!gameObject.activeSelf)
            {
                return;
            }

            fadeTween = canvasGroup
                .DOFade(0f, duration)
                .SetUpdate(true)
                .OnComplete(() => gameObject.SetActive(false));
        }

        public void PlaceBelow(params Transform[] topLayers)
        {
            transform.SetAsLastSibling();

            if (topLayers == null)
            {
                return;
            }

            for (int index = 0; index < topLayers.Length; index++)
            {
                Transform topLayer = topLayers[index];
                if (topLayer != null)
                {
                    topLayer.SetAsLastSibling();
                }
            }
        }

        private void OnDestroy()
        {
            fadeTween?.Kill();
        }
    }
}
