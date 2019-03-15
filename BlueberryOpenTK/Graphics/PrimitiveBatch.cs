using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;

namespace Blueberry.OpenGL
{
    public class PrimitiveBatch : IBatch
    {
        const int initialCapacity = 1024;
        protected int currentCapacity;
        protected ushort pPtr, vPtr, iPtr;
        protected GraphicsDevice device;
        protected PrimitiveEffect primitiveEffect;
        protected VertexPositionColor[] vertices;
        protected ushort[] indices;
        protected PrimitiveInfo[] primitives;
        protected Mat transform;

        #region Indices

        private static readonly ushort[][] rectangleIndices =
        {
            new ushort[] {0, 1, 3, 1, 2, 3},
            new ushort[] {0, 1, 1, 2, 2, 3, 3, 0}
        };

        #endregion

        public Mat Transform
        {
            get => transform;
            set
            {
                transform = value;
                primitiveEffect.Transform = value.m;
            }
        }

        public PrimitiveBatch(GraphicsDevice device)
        {
            this.device = device;
            device.PrimitiveRestartIndex = 0xFFFF;
            primitiveEffect = new PrimitiveEffect(device);

            currentCapacity = initialCapacity;
            vertices = new VertexPositionColor[currentCapacity];
            indices = new ushort[currentCapacity];
            primitives = new PrimitiveInfo[currentCapacity];
        }

        public void Begin()
        {
            Flush();

            primitiveEffect.Apply();
        }

        public void End()
        {
            Flush();
        }

        public void Flush()
        {
            if (pPtr == 0)
                return;

            primitiveEffect.Apply();

            int verticesCount = 0, primitivesCount = 0;
            PrimitiveInfo curr = primitives[0];
            for (int i = 0; i < pPtr; i++)
            {
                if (curr.type != primitives[i].type)
                {
                    device.DrawUserIndexedPrimitives(curr.type, vertices, 0, verticesCount, indices, curr.indicesOffset, primitivesCount, VertexPositionColor.VertexDeclaration);
                    curr = primitives[i];
                    verticesCount = 0;
                    primitivesCount = 0;
                }
                verticesCount += primitives[i].verticesCount;
                primitivesCount += primitives[i].primitivesCount;
            }

            device.DrawUserIndexedPrimitives(curr.type, vertices, 0, verticesCount, indices, curr.indicesOffset, primitivesCount, VertexPositionColor.VertexDeclaration);

            pPtr = 0;
            iPtr = 0;
            vPtr = 0;
        }

        public void DrawRectangle(float x, float y, float width, float height, Color4 c, bool border = false)
        {
            vertices[vPtr].Set(x, y, c);
            vertices[vPtr + 1].Set(x + width, y, c);
            vertices[vPtr + 2].Set(x + width, y + height, c);
            vertices[vPtr + 3].Set(x, y + height, c);

            if (border)
            {
                InsertIndices(rectangleIndices[1], indices, vPtr, iPtr);
                primitives[pPtr].Set(PrimitiveType.Lines, iPtr, 4, 4);
                iPtr += 8;
            }
            else
            {
                InsertIndices(rectangleIndices[0], indices, vPtr, iPtr);
                primitives[pPtr].Set(PrimitiveType.Triangles, iPtr, 4, 2);
                iPtr += 6;
            }
            vPtr += 4;
            ++pPtr;
        }

        protected static void InsertIndices(ushort[] indices, ushort[] array, ushort vPtr, ushort iPtr)
        {
            indices.CopyTo(array, iPtr);
            for(int i = 0; i < indices.Length; i++)
            {
                array[iPtr + i] += vPtr;
            }
        }

        protected struct PrimitiveInfo
        {
            public PrimitiveType type;
            public int indicesOffset, verticesCount, primitivesCount;

            public PrimitiveInfo(PrimitiveType type, int indicesOffset, int verticesCount, int primitivesCount)
            {
                this.type = type;
                this.indicesOffset = indicesOffset;
                this.verticesCount = verticesCount;
                this.primitivesCount = primitivesCount;
            }

            public void Set(PrimitiveType type, int indicesOffset, int verticesCount, int primitivesCount)
            {
                this.type = type;
                this.indicesOffset = indicesOffset;
                this.verticesCount = verticesCount;
                this.primitivesCount = primitivesCount;
            }
        }
    }
}
