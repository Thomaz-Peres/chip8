namespace Chip8;

public class CPU
{
    private byte[] RAM = new byte[4096];
    private Stack<ushort> Stack;
    private readonly Timer DelayTimer;
    private readonly byte[] V = new byte[16];
    private readonly ushort PC = 0x200;

    public CPU()
    {
    }
}
