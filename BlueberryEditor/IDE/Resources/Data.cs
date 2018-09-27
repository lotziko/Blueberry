
using Newtonsoft.Json;

namespace BlueberryEditor.IDE
{
    public abstract class Data
    {
        [JsonProperty]
        public string Name { get; protected set; }
    }
}
