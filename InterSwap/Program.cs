using System;
using System.Threading;
using Interceptor;

namespace InterSwap
{
    class Program
    {
        private static void FailSafe(object token)
        {
            var canToken = (CancellationToken)token;
            var watch = new System.Diagnostics.Stopwatch();
            watch.Start();
            while (watch.ElapsedMilliseconds < 10000 && !canToken.IsCancellationRequested) { }
            if (canToken.IsCancellationRequested)
                return;
            Console.WriteLine("Forced Ended.");
            Environment.Exit(0);
        }
        static void Main(string[] args)
        {
            // Setting up fail safe thread.
            var source = new CancellationTokenSource();
            var failSafety = new Thread(FailSafe);
            // Starting the fail safe thread.
            // If for some reason after 10 seconds we are still running
            // this thread will kill the program.
            // We should never reach this with this program but you may need
            // something like this while testing your own programs and you don't
            // cause a system soft-lock.
            failSafety.Start(source.Token);
            // Tell the driver to capture all input.
            var input = new Input
            {
                MouseFilterMode = MouseFilterMode.All,
                KeyboardFilterMode = KeyboardFilterMode.All
            };
            // Start the driver.
            input.Load();
            // Creating and starting our timer.
            var watch = new System.Diagnostics.Stopwatch();
            watch.Start();
            // Creating Escape Event.
            void EscapeHandler(object sender, KeyPressedEventArgs eventArgs)
            {
                if (eventArgs.Key != Keys.Escape) return;
                input.Unload();
                Console.WriteLine("Forced Stop!");
            }
            // Creating Reverse Events.
            void Handler(object sender, MousePressedEventArgs eventArgs)
            {
                Console.WriteLine(eventArgs.State);
                switch (eventArgs.State)
                {
                    case MouseState.LeftDown:
                        eventArgs.State = MouseState.RightDown;
                        break;
                    case MouseState.LeftUp:
                        eventArgs.State = MouseState.RightUp;
                        break;
                    case MouseState.RightDown:
                        eventArgs.State = MouseState.LeftDown;
                        break;
                    case MouseState.RightUp:
                        eventArgs.State = MouseState.LeftUp;
                        break;
                }
            }
            // Subscribing our events.
            input.OnKeyPressed += EscapeHandler;
            input.OnMousePressed += Handler;
            // Stall the main thread until the driver is unloaded or 5 seconds have passed.
            while (input.IsLoaded && watch.ElapsedMilliseconds <= 5000) { }
            // Unsubscribe our events.
            // You may not need this for every program.
            input.OnMousePressed -= Handler;
            input.OnKeyPressed -= EscapeHandler;
            // Unload the driver.
            input.Unload();
            // Canceling our fail safe thread.
            // The main thread will wait for all threads to end.
            // So we need to end all threads.
            source.Cancel();
            Console.WriteLine("Done!");
        }

    }
}
