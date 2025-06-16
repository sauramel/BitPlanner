using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Godot;

[JsonSourceGenerationOptions(AllowTrailingCommas = true, PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower)]
[JsonSerializable(typeof(Dictionary<int, CraftingItem>))]
[JsonSerializable(typeof(CraftingItem))]
[JsonSerializable(typeof(ConsumedItem))]
[JsonSerializable(typeof(Recipe))]
[JsonSerializable(typeof(List<Traveler>))]
[JsonSerializable(typeof(Traveler))]
[JsonSerializable(typeof(TravelerTask))]
public partial class GameDataContext : JsonSerializerContext
{
}

public sealed class GameData
{
    private static GameData _instance;

    public static GameData Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new GameData();
            }
            return _instance;
        }
    }

    public Dictionary<int, CraftingItem> CraftingItems { get; private set; }
    public List<Traveler> Travelers { get; private set; }

    private GameData()
    {
    }

    public bool Load()
    {
        using var craftingDataFile = FileAccess.Open("res://crafting_data.json", FileAccess.ModeFlags.Read);
        using var travelersDataFile = FileAccess.Open("res://travelers_data.json", FileAccess.ModeFlags.Read);

        try
        {
            CraftingItems = JsonSerializer.Deserialize(craftingDataFile.GetAsText(), GameDataContext.Default.DictionaryInt32CraftingItem);
            Travelers = JsonSerializer.Deserialize(travelersDataFile.GetAsText(), GameDataContext.Default.ListTraveler);
        }
        catch (Exception e)
        {
            GD.Print(e);
            return false;
        }
        return true;
    }
}