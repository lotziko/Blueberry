using Blueberry.DataTools;
using Blueberry.UI;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace OpenTKTest
{
    public class DirectoryTable : Table
    {
        public DirectoryTable(Skin skin) : base()
        {
            var field = new TextField("", skin);
            Add(field);

            var btn = new ImageButton(skin.GetDrawable("icon-search"), skin);
            btn.OnClicked += () =>
            {
                var dialog = new System.Windows.Forms.FolderBrowserDialog();
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    field.SetText(dialog.SelectedPath);
                }
            };
            Add(btn);

            var box = new SelectBox<ExporterContainer>(skin);
            var a = Assembly.GetAssembly(typeof(SelectBox<ExporterContainer>));
            var items = new List<ExporterContainer>();
            var at = typeof(AtlasExporterAttribute);
            foreach (var type in a.GetTypes())
            {
                if (Attribute.IsDefined(type, at))
                {
                    items.Add(new ExporterContainer(type));
                }
            }
            box.SetItems(items);
            Add(box).Width(200);
        }

        private class ExporterContainer
        {
            public Type type;

            public ExporterContainer(Type type)
            {
                this.type = type;
            }

            public override string ToString()
            {
                return type.Name;
            }
        }
    }
}
