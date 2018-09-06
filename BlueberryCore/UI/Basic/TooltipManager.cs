using BlueberryCore.UI.Actions;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace BlueberryCore.UI
{
    public class TooltipManager
    {
        private static TooltipManager instance;

        /** Seconds from when an actor is hovered to when the tooltip is shown. Default is 2. Call {@link #hideAll()} after changing to
	    * reset internal state. */
        public float initialTime = 2;
        /** Once a tooltip is shown, this is used instead of {@link #initialTime}. Default is 0. */
        public float subsequentTime = 0;
        /** Seconds to use {@link #subsequentTime}. Default is 1.5. */
        public float resetTime = 1.5f;
        /** If false, tooltips will not be shown. Default is true. */
        public bool enabled = true;
        /** If false, tooltips will be shown without animations. Default is true. */
        public bool animations = true;
        /** The maximum width of a {@link TextTooltip}. The label will wrap if needed. Default is Integer.MAX_VALUE. */
        public float maxWidth = int.MaxValue;
        /** The distance from the mouse position to offset the tooltip actor. Default is 15,19. */
        public float offsetX = 15, offsetY = 19;
        /** The distance from the tooltip actor position to the edge of the screen where the actor will be shown on the other side of
         * the mouse cursor. Default is 7. */
        public float edgeDistance = 7;

        internal readonly List<Tooltip> shown = new List<Tooltip>();

        internal float time;
        internal readonly Task resetTask;

        #region ResetTask

        private class ResetTask : Task
        {
            private TooltipManager manager;

            public ResetTask(TooltipManager manager)
            {
                this.manager = manager;
            }

            public override void Run()
            {
                manager.time = manager.initialTime;
            }
        }

        #endregion

        internal Tooltip showTooltip;
        internal readonly Task showTask;

        #region ShowTask

        private class ShowTask : Task
        {
            private TooltipManager manager;

            public ShowTask(TooltipManager manager)
            {
                this.manager = manager;
            }

            public override void Run()
            {
                if (manager.showTooltip == null || manager.showTooltip.targetElement == null) return;

                var stage = manager.showTooltip.targetElement.GetStage();
                if (stage == null) return;
                stage.AddElement(manager.showTooltip.container);
                manager.showTooltip.container.ToFront();
                manager.shown.Add(manager.showTooltip);

                manager.showTooltip.container.ClearActions();
                manager.ShowAction(manager.showTooltip);

                if (!manager.showTooltip.instant)
                {
                    manager.time = manager.subsequentTime;
                    manager.resetTask.Cancel();
                }
            }
        }

        #endregion

        public TooltipManager()
        {
            time = initialTime;
            resetTask = new ResetTask(this);
            showTask = new ShowTask(this);
        }

        public void TouchDown(Tooltip tooltip)
        {
            showTask.Cancel();
            if (tooltip.container.Remove()) resetTask.Cancel();
            resetTask.Run();
            if (enabled || tooltip.always)
            {
                showTooltip = tooltip;
                Timer.Schedule(showTask, TimeSpan.FromSeconds(time));
            }
        }
        
        public void Enter(Tooltip tooltip)
        {
            showTooltip = tooltip;
            showTask.Cancel();
            if (enabled || tooltip.always)
            {
                if (time == 0 || tooltip.instant)
                    showTask.Run();
                else
                    Timer.Schedule(showTask, TimeSpan.FromSeconds(time));
            }
        }

        public void Hide(Tooltip tooltip)
        {
            showTooltip = null;
            showTask.Cancel();
            if (tooltip.container.HasParent())
            {
                shown.Remove(tooltip);
                HideAction(tooltip);
                resetTask.Cancel();
                Timer.Schedule(resetTask, TimeSpan.FromSeconds(resetTime));
            }
        }

        /** Called when tooltip is shown. Default implementation sets actions to animate showing. */
        protected void ShowAction(Tooltip tooltip)
        {
            float actionTime = animations ? (time > 0 ? 0.5f : 0.15f) : 0.1f;
            tooltip.container.SetTransform(true);
            tooltip.container.SetColor(new Color(tooltip.container.GetColor(), 0.2f));
            tooltip.container.SetScale(0.05f);
            tooltip.container.AddAction(SAction.Parallel(SAction.FadeIn(actionTime, Interpolation.fade), SAction.ScaleTo(1, 1, actionTime, Interpolation.fade)));
        }

        /** Called when tooltip is hidden. Default implementation sets actions to animate hiding and to remove the actor from the stage
	     * when the actions are complete. A subclass must at least remove the actor. */
        protected void HideAction(Tooltip tooltip)
        {
            tooltip.container.AddAction(SAction.Sequence(SAction.Parallel(SAction.Alpha(0.2f, 0.2f, Interpolation.fade), SAction.ScaleTo(0.05f, 0.05f, 0.2f, Interpolation.fade)), SAction.RemoveElement()));
        }

        public void HideAll()
        {
            resetTask.Cancel();
            showTask.Cancel();
            time = initialTime;
            showTooltip = null;

            foreach (Tooltip tooltip in shown)
                tooltip.Hide();
            shown.Clear();
        }

        /** Shows all tooltips on hover without a delay for {@link #resetTime} seconds. */
        public void Instant()
        {
            time = 0;
            showTask.Run();
            showTask.Cancel();
        }

        static public TooltipManager GetInstance()
        {
            if (instance == null)
            {
                instance = new TooltipManager();
            }
            return instance;
        }
    }
}
