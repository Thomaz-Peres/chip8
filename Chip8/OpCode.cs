namespace Chip8;


public readonly ref struct OpCode
{
    public ushort Opcode { get; }

    // get NNN (12 bits)
    public ushort NNN => (ushort)(Opcode & 0x0FFF);

    // get NN (8 bits)
    public byte NN => (byte)(Opcode & 0x00FF);

    // get N (4 bits)
    public byte N => (byte)(Opcode & 0x000F);

    // get X (bits 8-11)
    public byte X => (byte)((Opcode & 0x0F00) >> 8);

    // get Y (bits 4-7)
    public byte Y => (byte)((Opcode & 0x00F0) >> 4);

    public ushort Nibble => (ushort)(Opcode & 0xF000);

    public OpCode(ushort opCode) =>
        Opcode = opCode;
}
