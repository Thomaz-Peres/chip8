using Sdl = SDL3.SDL;

bool done = false;

unsafe
{
    Sdl.SDL_Init(Sdl.SDL_InitFlags.SDL_INIT_VIDEO);

    var window = Sdl.SDL_CreateWindow("CHIP-8 interpreter", 640, 640, Sdl.SDL_WindowFlags.SDL_WINDOW_RESIZABLE);

    var renderer = Sdl.SDL_CreateRenderer(window, null);
    Sdl.SDL_Texture texture = *Sdl.SDL_CreateTexture(renderer, Sdl.SDL_PixelFormat.SDL_PIXELFORMAT_RGBA8888, Sdl.SDL_TextureAccess.SDL_TEXTUREACCESS_TARGET, 640, 640);

    if (window == IntPtr.Zero)
    {
        // Sdl.SDL_LogCategory.SDL_LOG_CATEGORY_ERROR = 1;
        Sdl.SDL_LogError(1, $"Could not create window: \n {Sdl.SDL_GetError()}");
        Console.WriteLine("Sdl could not create a window.");
        return 1;
    }

    while (true)
    {
        Sdl.SDL_Event @event;
        Sdl.SDL_PollEvent(out @event);

        if (@event.type == (uint)Sdl.SDL_EventType.SDL_EVENT_QUIT)
            break;


        byte[] zero = { 0xF0, 0x90, 0x90, 0x90, 0xF0 };
        Sdl.SDL_SetRenderDrawColor(renderer, 0, 0, 0, 0);
        Sdl.SDL_RenderClear(renderer);
        Sdl.SDL_SetRenderDrawColor(renderer, 255, 0, 0, 255);

        for (int i = 0; i < 640; i++)
            Sdl.SDL_RenderPoint(renderer, i, i);

        int x = 640;
        for (int i = 0; i < 640; i++)
        {
            Sdl.SDL_RenderPoint(renderer, i, x);
            x--;
        }

        Sdl.SDL_RenderPresent(renderer);
    }

    Sdl.SDL_DestroyRenderer(renderer);
    Sdl.SDL_DestroyWindow(window);

    Sdl.SDL_Quit();

    return 0;
}
