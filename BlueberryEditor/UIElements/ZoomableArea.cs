using BlueberryCore;
using BlueberryCore.SMath;
using BlueberryCore.UI;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlueberryEditor.UIElements
{
    public class ZoomableArea : Group
    {
        private static readonly int[] ZOOM_LEVELS = { 5, 10, 16, 25, 33, 50, 66, 100, 150, 200, 300, 400, 600, 800, 1000 };
        private static readonly int DEFAULT_ZOOM_INDEX = 7;
        private static readonly float SAVE_PADDING = 24f;

        private Element content;
        private int zoomIndex;
        private bool contentMoved;

        public Action<int> OnZoomChanged;

        public ZoomableArea() : this(new DebugArea())
        {

        }

        public ZoomableArea(Element content)
        {
            this.content = content;
            AddElement(content);
            AddListener(new ZoomListener(this));
            contentMoved = false;
        }
        
        private void SetZoomIndex(int index)
        {
            if (content == null)
                return;

            zoomIndex = index;
            int zoomPercent = ZOOM_LEVELS[index];
            float scale = (float)zoomPercent / 100f;
            content.SetScale(scale);
            OnZoomChanged?.Invoke(zoomPercent);
        }

        public override void Draw(Graphics graphics, float parentAlpha)
        {
            ClipBegin(graphics, GetX(), GetY(), GetWidth(), GetHeight());
            base.Draw(graphics, parentAlpha);
            ClipEnd(graphics);
        }

        private void FixContentPosition()
        {
            if (content == null) return;

            float x = content.GetX();
            float y = content.GetY();
            float width = content.GetWidth() * content.GetScaleX();
            float height = content.GetHeight() * content.GetScaleY();
            float availableWidth = GetWidth();
            float availableHeight = GetHeight();

            if (x < -width + SAVE_PADDING) content.SetX(-width + SAVE_PADDING);
            if (x > availableWidth - SAVE_PADDING) content.SetX(availableWidth - SAVE_PADDING);
            if (y < -height + SAVE_PADDING) content.SetY(-height + SAVE_PADDING);
            if (y > availableHeight - SAVE_PADDING) content.SetY(availableHeight - SAVE_PADDING);
        }

        protected override void SizeChanged()
        {
            base.SizeChanged();

            if (!contentMoved)
            {
                FitContentAtCenter();
            }
            FixContentPosition();
        }

        private void FitContentAtCenter()
        {
            if (content == null)
                return;
            if (GetWidth() <= 0f || GetHeight() <= 0f) return;

            for (int i = ZOOM_LEVELS.Length - 1; i >= 0; i--)
            {
                float zoomScale = (float)ZOOM_LEVELS[i] / 100f;
                float width = content.GetWidth() * zoomScale;
                float height = content.GetHeight() * zoomScale;

                if (width <= GetWidth() && height <= GetHeight())
                {
                    SetZoomIndex(i);
                    break;
                }
            }

            content.SetPosition((GetWidth() - content.GetWidth() * content.GetScaleX()) * 0.5f, (GetHeight() - content.GetHeight() * content.GetScaleY()) * 0.5f);
        }

        #region ZoomListener

        private class ZoomListener : InputListener
        {
            private readonly ZoomableArea area;
            private Vector2 lastPosition;

            public ZoomListener(ZoomableArea area)
            {
                this.area = area;
            }

            public override bool TouchDown(InputEvent ev, float x, float y, int pointer, int button)
            {
                area.GetStage().SetScrollFocus(area);
                lastPosition.Set(x, y);
                return true;
            }

            public override void TouchDragged(InputEvent ev, float x, float y, int pointer)
            {
                var content = area.content;
                content.SetPosition(content.GetX() - lastPosition.X + x, content.GetY() - lastPosition.Y + y);
                area.FixContentPosition();
                area.contentMoved = true;
                lastPosition.Set(x, y);
            }

            public override bool Scrolled(InputEvent ev, float x, float y, int amount)
            {
                var content = area.content;

                float preWidth = content.GetWidth() * content.GetScaleX();
                float preHeight = content.GetHeight() * content.GetScaleY();
                float normalizedX = x < content.GetX() ? 0f : (x > content.GetX() + preWidth ? 1f : (x - content.GetX()) / preWidth);
                float normalizedY = y < content.GetY() ? 0f : (y > content.GetY() + preHeight ? 1f : (y - content.GetY()) / preHeight);

                area.SetZoomIndex(SimpleMath.Clamp(area.zoomIndex - amount, 0, ZOOM_LEVELS.Length - 1));

                float postWidth = content.GetWidth() * content.GetScaleX();
                float postHeight = content.GetHeight() * content.GetScaleY();

                content.SetPosition(content.GetX() + (preWidth - postWidth) * normalizedX, content.GetY() + (preHeight - postHeight) * normalizedY);
                area.FixContentPosition();

                return true;
            }
        }

        #endregion

        private class DebugArea : Element
        {
            public DebugArea() : base()
            {
                SetSize(30, 30);
            }

            public override void Draw(Graphics graphics, float parentAlpha)
            {
                base.Draw(graphics, parentAlpha);
                graphics.DrawRectangleBorder(GetX(), GetY(), GetWidth() * GetScaleX(), GetHeight() * GetScaleY());
            }
        }
    }
}
