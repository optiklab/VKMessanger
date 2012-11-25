using System.Collections.Generic;

namespace SlXnaApp1.Infrastructure
{
    /// <summary>
    /// Simple transliteration helper.
    /// </summary>
    public class TransliteHelper
    {
        public TransliteHelper()
        {
            RussianToEnglish.Add('а', "a");
            RussianToEnglish.Add('б', "b");
            RussianToEnglish.Add('в', "v");
            RussianToEnglish.Add('г', "g");
            RussianToEnglish.Add('д', "d");
            RussianToEnglish.Add('е', "e");
            RussianToEnglish.Add('ё', "e");
            RussianToEnglish.Add('ж', "g");//j
            RussianToEnglish.Add('з', "z");
            RussianToEnglish.Add('и', "i");
            RussianToEnglish.Add('к', "k");
            RussianToEnglish.Add('л', "l");
            RussianToEnglish.Add('м', "m");
            RussianToEnglish.Add('н', "n");
            RussianToEnglish.Add('у', "u");
            RussianToEnglish.Add('ф', "f");
            RussianToEnglish.Add('х', "h");
            RussianToEnglish.Add('ц', "c");
            RussianToEnglish.Add('ч', "ch");
            RussianToEnglish.Add('ш', "sh");
            RussianToEnglish.Add('щ', "sch");
            RussianToEnglish.Add('э', "e");
            RussianToEnglish.Add('ю', "u");
            RussianToEnglish.Add('я', "y");

            EnglishToRussian.Add('a', "а");
            EnglishToRussian.Add('b', "б");
            EnglishToRussian.Add('c', "ц");//к
            EnglishToRussian.Add('d', "д");
            EnglishToRussian.Add('e', "е");
            EnglishToRussian.Add('f', "ф");
            EnglishToRussian.Add('g', "г");//ж
            EnglishToRussian.Add('h', "х");
            EnglishToRussian.Add('i', "и");
            EnglishToRussian.Add('j', "ж");
            EnglishToRussian.Add('k', "к");
            EnglishToRussian.Add('l', "л");
            EnglishToRussian.Add('m', "м");
            EnglishToRussian.Add('n', "н");
            EnglishToRussian.Add('o', "о");
            EnglishToRussian.Add('p', "п");
            EnglishToRussian.Add('q', "к");
            EnglishToRussian.Add('r', "р");
            EnglishToRussian.Add('s', "с");
            EnglishToRussian.Add('t', "т");
            EnglishToRussian.Add('u', "у");
            EnglishToRussian.Add('v', "в");
            EnglishToRussian.Add('w', "в");
            EnglishToRussian.Add('x', "кс");
            EnglishToRussian.Add('y', "я");
            EnglishToRussian.Add('z', "з");
        }

        public string ReverseString(string toReverse)
        {
            string result = string.Empty;

            if (toReverse.Length > 0)
            {
                char first = toReverse[0];

                if ((first >= 65 && first <= 90) ||
                    (first >= 97 && first <= 122))
                { // latin letters...

                    foreach (var el in toReverse)
                    {
                        result += EnglishToRussian[el];
                    }
                }
                else
                { // probably russian letter...

                    foreach (var el in toReverse)
                    {
                        result += RussianToEnglish[el];
                    }
                }
            }

            return result;
        }

        private Dictionary<char, string> RussianToEnglish = new Dictionary<char, string>();
        private Dictionary<char, string> EnglishToRussian = new Dictionary<char, string>();
    }
}
