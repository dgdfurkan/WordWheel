# UI Panel Management System - DoTween Edition

A robust, OOP and SOLID-compliant UI Panel management system for Unity games with stylish **DoTween** animations. Prevents state conflicts, nested opens, and other common UI bugs.

## ✨ What's New

- **DoTween Integration** — Smooth, professional animations (fade, slide, pop, shake, bounce)
- **Language Selection System** — LanguagePanel for native & target language selection
- **Professional Animation Library** — UIAnimationHelper with 10+ preset animations
- **Organized Structure** — Runtime, Interfaces, Core, Utilities, Panels folders
- **Zero External Dependencies** — Except DoTween (included in your package manager)

## 📁 File Structure

```
Assets/Scripts/Runtime/
├── Enums/
│   └── Language.cs
├── UI/
│   ├── Interfaces/
│   │   └── IUIPanel.cs
│   ├── Core/
│   │   ├── UIPanel.cs
│   │   └── UIManager.cs
│   ├── Utilities/
│   │   └── UIAnimationHelper.cs (DoTween animations)
│   └── Panels/
│       ├── MainMenuPanel.cs
│       ├── LanguagePanel.cs (NEW!)
│       ├── SettingsPanel.cs
│       ├── GameplayPanel.cs
│       └── PausePanel.cs
└── ...
```

## 🎬 Animation Types

Built-in animations via `UIAnimationHelper`:

- **FadeIn / FadeOut** — Smooth alpha transitions
- **SlideInFromLeft / SlideOutToRight** — Directional slide animations
- **PopIn / PopOut** — Scale pop effects
- **EntranceAnimation / ExitAnimation** — Combined fade + slide (stylish!)
- **BounceScale** — Button press feedback effect
- **Shake** — Error or warning effect
- **Pulse** — Continuous subtle scaling loop

All with customizable **duration** and **easing**!

## 🚀 Quick Start (5 Minutes)

### Step 1: Install DoTween

1. Open `Window → TextMesh Pro → Import TMP Essential Resources` (if needed)
2. Install DoTween:
   - Via Package Manager: `https://github.com/Demigiant/dotween.git` (or download from Asset Store)
   - Or: `openupm add com.demigiant.dotween`

### Step 2: Create Canvas & UIManager

1. Create Canvas (Right-click Hierarchy → UI → Canvas)
2. Create empty GameObject: `UIManager`
3. Attach `UIManager.cs` script

### Step 3: Create Panels

Right-click UIManager, Create Empty for each:
- `MainMenuPanel` → Attach `MainMenuPanel.cs`
- `LanguagePanel` → Attach `LanguagePanel.cs`
- `SettingsPanel` → Attach `SettingsPanel.cs`
- `GameplayPanel` → Attach `GameplayPanel.cs`
- `PausePanel` → Attach `PausePanel.cs`

### Step 4: Add UI Elements & Connect Buttons

For each panel, add buttons and connect via Inspector:
- Button onClick() → Select Panel → Select method (e.g., `OnPlayButtonClicked()`)

### Step 5: Test

Press Play! Panels should appear with smooth animations.

---

## 💡 Code Usage

### Open Panel
```csharp
using Runtime.UI.Core;

UIManager.Instance.OpenPanel<MainMenuPanel>();
```

### Close Panel
```csharp
UIManager.Instance.ClosePanel<SettingsPanel>();
```

### Check if Open
```csharp
if (UIManager.Instance.IsPanelOpen<PausePanel>())
{
    Debug.Log("Pause menu is active!");
}
```

### Listen to Events
```csharp
UIManager.Instance.OnPanelOpened.AddListener((panelType) =>
{
    Debug.Log($"Panel opened: {panelType.Name}");
});
```

---

## 🎨 Custom Animations

Override `OnOpenPanel()` and `OnClosePanel()` in your panel:

```csharp
using Runtime.UI.Core;
using Runtime.UI.Utilities;
using DG.Tweening;

protected override void OnOpenPanel()
{
    // Your custom animation
    UIAnimationHelper.SlideInFromLeft(PanelTransform, 0.6f, Ease.OutCubic);
    UIAnimationHelper.FadeIn(canvasGroup, 0.6f, Ease.OutQuad);
}

protected override void OnClosePanel()
{
    UIAnimationHelper.SlideOutToRight(PanelTransform, 0.4f, Ease.InCubic);
    UIAnimationHelper.FadeOut(canvasGroup, 0.4f, Ease.InQuad);
}
```

---

## 🌍 Language Panel

The new `LanguagePanel` lets players select:
1. **Native Language** — Their main language (for instructions)
2. **Target Language** — Language they want to learn

