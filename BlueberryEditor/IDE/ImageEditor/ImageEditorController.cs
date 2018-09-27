using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace BlueberryEditor.IDE.ImageEditor
{
    public class ImageEditorController
    {
        protected Texture2D currentTexture;
        protected Color firstColor, secondColor;
        private List<Action> actionsLog;
        private ImageEditorSelection currentSelection;

        public Texture2D GetTexture() => currentTexture;

        public ImageEditorController(Texture2D texture)
        {
            currentTexture = texture;
        }

        public ImageEditorController(SpriteResource resource)
        {
            
        }
    }
}
