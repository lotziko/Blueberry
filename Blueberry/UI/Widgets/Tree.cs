using System;
using System.Collections.Generic;

namespace Blueberry.UI
{
    public class Tree : Group
    {
        TreeStyle style;
        readonly List<Node> rootNodes = new List<Node>();
        readonly Selection<Node> selection;
        float ySpacing = 4, iconSpacingLeft = 2, iconSpacingRight = 2, padding = 0, indentSpacing;
        private float prefWidth, prefHeight;
        private bool sizeInvalid = true;
        private Node foundNode;
        Node overNode, rangeStart;
        private ClickListener clickListener;

        #region Constructors

        public Tree(Skin skin, String stylename = "default") : this(skin.Get<TreeStyle>(stylename))
        {
        }

        public Tree(TreeStyle style)
        {
            selection = new NodeSelection
            {
                tree = this
            };
            selection.SetElement(this);
            selection.SetMultiple(true);
            SetStyle(style);
            AddListener(clickListener = new Listener(this));
        }

        #endregion

        public void SetStyle(TreeStyle style)
        {
            this.style = style;

            // Reasonable default.
            if (indentSpacing == 0) indentSpacing = Math.Max(style.plus.MinWidth, style.minus.MinWidth);
        }

        public TreeStyle GetStyle() => style;

        #region Node methods

        public void Add(Node node)
        {
            Insert(rootNodes.Count, node);
        }

        public void Insert(int index, Node node)
        {
            Remove(node);
            node.parent = null;
            rootNodes.Insert(index, node);
            node.AddToTree(this);
            InvalidateHierarchy();
        }

        public void Remove(Node node)
        {
            if (node.parent != null)
            {
                node.parent.Remove(node);
                return;
            }
            rootNodes.Remove(node);
            node.RemoveFromTree(this);
            InvalidateHierarchy();
        }

        /** Removes all tree nodes. */
        public override void ClearElements()
        {
            base.ClearElements();
            SetOverNode(null);
            rootNodes.Clear();
            selection.Clear();
        }

        public List<Node> GetNodes()
        {
            return rootNodes;
        }

        /** @return May be null. */
        public Node GetNodeAt(float y)
        {
            foundNode = null;
            GetNodeAt(rootNodes, y, /*getHeight()*/0);
            return foundNode;
        }

        private float GetNodeAt(List<Node> nodes, float y, float rowY)
        {
            for (int i = 0, n = nodes.Count; i < n; i++)
            {
                Node node = nodes[i];
                float height = node.height;
                rowY += node.GetHeight() - height; // Node subclass may increase getHeight.
                if (y > rowY && y <= rowY + height + ySpacing)//y >= rowY - height - ySpacing && y < rowY)
                {
                    foundNode = node;
                    return -1;
                }
                rowY += height + ySpacing;
                if (node.expanded)
                {
                    rowY = GetNodeAt(node.children, y, rowY);
                    if (rowY == -1) return -1;
                }
            }
            return rowY;
        }

        void SelectNodes(List<Node> nodes, float low, float high)
        {
            for (int i = 0, n = nodes.Count; i < n; i++)
            {
                Node node = nodes[i];
                if (node.element.GetY() >= high + node.element.GetHeight()) break;
                if (!node.IsSelectable()) continue;
                if (node.element.GetY() > low - node.element.GetHeight()) selection.Add(node);
                if (node.expanded) SelectNodes(node.children, low, high);
            }
        }

        public Selection<Node> GetSelection()
        {
            return selection;
        }

        public List<Node> GetRootNodes()
        {
            return rootNodes;
        }

        /** @return May be null. */
        public Node GetOverNode()
        {
            return overNode;
        }

        /** @return May be null. */
        public object GetOverObject()
        {
            if (overNode == null) return null;
            return overNode.GetObject();
        }

        /** @param overNode May be null. */
        public void SetOverNode(Node overNode)
        {
            this.overNode = overNode;
        }

        /** Sets the amount of horizontal space between the nodes and the left/right edges of the tree. */
        public void SetPadding(float padding)
        {
            this.padding = padding;
        }

        public void SetIndentSpacing(float indentSpacing)
        {
            this.indentSpacing = indentSpacing;
        }

