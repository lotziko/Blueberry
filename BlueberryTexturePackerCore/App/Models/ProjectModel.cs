using Blueberry.DataTools;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace BlueberryTexturePackerCore
{
    public class ProjectModel
    {
        [JsonIgnore]
        private string projectFile;

        [JsonProperty]
        public Settings Settings { get; set; }

        [JsonProperty]
        public List<AtlasModel> AtlasModels { get; set; }
    }
}
