using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Stroke = Interceptor.InterceptionDriver.Stroke;
using KeyStroke = Interceptor.InterceptionDriver.KeyStroke;
using MouseStroke = Interceptor.InterceptionDriver.MouseStroke;

namespace Interceptor
{
    public class KeyPressedEventArgs : EventArgs
    {
        public Stroke Stroke { get; set; }
    }
}
