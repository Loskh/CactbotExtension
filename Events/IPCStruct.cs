using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace CactbotExtension.Events
{
    [StructLayout(LayoutKind.Explicit, Size = 16)]
    public struct FFXIVIpcMapEffect
    {
        [FieldOffset(0)]
        public uint parm1;

        [FieldOffset(4)]
        public uint parm2;

        [FieldOffset(8)]
        public ushort parm3;

        [FieldOffset(12)]
        public ushort parm4;
    }
}
