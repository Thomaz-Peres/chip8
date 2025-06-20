using SDL3;
using static SDL3.SDL;

namespace Chip8;

public sealed class SdlManager
{
    private static IntPtr Window;
    private static IntPtr Renderer;
    private static IntPtr Texture;

    public void Init()
    {
        SDL.SDL_Init(SDL.SDL_InitFlags.SDL_INIT_VIDEO);

        SDL.SDL_CreateWindowAndRenderer("CHIP-8 Interpreter", 64 * 10, 32 * 10, SDL.SDL_WindowFlags.SDL_WINDOW_RESIZABLE, out Window, out Renderer);

        InitTexture();

        if (Window == IntPtr.Zero)
        {
            // Sdl.SDL_LogCategory.SDL_LOG_CATEGORY_ERROR = 1;
            SDL.SDL_LogError(1, $"Could not create window: \n {SDL.SDL_GetError()}");
            Console.WriteLine("Sdl could not create a window.");
            return;
        }

        // SDL.SDL_SetRenderScale(Renderer, 8, 8);
    }

    private unsafe static void InitTexture() =>
        Texture = (nint)SDL.SDL_CreateTexture(Renderer, SDL.SDL_PixelFormat.SDL_PIXELFORMAT_RGBA8888, SDL.SDL_TextureAccess.SDL_TEXTUREACCESS_STREAMING, 64, 32);

    public void Quit()
    {
        SDL.SDL_DestroyTexture(Texture);
        SDL.SDL_DestroyRenderer(Renderer);
        SDL.SDL_DestroyWindow(Window);
        SDL.SDL_Quit();
    }

    public void Update(uint[] Video)
    {
        int scale = 10;
        SDL_SetRenderDrawColor(Renderer, 0, 0, 0, 255);
        SDL_RenderClear(Renderer);
        SDL_SetRenderDrawColor(Renderer, 255, 255, 255, 255);

        for (int y = 0; y < 32; y++)
        {
            for (int x = 0; x < 64; x++)
            {
                int index = y * 64 + x;
                if (Video[index] == 0xFFFFFFFF)
                {
                    SDL_FRect rect = new SDL_FRect
                    {
                        x = x * scale,
                        y = y * scale,
                        w = scale,
                        h = scale
                    };
                    SDL_RenderFillRect(Renderer, ref rect);
                }
            }
        }
        SDL_RenderPresent(Renderer);
    }

    public bool ProcessInput(bool[] Keypad)
    {
        bool running = false;

        while (SDL.SDL_PollEvent(out var @event))
        {
            switch (@event.type)
            {
                case (uint)SDL.SDL_EventType.SDL_EVENT_QUIT:
                    running = true;
                    break;
                case (uint)SDL.SDL_EventType.SDL_EVENT_KEY_DOWN:
                    switch (@event.key.key)
                    {
                        case (uint)SDL.SDL_Keycode.SDLK_ESCAPE:
                            running = true;
                            break;

                        case (uint)SDL.SDL_Keycode.SDLK_0:
                            Keypad[0] = true;
                            break;
                        case (uint)SDL.SDL_Keycode.SDLK_1:
                            Keypad[1] = true;
                            break;
                        case (uint)SDL.SDL_Keycode.SDLK_2:
                            Keypad[2] = true;
                            break;
                        case (uint)SDL.SDL_Keycode.SDLK_3:
                            Keypad[3] = true;
                            break;
                        case (uint)SDL.SDL_Keycode.SDLK_4:
                            Keypad[4] = true;
                            break;
                        case (uint)SDL.SDL_Keycode.SDLK_5:
                            Keypad[5] = true;
                            break;
                        case (uint)SDL.SDL_Keycode.SDLK_6:
                            Keypad[6] = true;
                            break;
                        case (uint)SDL.SDL_Keycode.SDLK_7:
                            Keypad[7] = true;
                            break;
                        case (uint)SDL.SDL_Keycode.SDLK_8:
                            Keypad[8] = true;
                            break;
                        case (uint)SDL.SDL_Keycode.SDLK_9:
                            Keypad[9] = true;
                            break;
                        case (uint)SDL.SDL_Keycode.SDLK_A:
                            Keypad[0xA] = true;
                            break;
                        case (uint)SDL.SDL_Keycode.SDLK_B:
                            Keypad[0xB] = true;
                            break;
                        case (uint)SDL.SDL_Keycode.SDLK_C:
                            Keypad[0xC] = true;
                            break;
                        case (uint)SDL.SDL_Keycode.SDLK_E:
                            Keypad[0xE] = true;
                            break;
                        case (uint)SDL.SDL_Keycode.SDLK_F:
                            Keypad[0xF] = true;
                            break;
                    }
                    break;
                case (uint)SDL.SDL_EventType.SDL_EVENT_KEY_UP:
                    switch (@event.key.key)
                    {
                        case (uint)SDL.SDL_Keycode.SDLK_ESCAPE:
                            running = false;
                            break;

                        case (uint)SDL.SDL_Keycode.SDLK_0:
                            Keypad[0] = false;
                            break;

                        case (uint)SDL.SDL_Keycode.SDLK_1:
                            Keypad[1] = false;
                            break;
                        case (uint)SDL.SDL_Keycode.SDLK_2:
                            Keypad[2] = false;
                            break;
                        case (uint)SDL.SDL_Keycode.SDLK_3:
                            Keypad[3] = false;
                            break;
                        case (uint)SDL.SDL_Keycode.SDLK_4:
                            Keypad[4] = false;
                            break;
                        case (uint)SDL.SDL_Keycode.SDLK_5:
                            Keypad[5] = false;
                            break;
                        case (uint)SDL.SDL_Keycode.SDLK_6:
                            Keypad[6] = false;
                            break;
                        case (uint)SDL.SDL_Keycode.SDLK_7:
                            Keypad[7] = false;
                            break;
                        case (uint)SDL.SDL_Keycode.SDLK_8:
                            Keypad[8] = false;
                            break;
                        case (uint)SDL.SDL_Keycode.SDLK_9:
                            Keypad[9] = false;
                            break;
                        case (uint)SDL.SDL_Keycode.SDLK_A:
                            Keypad[0xA] = false;
                            break;
                        case (uint)SDL.SDL_Keycode.SDLK_B:
                            Keypad[0xB] = false;
                            break;
                        case (uint)SDL.SDL_Keycode.SDLK_C:
                            Keypad[0xC] = false;
                            break;
                        case (uint)SDL.SDL_Keycode.SDLK_E:
                            Keypad[0xE] = false;
                            break;
                        case (uint)SDL.SDL_Keycode.SDLK_F:
                            Keypad[0xF] = false;
                            break;
                    }
                    break;
            };
        }

        return running;
    }
}
