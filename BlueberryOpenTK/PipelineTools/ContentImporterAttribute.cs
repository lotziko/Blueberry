using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlueberryOpenTK.PipelineTools
{
    public class ContentImporterAttribute : Attribute
    {
        public List<string> Extensions { get; } = new List<string>();

        public ContentImporterAttribute(string fileExtension)
        {
            Extensions.Add(fileExtension);
        }

        public ContentImporterAttribute(params string[] fileExtensions)
        {
            Extensions.AddRange(fileExtensions);
        }
    }
}
