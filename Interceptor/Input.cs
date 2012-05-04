using System;
using System.Collections.Generic;
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
        private KeyboardFilterMode _keyboardFilterMode;
        private MouseFilterMode _mouseFilterMode;

        public bool IsLoaded { get; set; }

        public event EventHandler<KeyPressedEventArgs> OnKeyPressed;
        public event EventHandler<MousePressedEventArgs> OnMousePressed;

        /// <summary>
        /// Determines the keyboard filter mode. Set this before loading otherwise it will not filter any events.
        /// </summary>
        public KeyboardFilterMode KeyboardFilterMode
        {
            get
            {
                return _keyboardFilterMode;
            }
            set
            {
                _keyboardFilterMode = value;

                if (IsLoaded)
                {
                    Unload();
                    Load();
                }
            }
        }

        /// <summary>
        /// Determines the mouse filter mode. Set this before loading otherwise it will not filter any events.
        /// </summary>
        public MouseFilterMode MouseFilterMode 
        {
            get
            {
                return _mouseFilterMode; 
            }
            set
            {
                _mouseFilterMode = value;

                if (IsLoaded)
                {
                    Unload();
                    Load();
                }
            }
        }

        public Input()
        {
            context = IntPtr.Zero;

            KeyboardFilterMode = KeyboardFilterMode.None;
            MouseFilterMode = MouseFilterMode.None;
        }

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

        public bool DriverExists()
        {
            if (Load())
            {
                Unload();
                return true;
            }
            else
            {
                return false;
            }
        }

        private void DriverCallback()
        {
            InterceptionDriver.SetFilter(context, InterceptionDriver.IsKeyboard, (Int32) KeyboardFilterMode);
            InterceptionDriver.SetFilter(context, InterceptionDriver.IsMouse, (Int32) MouseFilterMode);

            Stroke stroke = new Stroke();
            Int32 device = 0;

            while (InterceptionDriver.Receive(context, device = InterceptionDriver.Wait(context), ref stroke, 1) > 0)
            {
                if (InterceptionDriver.IsMouse(device) > 0)
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

                if (InterceptionDriver.IsKeyboard(device) > 0)
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

                InterceptionDriver.Send(context, device, ref stroke, 1);
            }

            InterceptionDriver.DestroyContext(context);
            throw new Exception("Interception.Receive() failed for an unknown reason. The device context has been disposed.");
        }

        public void SendKey(Keys key, KeyState state)
        {
            Stroke stroke = new Stroke();
            KeyStroke keyStroke = new KeyStroke();

            keyStroke.Code = key;
            keyStroke.State = state;

            stroke.Key = keyStroke;

            InterceptionDriver.Send(context, 1, ref stroke, 1);
        }

        public void SendKey(Keys key)
        {
            Stroke stroke = new Stroke();
            KeyStroke keyStroke = new KeyStroke();

            keyStroke.Code = key;
            keyStroke.State = KeyState.Down;

            stroke.Key = keyStroke;

            InterceptionDriver.Send(context, 1, ref stroke, 1);

            stroke.Key.State = KeyState.Up;
            InterceptionDriver.Send(context, 1, ref stroke, 1);
        }

        public void SendKeys(params Keys[] keys)
        {
            Stroke stroke = new Stroke();
            KeyStroke keyStroke = new KeyStroke();

            foreach (Keys key in keys)
            {
                keyStroke.Code = key;
                keyStroke.State = 0;

                stroke.Key = keyStroke;

                InterceptionDriver.Send(context, 1, ref stroke, 1);
            }

            foreach (Keys key in keys)
            {
                keyStroke.Code = key;
                keyStroke.State = KeyState.Up;

                stroke.Key = keyStroke;

                InterceptionDriver.Send(context, 1, ref stroke, 1);
            }

        }

        public void SendClick(MouseState state)
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
            SendLeftClick(0);
        }

        public void SendLeftClick(int millisecondsDelay)
        {
            SendClick(MouseState.LeftDown);
            Thread.Sleep(millisecondsDelay);
            SendClick(MouseState.LeftUp);
        }

        public void SendRightClick()
        {
            SendRightClick(0);
        }

        public void SendRightClick(int millisecondsDelay)
        {
            SendClick(MouseState.RightDown);
            Thread.Sleep(millisecondsDelay);
            SendClick(MouseState.RightUp);
        }

        public void MoveMouseBy(int deltaX, int deltaY)
        {
            Stroke stroke = new Stroke();
            MouseStroke mouseStroke = new MouseStroke();

            mouseStroke.X = deltaX;
            mouseStroke.Y = deltaY;

            stroke.Mouse = mouseStroke;
            stroke.Mouse.Flags = MouseFlags.MoveRelative;

            InterceptionDriver.Send(context, 12, ref stroke, 1);
        }

        public void MoveMouseTo(int x, int y)
        {
            MoveMouseBy(Int32.MinValue, Int32.MinValue);
            MoveMouseBy(x, y);
        }
    }
}
 