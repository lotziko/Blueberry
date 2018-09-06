using Microsoft.Xna.Framework;

namespace BlueberryCore.UI
{
    public class Tooltip : InputListener
    {
        internal static Vector2 tmp = new Vector2();

        private readonly TooltipManager manager;
	    internal readonly Container<Element> container;
        internal bool instant, always;
        internal Element targetElement;

        /** @param contents May be null. */
        public Tooltip(Element contents) : this(contents, TooltipManager.GetInstance())
        {
            
        }

        /** @param contents May be null. */
        public Tooltip(Element contents, TooltipManager manager)
        {
            this.manager = manager;

            container = new TipContainer(this, contents);
            container.SetTouchable(Touchable.Disabled);
        }

        #region Container

        private class TipContainer : Container<Element>
        {
            private readonly Tooltip tooltip;

            public TipContainer(Tooltip tooltip, Element element) : base(element)
            {
                this.tooltip = tooltip;
            }

            public override void Update(float delta)
            {
                base.Update(delta);
                if (tooltip.targetElement != null && tooltip.targetElement.GetStage() == null) Remove();
            }
        }

        #endregion

        public TooltipManager GetManager()
        {
            return manager;
        }

        public Container<Element> GetContainer()
        {
            return container;
        }

        public void SetElement(Element contents)
        {
            container.SetElement(contents);
        }

        public Element GetElement()
        {
            return container.GetElement();
        }

        /** If true, this tooltip is shown without delay when hovered. */
        public void SetInstant(bool instant)
        {
            this.instant = instant;
        }

        /** If true, this tooltip is shown even when tooltips are not {@link TooltipManager#enabled}. */
        public void SetAlways(bool always)
        {
            this.always = always;
        }

        public override bool TouchDown(InputEvent ev, float x, float y, int pointer, int button)
        {
            if (instant)
            {
                container.ToFront();
                return false;
            }
            manager.TouchDown(this);
            return false;
        }

        public override bool MouseMoved(InputEvent ev, float x, float y)
        {
            if (container.HasParent()) return false;
            SetContainerPosition(ev.GetListenerElement(), x, y);
		    return true;
        }

        private void SetContainerPosition(Element element, float x, float y)
        {
            targetElement = element;
            var stage = element.GetStage();
            if (stage == null) return;

            container.Pack();
            float offsetX = manager.offsetX, offsetY = manager.offsetY, dist = manager.edgeDistance;
            tmp.Set(x + offsetX, y - offsetY - container.GetHeight());
            Vector2 point = element.LocalToStageCoordinates(tmp);
            tmp.Set(x + offsetX, y + offsetY);
            if (point.Y < dist) point = element.LocalToStageCoordinates(tmp);
            if (point.X < dist) point.X = dist;
            if (point.X + container.GetWidth() > stage.GetWidth() - dist) point.X = stage.GetWidth() - dist - container.GetWidth();
            if (point.Y + container.GetHeight() > stage.GetHeight() - dist) point.Y = stage.GetHeight() - dist - container.GetHeight();
            container.SetPosition(point.X, point.Y);

            tmp.Set(element.GetWidth() / 2, element.GetHeight() / 2);
            point = element.LocalToStageCoordinates(tmp);
            point -= new Vector2(container.GetX(), container.GetY());
            container.SetOrigin(point.X, point.Y);
        }

        public override void Enter(InputEvent ev, float x, float y, int pointer, Element fromElement)
        {
            if (pointer != -1) return;
            if (Input.IsTouched()) return;
            var element = ev.GetListenerElement();
            if (fromElement != null && fromElement.IsDescendantOf(element)) return;
            SetContainerPosition(element, x, y);
            manager.Enter(this);
        }

        public override void Exit(InputEvent ev, float x, float y, int pointer, Element toElement)
        {
            if (toElement != null && toElement.IsDescendantOf(ev.GetListenerElement())) return;
            Hide();
        }

        public void Hide()
        {
            manager.Hide(this);
        }
    }
}
