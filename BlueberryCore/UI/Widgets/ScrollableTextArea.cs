using Microsoft.Xna.Framework;
using System;

namespace BlueberryCore.UI
{
    public class ScrollableTextArea : TextArea, ICullable
    {
        private Rectangle cullingArea;

        public ScrollableTextArea(string text, TextFieldStyle style) : base(text, style)
        {
        }

        public ScrollableTextArea(string text, Skin skin, string stylename = "default") : base(text, skin, stylename)
        {
        }
        
        protected override InputListener CreateListener()
        {
            return new ScrollTextAreaListener(this);
        }

        public override void SetParent(Group parent)
        {
            base.SetParent(parent);
            if (parent is ScrollPane) {
                CalculateOffsets();
            }
        }

        private void UpdateScrollPosition()
        {
            if (cullingArea == null || GetParent() is ScrollPane == false) return;
            ScrollPane scrollPane = (ScrollPane)GetParent();

            if (cullingArea.Contains(GetCursorX(), cullingArea.Y) == false)
            {
                scrollPane.SetScrollPercentX(GetCursorX() / GetWidth());
            }

            if (cullingArea.Contains(cullingArea.X, (/*GetHeight() - */GetCursorY())) == false)
            {
                scrollPane.SetScrollPercentY(GetCursorY() / GetHeight());
            }
        }

        public void SetCullingArea(Rectangle cullingArea)
        {
            this.cullingArea = cullingArea;
        }

        public ScrollPane CreateCompatibleScrollPane(Skin skin)
        {
            var scrollPane = new ScrollPane(this, skin);
            scrollPane.SetOverscroll(false, false);
            scrollPane.SetFlickScroll(false);
            scrollPane.SetFadeScrollBars(false);
            scrollPane.SetScrollbarsOnTop(true);
            scrollPane.SetScrollingDisabled(true, false);
            return scrollPane;
        }

        protected override void SizeChanged()
        {
            base.SizeChanged();
            linesShowing = 1000000000; //aka a lot, forces text area not to use its stupid 'scrolling'
        }

        #region ILayout

        public override float PreferredHeight
        {
            get
            {
                return GetLines() * style.font.lineHeight;
            }
        }

        #endregion

        public override void SetText(string str/*, bool triggerEvent = true*/)
        {
            base.SetText(str/*, triggerEvent*/);
            if (programmaticChangeEvents == false)
            { //changeText WILL NOT be called when programmaticChangeEvents are disabled
                UpdateScrollLayout();
            }
        }

        protected override bool ChangeText(string oldText, string newText)
        {
            bool changed = base.ChangeText(oldText, newText);
            UpdateScrollLayout();
            return changed;
        }

        void UpdateScrollLayout()
        {
            InvalidateHierarchy();
            Layout();
            if (GetParent() is ScrollPane) ((ScrollPane)GetParent()).Layout();
            UpdateScrollPosition();
        }

        public class ScrollTextAreaListener : TextAreaListener
        {
            public ScrollTextAreaListener(ScrollableTextArea t) : base(t)
            {
            }

            public override bool KeyDown(InputEvent ev, int keycode)
            {
                var t = this.t as ScrollableTextArea;
                t.UpdateScrollPosition();
                return base.KeyDown(ev, keycode);
            }

            public override bool KeyTyped(InputEvent ev, int keycode, char character)
            {
                var t = this.t as ScrollableTextArea;
                t.UpdateScrollPosition();
                return base.KeyTyped(ev, keycode, character);
            }
        }
    }
}
