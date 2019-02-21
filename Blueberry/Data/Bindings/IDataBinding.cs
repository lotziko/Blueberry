using System;

namespace Blueberry.DataTools
{
    public interface IDataBinding
    {
        event Action<object> OnChange;

        object Value { get; set; }

        /// <summary>
        /// Force call a change event
        /// </summary>
        void ForceChange();
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
