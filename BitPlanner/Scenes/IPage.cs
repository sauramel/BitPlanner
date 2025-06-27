using System;
using System.Collections.Generic;

public interface IPage
{
    public Action BackButtonCallback { get; }
    public Dictionary<string, Action> MenuActions { get; }
    public bool Visible { get; set; }
}
