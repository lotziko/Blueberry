
namespace BlueberryCore.UI
{
    public interface ILayout
    {
        float PreferredWidth { get; }

        float PreferredHeight { get; }

        float MinWidth { get; }

        float MinHeight { get; }

        float MaxWidth { get; }

        float MaxHeight { get; }

        bool FillParent { get; }

        void InvalidateHierarchy();

        void Validate();

        void Invalidate();

        void Layout();

        void Pack();
    }
}
