namespace Chip8;

public sealed class CPU
{
    private byte[] RAM = new byte[4096];
    private Stack<ushort> Stack = new Stack<ushort>(16);
    private byte DelayTimer;
    private byte SoundTimer;
    private ushort PC;
    private ushort I;

    private readonly byte[] V = new byte[16]; // Register
    private readonly static ushort Start_Address = 0x200;
    private readonly static ushort Start_Font = 0x50;

    private uint[] Video = new uint[64 * 32];
    public bool[] Keypad = new bool[0xF];

    public Span<uint> GetVideoPtr() => Video;

    public CPU()
    {
        PC = Start_Address;
        LoadFontSet();
    }

    public void EmulateCycle()
    {
        var opcode = Fetch();

        var param = Decode(opcode);

        Execute(param);

        if (DelayTimer > 0)
            DelayTimer--;

        if (SoundTimer > 0)
            SoundTimer--;
    }

    public ushort GetNextInstruction(in ushort PC) =>
        (ushort)((RAM[PC] << 8) | RAM[PC + 1]);

    private ushort Fetch()
    {
        var opcode = GetNextInstruction(PC);
        PC += 2;

        return opcode;
    }

    private OpCode Decode(ushort opcode) =>
        new OpCode(opcode);

    private void Execute(OpCode opCode)
    {
        var instruction = opCode.Nibble;

        switch (instruction)
        {
            case 0x0000:
                {
                    switch (opCode.N)
                    {
                        case 0x0:
                            Array.Clear(Video, 0, Video.Length);
                            break;
                        case 0xE:
                            Stack.Pop();
                            break;
                    }
                }
                break;
            case 0x1000:
                OP_1NNN(opCode);
                break;
            case 0x2000:
                OP_2NNN(opCode);
                break;
            case 0x3000:
                OP_3XNN(opCode);
                break;
            case 0x4000:
                OP_4XNN(opCode);
                break;
            case 0x5000:
                OP_5XY0(opCode);
                break;
            case 0x6000:
                OP_6XNN(opCode);
                break;
            case 0x7000:
                OP_7XNN(opCode);
                break;
            case 0x8000:
                {
                    switch (opCode.N)
                    {
                        case 0x0: OP_8XY0(opCode); break;
                        case 0x1: OP_8XY1(opCode); break;
                        case 0x2: OP_8XY2(opCode); break;
                        case 0x3: OP_8XY3(opCode); break;
                        case 0x4: OP_8XY4(opCode); break;
                        case 0x5: OP_8XY5(opCode); break;
                        case 0x6: OP_8XY6(opCode); break;
                        case 0x7: OP_8XY7(opCode); break;
                        case 0xE: OP_8XYE(opCode); break;
                    }
                };
                break;
            case 0x9000:
                OP_9XY0(opCode);
                break;
            case 0xA000:
                OP_ANNN(opCode);
                break;
            case 0xB000:
                OP_BNNN(opCode);
                break;
            case 0xC000:
                OP_CXNN(opCode);
                break;
            case 0xD000:
                OP_DXYN(opCode);
                break;
            case 0xE000:
                {
                    if (opCode.NN == 0x9E)
                        OP_EX9E(opCode);
                    else if (opCode.NN == 0xA1)
                        OP_EXA1(opCode);
                };
                break;
            case 0xF000:
                {
                    switch (opCode.NN)
                    {
                        case 0x07: OP_FX07(opCode); break;
                        case 0x0A: OP_FX0A(opCode); break;
                        case 0x15: OP_FX15(opCode); break;
                        case 0x18: OP_FX18(opCode); break;
                        case 0x1E: OP_FX1E(opCode); break;
                        case 0x29: OP_FX29(opCode); break;
                        case 0x33: OP_FX33(opCode); break;
                        case 0x55: OP_FX55(opCode); break;
                        case 0x65: OP_FX65(opCode); break;
                    }
                };
                break;
        }
    }

    #region intruction/opcodes
    // JMP (jump to location NNN)
    public void OP_1NNN(OpCode opCode)
    {
        PC = opCode.NNN;
    }

    // Call subroutine at NNN
    public void OP_2NNN(OpCode opCode)
    {
        Stack.Push(PC);
        PC = opCode.NNN;
    }

    // SE Vx, byte
    // Skip next instruction if Vx = kk.
    public void OP_3XNN(OpCode opCode)
    {
        ushort Vx = opCode.X;
        if (V[Vx] == opCode.NN)
            PC += 2; // Skip next instruction
    }

    // SNE Vx, byte
    // Skip next instruction if Vx != kk.
    public void OP_4XNN(OpCode opCode)
    {
        ushort Vx = opCode.X;
        if (V[Vx] != opCode.NN)
            PC += 2; // Skip next instruction
    }

