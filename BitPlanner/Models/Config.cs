using Godot;

public static class Config
{
    private static ConfigFile _configFile;
    private const string _configPath = "user://config.ini";

    public static Vector2I WindowSize
    {
        get
        {
            Load();
            var width = _configFile.GetValue("Window", "Width", 0).AsInt32();
            var height = _configFile.GetValue("Window", "Height", 0).AsInt32();
            return new(width, height);
        }

        set
        {
            Load();
            _configFile.SetValue("Window", "Width", value.X);
            _configFile.SetValue("Window", "Height", value.Y);
        }
    }

    public static double Scale
    {
        get
        {
            Load();
            return _configFile.GetValue("Window", "Scale", OS.GetName().Contains("Android") ? 1.5 : 1.0).AsDouble();
        }

        set
        {
            Load();
            _configFile.SetValue("Window", "Scale", value);
        }
    }

    public static bool TreatNonGuaranteedItemsAsBase
    {
        get
        {
            Load();
            return _configFile.GetValue("Craft", "NonGuaranteedAsBase", true).AsBool();
        }

        set
        {
            Load();
            _configFile.SetValue("Craft", "NonGuaranteedAsBase", value);
        }
    }

    private static void Load()
    {
        if (_configFile == null)
        {
            _configFile = new ConfigFile();
            if (_configFile.Load(_configPath) != Error.Ok)
            {
                GD.Print("Failed to load user config");
            }
        }
    }

    public static void Save()
    {
        if (_configFile == null)
        {
            GD.Print("Cannot save user config as it isn't loaded");
            return;
        }
        if (_configFile.Save(_configPath) != Error.Ok)
        {
            GD.Print("Failed to save user config");
        }
        GD.Print("User config saved");
    }
}