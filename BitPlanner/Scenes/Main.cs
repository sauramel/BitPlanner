using System;
using Godot;

public partial class Main : Control
{
    private const int SIDEBAR_THRESHOLD = 850;
    private Action _backButtonCallback;
    private Button _sidebarButton;
    private Button _backButton;
    private Button _settingsButton;
    private Control _sidebarPlaceholder;
    private PanelContainer _sidebar;
    private PanelContainer _sidebarContent;
    private Button _craftPageButton;
    private Button _tasksPageButton;
    private Button _aboutPageButton;
    private ColorRect _sidebarOverlay;
    private CraftPage _craftPage;
    private TasksPage _tasksPage;
    private AboutPage _aboutPage;
    private PopupPanel _settingsPopup;
    private SpinBox _uiScaleSpin;
    private CheckButton _nonGuaranteedAsBaseCheck;
    private Button _settingsSaveButton;

    public override void _Ready()
    {
        var data = GameData.Instance;
        var dataLoaded = data.Load();
        if (!dataLoaded)
        {
            GD.Print("FATAL: Failed to load game data");
            GetTree().Quit();
        }

        _sidebarButton = GetNode<Button>("VBoxContainer/PanelContainer/MarginContainer/HBoxContainer/SidebarButton");
        _sidebarPlaceholder = GetNode<Control>("VBoxContainer/HBoxContainer/SidebarPlaceholder");
        _sidebar = GetNode<PanelContainer>("Sidebar");
        _sidebarContent = _sidebar.GetNode<PanelContainer>("HBoxContainer/Content");
        _sidebarButton.Toggled += (toggled) =>
        {
            _sidebar.Visible = toggled;
            _sidebarPlaceholder.Visible = toggled;
        };
        _sidebarOverlay = _sidebar.GetNode<ColorRect>("HBoxContainer/Overlay");
        _sidebarOverlay.GuiInput += (inputEvent) =>
        {
            if (inputEvent is InputEventMouseButton && inputEvent.IsPressed())
            {
                _sidebarButton.ButtonPressed = false;
            }
        };
        _craftPageButton = _sidebarContent.GetNode<Button>("MarginContainer/VBoxContainer/CraftPageButton");
        _craftPageButton.Toggled += (toggled) => ActivatePage(_craftPage, toggled);
        _tasksPageButton = _sidebarContent.GetNode<Button>("MarginContainer/VBoxContainer/TasksPageButton");
        _tasksPageButton.Toggled += (toggled) => ActivatePage(_tasksPage, toggled);
        _aboutPageButton = _sidebarContent.GetNode<Button>("MarginContainer/VBoxContainer/AboutPageButton");
        _aboutPageButton.Toggled += (toggled) => ActivatePage(_aboutPage, toggled);

        _backButton = GetNode<Button>("VBoxContainer/PanelContainer/MarginContainer/HBoxContainer/BackButton");
        _backButton.Pressed += () => _backButtonCallback?.Invoke();

        _settingsButton = GetNode<Button>("VBoxContainer/PanelContainer/MarginContainer/HBoxContainer/SettingsButton");
        _settingsPopup = GetNode<PopupPanel>("SettingsPopup");
        _settingsPopup.Visible = false;
        _settingsButton.Pressed += () => _settingsPopup.PopupCentered();

        _craftPage = GetNode<CraftPage>("VBoxContainer/HBoxContainer/Pages/CraftPage");
        _craftPage.BackButtonCallbackChanged += (_, callback) => SetBackButtonCallback(callback);
        _craftPage.Load();
        _craftPage.Visible = false;

        _tasksPage = GetNode<TasksPage>("VBoxContainer/HBoxContainer/Pages/TasksPage");
        _tasksPage.CraftRequested += (_, e) =>
        {
            _craftPageButton.ButtonPressed = true;
            _craftPage.ShowRecipe(e.Id, e.Quantity);
        };
        _tasksPage.Load();
        _tasksPage.Visible = false;

        _aboutPage = GetNode<AboutPage>("VBoxContainer/HBoxContainer/Pages/AboutPage");
        _aboutPage.Visible = false;

        _uiScaleSpin = _settingsPopup.GetNode<SpinBox>("MarginContainer/VBoxContainer/HBoxContainer/ScaleSpin");
        _uiScaleSpin.SetValueNoSignal(Config.Scale);
        _uiScaleSpin.ValueChanged += UpdateScale;

        _nonGuaranteedAsBaseCheck = _settingsPopup.GetNode<CheckButton>("MarginContainer/VBoxContainer/NonGuaranteedAsBaseCheck");
        _nonGuaranteedAsBaseCheck.SetPressedNoSignal(Config.TreatNonGuaranteedItemsAsBase);
        _nonGuaranteedAsBaseCheck.Toggled += (toggled) => Config.TreatNonGuaranteedItemsAsBase = toggled;

        _settingsSaveButton = _settingsPopup.GetNode<Button>("MarginContainer/VBoxContainer/SaveButton");
        _settingsSaveButton.Pressed += () =>
        {
            Config.Save();
            _settingsPopup.Hide();
        };

        GetWindow().MinSize = new Vector2I(600, 400);
        GetWindow().GuiEmbedSubwindows = true;
        var configWindowSize = Config.WindowSize;
        if (configWindowSize.X > 0 && configWindowSize.Y > 0)
        {
            GetWindow().Size = configWindowSize;
        }
        Resized += AdjustLayout;
        AdjustLayout();
        UpdateScale(Config.Scale);

        _craftPageButton.ButtonPressed = true;
        _craftPage.ShowOverview();
    }

    private void UpdateScale(double scale)
    {
        GetWindow().ContentScaleFactor = (float)scale;
        _settingsPopup.PopupCentered();
        Config.Scale = scale;
        AdjustLayout();
    }

    private void AdjustLayout()
    {
        var width = Size.X;
        if (width < SIDEBAR_THRESHOLD * Config.Scale)
        {
            _sidebar.AnchorRight = 1.0f;
            _sidebarPlaceholder.CustomMinimumSize = new Vector2(0, 0);
        }
        else
        {
            _sidebar.AnchorRight = 0.0f;
            _sidebarPlaceholder.CustomMinimumSize = new Vector2(_sidebarContent.Size.X, 0);
        }
    }

    private void SetBackButtonCallback(Action callback)
    {
        _backButtonCallback = callback;
        _backButton.Visible = (callback != null);
    }

    private void ActivatePage(IPage page, bool activated)
    {
        page.Visible = activated;
        if (activated)
        {
            SetBackButtonCallback(page.BackButtonCallback);
        }
    }

    public override void _Notification(int what)
    {
        if (what == NotificationWMCloseRequest)
        {
            Config.WindowSize = GetWindow().Size;
            Config.Save();
        }
        else if (what == NotificationWMGoBackRequest)
        {
            GD.Print(_backButton.Visible);
            if (_backButton.Visible)
            {
                _backButton.EmitSignal(BaseButton.SignalName.Pressed);
            }
            else
            {
                GetTree().Quit();
            }
        }
    }
}
