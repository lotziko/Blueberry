
using Microsoft.Xna.Framework.Content;

namespace Blueberry
{
    public static partial class Content
    {
        private static ContentManager manager;

        internal static void Initialize(ContentManager cManager)
        {
            manager = cManager;   
        }

        public static T Load<T>(string filename)
        {
            return manager.Load<T>(filename);
        }
    }
}
