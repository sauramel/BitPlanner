using System;
using Godot;

public partial class CraftPage : PanelContainer, IPage
{
    private Action _backButtonCallback;
    private CraftOverview _overview;
    private RecipeView _recipeView;

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
        _overview = GetNode<CraftOverview>("Overview");
        _overview.RecipeRequested += (_, id) => ShowRecipe(id, 1);
        _recipeView = GetNode<RecipeView>("RecipeView");
    }

    public void Load()
    {
        _overview.Load();
    }

    public void ShowOverview()
    {
        _overview.Visible = true;
        _recipeView.Visible = false;
        BackButtonCallback = null;
    }

    public void ShowRecipe(ulong id, uint quantity)
    {
        _overview.Visible = false;
        _recipeView.Visible = true;
        BackButtonCallback = ShowOverview;
        _recipeView.ShowRecipe(id, quantity);
    }
}
