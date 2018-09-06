using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
namespace BlueberryCore.UI
{
    public class Element : ILayout
    {
        internal float x, y, width, height;
        internal Color color = Color.White;
        internal Group parent;

        protected float originX, originY;
        protected float scaleX = 1, scaleY = 1;
        protected float rotation;
        protected bool visible = true;
        protected bool debug = false;
        protected Stage stage;
        protected Touchable touchable = Touchable.Enabled;

        #region Actions

        protected List<Action> actions = new List<Action>();

        public void AddAction(Action action)
        {
            action.SetElement(this);
            actions.Add(action);

            if (stage != null /*&& stage.getActionsRequestRendering()*/) Core.RequestRender();
        }

        public void RemoveAction(Action action)
        {
            lock (actions)
                if (actions.Remove(action)) action.SetElement(null);
        }

        public List<Action> GetActions()
        {
            return actions;
        }

        /** Returns true if the actor has one or more actions. */
        public bool HasActions()
        {
            return actions.Count > 0;
        }

        /** Removes all actions on this actor. */
        public void ClearActions()
        {
            lock (actions)
            {
                for (int i = actions.Count - 1; i >= 0; i--)
                    actions[i].SetElement(null);
                actions.Clear();
            }
        }

        #endregion
       
        public virtual void Update(float delta)
        {
            if (actions.Count > 0)
            {
                for (int i = 0; i < actions.Count; i++)
                {
                    var action = actions[i];
                    if (action.Update(delta) && i < actions.Count)
                    {
                        var current = actions[i];
                        int actionIndex = current == action ? i : actions.IndexOf(action);
                        if (actionIndex != -1)
                        {
                            actions.RemoveAt(actionIndex);
                            action.SetElement(null);
                            i--;
                        }
                        Core.RequestRender();
                    }
                }
            }
        }

        #region Listeners

        protected DelayedRemovalList<IEventListener> listeners = new DelayedRemovalList<IEventListener>();
        protected DelayedRemovalList<IEventListener> captureListeners = new DelayedRemovalList<IEventListener>();

        public List<IEventListener> GetListeners()
        {
            return listeners.Items();
        }

        public bool AddListener(IEventListener listener)
        {
            if (listener == null) throw new ArgumentNullException("listener cannot be null.");
            if (!listeners.Contains(listener))
            {
                listeners.Add(listener);
                return true;
            }
            return false;
        }

        public bool RemoveListener(IEventListener listener)
        {
            if (listener == null) throw new ArgumentNullException("listener cannot be null.");
            return listeners.Remove(listener);
        }

        public bool AddCaptureListener(IEventListener listener)
        {
            if (listener == null) throw new ArgumentNullException("listener cannot be null.");
            if (!captureListeners.Contains(listener)) captureListeners.Add(listener);
            return true;
        }

        public bool RemoveCaptureListener(IEventListener listener)
        {
            if (listener == null) throw new ArgumentNullException("listener cannot be null.");
            return captureListeners.Remove(listener);
        }

        /** Removes all listeners on this actor. */
        public void ClearListeners()
        {
            listeners.Clear();
            captureListeners.Clear();
        }

        #endregion

        /** Removes all actions and listeners on this actor. */
        public virtual void Clear()
        {
            ClearActions();
            ClearListeners();
        }

        /// <summary>
        /// Send all listener events here
        /// </summary>
        /// <param name="ev"></param>
        public virtual bool Fire(Event ev)
        {
            if (ev.GetStage() == null) ev.SetStage(GetStage());
            ev.SetTarget(this);

            var ancestors = new List<Group>();
            var parent = this.parent;
            while (parent != null)
            {
                ancestors.Add(parent);
                parent = parent.parent;
            }

            try
            {
                //Notify all parent capture listeners, starting at the root. Ancestors may stop an event before children receive it.
                var ancestorsArray = ancestors.ToArray();
                for (int i = ancestors.Count - 1; i >= 0; i--)
                {
                    var currentTarget = ancestorsArray[i];
                    currentTarget.Notify(ev, true);
                    if (ev.IsStopped()) return ev.IsCancelled();
                }

                // Notify the target capture listeners.
                Notify(ev, true);
                if (ev.IsStopped()) return ev.IsCancelled();

                // Notify the target listeners.
                Notify(ev, false);
                if (!ev.GetBubbles()) return ev.IsCancelled();
                if (ev.IsStopped()) return ev.IsCancelled();

                // Notify all parent listeners, starting at the target. Children may stop an event before ancestors receive it.
                for (int i = 0, n = ancestors.Count; i < n; i++)
                {
                    (ancestorsArray[i]).Notify(ev, false);
                    if (ev.IsStopped()) return ev.IsCancelled();
                }

                return ev.IsCancelled();
            }
            finally
            {
                ancestors.Clear();
            }

        }

