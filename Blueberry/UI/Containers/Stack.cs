using System;
using System.Collections.Generic;
using System.Text;

namespace Blueberry.UI
{
    public class Stack : Group
    {
        private float prefWidth, prefHeight, minWidth, minHeight, maxWidth, maxHeight;
        private bool sizeInvalid = true;

        public Stack()
        {
            SetTransform(false);
            SetSize(150, 150);
            SetTouchable(Touchable.ChildrenOnly);
        }

        public Stack(params Element[] elements) : this()
        {
            foreach (Element e in elements)
                AddElement(e);
        }

        #region ILayout

        public override void Invalidate()
        {
            base.Invalidate();
            sizeInvalid = true;
        }

        public override void Layout()
        {
            if (sizeInvalid) ComputeSize();
            float width = GetWidth(), height = GetHeight();
            var elements = GetElements();
            for (int i = 0, n = elements.Count; i < n; i++)
            {
                var e = elements[i];
                e.SetBounds(0, 0, width, height);
                if (e is ILayout) (e as ILayout).Validate();
            }
        }

        public override float PreferredWidth
        {
            get
            {
                if (sizeInvalid) ComputeSize();
                return prefWidth;
            }
        }

        public override float PreferredHeight
        {
            get
            {
                if (sizeInvalid) ComputeSize();
                return prefHeight;
            }
        }

        public override float MinWidth
        {
            get
            {
                if (sizeInvalid) ComputeSize();
                return minWidth;
            }
        }

        public override float MinHeight
        {
            get
            {
                if (sizeInvalid) ComputeSize();
                return minHeight;
            }
        }

        public override float MaxWidth
        {
            get
            {
                if (sizeInvalid) ComputeSize();
                return maxWidth;
            }
        }

        public override float MaxHeight
        {
            get
            {
                if (sizeInvalid) ComputeSize();
                return maxHeight;
            }
        }

        #endregion

        protected void ComputeSize()
        {
            sizeInvalid = false;
            prefWidth = 0;
            prefHeight = 0;
            minWidth = 0;
            minHeight = 0;
            maxWidth = 0;
            maxHeight = 0;

            var elements = GetElements();
            for (int i = 0, n = elements.Count; i < n; i++)
            {
                var e = elements[i];
                float elementMaxWidth, elementMaxHeight;
                if (e is ILayout)
                {
                    var layout = e as ILayout;
                    prefWidth = Math.Max(prefWidth, layout.PreferredWidth);
                    prefHeight = Math.Max(prefHeight, layout.PreferredHeight);
                    minWidth = Math.Max(minWidth, layout.MinWidth);
                    minHeight = Math.Max(minHeight, layout.MinHeight);
                    elementMaxWidth = layout.MaxWidth;
                    elementMaxHeight = layout.MaxHeight;
                }
                else
                {
                    prefWidth = Math.Max(prefWidth, e.GetWidth());
                    prefHeight = Math.Max(prefHeight, e.GetHeight());
                    minWidth = Math.Max(minWidth, e.GetWidth());
                    minHeight = Math.Max(minHeight, e.GetHeight());
                    elementMaxWidth = 0;
                    elementMaxHeight = 0;
                }
                if (elementMaxWidth > 0) maxWidth = maxWidth == 0 ? elementMaxWidth : Math.Min(maxWidth, elementMaxWidth);
                if (elementMaxHeight > 0) maxHeight = maxHeight == 0 ? elementMaxHeight : Math.Min(maxHeight, elementMaxHeight);
            }
        }

        public void Add(Element element)
        {
            AddElement(element);
        }
    }
}
