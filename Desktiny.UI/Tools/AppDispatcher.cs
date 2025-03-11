using Microsoft.UI.Dispatching;

namespace Desktiny.UI.Tools
{
    public static class AppDispatcher
    {
        public static DispatcherQueue UIThreadDispatcher { get; set; }
    }
}
