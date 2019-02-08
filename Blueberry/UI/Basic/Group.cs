using System;
using System.Collections.Generic;
using System.Text;

namespace Blueberry.UI
{
    public partial class Group : Element, ICullable
    {
        internal List<Element> elements = new List<Element>();
        protected bool transform = true;
        Mat previousBatcherTransform;
        Rect? cullingArea;

        public override void Update(float delta)
        {
            base.Update(delta);
            lock (elements)
                for (int i = 0, n = elements.Count; i < n; i++)
                    elements[i].Update(delta);
        }

        public override void SetStage(Stage stage)
        {
            for (int i = 0; i < elements.Count; i++)
            {
                elements[i].SetStage(stage);
            }
            base.SetStage(stage);
        }

        public virtual T AddElement<T>(T element) where T : Element
        {
            if (element == null)
                return null;
            if (element.parent != null)
                element.parent.RemoveElement(element);

            elements.Add(element);
            element.SetParent(this);
            element.SetStage(stage);
            OnChildrenChanged();

            return element;
        }

        public T InsertElement<T>(int index, T element) where T : Element
        {
            if (element.parent != null)
                element.parent.RemoveElement(element);

            if (index >= elements.Count)
                return AddElement(element);

            elements.Insert(index, element);
            element.SetParent(this);
            element.SetStage(stage);
            OnChildrenChanged();

            return element;
        }

        public virtual bool RemoveElement(Element element)
        {
            if (!elements.Contains(element))
                return false;

            element.parent = null;
            elements.Remove(element);
            OnChildrenChanged();
            return true;
        }

        public List<Element> GetElements()
        {
            return elements;
        }

        public bool IsTransform() => transform;

        public void SetTransform(bool transform) => this.transform = transform;

        /// <summary>
        /// Removes all children
        /// </summary>
        public override void Clear()
        {
            base.Clear();
            ClearElements();
        }

        /// <summary>
        /// Removes all elements from this group
        /// </summary>
        public virtual void ClearElements()
        {
            for (var i = 0; i < elements.Count; i++)
            {
                elements[i].parent = null;
                elements[i].SetStage(null);
            }


            elements.Clear();
            OnChildrenChanged();
        }

        protected virtual void OnChildrenChanged()
        {
            InvalidateHierarchy();
        }

        public override Element Hit(float x, float y, bool touchable = true)
        {
            if (this.touchable == Touchable.Disabled)
                return null;

            float tmpX, tmpY;

            for (var i = elements.Count - 1; i >= 0; i--)
            {
                var child = elements[i];

                tmpX = x;
                tmpY = y;

                if (!child.IsVisible())
                    continue;

                child.ParentToLocalCoordinates(ref tmpX, ref tmpY);
                var hit = child.Hit(tmpX, tmpY, touchable);
                if (hit != null)
                    return hit;
            }

            return base.Hit(x, y, touchable);
        }

        #region Culling

        /** Children completely outside of this rectangle will not be drawn. This is only valid for use with unrotated and unscaled
	     * actors.
	     * @param cullingArea May be null. */
        public virtual void SetCullingArea(Rect cullingArea)
        {
            this.cullingArea = cullingArea;
        }

        /** @return May be null.
         * @see #setCullingArea(Rectangle) */
        public virtual Rect GetCullingArea()
        {
            return cullingArea.Value;
        }

        #endregion

        /** If true, {@link #drawDebug(ShapeRenderer)} will be called for this group and, optionally, all children recursively. */
        public void SetDebug(bool enabled, bool recursively)
        {
            SetDebug(enabled);
            if (recursively)
            {
                foreach (var element in elements)
                {
                    if (element is Group)
                    {
                        ((Group)element).SetDebug(enabled, recursively);
                    }
                    else
                    {
                        element.SetDebug(enabled);
                    }
                }
            }
        }

        /** Calls {@link #setDebug(boolean, boolean)} with {@code true, true}. */
        public Group DebugAll()
        {
            SetDebug(true, true);
            return this;
        }


