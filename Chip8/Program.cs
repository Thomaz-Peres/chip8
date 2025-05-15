using Sdl = SDL3.SDL;

bool done = false;

Sdl.SDL_Init(Sdl.SDL_InitFlags.SDL_INIT_VIDEO);

var window = Sdl.SDL_CreateWindow("CHIP-8 interpreter", 640, 480, Sdl.SDL_WindowFlags.SDL_WINDOW_RESIZABLE);

if (window == IntPtr.Zero)
{
    // Sdl.SDL_LogCategory.SDL_LOG_CATEGORY_ERROR = 1;
    Sdl.SDL_LogError(1, $"Could not create window: \n {Sdl.SDL_GetError()}");
    Console.WriteLine("Sdl could not create a window.");
    return 1;
}

while (!done)
{
    Sdl.SDL_Event @event;

    while (Sdl.SDL_PollEvent(out @event))
    {
        if (@event.type == (uint)Sdl.SDL_EventType.SDL_EVENT_QUIT)
            done = true;
    }
    var b = Sdl.SDL_CreateRenderer(window, x);

    // Sdl.
}

Sdl.SDL_DestroyWindow(window);

Sdl.SDL_Quit();

return 0;
