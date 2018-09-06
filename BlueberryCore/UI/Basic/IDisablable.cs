using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlueberryCore.UI
{
    public interface IDisablable
    {
        void SetDisabled(bool isDisabled);

        bool IsDisabled();
    }
}
