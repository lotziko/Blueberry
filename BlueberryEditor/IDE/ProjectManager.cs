using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlueberryEditor.IDE
{
    public static class ProjectManager
    {
        public static ProjectResource current;

        public static ProjectResource Load(string path)
        {
            if (current != null)
                throw new Exception("Unload previous project first");
            current = new ProjectReader().Read(path);
            return current;
        }

        public static void Unload()
        {
            
        }
    }
}
