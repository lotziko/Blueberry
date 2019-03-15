﻿using OpenTK;

namespace Blueberry.OpenGL
{
    public class SpriteEffect : Effect
    {
        private EffectUniform matrixUniform;
        private Matrix4 transform = Matrix4.Identity;

        public Matrix4 Transform
        {
            get => transform;
            set
            {
                transform = value;
            }
        }

        public SpriteEffect(GraphicsDevice device) : base(device, EffectResource.SpriteEffect.Vertex, EffectResource.SpriteEffect.Fragment)
        {
            matrixUniform = Uniforms["TransformMatrix"];
        }

        public override void Apply()
        {
            base.Apply();

            var viewport = GraphicsDevice.Viewport;

            var projection = Matrix4.CreateOrthographicOffCenter(0, viewport.Width, viewport.Height, 0, -1, 1);

            matrixUniform.SetValue(transform * projection);
        }
    }
}
