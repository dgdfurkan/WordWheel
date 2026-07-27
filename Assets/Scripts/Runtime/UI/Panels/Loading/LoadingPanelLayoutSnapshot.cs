using System;
using UnityEngine;

namespace Runtime.UI.Panels.Loading
{
    [Serializable]
    public class LoadingPanelElementLayout
    {
        public string path;
        public bool active = true;
        public Vector2 anchoredPosition;
        public Vector2 sizeDelta;
        public Vector2 anchorMin = new Vector2(0.5f, 0.5f);
        public Vector2 anchorMax = new Vector2(0.5f, 0.5f);
        public Vector2 pivot = new Vector2(0.5f, 0.5f);
        public Vector3 localScale = Vector3.one;
        public Vector3 localEulerAngles;
    }

    [Serializable]
    public class LoadingPanelLayoutSnapshot
    {
        public string capturedAt;
        public string sourceScene;
        public LoadingPanelElementLayout logoAssembly;
        public LoadingPanelElementLayout[] elements = Array.Empty<LoadingPanelElementLayout>();
    }
}
