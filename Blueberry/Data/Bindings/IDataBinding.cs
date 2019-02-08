using System;

namespace Blueberry.DataTools
{
    public interface IDataBinding
    {
        event Action<object> OnChange;

        object Value { get; set; }
    }

    public interface IDataBinding<T> : IDataBinding
    {
        new T Value { get; set; }
    }

    public enum BindingSide
    {
        OneSided, TwoSided
    }
}
