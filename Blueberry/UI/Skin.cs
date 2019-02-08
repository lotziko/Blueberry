using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Blueberry.UI
{
    public class Skin
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

        #region Json

        private class ColorConverter : JsonConverter<Col>
        {
            public override Col ReadJson(JsonReader reader, Type objectType, Col existingValue, bool hasExistingValue, JsonSerializer serializer)
            {
                throw new NotImplementedException();
            }

            public override void WriteJson(JsonWriter writer, Col value, JsonSerializer serializer)
            {
                throw new NotImplementedException();
            }
        }

        #endregion

        public class Builder
        {
            private string jsonPath;
            private Dictionary<Type, Dictionary<string, object>> resources = new Dictionary<Type, Dictionary<string, object>>();
            private TextureAtlas atlas;

            public Builder(string jsonPath)
            {
                this.jsonPath = jsonPath;
            }

            public Builder Font(string name, IFont font)
            {
                
                //fonts.Add(name, font);
                return this;
            }

            public Builder Atlas(TextureAtlas atlas)
            {
                this.atlas = atlas;
                return this;
            }

            public Skin Build()
            {
                return null;
            }
        }
    }
}
