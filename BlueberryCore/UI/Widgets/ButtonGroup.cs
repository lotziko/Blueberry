using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlueberryCore.UI
{
    public class ButtonGroup
    {
        private readonly List<Button> buttons = new List<Button>();
        private List<Button> checkedButtons = new List<Button>(1);
        private int minCheckCount, maxCheckCount = 1;
        private bool uncheckLast = true;
        private Button lastChecked;

        public ButtonGroup()
        {
            minCheckCount = 1;
        }

        public ButtonGroup(params Button[] buttons)
        {
            minCheckCount = 0;
            Add(buttons);
            minCheckCount = 1;
        }

        public void Add(Button button)
        {
            if (button == null) throw new ArgumentNullException("button cannot be null.");
            button.buttonGroup = null;
            var shouldCheck = button.isChecked || buttons.Count < minCheckCount;
            button.SetChecked(false, false);
            button.buttonGroup = this;
            buttons.Add(button);
            button.SetChecked(shouldCheck, true);
        }

        public void Add(params Button[] buttons)
        {
            if (buttons == null) throw new ArgumentNullException("buttons cannot be null.");
            for (int i = 0, n = buttons.Length; i < n; i++)
               Add(buttons[i]);
        }

        public void Remove(Button button)
        {
            if (button == null) throw new ArgumentNullException("button cannot be null.");
            button.buttonGroup = null;
            buttons.Remove(button);
            checkedButtons.Remove(button);
        }

        public void Remove(params Button[] buttons)
        {
            if (buttons == null) throw new ArgumentNullException("buttons cannot be null.");
            for (int i = 0, n = buttons.Length; i < n; i++)
                Remove(buttons[i]);
        }

        public void Clear()
        {
            buttons.Clear();
            checkedButtons.Clear();
        }

        /// <summary>
        /// Sets the first TextButton with the specified text to checked.
        /// </summary>
        /// <param name="text">Text on TextButton</param>
        public void SetChecked(String text)
        {
            if (text == null) throw new ArgumentNullException("text cannot be null.");
            for (int i = 0, n = buttons.Count; i < n; i++)
            {
                Button button = buttons[i];
                if (button is TextButton && text == (button as TextButton).GetText())
                {
                button.SetChecked(true, true);
                return;
                }
            }
        }

        /// <summary>
        /// Called when a button is checked or unchecked. If overridden, generally changing button checked states should not be done from within this method.
        /// </summary>
        /// <param name="button"></param>
        /// <param name="newState"></param>
        /// <returns>True if the new state should be allowed.</returns>
        protected bool CanCheck(Button button, bool newState)
        {
            if (button.isChecked == newState) return false;

            if (!newState)
            {
                // Keep button checked to enforce minCheckCount.
                if (checkedButtons.Count <= minCheckCount) return false;
                checkedButtons.Remove(button);
            }
            else
            {
                // Keep button unchecked to enforce maxCheckCount.
                if (maxCheckCount != -1 && checkedButtons.Count >= maxCheckCount)
                {
                    if (uncheckLast)
                    {
                        int old = minCheckCount;
                        minCheckCount = 0;
                        lastChecked.SetChecked(false, true);
                        minCheckCount = old;
                    }
                    else
                        return false;
                }
                checkedButtons.Add(button);
                lastChecked = button;
            }

            return true;
        }

        public void UncheckAll()
        {
            int old = minCheckCount;
            minCheckCount = 0;
            for (int i = 0, n = buttons.Count; i < n; i++)
            {
                Button button = buttons[i];
                button.SetChecked(false, true);
            }
            minCheckCount = old;
        }

        public Button GetChecked()
        {
            if (checkedButtons.Count > 0) return checkedButtons[0];
            return null;
        }

        /** @return The first checked button index, or -1. */
        public int GetCheckedIndex()
        {
            if (checkedButtons.Count > 0) return buttons.IndexOf(checkedButtons[0]);
            return -1;
        }

        public List<Button> GetAllChecked()
        {
            return checkedButtons;
        }

        public List<Button> GetButtons()
        {
            return buttons;
        }

        /** Sets the minimum number of buttons that must be checked. Default is 1. */
        public void SetMinCheckCount(int minCheckCount)
        {
            this.minCheckCount = minCheckCount;
        }

        /** Sets the maximum number of buttons that can be checked. Set to -1 for no maximum. Default is 1. */
        public void SetMaxCheckCount(int maxCheckCount)
        {
            if (maxCheckCount == 0) maxCheckCount = -1;
            this.maxCheckCount = maxCheckCount;
        }

        /** If true, when the maximum number of buttons are checked and an additional button is checked, the last button to be checked
         * is unchecked so that the maximum is not exceeded. If false, additional buttons beyond the maximum are not allowed to be
         * checked. Default is true. */
        public void SetUncheckLast(bool uncheckLast)
        {
            this.uncheckLast = uncheckLast;
        }
    }
}
