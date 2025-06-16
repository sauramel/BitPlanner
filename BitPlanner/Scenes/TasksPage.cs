using Godot;
using System;

public partial class TasksPage : PanelContainer
{
    public class CraftRequestedEventArgs : EventArgs
    {
        public int Id;
        public uint Quantity;
    }

    private readonly GameData _data = GameData.Instance;
    private PackedScene _travelerScene;
    private TabContainer _travelers;
    private Texture2D _coinsIcon;
    private Texture2D _craftingIcon;

    public event EventHandler<CraftRequestedEventArgs> CraftRequested;

    public override void _Ready()
    {
        _travelerScene = GD.Load<PackedScene>("res://Scenes/Traveler.tscn");
        _travelers = GetNode<TabContainer>("MarginContainer/Travelers");
        _coinsIcon = GD.Load<Texture2D>("res://Assets/HexCoin.png");
        _craftingIcon = GD.Load<Texture2D>("res://Assets/CraftingSmall.png");
    }

    public void Load()
    {
        foreach (var traveler in _data.Travelers)
        {
            var tab = _travelerScene.Instantiate();
            _travelers.AddChild(tab);
            tab.Name = traveler.Name;

            var travelerImage = tab.GetNode<TextureRect>("HBoxContainer/MarginContainer/Image");
            travelerImage.Texture = GD.Load<Texture2D>($"res://Assets/Travelers/{traveler.Name}.png");

            var travelerName = tab.GetNode<Label>("HBoxContainer/VBoxContainer/Name");
            travelerName.Text = traveler.Name;

            var travelerSkillIcon = tab.GetNode<TextureRect>("HBoxContainer/VBoxContainer/HBoxContainer/SkillIcon").Texture as AtlasTexture;
            travelerSkillIcon.ResourceLocalToScene = true;
            travelerSkillIcon.Region = Skill.GetAtlasRect(traveler.Skill);

            var travelerSkillName = tab.GetNode<Label>("HBoxContainer/VBoxContainer/HBoxContainer/SkillName");
            travelerSkillName.Text = Skill.GetName(traveler.Skill);

            var tasksTree = tab.GetNode<Tree>("Tasks");
            tasksTree.SetColumnExpandRatio(0, 8);
            tasksTree.SetColumnCustomMinimumWidth(1, 98);
            tasksTree.ButtonClicked += OnTasksTreeButtonClicked;

            var root = tasksTree.CreateItem();
            foreach (var task in traveler.Tasks)
            {
                var taskDescription = tasksTree.CreateItem(root);

                if (task.Levels[1] < 120)
                {
                    taskDescription.SetText(0, $"Level requirements: {task.Levels[0]}—{task.Levels[1]}");
                }
                else
                {
                    taskDescription.SetText(0, $"Level requirements: {task.Levels[0]}+");
                }

                taskDescription.SetText(1, $"{task.Experience:N0} XP");
                taskDescription.SetTextAlignment(1, HorizontalAlignment.Right);

                taskDescription.SetText(2, $"{task.Reward:N0}");
                taskDescription.SetIcon(2, _coinsIcon);

                foreach (var item in task.RequiredItems)
                {
                    var itemData = _data.CraftingItems[item.Key];
                    var quantity = item.Value;
                    var itemRow = tasksTree.CreateItem(taskDescription);

                    itemRow.SetText(0, $"{itemData.Name}{(quantity > 1 ? $" x{quantity}" : "")}");
                    var resourcePath = $"res://Assets/{itemData.Icon}.png";
                    if (ResourceLoader.Exists(resourcePath))
                    {
                        itemRow.SetIcon(0, GD.Load<Texture2D>(resourcePath));
                    }

                    if (itemData.Craftable)
                    {
                        itemRow.AddButton(0, _craftingIcon);
                    }

                    var meta = new Godot.Collections.Array
                    {
                        item.Key,
                        quantity
                    };
                    itemRow.SetMetadata(0, meta);
                }
            }
        }
    }

    private void OnTasksTreeButtonClicked(TreeItem item, long column, long it, long mouseButtonIndex)
    {
        var data = item.GetMetadata(0).AsGodotArray();
        var e = new CraftRequestedEventArgs()
        {
            Id = data[0].AsInt32(),
            Quantity = data[1].AsUInt32()
        };
        CraftRequested?.Invoke(this, e);
    }
}
