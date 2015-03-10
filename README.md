Interceptor
===========

Note: Windows 8/8.1 is not supported.

Interceptor is a wrapper for a Windows keyboard driver (Wrapping http://oblita.com/Interception). 

Using the driver, Interceptor can simulate keystrokes and mouse clicks in...

  * Games that use DirectX, which don't normally accept keystrokes using SendInput()
  * Protected areas of Windows like the Windows Logon Screen or UAC-dimmed screens
  * And any app
  
Because the driver simulates keystrokes and mouse clicks, the target window must be active (i.e. you can't multitask on another window while sending keystrokes and mouse clicks).

How to use
===========

1. Download and build this project and reference its DLL in your project.

2. Download 'interception.dll', a separate library written by the driver author. Put it in the same directory as your executable. This is required.

3. Download and install 'install-interception.exe' from the author's webpage. Restart your computer after installation.

4. In your code, to load your driver, call (read the code comments below; you must set a filter mode to capture key press events or send key presses!):

```
Input input = new Input();

// Be sure to set your keyboard filter to be able to capture key presses and simulate key presses
// KeyboardFilterMode.All captures all events; 'Down' only captures presses for non-special keys; 'Up' only captures releases for non-special keys; 'E0' and 'E1' capture presses/releases for special keys
input.KeyboardFilterMode = KeyboardFilterMode.All;
// You can set a MouseFilterMode as well, but you don't need to set a MouseFilterMode to simulate mouse clicks

// Finally, load the driver
input.Load();
```

5. Do your stuff.

```
input.MoveMouseTo(5, 5);  // Please note this doesn't use the driver to move the mouse; it uses System.Windows.Forms.Cursor.Position
input.MoveMouseBy(25, 25); //  Same as above ^
input.SendLeftClick();

input.KeyDelay = 1; // See below for explanation; not necessary in non-game apps
input.SendKeys(Keys.Enter);  // Presses the ENTER key down and then up (this constitutes a key press)

// Or you can do the same thing above using these two lines of code
input.SendKeys(Keys.Enter, KeyState.Down);
Thread.Sleep(1); // For use in games, be sure to sleep the thread so the game can capture all events. A lagging game cannot process input quickly, and you so you may have to adjust this to as much as 40 millisecond delay. Outside of a game, a delay of even 0 milliseconds can work (instant key presses).
input.SendKeys(Keys.Enter, KeyState.Up);

input.SendText("hello, I am typing!");

/* All these following characters / numbers / symbols work */
input.SendText("abcdefghijklmnopqrstuvwxyz");
input.SendText("1234567890");
input.SendText("!@#$%^&*()");
input.SendText("[]\\;',./");
input.SendText("{}|:\"<>?");


// And finally
input.Unload();
```

Notes:

1. You may get a ```BadImageFormatException``` if you don't use the proper architecture (x86 or x64) for all your projects in the solution, including this project. So you may have to download the source of this project to rebuild it to the right architecture. This should be easy and the build process should have no errors.

2. You MUST download the 'interception.dll' available from http://oblita.com/Interception.

3. If you've done all the above (installed the Interception driver correctly, put interception.dll in your project folder) and you're still not able to send keystrokes:
 
The driver has a limitation in that it can't send keystrokes without receiving at least one keystroke. This is because the driver doesn't know which device id the keyboard is, so it has to wait to receive a keystroke to deduce the device id from your keystroke.

In summary, before sending a keystroke, always physically press the keyboard once. Tap any key. Then you can send keystrokes. This doesn't apply to receiving keystrokes, because by receiving a keystroke, you have of course already pressed a key.

4. MoveMouseTo() and MoveMouseBy() completely ignore the keyboard driver. It uses System.Windows.Forms.Position to set and get the cursor's position (which calls the standard Win32 API underneath for those respective functions).

The reason for this is, while exploring the keyboard driver's mouse moving capabilities, I noticed it didn't move the cursor by pixel units, but rather it seemed to move the cursor by acceleration. This would continually produce inconsistent values when I wanted to move the cursor to a certain location. Because the Win32 cursor setting API isn't usually blocked by games and the like, I find simply calling these standard APIs to be sufficient without resorting to the driver. Please note that this only applies for setting cursor position. Intercepting the cursor still works okay. You can, for example, invert the x and y axes of the mouse using Interceptor.
