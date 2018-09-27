using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlueberryEditor.IDE
{
    public static class ResourceManager
    {
        private static readonly Dictionary<Type, object> readers = new Dictionary<Type, object>();
        private static readonly Dictionary<Type, object> writers = new Dictionary<Type, object>();

        static ResourceManager()
        {
            AddReader<SpriteResource>(new SpriteReader());
            AddWriter<SpriteResource>(new SpriteWriter());
        }

        public static void AddReader<T>(object reader)
        {
            if (!(reader is Reader<T>))
                throw new Exception("Reader must override Reader<T>");
            readers.Add(typeof(T), reader);
        }

        public static void AddWriter<T>(object writer)
        {
            if (!(writer is Writer<T>))
                throw new Exception("Writer must override Writer<T>");
            writers.Add(typeof(T), writer);
        }

        public static T Read<T>(string path)
        {
            if (!readers.ContainsKey(typeof(T)))
                return default;
            var value = readers[typeof(T)];
            if (value is Reader<T>)
            {
                return (value as Reader<T>).Read(path);
            }
            else
                throw new Exception("Reader does not override Reader<T> class");
        }

        public static void Write<T>(T content)
        {
            if (!writers.ContainsKey(typeof(T)))
                return;
            var value = writers[typeof(T)];
            if (value is Writer<T>)
            {
                (value as Writer<T>).Write(content);
            }
            else
                throw new Exception("Writer does not override Writer<T> class");
        }
    }
}
