using Godot;
using System;

public partial class Traveler : VBoxContainer
{
    public class CraftRequestedEventArgs : EventArgs
    {
        public int Id;
        public uint Quantity;
    }

    private readonly GameData _data = GameData.Instance;
    private TextureRect _image;
    private Label _name;
    private TextureRect _skillIcon;
    private AtlasTexture _skillIconTexture;
    private Label _skillName;
    private Tree _tasks;
    private Texture2D _coinsIcon;
    private Texture2D _craftingIcon;

    public event EventHandler<CraftRequestedEventArgs> CraftRequested;

    public override void _Ready()
    {
        _image = GetNode<TextureRect>("HBoxContainer/MarginContainer/Image");
        _name = GetNode<Label>("HBoxContainer/VBoxContainer/Name");
        _skillIcon = GetNode<TextureRect>("HBoxContainer/VBoxContainer/HBoxContainer/SkillIcon");
        _skillIconTexture = _skillIcon.Texture as AtlasTexture;
        _skillIconTexture.ResourceLocalToScene = true;
        _skillName = GetNode<Label>("HBoxContainer/VBoxContainer/HBoxContainer/SkillName");

        _tasks = GetNode<Tree>("Tasks");
        _tasks.SetColumnExpandRatio(0, 8);
        _tasks.SetColumnCustomMinimumWidth(1, 98);
        _tasks.ButtonClicked += OnTasksTreeButtonClicked;

        _coinsIcon = GD.Load<Texture2D>("res://Assets/HexCoin.png");
        _craftingIcon = GD.Load<Texture2D>("res://Assets/CraftingSmall.png");

        ThemeChanged += OnThemeChanged;
    }

    public void Load(TravelerData data)
    {
        var iconsColor = Color.FromHtml(Config.Theme == Config.ThemeVariant.Dark ? "e9dfc4" : "15567e");

        Name = data.Name;
        _image.Texture = GD.Load<Texture2D>($"res://Assets/Travelers/{data.Name}.png");
        _name.Text = data.Name;
        _skillIcon.Modulate = iconsColor;
        _skillIconTexture.Region = Skill.GetAtlasRect(data.Skill);
        _skillName.Text = Skill.GetName(data.Skill);

        var root = _tasks.CreateItem();
        foreach (var task in data.Tasks)
        {
            var taskDescription = _tasks.CreateItem(root);

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
                var itemRow = _tasks.CreateItem(taskDescription);

                itemRow.SetText(0, $"{itemData.Name}{(quantity > 1 ? $" x{quantity}" : "")}");
                var resourcePath = $"res://Assets/{itemData.Icon}.png";
                if (ResourceLoader.Exists(resourcePath))
                {
                    itemRow.SetIcon(0, GD.Load<Texture2D>(resourcePath));
                }

                if (itemData.Craftable)
                {
                    itemRow.AddButton(0, _craftingIcon);
                    itemRow.SetButtonColor(0, 0, iconsColor);
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

    private void OnThemeChanged()
    {
        var iconsColor = Color.FromHtml(Config.Theme == Config.ThemeVariant.Dark? "e9dfc4" : "15567e");

        var root = _tasks.GetRoot();
        foreach (var task in root.GetChildren())
        {
            foreach (var item in task.GetChildren())
            {
                if (item.GetButtonCount(0) > 0)
                {
                    item.SetButtonColor(0, 0, iconsColor);
                }
            }
        }
    }

    private void OnTasksTreeButtonClicked(TreeItem item, long column, long id, long mouseButtonIndex)
    {
        var data = item.GetMetadata((int)column).AsGodotArray();
        var e = new CraftRequestedEventArgs()
        {
            Id = data[0].AsInt32(),
            Quantity = data[1].AsUInt32()
        };
        CraftRequested?.Invoke(this, e);
    }
}
