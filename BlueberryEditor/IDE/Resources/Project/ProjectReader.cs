using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlueberryEditor.IDE
{
    public class ProjectReader : Reader<ProjectResource>
    {
        public override ProjectResource Read(string path)
        {
            OnReadStarted?.Invoke();

            if (!Directory.Exists(path))
                throw new FileLoadException("Path does not exists");

            ProjectResource resource = null;
            var files = Directory.GetFiles(path);
            foreach (var file in files)
            {
                if (Path.GetExtension(file) == ".bbp")
                {
                    using (var sr = File.OpenText(file))
                    {
                        resource = new ProjectResource(path, JsonConvert.DeserializeObject<ProjectData>(sr.ReadToEnd()));
                        break;
                    }
                }
            }
            if (resource == null)
                throw new FileLoadException("Config file not found");

            OnReadEnded?.Invoke();

            return resource;
        }
    }
}
