using Blueberry.DataTools;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace BlueberryTexturePackerCore.App.Models
{
    public class PackModel
    {
        [JsonIgnore]
        public Settings Settings { get; set; }

        [JsonProperty]
        public string Name { get; set; }

        [JsonProperty]
        public string Filename { get; set; }

        [JsonProperty]
        public string Output { get; set; }
    }
}
