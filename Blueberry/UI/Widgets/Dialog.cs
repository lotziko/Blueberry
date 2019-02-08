using System.Collections.Generic;

namespace Blueberry.UI
{
    public class Dialog : Window
    {
        Table contentTable, buttonTable;
        //private Skin skin;
	    Dictionary<Element, object> values = new Dictionary<Element, object>();
        bool cancelHide;
        Element previousKeyboardFocus, previousScrollFocus;
        FocusListener focusListener;

        protected InputListener ignoreTouchDown = new IgnoreListener();

        #region Listener

        private class IgnoreListener : InputListener
        {
            public override bool TouchDown(InputEvent ev, float x, float y, int pointer, int button)
            {
                ev.Cancel();
                return false;
            }
        }

        #endregion

        public Dialog(string title, Skin skin, string stylename = "default") : base(title, skin, stylename)
        {
            //this.skin = VisUI.getSkin();
            //setSkin(skin);
            Initialize();
        }

        public Dialog(string title, WindowStyle windowStyle) : base(title, windowStyle)
        {
            //this.skin = VisUI.getSkin();
            //setSkin(skin);
            Initialize();
        }

        private void Initialize()
        {
            SetModal(true);
            GetTitleLabel().SetAlign(UI.Align.center);

            Defaults().Space(6);
            Add(contentTable = new Table()).Expand().Fill();
            Row();
            Add(buttonTable = new Table());

            contentTable.Defaults().Space(2).PadLeft(3).PadRight(3);
            buttonTable.Defaults().Space(6).PadBottom(3);

            AddListener(new Change(this));
            focusListener = new Focus(this);
        }

        #region Listeners

        private class Change : ChangeListener
        {
            private readonly Dialog d;

            public Change(Dialog d)
            {
                this.d = d;
            }

            public override void Changed(ChangeEvent ev, Element element)
            {
                if (!d.values.ContainsKey(element)) return;
                while (element.GetParent() != d.buttonTable)
                    element = element.GetParent();
                d.Result(d.values[element]);
                if (!d.cancelHide) d.Hide();
                d.cancelHide = false;
            }
        }

        private class Focus : FocusListener
        {
            private readonly Dialog d;

            public Focus(Dialog d)
            {
                this.d = d;
            }

            public override void ScrollFocusChanged(FocusEvent ev, Element element, bool focused)
            {
                if (!focused) FocusChanged(ev);
            }

            public override void KeyboardFocusChanged(FocusEvent ev, Element element, bool focused)
            {
                if (!focused) FocusChanged(ev);
            }

            private void FocusChanged(FocusEvent ev)
            {
                var stage = d.GetStage();
                if (d.IsModal() && stage != null && stage.Root.GetElements().Count > 0
                        && stage.Root.GetElements().Peek() == d) { // Dialog is top most actor.
                    var newFocusedElement = ev.GetRelatedElement();
                    if (newFocusedElement != null && !newFocusedElement.IsDescendantOf(d)) ev.Cancel();
                }
            }
        }

        #endregion

        public override void SetStage(Stage stage)
        {
            if (stage == null)
                AddListener(focusListener);
            else
                RemoveListener(focusListener);
            base.SetStage(stage);
        }

        public Table GetContentTable()
        {
            return contentTable;
        }

        public Table GetButtonsTable()
        {
            return buttonTable;
        }


        /*
         * building
         *
         */

        /** {@link #pack() Packs} the dialog and adds it to the stage with custom action which can be null for instant show */
        public Dialog Show(Stage stage, Action action)
        {
            ClearActions();
            RemoveCaptureListener(ignoreTouchDown);

            previousKeyboardFocus = null;
            var element = stage.GetKeyboardFocus();
            if (element != null && !element.IsDescendantOf(this)) previousKeyboardFocus = element;

            previousScrollFocus = null;
            element = stage.GetScrollFocus();
            if (element != null && !element.IsDescendantOf(this)) previousScrollFocus = element;

            Pack();
            stage.AddElement(this);
            stage.SetKeyboardFocus(this);
            stage.SetScrollFocus(this);
            if (action != null) AddAction(action);

            return this;
        }

        /** {@link #pack() Packs} the dialog and adds it to the stage, centered with default fadeIn action */
        public Dialog Show(Stage stage)
        {
            Show(stage, Actions.Sequence(Actions.Alpha(0), Actions.FadeIn(0.4f, Interpolation.fade)));
            SetPosition(MathF.Round((stage.Width - GetWidth()) / 2), MathF.Round((stage.Height - GetHeight()) / 2));
            return this;
        }

        /** Hides the dialog with the given action and then removes it from the stage. */
        public void Hide(Action action)
        {
            var stage = GetStage();
            if (stage != null)
            {
                RemoveListener(focusListener);
                if (previousKeyboardFocus != null && previousKeyboardFocus.GetStage() == null) previousKeyboardFocus = null;
                var element = stage.GetKeyboardFocus();
                if (element == null || element.IsDescendantOf(this)) stage.SetKeyboardFocus(previousKeyboardFocus);

                if (previousScrollFocus != null && previousScrollFocus.GetStage() == null) previousScrollFocus = null;
                element = stage.GetScrollFocus();
                if (element == null || element.IsDescendantOf(this)) stage.SetScrollFocus(previousScrollFocus);
            }
            if (action != null)
            {
                AddCaptureListener(ignoreTouchDown);
                AddAction(Actions.Sequence(action, Actions.RemoveListener(ignoreTouchDown, true), Actions.RemoveElement()));
            }
            else
                Remove();
        }

        /**
	     * Hides the dialog. Called automatically when a button is clicked. The default implementation fades out the dialog over 400
	     * milliseconds and then removes it from the stage.
	     */
        public void Hide()
        {
            Hide(Actions.Sequence(Actions.FadeOut(FADE_TIME, Interpolation.fade), Actions.RemoveListener(ignoreTouchDown, true), Actions.RemoveElement()));
        }

        public void SetObject(Element element, object obj)
        {
            values.Add(element, obj);
        }

        public Dialog Key(int keycode, object obj)
        {
            AddListener(new KeyListener(this, keycode, obj));
            return this;
        }

        #region Listener

        private class KeyListener : InputListener
        {
            private readonly Dialog d;
            private readonly int keycode;
            private readonly object obj;

            public KeyListener(Dialog d, int keycode, object obj)
            {
                this.d = d;
                this.keycode = keycode;
                this.obj = obj;
            }

            public override bool KeyDown(InputEvent ev, int keycode2)
            {
                if (keycode == keycode2)
                {
                    d.Result(obj);
                    if (!d.cancelHide) d.Hide();
                    d.cancelHide = false;
                }
                return false;
            }
        }

        #endregion

        /**
	     * Called when a button is clicked. The dialog will be hidden after this method returns unless {@link #cancel()} is called.
	     * @param object The object specified when the button was added.
	     */
        protected void Result(object obj)
        {

        }

        public void Cancel()
        {
            cancelHide = true;
        }
    }
}
