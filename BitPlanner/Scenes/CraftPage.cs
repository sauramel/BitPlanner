using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Godot;

public partial class CraftPage : PanelContainer, IPage
{
    private readonly GameData _data = GameData.Instance;
    private Action _backButtonCallback;
    private MarginContainer _overview;
    private LineEdit _searchEntry;
    private OptionButton _tierSelection;
    private FlowContainer _items;
    private VBoxContainer _recipeView;
    private Tree _recipeTree;
    private TextureRect _recipeIcon;
    private Label _recipeName;
    private Label _recipeTier;
    private Label _recipeRarity;
    private TextureRect _skillIcon;
    private AtlasTexture _skillIconTexture;
    private Label _skillLabel;
    private OptionButton _recipeSelection;
    private SpinBox _quantitySelection;
    private Button _baseIngredientsButton;
    private PopupPanel _baseIngredientsPopup;
    private Tree _baseIngredientsTree;
    private Button _baseIngredientsCopyPlain;
    private Button _baseIngredientsCopyCsv;
    private Texture2D _errorIcon;
    private PopupPanel _recipeLoopPopup;
    private Label _recipeLoopLabel;

    public Action BackButtonCallback
    {
        get => _backButtonCallback;

        private set
        {
            _backButtonCallback = value;
            BackButtonCallbackChanged?.Invoke(this, value);
        }
    }

    public event EventHandler<Action> BackButtonCallbackChanged;

    public override void _Ready()
    {
        _overview = GetNode<MarginContainer>("Overview");
        _searchEntry = _overview.GetNode<LineEdit>("VBoxContainer/HBoxContainer/SearchEntry");
        _searchEntry.TextChanged += (_) => OnOverviewFilterChanged();
        _tierSelection = _overview.GetNode<OptionButton>("VBoxContainer/HBoxContainer/TierSelection");
        _tierSelection.ItemSelected += (_) => OnOverviewFilterChanged();

        _items = _overview.GetNode<FlowContainer>("VBoxContainer/ScrollContainer/Items");
        _recipeView = GetNode<VBoxContainer>("RecipeView");
        var recipeHeader = _recipeView.GetNode<HBoxContainer>("MarginContainer/HBoxContainer");

        _recipeIcon = recipeHeader.GetNode<TextureRect>("MarginContainer/Icon");
        _recipeName = recipeHeader.GetNode<Label>("VBoxContainer/Name");
        _recipeTier = recipeHeader.GetNode<Label>("VBoxContainer/Tier");
        _recipeRarity = recipeHeader.GetNode<Label>("VBoxContainer/Rarity");

        _skillIcon = recipeHeader.GetNode<TextureRect>("VBoxContainer2/HBoxContainer/SkillIcon");
        _skillIconTexture = _skillIcon.Texture as AtlasTexture;
        _skillLabel = recipeHeader.GetNode<Label>("VBoxContainer2/HBoxContainer/SkillLabel");
        _recipeSelection = recipeHeader.GetNode<OptionButton>("VBoxContainer2/HBoxContainer/RecipeSelection");
        _recipeSelection.ItemSelected += OnRecipeChanged;

        _quantitySelection = recipeHeader.GetNode<SpinBox>("VBoxContainer2/HBoxContainer2/Quantity");
        _quantitySelection.ValueChanged += OnQuantityChanged;

        _baseIngredientsButton = recipeHeader.GetNode<Button>("VBoxContainer2/HBoxContainer2/BaseIngredientsButton");
        _baseIngredientsPopup = GetNode<PopupPanel>("BaseIngredientsPopup");
        _baseIngredientsPopup.Visible = false;
        _baseIngredientsTree = _baseIngredientsPopup.GetNode<Tree>("VBoxContainer/BaseIngredientsTree");
        _baseIngredientsButton.Pressed += OnBaseIngredientsRequested;
        _baseIngredientsCopyPlain = _baseIngredientsPopup.GetNode<Button>("VBoxContainer/MarginContainer/HBoxContainer/CopyPlain");
        _baseIngredientsCopyPlain.Pressed += OnBaseIngredientsCopyPlainRequested;
        _baseIngredientsCopyCsv = _baseIngredientsPopup.GetNode<Button>("VBoxContainer/MarginContainer/HBoxContainer/CopyCSV");
        _baseIngredientsCopyCsv.Pressed += OnBaseIngredientsCopyCsvRequested;

        _recipeTree = _recipeView.GetNode<Tree>("RecipeTree");
        _recipeTree.SetColumnCustomMinimumWidth(1, 86);
        _recipeTree.SetColumnExpand(1, false);
        _recipeTree.SetColumnCustomMinimumWidth(2, 98);
        _recipeTree.ItemEdited += OnTreeItemEdited;
        _recipeTree.ButtonClicked += OnRecipeTreeButtonClicked;

        _errorIcon = GD.Load<Texture2D>("res://Assets/Error.png");
        _recipeLoopPopup = GetNode<PopupPanel>("RecipeLoopPopup");
        _recipeLoopPopup.Visible = false;
        _recipeLoopLabel = _recipeLoopPopup.GetNode<Label>("MarginContainer/Label");

        ThemeChanged += OnThemeChanged;
    }

