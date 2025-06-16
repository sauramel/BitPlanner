using System.Collections.Generic;

public class CraftingItem
{
    public string Name { get; set; }
    public int Tier { get; set; }
    public int Rarity { get; set; }
    public string Icon { get; set; }
    public List<Recipe> Recipes { get; set; } = [];
    public bool Craftable => Recipes.Count > 0;
}