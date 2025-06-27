using System;
using System.Collections.Generic;

public class CraftingItem
{
    public string Name { get; set; }
    public int Tier { get; set; }
    public int Rarity { get; set; }
    public string Icon { get; set; }
    public List<Recipe> Recipes { get; set; } = [];
    public int ExtractionSkill { get; set; } = -1;
    public bool Craftable => Recipes.Count > 0;
    public string GenericName => Name.Substring(Math.Max(0, Name.IndexOf(' ') + 1));
}