    public void Load()
    {
        foreach (var item in _data.CraftingItems.Where((i) => i.Value.Craftable))
        {
            var data = item.Value;
            var button = new Button();
            _items.AddChild(button);
            button.Text = data.Name;
            button.SetMeta("Tier", data.Tier);
            button.TooltipText = $"{data.Name} ({Rarity.GetName(data.Rarity)})";
            if (!string.IsNullOrEmpty(data.Icon))
            {
                var resourcePath = $"res://Assets/{data.Icon}.png";
                if (ResourceLoader.Exists(resourcePath))
                {
                    button.Icon = GD.Load<Texture2D>(resourcePath);
                }
            }

            button.SetMeta("Rarity", data.Rarity);
            foreach (var styleName in new[] { "normal", "pressed", "hover", "hover_pressed" })
            {
                var defaultStyle = button.GetThemeStylebox(styleName) as StyleBoxFlat;
                var customStyle = defaultStyle.Duplicate() as StyleBoxFlat;
                customStyle.SetBorderWidthAll(1);
                customStyle.BorderColor = Rarity.GetColor(data.Rarity);
                button.AddThemeStyleboxOverride(styleName, customStyle);
            }
            button.CustomMinimumSize = new Vector2(0, 48);

            button.MouseFilter = MouseFilterEnum.Pass;
            button.Pressed += () => ShowRecipe(item.Key);
        }
    }

    public void ShowOverview()
    {
        _overview.Visible = true;
        _recipeView.Visible = false;
        BackButtonCallback = null;
    }

    private void OnOverviewFilterChanged()
    {
        var text = _searchEntry.Text;
        var tier = _tierSelection.Selected;
        foreach (var node in _items.GetChildren())
        {
            var item = node as Button;
            item.Visible = item.Text.IndexOf(text, StringComparison.InvariantCultureIgnoreCase) > -1 && (tier <= 0 || item.GetMeta("Tier", 1).AsInt32() == tier);
        }
    }

    private void OnThemeChanged()
    {
        foreach (var button in _items.GetChildren().Cast<Button>())
        {
            foreach (var styleName in new[] { "normal", "pressed", "hover", "hover_pressed" })
            {
                button.RemoveThemeStyleboxOverride(styleName);
                var defaultStyle = button.GetThemeStylebox(styleName) as StyleBoxFlat;
                var customStyle = defaultStyle.Duplicate() as StyleBoxFlat;
                customStyle.SetBorderWidthAll(1);
                customStyle.BorderColor = Rarity.GetColor(button.GetMeta("Rarity").AsInt32());
                button.AddThemeStyleboxOverride(styleName, customStyle);
            }
        }
        _skillIcon.Modulate = Color.FromHtml(Config.Theme == Config.ThemeVariant.Dark ? "e9dfc4" : "15567e");
    }

