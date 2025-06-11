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

    private static uint[,] Video = new uint [64, 32];
    private static bool[] Keypad = new bool[0xF];

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

    public void EmulateCycle()
    {
        var opcode = Fetch();

        var param = Decode(opcode);


    }

    public ushort GetNextInstruction(in ushort PC) =>
        (ushort)((RAM[PC] << 8) | RAM[PC + 1]);

    internal ushort Fetch()
    {
        var opcode = GetNextInstruction(PC);
        PC += 2;

        return opcode;
    }


    internal OpCode Decode(ushort opcode) =>
        new OpCode(opcode);


    internal void Execute(OpCode opCode)
    {
        var instruction = opCode.Nibble;

        Action x = instruction switch
        {
            0x0000 => () => OpCodeHandlers[instruction](),
            0x1000 => () => OP_1NNN(instruction),
            0x2000 => () => OP_2NNN(instruction),
            0x3000 => () => OP_3XNN(instruction),
            0x4000 => () => OP_4XNN(instruction),
            0x5000 => () => OP_5XY0(instruction),
            0x6000 => () => OP_6XNN(instruction),
            0x7000 => () => OP_7XNN(instruction),
            0x8000 => () =>
            {
                var subCode = (ushort)(instruction & 0x000F);
                Action y = subCode switch
                {
                    0x0 => () => OP_8XY0(instruction),
                    0x1 => () => OP_8XY1(instruction),
                    0x2 => () => OP_8XY2(instruction),
                    0x3 => () => OP_8XY3(instruction),
                    0x4 => () => OP_8XY4(instruction),
                    0x5 => () => OP_8XY5(instruction),
                    0x6 => () => OP_8XY6(instruction),
                    0x7 => () => OP_8XY7(instruction),
                    0xE => () => OP_8XYE(instruction),
                    _ => throw new NotSupportedException()
                };

                y();
            },
            0x9000 => () => OP_9XY0(instruction),
            0xA000 => () => OP_ANNN(instruction),
            0xB000 => () => OP_BNNN(instruction),
            0xC000 => () => OP_CXNN(instruction),
            0xD000 => () => OP_DXYN(instruction),
            0xE000 => () =>
            {
                var subCode = instruction & 0x00FF;
                Action action = subCode == 0X9E
                    ? () => OP_EX9E(instruction)
                    : () => OP_EXA1(instruction);

                action();
            },
            0xF000 => () =>
            {
                var subCode = (ushort)(instruction & 0x00FF);
                Action y = subCode switch
                {
                    0X07 => () => OP_FX07(instruction),
                    0X0A => () => OP_FX0A(instruction),
                    0X15 => () => OP_FX15(instruction),
                    0X18 => () => OP_FX18(instruction),
                    0X1E => () => OP_FX1E(instruction),
                    0X29 => () => OP_FX29(instruction),
                    0X33 => () => OP_FX33(instruction),
                    0X55 => () => OP_FX55(instruction),
                    0X65 => () => OP_FX65(instruction),
                    _ => throw new NotSupportedException()
                };

                y();
            }
            ,
            _ => throw new NotSupportedException(),
        };

        x();
    }

#region intruction/opcodes
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
    public void OP_3XNN(ushort address)
    {
        var opCode = new OpCode(address);
        ushort Vx = opCode.X;
        if (V[Vx] == opCode.NN)
            PC += 2; // Skip next instruction
    }

    // SNE Vx, byte
    // Skip next instruction if Vx != kk.
    public void OP_4XNN(ushort address)
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
    public void OP_6XNN(ushort address)
    {
        ushort Vx = new OpCode(address).X;
        V[Vx] = (byte)(address & 0x00FF);
    }

    // ADD Vx, byte
    // Set Vx = Vx + kk.
    public void OP_7XNN(ushort address)
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
    public void OP_CXNN(ushort address)
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
    // Skip next instruction if key with the value of Vx is pressed.
    public void OP_EX9E(ushort address)
    {
        byte Vx = new OpCode(address).X;

        if (Keypad[V[Vx]])
        {
            PC += 2;
        }
    }

    // SKPP Vx
    // Skip next instruction if key with the value of Vx is not pressed.
    public void OP_EXA1(ushort address)
    {
        byte Vx = new OpCode(address).X;

        if (!Keypad[V[Vx]])
        {
            PC += 2;
        }
    }

    // LD Vx, DT
    // Set Vx = delay timer value
    public void OP_FX07(ushort address)
    {
        byte Vx = new OpCode(address).X;

        V[Vx] = DelayTimer;
    }

    // LD Vx, K
    // Wait for a key press, store the value of the key in Vx.
    public void OP_FX0A(ushort address)
    {
        byte Vx = new OpCode(address).X;

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
    public void OP_FX15(ushort address)
    {
        byte Vx = new OpCode(address).X;

        DelayTimer = V[Vx];
    }

    // LD ST, Vx
    // Set sound timer = Vx
    public void OP_FX18(ushort address)
    {
        byte Vx = new OpCode(address).X;

        SoundTimer = V[Vx];
    }

    // ADD I, Vx
    // Set I = I + Vx
    public void OP_FX1E(ushort address)
    {
        byte Vx = new OpCode(address).X;

        I += V[Vx];
    }

    // LD F, Vx
    // Set I = location of sprite for digit Vx.
    public void OP_FX29(ushort address)
    {
        byte Vx = new OpCode(address).X;

        // Getting the values in the memory address where the digit begins.
        I = (ushort)(Start_Font + (Vx * 5));
    }

    // LD B, Vx
    // Store BCD representation of Vx in memory locations I, I + 1, and I + 2.
    public void OP_FX33(ushort address)
    {
        byte Vx = new OpCode(address).X;
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
    public void OP_FX55(ushort address)
    {
        byte Vx = new OpCode(address).X;

        for (int i = 0; i < Vx; i++)
        {
            RAM[I + i] = V[i];
        }
    }

    // LD Vx, [I]
    // Read registers V0 through VVx from memory starting at location I.
    public void OP_FX65(ushort address)
    {
        byte Vx = new OpCode(address).X;

        for (int i = 0; i < Vx; i++)
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
