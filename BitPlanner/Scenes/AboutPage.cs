using Godot;
using System;
using System.Globalization;

public partial class AboutPage : PanelContainer, IPage
{
    public Action BackButtonCallback { get => null; }

    public override void _Ready()
    {
        var appVersion = ProjectSettings.GetSetting("application/config/version");
        GetNode<Label>("ScrollContainer/MarginContainer/VBoxContainer/AppVersion").Text = $"Version {appVersion}";

        using var dataVersionFile = FileAccess.Open("res://data_version.txt", FileAccess.ModeFlags.Read);
        var dataVersion = DateOnly.Parse(dataVersionFile.GetAsText(), CultureInfo.InvariantCulture);
        GetNode<Label>("ScrollContainer/MarginContainer/VBoxContainer/DataVersion").Text = $"Game data as of {dataVersion}";
    }
}
