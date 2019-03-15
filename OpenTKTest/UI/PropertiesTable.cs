using Blueberry.DataTools;
using Blueberry.UI;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace OpenTKTest
{
    public class PropertiesTable : Table
    {
        public PropertiesTable(AtlasController controller, Skin skin)
        {
            var boolFields = new List<FieldInfo>();
            foreach(var f in typeof(Settings).GetFields())
            {
                if (f.FieldType == typeof(bool))
                {
                    boolFields.Add(f);
                }
            }

            for(int i = 0, n = boolFields.Count; i < n; i += 2)
            {
                for(int j = 0; j < Math.Min(2, n - i); j++)
                {
                    var f = boolFields[i + j];
                    var box = new CheckBox(f.Name, skin);
                    box.SetChecked((bool)f.GetValue(controller.AtlasSettings));
                    box.OnClicked += () =>
                    {
                        f.SetValue(controller.AtlasSettings, box.IsChecked());
                    };
                    Add(box).Left();
                }
                Row();
            }
        }
    }
}
