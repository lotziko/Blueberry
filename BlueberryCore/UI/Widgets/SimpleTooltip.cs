using BlueberryCore.UI.Actions;
using Microsoft.Xna.Framework;
using System;

namespace BlueberryCore.UI
{
    public class SimpleTooltip : Table
    {
        public static float DEFAULT_FADE_TIME = 0.3f;
        public static float DEFAULT_APPEAR_DELAY_TIME = 0.6f;
        /**
	     * Controls whether to fade out tooltip when mouse was moved. Changing this will not affect already existing tooltips.
	     * @see #setMouseMoveFadeOut(boolean)
	     */
        public static bool MOUSE_MOVED_FADEOUT = false;

        private Element target;
        private Element content;
        private Cell contentCell;

        private bool mouseMoveFadeOut = MOUSE_MOVED_FADEOUT;
        private TooltipInputListener listener;

        private DisplayTask displayTask;

        private float fadeTime = DEFAULT_FADE_TIME;
        private float appearDelayTime = DEFAULT_APPEAR_DELAY_TIME;


        private SimpleTooltip(Builder builder)
        {
            var style = builder.style;
            if (style == null) throw new Exception("Style can't be null");//style = VisUI.getSkin().get("default", TooltipStyle.class);

            Init(style, builder.target, builder.content);
            if (builder.width != -1)
            {
                contentCell.Width(builder.width);
                Pack();
            }

            //SetTouchable(Touchable.Enabled);
        }

        public SimpleTooltip(Skin skin, string stylename = "default") : this(skin.Get<SimpleTooltipStyle>(stylename))
        {
            Init(skin.Get<SimpleTooltipStyle>(stylename), null, null);
        }

        public SimpleTooltip(SimpleTooltipStyle style)
        {
            Init(style, null, null);
        }


        /**
	     * Remove any attached tooltip from target actor
	     * @param target that tooltips will be removed
	     */
        public static void RemoveTooltip(Element target)
        {
            var listeners = target.GetListeners();
            foreach (IEventListener listener in listeners)
            {
                if (listener is TooltipInputListener) target.RemoveListener(listener);
            }
        }





        private void Init(SimpleTooltipStyle style, Element target, Element content)
        {
            this.target = target;
            this.content = content;
            this.listener = new TooltipInputListener(this);
            this.displayTask = new DisplayTask(this);

            SetBackground(style.background);

            contentCell = Add(content).PadLeft(3).PadRight(3).PadBottom(2);
            Pack();

            if (target != null) Attach();

            AddListener(new Listener(this));
        }

        #region Listener

        private class Listener : InputListener
        {
            private SimpleTooltip tooltip;

            public Listener(SimpleTooltip tooltip)
            {
                this.tooltip = tooltip;
            }

            public override bool TouchDown(InputEvent ev, float x, float y, int pointer, int button)
            {
                tooltip.ToFront();
                return true;
            }

            public override void Enter(InputEvent ev, float x, float y, int pointer, Element fromElement)
            {
                if (pointer == -1)
                {
                    tooltip.ClearActions();
                    tooltip.AddAction(SAction.Sequence(SAction.FadeIn(tooltip.fadeTime, Interpolation.fade)));
                }
            }

            public override void Exit(InputEvent ev, float x, float y, int pointer, Element toElement)
            {
                if (pointer == -1)
                {
                    tooltip.FadeOut();
                }
            }
        }

        #endregion

        public override void Draw(Graphics graphics, float parentAlpha)
        {
            base.Draw(graphics, parentAlpha);
        }

        /**
	     * Attaches tooltip to current target, must be called if tooltip listener was removed from target (for example by
	     * calling target.clearListeners() )
	     */
        public void Attach()
        {
            if (target == null) return;
            var listeners = target.GetListeners();
            foreach (var listener in listeners)
            {
                if (listener is TooltipInputListener) {
                    throw new Exception("More than one tooltip cannot be added to the same target!");
                }
            }

            target.AddListener(listener);
        }

        /**
         * Detaches tooltip form current target, does not change tooltip target meaning that this tooltip can be reattached to
         * same target by calling {@link Tooltip#attach()}
         */
        public void Detach()
        {
            if (target == null) return;
            target.RemoveListener(listener);
        }

        /** Sets new target for this tooltip, tooltip will be automatically detached from old target. */
        public void SetTarget(Element newTarget)
        {
            Detach();
            target = newTarget;
            Attach();
        }

        public Element GetTarget()
        {
            return target;
        }

        private void FadeOut()
        {
            ClearActions();
            AddAction(SAction.Sequence(SAction.FadeOut(fadeTime, Interpolation.fade), SAction.RemoveElement()));
        }

        private Table FadeIn()
        {
            ClearActions();
            SetColor(new Color(255, 255, 255, 0));
            AddAction(SAction.Sequence(SAction.FadeIn(fadeTime, Interpolation.fade)));
            return this;
        }

        public Element GetContent()
        {
            return content;
        }

        public void SetContent(Element content)
        {
            this.content = content;
            contentCell.SetElement(content);
            Pack();
        }

        public Cell GetContentCell()
        {
            return contentCell;
        }