        /** Returns the amount of horizontal space for indentation level. */
        public float GetIndentSpacing()
        {
            return indentSpacing;
        }

        /** Sets the amount of vertical space between nodes. */
        public void SetYSpacing(float ySpacing)
        {
            this.ySpacing = ySpacing;
        }

        public float GetYSpacing()
        {
            return ySpacing;
        }

        /** Sets the amount of horizontal space between the node actors and icons. */
        public void SetIconSpacing(float left, float right)
        {
            iconSpacingLeft = left;
            iconSpacingRight = right;
        }

        public void FindExpandedObjects(List<object> objects)
        {
            FindExpandedObjects(rootNodes, objects);
        }

        public void RestoreExpandedObjects(List<object> objects)
        {
            for (int i = 0, n = objects.Count; i < n; i++)
            {
                Node node = FindNode(objects[i]);
                if (node != null)
                {
                    node.SetExpanded(true);
                    node.ExpandTo();
                }
            }
        }

        static bool FindExpandedObjects(List<Node> nodes, List<object> objects)
        {
            bool expanded = false;
            for (int i = 0, n = nodes.Count; i < n; i++)
            {
                Node node = nodes[i];
                if (node.expanded && !FindExpandedObjects(node.children, objects)) objects.Add(node.obj);
            }
            return expanded;
        }

        /** Returns the node with the specified object, or null. */
        public Node FindNode(object obj)
        {
            if (obj == null) throw new ArgumentNullException("object cannot be null.");
            return FindNode(rootNodes, obj);
        }

        static Node FindNode(List<Node> nodes, object obj)
        {
            for (int i = 0, n = nodes.Count; i < n; i++)
            {
                Node node = nodes[i];
                if (obj.Equals(node.obj)) return node;
            }
            for (int i = 0, n = nodes.Count; i < n; i++)
            {
                Node node = nodes[i];
                Node found = FindNode(node.children, obj);
                if (found != null) return found;
            }
            return null;
        }

        public void CollapseAll()
        {
            CollapseAll(rootNodes);
        }

        static void CollapseAll(List<Node> nodes)
        {
            for (int i = 0, n = nodes.Count; i < n; i++)
            {
                Node node = nodes[i];
                node.SetExpanded(false);
                CollapseAll(node.children);
            }
        }

        public void ExpandAll()
        {
            ExpandAll(rootNodes);
        }

        static void ExpandAll(List<Node> nodes)
        {
            for (int i = 0, n = nodes.Count; i < n; i++)
                nodes[i].ExpandAll();
        }

        #endregion

        public override void Draw(Graphics graphics, float parentAlpha)
        {
            DrawBackground(graphics, parentAlpha);
            float plusMinusWidth = Math.Max(style.plus.MinWidth, style.minus.MinWidth);
            Draw(graphics, parentAlpha, rootNodes, padding, plusMinusWidth);
            base.Draw(graphics, parentAlpha); // Draw actors.
        }

        /** Called to draw the background. Default implementation draws the style background drawable. */
        protected void DrawBackground(Graphics graphics, float parentAlpha)
        {
            if (style.background != null)
            {
                style.background.Draw(graphics, x, y, width, height, new Col(color, color.A * parentAlpha));
            }
        }

        /** Draws selection, icons, and expand icons. */
        private void Draw(Graphics graphics, float parentAlpha, List<Node> nodes, float indent, float plusMinusWidth)
        {
            IDrawable plus = style.plus, minus = style.minus;
            var col = new Col(color, color.A * parentAlpha);
            float x = GetX(), y = GetY(), expandX = x + indent, iconX = expandX + plusMinusWidth + iconSpacingLeft;
            lock (nodes)
                for (int i = 0, n = nodes.Count; i < n; i++)
                {
                    Node node = nodes[i];
                    float height = node.height;
                    Element element = node.element;

                    if (selection.Contains(node) && style.selection != null)
                    {
                        style.selection.Draw(graphics, x, y + element.GetY() - ySpacing / 2, GetWidth(), height + ySpacing, col);
                    }
                    else if (node == overNode && style.over != null)
                    {
                        style.over.Draw(graphics, x, y + element.GetY() - ySpacing / 2, GetWidth(), height + ySpacing, col);
                    }

                    if (node.icon != null)
                    {
                        float _iconY = (float)(y + element.GetY() + Math.Round((height - node.icon.MinHeight) / 2));
                        node.icon.Draw(graphics, iconX, _iconY, node.icon.MinWidth, node.icon.MinHeight, col);
                    }

                    if (node.children.Count == 0) continue;

                    IDrawable expandIcon = node.expanded ? minus : plus;
                    float iconY = (float)(y + element.GetY() + Math.Round((height - expandIcon.MinHeight) / 2));
                    expandIcon.Draw(graphics, expandX, iconY, expandIcon.MinWidth, expandIcon.MinHeight, col);
                    if (node.expanded) Draw(graphics, parentAlpha, node.children, indent + indentSpacing, plusMinusWidth);
                }
        }


