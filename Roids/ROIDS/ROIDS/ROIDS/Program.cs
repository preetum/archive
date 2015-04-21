using System;

namespace ROIDS
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            using (XnaGame game = new XnaGame())
            {
                game.Run();
            }
        }
    }
#endif
}

