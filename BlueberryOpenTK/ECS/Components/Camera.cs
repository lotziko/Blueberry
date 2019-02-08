using OpenTK;
using System;

namespace Blueberry
{
    public partial class Camera : Component
    {
        private static Vec2 tmp;

        public void CalculateMatrixes()
        {
            if (isCalculated)
                return;

            float bufferWidth = 0, bufferHeight = 0;

            bufferWidth = _width;//Graphics.CurrentTargetWidth;
            bufferHeight = _height;//Graphics.CurrentTargetHeigth;

            tmp.Set(Position.X - _width * 0.5f, Position.Y - _height * 0.5f);

            _transformMatrix.m = Matrix4.LookAt(tmp.X, tmp.Y, 0, tmp.X, tmp.Y, -1, 0, 1, 0);
            //_transformMatrix.m = Matrix4.CreateTranslation(Round ? new Vector3(-(int)Position.X, -(int)Position.Y, 0) : new Vector3(-Position.X, -Position.Y, 0));
            //_transformMatrix.m *= Matrix4.CreateTranslation(new Vector3(_width * 0.5f, _height * 0.5f, 0));
            //_transformMatrix.m *= Matrix4.CreateScale(bufferWidth / _width, bufferHeight / _height, 1);
            _projectionMatrix.m = Graphics.IsDrawingToFramebuffer ? Matrix4.CreateOrthographicOffCenter(0, bufferWidth, bufferHeight, 0, -1, 1) : Matrix4.CreateOrthographicOffCenter(0, bufferWidth, 0, bufferHeight, -1, 1);
            _inverseTransformMatrix.m = Matrix4.Invert(_transformMatrix.m);
            _combinedMatrix.m = Matrix4.Mult(_transformMatrix.m, _projectionMatrix.m);
            _inverseCombinedMatrix.m = Matrix4.Invert(_combinedMatrix.m);

            isCalculated = true;
        }

        public Vec2 Project(Vec2 worldCoords, float viewportX, float viewportY, float viewportWidth, float viewportHeight)
        {
            worldCoords = Vec2.Transform(worldCoords, _combinedMatrix);

            worldCoords.X = (float)Math.Round(viewportWidth * (worldCoords.X + 1) / 2 + viewportX, 3);
            worldCoords.Y = (float)Math.Round(Screen.Height - (viewportHeight * (worldCoords.Y + 1) / 2 + viewportY), 3);

            return worldCoords;
        }

        public Vec2 Unproject(Vec2 screenCoords, float viewportX, float viewportY, float viewportWidth, float viewportHeight)
        {
            float x = screenCoords.X - viewportX;
            float y = screenCoords.Y;
            y = Screen.Height - y - 1;
            y = y - viewportY;

            screenCoords.X = (2 * x) / viewportWidth - 1;
            screenCoords.Y = (2 * y) / viewportHeight - 1;

            screenCoords = Vec2.Transform(screenCoords, _inverseCombinedMatrix);

            screenCoords.X = (float)Math.Round(screenCoords.X, 3);
            screenCoords.Y = (float)Math.Round(screenCoords.Y, 3);

            return screenCoords;
        }
    }
}
