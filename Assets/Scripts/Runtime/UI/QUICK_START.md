# UI Panel Management System - Quick Start with DoTween

## ⚡ 5-Minute Setup

### Prerequisite: Install DoTween

```
Window → Package Manager → Add package from git URL
https://github.com/Demigiant/dotween.git#upm
```

Or install from Asset Store.

---

### Step 1: Create Canvas

- Right-click in Hierarchy → UI → Canvas

---

### Step 2: Create UIManager

1. Right-click Canvas → Create Empty → Rename to "UIManager"
2. Drag `UIManager.cs` onto UIManager GameObject
3. UIManager will auto-discover all child panels

---

### Step 3: Create Panels

Create these as children of UIManager:

**MainMenuPanel**
- Right-click UIManager → Create Empty → Rename to "MainMenuPanel"
- Attach `MainMenuPanel.cs`
- Add Button children: Play, Settings, Quit

**LanguagePanel** (NEW!)
- Right-click UIManager → Create Empty → Rename to "LanguagePanel"
- Attach `LanguagePanel.cs`
- Add Button groups for native languages and target languages

**SettingsPanel**
- Right-click UIManager → Create Empty → Rename to "SettingsPanel"
- Attach `SettingsPanel.cs`
- Add Sliders/Buttons for Volume, Language, Difficulty

**GameplayPanel**
- Right-click UIManager → Create Empty → Rename to "GameplayPanel"
- Attach `GameplayPanel.cs`
- Add Score display, Pause button, etc.

**PausePanel**
- Right-click UIManager → Create Empty → Rename to "PausePanel"
- Attach `PausePanel.cs`
- Add Buttons: Resume, Settings, Main Menu

---

### Step 4: Connect Buttons

For each button:

1. Select Button → Inspector
2. Find Button component
3. Under "On Click ()", click + button
4. Drag the Panel GameObject into the object field
5. From dropdown: Panel → OnButtonClicked()

**Example connections:**
- MainMenuPanel Play → `MainMenuPanel.OnPlayButtonClicked()`
- MainMenuPanel Settings → `MainMenuPanel.OnSettingsButtonClicked()`
- PausePanel Resume → `PausePanel.OnResumeButtonClicked()`
- LanguagePanel native buttons → `LanguagePanel.OnNativeLanguageSelected(int index)`
- LanguagePanel target buttons → `LanguagePanel.OnTargetLanguageSelected(int index)`

---

### Step 5: Test

Press Play! You should see:
- MainMenuPanel appears with pop-in animation
- Buttons have bounce effect on click
- Smooth transitions between panels

---

## 🎬 Animation Showcase

Each panel has unique animations:

- **MainMenuPanel**: Pop-in + fade entrance
- **LanguagePanel**: Slide-in from left with fade
- **SettingsPanel**: Pop-in + fade (quick)
- **GameplayPanel**: Simple fade
- **PausePanel**: Pop-in + fade with Time.timeScale = 0

All use **DoTween** for smooth, professional feel!

---

## 💻 Code Examples

### Open Main Menu
```csharp
UIManager.Instance.OpenPanel<MainMenuPanel>();
```

### Open Language Selection
```csharp
UIManager.Instance.OpenPanel<LanguagePanel>();
```

### Close Pause Menu
```csharp
UIManager.Instance.ClosePanel<PausePanel>();
```

### Check if Settings Open
```csharp
if (UIManager.Instance.IsPanelOpen<SettingsPanel>())
{
    Debug.Log("Settings are open!");
}
```

### Get Language Preferences
```csharp
LanguagePanel langPanel = UIManager.Instance.GetPanel<LanguagePanel>();
Language nativeLanguage = langPanel.GetNativeLanguage();
Language targetLanguage = langPanel.GetTargetLanguage();
```

### Listen to Panel Events
```csharp
private void Start()
{
    UIManager.Instance.OnPanelOpened.AddListener((panelType) =>
    {
        Debug.Log($"Opened: {panelType.Name}");
    });

    UIManager.Instance.OnPanelClosed.AddListener((panelType) =>
    {
        Debug.Log($"Closed: {panelType.Name}");
    });
}
```

---

## 🎮 Typical Game Flow

```
Start Game
    ↓
MainMenuPanel appears (pop-in animation)
    ↓
Player clicks "Play"
    ↓
LanguagePanel appears (slide-in animation)
    ↓
Player selects Native Language + Target Language
    ↓
Player clicks "Confirm"
    ↓
GameplayPanel appears (fade-in)
    ↓
[Player plays game]
    ↓
Player clicks "Pause"
    ↓
PausePanel appears (pop-in animation, Time.timeScale = 0)
    ↓
[Player can Resume, Settings, or Main Menu]
```

---

## 🎨 Custom Animations

Want different animations? Override `OnOpenPanel()`:

```csharp
public class MyCustomPanel : UIPanel
{
    protected override void OnOpenPanel()
    {
        // Slide in from left + fade
        UIAnimationHelper.SlideInFromLeft(PanelTransform, 0.6f, Ease.OutCubic);
        UIAnimationHelper.FadeIn(canvasGroup, 0.6f, Ease.OutQuad);
    }

    protected override void OnClosePanel()
    {
        // Slide out to right + fade
        UIAnimationHelper.SlideOutToRight(PanelTransform, 0.4f, Ease.InCubic);
        UIAnimationHelper.FadeOut(canvasGroup, 0.4f, Ease.InQuad);
    }
}
```

**Available animations:**
- `FadeIn()` / `FadeOut()`
- `SlideInFromLeft()` / `SlideOutToRight()`
- `PopIn()` / `PopOut()`
- `EntranceAnimation()` / `ExitAnimation()`
- `BounceScale()` / `Shake()` / `Pulse()`

---

## 🌍 Language System

### Save Languages
```csharp
// Automatic via LanguagePanel.OnConfirmButtonClicked()
PlayerPrefs.SetInt("NativeLanguage", (int)Language.Turkish);
PlayerPrefs.SetInt("TargetLanguage", (int)Language.English);
```

### Load Languages
```csharp
Language native = (Language)PlayerPrefs.GetInt("NativeLanguage", 0);
Language target = (Language)PlayerPrefs.GetInt("TargetLanguage", 1);
```

### Language Enum
```csharp
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

## ⚠️ Common Issues

**DoTween not found?**
```
Make sure you installed via Package Manager or Asset Store
using DG.Tweening; at top of file
```

**Buttons not responding?**
```
Check:
1. Button has Button component
2. On Click() has panel selected
3. Method signature matches (e.g., OnPlayButtonClicked())
```

**Animations not smooth?**
```
Check Time.timeScale isn't 0 (except for PausePanel)
```

**Panel doesn't appear?**
```
Ensure panel is child of UIManager
Ensure correct script is attached
Press Play and check Console for errors
```

---

## 📚 Full Documentation

See `README.md` for complete API reference and architecture details.

---

**You're ready!** 🚀 Create your panels and enjoy smooth, professional animations!

