using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Runtime.UI.Panels.Loading
{
    /// <summary>
    /// Builds placeholder loading logo hierarchy. Atlas sprites replace Images later — names stay the same.
    /// Wheel: WheelHalf_0/1. Word/Wheel labels: image pieces (Word_0..3, Wheel_0..4), not text.
    /// </summary>
    public static class LoadingPanelViewBuilder
    {
        private static readonly Color LogoWhite = Color.white;

        public static void EnsureBuilt(Transform panelRoot, LoadingPanelView view)
        {
            if (view == null)
            {
                return;
            }

            view.CaptureReferences();
            view.ConsolidateVisualLayers();
            EnsureBackgroundExists(panelRoot, view);

            if (view.HasQuickTransitionMask)
            {
                EnsureQuickMaskVisual(view.QuickTransitionMask);
                view.ApplyLayerOrder();
            }
            else
            {
                EnsureQuickTransitionMask(panelRoot, view);
            }

            if (view.IsBuilt && !view.HasLegacyTextLayout())
            {
                ApplyWhiteTheme(view);
                view.TryApplySavedLayoutAfterBuild();
                view.ApplyLayerOrder();
                return;
            }

            if (view.HasLegacyTextLayout())
            {
                Transform logoAssemblyTransform = panelRoot.Find("LogoAssembly");
                RemoveChildGroup(logoAssemblyTransform, "WordGroup");
                RemoveChildGroup(logoAssemblyTransform, "WheelLabelGroup");
                view.CaptureReferences();
            }

            RectTransform rootRect = panelRoot as RectTransform;

            CanvasGroup background = view.Background;
            if (background == null && rootRect != null)
            {
                background = CreateBackground(rootRect);
            }

            RectTransform quickTransitionMask = EnsureQuickTransitionMaskTransform(panelRoot, view);

            RectTransform logoAssembly = view.LogoAssembly
                ?? CreateRect("LogoAssembly", rootRect, Vector2.zero, new Vector2(760f, 980f));
            RectTransform wheelRoot = view.WheelRoot
                ?? CreateRect("WheelRoot", logoAssembly, Vector2.zero, new Vector2(520f, 520f));

            RectTransform[] wheelHalves = EnsureWheelHalves(wheelRoot, view);
            RectTransform[] wordPieces = EnsureImagePieceRow(
                logoAssembly,
                view.WordLetters,
                "WordGroup",
                pieceCount: 4,
                anchoredPosition: new Vector2(0f, 150f),
                pieceSize: new Vector2(80f, 100f),
                spacing: 82f,
                piecePrefix: "Word");
            RectTransform[] wheelPieces = EnsureImagePieceRow(
                logoAssembly,
                view.WheelLetters,
                "WheelLabelGroup",
                pieceCount: 5,
                anchoredPosition: new Vector2(0f, -150f),
                pieceSize: new Vector2(80f, 100f),
                spacing: 82f,
                piecePrefix: "Wheel");
            RectTransform[] leftSparkles = EnsureSparkles(
                logoAssembly,
                view.LeftSparkles,
                "SparkLeftGroup",
                new Vector2(-300f, 40f),
                isLeft: true);
            RectTransform[] rightSparkles = EnsureSparkles(
                logoAssembly,
                view.RightSparkles,
                "SparkRightGroup",
                new Vector2(300f, 40f),
                isLeft: false);

            view.AssignBuiltReferences(
                background,
                quickTransitionMask,
                logoAssembly,
                wheelRoot,
                wheelHalves,
                wordPieces,
                wheelPieces,
                leftSparkles,
                rightSparkles);

            ApplyWhiteTheme(view);
            view.TryApplySavedLayoutAfterBuild();
            view.ApplyLayerOrder();
        }

        private static void EnsureQuickTransitionMask(Transform panelRoot, LoadingPanelView view)
        {
            if (view.HasQuickTransitionMask)
            {
                EnsureQuickMaskVisual(view.QuickTransitionMask);
                view.ApplyLayerOrder();
                return;
            }

            EnsureQuickTransitionMaskTransform(panelRoot, view);
            view.CaptureReferences();
        }

        private static RectTransform EnsureQuickTransitionMaskTransform(Transform panelRoot, LoadingPanelView view)
        {
            if (view.HasQuickTransitionMask)
            {
                EnsureQuickMaskVisual(view.QuickTransitionMask);
                view.ApplyLayerOrder();
                return view.QuickTransitionMask;
            }

            Transform existing = panelRoot.Find("QuickTransitionMask");
            if (existing is RectTransform existingRect)
            {
                EnsureQuickMaskVisual(existingRect);
                view.ApplyLayerOrder();
                return existingRect;
            }

            if (panelRoot is not RectTransform rootRect)
            {
                return null;
            }

            RectTransform mask = CreateQuickTransitionMask(rootRect);
            view.ApplyLayerOrder();
            return mask;
        }

        private static RectTransform CreateQuickTransitionMask(RectTransform parent)
        {
            RectTransform rect = CreateRect("QuickTransitionMask", parent, Vector2.zero, new Vector2(1200f, 1200f));
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.localScale = Vector3.zero;

            EnsureQuickMaskVisual(rect);

            CanvasGroup group = rect.gameObject.AddComponent<CanvasGroup>();
            group.alpha = 0f;
            group.blocksRaycasts = false;
            rect.gameObject.SetActive(false);
            return rect;
        }

        private static void EnsureQuickMaskVisual(RectTransform mask)
        {
            Image image = mask.GetComponent<Image>();
            if (image == null)
            {
                image = mask.gameObject.AddComponent<Image>();
            }

            if (image.sprite == null)
            {
                image.sprite = GetDefaultCircleSprite();
            }

            image.color = Color.black;
            image.raycastTarget = false;
            image.preserveAspect = true;
        }

        private static Sprite GetDefaultCircleSprite()
        {
            return Resources.GetBuiltinResource<Sprite>("Knob.psd");
        }

        private static void EnsureBackgroundExists(Transform panelRoot, LoadingPanelView view)
        {
            if (view.Background != null)
            {
                EnsureSplashBackgroundVisual(view.Background);
                return;
            }

            Transform backgroundTransform = panelRoot.Find("Background");
            if (backgroundTransform == null && panelRoot is RectTransform rootRect)
            {
                CreateBackground(rootRect);
                view.CaptureReferences();
                return;
            }

            if (backgroundTransform != null)
            {
                EnsureSplashBackgroundVisual(backgroundTransform.GetComponent<CanvasGroup>());
            }
        }

        private static void EnsureSplashBackgroundVisual(CanvasGroup backgroundGroup)
        {
            if (backgroundGroup == null)
            {
                return;
            }

            RectTransform rect = backgroundGroup.transform as RectTransform;
            if (rect != null)
            {
                StretchFull(rect);
                rect.localScale = Vector3.one;
            }

            Image image = backgroundGroup.GetComponent<Image>();
            if (image != null)
            {
                image.sprite = null;
                image.color = Color.black;
                image.type = Image.Type.Simple;
                image.preserveAspect = false;
            }
        }

        private static RectTransform[] EnsureWheelHalves(RectTransform wheelRoot, LoadingPanelView view)
        {
            IReadOnlyList<RectTransform> existing = view.GetActiveWheelPieces();
            if (existing.Count >= 2)
            {
                RectTransform[] cached = new RectTransform[existing.Count];
                for (int index = 0; index < existing.Count; index++)
                {
                    cached[index] = existing[index];
                    EnsureLogoImage(cached[index]);
                }

                return cached;
            }

            return new RectTransform[]
            {
                CreateWheelHalf(wheelRoot, index: 0, fromLeft: true),
                CreateWheelHalf(wheelRoot, index: 1, fromLeft: false)
            };
        }

        private static RectTransform[] EnsureImagePieceRow(
            RectTransform parent,
            RectTransform[] cachedPieces,
            string groupName,
            int pieceCount,
            Vector2 anchoredPosition,
            Vector2 pieceSize,
            float spacing,
            string piecePrefix)
        {
            if (cachedPieces != null && cachedPieces.Length == pieceCount && HasValidPieces(cachedPieces))
            {
                for (int index = 0; index < cachedPieces.Length; index++)
                {
                    EnsureLogoImage(cachedPieces[index]);
                }

                return cachedPieces;
            }

            RemoveChildGroup(parent, groupName);

            RectTransform group = CreateRect(groupName, parent, anchoredPosition, new Vector2(pieceCount * spacing, pieceSize.y));
            RectTransform[] result = new RectTransform[pieceCount];
            float startX = -((pieceCount - 1) * 0.5f) * spacing;

            for (int index = 0; index < pieceCount; index++)
            {
                RectTransform piece = CreateRect(
                    $"{piecePrefix}_{index}",
                    group,
                    new Vector2(startX + index * spacing, 0f),
                    pieceSize);

                EnsureLogoImage(piece);
                piece.gameObject.AddComponent<CanvasGroup>();
                result[index] = piece;
            }

            return result;
        }

        private static RectTransform[] EnsureSparkles(
            RectTransform parent,
            RectTransform[] cachedSparkles,
            string groupName,
            Vector2 anchoredPosition,
            bool isLeft)
        {
            if (cachedSparkles != null && cachedSparkles.Length == 2 && HasValidPieces(cachedSparkles))
            {
                for (int index = 0; index < cachedSparkles.Length; index++)
                {
                    EnsureLogoImage(cachedSparkles[index]);
                }

                return cachedSparkles;
            }

            return CreateSparkles(parent, groupName, anchoredPosition, isLeft);
        }

        private static bool HasValidPieces(RectTransform[] pieces)
        {
            for (int index = 0; index < pieces.Length; index++)
            {
                if (pieces[index] == null)
                {
                    return false;
                }
            }

            return true;
        }

        private static void ApplyWhiteTheme(LoadingPanelView view)
        {
            SetWhiteOnPieces(view.GetActiveWheelPieces());
            SetWhiteOnPieces(view.WordLetters);
            SetWhiteOnPieces(view.WheelLetters);
            SetWhiteOnPieces(view.LeftSparkles);
            SetWhiteOnPieces(view.RightSparkles);
        }

        private static void SetWhiteOnPieces(System.Collections.Generic.IReadOnlyList<RectTransform> pieces)
        {
            if (pieces == null)
            {
                return;
            }

            for (int index = 0; index < pieces.Count; index++)
            {
                SetWhiteOnPieces(pieces[index]);
            }
        }

        private static void SetWhiteOnPieces(RectTransform piece)
        {
            if (piece == null)
            {
                return;
            }

            Image image = EnsureLogoImage(piece);
            image.color = LogoWhite;
        }

        private static void SetWhiteOnPieces(RectTransform[] pieces)
        {
            if (pieces == null)
            {
                return;
            }

            for (int index = 0; index < pieces.Length; index++)
            {
                SetWhiteOnPieces(pieces[index]);
            }
        }

        private static Image EnsureLogoImage(RectTransform target)
        {
            Image image = target.GetComponent<Image>();
            if (image == null)
            {
                image = target.gameObject.AddComponent<Image>();
            }

            image.color = LogoWhite;
            image.raycastTarget = false;
            image.preserveAspect = true;
            return image;
        }

        private static CanvasGroup CreateBackground(RectTransform parent)
        {
            RectTransform rect = CreateRect("Background", parent, Vector2.zero, Vector2.zero);
            StretchFull(rect);

            Image image = rect.gameObject.AddComponent<Image>();
            image.sprite = null;
            image.color = Color.black;
            image.type = Image.Type.Simple;
            image.preserveAspect = false;
            image.raycastTarget = true;

            CanvasGroup group = rect.gameObject.AddComponent<CanvasGroup>();
            group.alpha = 0f;
            group.blocksRaycasts = true;
            return group;
        }

        private static RectTransform CreateWheelHalf(RectTransform wheelRoot, int index, bool fromLeft)
        {
            RectTransform half = CreateRect($"WheelHalf_{index}", wheelRoot, Vector2.zero, new Vector2(270f, 540f));
            half.pivot = fromLeft ? new Vector2(1f, 0.5f) : new Vector2(0f, 0.5f);
            half.anchorMin = new Vector2(0.5f, 0.5f);
            half.anchorMax = new Vector2(0.5f, 0.5f);
            half.anchoredPosition = fromLeft ? new Vector2(-4f, 0f) : new Vector2(4f, 0f);

            EnsureLogoImage(half);
            half.gameObject.AddComponent<CanvasGroup>();
            return half;
        }

        private static RectTransform[] CreateSparkles(RectTransform parent, string groupName, Vector2 anchoredPosition, bool isLeft)
        {
            RemoveChildGroup(parent, groupName);

            RectTransform group = CreateRect(groupName, parent, anchoredPosition, new Vector2(120f, 220f));
            RectTransform[] sparkles = new RectTransform[2];
            float[] offsets = { -36f, 36f };

            for (int index = 0; index < sparkles.Length; index++)
            {
                float y = index == 0 ? 48f : -52f;
                RectTransform sparkle = CreateRect(
                    $"{(isLeft ? "SparkLeft" : "SparkRight")}_{index}",
                    group,
                    new Vector2(offsets[index], y),
                    new Vector2(34f, 34f));

                EnsureLogoImage(sparkle);
                sparkle.gameObject.AddComponent<CanvasGroup>();
                sparkles[index] = sparkle;
            }

            return sparkles;
        }

        private static void RemoveChildGroup(Transform parent, string groupName)
        {
            if (parent == null)
            {
                return;
            }

            Transform group = parent.Find(groupName);
            if (group == null)
            {
                return;
            }

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                Object.DestroyImmediate(group.gameObject);
                return;
            }
#endif
            Object.Destroy(group.gameObject);
        }

        private static RectTransform CreateRect(string name, RectTransform parent, Vector2 anchoredPosition, Vector2 size)
        {
            Transform existing = parent.Find(name);
            if (existing is RectTransform existingRect)
            {
                return existingRect;
            }

            GameObject obj = new GameObject(name, typeof(RectTransform));
            RectTransform rect = obj.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;
            rect.localScale = Vector3.one;
            return rect;
        }

        private static void StretchFull(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.pivot = new Vector2(0.5f, 0.5f);
        }
    }
}
