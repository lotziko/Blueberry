using System;

namespace Blueberry.DataTools
{
    public class DataBinding<T> : IDataBinding<T>
    {
        protected T value;

        public event Action<object> OnChange;

        public virtual T Value
        {
            get => value;
            set
            {
                if (this.value == null || !this.value.Equals(value))
                {
                    this.value = value;
                    CallChangeEvent(value);
                }
            }
        }

        object IDataBinding.Value
        {
            get => value;
            set
            {
                if (this.value == null || !this.value.Equals(value))
                {
                    this.value = (T)value;
                    CallChangeEvent(value);
                }
            }
        }

        protected void CallChangeEvent(object value)
        {
            OnChange?.Invoke(value);
        }

        public void ForceChange()
        {
            CallChangeEvent(Value);
        }
    }
}
