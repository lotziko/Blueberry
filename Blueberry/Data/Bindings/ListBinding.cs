using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Blueberry.DataTools
{
    public class ListBinding<T> : IDataBinding<ObservableCollection<T>>
    {
        private ObservableCollection<T> value;

        public event Action<object> OnChange;

        public ObservableCollection<T> Value
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
                    this.value = (ObservableCollection<T>)value;
                    OnChange?.Invoke(value);
                }
            }
        }

        public ListBinding()
        {
            value = new ObservableCollection<T>();
            value.CollectionChanged += Value_CollectionChanged;
        }

        private void Value_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnChange?.Invoke(value);
        }
    }
}
