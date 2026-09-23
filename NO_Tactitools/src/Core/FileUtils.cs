using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace NO_Tactitools.Core;
public static class FileUtilities
{
    public static string GetConfigDir() {
        string assemblyDir = Path.GetDirectoryName(typeof(Plugin).Assembly.Location) ?? Environment.CurrentDirectory;
        string absolutePath = Path.Combine(assemblyDir, "config");
        return absolutePath;
    }

    public static string GetConfigPath(string configFile) {
        return Path.Combine(GetConfigDir(), configFile);
    }

    public static FileStream OpenConfigFile(string configFile, bool createMissing = false) {
        string absolutePath = GetConfigPath(configFile);
        return (!createMissing && !File.Exists(absolutePath)) ? null : File.Open(absolutePath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
    }

    public static List<string> GetListFromConfigFile(string configFile) {
        var configStream = OpenConfigFile(configFile);
        List<string> result = new();
        if (configStream != null)
        {
            using (configStream)
            using (var configReader = new StreamReader(configStream)) {
                string line;
                while ((line = configReader.ReadLine()) != null) {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    string trimmedStart = line.TrimStart();
                    if (trimmedStart.StartsWith("//")) continue;
                    result.Add(trimmedStart.TrimEnd());
                }
                Plugin.Log($"Loaded {result.Count} entries from config file {configFile}.");
            }
        }
        return result;
    }

    public static void WriteListToConfigFile(string configFile, List<string> lines) {
        var configStream = OpenConfigFile(configFile, true);
        if (configStream != null) {
            using (configStream)
            using (var configWriter = new StreamWriter(configStream)) {
                foreach (string line in lines)
                  configWriter.WriteLine(line);
                Plugin.Log($"Saved {lines.Count} entries to config file {configFile}.");
            }
        }
    }

    public class IgnorePropertiesResolver : DefaultContractResolver {
        private readonly HashSet<string> propertiesToIgnore;

        public IgnorePropertiesResolver(IEnumerable<string> propertiesToIgnore) {
            this.propertiesToIgnore = new HashSet<string>(propertiesToIgnore);
        }

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization) {
            JsonProperty property = base.CreateProperty(member, memberSerialization);

            if (propertiesToIgnore.Contains(property.PropertyName)) {
                property.Ignored = true;
            }

            return property;
        }
    }

    //Usage example:
    /*
    var resolver = new IgnorePropertiesResolver(["ignoreThis", "ignoreThisToo"]);
    var settings = new JsonSerializerSettings {
        ContractResolver = resolver
    };
    string json = JsonConvert.SerializeObject(someObj, Formatting.Indented, settings);
    */
}