    // SE Vx, Vy
    // Skip next instruction if Vx = Vy.
    public void OP_5XY0(OpCode opCode)
    {
        ushort Vx = opCode.X;
        ushort Vy = opCode.Y;
        if (V[Vx] == V[Vy])
            PC += 2; // Skip next instruction
    }

    // LD Vx, byte
    // Set Vx = kk.
    public void OP_6XNN(OpCode opCode)
    {
        ushort Vx = opCode.X;
        V[Vx] = opCode.NN;
    }

    // ADD Vx, byte
    // Set Vx = Vx + kk.
    public void OP_7XNN(OpCode opCode)
    {
        ushort Vx = opCode.X;
        V[Vx] += opCode.NN;
    }

    // LD Vx, Vy
    // Set Vx = Vy.
    public void OP_8XY0(OpCode opCode)
    {
        ushort Vx = opCode.X;
        ushort Vy = opCode.Y;
        V[Vx] = V[Vy];
    }

    // OR Vx, Vy
    // Set Vx = Vx OR Vy.
    public void OP_8XY1(OpCode opCode)
    {
        ushort Vx = opCode.X;
        ushort Vy = opCode.Y;
        V[Vx] |= V[Vy];
    }

    // AND Vx, Vy
    // Set Vx = Vx AND Vy.
    public void OP_8XY2(OpCode opCode)
    {
        ushort Vx = opCode.X;
        ushort Vy = opCode.Y;

        V[Vx] &= V[Vy];
    }

    // XOR Vx, Vy
    // Set Vx = Vx XOR Vy.
    public void OP_8XY3(OpCode opCode)
    {
        ushort Vx = opCode.X;
        ushort Vy = opCode.Y;

        V[Vx] ^= V[Vy];
    }

    // ADD Vx, Vy
    // Set Vx = Vx + Vy, set VF = carry.
    public void OP_8XY4(OpCode opCode)
    {
        ushort Vx = opCode.X;
        ushort Vy = opCode.Y;

        if ((V[Vx] + V[Vy]) > 0xFF)
            V[0xF] = 1;
        else
            V[0xF] = 0;

        V[Vx] = (byte)(V[Vx] + V[Vy] & 0xFF);
    }

    // SUB Vx, Vy
    // Set Vx = Vx - Vy, set VF = NOT borrow.
    public void OP_8XY5(OpCode opCode)
    {
        ushort Vx = opCode.X;
        ushort Vy = opCode.Y;

        if (V[Vx] > V[Vy])
            V[0xF] = 1;
        else
            V[0xF] = 0;

        V[Vx] -= V[Vy];
    }

    // SHR Vx {, Vy}
    // Set Vx = Vx SHR 1.
    public void OP_8XY6(OpCode opCode)
    {
        ushort Vx = opCode.X;

        V[0xF] = (byte)(V[Vx] & 0x1);

        // Right shift is perfomed division by 2 (0100 -> 0010)
        V[Vx] >>= 1;
    }

    // SUBN Vx, Vy
    // Set Vx = Vx - Vy, set VF = NOT borrow
    public void OP_8XY7(OpCode opCode)
    {
        ushort Vx = opCode.X;
        ushort Vy = opCode.X;

        if (V[Vy] > V[Vx])
            V[0xF] = 1;
        else
            V[0xF] = 0;

        V[Vx] = (byte)(V[Vy] - V[Vx]);
    }

    // SHL Vx {, Vy}
    // Set Vx = Vx SHL 1.
    public void OP_8XYE(OpCode opCode)
    {
        ushort Vx = opCode.X;

        V[0xF] = (byte)((V[Vx] & 0x80) >> 7);

        // Left shift is perfomed multiplication by 2 (0100 -> 1000)
        V[Vx] <<= 1;
    }

    // SNE Vx {, Vy}
    // Skip next instruction if Vx != Vy.
    public void OP_9XY0(OpCode opCode)
    {
        ushort Vx = opCode.X;
        ushort Vy = opCode.Y;

        if (V[Vx] != V[Vy])
            PC += 2;
    }

    // SET I, addr
    // Set I = nnn
    public void OP_ANNN(OpCode opCode) =>
        I = opCode.NNN;

    // JP V0, addr
    // Jump to location NNN + V0.
    public void OP_BNNN(OpCode opCode) =>
        PC = (byte)(opCode.NNN + V[0]);

    // RND Vx, byte
    // Set Vx = random byte and kk.
    public void OP_CXNN(OpCode opCode)
    {
        byte randByte = (byte)new Random().Next(0, 255);

        V[opCode.X] = (byte)(randByte & opCode.NN);
    }