        public override void Draw(Graphics graphics, float parentAlpha)
        {
            if (!IsVisible())
                return;

            Validate();

            if (transform)
                ApplyTransform(graphics, ComputeTransform());

            DrawElements(graphics, parentAlpha);

            if (transform)
                ResetTransform(graphics);
        }

        public void DrawElements(Graphics graphics, float parentAlpha)
        {
            parentAlpha *= color.A;

            if (cullingArea.HasValue)
            {
                float cullLeft = cullingArea.Value.X;
                float cullRight = cullLeft + cullingArea.Value.Width;
                float cullBottom = cullingArea.Value.Y;
                float cullTop = cullBottom + cullingArea.Value.Height;

                if (transform)
                {
                    for (int i = 0, n = elements.Count; i < n; i++)
                    {
                        var child = elements[i];
                        if (!child.IsVisible()) continue;
                        float cx = child.x, cy = child.y;
                        if (cx <= cullRight && cy <= cullTop && cx + child.width >= cullLeft &&
                            cy + child.height >= cullBottom)
                        {
                            child.Draw(graphics, parentAlpha);
                        }
                    }
                }
                else
                {
                    float offsetX = x, offsetY = y;
                    x = 0;
                    y = 0;
                    for (int i = 0, n = elements.Count; i < n; i++)
                    {
                        var child = elements[i];
                        if (!child.IsVisible()) continue;
                        float cx = child.x, cy = child.y;
                        if (cx <= cullRight && cy <= cullTop && cx + child.width >= cullLeft &&
                            cy + child.height >= cullBottom)
                        {
                            child.x = cx + offsetX;
                            child.y = cy + offsetY;
                            child.Draw(graphics, parentAlpha);
                            child.x = cx;
                            child.y = cy;
                        }
                    }
                    x = offsetX;
                    y = offsetY;
                }
            }
            else
            {
                // No culling, draw all children.
                if (transform)
                {
                    for (int i = 0, n = elements.Count; i < n; i++)
                    {
                        var child = elements[i];
                        if (!child.IsVisible()) continue;
                        child.Draw(graphics, parentAlpha);
                    }
                }
                else
                {
                    // No transform for this group, offset each child.
                    float offsetX = x, offsetY = y;
                    x = 0;
                    y = 0;
                    for (int i = 0, n = elements.Count; i < n; i++)
                    {
                        var child = elements[i];
                        if (!child.IsVisible()) continue;
                        float cx = child.x, cy = child.y;
                        child.x = cx + offsetX;
                        child.y = cy + offsetY;
                        child.Draw(graphics, parentAlpha);
                        child.x = cx;
                        child.y = cy;
                    }
                    x = offsetX;
                    y = offsetY;
                }
            }
        }

        public override void DrawDebug(Graphics graphics)
        {
            if (transform)
                ApplyTransform(graphics, ComputeTransform());

            DrawDebugElements(graphics, 1f);

            if (transform)
                ResetTransform(graphics);
        }

        public void DrawDebugElements(Graphics graphics, float parentAlpha)
        {
            parentAlpha *= color.A;
            if (transform)
            {
                for (var i = 0; i < elements.Count; i++)
                {
                    if (!elements[i].IsVisible())
                        continue;

                    if (!elements[i].GetDebug() && !(elements[i] is Group))
                        continue;

                    elements[i].DrawDebug(graphics);
                }
            }
            else
            {
                // No transform for this group, offset each child.
                float offsetX = x, offsetY = y;
                x = 0;
                y = 0;
                for (var i = 0; i < elements.Count; i++)
                {
                    if (!elements[i].IsVisible())
                        continue;

                    if (!elements[i].GetDebug() && !(elements[i] is Group))
                        continue;

                    elements[i].x += offsetX;
                    elements[i].y += offsetY;
                    elements[i].DrawDebug(graphics);
                    elements[i].x -= offsetX;
                    elements[i].y -= offsetY;
                }
                x = offsetX;
                y = offsetY;
            }
        }
    }
}
