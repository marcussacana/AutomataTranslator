using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AdvancedBinary;
using System.IO;

namespace AutomataTranslator {    
     public class MRubyStringEditor {
        byte[] Script;
        int StringStartPos;
        int StringEndPos;
        RiteHdr Hdr;
        public MRubyStringEditor(byte[] Script) {
            this.Script = Script;
        }

        public string[] Import() {
            StructReader Reader = new StructReader(new MemoryStream(Script),true ,Encoding.UTF8);

            Hdr = new RiteHdr();
            try { Reader.ReadStruct(ref Hdr); }
            catch { throw new Exception("Invalid Script"); }

            if (Hdr.Signature != "RITE0003")
                throw new Exception("Invalid Script");
            //return if looks other script type
            //if (Hdr.UnkSignature2 != "MATZ0000IREP" || Hdr.UnkSignature3 != "ScriptProxy")
            //    return new string[0];

           //Crash if the scripts have a crc missmatch
           // if (Hdr.Checksum != CRC(Script))
           //     throw new Exception("Corrupted Script");
            StringStartPos = (Hdr.Unk2DataLength * 4) + 0x84 + 0x08;//0x84 = Relative Offset; 0x8 = ??
            Reader.BaseStream.Position = StringStartPos;
            List<string> Strings = new List<string>();
            while (true) {
                //Get Length
                byte[] LenBuff = new byte[3];
                Reader.Read(LenBuff, 0, LenBuff.Length);
                Array.Reverse(LenBuff, 0, LenBuff.Length);
                byte[] dw = new byte[4];
                LenBuff.CopyTo(dw, 0);
                int Len = BitConverter.ToInt32(dw, 0);
                if (Len == 0)
                    break;
                //Get Content
                byte[] Buffer = new byte[Len];
                Reader.Read(Buffer, 0, Buffer.Length);
                Strings.Add(Encoding.UTF8.GetString(Buffer));
            }
            StringEndPos = (int)Reader.BaseStream.Position - 3;
            Reader.Close();
            return Strings.ToArray();
        }

        public byte[] Export(string[] Strings) {
            byte[] Prefix = new byte[StringStartPos];
            Array.Copy(Script, Prefix, Prefix.Length);

            byte[] Sufix = new byte[Script.Length - StringEndPos];
            Array.Copy(Script, StringEndPos, Sufix, 0, Sufix.Length);

            int StringTableLen = StringEndPos - StringStartPos;
            
            byte[] StringTable = new byte[0];
            for (int i = 0; i < Strings.Length; i++) {
                byte[] String = Encoding.UTF8.GetBytes(Strings[i]);
                if (!(i+1 < Strings.Length)) { //if is the last string
                    const byte SPACE = 0x20;
                    //while the output file is smaller than the input
                    while ((StringTable.Length + String.Length + 3) - StringTableLen < 0)//+3 = str offset len
                        Append(ref String, new byte[] { SPACE });
                }
                Append(ref StringTable, UInt24(Tools.Reverse(String.Length)));
                Append(ref StringTable, String);
            }

            int Diff = StringTable.Length - StringTableLen;
            Hdr.Unk1DataOffset = (ushort)(Hdr.Unk1DataOffset + Diff);
            Hdr.UnkOff = (ushort)(Hdr.UnkOff + Diff);
            Hdr.ScriptSize = (ushort)(Prefix.Length + Sufix.Length + StringTable.Length);
            Tools.BuildStruct(ref Hdr, true, Encoding.UTF8).CopyTo(Prefix, 0);

            byte[] OutScript = new byte[0];
            Append(ref OutScript, Prefix);
            Append(ref OutScript, StringTable);
            Append(ref OutScript, Sufix);
            BitConverter.GetBytes(Tools.Reverse(CRC(OutScript))).CopyTo(OutScript, 0x8);

            return OutScript;
        }

        private byte[] UInt24(int Value) {
            byte[] Bytes = BitConverter.GetBytes(Value);
            return new byte[] { Bytes[1], Bytes[2], Bytes[3] };
        }

        private void Append<T>(ref T[] Arr1, T[] Arr2) {
            T[] Arr3 = new T[Arr1.Length + Arr2.Length];
            Arr1.CopyTo(Arr3, 0);
            Arr2.CopyTo(Arr3, Arr1.Length);
            Arr1 = Arr3;
        }

        public ushort CRC(byte[] Binary, int Index = 0xA) {
            uint crc = 0;
            for (int i = Index; i < Binary.Length; i++) {
                crc |= Binary[i];
                for (int x = 0; x < 8; x++) {
                    crc <<= 1;
                    if ((crc & 0x01000000) != 0)
                        crc ^= (uint)0x11021 << 8;
                }
            }
            return (ushort)(crc >> 8);
        }
    }
#pragma warning disable CS0649, CS0169
    struct RiteHdr {
        [FString(Length = 0x8)]
        internal string Signature;

        internal ushort Checksum;
        private ushort unk1;
        internal ushort ScriptSize;

        [FString(Length = 0xC)]
        internal string UnkSignature2;
        internal uint UnkOff;

        private ushort unk2;
        private ulong unk3;
        private ulong unk4;
        private ulong unk5;
        private ulong unk6;
        private ulong unk7;
        private ulong unk8;
        private ulong unk9;
        private uint unk10;

        
        [PString(PrefixType = Const.UINT16)]
        internal string UnkSignature3;
        private byte StrEnd1;

        [PString(PrefixType = Const.UINT16)]
        private string UnkSignature4;
        private byte StrEnd2;

        [PString(PrefixType = Const.UINT16)]
        private string UnkSignature5;
        private byte StrEnd3;
        
        internal uint Unk1DataOffset;
        internal ulong unk11;
        internal ushort Unk2DataLength;
    }

#pragma warning restore CS1030, CS0169
}
