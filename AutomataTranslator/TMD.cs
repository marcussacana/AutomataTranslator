using AdvancedBinary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AutomataTranslator {
    public class TMDEditor {
        Dictionary<uint, bool> EscapeMap;
        byte[] Script;
        public TMDEditor(byte[] Script) {
            this.Script = Script;
        }

        public string[] Import() {
            EscapeMap = new Dictionary<uint, bool>();
            StructReader Reader = new StructReader(new MemoryStream(Script), false, Encoding.Unicode);
            uint Count = Reader.ReadUInt32() * 2;
            string[] Strings = new string[Count];
            for (uint i = 0; i < Count; i++) {
                TMDEntry Entry = new TMDEntry();
                Reader.ReadStruct(ref Entry);
                EscapeMap[i] = Entry.String.EndsWith("\0");
                if (EscapeMap[i]) 
                    Entry.String = Entry.String.Substring(0, Entry.String.Length);
                Strings[i] = Entry.String;
            }
            Reader.Close();
            return Strings;
        }

        public byte[] Export(string[] Strs) {
            MemoryStream Out = new MemoryStream();
            StructWriter Writer = new StructWriter(Out, false, Encoding.Unicode);
            Writer.Write((uint)Strs.LongLength/2);
            for (uint i = 0; i < Strs.Length; i++) {
                string Str = Strs[i];
                if (EscapeMap[i])
                    Str += '\0';
                TMDEntry Entry = new TMDEntry() {
                    String = Str
                };
                Writer.WriteStruct(ref Entry);
            }
            Out.Position = 0;
            byte[] Output = Out.ToArray();
            Writer.Close();
            return Output;
        }


        private struct TMDEntry {
            [PString(PrefixType = Const.UINT32, UnicodeLength = true)]
            public string String;
        }

    }
}
