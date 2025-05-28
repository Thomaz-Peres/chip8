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

        // This looks good on beginning, problably I will remove it.
        OpCodeHandlers = new Dictionary<ushort, Action>()
        {
            [0x00E0] = () => Array.Clear(Video, 0, Video.Length),
            [0x00EE] = () => Stack.Pop(),
        };
    }

    public ushort GetNextInstruction(in ushort PC) => (ushort)(RAM[PC] << 8 | RAM[PC + 1]);

    // JMP (jump to location NNN)
    public void OP_1NNN(ushort address)
    {
        PC = (ushort)(address & 0x0FFF);
    }

    // Call subroutine at NNN
    public void OP_2NNN(ushort address)
    {
        address = (ushort)(address & 0x0FFF);
        Stack.Push(PC);
        PC = address;
    }

    // SE Vx, byte
    // Skip next instruction if Vx = kk.
    public void OP_3XKK(ushort address)
    {
        ushort Vx = new OpCode(address).X;
        if (V[Vx] == (address & 0x00FF))
            PC += 2; // Skip next instruction
    }

    // SNE Vx, byte
    // Skip next instruction if Vx != kk.
    public void OP_4XKK(ushort address)
    {
        ushort Vx = new OpCode(address).X;
        if (V[Vx] != (address & 0x00FF))
            PC += 2; // Skip next instruction
    }

    // SE Vx, Vy
    // Skip next instruction if Vx = Vy.
    public void OP_5XY0(ushort address)
    {
        ushort Vx = new OpCode(address).X;
        ushort Vy = new OpCode(address).Y;
        if (V[Vx] == V[Vy])
            PC += 2; // Skip next instruction
    }

    // LD Vx, byte
    // Set Vx = kk.
    public void OP_6XKK(ushort address)
    {
        ushort Vx = new OpCode(address).X;
        V[Vx] = (byte)(address & 0x00FF);
    }

    // ADD Vx, byte
    // Set Vx = Vx + kk.
    public void OP_7XKK(ushort address)
    {
        ushort Vx = new OpCode(address).X;
        V[Vx] += (byte)(address & 0x00FF);
    }

    // LD Vx, Vy
    // Set Vx = Vy.
    public void OP_8XY0(ushort address)
    {
        var opCode = new OpCode(address);
        ushort Vx = opCode.X;
        ushort Vy = opCode.Y;
        V[Vx] = V[Vy];
    }

    // OR Vx, Vy
    // Set Vx = Vx OR Vy.
    public void OP_8XY1(ushort address)
    {
        var opCode = new OpCode(address);
        ushort Vx = opCode.X;
        ushort Vy = opCode.Y;
        V[Vx] |= V[Vy];
    }

    // AND Vx, Vy
    // Set Vx = Vx AND Vy.
    public void OP_8XY2(ushort address)
    {
        var opCode = new OpCode(address);
        ushort Vx = opCode.X;
        ushort Vy = opCode.Y;

        V[Vx] &= V[Vy];
    }

    // XOR Vx, Vy
    // Set Vx = Vx XOR Vy.
    public void OP_8XY3(ushort address)
    {
        var opCode = new OpCode(address);
        ushort Vx = opCode.X;
        ushort Vy = opCode.Y;

        V[Vx] ^= V[Vy];
    }

    // ADD Vx, Vy
    // Set Vx = Vx + Vy, set VF = carry.
    public void OP_8XY4(ushort address)
    {
        var opCode = new OpCode(address);
        ushort Vx = opCode.X;
        ushort Vy = opCode.Y;

        if ((V[Vx] + V[Vy]) > 0xFF - 1)
            V[0xF] = 1;
        else
            V[0xF] = 0;

        V[Vx] = (byte)(V[Vx] + V[Vy] & 0xFF);
    }

    // SUB Vx, Vy
    // Set Vx = Vx - Vy, set VF = NOT borrow.
    public void OP_8XY5(ushort address)
    {
        var opCode = new OpCode(address);
        ushort Vx = opCode.X;
        ushort Vy = opCode.Y;

        if (V[Vx] > V[Vy])
            V[0xF] = 1;
        else
            V[0xF] = 0;

        V[Vx] = (byte)(V[Vx] - V[Vy] & 0xFF);
    }

    // SHR Vx {, Vy}
    // Set Vx = Vx = Vx SHR 1.
    public void OP_8XY6(ushort address)
    {
        var opCode = new OpCode(address);
        ushort Vx = opCode.X;

        V[0xF] = (byte)(V[Vx] & 0x1);

        // Right shift
        V[Vx] >>= 1;
    }

    // SUBN Vx, Vy
    // Set Vx = Vx = Vx SHR 1.
    public void OP_8XY7(ushort address)
    {
        var opCode = new OpCode(address);
        ushort Vx = opCode.X;

        V[0xF] = (byte)(V[Vx] & 0x1);

        // Right shift
        V[Vx] >>= 1;
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
