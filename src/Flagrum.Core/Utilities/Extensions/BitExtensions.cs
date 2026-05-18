using System;

namespace Flagrum.Core.Utilities.Extensions;

public class BitExtensions
{
    public static bool BitTest64(ulong address, int position)
    {
        if (position is < 0 or > 63)
        {
            throw new ArgumentOutOfRangeException(nameof(position), position,
            "Bit position of a 64-bit integer must be between 0 and 63");
        }

        var bytes = BitConverter.GetBytes(address);
        var index = position / 8;
        var positionInByte = position - index * 8;
        return (bytes[index] & GetBitTestNumber(positionInByte)) > 0;
    }

    private static int GetBitTestNumber(int positionInByte)
    {
        return positionInByte switch
        {
            7 => 128,
            6 => 64,
            5 => 32,
            4 => 16,
            3 => 8,
            2 => 4,
            1 => 2,
            0 => 1,
            _ => throw new ArgumentOutOfRangeException(nameof(positionInByte), positionInByte,
                "Bit position of a byte must be between 0 and 7")
        };
    }
}