    public void ShowRecipe(int id, uint quantity = 1)
    {
        _overview.Visible = false;
        _recipeView.Visible = true;
        BackButtonCallback = ShowOverview;
        var craftingItem = _data.CraftingItems[id];

        if (!string.IsNullOrEmpty(craftingItem.Icon))
        {
            var resourcePath = $"res://Assets/{craftingItem.Icon}.png";
            if (ResourceLoader.Exists(resourcePath))
            {
                _recipeIcon.Texture = GD.Load<Texture2D>(resourcePath);
            }
        }
        _recipeName.Text = craftingItem.Name;
        _recipeTier.Text = $"Tier {Math.Max(craftingItem.Tier, 0)}";
        _recipeRarity.Text = Rarity.GetName(craftingItem.Rarity);
        _recipeRarity.AddThemeColorOverride("font_color", Rarity.GetColor(craftingItem.Rarity));

        _skillIconTexture.Region = Skill.GetAtlasRect(craftingItem.Recipes[0].LevelRequirements[0]);
        _skillLabel.Text = $"{Skill.GetName(craftingItem.Recipes[0].LevelRequirements[0])} Lv. {craftingItem.Recipes[0].LevelRequirements[1]}";

        _recipeSelection.Clear();
        for (var i = 1; i <= craftingItem.Recipes.Count; i++)
        {
            _recipeSelection.AddItem($"Recipe {i}");
        }
        _recipeSelection.Visible = craftingItem.Recipes.Count > 1;

        _recipeTree.Clear();
        _recipeTree.SetColumnExpandRatio(0, (int)Math.Round(10 / Config.Scale * 2));
        var rootItem = _recipeTree.CreateItem();
        BuildTree(id, rootItem, [id], 0, quantity, quantity);
        _recipeSelection.Select(0);
        _quantitySelection.SetValueNoSignal(quantity);
    }

    private void BuildTree(int id, TreeItem treeItem, HashSet<int> shownIds, uint recipeIndex, uint minQuantity, uint maxQuantity)
    {
        foreach (var child in treeItem.GetChildren())
        {
            treeItem.RemoveChild(child);
            child.Free();
        }

        var craftingItem = _data.CraftingItems[id];
        treeItem.SetMetadata(0, id);

        treeItem.SetText(0, craftingItem.Name);
        treeItem.SetTooltipText(0, $"{craftingItem.Name} ({Rarity.GetName(craftingItem.Rarity)})");
        treeItem.SetCustomColor(0, Rarity.GetColor(craftingItem.Rarity));
        if (!string.IsNullOrEmpty(craftingItem.Icon))
        {
            var resourcePath = $"res://Assets/{craftingItem.Icon}.png";
            if (ResourceLoader.Exists(resourcePath))
            {
                treeItem.SetIcon(0, GD.Load<Texture2D>(resourcePath));
            }
        }

        if (treeItem.GetCellMode(1) != TreeItem.TreeCellMode.Range)
        {
            if (craftingItem.Recipes.Count > 1)
            {
                treeItem.SetCellMode(1, TreeItem.TreeCellMode.Range);
                treeItem.SetRangeConfig(1, 1, craftingItem.Recipes.Count, 1.0);
                var rangeText = new StringBuilder();
                for (var i = 1; i <= craftingItem.Recipes.Count; i++)
                {
                    rangeText.Append($"Recipe {i},");
                }
                rangeText.Remove(rangeText.Length - 1, 1);
                treeItem.SetText(1, rangeText.ToString());
                treeItem.SetRange(1, recipeIndex);
                treeItem.SetEditable(1, true);
            }
            else
            {
                treeItem.SetText(1, "");
            }
        }
        var recipeMeta = new Godot.Collections.Array()
        {
            recipeIndex,
            new Godot.Collections.Array(shownIds.Select(i => Variant.CreateFrom(i)))
        };
        treeItem.SetMetadata(1, recipeMeta);

        treeItem.SetTextAlignment(2, HorizontalAlignment.Right);
        var quantityString = GetQuantityString(minQuantity, maxQuantity);
        treeItem.SetText(2, quantityString);
        if (quantityString.Length > 9)
        {
            treeItem.SetTooltipText(2, quantityString);
        }
        var quantityMeta = new Godot.Collections.Array
        {
            minQuantity,
            maxQuantity
        };
        treeItem.SetMetadata(2, quantityMeta);

        if (recipeIndex < craftingItem.Recipes.Count)
        {
            // Calculating quantity of items produced by the recipe.
            // If the quantity is fixed and guaranteed, minOutput and maxOutput are the same.
            var recipe = craftingItem.Recipes[(int)recipeIndex];
            var minOutput = UInt32.MaxValue;
            var maxOutput = 1u;
            var possibilitiesSum = 0.0;
            foreach (var possibility in recipe.Possibilities)
            {
                possibilitiesSum += possibility.Value;
                // 8.0 seems to indicate equal chance to get 1-2 items
                if (possibility.Value >= 8.0)
                {
                    minOutput = 1;
                    maxOutput = possibility.Key;
                }
                // 2.0 seems to indicate equal chance to get 0-1 item
                else if (possibility.Value >= 2.0 || possibility.Value < 1.0)
                {
                    minOutput = 0;
                    maxOutput = possibility.Key;
                }

                if (possibility.Key < minOutput)
                {
                    minOutput = possibility.Key;
                }
                if (possibility.Key > maxOutput)
                {
                    maxOutput = possibility.Key;
                }
            }
            if (minOutput == UInt32.MaxValue)
            {
                minOutput = 1;
            }
            else if (minOutput == 0 && possibilitiesSum >= 1.0)
            {
                minOutput = recipe.Possibilities.Min(p => p.Key);
            }
            minOutput *= recipe.OutputQuantity;
            maxOutput *= recipe.OutputQuantity;

            foreach (var consumedItem in recipe.ConsumedItems)
            {
                if (!shownIds.Add(consumedItem.Id))
                {
                    treeItem.AddButton(0, _errorIcon);
                    return;
                }
            }
            foreach (var consumedItem in recipe.ConsumedItems)
            {
                if (!_data.CraftingItems.ContainsKey(consumedItem.Id))
                {
                    continue;
                }
                var child = treeItem.CreateChild();
                var childMinQuantity = (uint)Math.Ceiling((double)minQuantity / maxOutput) * consumedItem.Quantity;
                // If minOutput is 0 it means that the item is not guaranteed to craft, so we can't know maximum quantity for ingredients and it's therefore set to 0
                var childMaxQuantity = minOutput > 0 ? (uint)Math.Ceiling((double)maxQuantity / minOutput) * consumedItem.Quantity : 0;
                BuildTree(consumedItem.Id, child, new(shownIds), 0, childMinQuantity, childMaxQuantity);
            }
        }
    }