### Features:
- Prevents selecting same language for both
- Shake animation on invalid selection
- Saves preferences to PlayerPrefs
- Smooth slide-in animation

### Usage:
```csharp
using Runtime.UI.Panels;
using Runtime.Enums;

// Get language preferences
LanguagePanel langPanel = UIManager.Instance.GetPanel<LanguagePanel>();
Language nativeLanguage = langPanel.GetNativeLanguage();
Language targetLanguage = langPanel.GetTargetLanguage();

// Language enum
public enum Language
{
    Turkish = 0,
    English = 1,
    German = 2,
    French = 3,
    Spanish = 4,
}
```

---

## 🎯 Common Workflows

### Menu Transition
```csharp
UIManager.Instance.ClosePanel<MainMenuPanel>();
UIManager.Instance.OpenPanel<LanguagePanel>();
```

### Pause Game
```csharp
UIManager.Instance.OpenPanel<PausePanel>();
// Time.timeScale = 0 handled automatically
```

### Resume Game
```csharp
UIManager.Instance.ClosePanel<PausePanel>();
// Time.timeScale = 1 handled automatically
```

### Game Over
```csharp
Time.timeScale = 0f; // Pause
UIManager.Instance.OpenPanel<GameOverPanel>();
```

---

## 🛡️ Safety Features

✅ **Transition Guards** — No rapid state changes  
✅ **Redundancy Protection** — Can't open panel twice  
✅ **State Tracking** — isOpen, isTransitioning flags  
✅ **SetActive Management** — Automatic enable/disable  
✅ **Event System** — Loose coupling via UnityEvents  

---

## 📝 Creating Your Own Panel

```csharp
using UnityEngine;
using DG.Tweening;
using Runtime.UI.Core;
using Runtime.UI.Utilities;

namespace Runtime.UI.Panels
{
    public class MyCustomPanel : UIPanel
    {
        protected override void OnOpenPanel()
        {
            // Entrance animation
            UIAnimationHelper.PopIn(PanelTransform, 0.4f, Ease.OutBack);
            UIAnimationHelper.FadeIn(canvasGroup, 0.4f, Ease.OutQuad);
        }

        protected override void OnClosePanel()
        {
            // Exit animation
            UIAnimationHelper.PopOut(PanelTransform, 0.3f, Ease.InBack);
            UIAnimationHelper.FadeOut(canvasGroup, 0.3f, Ease.InQuad);
        }

        public void OnButtonClicked()
        {
            UIAnimationHelper.BounceScale(PanelTransform, 1.1f, 0.3f);
            
            DOVirtual.DelayedCall(0.2f, () =>
            {
                UIManager.Instance.ClosePanel<MyCustomPanel>();
            });
        }
    }
}
```

---

## 🔧 UIAnimationHelper API

```csharp
// Simple animations
FadeIn(CanvasGroup canvasGroup, float duration, Ease easeType)
FadeOut(CanvasGroup canvasGroup, float duration, Ease easeType)
SlideInFromLeft(RectTransform rect, float duration, Ease easeType)
SlideOutToRight(RectTransform rect, float duration, Ease easeType)
PopIn(RectTransform rect, float duration, Ease easeType)
PopOut(RectTransform rect, float duration, Ease easeType)

// Complex animations
EntranceAnimation(CanvasGroup cg, RectTransform rect, float duration, Ease ease)
ExitAnimation(CanvasGroup cg, RectTransform rect, float duration, Ease ease)
BounceScale(RectTransform rect, float scale, float duration)
Shake(RectTransform rect, float strength, int vibrato, float duration)
Pulse(RectTransform rect, float scale, float duration, int loops)
```

---

## 🎮 All Panels Included

1. **MainMenuPanel** — Play, Settings, Quit with animations
2. **LanguagePanel** — Native & target language selection (NEW!)
3. **SettingsPanel** — Volume, Language, Difficulty controls
4. **GameplayPanel** — In-game HUD
5. **PausePanel** — Pause menu with Time.timeScale control

Each with professional DoTween animations!

---

## 💾 Language Preferences Storage

`LanguagePanel` automatically saves to PlayerPrefs:

```csharp
PlayerPrefs.SetInt("NativeLanguage", (int)selectedNativeLanguage);
PlayerPrefs.SetInt("TargetLanguage", (int)selectedTargetLanguage);
```

Load them anytime:

```csharp
Language nativeLanguage = (Language)PlayerPrefs.GetInt("NativeLanguage", 0);
Language targetLanguage = (Language)PlayerPrefs.GetInt("TargetLanguage", 1);
```

---

## 🚨 Troubleshooting

**"DoTween not found"**
- Install DoTween via Package Manager or Asset Store
- Check: `using DG.Tweening;`

