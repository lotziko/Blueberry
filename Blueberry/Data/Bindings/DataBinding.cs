using System;

namespace Blueberry.DataTools
{
    public class DataBinding<T> : IDataBinding<T>
    {
        private T value;

        public event Action<object> OnChange;

        public T Value
        {
            get => value;
            set
            {
                if (!this.value.Equals(value))
                {
                    this.value = value;
                    OnChange?.Invoke(value);
                }
            }
        }

        object IDataBinding.Value
        {
            get => value;
            set
            {
                if (!this.value.Equals(value))
                {
                    this.value = (T)value;
                    OnChange?.Invoke(value);
                }
            }
        }
    }
}
