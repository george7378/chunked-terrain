using ChunkedTerrainCore;
using System;

namespace ChunkedTerrainWindows
{
    /// <summary>
    /// The main class.
    /// </summary>
    public static class ProgramMain
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            using (var game = new ChunkedTerrainGame())
            {
                game.Run();
            }
        }
    }
}
