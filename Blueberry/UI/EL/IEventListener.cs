
namespace Blueberry.UI
{
    public interface IEventListener
    {
        /// <summary>
        /// Try to handle the given event, if it is applicable.
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        bool Handle(Event e);

    }
}