using BlueberryOpenTK.PipelineTools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace BlueberryOpenTK
{
    public static class Content
    {
        private static Dictionary<string, Dictionary<Type, IContentImporter>> importers = new Dictionary<string, Dictionary<Type, IContentImporter>>();
        private static Dictionary<string, object> loadedAssets = new Dictionary<string, object>();

        static Content()
        {
            var classes = Assembly.GetExecutingAssembly().GetTypes();
            foreach (Type t in classes)
            {
                if (Attribute.IsDefined(t, typeof(ContentImporterAttribute)))
                {
                    var importer = Activator.CreateInstance(t);
                    ContentImporterAttribute attribute = (ContentImporterAttribute)Attribute.GetCustomAttribute(t, typeof(ContentImporterAttribute));

                    foreach (string ext in attribute.Extensions)
                    {
                        if (!importers.TryGetValue(ext, out Dictionary<Type, IContentImporter> extImporters))
                        {
                            extImporters = new Dictionary<Type, IContentImporter>();
                            importers.Add(ext, extImporters);
                        }

                        var arg = importer.GetType().BaseType.GetTypeInfo().GenericTypeArguments;

                        extImporters.Add(arg[0], importer as IContentImporter);
                    }
                }
            }
        }

        public static T Load<T>(string filename)
        {
            if (loadedAssets.ContainsKey(filename))
                return (T)loadedAssets[filename];

            var extension = Path.GetExtension(filename);

            //check if extension importers exist
            if (!importers.TryGetValue(extension, out Dictionary<Type, IContentImporter> extensionImporters))
            {
                throw new Exception("Can't find importers for " + extension + " extension");
            }

            //check if type importers exist
            if (!extensionImporters.TryGetValue(typeof(T), out IContentImporter importer))
            {
                throw new Exception("Can't find importer for " + typeof(T).Name + " type");
            }

            var asset = importer.Import(filename);
            loadedAssets.Add(filename, asset);

            return (T)asset;
        }
    }
}
