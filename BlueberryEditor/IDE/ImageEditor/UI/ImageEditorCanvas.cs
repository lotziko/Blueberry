using BlueberryCore;
using BlueberryCore.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlueberryEditor.IDE.ImageEditor
{
    public class ImageEditorCanvas : Element
    {
        private static Rectangle textureBounds = Rectangle.Empty;

        protected ImageEditorController controller;
        
        protected Texture2D CurrentTexture
        {
            get
            {
                return controller?.GetTexture();
            }
        }

        public ImageEditorCanvas(ImageEditorController controller) : base()
        {
            this.controller = controller;
            SetSize(PreferredWidth, PreferredHeight);
        }


        protected override void PositionChanged()
        {
            base.PositionChanged();
            textureBounds.X = (int)GetX();
            textureBounds.Y = (int)GetY();
        }

        protected override void ScaleChanged()
        {
            base.ScaleChanged();
            textureBounds.Width = (int)(CurrentTexture?.Width * GetScaleX());
            textureBounds.Height = (int)(CurrentTexture?.Height * GetScaleY());
        }

        public override void Draw(Graphics graphics, float parentAlpha)
        {
            base.Draw(graphics, parentAlpha);
            graphics.Draw(CurrentTexture, Vector2.Zero, null, null, null, textureBounds);
            graphics.DrawRectangleBorder(textureBounds.X, textureBounds.Y, textureBounds.Width, textureBounds.Height, Color.White);
        }

        #region ILayout

        public override float PreferredWidth
        {
            get { return CurrentTexture != null ? CurrentTexture.Width : 0; }
        }

        public override float PreferredHeight
        {
            get { return CurrentTexture != null ? CurrentTexture.Height : 0; }
        }

        #endregion
    }
}
