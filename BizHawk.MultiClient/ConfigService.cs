﻿using System.IO;
using Newtonsoft.Json;

namespace BizHawk.MultiClient
{
    public static class ConfigService
    {
        public static T Load<T>(string filepath) where T : new()
        {
            T config = new T();

            try
            {
                var file = new FileInfo(filepath);
                using (var reader = file.OpenText())
                {
                    var s = new JsonSerializer();
                    var r = new JsonReader(reader);
                    config = (T) s.Deserialize(r, typeof (T));
                }
            }
            catch { }
            return config;
        }

        public static void Save(string filepath, object config)
        {
            var file = new FileInfo(filepath);
            using (var writer = file.CreateText())
            {
                var s = new JsonSerializer();
                var w = new JsonWriter(writer) { Formatting = Formatting.Indented };
                s.Serialize(w, config);
            }
        }
    }
}