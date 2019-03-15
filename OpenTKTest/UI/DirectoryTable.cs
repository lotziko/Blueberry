using Blueberry.DataTools;
using Blueberry.UI;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace OpenTKTest
{
    public class DirectoryTable : Table
    {
        public DirectoryTable(AtlasController controller, Skin skin) : base()
        {
            var field = new TextField("", skin);
            field.OnChange += (value) =>
            {
                controller.OutputDirectory = value;
            };
            Add(field).PadRight(10).Fill();

            var btn = new ImageButton(skin.GetDrawable("icon-search"), skin, "highlighted");
            btn.OnClicked += () =>
            {
                var dialog = new System.Windows.Forms.FolderBrowserDialog();
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    field.SetText(dialog.SelectedPath);
                }
            };
            Add(btn).Fill().Row();

            var box = new SelectBox<ExporterContainer>(skin);
            box.OnChange += (value) =>
            {
                controller.Exporter = value.exporter;
            };
            var a = Assembly.GetAssembly(typeof(SelectBox<ExporterContainer>));
            var items = new List<ExporterContainer>();
            var at = typeof(AtlasExporterAttribute);
            foreach (var type in a.GetTypes())
            {
                if (Attribute.IsDefined(type, at))
                {
                    items.Add(new ExporterContainer(type.GetCustomAttribute<AtlasExporterAttribute>().DisplayName, type));
                }
            }
            box.SetItems(items);
            Add(box).Width(200).PadTop(10).Colspan(2);

            Pad(10);
        }

        private class ExporterContainer
        {
            public string name;
            public IContentExporter exporter;

            public ExporterContainer(string name, Type type)
            {
                this.name = name;
                exporter = Activator.CreateInstance(type) as IContentExporter;
            }

            public override string ToString()
            {
                return name;
            }
        }
    }
}
