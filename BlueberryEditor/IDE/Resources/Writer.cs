using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlueberryEditor.IDE
{
    public abstract class Writer<T> where T : Resource<T>
    {
        public abstract void Write<T>(string path, T content);
    }
}
