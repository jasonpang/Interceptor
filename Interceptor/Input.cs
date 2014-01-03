using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Interceptor
{
    public class Input
    {
        private IntPtr context;
        private Thread callbackThread;

        /// <summary>
        /// Determines whether the driver traps no keyboard events, all events, or a range of events in-between (down only, up only...etc). Set this before loading otherwise the driver will not filter any events and no keypresses can be sent.
        /// </summary>
        public KeyboardFilterMode KeyboardFilterMode { get; set; }
        
        /// <summary>
        /// Determines whether the driver traps no events, all events, or a range of events in-between. Set this before loading otherwise the driver will not filter any events and no mouse clicks can be sent.
        /// </summary>
        public MouseFilterMode MouseFilterMode { get; set; }

        public bool IsLoaded { get; set; }

        /// <summary>
        /// Gets or sets the delay in milliseconds after each key stroke down and up. Pressing a key requires both a key stroke down and up. A delay of 0 (inadvisable) may result in no keys being apparently pressed. A delay of 20 - 40 milliseconds makes the key presses visible.
        /// </summary>
        public int KeyPressDelay { get; set; }

        /// <summary>
        /// Gets or sets the delay in milliseconds after each mouse event down and up. 'Clicking' the cursor (whether left or right) requires both a mouse event down and up. A delay of 0 (inadvisable) may result in no apparent click. A delay of 20 - 40 milliseconds makes the clicks apparent.
        /// </summary>
        public int ClickDelay { get; set; }

        public int ScrollDelay { get; set; }

        public event EventHandler<KeyPressedEventArgs> OnKeyPressed;
        public event EventHandler<MousePressedEventArgs> OnMousePressed;

        private int deviceId; /* Very important; which device the driver sends events to */

        public Input()
        {
            context = IntPtr.Zero;

            KeyboardFilterMode = KeyboardFilterMode.None;
            MouseFilterMode = MouseFilterMode.None;

            KeyPressDelay = 1;
            ClickDelay = 1;
            ScrollDelay = 15;
        }

        /*
         * Attempts to load the driver. You may get an error if the C++ library 'interception.dll' is not in the same folder as the executable and other DLLs. MouseFilterMode and KeyboardFilterMode must be set before Load() is called. Calling Load() twice has no effect if already loaded.
         */
        public bool Load()
        {
            if (IsLoaded) return false;

            context = InterceptionDriver.CreateContext();

            if (context != IntPtr.Zero)
            {
                callbackThread = new Thread(new ThreadStart(DriverCallback));
                callbackThread.Priority = ThreadPriority.Highest;
                callbackThread.IsBackground = true;
                callbackThread.Start();

                IsLoaded = true;

                return true;
            }
            else
            {
                IsLoaded = false;

                return false;
            }
        }

        /*
         * Safely unloads the driver. Calling Unload() twice has no effect.
         */
        public void Unload()
        {
            if (!IsLoaded) return;

            if (context != IntPtr.Zero)
            {
                callbackThread.Abort();
                InterceptionDriver.DestroyContext(context);
                IsLoaded = false;
            }
        }

        private void DriverCallback()
        {
            InterceptionDriver.SetFilter(context, InterceptionDriver.IsKeyboard, (Int32) KeyboardFilterMode);
            InterceptionDriver.SetFilter(context, InterceptionDriver.IsMouse, (Int32) MouseFilterMode);

            Stroke stroke = new Stroke();

            while (InterceptionDriver.Receive(context, deviceId = InterceptionDriver.Wait(context), ref stroke, 1) > 0)
            {
                if (InterceptionDriver.IsMouse(deviceId) > 0)
                {
                    if (OnMousePressed != null)
                    {
                        var args = new MousePressedEventArgs() { X = stroke.Mouse.X, Y = stroke.Mouse.Y, State = stroke.Mouse.State, Rolling = stroke.Mouse.Rolling };
                        OnMousePressed(this, args);

                        if (args.Handled)
                        {
                            continue;
                        }
                        stroke.Mouse.X = args.X;
                        stroke.Mouse.Y = args.Y;
                        stroke.Mouse.State = args.State;
                        stroke.Mouse.Rolling = args.Rolling;
                    }
                }

                if (InterceptionDriver.IsKeyboard(deviceId) > 0)
                {
                    if (OnKeyPressed != null)
                    {
                        var args = new KeyPressedEventArgs() { Key = stroke.Key.Code, State = stroke.Key.State};
                        OnKeyPressed(this, args);

                        if (args.Handled)
                        {
                            continue;
                        }
                        stroke.Key.Code = args.Key;
                        stroke.Key.State = args.State;
                    }
                }

                InterceptionDriver.Send(context, deviceId, ref stroke, 1);
            }

            Unload();
            throw new Exception("Interception.Receive() failed for an unknown reason. The driver has been unloaded.");
        }

        public void SendKey(Keys key, KeyState state)
        {
            Stroke stroke = new Stroke();
            KeyStroke keyStroke = new KeyStroke();

            keyStroke.Code = key;
            keyStroke.State = state;

            stroke.Key = keyStroke;

            InterceptionDriver.Send(context, deviceId, ref stroke, 1);

            if (KeyPressDelay > 0)
                Thread.Sleep(KeyPressDelay);
        }

        /// <summary>
        /// Warning: Do not use this overload of SendKey() for non-letter, non-number, or non-ENTER keys. It may require a special KeyState of not KeyState.Down or KeyState.Up, but instead KeyState.E0 and KeyState.E1.
        /// </summary>
        public void SendKey(Keys key)
        {
            SendKey(key, KeyState.Down);

            if (KeyPressDelay > 0)
                Thread.Sleep(KeyPressDelay);

            SendKey(key, KeyState.Up);
        }

        public void SendKeys(params Keys[] keys)
        {
            foreach (Keys key in keys)
            {
                SendKey(key);
            }
        }

        /// <summary>
        /// Warning: Only use this overload for sending letters, numbers, and symbols (those to the right of the letters on a U.S. keyboard and those obtained by pressing shift-#). Do not send special keys like Tab or Control or Enter.
        /// </summary>
        /// <param name="text"></param>
        public void SendText(string text)
        {
            foreach (char letter in text)
            {
                var tuple = CharacterToKeysEnum(letter);

                if (tuple.Item2 == true) // We need to press shift to get the next character
                    SendKey(Keys.LeftShift, KeyState.Down);

                SendKey(tuple.Item1);

                if (tuple.Item2 == true)
                    SendKey(Keys.LeftShift, KeyState.Up);
            }
        }

        /// <summary>
        /// Converts a character to a Keys enum and a 'do we need to press shift'.
        /// </summary>
        private Tuple<Keys, bool> CharacterToKeysEnum(char c)
        {
            switch (Char.ToLower(c))
            {
                case 'a':
                    return new Tuple<Keys,bool>(Keys.A, false);
                case 'b':
                    return new Tuple<Keys,bool>(Keys.B, false);
                case 'c':
                    return new Tuple<Keys,bool>(Keys.C, false);
                case 'd':
                    return new Tuple<Keys,bool>(Keys.D, false);
                case 'e':
                    return new Tuple<Keys,bool>(Keys.E, false);
                case 'f':
                    return new Tuple<Keys,bool>(Keys.F, false);
                case 'g':
                    return new Tuple<Keys,bool>(Keys.G, false);
                case 'h':
                    return new Tuple<Keys,bool>(Keys.H, false);
                case 'i':
                    return new Tuple<Keys,bool>(Keys.I, false);
                case 'j':
                    return new Tuple<Keys,bool>(Keys.J, false);
                case 'k':
                    return new Tuple<Keys,bool>(Keys.K, false);
                case 'l':
                    return new Tuple<Keys,bool>(Keys.L, false);
                case 'm':
                    return new Tuple<Keys,bool>(Keys.M, false);
                case 'n':
                    return new Tuple<Keys,bool>(Keys.N, false);
                case 'o':
                    return new Tuple<Keys,bool>(Keys.O, false);
                case 'p':
                    return new Tuple<Keys,bool>(Keys.P, false);
                case 'q':
                    return new Tuple<Keys,bool>(Keys.Q, false);
                case 'r':
                    return new Tuple<Keys,bool>(Keys.R, false);
                case 's':
                    return new Tuple<Keys,bool>(Keys.S, false);
                case 't':
                    return new Tuple<Keys,bool>(Keys.T, false);
                case 'u':
                    return new Tuple<Keys,bool>(Keys.U, false);
                case 'v':
                    return new Tuple<Keys,bool>(Keys.V, false);
                case 'w':
                    return new Tuple<Keys,bool>(Keys.W, false);
                case 'x':
                    return new Tuple<Keys,bool>(Keys.X, false);
                case 'y':
                    return new Tuple<Keys,bool>(Keys.Y, false);
                case 'z':
                    return new Tuple<Keys,bool>(Keys.Z, false);
                case '1':
                    return new Tuple<Keys,bool>(Keys.One, false);
                case '2':
                    return new Tuple<Keys,bool>(Keys.Two, false);
                case '3':
                    return new Tuple<Keys,bool>(Keys.Three, false);
                case '4':
                    return new Tuple<Keys,bool>(Keys.Four, false);
                case '5':
                    return new Tuple<Keys,bool>(Keys.Five, false);
                case '6':
                    return new Tuple<Keys,bool>(Keys.Six, false);
                case '7':
                    return new Tuple<Keys,bool>(Keys.Seven, false);
                case '8':
                    return new Tuple<Keys,bool>(Keys.Eight, false);
                case '9':
                    return new Tuple<Keys,bool>(Keys.Nine, false);
                case '0':
                    return new Tuple<Keys,bool>(Keys.Zero, false);
                case '-':
                    return new Tuple<Keys,bool>(Keys.DashUnderscore, false);
                case '+':
                    return new Tuple<Keys,bool>(Keys.PlusEquals, false);
                case '[':
                    return new Tuple<Keys,bool>(Keys.OpenBracketBrace, false);
                case ']':
                    return new Tuple<Keys,bool>(Keys.CloseBracketBrace, false);
                case ';':
                    return new Tuple<Keys,bool>(Keys.SemicolonColon, false);
                case '\'':
                    return new Tuple<Keys,bool>(Keys.SingleDoubleQuote, false);
                case ',':
                    return new Tuple<Keys,bool>(Keys.CommaLeftArrow, false);
                case '.':
                    return new Tuple<Keys,bool>(Keys.PeriodRightArrow, false);
                case '/':
                    return new Tuple<Keys,bool>(Keys.ForwardSlashQuestionMark, false);
                case '{':
                    return new Tuple<Keys,bool>(Keys.OpenBracketBrace, true);
                case '}':
                    return new Tuple<Keys,bool>(Keys.CloseBracketBrace, true);
                case ':':
                    return new Tuple<Keys,bool>(Keys.SemicolonColon, true);
                case '\"':
                    return new Tuple<Keys,bool>(Keys.SingleDoubleQuote, true);
                case '<':
                    return new Tuple<Keys,bool>(Keys.CommaLeftArrow, true);
                case '>':
                    return new Tuple<Keys,bool>(Keys.PeriodRightArrow, true);
                case '?':
                    return new Tuple<Keys,bool>(Keys.ForwardSlashQuestionMark, true);
                case '\\':
                    return new Tuple<Keys,bool>(Keys.BackslashPipe, false);
                case '|':
                    return new Tuple<Keys,bool>(Keys.BackslashPipe, true);
                case '`':
                    return new Tuple<Keys,bool>(Keys.Tilde, false);
                case '~':
                    return new Tuple<Keys,bool>(Keys.Tilde, true);
                case '!':
                    return new Tuple<Keys,bool>(Keys.One, true);
                case '@':
                    return new Tuple<Keys,bool>(Keys.Two, true);
                case '#':
                    return new Tuple<Keys,bool>(Keys.Three, true);
                case '$':
                    return new Tuple<Keys,bool>(Keys.Four, true);
                case '%':
                    return new Tuple<Keys,bool>(Keys.Five, true);
                case '^':
                    return new Tuple<Keys,bool>(Keys.Six, true);
                case '&':
                    return new Tuple<Keys,bool>(Keys.Seven, true);
                case '*':
                    return new Tuple<Keys,bool>(Keys.Eight, true);
                case '(':
                    return new Tuple<Keys,bool>(Keys.Nine, true);
                case ')':
                    return new Tuple<Keys, bool>(Keys.Zero, true);
                case ' ':
                    return new Tuple<Keys, bool>(Keys.Space, true);
                default:
                    return new Tuple<Keys, bool>(Keys.ForwardSlashQuestionMark, true);
            }
        }

        public void SendMouseEvent(MouseState state)
        {
            Stroke stroke = new Stroke();
            MouseStroke mouseStroke = new MouseStroke();

            mouseStroke.State = state;

            if (state == MouseState.ScrollUp)
            {
                mouseStroke.Rolling = 120;
            }
            else if (state == MouseState.ScrollDown)
            {
                mouseStroke.Rolling = -120;
            }

            stroke.Mouse = mouseStroke;

            InterceptionDriver.Send(context, 12, ref stroke, 1);
        }

        public void SendLeftClick()
        {
            SendMouseEvent(MouseState.LeftDown);
            Thread.Sleep(ClickDelay);
            SendMouseEvent(MouseState.LeftUp);
        }

        public void SendRightClick()
        {
            SendMouseEvent(MouseState.RightDown);
            Thread.Sleep(ClickDelay);
            SendMouseEvent(MouseState.RightUp);
        }

        public void ScrollMouse(ScrollDirection direction)
        {
            switch (direction)
            { 
                case ScrollDirection.Down:
                    SendMouseEvent(MouseState.ScrollDown);
                    break;
                case ScrollDirection.Up:
                    SendMouseEvent(MouseState.ScrollUp);
                    break;
            }
        }

        /// <summary>
        /// Warning: This function, if using the driver, does not function reliably and often moves the mouse in unpredictable vectors. An alternate version uses the standard Win32 API to get the current cursor's position, calculates the desired destination's offset, and uses the Win32 API to set the cursor to the new position.
        /// </summary>
        public void MoveMouseBy(int deltaX, int deltaY, bool useDriver = false)
        {
            if (useDriver)
            {
                Stroke stroke = new Stroke();
                MouseStroke mouseStroke = new MouseStroke();

                mouseStroke.X = deltaX;
                mouseStroke.Y = deltaY;

                stroke.Mouse = mouseStroke;
                stroke.Mouse.Flags = MouseFlags.MoveRelative;

                InterceptionDriver.Send(context, 12, ref stroke, 1);
            }
            else
            {
                var currentPos = Cursor.Position;
                Cursor.Position = new Point(currentPos.X + deltaX, currentPos.Y - deltaY); // Coordinate system for y: 0 begins at top, and bottom of screen has the largest number
            }
        }

        /// <summary>
        /// Warning: This function, if using the driver, does not function reliably and often moves the mouse in unpredictable vectors. An alternate version uses the standard Win32 API to set the cursor's position and does not use the driver.
        /// </summary>
        public void MoveMouseTo(int x, int y, bool useDriver = false)
        {
            if (useDriver)
            {
                Stroke stroke = new Stroke();
                MouseStroke mouseStroke = new MouseStroke();

                mouseStroke.X = x;
                mouseStroke.Y = y;

                stroke.Mouse = mouseStroke;
                stroke.Mouse.Flags = MouseFlags.MoveAbsolute;

                InterceptionDriver.Send(context, 12, ref stroke, 1);
            }
            {
                Cursor.Position = new Point(x, y);
            }
        }
    }
}
 