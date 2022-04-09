using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace CactbotExtension.Events
{
    [StructLayout(LayoutKind.Explicit)]
    public struct ServerMessageHeader
    {
        [FieldOffset(0)]
        public uint MessageLength;

        [FieldOffset(4)]
        public uint ActorID;

        [FieldOffset(8)]
        public uint LoginUserID;

        [FieldOffset(12)]
        public uint Unknown1;

        [FieldOffset(16)]
        public ushort Unknown2;

        [FieldOffset(18)]
        public ushort MessageType;

        [FieldOffset(20)]
        public uint Unknown3;

        [FieldOffset(24)]
        public uint Seconds;

        [FieldOffset(28)]
        public uint Unknown4;
    }
}
