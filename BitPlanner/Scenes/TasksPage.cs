using Godot;
using System;

public partial class TasksPage : PanelContainer, IPage
{

    private readonly GameData _data = GameData.Instance;
    private PackedScene _travelerScene;
    private TabContainer _travelers;
    public Action BackButtonCallback { get => null; }

    public event EventHandler<Traveler.CraftRequestedEventArgs> CraftRequested;

    public override void _Ready()
    {
        _travelerScene = GD.Load<PackedScene>("res://Scenes/Traveler.tscn");
        _travelers = GetNode<TabContainer>("MarginContainer/Travelers");
    }

    public void Load()
    {
        foreach (var travelerData in _data.Travelers)
        {
            var traveler = (Traveler)_travelerScene.Instantiate();
            _travelers.AddChild(traveler);
            traveler.Load(travelerData);
            traveler.CraftRequested += (s, e) => CraftRequested?.Invoke(s, e);
        }
    }
}
