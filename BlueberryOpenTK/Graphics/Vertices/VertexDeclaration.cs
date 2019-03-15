using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;

namespace Blueberry.OpenGL
{
    public class VertexDeclaration
    {
        public int VertexStride { get; }
        public GraphicsDevice GraphicsDevice { get; internal set; }
        public VertexElement[] Elements { get; }

        private readonly Dictionary<int, VertexDeclarationAttributeInfo> shaderAttributeInfo = new Dictionary<int, VertexDeclarationAttributeInfo>();

        public VertexDeclaration(int stride, params VertexElement[] elements)
        {
            VertexStride = stride;
            Elements = elements;
        }

        public VertexDeclaration(params VertexElement[] elements) : this(GetVertexStride(elements), elements)
        {
        }

        internal VertexDeclarationAttributeInfo GetAttributeInfo(Effect shader)
        {
            VertexDeclarationAttributeInfo info;
            if (shaderAttributeInfo.TryGetValue(shader.Program, out info))
                return info;

            info = new VertexDeclarationAttributeInfo(GraphicsDevice.MaxVertexAttributes);

            foreach(var v in Elements)
            {
                int location = shader.GetAttributeLocation(v.VertexElementUsage, v.UsageIndex);

                if (location < 0)
                    continue;

                info.Elements.Add(new VertexDeclarationAttributeInfo.Element
                {
                    Offset = v.Offset,
                    AttributeLocation = location,
                    NumberOfElements = v.VertexElementFormat.GetNumberOfElements(),
                    VertexAttribPointerType = v.VertexElementFormat.GetVertexPointerType(),
                    Normalized = false
                });
                info.EnabledAttributes[location] = true;
            }

            shaderAttributeInfo.Add(shader.Program, info);
            return info;
        }

        internal void Apply(Effect shader, IntPtr offset)
        {
            var info = GetAttributeInfo(shader);

            foreach (var e in info.Elements)
            {
                GL.VertexAttribPointer(e.AttributeLocation, e.NumberOfElements, e.VertexAttribPointerType, e.Normalized, VertexStride, (IntPtr)(offset.ToInt64() + e.Offset));
            }

            GraphicsDevice.SetVertexAttributeArray(info.EnabledAttributes);
        }

        private static int GetVertexStride(VertexElement[] elements)
        {
            int max = 0;
            for (var i = 0; i < elements.Length; i++)
            {
                var start = elements[i].Offset + elements[i].VertexElementFormat.GetSize();
                if (max < start)
                    max = start;
            }

            return max;
        }
    }

    internal class VertexDeclarationAttributeInfo
    {
        internal bool[] EnabledAttributes;

        internal class Element
        {
            public int Offset;
            public int AttributeLocation;
            public int NumberOfElements;
            public VertexAttribPointerType VertexAttribPointerType;
            public bool Normalized;
        }

        internal List<Element> Elements;

        internal VertexDeclarationAttributeInfo(int maxVertexAttributes)
        {
            EnabledAttributes = new bool[maxVertexAttributes];
            Elements = new List<Element>();
        }
    }
}
