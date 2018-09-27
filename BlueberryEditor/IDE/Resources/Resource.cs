using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlueberryEditor.IDE
{
    public abstract class Resource<T> where T : Data
    {
        public string Path { get; protected set; }
        public T Content { get; protected set; }

        public Resource(string path, T content)
        {
            Path = path;
            Content = content;
        }

        public abstract string ResourceTypeName { get; }
    }
}
