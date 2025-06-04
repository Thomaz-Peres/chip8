using SDL3;

namespace Chip8;

public class SdlManager
{
    private static IntPtr window;
    private static IntPtr renderer;
    private static CPU Cpu;
    private static uint[,] Video = new uint[64, 32];

    public void Init() => Task.Run(() =>
    {
        if (SDL.SDL_Init(SDL.SDL_InitFlags.SDL_INIT_VIDEO))
            return;

        if (SDL.SDL_CreateWindowAndRenderer("CHIP-8 Interpreter", 64 * 8, 32 * 8, SDL.SDL_WindowFlags.SDL_WINDOW_RESIZABLE, out window, out renderer))
            return;

        if (window == IntPtr.Zero)
        {
            // Sdl.SDL_LogCategory.SDL_LOG_CATEGORY_ERROR = 1;
            SDL.SDL_LogError(1, $"Could not create window: \n {SDL.SDL_GetError()}");
            Console.WriteLine("Sdl could not create a window.");
            return;
        }

        SDL.SDL_SetRenderScale(renderer, 8, 8);

        bool running = true;

        SDL.SDL_Event @event;

        while (running)
        {
            // Cpu.LoadROM();
            while (SDL.SDL_PollEvent(out @event))
            {
                if (@event.type == (uint)SDL.SDL_EventType.SDL_EVENT_QUIT)
                    running = false;
            }

            SDL.SDL_SetRenderDrawColor(renderer, 0, 0, 0, 255);

            SDL.SDL_RenderClear(renderer);
        }

    });

}