        /**
	     * Changes text tooltip to specified text. If tooltip content is not instance of Label then previous tooltip content
	     * will be replaced by Label instance.
	     * @param text next tooltip text
	     */
        public void SetText(string text, Skin skin, string stylename = "default")
        {
            if (content is Label) {
                ((Label)content).SetText(text);
            } else {
                SetContent(new Label(text, skin, stylename));
            }
            Pack();
        }

        public override void SetPosition(float x, float y)
        {
            base.SetPosition((int)x, (int)y);
        }

        public float GetAppearDelayTime()
        {
            return appearDelayTime;
        }

        public void SetAppearDelayTime(float appearDelayTime)
        {
            this.appearDelayTime = appearDelayTime;
        }

        public float GetFadeTime()
        {
            return fadeTime;
        }

        public void SetFadeTime(float fadeTime)
        {
            this.fadeTime = fadeTime;
        }

        public bool IsMouseMoveFadeOut()
        {
            return mouseMoveFadeOut;
        }

        /**
	     * @param mouseMoveFadeOut if true tooltip fill fade out when mouse was moved. If false tooltip will only fadeout on
	     * mouse click or when mouse has exited target widget. Default is {@link Tooltip#MOUSE_MOVED_FADEOUT}.
	     */
        public void SetMouseMoveFadeOut(bool mouseMoveFadeOut)
        {
            this.mouseMoveFadeOut = mouseMoveFadeOut;
        }

        private class DisplayTask : Task
        {
            private readonly SimpleTooltip tooltip;

            public DisplayTask(SimpleTooltip tooltip)
            {
                this.tooltip = tooltip;
            }

            public override void Run()
            {
                if (tooltip.target.GetStage() == null) return;
                //if (!tooltip.target.GetStage().GetElements().Contains(tooltip))
                //{
                tooltip.target.GetStage().AddElement(tooltip.FadeIn());
                //}
                    
                //ActorUtils.keepWithinStage(getStage(), Tooltip.this);
            }
        }

        private class TooltipInputListener : InputListener
        {
            private SimpleTooltip tooltip;

            public TooltipInputListener(SimpleTooltip tooltip)
            {
                this.tooltip = tooltip;
            }

            public override bool TouchDown(InputEvent ev, float x, float y, int pointer, int button)
            {
                tooltip.displayTask.Cancel();
                tooltip.ToFront();
                tooltip.FadeOut();
                return true;
            }

            public override void Enter(InputEvent ev, float x, float y, int pointer, Element fromElement)
            {
                if (pointer == -1)
                {
                    Vector2 targetPos = tooltip.target.LocalToStageCoordinates(new Vector2());

                    tooltip.SetX(targetPos.X + (tooltip.target.GetWidth() - tooltip.GetWidth()) / 2);

                    float tooltipY = targetPos.Y - tooltip.GetHeight() - 6;
                    float stageHeight = tooltip.target.GetStage().GetHeight();

                    //is there enough space to display above widget?
                    //if (stageHeight - tooltipY > stageHeight)
                        tooltip.SetY(targetPos.Y + tooltip.target.GetHeight() + 6); //display above widget
                                                                                    //else
                                                                                    //   tooltip.SetY(tooltipY); //display below
                    
                    tooltip.displayTask.Cancel();
                    Timer.Schedule(tooltip.displayTask, TimeSpan.FromSeconds(tooltip.appearDelayTime));
                }
            }

            public override void Exit(InputEvent ev, float x, float y, int pointer, Element toElement)
            {
                if (pointer == -1)
                {
                    tooltip.displayTask.Cancel();
                    tooltip.FadeOut();
                }
            }

            public override bool MouseMoved(InputEvent ev, float x, float y)
            {
                if (tooltip.mouseMoveFadeOut && tooltip.IsVisible() && tooltip.GetActions().Count == 0) tooltip.FadeOut();
                return false;
            }
        }


        public class Builder
        {
            internal readonly Element content;

		    internal Element target = null;
            internal SimpleTooltipStyle style = null;
            internal float width = -1;

            public Builder(Element content)
            {
                this.content = content;
            }

            public Builder(string text, LabelStyle style) : this(text, UI.Align.center, style)
            {
                
            }

            public Builder(string text, Align textAlign, LabelStyle style)
            {
                var label = new Label(text, style);
                label.SetAlign(textAlign);
                this.content = label;
            }

            public Builder Target(Element target)
            {
                this.target = target;
                return this;
            }

            public Builder Style(Skin skin, string stylename = "default")
            {
                this.style = skin.Get<SimpleTooltipStyle>(stylename);
                return this;
            }

            public Builder Style(SimpleTooltipStyle style)
            {
                this.style = style;
                return this;
            }

            /** Sets tooltip width. If tooltip content is text only then calling this will automatically enable label wrapping. */
            public Builder Width(float width)
            {
                if (width < 0) throw new ArgumentOutOfRangeException("width must be > 0");
                this.width = width;
                if (content is Label)
                {
                    ((Label)content).SetWrap(true);
                }
                return this;
            }

            public SimpleTooltip Build()
            {
                return new SimpleTooltip(this);
            }
        }
    }

    public class SimpleTooltipStyle
    {
        public IDrawable background;

        public SimpleTooltipStyle()
        {
        }

        public SimpleTooltipStyle(SimpleTooltipStyle style)
        {
            this.background = style.background;
        }

        public SimpleTooltipStyle(IDrawable background)
        {
            this.background = background;
        }
    }
}
