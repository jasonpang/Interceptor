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

    public static class MouseHelper
    {
    }
}
