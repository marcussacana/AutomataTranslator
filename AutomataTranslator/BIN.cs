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
        public int StringTableLength { get { return StringEndPos - StringStartPos; } }
        RiteHdr Hdr;
        public bool AssertLength = true;
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
                string String = ReadString(Reader.BaseStream);
                if (String == string.Empty) {
                    long Pos = Reader.BaseStream.Position;
                    bool Result = false;
                    try {
                        string tmp = ReadString(Reader.BaseStream);
                        if (!(tmp == string.Empty || tmp.Contains("_")))
                            Result = true;
                    }
                    catch { }
                    Reader.BaseStream.Position = Pos;
                    if (!Result)
                        break;
                }
                Strings.Add(String);
            }
            StringEndPos = (int)Reader.BaseStream.Position - 3;
            Reader.Close();
            return Strings.ToArray();
        }

        public string ReadString(Stream Reader) {
            //Get Length
            byte[] LenBuff = new byte[3];
            Reader.Read(LenBuff, 0, LenBuff.Length);
            Array.Reverse(LenBuff, 0, LenBuff.Length);
            byte[] dw = new byte[4];
            LenBuff.CopyTo(dw, 0);
            int Len = BitConverter.ToInt32(dw, 0);
            if (Len == 0)
                return string.Empty;

            //Get Content
            byte[] Buffer = new byte[Len];
            Reader.Read(Buffer, 0, Buffer.Length);
            return Encoding.UTF8.GetString(Buffer);
        }

        public byte[] Export(string[] Strings) {
            byte[] Prefix = new byte[StringStartPos];
            Array.Copy(Script, Prefix, Prefix.Length);

            byte[] Sufix = new byte[Script.Length - StringEndPos];
            Array.Copy(Script, StringEndPos, Sufix, 0, Sufix.Length);

            int FreeSpace = CalculateLength(Strings) - StringTableLength;
            bool FillMode = false;
            byte[] StringTable = new byte[0];
            for (int i = 0; i < Strings.Length; i++) {
                if (Strings[i] == " ")
                    FillMode = true;
                byte[] String = Encoding.UTF8.GetBytes(Strings[i]);
                if (FillMode && Strings[i] == " ") { //if is a blank string
                    const byte SPACE = 0x20;
                    //while the output file is smaller than the input
                    while (FreeSpace++ < 0)//
                        Append(ref String, new byte[] { SPACE });
                }
                Append(ref StringTable, UInt24(Tools.Reverse(String.Length)));
                Append(ref StringTable, String);
            }

            int Diff = StringTable.Length - StringTableLength;
            if (AssertLength && Diff != 0)
                throw new Exception("Failed to Protect the file length");
            Hdr.Unk1DataOffset = (uint)(Hdr.Unk1DataOffset + Diff);
            Hdr.UnkOff = (uint)(Hdr.UnkOff + Diff);
            Hdr.ScriptSize = (uint)(Prefix.Length + Sufix.Length + StringTable.Length);
            Tools.BuildStruct(ref Hdr, true, Encoding.UTF8).CopyTo(Prefix, 0);

            byte[] OutScript = new byte[0];
            Append(ref OutScript, Prefix);
            Append(ref OutScript, StringTable);
            Append(ref OutScript, Sufix);
            BitConverter.GetBytes(Tools.Reverse(CRC(OutScript))).CopyTo(OutScript, 0x8);

            return OutScript;
        }
        public int CalculateLength(string[] Strings) {
            int result = Strings.Length * 3;
            foreach (string str in Strings)
                result += Encoding.UTF8.GetByteCount(str);
            return result;
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
        internal uint ScriptSize;

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
