using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlueberryEditor.IDE
{
    public abstract class Resource<T>
    {
        public string Path { get; private set; }
        public T Content { get; private set; }
    }
}
