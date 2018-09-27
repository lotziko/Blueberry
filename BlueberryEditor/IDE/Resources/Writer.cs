using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlueberryEditor.IDE
{
    public abstract class Writer<T>
    {
        public abstract void Write(T resource);

        public Action OnWriteStarted, OnWriteEnded;
    }
}