        public virtual bool Notify(Event ev, bool capture)
        {
            if (ev.GetTarget() == null) throw new ArgumentNullException("The event target cannot be null.");

            var listeners = capture ? captureListeners : this.listeners;
            if (listeners.Count == 0) return ev.IsCancelled();

            ev.SetListenerElement(this);
            ev.SetCapture(capture);
		    if (ev.GetStage() == null) ev.SetStage(stage);

            listeners.Begin();
            for (int i = 0, n = listeners.Count; i < n; i++)
            {
                var listener = listeners[i];
                if (listener.Handle(ev))
                {
                    ev.Handle();
                    if (ev is InputEvent)
                    {
                        var iev = ev as InputEvent;
                        if (iev.GetInputType() == InputType.touchDown)
                        {
                            ev.GetStage().AddTouchFocus(listener, this, iev.GetTarget(), iev.GetPointer(), iev.GetButton());
                        }
                    }
                }
            }
            listeners.End();

            return ev.IsCancelled();
        }

        /** Returns true if input events are processed by this actor. */
        public bool IsTouchable()
        {
            return touchable == Touchable.Enabled;
        }

        public void SetTouchable(Touchable touchable)
        {
            this.touchable = touchable;
        }

        public Touchable GetTouchable() => touchable;
        
        public virtual void Draw(Graphics graphics, float parentAlpha)
        {
            Validate();
            graphics.DrawRectangleBorder(x, y, width, height, color);
        }

        public virtual void SetStage(Stage stage)
        {
            this.stage = stage;
        }

        public Stage GetStage()
        {
            return stage;
        }

        /** Returns true if this actor is the same as or is the descendant of the specified actor. */
        public bool IsDescendantOf(Element element)
        {
            if (element == null) throw new ArgumentNullException("actor cannot be null.");
            Element parent = this;
            while (true)
            {
                if (parent == null) return false;
                if (parent == element) return true;
                parent = parent.parent;
            }
        }

        /** Returns true if this actor is the same as or is the ascendant of the specified actor. */
        public bool IsAscendantOf(Element element)
        {
            if (element == null) throw new ArgumentNullException("actor cannot be null.");
            while (true)
            {
                if (element == null) return false;
                if (element == this) return true;
                element = element.parent;
            }
        }

        public bool HasParent()
        {
            return parent != null;
        }

        public virtual void SetParent(Group parent)
        {
            this.parent = parent;
        }

        public Group GetParent() => parent;

        /// <summary>
		/// Sets the x, y, width, and height.
		/// </summary>
		/// <param name="x">The x coordinate.</param>
		/// <param name="y">The y coordinate.</param>
		/// <param name="width">Width.</param>
		/// <param name="height">Height.</param>
		public virtual void SetBounds(float x, float y, float width, float height)
        {
            if (this.x != x || this.y != y)
            {
                this.x = x;
                this.y = y;
                PositionChanged();
            }

            if (this.width != width || this.height != height)
            {
                this.width = width;
                this.height = height;
                SizeChanged();
            }
        }

        public virtual void SetSize(float width, float height)
        {
            if (this.width != width || this.height != height)
            {
                this.width = width;
                this.height = height;
                SizeChanged();
            }
        }

        public virtual void SetPosition(float x, float y)
        {
            if (this.x != x || this.y != y)
            {
                this.x = x;
                this.y = y;
                PositionChanged();
            }
        }

