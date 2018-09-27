using System.Collections.Generic;

namespace BlueberryCore.UI
{
    public interface IUndoable<T>
    {
        void Undo();

        void Redo();

        void AddNewState(T state);

        bool CanUndo();

        bool CanRedo();
    }
}