    private void OnRecipeChanged(long index)
    {
        var treeItem = _recipeTree.GetRoot();
        var recipeMeta = treeItem.GetMetadata(1).AsGodotArray();
        if (recipeMeta[0].AsInt64() == index)
        {
            return;
        }

        var id = treeItem.GetMetadata(0).AsInt32();
        var quantityMeta = treeItem.GetMetadata(2).AsGodotArray();
        BuildTree(id, treeItem, [id], (uint)index, quantityMeta[0].AsUInt32(), quantityMeta[1].AsUInt32());
    }

    private void OnQuantityChanged(double quantity)
    {
        var treeItem = _recipeTree.GetRoot();
        var quantityMeta = treeItem.GetMetadata(2).AsGodotArray();
        if (quantityMeta[0].AsDouble() == quantity)
        {
            return;
        }

        var id = treeItem.GetMetadata(0).AsInt32();
        var recipeMeta = treeItem.GetMetadata(1).AsGodotArray();
        BuildTree(id, treeItem, [id], recipeMeta[0].AsUInt32(), (uint)quantity, (uint)quantity);
    }

    private void OnBaseIngredientsRequested()
    {
        var recipeRoot = _recipeTree.GetRoot();
        var data = new Dictionary<int, int[]>();
        GetBaseIngredients(recipeRoot, ref data);

        _baseIngredientsTree.Clear();
        var ingredientsRoot = _baseIngredientsTree.CreateItem();

        var dataForCopying = new Godot.Collections.Dictionary<string, string>();
        foreach (var item in data)
        {
            var craftingItem = _data.CraftingItems[item.Key];
            var treeItem = ingredientsRoot.CreateChild();

            treeItem.SetText(0, craftingItem.Name);
            treeItem.SetTooltipText(0, $"{craftingItem.Name} ({Rarity.GetName(craftingItem.Rarity)})");
            treeItem.SetCustomColor(0, Rarity.GetColor(craftingItem.Rarity));
            if (!string.IsNullOrEmpty(craftingItem.Icon))
            {
                var resourcePath = $"res://Assets/{craftingItem.Icon}.png";
                if (ResourceLoader.Exists(resourcePath))
                {
                    treeItem.SetIcon(0, GD.Load<Texture2D>(resourcePath));
                }
            }

            treeItem.SetTextAlignment(1, HorizontalAlignment.Right);
            var minQuantity = (uint)item.Value[0];
            // Here maxQuantity can be -1, see GetBaseIngredients()
            var maxQuantity = item.Value[1] < 0 ? 0u : (uint)item.Value[1];
            var quantityString = GetQuantityString(minQuantity, maxQuantity);
            treeItem.SetText(1, quantityString);

            dataForCopying.Add(craftingItem.Name, quantityString);
        }
        ingredientsRoot.SetMetadata(0, dataForCopying);
        _baseIngredientsPopup.PopupCentered();
    }