        #region ILayout

        public override float PreferredWidth
        {
            get
            {
                Validate();
                return prefWidth;
            }
        }

        public override float PreferredHeight
        {
            get
            {
                Validate();
                return prefHeight;
            }
        }

        public override void Invalidate()
        {
            base.Invalidate();
            sizeInvalid = true;
        }

        public override void Layout()
        {
            if (sizeInvalid) ComputeSize();
            float plusMinusWidth = Math.Max(style.plus.MinWidth, style.minus.MinWidth);
            Layout(rootNodes, padding, /*getHeight() - ySpacing / 2*/ 0, plusMinusWidth);
        }

        private float Layout(List<Node> nodes, float indent, float y, float plusMinusWidth)
        {
            float ySpacing = this.ySpacing;
            float spacing = iconSpacingLeft + iconSpacingRight;
            for (int i = 0, n = nodes.Count; i < n; i++)
            {
                Node node = nodes[i];
                float x = indent + plusMinusWidth;
                if (node.icon != null) x += spacing + node.icon.MinWidth;
                //y += node.getHeight();
                node.element.SetPosition(x, y);
                y += node.GetHeight() + ySpacing;
                if (node.expanded) y = Layout(node.children, indent + indentSpacing, y, plusMinusWidth);
            }
            return y;
        }

        private void ComputeSize()
        {
            sizeInvalid = false;
            prefWidth = style.plus.MinWidth;
            prefWidth = Math.Max(prefWidth, style.minus.MinWidth);
            prefHeight = 0;//getHeight();
            float plusMinusWidth = Math.Max(style.plus.MinWidth, style.minus.MinWidth);
            ComputeSize(rootNodes, indentSpacing, plusMinusWidth);
            prefWidth += padding * 2;
            //_prefHeight = getHeight() - _prefHeight;
        }

        private void ComputeSize(List<Node> nodes, float indent, float plusMinusWidth)
        {
            float ySpacing = this.ySpacing;
            float spacing = iconSpacingLeft + iconSpacingRight;
            for (int i = 0, n = nodes.Count; i < n; i++)
            {
                Node node = nodes[i];
                float rowWidth = indent + plusMinusWidth;
                Element element = node.element;
                if (element is ILayout layout)
                {
                    rowWidth += layout.PreferredWidth;
                    node.height = layout.PreferredHeight;
                    layout.Pack();
                }
                else
                {
                    rowWidth += element.GetWidth();
                    node.height = element.GetHeight();
                }
                if (node.icon != null)
                {
                    rowWidth += spacing + node.icon.MinWidth;
                    node.height = Math.Max(node.height, node.icon.MinHeight);
                }
                prefWidth = Math.Max(prefWidth, rowWidth);
                prefHeight += node.height + ySpacing;
                if (node.expanded) ComputeSize(node.children, indent + indentSpacing, plusMinusWidth);
            }
        }

        #endregion

        #region ClickListener

        private class Listener : ClickListener<Tree>
        {
            public Listener(Tree par) : base(par)
            {
            }

