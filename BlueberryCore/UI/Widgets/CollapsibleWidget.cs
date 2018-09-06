using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlueberryCore.UI
{
    public class CollapsibleWidget : Group
    {
        Table table;

        private CollapseAction collapseAction;

        bool collapsed;
        bool actionRunning;

        float currentHeight;

        public CollapsibleWidget() { }

        public CollapsibleWidget(Table table) : this(table, false) { }

        public CollapsibleWidget(Table table, bool collapsed)
        {
            this.collapsed = collapsed;
            this.table = table;

            UpdateTouchable();

            if (table != null) AddElement(table);

            collapseAction = new CollapseAction(this);
        }

        public void SetCollapsed(bool collapse, bool withAnimation)
        {
            collapsed = collapse;
            UpdateTouchable();

            if (table == null) return;

            actionRunning = true;

            if (withAnimation)
            {
                AddAction(collapseAction);
            }
            else
            {
                if (collapse)
                {
                    currentHeight = 0;
                    collapsed = true;
                }
                else
                {
                    currentHeight = table.PreferredHeight;
                    collapsed = false;
                }

                actionRunning = false;
                InvalidateHierarchy();
            }
        }

        public void SetCollapsed(bool collapse)
        {
            SetCollapsed(collapse, true);
        }

        public bool IsCollapsed()
        {
            return collapsed;
        }

        private void UpdateTouchable()
        {
            if (collapsed)
                SetTouchable(Touchable.Disabled);
            else
                SetTouchable(Touchable.Enabled);
        }

        public override void Draw(Graphics graphics, float parentAlpha)
        {
            if (currentHeight > 1)
            {
                graphics.Flush();
                bool clipEnabled = ClipBegin(graphics, GetX(), GetY(), GetWidth(), currentHeight);

                base.Draw(graphics, parentAlpha);

                graphics.Flush();
                if (clipEnabled) ClipEnd(graphics);
            }
        }

        #region ILayout

        public override void Layout()
        {
            if (table == null) return;

            table.SetBounds(0, 0, table.PreferredWidth, table.PreferredHeight);

            if (actionRunning == false)
            {
                if (collapsed)
                    currentHeight = 0;
                else
                    currentHeight = table.PreferredHeight;
            }
        }

        public override float PreferredWidth
        {
            get
            {
                return table == null ? 0 : table.PreferredWidth;
            }
        }

        public override float PreferredHeight
        {
            get
            {
                if (table == null) return 0;

                if (actionRunning == false)
                {
                    if (collapsed)
                        return 0;
                    else
                        return table.PreferredHeight;
                }

                return currentHeight;
            }
        }

        #endregion

        public void SetTable(Table table)
        {
            this.table = table;
            ClearElements();
            AddElement(table);
        }

        protected override void OnChildrenChanged()
        {
            base.OnChildrenChanged();
            if (GetElements().Count > 1) throw new Exception("Only one element can be added to CollapsibleWidget");
        }

        private class CollapseAction : Action
        {
            private CollapsibleWidget _w;

            public CollapseAction(CollapsibleWidget w)
            {
                _w = w;
            }

            public override bool Update(float delta)
            {
                if (_w.collapsed)
                {
                    _w.currentHeight -= delta * 1000;
                    Core.RequestRender();
                    if (_w.currentHeight <= 0)
                    {
                        _w.currentHeight = 0;
                        _w.collapsed = true;
                        _w.actionRunning = false;
                    }
                }
                else
                {
                    _w.currentHeight += delta * 1000;
                    Core.RequestRender();
                    if (_w.currentHeight > _w.table.PreferredHeight)
                    {
                        _w.currentHeight = _w.table.PreferredHeight;
                        _w.collapsed = false;
                        _w.actionRunning = false;
                    }
                }

                _w.InvalidateHierarchy();
                return !_w.actionRunning;
            }
        }
    }
}
