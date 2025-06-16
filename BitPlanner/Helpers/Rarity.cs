using Godot;

public static class Rarity
{
    public static string GetName(int rarity)
    {
        return rarity switch
        {
            1 => "Common",
            2 => "Uncommon",
            3 => "Rare",
            4 => "Epic",
            5 => "Legendary",
            6 => "Mythic",
            _ => "Unknown"
        };
    }

    public static Color GetColor(int rarity)
    {
        return rarity switch
        {
            2 => Color.FromHtml("955c52"),
            3 => Color.FromHtml("6a809f"),
            4 => Color.FromHtml("c37513"),
            5 => Color.FromHtml("1096bd"),
            6 => Color.FromHtml("7e4de7"),
            _ => Color.FromHtml("866d49")
        };
    }
}