    // DRW Vx, Vy, nibble
    // Display n-byte sprite starting at memory location I at (Vx, Vy), set VF = collision.
    public void OP_DXYN(OpCode opCode)
    {
        var x = opCode.X;
        var y = opCode.Y;
        var height = opCode.N;

        var xPos = V[x] % 64;
        var yPos = V[y] % 32;

        V[0xF] = 0;

        for (var row = 0; row < height; row++)
        {
            var spriteByte = RAM[I + row];

            for (int col = 0; col < 8; col++)
            {
                // var spritePixel = spriteByte & (0x80 >> col);
                // var screenPixel = Video[(yPos + row) * 64 + (xPos + col)];

                // if (spritePixel != 0)
                // {
                //     if (screenPixel == 0xFFFFFFFF)
                //     {
                //         V[0xF] = 1;
                //     }

                //     screenPixel ^= 0xFFFFFFFF;
                // }

                byte pixel = (byte)((spriteByte >> (7 - col)) & 1);
                var index = Video[((y + row) % 32) * 64 + ((x + col) % 64)];

                if (pixel == 1)
                {
                    if (Video[index] == 0xFFFFFFFF) // White pixel
                    {
                        V[0xF] = 1; // Set collision flag
                    }
                    Video[index] ^= 0xFFFFFFFF; // XOR the pixel
                }
            }
        }
    }


    // SKP Vx
    // Skip next instruction if key with the value of Vx is pressed.
    public void OP_EX9E(OpCode opCode)
    {
        byte Vx = opCode.X;

        if (Keypad[V[Vx]])
        {
            PC += 2;
        }
    }

    // SKPP Vx
    // Skip next instruction if key with the value of Vx is not pressed.
    public void OP_EXA1(OpCode opCode)
    {
        byte Vx = opCode.X;

        if (!Keypad[V[Vx]])
        {
            PC += 2;
        }
    }

    // LD Vx, DT
    // Set Vx = delay timer value
    public void OP_FX07(OpCode opCode)
    {
        byte Vx = opCode.X;

        V[Vx] = DelayTimer;
    }

    // LD Vx, K
    // Wait for a key press, store the value of the key in Vx.
    public void OP_FX0A(OpCode opCode)
    {
        byte Vx = opCode.X;

        for (byte i = 0x0; i < 0xF; i++)
        {
            if (Keypad[i])
            {
                V[Vx] = i;
                return;
            }
        }

        PC -= 2;
    }

    // LD DT, Vx
    // Set delay timer = Vx
    public void OP_FX15(OpCode opCode)
    {
        byte Vx = opCode.X;

        DelayTimer = V[Vx];
    }

    // LD ST, Vx
    // Set sound timer = Vx
    public void OP_FX18(OpCode opCode)
    {
        byte Vx = opCode.X;

        SoundTimer = V[Vx];
    }

    // ADD I, Vx
    // Set I = I + Vx
    public void OP_FX1E(OpCode opCode)
    {
        byte Vx = opCode.X;

        I += V[Vx];
    }

    // LD F, Vx
    // Set I = location of sprite for digit Vx.
    public void OP_FX29(OpCode opCode)
    {
        byte Vx = opCode.X;

        // Getting the values in the memory address where the digit begins.
        I = (ushort)(Start_Font + (Vx * 5));
    }

    // LD B, Vx
    // Store BCD representation of Vx in memory locations I, I + 1, and I + 2.
    public void OP_FX33(OpCode opCode)
    {
        byte Vx = opCode.X;
        var value = V[Vx];

        // Ones-place
        RAM[I + 2] = (byte)(value % 10);
        value /= 10;

        // Tens-place
        RAM[I + 1] = (byte)(value % 10);
        value /= 10;

        // Hundreds-place
        RAM[I] = (byte)(value % 10);
    }

    // LD [I], Vx
    // Store registers V0 through Vx in memory starting at location I.
    public void OP_FX55(OpCode opCode)
    {
        byte Vx = opCode.X;

        for (int i = 0; i <= Vx; i++)
        {
            RAM[I + i] = V[i];
        }
    }

    // LD Vx, [I]
    // Read registers V0 through VVx from memory starting at location I.
    public void OP_FX65(OpCode opCode)
    {
        byte Vx = opCode.X;

        for (int i = 0; i <= Vx; i++)
        {
            V[i] = RAM[I + i];
        }
    }
# endregion

    // Reading somethings, I understand the usually started font is the 0x50, and all roms start at this.
    // but, if create a rom (i will try) can start from 0x00
    private void LoadFontSet()
    {
        for (int i = 0; i < Fonts.Sprites.Length; i++)
        {
            RAM[Start_Font + i] = Fonts.Sprites[i];
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