            public override void Clicked(InputEvent ev, float x, float y)
            {
                var node = par.GetNodeAt(y);
                if (node == null) return;
                if (node != par.GetNodeAt(GetTouchDownY())) return;
                if (par.selection.GetMultiple() && par.selection.HasItems() && Input.IsShiftDown())
                {
                    // Select range (shift).
                    if (par.rangeStart == null) par.rangeStart = node;
                    Node rangeStart = par.rangeStart;
                    if (!Input.IsCtrlDown()) par.selection.Clear();
                    float start = rangeStart.element.GetY(), end = node.element.GetY();
                    if (start > end)
                        par.SelectNodes(par.rootNodes, end, start);
                    else
                    {
                        par.SelectNodes(par.rootNodes, start, end);
                        //par.selection.Items()//.ord().Reverse();
                    }

                    par.selection.FireChangeEvent();
                    par.rangeStart = rangeStart;
                    return;
                }
                if (node.children.Count > 0 && (!par.selection.GetMultiple() || !Input.IsCtrlDown()))
                {
                    // Toggle expanded if left of icon.
                    float rowX = node.element.GetX();
                    if (node.icon != null) rowX -= par.iconSpacingRight + node.icon.MinWidth;
                    if (x < rowX)
                    {
                        node.SetExpanded(!node.expanded);
                        node.OnExpanded?.Invoke(node.expanded);
                        return;
                    }
                }
                if (!node.IsSelectable()) return;
                par.selection.Choose(node);
                if (!par.selection.IsEmpty()) par.rangeStart = node;
            }

            public override bool MouseMoved(InputEvent ev, float x, float y)
            {
                par.SetOverNode(par.GetNodeAt(y));
                return false;
            }

            public override void Exit(InputEvent ev, float x, float y, int pointer, Element toElement)
            {
                base.Exit(ev, x, y, pointer, toElement);
                if (toElement == null || !toElement.IsDescendantOf(par)) par.SetOverNode(null);
            }
        }

        #endregion

        #region NodeSelection

        public class NodeSelection : Selection<Node>
        {
            public Tree tree;

            protected override void Changed()
            {
                switch (Size())
                {
                    case 0:
                        tree.rangeStart = null;
                        break;
                    case 1:
                        tree.rangeStart = First();
                        break;
                }
            }
        }

        #endregion

        #region Node

        public class Node
        {
            internal readonly Element element;
            public Node parent;
            internal readonly List<Node> children = new List<Node>(0);
            internal bool selectable = true;
            internal bool expanded;
            internal IDrawable icon;
            internal float height;
            internal object obj;

            public Action<bool> OnExpanded;

            public Node(Element element)
            {
                this.element = element ?? throw new ArgumentNullException("element cannot be null.");
            }

            public void SetExpanded(bool expanded)
            {
                if (expanded == this.expanded) return;
                this.expanded = expanded;
                if (children.Count == 0) return;
                Tree tree = GetTree();
                if (tree == null) return;
                if (expanded)
                {
                    for (int i = 0, n = children.Count; i < n; i++)
                        children[i].AddToTree(tree);
                }
                else
                {
                    for (int i = children.Count - 1; i >= 0; i--)
                        children[i].RemoveFromTree(tree);
                }
                tree.InvalidateHierarchy();
            }

            /** Called to add the element to the tree when the node's parent is expanded. */
            internal void AddToTree(Tree tree)
            {
                tree.AddElement(element);
                if (!expanded) return;
                object[] children = this.children.ToArray();
                for (int i = this.children.Count - 1; i >= 0; i--)
                    ((Node)children[i]).AddToTree(tree);
            }

            /** Called to remove the element from the tree when the node's parent is collapsed. */
            internal void RemoveFromTree(Tree tree)
            {
                tree.RemoveElement(element);
                if (!expanded) return;
                object[] children = this.children.ToArray();
                for (int i = this.children.Count - 1; i >= 0; i--)
                    ((Node)children[i]).RemoveFromTree(tree);
            }

            public void Add(Node node)
            {
                Insert(children.Count, node);
            }

            public void AddAll(List<Node> nodes)
            {
                for (int i = 0, n = nodes.Count; i < n; i++)
                    Insert(children.Count, nodes[i]);
            }

            public void Insert(int index, Node node)
            {
                node.parent = this;
                children.Insert(index, node);
                UpdateChildren();
            }

            public void Remove()
            {
                Tree tree = GetTree();
                if (tree != null)
                    tree.Remove(this);
                else if (parent != null) //
                    parent.Remove(this);
            }

