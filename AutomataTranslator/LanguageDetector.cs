/*
 * Based on the idea and JavaScript code developed by Rich Tibbett (https://github.com/richtr/guessLanguage.js)
 * 
 * C# version by Sasvári Tamás 2013.01.30
 *  
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Text.RegularExpressions;

namespace LiteMiner.classes
{
    public class TrigramModel
    {
        Trigram[] trigrams;
        public Trigram[] Trigrams
        {
            get { return trigrams; }
        }

        public TrigramModel(Hashtable trigramsAndCounts)
        {
            List<string> keys2 = new List<string>();
            List<int> scores2 = new List<int>();
            //convert hashtable to arrays
            foreach (string key in trigramsAndCounts.Keys)
            {
                keys2.Add(key);
                scores2.Add((int)trigramsAndCounts[key]);
            }

            string[] keys = keys2.ToArray();
            int[] scores = scores2.ToArray();

            // sort array results
            Array.Sort(scores, keys);
            Array.Reverse(keys);
            Array.Reverse(scores);

            //build final array
            List<Trigram> result = new List<Trigram>();
            for (int x = 0; x < keys.Length; x++)
            {
                result.Add(new Trigram(keys[x], scores[x]));
            }

            trigrams = result.ToArray();

        }

        public TrigramModel(string[] tgrams)
        {
            List<string> keys2 = new List<string>();
            List<int> scores2 = new List<int>();
            //convert hashtable to arrays
            int score = 0;
            foreach (string key in tgrams)
            {
                keys2.Add(key);
                scores2.Add(score++);
            }

            string[] keys = keys2.ToArray();
            int[] scores = scores2.ToArray();

            // sort array results
            Array.Sort(scores, keys);
            Array.Reverse(keys);
            Array.Reverse(scores);

            //build final array
            List<Trigram> result = new List<Trigram>();
            for (int x = 0; x < keys.Length; x++)
            {
                result.Add(new Trigram(keys[x], scores[x]));
            }

            trigrams = result.ToArray();

        }

        public bool HasTrigram(string trigram)
        {
            foreach (Trigram t in trigrams)
            {
                if (t.t == trigram) return true;
            }
            return false;
        }

        public int GetScore(string trigram)
        {
            foreach (Trigram t in trigrams)
            {
                if (t.t == trigram) return t.score;
            }
            throw new Exception("No score found for '" + trigram + "'");
        }
    }

    public class Trigram
    {
        public string t = null;
        public int score = 0;
        public Trigram(string t, int s)
        {
            this.t = t;
            score = s;
        }
    }

    public class LanguageDetector
    {
        public const int MAX_LENGTH = 4096;
        public const int MIN_LENGTH = 20;
        public const int MAX_GRAMS = 300;

        LanguageStatistics langStat = new LanguageStatistics();
        Hashtable NAME_MAP = new Hashtable() {
        {"ab", "Abkhazian"},
        {"af", "Afrikaans"},
        {"ar", "Arabic"},
        {"az", "Azeri"},
        {"be", "Belarusian"},
        {"bg", "Bulgarian"},
        {"bn", "Bengali"},
        {"bo", "Tibetan"},
        {"br", "Breton"},
        {"ca", "Catalan"},
        {"ceb", "Cebuano"},
        {"cs", "Czech"},
        {"cy", "Welsh"},
        {"da", "Danish"},
        {"de", "German"},
        {"el", "Greek"},
        {"en", "English"},
        {"eo", "Esperanto"},
        {"es", "Spanish"},
        {"et", "Estonian"},
        {"eu", "Basque"},
        {"fa", "Farsi"},
        {"fi", "Finnish"},
        {"fo", "Faroese"},
        {"fr", "French"},
        {"fy", "Frisian"},
        {"gd", "Scots Gaelic"},
        {"gl", "Galician"},
        {"gu", "Gujarati"},
        {"ha", "Hausa"},
        {"haw", "Hawaiian"},
        {"he", "Hebrew"},
        {"hi", "Hindi"},
        {"hr", "Croatian"},
        {"hu", "Hungarian"},
        {"hy", "Armenian"},
        {"id", "Indonesian"},
        {"is", "Icelandic"},
        {"it", "Italian"},
        {"ja", "Japanese"},
        {"ka", "Georgian"},
        {"kk", "Kazakh"},
        {"km", "Cambodian"},
        {"ko", "Korean"},
        {"ku", "Kurdish"},
        {"ky", "Kyrgyz"},
        {"la", "Latin"},
        {"lt", "Lithuanian"},
        {"lv", "Latvian"},
        {"mg", "Malagasy"},
        {"mk", "Macedonian"},
        {"ml", "Malayalam"},
        {"mn", "Mongolian"},
        {"mr", "Marathi"},
        {"ms", "Malay"},
        {"nd", "Ndebele"},
        {"ne", "Nepali"},
        {"nl", "Dutch"},
        {"nn", "Nynorsk"},
        {"no", "Norwegian"},
        {"nso", "Sepedi"},
        {"pa", "Punjabi"},
        {"pl", "Polish"},
        {"ps", "Pashto"},
        {"pt", "Portuguese"},
        {"pt_PT", "Portuguese (Portugal)"},
        {"pt_BR", "Portuguese (Brazil)"},
        {"ro", "Romanian"},
        {"ru", "Russian"},
        {"sa", "Sanskrit"},
        {"sh", "Serbo-Croatian"},
        {"sk", "Slovak"},
        {"sl", "Slovene"},
        {"so", "Somali"},
        {"sq", "Albanian"},
        {"sr", "Serbian"},
        {"sv", "Swedish"},
        {"sw", "Swahili"},
        {"ta", "Tamil"},
        {"te", "Telugu"},
        {"th", "Thai"},
        {"tl", "Tagalog"},
        {"tlh", "Klingon"},
        {"tn", "Setswana"},
        {"tr", "Turkish"},
        {"ts", "Tsonga"},
        {"tw", "Twi"},
        {"uk", "Ukrainian"},
        {"ur", "Urdu"},
        {"uz", "Uzbek"},
        {"ve", "Venda"},
        {"vi", "Vietnamese"},
        {"xh", "Xhosa"},
        {"zh", "Chinese"},
        {"zh_TW", "Traditional Chinese (Taiwan)"},
        {"zu", "Zulu"}
        };
        
        string[] SINGLETONS = {
        "Armenian", "hy",
        "Hebrew", "he",
        "Bengali", "bn",
        "Gurmukhi", "pa",
        "Greek", "el",
        "Gujarati", "gu",
        "Oriya", "or",
        "Tamil", "ta",
        "Telugu", "te",
        "Kannada", "kn",
        "Malayalam", "ml",
        "Sinhala", "si",
        "Thai", "th",
        "Lao", "lo",
        "Tibetan", "bo",
        "Burmese", "my",
        "Georgian", "ka",
        "Mongolian", "mn",
        "Khmer", "km"
        };
        
        string[] BASIC_LATIN = {"en", "ceb", "ha", "so", "tlh", "id", "haw", "la", "sw", "eu", "nr", "nso", "zu", "xh", "ss", "st", "tn", "ts"};
        string[] EXTENDED_LATIN = {"cs", "af", "pl", "hr", "ro", "sk", "sl", "tr", "hu", "az", "et", "sq", "ca", "es", "fr", "de", "nl", "it", "da", "is", "no", "sv", "fi", "lv", "pt", "ve", "lt", "tl", "cy", "vi"};
        string[] ALL_LATIN;
        string[] CYRILLIC = {"ru", "uk", "kk", "uz", "mn", "sr", "mk", "bg", "ky"};
        string[] ARABIC = {"ar", "fa", "ps", "ur"};
        string[] DEVANAGARI = {"hi", "ne"};
        string[] PT = {"pt_BR", "pt_PT"};
        Hashtable RegexCache = new Hashtable();

        // Unicode char greedy regex block range matchers
        string[] unicodeBlockTests = {
        "Basic Latin", "[\u0000-\u007F]",
        "Latin-1 Supplement", "[\u0080-\u00FF]",
        "Latin Extended-A", "[\u0100-\u017F]",
        "Latin Extended-B", "[\u0180-\u024F]",
        "IPA Extensions", "[\u0250-\u02AF]",
        "Spacing Modifier Letters", "[\u02B0-\u02FF]",
        "Combining Diacritical Marks", "[\u0300-\u036F]",
        "Greek and Coptic", "[\u0370-\u03FF]",
        "Cyrillic", "[\u0400-\u04FF]",
        "Cyrillic Supplement", "[\u0500-\u052F]",
        "Armenian", "[\u0530-\u058F]",
        "Hebrew", "[\u0590-\u05FF]",
        "Arabic", "[\u0600-\u06FF]",
        "Syriac", "[\u0700-\u074F]",
        "Arabic Supplement", "[\u0750-\u077F]",
        "Thaana", "[\u0780-\u07BF]",
        "NKo", "[\u07C0-\u07FF]",
        "Devanagari", "[\u0900-\u097F]",
        "Bengali", "[\u0980-\u09FF]",
        "Gurmukhi", "[\u0A00-\u0A7F]",
        "Gujarati", "[\u0A80-\u0AFF]",
        "Oriya", "[\u0B00-\u0B7F]",
        "Tamil", "[\u0B80-\u0BFF]",
        "Telugu", "[\u0C00-\u0C7F]",
        "Kannada", "[\u0C80-\u0CFF]",
        "Malayalam", "[\u0D00-\u0D7F]",
        "Sinhala", "[\u0D80-\u0DFF]",
        "Thai", "[\u0E00-\u0E7F]",
        "Lao", "[\u0E80-\u0EFF]",
        "Tibetan", "[\u0F00-\u0FFF]",
        "Myanmar", "[\u1000-\u109F]",
        "Georgian", "[\u10A0-\u10FF]",
        "Hangul Jamo", "[\u1100-\u11FF]",
        "Ethiopic", "[\u1200-\u137F]",
        "Ethiopic Supplement", "[\u1380-\u139F]",
        "Cherokee", "[\u13A0-\u13FF]",
        "Unified Canadian Aboriginal Syllabics", "[\u1400-\u167F]",
        "Ogham", "[\u1680-\u169F]",
        "Runic", "[\u16A0-\u16FF]",
        "Tagalog", "[\u1700-\u171F]",
        "Hanunoo", "[\u1720-\u173F]",
        "Buhid", "[\u1740-\u175F]",
        "Tagbanwa", "[\u1760-\u177F]",
        "Khmer", "[\u1780-\u17FF]",
        "Mongolian", "[\u1800-\u18AF]",
        "Limbu", "[\u1900-\u194F]",
        "Tai Le", "[\u1950-\u197F]",
        "New Tai Lue", "[\u1980-\u19DF]",
        "Khmer Symbols", "[\u19E0-\u19FF]",
        "Buginese", "[\u1A00-\u1A1F]",
        "Balinese", "[\u1B00-\u1B7F]",
        "Phonetic Extensions", "[\u1D00-\u1D7F]",
        "Phonetic Extensions Supplement", "[\u1D80-\u1DBF]",
        "Combining Diacritical Marks Supplement", "[\u1DC0-\u1DFF]",
        "Latin Extended Additional", "[\u1E00-\u1EFF]",
        "Greek Extended", "[\u1F00-\u1FFF]",
        "General Punctuation", "[\u2000-\u206F]",
        "Superscripts and Subscripts", "[\u2070-\u209F]",
        "Currency Symbols", "[\u20A0-\u20CF]",
        "Combining Diacritical Marks for Symbols", "[\u20D0-\u20FF]",
        "Letterlike Symbols", "[\u2100-\u214F]",
        "Number Forms", "[\u2150-\u218F]",
        "Arrows", "[\u2190-\u21FF]",
        "Mathematical Operators", "[\u2200-\u22FF]",
        "Miscellaneous Technical", "[\u2300-\u23FF]",
        "Control Pictures", "[\u2400-\u243F]",
        "Optical Character Recognition", "[\u2440-\u245F]",
        "Enclosed Alphanumerics", "[\u2460-\u24FF]",
        "Box Drawing", "[\u2500-\u257F]",
        "Block Elements", "[\u2580-\u259F]",
        "Geometric Shapes", "[\u25A0-\u25FF]",
        "Miscellaneous Symbols", "[\u2600-\u26FF]",
        "Dingbats", "[\u2700-\u27BF]",
        "Miscellaneous Mathematical Symbols-A", "[\u27C0-\u27EF]",
        "Supplemental Arrows-A", "[\u27F0-\u27FF]",
        "Braille Patterns", "[\u2800-\u28FF]",
        "Supplemental Arrows-B", "[\u2900-\u297F]",
        "Miscellaneous Mathematical Symbols-B", "[\u2980-\u29FF]",
        "Supplemental Mathematical Operators", "[\u2A00-\u2AFF]",
        "Miscellaneous Symbols and Arrows", "[\u2B00-\u2BFF]",
        "Glagolitic", "[\u2C00-\u2C5F]",
        "Latin Extended-C", "[\u2C60-\u2C7F]",
        "Coptic", "[\u2C80-\u2CFF]",
        "Georgian Supplement", "[\u2D00-\u2D2F]",
        "Tifinagh", "[\u2D30-\u2D7F]",
        "Ethiopic Extended", "[\u2D80-\u2DDF]",
        "Supplemental Punctuation", "[\u2E00-\u2E7F]",
        "CJK Radicals Supplement", "[\u2E80-\u2EFF]",
        "KangXi Radicals", "[\u2F00-\u2FDF]",
        "Ideographic Description Characters", "[\u2FF0-\u2FFF]",
        "CJK Symbols and Punctuation", "[\u3000-\u303F]",
        "Hiragana", "[\u3040-\u309F]",
        "Katakana", "[\u30A0-\u30FF]",
        "Bopomofo", "[\u3100-\u312F]",
        "Hangul Compatibility Jamo", "[\u3130-\u318F]",
        "Kanbun", "[\u3190-\u319F]",
        "Bopomofo Extended", "[\u31A0-\u31BF]",
        "CJK Strokes", "[\u31C0-\u31EF]",
        "Katakana Phonetic Extensions", "[\u31F0-\u31FF]",
        "Enclosed CJK Letters and Months", "[\u3200-\u32FF]",
        "CJK Compatibility", "[\u3300-\u33FF]",
        "CJK Unified Ideographs Extension A", "[\u3400-\u4DBF]",
        "Yijing Hexagram Symbols", "[\u4DC0-\u4DFF]",
        "CJK Unified Ideographs", "[\u4E00-\u9FFF]",
        "Yi Syllables", "[\uA000-\uA48F]",
        "Yi Radicals", "[\uA490-\uA4CF]",
        "Modifier Tone Letters", "[\uA700-\uA71F]",
        "Latin Extended-D", "[\uA720-\uA7FF]",
        "Syloti Nagri", "[\uA800-\uA82F]",
        "Phags-pa", "[\uA840-\uA87F]",
        "Hangul Syllables", "[\uAC00-\uD7AF]",
        "High Surrogates", "[\uD800-\uDB7F]",
        "High Private Use Surrogates", "[\uDB80-\uDBFF]",
        "Low Surrogates", "[\uDC00-\uDFFF]",
        "Private Use Area", "[\uE000-\uF8FF]",
        "CJK Compatibility Ideographs", "[\uF900-\uFAFF]",
        "Alphabetic Presentation Forms", "[\uFB00-\uFB4F]",
        "Arabic Presentation Forms-A", "[\uFB50-\uFDFF]",
        "Variation Selectors", "[\uFE00-\uFE0F]",
        "Vertical Forms", "[\uFE10-\uFE1F]",
        "Combining Half Marks", "[\uFE20-\uFE2F]",
        "CJK Compatibility Forms", "[\uFE30-\uFE4F]",
        "Small Form Variants", "[\uFE50-\uFE6F]",
        "Arabic Presentation Forms-B", "[\uFE70-\uFEFF]",
        "Halfwidth and Fullwidth Forms", "[\uFF00-\uFFEF]",
        "Specials", "[\uFFF0-\uFFFF]"};

        public LanguageDetector()
        {
            List<string> temp = new List<string>(BASIC_LATIN);
            temp.AddRange(EXTENDED_LATIN);
            ALL_LATIN = temp.ToArray();
        }

        private Hashtable FindRuns(string text)
        {

            Hashtable relevant_runs = new Hashtable();

            for (int x = 0; x < unicodeBlockTests.Length; x+=2)
            {
                string name = unicodeBlockTests[x];
                string regex = unicodeBlockTests[x + 1];
                if (RegexCache[name] == null) RegexCache[name] = new Regex(regex);
                // Count the number of characters in each character block.
                int charCount = ((Regex)RegexCache[name]).Matches(text).Count;

                // return run types that used for 40% or more of the string
                // always return basic latin if found more than 15%
                // and extended additional latin if over 10% (for Vietnamese)
                double pct = (double)charCount / (double)text.Length;

                relevant_runs[name] = pct;
           
            }

            return relevant_runs;
        }

        public string GetLanguageNameByCode(string code)
        {
            if (string.IsNullOrEmpty(code)) return null;
            code = code.ToLower();
            if (NAME_MAP[code] == null) return null;
            return NAME_MAP[code] as string;
        }

        public string Detect(string text)
        {
      
            if (string.IsNullOrEmpty(text)) return null;

            if (text.Length > MAX_LENGTH) text = text.Substring(0,MAX_LENGTH);
            text = Regex.Replace(text, "[\u0021-\u0040„”|\\-]", "");//remove numbers and punctuations
            text = text.Replace("\t", " ").Replace("\n", " ").Replace("\r", " ");
            text = text.Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ");//normalize multiple spaces
            return Identify(text.Trim());
        }

        private string Identify(string text)
        {

            Hashtable scripts = FindRuns(text);

            // Identify the language using most significant character usage.
            if ((double)scripts["Hangul Syllables"] + (double)scripts["Hangul Jamo"] + (double)scripts["Hangul Compatibility Jamo"] >= 0.4) {
                return "ko";
            }

            if ((double)scripts["Greek and Coptic"] >= 0.4) {
                return "el";
            }

            if ((double)scripts["Hiragana"] + (double)scripts["Katakana"] + (double)scripts["Katakana Phonetic Extensions"] >= 0.2) {
                return "ja";
            }

            if ((double)scripts["CJK Unified Ideographs"] + (double)scripts["Bopomofo"] + (double)scripts["Bopomofo Extended"] + (double)scripts["KangXi Radicals"] >= 0.4) {
                return "zh";
            }

            if ((double)scripts["Cyrillic"] >= 0.4) {
                return Check(text, CYRILLIC);//decide language using cyrillic letters
            }

            if ((double)scripts["Arabic"] + (double)scripts["Arabic Presentation Forms-A"] + (double)scripts["Arabic Presentation Forms-B"] >= 0.4) {
                return Check(text, ARABIC); //decide language using arabic letters
            }

            if ((double)scripts["Devanagari"] >= 0.4) {
                return Check(text, DEVANAGARI);
            }

            // Try languages with unique scripts
            for (int x = 0; x < SINGLETONS.Length; x+=2)
            {
                string name = SINGLETONS[x];
                string code = SINGLETONS[x + 1];
                if (scripts[name] != null)
                {
                    if ((double)scripts[name] >= 0.4)
                    {
                        return code;
                    }
                }
            }

            // Extended Latin
            if ((double)scripts["Latin-1 Supplement"] + (double)scripts["Latin Extended-A"] + (double)scripts["IPA Extensions"] >= 0.4)
            {
                string latin_lang = Check(text, EXTENDED_LATIN);
                if (latin_lang == "pt")
                {
                    return Check(text, PT);
                } else {
                    return latin_lang;
                }
            }

            if ((double)scripts["Basic Latin"] >= 0.15) {
                return Check(text, ALL_LATIN);
            }

            return null; //give up, no match
        }

        private string Check(string sample, string[] langs)
        {

            if (sample.Length < MIN_LENGTH) {
                return null;
            }

            Hashtable scores = new Hashtable();
            TrigramModel model = CreateOrderedModel(sample);
            int lowestScore = Int32.MaxValue;
            string lowestScoreLanCode = null;
            for (int i = 0; i < langs.Length; i++)
            {

                string lkey = langs[i].ToLower();

                if (langStat.Models[lkey] == null) continue;//next please, no known model for this
                TrigramModel known_model = (TrigramModel)langStat.Models[lkey];

                int dist = Distance(model, known_model);
                scores[lkey] = dist;
                if (dist< lowestScore)
                {
                    lowestScore = dist;
                    lowestScoreLanCode = lkey;
                }

            }

            return lowestScoreLanCode;
        }

        private TrigramModel CreateOrderedModel(string content)
        {
            // Create a list of trigrams in content sorted by frequency.
            Hashtable trigrams = new Hashtable();
            content = content.ToLower();

            for (int i = 0; i <content.Length - 2; i++)
            {
                string trigramKey = "" + content[i] + content[i + 1] + content[i + 2];
                if (trigrams[trigramKey] == null) {
                    trigrams[trigramKey] = 1;
                } else {
                    trigrams[trigramKey] = ((int)trigrams[trigramKey]) + 1;
                }
            }

            return new TrigramModel(trigrams);
        }

        private int Distance(TrigramModel model, TrigramModel known_model)
        {
            // Calculate the distance to the known model.
            int dist = 0;

            for (int i = 0; i < model.Trigrams.Length; i++)
            {
                if (known_model.HasTrigram(model.Trigrams[i].t))
                {
                    dist += Math.Abs(model.Trigrams[i].score - known_model.GetScore(model.Trigrams[i].t));
                } else {
                    dist += MAX_GRAMS;
                }
            }

            return dist;
        }

    }
}
