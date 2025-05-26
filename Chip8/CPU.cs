namespace Chip8;

public class CPU
{
    private byte[] RAM = new byte[4096];
    private Stack<ushort> Stack = new Stack<ushort>(16);
    private byte DelayTimer;
    private byte SoundTimer;
    private readonly byte[] V = new byte[16];
    private ushort I;
    private readonly static ushort Start_Address = 0x200;
    private ushort PC;
    private static uint[,] Video = new uint [64, 32];

    private readonly IDictionary<ushort, Action> OpCodeHandlers;

    public CPU()
    {
        PC = Start_Address;
        LoadFontSet();

        OpCodeHandlers = new Dictionary<ushort, Action>()
        {
            [0x00E0] = () => Array.Clear(Video, 0, Video.Length),
            [0x00EE] = () => Stack.Pop(),
        };
    }

    public void OP_1NNN(ushort address)
    {
        PC = address;
    }

    // Reading somethings, I understand the usually started font is the 0x50, and all roms start at this.
    // but, if create a rom (i will try) can start from 0x00
    private void LoadFontSet()
    {
        for (int i = 0; i < Fonts.Sprites.Length; i++)
        {
            RAM[0x50 + i] = Fonts.Sprites[i];
        }
    }

    /// <summary>
    /// Consider the file alredy readed in this time, just get the program lenght
    /// </summary>
    /// <param name="program"></param>
    public void LoadROM(byte[] program)
    {
        for (int i = 0; i < program.Length; i++)
        {
            RAM[Start_Address + i] = program[i];
        }
    }
}
