using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blueberry
{
    public static partial class Content
    {
        public static T Load<T>(string filename)
        {
            return Blueberry.OpenGL.Content.Load<T>(filename);
        }

        public static void Unload<T>(T content) where T : class
        {
            Blueberry.OpenGL.Content.Unload(content);
        }
    }
}
