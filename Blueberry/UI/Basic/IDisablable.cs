using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blueberry.UI
{
    public interface IDisablable
    {
        void SetDisabled(bool isDisabled);

        bool IsDisabled();
    }
}
