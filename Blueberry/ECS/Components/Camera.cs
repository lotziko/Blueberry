using System;
using System.Collections.Generic;
using System.Text;

namespace Blueberry
{
    public partial class Camera : Component
    {
        private Mat _transformMatrix, _inverseTransformMatrix, _projectionMatrix, _combinedMatrix, _inverseCombinedMatrix;

        protected int _width, _height;

        public Vec3 Position;

        public Mat TransformMatrix { get { return _transformMatrix; } }
        public Mat ProjectionMatrix { get { return _projectionMatrix; } }

        public bool Round { get; set; }
        public bool FlipY { get; set; } = true;
        private bool isCalculated = false;

        public int Width
        {
            get
            {
                return _width;
            }
            set
            {
                _width = value;
                isCalculated = false;
            }
        }
        public int Height
        {
            get
            {
                return _height;
            }
            set
            {
                _height = value;
                isCalculated = false;
            }
        }

        public void SetSize(int width, int height)
        {
            Width = width;
            Height = height;
            isCalculated = false;
        }

        public void SetPosition(float x, float y)
        {
            Position.X = x;
            Position.Y = y;
            isCalculated = false;
        }

        public void SetPosition(Vec2 position)
        {
            Position.X = position.X;
            Position.Y = position.Y;
            isCalculated = false;
        }

        public void ForceCalculate()
        {
            isCalculated = false;
            CalculateMatrixes();
        }

        //public void CalculateMatrixes();

        #region Transforms

        public Vec2 WorldToScreenPoint(Vec2 worldPosition)
        {
            CalculateMatrixes();
            worldPosition = Vec2.Transform(worldPosition, _transformMatrix);
            return worldPosition;
        }

        public Vec2 ScreenToWorldPoint(Vec2 screenPosition)
        {
            CalculateMatrixes();
            screenPosition = Vec2.Transform(screenPosition, _inverseTransformMatrix);
            return screenPosition;
        }

        public void ScreenToWorldPoint(ref float x, ref float y)
        {
            CalculateMatrixes();
            Mat.Transform(ref x, ref y, _inverseTransformMatrix);
        }

        #endregion

        public Camera()
        {
            
        }
    }
}
