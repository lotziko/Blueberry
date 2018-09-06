using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;

namespace BlueberryCore
{
    public class ContentPool
    {
        private ContentManager _manager;

        private Dictionary<Type, Dictionary<string, object>> _pool;

        /// <summary>
        /// Basic content load
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>

        public T Load<T>(string name)
        {
            var type = typeof(T);
            _pool.TryGetValue(type, out Dictionary<string, object> category);
            if (category == null)
            {
                category = new Dictionary<string, object>();
                _pool.Add(type, category);
            }

            category.TryGetValue(name, out object item);
            if (item == null)
            {
                item = _manager.Load<T>(name);
                category.Add(name, item);
            }

            return (T) item;
        }

        public ContentPool(IServiceProvider serviceProvider, String rootDirectory)
        {
            _manager = new ContentManager(serviceProvider, rootDirectory);
            _pool = new Dictionary<Type, Dictionary<string, object>>();
        }

        public ContentPool(ResourceContentManager manager)
        {
            _manager = manager;
            _pool = new Dictionary<Type, Dictionary<string, object>>();
        }
    }
}
