using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlueberryEditor.IDE
{
    public class ProjectWriter : Writer<ProjectResource>
    {
        public override void Write(ProjectResource resource)
        {
            OnWriteStarted?.Invoke();

            if (!Directory.Exists(resource.Path))
                Directory.CreateDirectory(resource.Path);

            if (resource.Content.Name == null)
                throw new Exception("Project name can't be null");

            using (var sw = File.CreateText(resource.Path + resource.Content.Name + ".bbp"))
            {
                sw.Write(JsonConvert.SerializeObject(resource.Content, Formatting.Indented));
            }

            OnWriteEnded?.Invoke();
        }
    }
}