        public void SetWidth(float width)
        {
            if (this.width != width)
            {
                this.width = width;
                SizeChanged();
            }
        }

        public void SetHeight(float height)
        {
            if (this.height != height)
            {
                this.height = height;
                SizeChanged();
            }
        }

        public Element SetX(float x)
        {
            if (this.x != x)
            {
                this.x = x;
                PositionChanged();
            }
            return this;
        }

        public Element SetY(float y)
        {
            if (this.y != y)
            {
                this.y = y;
                PositionChanged();
            }
            return this;
        }

        public float GetWidth()
        {
            return width;
        }

        public float GetHeight()
        {
            return height;
        }

        public float GetX()
        {
            return x;
        }

        public float GetY()
        {
            return y;
        }

        public float GetBottom()
        {
            return y + height;
        }

        public float GetRight()
        {
            return x + width;
        }

        public float GetOriginX()
        {
            return originX;
        }

        public void SetOriginX(float originX)
        {
            this.originX = originX;
        }

        public float GetOriginY()
        {
            return originY;
        }

        public void SetOriginY(float originY)
        {
            this.originY = originY;
        }

        /** Sets the origin position which is relative to the actor's bottom left corner. */
        public void SetOrigin(float originX, float originY)
        {
            this.originX = originX;
            this.originY = originY;
        }

        /** Sets the origin position to the specified {@link Align alignment}. */
        public void SetOrigin(int alignment)
        {
            if ((alignment & AlignInternal.left) != 0)
                originX = 0;
            else if ((alignment & AlignInternal.right) != 0)
                originX = width;
            else
                originX = width / 2;

            if ((alignment & AlignInternal.top) != 0)
                originY = 0;
            else if ((alignment & AlignInternal.bottom) != 0)
                originY = height;
            else
                originY = height / 2;
        }

        public float GetScaleX()
        {
            return scaleX;
        }

        public void SetScaleX(float scaleX)
        {
            this.scaleX = scaleX;
        }

        public float GetScaleY()
        {
            return scaleY;
        }

        public void SetScaleY(float scaleY)
        {
            this.scaleY = scaleY;
        }

        /** Sets the scale for both X and Y */
        public void SetScale(float scaleXY)
        {
            this.scaleX = scaleXY;
            this.scaleY = scaleXY;
        }

        /** Sets the scale X and scale Y. */
        public void SetScale(float scaleX, float scaleY)
        {
            this.scaleX = scaleX;
            this.scaleY = scaleY;
        }

        /** Adds the specified scale to the current scale. */
        public void ScaleBy(float scale)
        {
            scaleX += scale;
            scaleY += scale;
        }

        /** Adds the specified scale to the current scale. */
        public void ScaleBy(float scaleX, float scaleY)
        {
            this.scaleX += scaleX;
            this.scaleY += scaleY;
        }

        public float GetRotation()
        {
            return rotation;
        }

        public void SetRotation(float degrees)
        {
            if (this.rotation != degrees)
            {
                this.rotation = degrees;
                RotationChanged();
            }
        }

        /** Adds the specified rotation to the current rotation. */
        public void RotateBy(float amountInDegrees)
        {
            if (amountInDegrees != 0)
            {
                rotation += amountInDegrees;
                RotationChanged();
            }
        }

        public Color GetColor()
        {
            return color;
        }

        public void SetColor(Color color)
        {
            this.color = color;
        }

        /** Changes the z-order for this actor so it is in front of all siblings. */
        public void ToFront()
        {
            if (parent != null)
            {
                var elements = parent.GetElements();
                elements.Remove(this);
                elements.Add(this);
            }
        }

        /** Changes the z-order for this actor so it is in back of all siblings. */
        public void ToBack()
        {
            if (parent != null)
            {
                var elements = parent.GetElements();
                elements.Remove(this);
                elements.Insert(0, this);
            }
        }

        protected virtual void PositionChanged()
        {

        }

        protected virtual void SizeChanged()
        {
            Invalidate();
        }

        protected virtual void RotationChanged()
        {

        }

        public bool IsVisible()
        {
            return visible;
        }

