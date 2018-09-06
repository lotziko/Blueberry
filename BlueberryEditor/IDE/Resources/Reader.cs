using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlueberryEditor.IDE
{
    public abstract class Reader<T> where T : Resource<T>
    {
        public abstract T Read<T>(string path);
    }
}
