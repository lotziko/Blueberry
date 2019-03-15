﻿using Blueberry.Monogame;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Blueberry
{
    public class PrimitiveBatch : IDisposable, IBatch
    {
        #region Constants and Fields

        // this constant controls how large the vertices buffer is. Larger buffers will
        // require flushing less often, which can increase performance. However, having
        // buffer that is unnecessarily large will waste memory.
        const int DefaultBufferSize = 500;

        // a block of vertices that calling AddVertex will fill. Flush will draw using
        // this array, and will determine how many primitives to draw from
        // positionInBuffer.
        VertexPositionColor[] vertices = new VertexPositionColor[DefaultBufferSize];

        // keeps track of how many vertices have been added. this value increases until
        // we run out of space in the buffer, at which time Flush is automatically
        // called.
        int positionInBuffer = 0;

        // a basic effect, which contains the shaders that we will use to draw our
        // primitives.
        BasicEffect basicEffect;

        // the device that we will issue draw calls to.
        GraphicsDevice device;

        // this value is set by Begin, and is the type of primitives that we are
        // drawing.
        PrimitiveType primitiveType;

        public PrimitiveType Type
        {
            get => primitiveType;
            set
            {
                if (value != primitiveType)
                {
                    Flush();
                    primitiveType = value;
                    numVertsPerPrimitive = NumVertsPerPrimitive(primitiveType);
                }
            }
        }

        // how many verts does each of these primitives take up? points are 1,
        // lines are 2, and triangles are 3.
        int numVertsPerPrimitive;

        // hasBegun is flipped to true once Begin is called, and is used to make
        // sure users don't call End before Begin is called.
        //bool hasBegun = false;

        //public bool HasBegun => hasBegun;

        bool isDisposed = false;

        #endregion

        #region RasterizerState

        RasterizerState _rasterizerState, _rasterizerStateScissor, _rasterizerStateNoScissor;

        public void SetScissorTest(bool state)
        {
            //if (hasBegun)
              //  End();
            if (state == _rasterizerState.ScissorTestEnable)
                return;
            if (state)
            {
                _rasterizerState = _rasterizerStateScissor;
            }
            else
            {
                _rasterizerState = _rasterizerStateNoScissor;
            }
        }
        
        #endregion

        // the constructor creates a new PrimitiveBatch and sets up all of the internals
        // that PrimitiveBatch will need.
        public PrimitiveBatch(GraphicsDevice graphicsDevice)
        {
            device = graphicsDevice ?? throw new ArgumentNullException("graphicsDevice can't be null");

            // set up a new basic effect, and enable vertex colors.
            basicEffect = new BasicEffect(graphicsDevice);
            basicEffect.VertexColorEnabled = true;

            // projection uses CreateOrthographicOffCenter to create 2d projection
            // matrix with 0,0 in the upper left.
            basicEffect.Projection = Matrix.Identity;
            basicEffect.World = Matrix.Identity;
            basicEffect.View = Matrix.Identity;

            var rState = RasterizerState.CullNone;

            _rasterizerStateNoScissor = new RasterizerState
            {
                CullMode = rState.CullMode,
                DepthBias = rState.DepthBias,
                FillMode = rState.FillMode,
                MultiSampleAntiAlias = rState.MultiSampleAntiAlias,
                SlopeScaleDepthBias = rState.SlopeScaleDepthBias,
                ScissorTestEnable = false
            };
            _rasterizerStateScissor = new RasterizerState
            {
                CullMode = rState.CullMode,
                DepthBias = rState.DepthBias,
                FillMode = rState.FillMode,
                MultiSampleAntiAlias = rState.MultiSampleAntiAlias,
                SlopeScaleDepthBias = rState.SlopeScaleDepthBias,
                ScissorTestEnable = true
            };
            _rasterizerState = _rasterizerStateNoScissor;
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && !isDisposed)
            {
                if (basicEffect != null)
                    basicEffect.Dispose();

                isDisposed = true;
            }
        }

        // Begin is called to tell the PrimitiveBatch what kind of primitives will be
        // drawn, and to prepare the graphics card to render those primitives.
        public void Begin()
        {
            /*if (hasBegun)
            {
                throw new InvalidOperationException
                    ("End must be called before Begin can be called again.");
            }*/

            // these three types reuse vertices, so we can't flush properly without more
            // complex logic. Since that's a bit too complicated for this sample, we'll
            // simply disallow them.
            /*if (primitiveType == PrimitiveType.LineStrip)
            {
                throw new NotSupportedException
                    ("The specified primitiveType is not supported by PrimitiveBatch.");
            }*/

            //tell our basic effect to begin.
            
            basicEffect.CurrentTechnique.Passes[0].Apply();

            // flip the error checking boolean. It's now ok to call AddVertex, Flush,
            // and End.
            //hasBegun = true;
        }

        // AddVertex is called to add another vertex to be rendered. To draw a point,
        // AddVertex must be called once. for lines, twice, and for triangles 3 times.
        // this function can only be called once begin has been called.
        // if there is not enough room in the vertices buffer, Flush is called
        // automatically.
        public void AddVertex(float x, float y, Color color)
        {
            /*if (!hasBegun)
            {
                throw new InvalidOperationException
                    ("Begin must be called before AddVertex can be called.");
            }*/

            // are we starting a new primitive? if so, and there will not be enough room
            // for a whole primitive, flush.
            bool newPrimitive = ((positionInBuffer % numVertsPerPrimitive) == 0);

            if (newPrimitive && (positionInBuffer + numVertsPerPrimitive) >= vertices.Length)
            {
                Flush();
            }

            // once we know there's enough room, set the vertex in the buffer,
            // and increase position.
            vertices[positionInBuffer].Position = new Vector3(x, y, 0);
            vertices[positionInBuffer].Color = color;
            
            positionInBuffer++;
        }

        // End is called once all the primitives have been drawn using AddVertex.
        // it will call Flush to actually submit the draw call to the graphics card, and
        // then tell the basic effect to end.
        public void End()
        {
            /*if (!hasBegun)
            {
                throw new InvalidOperationException
                    ("Begin must be called before End can be called.");
            }*/

            // Draw whatever the user wanted us to draw
            Flush();

            //hasBegun = false;
        }

        // Flush is called to issue the draw call to the graphics card. Once the draw
        // call is made, positionInBuffer is reset, so that AddVertex can start over
        // at the beginning. End will call this to draw the primitives that the user
        // requested, and AddVertex will call this if there is not enough room in the
        // buffer.
        public void Flush()
        {
            /*if (!hasBegun)
            {
                throw new InvalidOperationException
                    ("Begin must be called before Flush can be called.");
            }*/

            // no work to do
            if (positionInBuffer == 0)
            {
                return;
            }

            // how many primitives will we draw?
            int primitiveCount;
            switch (primitiveType)
            {
                case PrimitiveType.LineStrip:
                    primitiveCount = (positionInBuffer - 1);
                    break;
                case PrimitiveType.TriangleStrip:
                    primitiveCount = (positionInBuffer - 2);
                    break;
                default:
                    primitiveCount = positionInBuffer / numVertsPerPrimitive;
                    break;
            }

            if (primitiveCount <= 0)
                return;
            
            device.RasterizerState = _rasterizerState;
            device.BlendState = BlendState.NonPremultiplied;
            device.DepthStencilState = DepthStencilState.DepthRead;
            device.DrawUserPrimitives(primitiveType, vertices, 0, primitiveCount);
            device.DepthStencilState = DepthStencilState.Default;
            device.BlendState = BlendState.AlphaBlend;
            device.RasterizerState = RasterizerState.CullClockwise;

            // now that we've drawn, it's ok to reset positionInBuffer back to zero,
            // and write over any vertices that may have been set previously.
            positionInBuffer = 0;
        }

        public Mat Transform
        {
            get
            {
                return default;
            }
            set
            {
                basicEffect.View = value.m;
            }
        }

        public Mat Projection
        {
            get
            {
                return default;
            }
            set
            {
                basicEffect.Projection = value.m;
            }
        }

        /*#region Drawing functions

        public void DrawLine(float x1, float y1, float x2, float y2, Color color)
        {
            if (hasBegun)
                End();
            Begin(PrimitiveType.LineList);
            AddVertex(x1, y1, color);
            AddVertex(x2, y2, color);
            End();
        }

        public void DrawLine(float x1, float y1, float x2, float y2)
        {
            DrawLine(x1, y1, x2, y2, Color.Black);
        }

        public void DrawPoint(float x, float y, Color color)
        {
            if (hasBegun)
                End();
            Begin(PrimitiveType.LineList);
            AddVertex(x, y, color);
            AddVertex(x, y + 1, color);
            End();
        }

        public void DrawPoint(float x, float y)
        {
            DrawPoint(x, y, Color.Black);
        }

        public void DrawRectangle(float x, float y, float width, float height, Color color)
        {
            if (hasBegun)
                End();
            Begin(PrimitiveType.TriangleStrip);
            AddVertex(x, y, color);
            AddVertex(x + width, y, color);
            AddVertex(x, y + height, color);
            AddVertex(x + width, y + height, color);
            End();
        }

        public void DrawRectangle(float x, float y, float width, float height)
        {
            DrawRectangle(x, y, width, height, Color.Black);
        }

        public void DrawRectangleBorder(float x, float y, float width, float height, Color color)
        {
            if (hasBegun)
                End();
            Begin(PrimitiveType.LineStrip);
            AddVertex(x, y, color);
            AddVertex(x + width, y, color);
            AddVertex(x + width, y + height, color);
            AddVertex(x, y + height, color);
            AddVertex(x, y, color);
            End();
        }

        public void DrawRectangleBorder(float x, float y, float width, float height)
        {
            DrawRectangleBorder(x, y, width, height, Color.Black);
        }

        public void DrawQuadrangle(float x1, float y1, float x2, float y2, float x3, float y3, float x4, float y4, Color color)
        {
            if (hasBegun)
                End();
            Begin(PrimitiveType.TriangleStrip);
            AddVertex(x1, y1, color);
            AddVertex(x2, y2, color);
            AddVertex(x4, y4, color);
            AddVertex(x3, y3, color);
            End();
        }

        public void DrawQuadrangle(float x1, float y1, float x2, float y2, float x3, float y3, float x4, float y4)
        {
            DrawQuadrangle(x1, y1, x2, y2, x3, y3, x4, y4, Color.Black);
        }

        public void DrawTriangle(float x1, float y1, float x2, float y2, float x3, float y3, Color color)
        {
            if (hasBegun)
                End();
            Begin(PrimitiveType.TriangleList);
            AddVertex(x1, y1, color);
            AddVertex(x2, y2, color);
            AddVertex(x3, y3, color);
            End();
        }

        public void DrawTriangle(float x1, float y1, float x2, float y2, float x3, float y3)
        {
            DrawTriangle(x1, y1, x2, y2, x3, y3, Color.Black);
        }

        public void DrawCircle(float xc, float yc, float radius, Color color)
        {
            if (radius < 0)
                radius = -radius;

            float x = 0;
            float y = radius;
            float d = (5 - radius * 4) / 4;
            float previous = y;
            do
            {
                if (y != previous)
                {
                    if (x != y)
                    {
                        DrawLine(xc - x, yc + y, xc + x, yc + y, color);
                        DrawLine(xc - x, yc - y, xc + x, yc - y, color);
                    }
                    previous = y;
                }
                if (x != 0)
                    DrawLine(xc - y, yc + x, xc + y, yc + x, color);
                DrawLine(xc - y, yc - x, xc + y, yc - x, color);

                if (d < 0)
                {
                    d += 2 * x + 1;
                }
                else
                {
                    d += 2 * (x - y) + 1;
                    y--;
                }
                x++;
            }
            while (x <= y);
        }

        #endregion*/

        #region Helper functions

        // NumVertsPerPrimitive is a boring helper function that tells how many vertices
        // it will take to draw each kind of primitive.
        static private int NumVertsPerPrimitive(PrimitiveType primitive)
        {
            int numVertsPerPrimitive;
            switch (primitive)
            {
                case PrimitiveType.LineList:
                    numVertsPerPrimitive = 2;
                    break;
                case PrimitiveType.LineStrip:
                    numVertsPerPrimitive = 2;
                    break;
                case PrimitiveType.TriangleList:
                    numVertsPerPrimitive = 3;
                    break;
                case PrimitiveType.TriangleStrip:
                    numVertsPerPrimitive = 3;
                    break;
                default:
                    throw new InvalidOperationException("primitive is not valid");
            }
            return numVertsPerPrimitive;
        }

        #endregion
    }
}