using BlueberryCore.UI;
using System;
using System.Collections.Generic;
using System.IO;

namespace BlueberryEditor.UIElements
{
    /// <summary>
    /// Has left/right go button, file icons and ignore extension list
    /// </summary>
    public class FolderTree : Window, IUndoable<string>
    {
        private FolderTreeStyle style;
        protected string currentPath = "";
        protected readonly List<string> extensionsToAvoid = new List<string>();
        protected Tree tree;
        protected TextField currPathField;
        protected ImageButton left, right, parentFolder;
        protected ScrollPane scrollpane;

        #region IUndoable

        protected List<string> undoStack = new List<string>(), redoStack = new List<string>();

        public void AddNewState(string state)
        {
            redoStack.Clear();
            if (currentPath != "")
                undoStack.Add(currentPath);

            /*if (currState < states.Count - 1)
                states.RemoveRange(currState + 1, states.Count - currState - 1);
            states.Add(state);
            currState = states.Count - 1;*/
        }

        public void Undo()
        {
            if (CanUndo())
            {
                var last = undoStack[undoStack.Count - 1];
                redoStack.Add(currentPath);
                undoStack.Remove(last);
                SetPath(last);
            }   
        }

        public void Redo()
        {
            if (CanRedo())
            {
                var last = redoStack[redoStack.Count - 1];
                undoStack.Add(currentPath);
                redoStack.Remove(last);
                SetPath(last);
            }
        }

        public bool CanUndo()
        {
            return undoStack.Count > 0;
        }

        public bool CanRedo()
        {
            return redoStack.Count > 0;
        }

        #endregion
        
        public FolderTree(string defaultPath, FolderTreeStyle style) : base("Resource tree", style.windowStyle)
        {
            SetStyle(style);
            SetSize(PreferredWidth, PreferredHeight);
            
            var buttonsBar = new HorizontalGroup();
            buttonsBar.Space(5f);
            Add(buttonsBar).FillX().ExpandX().Row();

            left = new ImageButton(style.arrowLeft, new ImageButtonStyle(style.buttonStyle));
            left.OnClicked += (a) =>
            {
                Undo();
                RefreshUndoRedoButtons();
            };
            buttonsBar.AddElement(left);

            right = new ImageButton(style.arrowRight, new ImageButtonStyle(style.buttonStyle));
            right.OnClicked += (a) =>
            {
                Redo();
                RefreshUndoRedoButtons();
            };
            buttonsBar.AddElement(right);

            parentFolder = new ImageButton(style.parentFolder, new ImageButtonStyle(style.buttonStyle));
            parentFolder.OnClicked += (a) =>
            {
                var newPath = Directory.GetParent(currentPath.TrimEnd('/'))?.FullName.Replace('\\', '/');
                if (newPath != null && !newPath.EndsWith("/"))
                    newPath += '/';
                if (Directory.Exists(newPath))
                {
                    AddNewState(newPath);
                    SetPath(newPath);
                    RefreshUndoRedoButtons();
                }
            };
            buttonsBar.AddElement(parentFolder);

            currPathField = new TextField("", style.fieldStyle);
            Add(currPathField).PadTop(5f).PadBottom(5f).FillX().ExpandX().Row();

            tree = new ClickableTree(this, style.treeStyle);
            tree.SetYSpacing(0);
            tree.GetSelection().SetMultiple(false);

            scrollpane = new ScrollPane(tree, style.scrollPaneStyle);
            scrollpane.SetFadeScrollBars(false);
            scrollpane.SetOverscroll(false, false);
            Add(scrollpane).Fill().Expand();

            AddNewState(defaultPath);
            SetPath(defaultPath);
            
            RefreshUndoRedoButtons();
        }

        public virtual void RefreshUndoRedoButtons()
        {
            if (CanUndo())
                left.SetDisabled(false);
            else
                left.SetDisabled(true);

            if (CanRedo())
                right.SetDisabled(false);
            else
                right.SetDisabled(true);
        }

