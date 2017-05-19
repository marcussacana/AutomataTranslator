using LiteMiner.classes;
using System.Collections.Generic;

namespace AutomataTranslator {

    public enum Language {
      Null = -1,  JP = 0, EN = 1, FR = 2, IT = 3, GE = 4 , SP = 5, KO = 6, CH = 7, Other = 9
    }
    public class BinTL {
        MRubyStringEditor Editor;
        Language TargetLang;
        string[] Strs;
        public Dictionary<int, int> IndexMap { private set; get; } = new Dictionary<int, int>();
        public Dictionary<int, Language> LanguageMap { private set; get; } = new Dictionary<int, Language>();

        public List<Language> ReplacesTargets = new List<Language>();


        /// <summary>
        /// Try Detect the string language by the string index
        /// </summary>
        public bool IndexCheck = true;

        /// <summary>
        /// Remove languages in the black list to generate a script more stable (prevent corrupt bytecode offests)
        /// </summary>
        public bool RemoveLangs = true;
        
        /// <summary>
        /// If can't detect a language try use the last to detect next language.
        /// </summary>
        public bool ConfirmLastLang = true;

        /// <summary>
        /// Import include strings string with unknown language.
        /// If you disabe the ImportTargetLangOnly this is option is ignored.
        /// </summary>
        public bool IncludeUnkLanguage = true;

        /// <summary>
        /// Import include only the target language.
        /// </summary>
        public bool ImportTargetLangOnly = true;
        public BinTL(byte[] Script, Language Lang = Language.EN) {
            TargetLang = Lang;
            Editor = new MRubyStringEditor(Script);
        }
        public BinTL(byte[] Script) {
            TargetLang = Language.EN;
            Editor = new MRubyStringEditor(Script);
        }

        public string[] Import() {
            Strs = Editor.Import();
            List<string> Strings = new List<string>();
            for (int i = 0; i < Strs.Length; i++) {
                if (Strs.Length % 8 == 0 && IndexCheck) {
                    LanguageMap[i] = (Language)(i % 8);
                } else {
                    LanguageDetector LD = new LanguageDetector();
                    Language NextLang = Language.Other;
                    if (LanguageMap.ContainsKey(i - 1)) {
                        NextLang = LanguageMap[i - 1] + 1;
                        if (NextLang == Language.Other)
                            NextLang = 0;
                    }
                    switch (LD.Detect(Strs[i])?.ToLower()) {
                        case "en":
                            LanguageMap[i] = Language.EN;
                            break;
                        case "ja":
                            LanguageMap[i] = Language.JP;
                            break;
                        case "fr":
                            LanguageMap[i] = Language.FR;
                            break;
                        case "it":
                            LanguageMap[i] = Language.IT;
                            break;
                        case "de":
                            LanguageMap[i] = Language.GE;
                            break;
                        case "es":
                            LanguageMap[i] = Language.SP;
                            break;
                        case "ko":
                            LanguageMap[i] = Language.KO;
                            break;
                        case "zh":
                            LanguageMap[i] = Language.CH;
                            break;
                        default:
                            LanguageMap[i] = NextLang;
                            break;
                    }
                }
                if (LanguageMap[i] == TargetLang || (LanguageMap[i] == Language.Other && IncludeUnkLanguage) || !ImportTargetLangOnly) {
                    IndexMap[Strings.Count] = i;
                    Strings.Add(Strs[i]);
                }
                if (ConfirmLastLang  && LanguageMap.ContainsKey(i-1) && (LanguageMap[i-1] == Language.Other) && LanguageMap[i] != Language.Other) {
                    Language Curr = LanguageMap[i];
                    Language Last = Curr - 1 == Language.Null ? Language.CH : Curr - 1;
                    LanguageMap[i - 1] = Last;
                }
            }
            return Strings.ToArray();
        }

        public byte[] Export(string[] Strings) {
            if (RemoveLangs && ReplacesTargets.Count == 0) {
                ReplacesTargets.Add(Language.KO);
                ReplacesTargets.Add(Language.CH);
            }
            string[] NewStrs = new string[Strs.Length];
            Strs.CopyTo(NewStrs, 0);
            for (int i = 0; i < Strings.Length; i++)
                NewStrs[IndexMap[i]] = Strings[i];
            for (int i = 0; i < Strs.Length && RemoveLangs; i++)
                if (ReplacesTargets.Contains(LanguageMap[i]) && LanguageMap[i] != TargetLang)
                    NewStrs[i] = " ";
            return Editor.Export(NewStrs);
        }
    }
}
