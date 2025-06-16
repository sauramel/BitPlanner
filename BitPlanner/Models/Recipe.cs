using System.Collections.Generic;

public class Recipe
{
    public List<ConsumedItem> ConsumedItems { get; set; } = [];
    public List<int> LevelRequirements { get; set; } = [];
    public uint OutputQuantity { get; set; }
    /// <summary>
    /// A dictionary containing possible quantity outcomes with specified chances. For recipes that don't have possibilities variety this dictionary is empty.
    /// </summary>
    public Dictionary<uint, double> Possibilities { get; set; } = [];
}