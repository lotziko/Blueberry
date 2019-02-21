using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Blueberry.UI
{
    public partial class Skin
    {
        Dictionary<Type, Dictionary<string, object>> resources = new Dictionary<Type, Dictionary<string, object>>();

        public void Add<T>(string name, T content)
        {
            if (!resources.TryGetValue(typeof(T), out Dictionary<string, object> category))
            {
                category = new Dictionary<string, object>();
                resources.Add(typeof(T), category);
            }
            category.Add(name, content);
        }

        public T Get<T>(string name = "default")
        {
            if (!resources.TryGetValue(typeof(T), out Dictionary<string, object> category))
                return default;
            if (!category.TryGetValue(name, out object item))
                return default;
            return (T)item;
        }

        public IDrawable GetDrawable(string name)
        {
            if (!resources.TryGetValue(typeof(IDrawable), out Dictionary<string, object> category))
                return default;
            if (!category.TryGetValue(name, out object item))
                return default;
            return (IDrawable)item;
        }

        #region Json

        private class ResourceConverter<T> : JsonConverter<T>
        {
            private Skin skin;

            public ResourceConverter(Skin skin)
            {
                this.skin = skin;
            }

            public override T ReadJson(JsonReader reader, Type objectType, T existingValue, bool hasExistingValue, JsonSerializer serializer)
            {
                if (reader.Value is string name)
                {
                    skin.resources.TryGetValue(typeof(T), out Dictionary<string, object> category);
                    if (category == null)
                        return default;
                    category.TryGetValue(name, out object obj);
                    if (obj == null)
                        return default;
                    return (T)obj;
                }
                else
                {
                    var jObject = JObject.Load(reader);
                    return jObject.ToObject<T>();
                }
            }

            public override void WriteJson(JsonWriter writer, T value, JsonSerializer serializer)
            {
                throw new NotImplementedException();
            }
        }

        private class ResConverter : JsonConverter
        {
            private Skin skin;

            public ResConverter(Skin skin)
            {
                this.skin = skin;
            }

            public override bool CanConvert(Type objectType)
            {
                return true;
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                if (reader.Value is string name)
                {
                    skin.resources.TryGetValue(objectType, out Dictionary<string, object> category);
                    if (category == null)
                        return default;
                    category.TryGetValue(name, out object obj);
                    return obj;
                }
                else
                {
                    var jObject = JObject.Load(reader);
                    return jObject.ToObject(objectType);
                }
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                throw new NotImplementedException();
            }
        }

        private class CustomResolver : DefaultContractResolver
        {
            private Skin skin;

            public CustomResolver(Skin skin)
            {
                this.skin = skin;
            }

            protected override JsonObjectContract CreateObjectContract(Type objectType)
            {
                JsonObjectContract contract = base.CreateObjectContract(objectType);
                if (objectType == typeof(IDrawable))
                {
                    contract.Converter = new ResourceConverter<IDrawable>(skin);
                }
                else if (objectType == typeof(IFont))
                {
                    contract.Converter = new ResourceConverter<IFont>(skin);
                }
                else if (objectType == typeof(Col))
                {
                    contract.Converter = new ResourceConverter<Col>(skin);
                }
                else
                {
                    //contract.Converter = new ResConverter(skin);
                }
                return contract;
            }
        }

        #endregion

        public class Builder
        {
            private readonly string jsonPath;
            private Dictionary<Type, Dictionary<string, object>> resources = new Dictionary<Type, Dictionary<string, object>>();

            public Builder(string jsonPath)
            {
                this.jsonPath = jsonPath;
            }

            public Builder Font(string name, IFont font)
            {
                resources.TryGetValue(typeof(IFont), out Dictionary<string, object> category);
                if (category == null)
                {
                    category = new Dictionary<string, object>();
                    resources.Add(typeof(IFont), category);
                }

                category.Add(name, font);

                return this;
            }

            public Builder Atlas(TextureAtlas atlas)
            {
                var regions = ProcessAtlas(atlas);
                resources.TryGetValue(typeof(IDrawable), out Dictionary<string, object> category);
                if (category == null)
                {
                    category = new Dictionary<string, object>();
                    resources.Add(typeof(IDrawable), category);
                }

                foreach(var (name, drawable) in regions)
                {
                    category.Add(name, drawable);
                }
                return this;
            }

            public Skin Build()
            {
                if (!File.Exists(jsonPath))
                    throw new Exception("Json not exists!");

                var data = File.ReadAllText(jsonPath);
                var d1 = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, JObject>>>(data);
                var skin = new Skin()
                {
                    resources = resources
                };

                var serializer = new JsonSerializer
                {
                    ContractResolver = new CustomResolver(skin)
                };

                var baseAssembly = Assembly.GetAssembly(typeof(Skin));
                var currentAssembly = Assembly.GetCallingAssembly();

                foreach (var t in d1.Keys)
                {
                    var type = baseAssembly.GetType(t);
                    if (type == null)
                    {
                        type = currentAssembly.GetType(t);
                    }
                    var dict = d1[t];

                    if (dict != null && type != null)
                    {
                        var newDict = new Dictionary<string, object>();
                        foreach(var styleName in dict.Keys)
                        {
                            var obj = dict[styleName];
                            var converted = obj.ToObject(type, serializer);
                            newDict.Add(styleName, converted);
                        }
                        skin.resources.Add(type, newDict);
                    }
                }
                return skin;
            }
        }
    }
}
