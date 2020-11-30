using System;
using System.Collections.Generic;
using System.Text;

namespace Hackbot.Util
{
    static class EmojiHelp
    {
        /// <summary>
        /// Получить эмодзи запись от введённого числа
        /// </summary>
        /// <param name="digit">Преобразуемое число</param>
        /// <returns></returns>
        public static string Digit(int digit)
        {
            StringBuilder sb = new StringBuilder();

            foreach(char c in digit.ToString())
                switch (c)
                {
                    case '0':
                        sb.Append("\u0030\u20e3");
                        break;
                    case '1':
                        sb.Append("\u0031\u20e3");
                        break;
                    case '2':
                        sb.Append("\u0032\u20e3");
                        break;
                    case '3':
                        sb.Append("\u0033\u20e3");
                        break;
                    case '4':
                        sb.Append("\u0034\u20e3");
                        break;
                    case '5':
                        sb.Append("\u0035\u20e3");
                        break;
                    case '6':
                        sb.Append("\u0036\u20e3");
                        break;
                    case '7':
                        sb.Append("\u0037\u20e3");
                        break;
                    case '8':
                        sb.Append("\u0038\u20e3");
                        break;
                    case '9':
                        sb.Append("\u0039\u20e3");
                        break;
                }

            return sb.ToString();
        }

        /// <summary>
        /// Number sign + combining enclosing keycap
        /// </summary>
        /// <returns></returns>
        public static string Hash() => "\x23\xE2\x83\xA3";

        /// <summary>
        /// InformationSource
        /// </summary>
        /// <returns></returns>
        public static string InformationSource() => "\xE2\x84\xB9";

        /// <summary>
        /// Traingle pointing right
        /// </summary>
        /// <returns></returns>
        public static string TraingleRight() => "\xE2\x96\xB6";


    }
}
