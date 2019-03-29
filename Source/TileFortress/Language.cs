using GeneralShare;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace TileFortress
{
    [JsonObject]
    public class Language : IReadOnlyDictionary<string, string>
    {
        private static Language _current;

        public static Language Current
        {
            get => _current;
            set => _current = value ?? throw new ArgumentNullException(nameof(value));
        }

        public string this[string key] => GetString(key);
        public string this[string key, object arg0] => GetString(key).Format(arg0);
        public string this[string key, object arg0, object arg1] => GetString(key).Format(arg0, arg1);
        public string this[string key, object arg0, object arg1, object arg2] => GetString(key).Format(arg0, arg1, arg2);
        public string this[string key, object arg0, object arg1, object arg2, object arg3] => GetString(key).Format(arg0, arg1, arg2, arg3);
        public string this[string key, params object[] args] => GetString(key).Format(args);

        [JsonProperty] public string Culture { get; }
        [JsonProperty] public string Name { get; }
        [JsonProperty] public string Country { get; }

        [JsonProperty("Strings")] private Dictionary<string, string> _strings;
        [JsonIgnore] public IReadOnlyDictionary<string, string> Strings { get; }

        [JsonIgnore] public int Count => Strings.Count;
        [JsonIgnore] public IEnumerable<string> Keys => Strings.Keys;
        [JsonIgnore] public IEnumerable<string> Values => Strings.Values;

        [JsonConstructor]
        public Language(string culture, string name, string country, Dictionary<string, string> strings)
        {
            Culture = culture;
            Name = name;
            Country = country;

            _strings = strings;
            Strings = new ReadOnlyDictionary<string, string>(_strings);

            if (Current == null)
                Current = this;
        }
        
        public string GetString(string key)
        {
            if (Strings.TryGetValue(key, out string value))
                return value;
            return $"{Culture}:[\"{key}\"]";
        }

        public bool ContainsKey(string key)
        {
            return Strings.ContainsKey(key);
        }

        public bool TryGetValue(string key, out string value)
        {
            return Strings.TryGetValue(key, out value);
        }

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            return Strings.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Strings.GetEnumerator();
        }

        public static void Save(Language value, string path)
        {
            JsonUtils.Serialize(value, path);
        }

        public static Language LoadJson(string json)
        {
            return JsonUtils.DeserializeString<Language>(json);
        }

        public static Language LoadFile(string path)
        {
            return JsonUtils.Deserialize<Language>(path);
        }
    }
}
