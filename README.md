# CactbotExtension
## About
CactbotExtension is an ACT plugin that adds some useful JS events and callbacks for [cactbot](https://github.com/quisquous/cactbot)
Here is a [demo](https://github.com/Loskh/EchoOverlay/blob/master/Zodiark/zodiark.ts) of it.
## JS Events
* MapEffect
### MapEffect
MapEffect usually indicates the AOE and other changes on a map, e.g removing roadblocks in a dungeon , add behemoth or firelines in Zodiark-Ex.
Here is the sturct of the packet.
```C#
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
```
#### How to Find it
* IDA （Not stable）
  - Search the following bytes and you may see 1 function at least.
  ```
  48 89 5C 24 ? 48 89 6C 24 ? 48 89 74 24 ? 57 48 83 EC ? 80 3D ? ? ? ? ? 41 0F B7 E8
  ```
  - Trace these functions up about 3 levels until enter the function that processes network down packets.And you will see a big function with a switch-case, The aob of this function is following. The case value is the opcode of MapEffect.
    - Intl Servers
    ```
    48 89 5C 24 ?? 56 48 83 EC 50 8B F2
    ```
    - CN Serves
    ```
    40 55 56 57 48 8D 6C 24 ?? 48 81 EC ?? ?? ?? ?? 48 8B 05 ?? ?? ?? ?? 48 33 C4 48 89 45 ?? 8B ?? 49 8B ??
    ```
* Analysis network package
  - Enter "Amaurot" and walking forward until a meteor smashed on the ground
  - Filter the packet
    - Length = 48 （ServerMessageHeader included）
    - param1 = 80030043, parm2 = 00080004, parm3 = 00000003 , parm4 = 00000000.The bytes is following.
      ```
      43 00 03 80 04 00 08 00  03 00 00 00 00 00 00 00
      ```

