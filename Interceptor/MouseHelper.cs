using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Interceptor
{
    public enum MouseState
    {
        LeftDown = 1,
        LeftUp = 2,
        RightDown = 4,
        RightUp = 8,
        MiddleDown = 16,
        MiddleUp = 32,
        ScrollUp = 1024,
        ScrollDown = 1024
    }

    public enum MouseFlags
    {
        MoveRelative = 0x000,
        MoveAbsolute = 0x001,
        VirtualDesktop = 0x002,
        AttributesChanged = 0x004,
        MoveWithoutCoalescing = 0x008,
        TerminalServicesSourceShadow = 0x100
    }

    public static class MouseHelper
    {
    }
}
