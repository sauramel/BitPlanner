using System;

public interface IPage
{
    public Action BackButtonCallback { get; }
    public bool Visible { get; set; }
}
