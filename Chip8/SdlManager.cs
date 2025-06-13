using SDL3;

namespace Chip8;

public sealed class SdlManager
{
    private static IntPtr Window;
    private static IntPtr Renderer;
    private static IntPtr Texture;


    private static CPU Cpu;
    private static uint[,] Video = new uint[64, 32];

    public void Init() => Task.Run(() =>
    {
        if (SDL.SDL_Init(SDL.SDL_InitFlags.SDL_INIT_VIDEO))
            return;

        if (SDL.SDL_CreateWindowAndRenderer("CHIP-8 Interpreter", 64 * 8, 32 * 8, SDL.SDL_WindowFlags.SDL_WINDOW_RESIZABLE, out Window, out Renderer))
            return;

        if (Window == IntPtr.Zero)
        {
            // Sdl.SDL_LogCategory.SDL_LOG_CATEGORY_ERROR = 1;
            SDL.SDL_LogError(1, $"Could not create window: \n {SDL.SDL_GetError()}");
            Console.WriteLine("Sdl could not create a window.");
            return;
        }

        // Texture = SDL.SDL_CreateTexture(Renderer, SDL.SDL_PixelFormat.SDL_PIXELFORMAT_RGBA8888, SDL.SDL_TextureAccess.SDL_TEXTUREACCESS_STREAMING, 64, 38);

        SDL.SDL_SetRenderScale(Renderer, 8, 8);

        bool running = true;
        SDL.SDL_Event @event;

        while (running)
        {
            while (SDL.SDL_PollEvent(out @event))
            {
                switch (@event.type)
                {
                    case (uint)SDL.SDL_EventType.SDL_EVENT_QUIT:
                        running = false;
                        break;
                    case (uint)SDL.SDL_EventType.SDL_EVENT_KEY_DOWN:
                        switch (@event.key.key)
                        {
                            case (uint)SDL.SDL_Keycode.SDLK_ESCAPE:
                                running = false;
                                break;

                            case (uint)SDL.SDL_Keycode.SDLK_0:
                                Cpu.Keypad[0] = true;
                                break;

                            case (uint)SDL.SDL_Keycode.SDLK_1:
                                Cpu.Keypad[1] = true;
                                break;
                            case (uint)SDL.SDL_Keycode.SDLK_2:
                                Cpu.Keypad[2] = true;
                                break;
                            case (uint)SDL.SDL_Keycode.SDLK_3:
                                Cpu.Keypad[3] = true;
                                break;
                            case (uint)SDL.SDL_Keycode.SDLK_4:
                                Cpu.Keypad[4] = true;
                                break;
                            case (uint)SDL.SDL_Keycode.SDLK_5:
                                Cpu.Keypad[5] = true;
                                break;
                            case (uint)SDL.SDL_Keycode.SDLK_6:
                                Cpu.Keypad[6] = true;
                                break;
                            case (uint)SDL.SDL_Keycode.SDLK_7:
                                Cpu.Keypad[7] = true;
                                break;
                            case (uint)SDL.SDL_Keycode.SDLK_8:
                                Cpu.Keypad[8] = true;
                                break;
                            case (uint)SDL.SDL_Keycode.SDLK_9:
                                Cpu.Keypad[9] = true;
                                break;
                            case (uint)SDL.SDL_Keycode.SDLK_A:
                                Cpu.Keypad[0xA] = true;
                                break;
                            case (uint)SDL.SDL_Keycode.SDLK_B:
                                Cpu.Keypad[0xB] = true;
                                break;
                            case (uint)SDL.SDL_Keycode.SDLK_C:
                                Cpu.Keypad[0xC] = true;
                                break;
                            case (uint)SDL.SDL_Keycode.SDLK_E:
                                Cpu.Keypad[0xE] = true;
                                break;
                            case (uint)SDL.SDL_Keycode.SDLK_F:
                                Cpu.Keypad[0xF] = true;
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
                                Cpu.Keypad[0] = false;
                                break;

                            case (uint)SDL.SDL_Keycode.SDLK_1:
                                Cpu.Keypad[1] = false;
                                break;
                            case (uint)SDL.SDL_Keycode.SDLK_2:
                                Cpu.Keypad[2] = false;
                                break;
                            case (uint)SDL.SDL_Keycode.SDLK_3:
                                Cpu.Keypad[3] = false;
                                break;
                            case (uint)SDL.SDL_Keycode.SDLK_4:
                                Cpu.Keypad[4] = false;
                                break;
                            case (uint)SDL.SDL_Keycode.SDLK_5:
                                Cpu.Keypad[5] = false;
                                break;
                            case (uint)SDL.SDL_Keycode.SDLK_6:
                                Cpu.Keypad[6] = false;
                                break;
                            case (uint)SDL.SDL_Keycode.SDLK_7:
                                Cpu.Keypad[7] = false;
                                break;
                            case (uint)SDL.SDL_Keycode.SDLK_8:
                                Cpu.Keypad[8] = false;
                                break;
                            case (uint)SDL.SDL_Keycode.SDLK_9:
                                Cpu.Keypad[9] = false;
                                break;
                            case (uint)SDL.SDL_Keycode.SDLK_A:
                                Cpu.Keypad[0xA] = false;
                                break;
                            case (uint)SDL.SDL_Keycode.SDLK_B:
                                Cpu.Keypad[0xB] = false;
                                break;
                            case (uint)SDL.SDL_Keycode.SDLK_C:
                                Cpu.Keypad[0xC] = false;
                                break;
                            case (uint)SDL.SDL_Keycode.SDLK_E:
                                Cpu.Keypad[0xE] = false;
                                break;
                            case (uint)SDL.SDL_Keycode.SDLK_F:
                                Cpu.Keypad[0xF] = false;
                                break;
                        }
                        break;
                    default:
                        SDL.SDL_LogError(1, SDL.SDL_GetError());
                        break;
                }
                ;
            }

            // SDL.SDL_SetRenderDrawColor(Renderer, 0, 0, 0, 255);

            // SDL.SDL_RenderClear(Renderer);
        }
    });
}
