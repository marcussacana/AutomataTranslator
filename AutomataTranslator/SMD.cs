using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomataTranslator
{
    public class SMDManager
    {
        byte[] Script;
        long[] StrEntries;
        public SMDManager(byte[] Script) {
            this.Script = Script;
        }

        public string[] Import() {
            uint StrCount = GetDW(Script, 0);
            StrEntries = new long[StrCount];
            string[] Strs = new string[StrCount];
            long Pos = 4;
            uint StrId = 0;
            while (Pos < Script.Length) {
                Pos += 0x88;
                StrEntries[StrId] = Pos;
                byte[] StrBuffer = new byte[0x800];
                Copy(Script, Pos, ref StrBuffer, 0, 0x800);
                Strs[StrId++] = Encoding.Unicode.GetString(StrBuffer).Replace("\x0", "");
                Pos += 0x800;
            }

            return Strs;
        }

        public byte[] Export(string[] Strs) {
            if (Strs.Length != StrEntries.Length)
                throw new Exception("You can't add/remove string entries");
            for (uint i = 0; i < Strs.Length; i++) {
                byte[] Buffer = new byte[0x800];
                byte[] Str = Encoding.Unicode.GetBytes(Strs[i]);
                if (Str.Length > 0x800)
                    throw new Exception("The biggest allowed length size is 0x800");
                Str.CopyTo(Buffer, 0);
                Copy(Buffer, 0, ref Script, StrEntries[i], 0x800);
            }

            return Script;
        }
        private void Copy(byte[] In, long ReadIndex, ref byte[] Out, long WriteIndex, long Length) {
            for (long i = 0; i < Length; i++)
                Out[i + WriteIndex] = In[ReadIndex + i];
        }

        private uint GetDW(byte[] File, long Pos) {
            byte[] Arr = new byte[] { File[Pos + 0], File[Pos + 1], File[Pos + 2], File[Pos +3]};
            return BitConverter.ToUInt32(Arr, 0);
        }
    }
}