            public void Remove(Node node)
            {
                children.Remove(node);
                if (!expanded) return;
                Tree tree = GetTree();
                if (tree == null) return;
                node.RemoveFromTree(tree);
                if (children.Count == 0) expanded = false;
            }

            public void RemoveAll()
            {
                Tree tree = GetTree();
                if (tree != null)
                {
                    object[] children = this.children.ToArray();
                    for (int i = this.children.Count - 1; i >= 0; i--)
                        ((Node)children[i]).RemoveFromTree(tree);
                }
                children.Clear();
            }

            /** Returns the tree this node is currently in, or null. */
            public Tree GetTree()
            {
                Group parent = element.GetParent();
                if (!(parent is Tree)) return null;
                return (Tree)parent;
            }

            public Element GetElement()
            {
                return element;
            }

            public bool IsExpanded()
            {
                return expanded;
            }

            /** If the children order is changed, {@link #updateChildren()} must be called. */
            public List<Node> GetChildren()
            {
                return children;
            }

            /** Adds the child node actors to the tree again. This is useful after changing the order of {@link #getChildren()}. */
            public void UpdateChildren()
            {
                if (!expanded) return;
                Tree tree = GetTree();
                if (tree == null) return;
                for (int i = children.Count - 1; i >= 0; i--)
                    children[i].RemoveFromTree(tree);
                for (int i = 0, n = children.Count; i < n; i++)
                    children[i].AddToTree(tree);
            }

            /** @return May be null. */
            public Node GetParent()
            {
                return parent;
            }

            /** Sets an icon that will be drawn to the left of the element. */
            public void SetIcon(IDrawable icon)
            {
                this.icon = icon;
            }

            public object GetObject()
            {
                return obj;
            }

            /** Sets an application specific object for this node. */
            public void SetObject(object obj)
            {
                this.obj = obj;
            }

            public IDrawable GetIcon()
            {
                return icon;
            }

            public int GetLevel()
            {
                int level = 0;
                Node current = this;
                do
                {
                    level++;
                    current = current.GetParent();
                } while (current != null);
                return level;
            }

            /** Returns this node or the child node with the specified object, or null. */
            public Node FindNode(object obj)
            {
                if (obj == null) throw new ArgumentNullException("object cannot be null.");
                if (obj.Equals(this.obj)) return this;
                return Tree.FindNode(children, obj);
            }

            /** Collapses all nodes under and including this node. */
            public void CollapseAll()
            {
                SetExpanded(false);
                Tree.CollapseAll(children);
            }

            /** Expands all nodes under and including this node. */
            public void ExpandAll()
            {
                SetExpanded(true);
                if (children.Count > 0) Tree.ExpandAll(children);
            }

            /** Expands all parent nodes of this node. */
            public void ExpandTo()
            {
                Node node = parent;
                while (node != null)
                {
                    node.SetExpanded(true);
                    node = node.parent;
                }
            }

            public bool IsSelectable()
            {
                return selectable;
            }

            public void SetSelectable(bool selectable)
            {
                this.selectable = selectable;
            }

            public void FindExpandedObjects(List<object> objects)
            {
                if (expanded && !Tree.FindExpandedObjects(children, objects)) objects.Add(obj);
            }

            public void RestoreExpandedObjects(List<object> objects)
            {
                for (int i = 0, n = objects.Count; i < n; i++)
                {
                    Node node = FindNode(objects[i]);
                    if (node != null)
                    {
                        node.SetExpanded(true);
                        node.ExpandTo();
                    }
                }
            }

            /** Returns the height of the node as calculated for layout. A subclass may override and increase the returned height to
             * create a blank space in the tree above the node, eg for a separator. */
            public float GetHeight()
            {
                return height;
            }
        }


        #endregion

    }

    /// <summary>
    /// The style for a tree
    /// </summary>
    public class TreeStyle
    {
        public IDrawable plus, minus;
        /** Optional. */
        public IDrawable over, selection, background;

        public TreeStyle()
        {
        }

        public TreeStyle(IDrawable plus, IDrawable minus, IDrawable selection)
        {
            this.plus = plus;
            this.minus = minus;
            this.selection = selection;
        }

        public TreeStyle(TreeStyle style)
        {
            this.plus = style.plus;
            this.minus = style.minus;
            this.selection = style.selection;
        }
    }
}
