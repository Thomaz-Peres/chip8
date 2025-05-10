using SDL3;

SDL.SDL_Init(SDL.SDL_InitFlags.SDL_INIT_VIDEO);

var window = SDL.SDL_CreateWindow("CHIP-8 interpreter", 64 * 8, 32 * 8, SDL.
SDL_WindowFlags.SDL_WINDOW_ALWAYS_ON_TOP);

if (window == IntPtr.Zero)
{
    Console.WriteLine("SDL could not create a window.");
    return;
}

var b = SDL.SDL_CreateRenderer(window, "teste");

Console.WriteLine(window);

Console.ReadLine();