        /// <summary>
		/// returns true if this Element and all parent Elements are visible
		/// </summary>
		/// <returns><c>true</c>, if parents visible was ared, <c>false</c> otherwise.</returns>
		bool AreParentsVisible()
        {
            if (!visible)
                return false;

            if (parent != null)
                return parent.AreParentsVisible();

            return visible;
        }

        public virtual Element Hit(Vector2 point, bool touchable = true)
        {
            // if we are not Touchable or us or any parent is not visible bail out
            if (touchable && this.touchable != Touchable.Enabled || !AreParentsVisible())
                return null;

            if (point.X >= 0 && point.X < width && point.Y >= 0 && point.Y < height)
                return this;
            return null;
        }

        /// <summary>
		/// Removes this element from its parent, if it has a parent
		/// </summary>
		public virtual bool Remove()
        {
            if (parent != null)
                return parent.RemoveElement(this);
            return false;
        }

        #region ILayout

        public bool FillParent { get; set; }

        public virtual float PreferredWidth
        {
            get { return 0; }
        }

        public virtual float PreferredHeight
        {
            get { return 0; }
        }

        public virtual float MinWidth
        {
            get { return PreferredWidth; }
        }

        public virtual float MinHeight
        {
            get { return PreferredHeight; }
        }

        public virtual float MaxWidth
        {
            get { return 0; }
        }

        public virtual float MaxHeight
        {
            get { return 0; }
        }

        public virtual void Invalidate()
        {
            
        }

        public virtual void InvalidateHierarchy()
        {
            Invalidate();

            if (parent is ILayout)
                ((ILayout)parent).InvalidateHierarchy();
        }

        public virtual void Validate()
        {
            if (FillParent && parent != null)
            {
                var stage = GetStage();
                float parentWidth, parentHeight;

                if (stage != null && parent == stage.GetRoot())
                {
                    parentWidth = stage.GetWidth();
                    parentHeight = stage.GetHeight();
                }
                else
                {
                    parentWidth = parent.GetWidth();
                    parentHeight = parent.GetHeight();
                }

                if (width != parentWidth || height != parentHeight)
                {
                    SetSize(parentWidth, parentHeight);
                    Invalidate();
                }
            }

            Layout();
        }

        public virtual void Layout()
        {
            
        }

        public virtual void Pack()
        {
            SetSize(PreferredWidth, PreferredHeight);
            Validate();
        }

        #endregion


        #region Coordinate transforms

        /// <summary>
		/// Converts the coordinates given in the parent's coordinate system to this element's coordinate system.
		/// </summary>
		/// <returns>The to local coordinates.</returns>
		/// <param name="parentCoords">Parent coords.</param>
		public Vector2 ParentToLocalCoordinates(Vector2 parentCoords)
        {
            if (rotation == 0)
            {
                if (scaleX == 1 && scaleY == 1)
                {
                    parentCoords.X -= x;
                    parentCoords.Y -= y;
                }
                else
                {
                    parentCoords.X = (parentCoords.X - x - originX) / scaleX + originX;
                    parentCoords.Y = (parentCoords.Y - y - originY) / scaleY + originY;
                }
            }
            else
            {
                var cos = (float) Math.Cos(MathHelper.ToRadians(rotation));
                var sin = (float) Math.Sin(MathHelper.ToRadians(rotation));
                var tox = parentCoords.X - x - originX;
                var toy = parentCoords.Y - y - originY;
                parentCoords.X = (tox * cos + toy * sin) / scaleX + originX;
                parentCoords.Y = (tox * -sin + toy * cos) / scaleY + originY;
            }

            return parentCoords;
        }

        /// <summary>
		/// Transforms the specified point in the stage's coordinates to the element's local coordinate system.
		/// </summary>
		/// <returns>The to local coordinates.</returns>
		/// <param name="stageCoords">Stage coords.</param>
		public Vector2 StageToLocalCoordinates(Vector2 stageCoords)
        {
            if (parent != null)
                stageCoords = parent.StageToLocalCoordinates(stageCoords);

            stageCoords = ParentToLocalCoordinates(stageCoords);
            return stageCoords;
        }

