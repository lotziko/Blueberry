using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace BlueberryCore.UI
{
    public class Group : Element, ICullable
    {
        internal List<Element> elements = new List<Element>();
        protected bool transform = true;
        Matrix previousBatcherTransform;
        Rectangle? cullingArea;

        public override void Update(float delta)
        {
            base.Update(delta);
            lock(elements)
                for (int i = 0, n = elements.Count; i < n; i++)
                    elements[i].Update(delta);
        }

        public override void SetStage(Stage stage)
        {
            for(int i = 0; i < elements.Count; i++)
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

        public bool IsTransform()
        {
            return transform;
        }

        public void SetTransform(bool transform)
        {
            this.transform = transform;
        }

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

        public override Element Hit(Vector2 point, bool touchable = true)
        {
            if (this.touchable == Touchable.Disabled)
                return null;

            for (var i = elements.Count - 1; i >= 0; i--)
            {
                var child = elements[i];
                if (!child.IsVisible())
                    continue;

                var childLocalPoint = child.ParentToLocalCoordinates(point);
                var hit = child.Hit(childLocalPoint, touchable);
                if (hit != null)
                    return hit;
            }

            return base.Hit(point, touchable);
        }

        /// <summary>
		/// Returns the transform for this group's coordinate system
		/// </summary>
		/// <returns>The transform.</returns>
        protected Matrix ComputeTransform()
        {
            var mat = Matrix.Identity;

            if (originX != 0 || originY != 0)
                mat = Matrix.Multiply(mat, Matrix.CreateTranslation(-originX, -originY, 0));

            if (rotation != 0)
                mat = Matrix.Multiply(mat, Matrix.CreateRotationX(MathHelper.ToRadians(rotation)));

            if (scaleX != 1 || scaleY != 1)
                mat = Matrix.Multiply(mat, Matrix.CreateScale(scaleX, scaleY, 1));

            mat = Matrix.Multiply(mat, Matrix.CreateTranslation(x + originX, y + originY, 0));

            // Find the first parent that transforms
            Group parentGroup = parent;
            while (parentGroup != null)
            {
                if (parentGroup.transform)
                    break;
                parentGroup = parentGroup.parent;
            }

            if (parentGroup != null)
                mat = Matrix.Multiply(mat, parentGroup.ComputeTransform());

            return mat;
        }

        /// <summary>
        /// Set the batch's transformation matrix, often with the result of {@link #computeTransform()}. Note this causes the batch to 
        /// be flushed. {@link #resetTransform(Batch)} will restore the transform to what it was before this call.
        /// </summary>
        /// <param name="graphics">Graphics.</param>
        /// <param name="transform">Transform.</param>
        protected void ApplyTransform(Graphics graphics, Matrix transform)
        {
            previousBatcherTransform = graphics.TransformMatrix;
            graphics.End();
            graphics.Begin(transform);
        }

        /// <summary>
		/// Restores the batch transform to what it was before {@link #applyTransform(Batch, Matrix4)}. Note this causes the batch to
		/// be flushed
		/// </summary>
		/// <param name="batch">Batch.</param>
		protected void ResetTransform(Graphics graphics)
        {
            graphics.End();
            graphics.Begin(previousBatcherTransform);
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
            parentAlpha *= color.A / 255.0f;

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

        /** Children completely outside of this rectangle will not be drawn. This is only valid for use with unrotated and unscaled
	     * actors.
	     * @param cullingArea May be null. */
        public virtual void SetCullingArea(Rectangle cullingArea)
        {
            this.cullingArea = cullingArea;
        }

        /** @return May be null.
         * @see #setCullingArea(Rectangle) */
        public virtual Rectangle GetCullingArea()
        {
            return cullingArea.Value;
        }

        public void DrawDebugElements(Graphics graphics, float parentAlpha)
        {
            parentAlpha *= color.A / 255.0f;
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
