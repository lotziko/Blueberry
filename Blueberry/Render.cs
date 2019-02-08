
namespace Blueberry
{
    public static class Render
    {
        public static bool Requested { get; private set; } = true;
        public static bool NeedsRequest { get; set; }

        public static void Request()
        {
            Requested = true;
        }

        public static void Complete()
        {
            Requested = false;
        }
    }
}
