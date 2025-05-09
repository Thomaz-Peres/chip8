using SDL3;

//Console.WriteLine("Current directory: " + Environment.CurrentDirectory);
//Console.WriteLine("Looking for SDL3.dll...");


SDL.SDL_Init(SDL.SDL_InitFlags.SDL_INIT_VIDEO);

var window = SDL.SDL_CreateWindow("CHIP-8 interpreter", 64 * 8, 32 * 8, SDL.SDL_WindowFlags.SDL_WINDOW_ALWAYS_ON_TOP);


Console.ReadLine();