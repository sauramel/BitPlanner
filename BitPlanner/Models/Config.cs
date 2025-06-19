using System.Runtime.InteropServices;
using Godot;

public static class Config
{
    private const string CONFIG_PATH = "user://config.ini";
    private const int DEFAULT_WINDOW_WIDTH = 640;
    private const int DEFAULT_WINDOW_HEIGHT = 720;
    private const double DEFAULT_SCALE = 1.0;
    private const bool DEFAULT_NON_GUARANTEED_AS_BASE = true;
    private static readonly bool _defaultCsd = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) || RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
    private static ConfigFile _configFile;

    public static Vector2I WindowSize
    {
        get
        {
            var width = _configFile?.GetValue("Window", "Width", DEFAULT_WINDOW_WIDTH).AsInt32() ?? DEFAULT_WINDOW_WIDTH;
            var height = _configFile?.GetValue("Window", "Height", DEFAULT_WINDOW_HEIGHT).AsInt32() ?? DEFAULT_WINDOW_HEIGHT;
            return new(width, height);
        }

        set
        {
            _configFile?.SetValue("Window", "Width", value.X);
            _configFile?.SetValue("Window", "Height", value.Y);
        }
    }

    public static bool ClientSideDecorations
    {
        get
        {
            if (OS.GetName().Contains("Android"))
            {
                return false;
            }
            return _configFile?.GetValue("Window", "CSD", _defaultCsd).AsBool() ?? _defaultCsd;
        }

        set => _configFile?.SetValue("Window", "CSD", value);
    }

    public static double Scale
    {
        get => _configFile?.GetValue("Window", "Scale", OS.GetName().Contains("Android") ? 1.5 : DEFAULT_SCALE).AsDouble() ?? DEFAULT_SCALE;

        set => _configFile?.SetValue("Window", "Scale", value);
    }

    public static bool TreatNonGuaranteedItemsAsBase
    {
        get => _configFile?.GetValue("Craft", "NonGuaranteedAsBase", DEFAULT_NON_GUARANTEED_AS_BASE).AsBool() ?? DEFAULT_NON_GUARANTEED_AS_BASE;

        set => _configFile?.SetValue("Craft", "NonGuaranteedAsBase", value);
    }

    public static void Load()
    {
        if (_configFile == null)
        {
            _configFile = new ConfigFile();
            if (_configFile.Load(CONFIG_PATH) != Error.Ok)
            {
                GD.Print("Failed to load user config");
            }
        }
    }

    public static void Save()
    {
        if (_configFile?.Save(CONFIG_PATH) != Error.Ok)
        {
            GD.Print("Failed to save user config");
        }
        GD.Print("User config saved");
    }
}