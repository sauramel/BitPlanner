using System.Collections.Generic;

public class TravelerTask
{
    public List<int> Levels { get; set; }
    public Dictionary<ulong, uint> RequiredItems { get; set; }
    public int Reward { get; set; }
    public float Experience { get; set; }
}