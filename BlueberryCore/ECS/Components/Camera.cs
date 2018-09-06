using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BlueberryCore
{
    public class Camera : Component
    {
        private Matrix _transformMatrix = Matrix.Identity, _inverseTransformMatrix = Matrix.Identity, _projectionMatrix = Matrix.Identity;

        protected int _width, _height;

        public Vector3 Position;
        
        public Matrix TransformMatrix { get { return _transformMatrix; } }

        public bool Round { get; set; }
        private bool isCalculated = false;

        public int Width
        {
            get
            {
                return _width;
            }
            private set
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
            private set
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

        public void SetPosition(int x, int y)
        {
            Position.X = x;
            Position.Y = y;
            isCalculated = false;
        }

        public void SetPosition(Vector2 position)
        {
            Position.X = position.X;
            Position.Y = position.Y;
            isCalculated = false;
        }

        public void CalculateMatrixes()
        {
            if (isCalculated)
                return;

            float bufferWidth = 0, bufferHeight = 0;

            if (Core.graphicsDevice.RenderTargetCount > 0)
            {
                var renderTarget = (RenderTarget2D)Core.graphicsDevice.GetRenderTargets()[0].RenderTarget;
                bufferWidth = renderTarget.Bounds.Width;
                bufferHeight = renderTarget.Bounds.Height;
            }
            else
            {
                bufferWidth = Core.graphicsDevice.Viewport.Width;
                bufferHeight = Core.graphicsDevice.Viewport.Height;
            }

            _transformMatrix = Matrix.CreateTranslation(Round ? new Vector3(-(int)Position.X, -(int)Position.Y, 0) : new Vector3(-Position.X, -Position.Y, 0)) *
                Matrix.CreateTranslation(new Vector3(_width * 0.5f, _height * 0.5f, 0)) *
                Matrix.CreateScale(bufferWidth / _width, bufferHeight / _height, 1);
            _projectionMatrix = Matrix.CreateOrthographicOffCenter(0, bufferWidth, bufferHeight, 0, 0, 1);
            _inverseTransformMatrix = Matrix.Invert(_transformMatrix);

             isCalculated = true;
        }

        public void ForceCalculate()
        {
            isCalculated = false;
            CalculateMatrixes();
        }

        #region Transforms

		public Vector2 WorldToScreenPoint(Vector2 worldPosition)
        {
            CalculateMatrixes();
            worldPosition = Vector2.Transform(worldPosition, _transformMatrix);
            return worldPosition;
        }

        public Vector2 ScreenToWorldPoint(Vector2 screenPosition)
        {
            CalculateMatrixes();
            screenPosition = Vector2.Transform(screenPosition, _inverseTransformMatrix);
            return screenPosition;
        }
        
        public Vector2 ScreenToWorldPoint(Point screenPosition)
        {
            return ScreenToWorldPoint(screenPosition.ToVector2());
        }

        #endregion

        public Camera()
        {
            CalculateMatrixes();
        }
    }
}
