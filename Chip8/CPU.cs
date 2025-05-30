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
        PC = new OpCode(address).NNN;
    }

    // Call subroutine at NNN
    public void OP_2NNN(ushort address)
    {
        address = new OpCode(address).NNN;
        Stack.Push(PC);
        PC = address;
    }

    // SE Vx, byte
    // Skip next instruction if Vx = kk.
    public void OP_3XKK(ushort address)
    {
        var opCode = new OpCode(address);
        ushort Vx = opCode.X;
        if (V[Vx] == opCode.NN)
            PC += 2; // Skip next instruction
    }

    // SNE Vx, byte
    // Skip next instruction if Vx != kk.
    public void OP_4XKK(ushort address)
    {
        var opCode = new OpCode(address);
        ushort Vx = opCode.X;
        if (V[Vx] != opCode.NN)
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
    // Set Vx = Vx SHR 1.
    public void OP_8XY6(ushort address)
    {
        var opCode = new OpCode(address);
        ushort Vx = opCode.X;

        V[0xF] = (byte)(V[Vx] & 0x1);

        // Right shift is perfomed division by 2 (0100 -> 0010)
        V[Vx] >>= 1;
    }

    // SUBN Vx, Vy
    // Set Vx = Vx - Vy, set VF = NOT borrow
    public void OP_8XY7(ushort address)
    {
        var opCode = new OpCode(address);
        ushort Vx = opCode.X;
        ushort Vy = opCode.X;

        if (V[Vy] > V[Vx])
            V[0xF] = 1;
        else
            V[0xF] = 0;

        V[Vx] -= V[Vy];
    }

    // SHL Vx {, Vy}
    // Set Vx = Vx SHL 1.
    public void OP_8XYE(ushort address)
    {
        var opCode = new OpCode(address);
        ushort Vx = opCode.X;

        V[0xF] = (byte)((V[Vx] & 0x80) >> 7);

        // Left shift is perfomed multiplication by 2 (0100 -> 1000)
        V[Vx] <<= 1;
    }

    // SNE Vx {, Vy}
    // Skip next instruction if Vx != Vy.
    public void OP_9XY0(ushort address)
    {
        var opCode = new OpCode(address);
        ushort Vx = opCode.X;
        ushort Vy = opCode.Y;

        if (V[Vx] != V[Vy])
            PC += 2;
    }

    // SET I, addr
    // Set I = nnn
    public void OP_ANNN(ushort address) => I = new OpCode(address).NNN;

    // JP V0, addr
    // Jump to location NNN + V0.
    public void OP_BNNN(ushort address) => PC = (byte)(new OpCode(address).NNN + V[0]);

    // RND Vx, byte
    // Set Vx = random byte and kk.
    public void OP_CXKK(ushort address)
    {
        var opCode = new OpCode(address);
        byte randByte = (byte)new Random().Next(0, 255);

        V[opCode.X] = (byte)(randByte & opCode.NN);
    }

    // DRW Vx, Vy, nibble
    // Display n-byte sprite starting at memory location I at (Vx, Vy), set VF = collision.
    public void OP_DXYN(ushort address)
    {
        var opCode = new OpCode(address);
        var Vx = opCode.X;
        var Vy = opCode.Y;
        var heigh = opCode.N;

        var xPos = V[Vx] % 64;
        var yPos = V[Vy] % 32;

        V[0xF] = 0;

        for (var row = 0; row < heigh; row++)
        {
            var spriteByte = RAM[I + row];

            for (int col = 0; col < 8; col++)
            {
                var spritePixel = spriteByte & (0b1000_0000 >> col);
                var screenPixel = Video[(xPos + col) * 32, (yPos + row) * 32];

                if (spritePixel != 0)
                {
                    if (screenPixel == 0XFFFFFFFF)
                    {
                        V[0xF] = 1;
                    }

                    screenPixel ^= 0xFFFFFFFF;
                }
            }
        }
    }


    // SKP Vx
    // Skip next instruction if key with the value of Vx is not pressed.
    public void OP_EX9E(ushort address)
    {
        byte Vx = new OpCode(address).X;

        byte key = V[Vx];

        if (keypad[key])
        {
            PC += 2;
        }
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