        public void SetPath(string path)
        {
            if (!Directory.Exists(path))
                return;//throw new Exception("Directory does not exist");

            tree.ClearElements();
            var directories = Directory.GetDirectories(path);
            var files = Directory.GetFiles(path);

            foreach(var directory in directories)
            {
                tree.Add(CreateDirectoryNode(directory));
            }
            foreach (var file in files)
            {
                tree.Add(CreateFileNode(file));
            }

            currentPath = path;
            currPathField.SetText(currentPath);

            var par = Directory.GetParent(currentPath.TrimEnd('/'));
            if (par == null)
                parentFolder.SetDisabled(true);
            else
                parentFolder.SetDisabled(false);
            scrollpane.SetScrollY(0);
            scrollpane.SetScrollY(0);
        }
        
        protected virtual Tree.Node CreateDirectoryNode(string directory)
        {
            var info = new DirectoryInfo(directory);
            style.icons.TryGetValue("folder", out var icon);

            var pane = new FileDirectoryLine(icon == null ? null : new Image(icon, Scaling.Fit), new Label(info.Name, style.labelStyle), style.iconMaxWidth, style.iconMaxHeight);
            var node = new Tree.Node(pane);
            node.Add(new Tree.Node(new Element()));
            return node;
        }

        protected virtual Tree.Node CreateFileNode(string file)
        {
            var info = new FileInfo(file);
            style.icons.TryGetValue(info.Extension, out var icon);

            if (icon == null)
                style.icons.TryGetValue("unknown", out icon);

            var pane = new FileDirectoryLine(icon == null ? null : new Image(icon, Scaling.Fit), new Label(info.Name, style.labelStyle), style.iconMaxWidth, style.iconMaxHeight);
            return new Tree.Node(pane);
        }

        public virtual void SetStyle(FolderTreeStyle style)
        {
            this.style = style;
        }

        public void SetExtensionsToAvoid(params string[] extensions)
        {
            if (extensionsToAvoid.Count > 0)
                extensionsToAvoid.Clear();
            extensionsToAvoid.AddRange(extensions);
        }

        protected class ClickableTree : Tree
        {
            protected readonly FolderTree tree;

            public ClickableTree(FolderTree tree, Skin skin, string stylename = "default") : this(tree, skin.Get<TreeStyle>(stylename))
            {
            }

            public ClickableTree(FolderTree tree, TreeStyle style) : base(style)
            {
                this.tree = tree;
                AddListener(new Listener(this));
            }

            #region Listener

            private class Listener : ClickListener
            {
                private ClickableTree crt;

                public Listener(ClickableTree crt)
                {
                    this.crt = crt;
                }

                public override void Clicked(InputEvent ev, float x, float y)
                {
                    if (GetTapCount() == 2)
                    {
                        var node = crt.GetNodeAt(y);
                        if (node != null)
                        {
                            var currNodeText = ((FileDirectoryLine)node.GetElement()).text.GetText();
                            var newPath = crt.tree.currentPath + currNodeText + "/";
                            if (Directory.Exists(newPath))
                            {
                                crt.tree.AddNewState(newPath);
                                crt.tree.SetPath(newPath);
                            }
                        }
                    }
                }
            }

            #endregion


        }

        protected class FileDirectoryLine : Table
        {
            public Label text;
            public Image icon;

            public FileDirectoryLine(Image icon, Label text, int iconMaxWidth = 0, int iconMaxHeight = 0)
            {
                var iconCell = Add(this.icon = icon);
                if (iconMaxWidth != 0)
                    iconCell.Width(iconMaxWidth);
                if (iconMaxHeight != 0)
                    iconCell.Height(iconMaxHeight);

                Add(this.text = text).PadLeft(5f);
            }
        }
    }

    public class FolderTreeStyle
    {
        public WindowStyle windowStyle;
        public ScrollPaneStyle scrollPaneStyle;
        public TreeStyle treeStyle;
        public ImageButtonStyle buttonStyle;
        public TextFieldStyle fieldStyle;
        public LabelStyle labelStyle;
        public IDrawable arrowLeft, arrowRight, parentFolder;
        public int iconMaxWidth, iconMaxHeight;
        public Dictionary<string, IDrawable> icons = new Dictionary<string, IDrawable>();
    }
}
