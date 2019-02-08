﻿using Microsoft.Xna.Framework;

namespace Blueberry
{
    public partial class Camera : Component
    {
        private static Vector2 tmp;

        public void CalculateMatrixes()
        {
            if (isCalculated)
                return;

            float bufferWidth = 0, bufferHeight = 0;

            bufferWidth = Graphics.CurrentTargetWidth;
            bufferHeight = Graphics.CurrentTargetHeigth;

            _transformMatrix.m = Matrix.CreateTranslation(Round ? new Vector3(-(int)Position.X, -(int)Position.Y, 0) : new Vector3(-Position.X, -Position.Y, 0)) *
                Matrix.CreateTranslation(new Vector3(_width * 0.5f, _height * 0.5f, 0)) *
                Matrix.CreateScale(bufferWidth / _width, bufferHeight / _height, 1);
            _projectionMatrix.m = Matrix.CreateOrthographicOffCenter(0, bufferWidth, bufferHeight, 0, 0, 1);
            _inverseTransformMatrix.m = Matrix.Invert(_transformMatrix.m);

            isCalculated = true;
        }

        public void Unproject(ref float screenX, ref float screenY)
        {
            throw new System.NotImplementedException();
        }

        public Vec2 Unproject(Vec2 worldCoords, float viewportX, float viewportY, float viewportWidth, float viewportHeight)
        {
            throw new System.NotImplementedException();
        }

        public void Project(ref float worldX, ref float worldY)
        {
            throw new System.NotImplementedException();
        }

        public Vec2 Project(Vec2 worldCoords, float viewportX, float viewportY, float viewportWidth, float viewportHeight)
        {
            tmp.X = worldCoords.X;
            tmp.Y = worldCoords.Y;
            tmp = Vector2.Transform(tmp, Matrix.Multiply(ProjectionMatrix.m, TransformMatrix.m));
            worldCoords.Set(tmp.X, tmp.Y);
            worldCoords.X = viewportWidth * (worldCoords.X + 1) / 2 + viewportX;
            worldCoords.Y = viewportHeight * (worldCoords.Y + 1) / 2 + viewportY;
            return worldCoords;
        }
    }
}