        /// <summary>
		/// Transforms the specified point in the element's coordinates to be in the stage's coordinates
		/// </summary>
		/// <returns>The to stage coordinates.</returns>
		/// <param name="localCoords">Local coords.</param>
		public Vector2 LocalToStageCoordinates(Vector2 localCoords)
        {
            return LocalToAscendantCoordinates(null, localCoords);
        }


        /// <summary>
        /// Converts coordinates for this element to those of a parent element. The ascendant does not need to be a direct parent
        /// </summary>
        /// <returns>The to ascendant coordinates.</returns>
        /// <param name="ascendant">Ascendant.</param>
        /// <param name="localCoords">Local coords.</param>
        public Vector2 LocalToAscendantCoordinates(Element ascendant, Vector2 localCoords)
        {
            Element element = this;
            while (element != null)
            {
                localCoords = element.LocalToParentCoordinates(localCoords);
                element = element.parent;
                if (element == ascendant)
                    break;
            }
            return localCoords;
        }

        /// <summary>
		/// Transforms the specified point in the element's coordinates to be in the parent's coordinates.
		/// </summary>
		/// <returns>The to parent coordinates.</returns>
		/// <param name="localCoords">Local coords.</param>
		public Vector2 LocalToParentCoordinates(Vector2 localCoords)
        {
            var rotation = -this.rotation;

            if (rotation == 0)
            {
                if (scaleX == 1 && scaleY == 1)
                {
                    localCoords.X += x;
                    localCoords.Y += y;
                }
                else
                {
                    localCoords.X = (localCoords.X - originX) * scaleX + originX + x;
                    localCoords.Y = (localCoords.Y - originY) * scaleY + originY + y;
                }
            }
            else
            {
                var cos = MathF.Cos(MathHelper.ToRadians(rotation));
                var sin = MathF.Sin(MathHelper.ToRadians(rotation));

                var tox = (localCoords.X - originX) * scaleX;
                var toy = (localCoords.Y - originY) * scaleY;
                localCoords.X = (tox * cos + toy * sin) + originX + x;
                localCoords.Y = (tox * -sin + toy * cos) + originY + y;
            }

            return localCoords;
        }

        #endregion


        #region Debug

        /// <summary>
        /// Draws this element's debug lines
        /// </summary>
        /// <param name="graphics">Graphics.</param>
        public virtual void DrawDebug(Graphics graphics)
        {
            if (debug)
                graphics.DrawRectangleBorder(x, y, width, height, Color.Red);
        }

        public bool GetDebug()
        {
            return debug;
        }

        #endregion

        /// <summary>
		/// Calls clipBegin(Batcher, float, float, float, float) to clip this actor's bounds
		/// </summary>
		/// <returns>The begin.</returns>
		public bool ClipBegin(Graphics graphics)
        {
            return ClipBegin(graphics, x, y, width, height);
        }


        /// <summary>
        /// Clips the specified screen aligned rectangle, specified relative to the transform matrix of the stage's Batch. The
        /// transform matrix and the stage's camera must not have rotational components. Calling this method must be followed by a call
        /// to clipEnd() if true is returned.
        /// </summary>
        public bool ClipBegin(Graphics graphics, float x, float y, float width, float height)
        {
            if (width <= 0 || height <= 0)
                return false;

            var tableBounds = new Rectangle((int)x, (int)y, (int)width, (int)height);
            var scissorBounds = ScissorStack.CalculateScissors(stage?.entity?.scene?.camera, graphics.TransformMatrix, tableBounds);
            if (ScissorStack.PushScissors(scissorBounds))
            {
                graphics.spriteBatch.SetScissorTest(true);
                graphics.primitiveBatch.SetScissorTest(true);
                return true;
            }

            return false;
        }


        /// <summary>
        /// Ends clipping begun by clipBegin(Batcher, float, float, float, float)
        /// </summary>
        /// <returns>The end.</returns>
        public void ClipEnd(Graphics graphics)
        {
            graphics.spriteBatch.SetScissorTest(false);
            graphics.primitiveBatch.SetScissorTest(false);
            ScissorStack.PopScissors();
        }
    }
}
