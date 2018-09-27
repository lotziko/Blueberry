using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlueberryEditor.IDE
{
    public class ProjectResource : Resource<ProjectData>
    {
        public ProjectResource(string path, ProjectData content) : base(path, content)
        {
        }

        public override string ResourceTypeName { get { return "BBProject"; } }

        public ProjectResource Add<T>(Resource<T> resource) where T : Data
        {
            Content.Add(resource, this);
            return this;
        }

        /// <summary>
        /// Returns all resource pathes of specified type
        /// </summary>
        /// <param name="resourceType"></param>
        /// <returns></returns>
        public List<string> Get(string resourceType)
        {
            return Content.Get(resourceType);
        }
    }

    public class ProjectData : Data
    {
        [JsonProperty]
        protected List<ResourceContainer> resources = new List<ResourceContainer>();

        public void Add<T>(Resource<T> resource, ProjectResource project) where T : Data
        {
            var container = new ResourceContainer(resource.Path.Replace(project.Path, "") + resource.Content.Name + ".bb", resource.ResourceTypeName);
            resources.Add(container);
        }

        public List<string> Get(string resourceType)
        {
            var result = new List<string>();
            foreach(var container in resources)
            {
                if (container.resourceType == resourceType)
                    result.Add(container.resourcePath);
            }

            return result;
        }

        public ProjectData(string name)
        {
            Name = name;
        }

        protected struct ResourceContainer
        {
            [JsonProperty]
            public string resourcePath;
            [JsonProperty]
            public string resourceType;

            public ResourceContainer(string resourcePath, string resourceType)
            {
                this.resourcePath = resourcePath;
                this.resourceType = resourceType;
            }
        }
    }
}
