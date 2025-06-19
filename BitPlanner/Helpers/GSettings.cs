#if LINUX

using System.Diagnostics;

public static class GSettings
{
    public static string GetString(string schema, string key)
    {
        var info = new ProcessStartInfo()
        {
            FileName = "gsettings",
            Arguments = $"get {schema} {key}",
            UseShellExecute = false,
            RedirectStandardOutput = true
        };
        using var process = new Process()
        {
            StartInfo = info
        };
        process.Start();
        process.WaitForExit();
        var output = process.StandardOutput.ReadToEnd();
        return output.Trim().Trim('\'');
    }
}

#endif