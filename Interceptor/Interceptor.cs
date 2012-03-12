using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Stroke = Interceptor.InterceptionDriver.Stroke;
using KeyStroke = Interceptor.InterceptionDriver.KeyStroke;
using MouseStroke = Interceptor.InterceptionDriver.MouseStroke;

namespace Interceptor
{
    public class Interceptor
    {
        private IntPtr context;
        private Thread callbackThread; 

        public Interceptor()
        {
            context = IntPtr.Zero;
        }

        public bool Load()
        {
            context = InterceptionDriver.CreateContext();

            if (context != IntPtr.Zero)
            {
                callbackThread = new Thread(new ThreadStart(DriverCallback));
                callbackThread.Priority = ThreadPriority.Highest;
                callbackThread.IsBackground = true;
                callbackThread.Start();

                return true;
            }
            else
            {
                return false;
            }
        }

        public void Unload()
        {
            if (context != IntPtr.Zero)
            {
                callbackThread.Abort();
                InterceptionDriver.DestroyContext(context);
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
            InterceptionDriver.SetFilter(context, InterceptionDriver.IsKeyboard, (Int32) InterceptionDriver.KeyboardFilterMode.All);
            InterceptionDriver.SetFilter(context, InterceptionDriver.IsMouse, (Int32) InterceptionDriver.MouseFilterMode.All);

            Stroke stroke = new Stroke();
            Int32 device = 0;

            while (InterceptionDriver.Receive(context, device = InterceptionDriver.Wait(context), ref stroke, 1) > 0)
            {
                if (InterceptionDriver.IsMouse(device) > 0)
                {

                    InterceptionDriver.Send(context, device, ref stroke, 1);
                }

                if (InterceptionDriver.IsKeyboard(device) > 0)
                {


                    InterceptionDriver.Send(context, device, ref stroke, 1);
                }
            }

            InterceptionDriver.DestroyContext(context);
            throw new Exception("InterceptionDriver.Receive() failed for an unknown reason. The device context has been disposed.");
        }

        public void SendKey(Keys key, KeyState state)
        {
            Stroke stroke = new Stroke();
            KeyStroke keyStroke = new KeyStroke();
            
            UInt32 scanCode = KeyHelper.MapVirtualKey((UInt32) key, KeyHelper.VirtualKeyToScanCode);

            keyStroke.Code = (UInt16) scanCode;
            keyStroke.State = (UInt16) state;

            stroke.Key = keyStroke;

            InterceptionDriver.Send(context, 1, ref stroke, 1);
        }

        public void SendKey(Keys key)
        {
            Stroke stroke = new Stroke();
            KeyStroke keyStroke = new KeyStroke();

            UInt32 scanCode = KeyHelper.MapVirtualKey((UInt32)key, KeyHelper.VirtualKeyToScanCode);

            keyStroke.Code = (UInt16)scanCode;
            keyStroke.State = 0;

            stroke.Key = keyStroke;

            InterceptionDriver.Send(context, 1, ref stroke, 1);

            stroke.Key.State = 1;
            InterceptionDriver.Send(context, 1, ref stroke, 1);
        }

        public void SendKeys(params Keys[] keys)
        {
            Stroke stroke = new Stroke();
            KeyStroke keyStroke = new KeyStroke();

            foreach (Keys key in keys)
            {
                UInt32 scanCode = KeyHelper.MapVirtualKey((UInt32) key, KeyHelper.VirtualKeyToScanCode);

                keyStroke.Code = (UInt16) scanCode;
                keyStroke.State = 0;

                stroke.Key = keyStroke;

                InterceptionDriver.Send(context, 1, ref stroke, 1);
            }

            foreach (Keys key in keys)
            {
                UInt32 scanCode = KeyHelper.MapVirtualKey((UInt32)key, KeyHelper.VirtualKeyToScanCode);

                keyStroke.Code = (UInt16)scanCode;
                keyStroke.State = 1;

                stroke.Key = keyStroke;

                InterceptionDriver.Send(context, 1, ref stroke, 1);
            }
        }

        public void SendClick(MouseState state)
        {
            Stroke stroke = new Stroke();
            MouseStroke mouseStroke = new MouseStroke();

            mouseStroke.State = (UInt16) state;

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
    }
}
