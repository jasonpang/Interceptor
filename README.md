Interceptor
===========

Interceptor is a wrapper for a Windows keyboard driver (Wrapping http://oblita.com/Interception). 

Using the driver, Interceptor can simulate keystrokes and mouse clicks in...

  * Games that use DirectX, which don't normally accept keystrokes using SendInput()
  * Protected areas of Windows like the Windows Logon Screen or UAC-dimmed screens
  * And any app
  
Because the driver simulates keystrokes and mouse clicks, the target window must be active (i.e. you can't multitask on another window while sending keystrokes and mouse clicks).

How to use
===========

1. Download and build this project and reference its DLL in your project.

2. Download and build this separate library written by the driver author. Put it in the same directory as your executable.

3. To load your driver, call (read the code comments below; you must set a filter mode to capture key press events or send key presses!):

```
Input input = new Input();

// Be sure to set your keyboard filter to be able to capture key presses and simulate key presses
// KeyboardFilterMode.All captures all events; 'Down' only captures presses for non-special keys; 'Up' only captures releases for non-special keys; 'E0' and 'E1' capture presses/releases for special keys
input.KeyboardFilterMode = KeyboardFilterMode.All;
// You can set a MouseFilterMode as well, but you don't need to set a MouseFilterMode to simulate mouse clicks

// Finally, load the driver
input.Load();
```

4. Do your stuff.

```
input.MoveMouseTo(5, 5);
input.MoveeMouseBy(25, 25);
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

Note:

1. You may get a ```BadImageFormatException``` if you don't use the proper architecture (x86 or x64) for all your projects in the solution, including this project. So you may have to download the source of this project to rebuild it to the right architecture. This should be easy and the build process should have no errors.

2. You MUST download the 'interception.dll' available from http://oblita.com/Interception.