    private void GetBaseIngredients(TreeItem item, ref Dictionary<int, int[]> data)
    {
        var guaranteedCraft = true;
        if (Config.TreatNonGuaranteedItemsAsBase)
        {
            foreach (var child in item.GetChildren())
            {
                var childQuantity = child.GetMetadata(2).AsInt32Array();
                if (childQuantity[1] == 0)
                {
                    guaranteedCraft = false;
                    break;
                }
            }
        }
        if (item.GetChildCount() > 0 && guaranteedCraft)
        {
            foreach (var child in item.GetChildren())
            {
                GetBaseIngredients(child, ref data);
            }
            return;
        }

        var id = item.GetMetadata(0).AsInt32();
        var quantity = item.GetMetadata(2).AsInt32Array();
        var minQuantity = quantity[0];
        var maxQuantity = quantity[1];
        if (!data.ContainsKey(id))
        {
            data[id] = [0, 0];
        }
        if (maxQuantity > 0)
        {
            data[id][0] += minQuantity;
            if (data[id][1] >= 0)
            {
                data[id][1] += maxQuantity;
            }
        }
        else
        {
            if (minQuantity > data[id][0])
            {
                data[id][0] = minQuantity;
            }
            // For the sake of code simplification, here -1 means unknown maximum quantity, unlike in BuildTree() and quantity metadata where it's 0
            data[id][1] = -1;
        }
    }

    private void OnBaseIngredientsCopyPlainRequested()
    {
        var data = _baseIngredientsTree.GetRoot().GetMetadata(0).AsGodotDictionary<string, string>();
        var text = new StringBuilder();
        foreach (var item in data)
        {
            text.Append($"{item.Key} — {item.Value}\n");
        }
        DisplayServer.ClipboardSet(text.ToString());
    }

    private void OnBaseIngredientsCopyCsvRequested()
    {
        var data = _baseIngredientsTree.GetRoot().GetMetadata(0).AsGodotDictionary<string, string>();
        var text = new StringBuilder();
        text.Append("Item Name,Quantity\n");
        foreach (var item in data)
        {
            text.Append($"{item.Key},{item.Value}\n");
        }
        DisplayServer.ClipboardSet(text.ToString());
    }

    private void OnTreeItemEdited()
    {
        var treeItem = _recipeTree.GetEdited();
        var recipeMeta = treeItem.GetMetadata(1).AsGodotArray();
        var newRecipeIndex = (uint)treeItem.GetRange(1);
        if (recipeMeta[0].AsUInt32() == newRecipeIndex)
        {
            return;
        }

        treeItem.ClearButtons();
        var id = treeItem.GetMetadata(0).AsInt32();
        var shownIds = recipeMeta[1].AsInt32Array().ToHashSet();
        var quantityMeta = treeItem.GetMetadata(2).AsGodotArray();
        BuildTree(id, treeItem, shownIds, newRecipeIndex, quantityMeta[0].AsUInt32(), quantityMeta[1].AsUInt32());
    }

    private void OnRecipeTreeButtonClicked(TreeItem item, long column, long it, long mouseButtonIndex)
    {
        _recipeLoopPopup.PopupCentered();
        _recipeLoopLabel.Text = $"Selected recipe for {item.GetText(0)} requires items that are already present on this branch of the crafting tree, creating an infinite loop.";
    }

    private static string GetQuantityString(uint minQuantity, uint maxQuantity)
    {
        if (maxQuantity == minQuantity)
        {
            return $"{minQuantity:N0}";
        }
        else if (maxQuantity == 0)
        {
            return $"≥ {minQuantity:N0}";
        }
        return $"{minQuantity:N0}—{maxQuantity:N0}";
    }
}
