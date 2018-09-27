using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BlueberryCore
{
    public class SpriteBatch : Microsoft.Xna.Framework.Graphics.SpriteBatch
    {
        SpriteSortMode _sortMode;
        BlendState _blendState;
        SamplerState _samplerState;
        DepthStencilState _depthStencilState;
        RasterizerState _rasterizerState, _rasterizerStateScissor, _rasterizerStateNoScissor;
        Effect _effect;
        Matrix? _transformMatrix = null;

        public bool HasBegun { get; private set; } = false;

        GraphicsDevice _graphicsDevice;

        public Matrix TransformMatrix
        {
            get
            {
                return _transformMatrix.HasValue ? (Matrix)_transformMatrix : default(Matrix);
            }
            set
            {
                _transformMatrix = value;
                Flush();
            }
        }

        public void SetSortMode(SpriteSortMode sortMode) => _sortMode = sortMode;
        public void SetBlendState(BlendState blendState) => _blendState = blendState;
        public void SetSamplerState(SamplerState samplerState) => _samplerState = samplerState;
        public void SetDepthStencilState(DepthStencilState depthStencilState) => _depthStencilState = depthStencilState;
        public void SetRasterizerState(RasterizerState rasterizerState) => _rasterizerState = rasterizerState;

        public void SetScissorTest(bool state)
        {
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
            Flush();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            _rasterizerStateScissor.Dispose();
            _rasterizerStateNoScissor.Dispose();
        }

        public void SetShader(Effect effect)
        {
            _effect = effect;
            Flush();
        }

        public void ResetShader()
        {
            _effect = null;
            Flush();
        }

        /// <summary>
        /// Begin with previous settings
        /// </summary>

        public void Begin()
        {
            Begin(_sortMode, _blendState, _samplerState, _depthStencilState, _rasterizerState, _effect, _transformMatrix);
            HasBegun = true;
        }

        public new void End()
        {
            base.End();
            HasBegun = false;
        }

        public new void Begin
        (
            SpriteSortMode sortMode = SpriteSortMode.Deferred,
            BlendState blendState = null,
            SamplerState samplerState = null,
            DepthStencilState depthStencilState = null,
            RasterizerState rasterizerState = null,
            Effect effect = null,
            Matrix? transformMatrix = null
        )
        {
            base.Begin(sortMode = SpriteSortMode.Deferred, blendState ?? BlendState.AlphaBlend, samplerState ?? SamplerState.PointClamp, depthStencilState, rasterizerState, effect, transformMatrix);
            _sortMode = sortMode;
            _blendState = blendState;
            _samplerState = samplerState;
            _depthStencilState = depthStencilState;
            _rasterizerState = rasterizerState;
            _effect = effect;
            _transformMatrix = transformMatrix;
            HasBegun = true;
        }

        public void Flush()
        {
            if (HasBegun)
            {
                End();
                Begin();
            }
            else
            {
                Begin();
                End();
            }

        }

        public SpriteBatch(GraphicsDevice graphicsDevice) : base(graphicsDevice)
        {
            _graphicsDevice = graphicsDevice;
            var rState = _graphicsDevice.RasterizerState;
            
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
            //ScissorsStack.Initialize(graphicsDevice);
        }
    }
}