**"No UIManager in scene"**
- Create GameObject named "UIManager"
- Attach UIManager.cs script

**"Panel not registering"**
- Panel must be child of UIManager
- Ensure correct script is attached

**Animations not smooth**
- Check Time.timeScale isn't 0 (or use TimeScale.Unscaled)
- Verify DoTween version is compatible

---

## SOLID Principles

✓ **S**ingle Responsibility — UIManager handles state, UIPanel handles display  
✓ **O**pen/Closed — Easy to add new panels without modifying existing  
✓ **L**iskov Substitution — All panels implement IUIPanel contract  
✓ **I**nterface Segregation — Minimal IUIPanel interface  
✓ **D**ependency Inversion — Panels depend on abstract UIPanel  

---

**Ready to ship!** 🚀 Enjoy your professional UI system!

## 🎬 Animation Types

Built-in animations via `UIAnimationHelper`:

- **FadeIn / FadeOut** — Smooth alpha transitions
- **SlideInFromLeft / SlideOutToRight** — Directional slide animations
- **PopIn / PopOut** — Scale pop effects
- **EntranceAnimation / ExitAnimation** — Combined fade + slide (stylish!)
- **BounceScale** — Button press feedback effect
- **Shake** — Error or warning effect
- **Pulse** — Continuous subtle scaling loop

All with customizable **duration** and **easing**!

## 🚀 Quick Start (5 Minutes)

### Step 1: Install DoTween

1. Open `Window → TextMesh Pro → Import TMP Essential Resources` (if needed)
2. Install DoTween:
   - Via Package Manager: `https://github.com/Demigiant/dotween.git` (or download from Asset Store)
   - Or: `openupm add com.demigiant.dotween`

### Step 2: Create Canvas & UIManager

1. Create Canvas (Right-click Hierarchy → UI → Canvas)
2. Create empty GameObject: `UIManager`
3. Attach `UIManager.cs` script

### Step 3: Create Panels

Right-click UIManager, Create Empty for each:
- `MainMenuPanel` → Attach `MainMenuPanel.cs`
- `LanguagePanel` → Attach `LanguagePanel.cs`
- `SettingsPanel` → Attach `SettingsPanel.cs`
- `GameplayPanel` → Attach `GameplayPanel.cs`
- `PausePanel` → Attach `PausePanel.cs`

### Step 4: Add UI Elements & Connect Buttons

For each panel, add buttons and connect via Inspector:
- Button onClick() → Select Panel → Select method (e.g., `OnPlayButtonClicked()`)

### Step 5: Test

Press Play! Panels should appear with smooth animations.

---

## 💡 Code Usage

### Open Panel
```csharp
UIManager.Instance.OpenPanel<MainMenuPanel>();
```

### Close Panel
```csharp
UIManager.Instance.ClosePanel<SettingsPanel>();
```

### Check if Open
```csharp
if (UIManager.Instance.IsPanelOpen<PausePanel>())
{
    Debug.Log("Pause menu is active!");
}
```

### Listen to Events
```csharp
UIManager.Instance.OnPanelOpened.AddListener((panelType) =>
{
    Debug.Log($"Panel opened: {panelType.Name}");
});
```

---

## 🎨 Custom Animations

Override `OnOpenPanel()` and `OnClosePanel()` in your panel:

```csharp
protected override void OnOpenPanel()
{
    // Your custom animation
    UIAnimationHelper.SlideInFromLeft(PanelTransform, 0.6f, Ease.OutCubic);
    UIAnimationHelper.FadeIn(canvasGroup, 0.6f, Ease.OutQuad);
}

protected override void OnClosePanel()
{
    UIAnimationHelper.SlideOutToRight(PanelTransform, 0.4f, Ease.InCubic);
    UIAnimationHelper.FadeOut(canvasGroup, 0.4f, Ease.InQuad);
}
```

---

## 🌍 Language Panel

The new `LanguagePanel` lets players select:
1. **Native Language** — Their main language (for instructions)
2. **Target Language** — Language they want to learn

### Features:
- Prevents selecting same language for both
- Shake animation on invalid selection
- Saves preferences to PlayerPrefs
- Smooth slide-in animation

### Usage:
```csharp
// Get language preferences
Language nativeLanguage = languagePanel.GetNativeLanguage();
Language targetLanguage = languagePanel.GetTargetLanguage();

// Language enum
public enum Language
{
    Turkish = 0,
    English = 1,
    German = 2,
    French = 3,
    Spanish = 4,
}
```

---

## 🎯 Common Workflows

### Menu Transition
```csharp
UIManager.Instance.ClosePanel<MainMenuPanel>();
UIManager.Instance.OpenPanel<LanguagePanel>();
```

