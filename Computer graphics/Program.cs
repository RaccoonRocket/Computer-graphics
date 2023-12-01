using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Computer_graphics
{
    public static class Program
    {
        private static void Main()
        {
            /*using (Game game = new Game(800, 600, "Computer graphics"))
            {
                game.Run();
            }*/
            var nativeWindowSettings = new NativeWindowSettings()
            {
                Size = new Vector2i(800, 600),
                Title = "Computer graphics",
                // This is needed to run on macos
                Flags = ContextFlags.ForwardCompatible,
            };

            using (Game game = new Game(GameWindowSettings.Default, nativeWindowSettings))
            {
                game.Run();
            }
        }
    }
}