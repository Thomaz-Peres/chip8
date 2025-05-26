using Chip8;
using Sdl = SDL3.SDL;

bool done = false;
unsafe
{
    Sdl.SDL_Init(Sdl.SDL_InitFlags.SDL_INIT_VIDEO);

    var window = Sdl.SDL_CreateWindow("CHIP-8 interpreter", 64 * 10, 32 * 10, Sdl.SDL_WindowFlags.SDL_WINDOW_RESIZABLE);

    var renderer = Sdl.SDL_CreateRenderer(window, null);
    Sdl.SDL_Texture texture = *Sdl.SDL_CreateTexture(renderer, Sdl.SDL_PixelFormat.SDL_PIXELFORMAT_RGBA8888, Sdl.SDL_TextureAccess.SDL_TEXTUREACCESS_TARGET, 64, 32);

    if (window == IntPtr.Zero)
    {
        // Sdl.SDL_LogCategory.SDL_LOG_CATEGORY_ERROR = 1;
        Sdl.SDL_LogError(1, $"Could not create window: \n {Sdl.SDL_GetError()}");
        Console.WriteLine("Sdl could not create a window.");
        return 1;
    }

    float scale = 8;
    float startY = 1;

    while (true)
    {
        Sdl.SDL_Event @event;
        Sdl.SDL_PollEvent(out @event);

        if (@event.type == (uint)Sdl.SDL_EventType.SDL_EVENT_QUIT)
            break;

        Sdl.SDL_SetRenderDrawColor(renderer, 0, 0, 0, 0);
        Sdl.SDL_RenderClear(renderer);
        Sdl.SDL_SetRenderDrawColor(renderer, 255, 0, 0, 255);


        // Sdl.SDL_RenderPoint(renderer, 0x3F, 0x1F);
        byte[] zero = Fonts.Font[0];
        byte[] one = Fonts.Font[1];

        foreach (var key in Fonts.Font)
        {
            float startX = key.Key * (5 + 1); // character width + spacing
            var item = key.Value;

            for (int row = 0; row < item.Length; row++)
            {
                byte rowData = item[row];
                for (int col = 0; col < 8; col++)
                {
                    if ((rowData & (0b1000_0000 >> col)) != 0)
                    {
                        float pixelX = (startX + col) * scale;
                        float pixelY = (startY + row) * scale;

                        Sdl.SDL_FRect pixelRect = new Sdl.SDL_FRect
                        {
                            x = pixelX,
                            y = pixelY,
                            w = scale,
                            h = scale,
                        };

                        Sdl.SDL_RenderFillRect(renderer, ref pixelRect);
                    }
                }
            }
        }

        // for (int i = 0; i < 640; i++)
        //     Sdl.SDL_RenderPoint(renderer, i, i);

        // int x = 640;
        // for (int i = 0; i < 640; i++)
        // {
        //     Sdl.SDL_RenderPoint(renderer, i, x);
        //     x--;
        // }

        Sdl.SDL_RenderPresent(renderer);

        // if (@event.key.key == 0x00)
        // {
        //     Sdl
        // }
    }

    Sdl.SDL_DestroyRenderer(renderer);
    Sdl.SDL_DestroyWindow(window);

    Sdl.SDL_Quit();

    return 0;
}
