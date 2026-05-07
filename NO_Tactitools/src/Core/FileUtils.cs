using System;
using System.IO;
using System.Collections.Generic;

namespace NO_Tactitools.Core;
public static class FileUtilities
{
    private static FileStream OpenConfigFile(string configFile, bool createMissing = false)
    {
        string assemblyDir = Path.GetDirectoryName(typeof(Plugin).Assembly.Location) ?? Environment.CurrentDirectory;
        string absolutePath = Path.Combine(assemblyDir, "config", configFile);
        if (!createMissing && !File.Exists(absolutePath))
          return null;
        else
          return File.Open(absolutePath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
    }

    public static List<string> GetListFromConfigFile(string configFile)
    {
        var configStream = OpenConfigFile(configFile);
        List<string> result = new();
        if (configStream != null)
        {
            using (configStream)
            using (var configReader = new StreamReader(configStream))
            {
                string line;
                while ((line = configReader.ReadLine()) != null)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    string trimmedStart = line.TrimStart();
                    if (trimmedStart.StartsWith("//")) continue;
                    result.Add(trimmedStart.TrimEnd());
                }
                Plugin.Log(string.Format("Loaded {0} entries from config file {1}.", result.Count, configFile));
            }
        }
        return result;
    }

    public static void WriteListToConfigFile(string configFile, List<string> lines)
    {
        var configStream = OpenConfigFile(configFile, true);
        if (configStream != null)
        {
            using (configStream)
            using (var configWriter = new StreamWriter(configStream))
            {
                foreach (string line in lines)
                  configWriter.WriteLine(line);
                Plugin.Log(string.Format("Saved {0} entries to config file {1}.", lines.Count, configFile));
            }
        }
    }
}
