using System;
using System.Collections.Generic;
using Godot;

public static partial class WindowManager
{
    public enum WindowControl
    {
        Unknown = -1,
        Close = 0,
        Maximize = 1,
        Minimize = 2
    }

    public static Tuple<List<WindowControl>, List<WindowControl>> GetControlsLayout()
    {
#if LINUX
        try
        {
            var result = new Tuple<List<WindowControl>, List<WindowControl>>([], []);
            var buttonLayout = GSettings.GetString("org.gnome.desktop.wm.preferences", "button-layout").Split(':');
            if (buttonLayout.Length > 0)
            {
                foreach (var controlName in buttonLayout[0].Split(","))
                {
                    var control = controlName switch
                    {
                        "minimize" => WindowControl.Minimize,
                        "maximize" => WindowControl.Maximize,
                        "close" => WindowControl.Close,
                        _ => WindowControl.Unknown
                    };
                    result.Item1.Add(control);
                }
            }
            if (buttonLayout.Length > 1)
            {
                foreach (var controlName in buttonLayout[1].Split(","))
                {
                    var control = controlName switch
                    {
                        "minimize" => WindowControl.Minimize,
                        "maximize" => WindowControl.Maximize,
                        "close" => WindowControl.Close,
                        _ => WindowControl.Unknown
                    };
                    result.Item2.Add(control);
                }
            }
            return result;
        }
        catch (Exception e)
        {
            GD.Print(e);
            return new([], [
                WindowControl.Minimize,
                WindowControl.Maximize,
                WindowControl.Close
            ]);
        }
#elif MACOS
        return new([
            WindowControl.Close,
            WindowControl.Minimize,
            WindowControl.Maximize
        ], []);
#else
        return new([], [
            WindowControl.Minimize,
            WindowControl.Maximize,
            WindowControl.Close
        ]);
#endif
        }
}