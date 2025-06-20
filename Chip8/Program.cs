using Chip8;
using SDL3;

public sealed class program
{
    public static void Main(string[] args)
    {
        string filename = args[0];

        var sdlManager = new SdlManager();

        sdlManager.Init();

        var reader = File.ReadAllBytes(filename);
        CPU cpu = new CPU();

        cpu.LoadROM(reader);

        int targetFrameTime = 1000 / 60; // ~16ms per frame
        bool quit = false;

        while (!quit)
        {
            var frameStart = SDL.SDL_GetTicks();
            quit = sdlManager.ProcessInput(cpu.Keypad);

            cpu.EmulateCycle();

            sdlManager.Update(cpu.Video);

            var frameTime = (int)(SDL.SDL_GetTicks() - frameStart);

            if (frameTime < targetFrameTime)
            {
                SDL.SDL_Delay((uint)(targetFrameTime - frameTime));
            }

            Thread.Sleep(1);
        }

        sdlManager.Quit();
    }
}
