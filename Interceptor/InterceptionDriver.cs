using System;
using System.Runtime.InteropServices;
// ReSharper disable UnusedMember.Global

namespace Interceptor
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int Predicate(int device);

    [Flags]
    public enum KeyState : ushort
    {
        Down = 0x00,
        Up = 0x01,
        E0 = 0x02,
        E1 = 0x04,
        TermsrvSetLED = 0x08,
        TermsrvShadow = 0x10,
        TermsrvVKPacket = 0x20
    }

    [Flags]
    public enum KeyboardFilterMode : ushort
    {
        None = 0x0000,
        All = 0xFFFF,
        KeyDown = KeyState.Up,
        KeyUp = KeyState.Up << 1,
        KeyE0 = KeyState.E0 << 1,
        KeyE1 = KeyState.E1 << 1,
        KeyTermsrvSetLED = KeyState.TermsrvSetLED << 1,
        KeyTermsrvShadow = KeyState.TermsrvShadow << 1,
        KeyTermsrvVKPacket = KeyState.TermsrvVKPacket << 1
    }

    [Flags]
    public enum MouseState : ushort
    {
        Moving = 0x00,
        LeftDown = 0x01,
        LeftUp = 0x02,
        RightDown = 0x04,
        RightUp = 0x08,
        MiddleDown = 0x10,
        MiddleUp = 0x20,
        LeftExtraDown = 0x40,
        LeftExtraUp = 0x80,
        RightExtraDown = 0x100,
        RightExtraUp = 0x200,
        ScrollVertical = 0x400,
        ScrollUp = 0x400,
        ScrollDown = 0x400,
        ScrollHorizontal = 0x800,
        ScrollLeft = 0x800,
        ScrollRight = 0x800
    }

    [Flags]
    public enum MouseFilterMode : ushort
    {
        None = 0x0000,
        All = 0xFFFF,
        LeftDown = 0x01,
        LeftUp = 0x02,
        RightDown = 0x04,
        RightUp = 0x08,
        MiddleDown = 0x10,
        MiddleUp = 0x20,
        LeftExtraDown = 0x40,
        LeftExtraUp = 0x80,
        RightExtraDown = 0x100,
        RightExtraUp = 0x200,
        MouseWheelVertical = 0x400,
        MouseWheelHorizontal = 0x800,
        MouseMove = 0x1000
    }

    [Flags]
    public enum MouseFlags : ushort
    {
        MoveRelative = 0x000,
        MoveAbsolute = 0x001,
        VirtualDesktop = 0x002,
        AttributesChanged = 0x004,
        MoveWithoutCoalescing = 0x008,
        TerminalServicesSourceShadow = 0x100
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MouseStroke
    {
        public MouseState State;
        public MouseFlags Flags;
        public short Rolling;
        public int X;
        public int Y;
        public uint Information;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct KeyStroke
    {
        public Keys Code;
        public KeyState State;
        public uint Information;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct Stroke
    {
        [FieldOffset(0)] public MouseStroke Mouse;

        [FieldOffset(0)] public KeyStroke Key;
    }

    /// <summary>
    ///     The .NET wrapper class around the C++ library interception.dll.
    /// </summary>
    public static class InterceptionDriver
    {
        [DllImport("interception.dll", EntryPoint = "interception_create_context",
            CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr CreateContext();

        [DllImport("interception.dll", EntryPoint = "interception_destroy_context",
            CallingConvention = CallingConvention.Cdecl)]
        public static extern void DestroyContext(IntPtr context);

        [DllImport("interception.dll", EntryPoint = "interception_get_precedence",
            CallingConvention = CallingConvention.Cdecl)]
        public static extern void GetPrecedence(IntPtr context, int device);

        [DllImport("interception.dll", EntryPoint = "interception_set_precedence",
            CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetPrecedence(IntPtr context, int device, int precedence);

        [DllImport("interception.dll", EntryPoint = "interception_get_filter",
            CallingConvention = CallingConvention.Cdecl)]
        public static extern ushort GetFilter(IntPtr context, int device);

        [DllImport("interception.dll", EntryPoint = "interception_set_filter",
            CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetFilter(IntPtr context, Predicate predicate, ushort keyboardFilterMode);

        [DllImport("interception.dll", EntryPoint = "interception_wait", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Wait(IntPtr context);

        [DllImport("interception.dll", EntryPoint = "interception_wait_with_timeout",
            CallingConvention = CallingConvention.Cdecl)]
        public static extern int WaitWithTimeout(IntPtr context, ulong milliseconds);

        [DllImport("interception.dll", EntryPoint = "interception_send", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Send(IntPtr context, int device, ref Stroke stroke, uint numStrokes);

        [DllImport("interception.dll", EntryPoint = "interception_receive",
            CallingConvention = CallingConvention.Cdecl)]
        public static extern int Receive(IntPtr context, int device, ref Stroke stroke, uint numStrokes);

        [DllImport("interception.dll", EntryPoint = "interception_get_hardware_id",
            CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetHardwareId(IntPtr context, int device, string hardwareIdentifier,
            uint sizeOfString);

        [DllImport("interception.dll", EntryPoint = "interception_is_invalid",
            CallingConvention = CallingConvention.Cdecl)]
        public static extern int IsInvalid(int device);

        [DllImport("interception.dll", EntryPoint = "interception_is_keyboard",
            CallingConvention = CallingConvention.Cdecl)]
        private static extern int _IsKeyboard(int device);

        public static int IsKeyboard(int device)
        {
            return _IsKeyboard(device);
        }

        [DllImport("interception.dll", EntryPoint = "interception_is_mouse",
            CallingConvention = CallingConvention.Cdecl)]
        private static extern int _IsMouse(int device);

        public static int IsMouse(int device)
        {
            return _IsMouse(device);
        }
    }
}