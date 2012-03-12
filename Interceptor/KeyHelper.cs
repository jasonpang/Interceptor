using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace Interceptor
{
    public enum KeyState
    {
        Pressed = 0,
        Released = 1
    }

    public static class KeyHelper
    {
        /// <summary>
        /// The MapVirtualKey function translates (maps) a virtual-key code into a scan
        /// code or character value, or translates a scan code into a virtual-key code    
        /// </summary>
        /// <param name="uCode">[in] Specifies the virtual-key code or scan code for a key.
        /// How this value is interpreted depends on the value of the uMapType parameter
        /// </param>
        /// <param name="uMapType">[in] Specifies the translation to perform. The value of this
        /// parameter depends on the value of the uCode parameter.
        /// </param>
        /// <returns>Either a scan code, a virtual-key code, or a character value, depending on
        /// the value of uCode and uMapType. If there is no translation, the return value is zero
        /// </returns>
        [DllImport("user32.dll")]
        public static extern UInt32 MapVirtualKey(UInt32 uCode, UInt32 uMapType);

        public const UInt32 VirtualKeyToScanCode = 0x00;
        public const UInt32 ScanCodeToVirtualKey = 0x01;
        public const UInt32 VirtualKeyToChar = 0x02;
        public const UInt32 ScanCodeToEx = 0x03;
        public const UInt32 VirtualKeyToScanCodeEx = 0x04;
    }
}
