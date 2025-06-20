using Godot;

public static class Skill
{
    public static readonly int[] All = [2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 17, 18, 19, 21];

    public static string GetName(int id)
    {
        return id switch
        {
            2 => "Forestry",
            3 => "Carpentry",
            4 => "Masonry",
            5 => "Mining",
            6 => "Smithing",
            7 => "Scholar",
            8 => "Leatherworking",
            9 => "Hunting",
            10 => "Tailoring",
            11 => "Farming",
            12 => "Fishing",
            13 => "Cooking",
            14 => "Foraging",
            15 => "Construction",
            17 => "Taming",
            18 => "Slayer",
            19 => "Merchanting",
            21 => "Sailing",
            _ => "Unknown"
        };
    }

    public static Rect2 GetAtlasRect(int id)
    {
        return id switch
        {
            2 => new Rect2(48, 336, 20, 18),
            3 => new Rect2(68, 182, 20, 20),
            4 => new Rect2(135, 198, 20, 18),
            5 => new Rect2(2, 341, 20, 20),
            6 => new Rect2(133, 157, 20, 20),
            7 => new Rect2(133, 177, 20, 20),
            8 => new Rect2(292, 207, 20, 20),
            9 => new Rect2(324, 186, 18, 20),
            10 => new Rect2(156, 195, 20, 20),
            11 => new Rect2(2, 293, 20, 20),
            12 => new Rect2(89, 95, 20, 20),
            13 => new Rect2(139, 216, 20, 20),
            14 => new Rect2(90, 138, 20, 20),
            15 => new Rect2(25, 239, 20, 20),
            17 => new Rect2(251, 207, 20, 20),
            18 => new Rect2(90, 159, 20, 20),
            19 => new Rect2(48, 210, 19, 18),
            21 => new Rect2(46, 34, 18, 16),
            _ => new Rect2(68, 119, 18, 20)
        };
    }
}