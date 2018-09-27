using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlueberryCore.InputModels
{
    public interface IInputModel
    {
        void Initialize();

        void Update();
    }
}