### Pause Game
```csharp
UIManager.Instance.OpenPanel<PausePanel>();
// Time.timeScale = 0 handled automatically
```

### Resume Game
```csharp
UIManager.Instance.ClosePanel<PausePanel>();
// Time.timeScale = 1 handled automatically
```

### Game Over
```csharp
Time.timeScale = 0f; // Pause
UIManager.Instance.OpenPanel<GameOverPanel>();
```

---

## 🛡️ Safety Features

✅ **Transition Guards** — No rapid state changes  
✅ **Redundancy Protection** — Can't open panel twice  
✅ **State Tracking** — isOpen, isTransitioning flags  
✅ **SetActive Management** — Automatic enable/disable  
✅ **Event System** — Loose coupling via UnityEvents  

---

## 📝 Creating Your Own Panel

```csharp
using UnityEngine;
using DG.Tweening;
using UI.Core;
using UI.Utilities;

namespace UI.Panels
{
    public class MyCustomPanel : UIPanel
    {
        protected override void OnOpenPanel()
        {
            // Entrance animation
            UIAnimationHelper.PopIn(PanelTransform, 0.4f, Ease.OutBack);
            UIAnimationHelper.FadeIn(canvasGroup, 0.4f, Ease.OutQuad);
        }

        protected override void OnClosePanel()
        {
            // Exit animation
            UIAnimationHelper.PopOut(PanelTransform, 0.3f, Ease.InBack);
            UIAnimationHelper.FadeOut(canvasGroup, 0.3f, Ease.InQuad);
        }

        public void OnButtonClicked()
        {
            UIAnimationHelper.BounceScale(PanelTransform, 1.1f, 0.3f);
            
            DOVirtual.DelayedCall(0.2f, () =>
            {
                UIManager.Instance.ClosePanel<MyCustomPanel>();
            });
        }
    }
}
```

---

## 🔧 UIAnimationHelper API

```csharp
// Simple animations
FadeIn(CanvasGroup canvasGroup, float duration, Ease easeType)
FadeOut(CanvasGroup canvasGroup, float duration, Ease easeType)
SlideInFromLeft(RectTransform rect, float duration, Ease easeType)
SlideOutToRight(RectTransform rect, float duration, Ease easeType)
PopIn(RectTransform rect, float duration, Ease easeType)
PopOut(RectTransform rect, float duration, Ease easeType)

// Complex animations
EntranceAnimation(CanvasGroup cg, RectTransform rect, float duration, Ease ease)
ExitAnimation(CanvasGroup cg, RectTransform rect, float duration, Ease ease)
BounceScale(RectTransform rect, float scale, float duration)
Shake(RectTransform rect, float strength, int vibrato, float duration)
Pulse(RectTransform rect, float scale, float duration, int loops)
```

---

## 🎮 All Panels Included

1. **MainMenuPanel** — Play, Settings, Quit with animations
2. **LanguagePanel** — Native & target language selection (NEW!)
3. **SettingsPanel** — Volume, Language, Difficulty controls
4. **GameplayPanel** — In-game HUD
5. **PausePanel** — Pause menu with Time.timeScale control

Each with professional DoTween animations!

---

## 💾 Language Preferences Storage

`LanguagePanel` automatically saves to PlayerPrefs:

```csharp
PlayerPrefs.SetInt("NativeLanguage", (int)selectedNativeLanguage);
PlayerPrefs.SetInt("TargetLanguage", (int)selectedTargetLanguage);
```

Load them anytime:

```csharp
Language nativeLanguage = (Language)PlayerPrefs.GetInt("NativeLanguage", 0);
Language targetLanguage = (Language)PlayerPrefs.GetInt("TargetLanguage", 1);
```

---

## 🚨 Troubleshooting

**"DoTween not found"**
- Install DoTween via Package Manager or Asset Store
- Check: `using DG.Tweening;`

**"No UIManager in scene"**
- Create GameObject named "UIManager"
- Attach UIManager.cs script

**"Panel not registering"**
- Panel must be child of UIManager
- Ensure correct script is attached

**Animations not smooth**
- Check Time.timeScale isn't 0 (or use TimeScale.Unscaled)
- Verify DoTween version is compatible

---

## SOLID Principles

✓ **S**ingle Responsibility — UIManager handles state, UIPanel handles display  
✓ **O**pen/Closed — Easy to add new panels without modifying existing  
✓ **L**iskov Substitution — All panels implement IUIPanel contract  
✓ **I**nterface Segregation — Minimal IUIPanel interface  
✓ **D**ependency Inversion — Panels depend on abstract UIPanel  

---

**Ready to ship!** 🚀 Enjoy your professional UI system!

