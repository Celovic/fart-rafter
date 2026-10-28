using System.Text;

namespace FartRafters
{
    public static class ProfanityFilter
    {
        public const int MinLength = 3;
        public const int MaxLength = 20;

        static readonly string[] Blocked =
        {
            "amk", "aq", "sik", "sık", "yarak", "yarrak", "taşak", "tasak", "göt", "got", "orospu", "oruspu", "piç", "pic", "kahpe", "pezevenk", "ibne", "gavat", "kaltak", "sürtük", "surtuk",
            "salak", "aptal", "gerizekalı", "gerizekali", "mal", "dangalak", "şerefsiz", "serefsiz", "haysiyetsiz", "puşt", "pust", "yavşak", "yavsak", "kerhane", "meme", "seks", "sex", "porno",
            "fuck", "shit", "bitch", "dick", "cock", "pussy", "nigger", "nigga", "fag", "whore", "slut", "cunt", "bastard", "asshole", "hitler", "nazi", "şişko", "sisko"
        };

        public enum Result
        {
            Ok,
            TooShort,
            TooLong,
            BadCharacters,
            Blocked
        }

        public static Result Check(string name)
        {
            if (name == null)
            {
                return Result.TooShort;
            }
            string trimmed = name.Trim();
            if (trimmed.Length < MinLength)
            {
                return Result.TooShort;
            }
            if (trimmed.Length > MaxLength)
            {
                return Result.TooLong;
            }
            foreach (char c in trimmed)
            {
                if (!char.IsLetterOrDigit(c) && c != ' ')
                {
                    return Result.BadCharacters;
                }
            }
            if (ContainsBlocked(trimmed))
            {
                return Result.Blocked;
            }
            return Result.Ok;
        }

        public static bool IsClean(string name)
        {
            return string.IsNullOrEmpty(name) || !ContainsBlocked(name.Trim());
        }

        static bool ContainsBlocked(string trimmed)
        {
            string normalized = Normalize(trimmed);
            foreach (string word in Blocked)
            {
                string bad = Normalize(word);
                if (bad.Length <= 4)
                {
                    foreach (string part in normalized.Split(' '))
                    {
                        if (part == bad)
                        {
                            return true;
                        }
                    }
                }
                else if (normalized.Replace(" ", "").Contains(bad))
                {
                    return true;
                }
            }
            return false;
        }

        public static string Message(Result result)
        {
            switch (result)
            {
                case Result.TooShort:
                    return "İsim en az " + MinLength + " harf olmalı.";
                case Result.TooLong:
                    return "İsim en fazla " + MaxLength + " harf olabilir.";
                case Result.BadCharacters:
                    return "Yalnızca harf, rakam ve boşluk kullan.";
                case Result.Blocked:
                    return "Bu isim uygun değil, başka bir isim dene.";
                default:
                    return "";
            }
        }

        static string Normalize(string text)
        {
            var builder = new StringBuilder();
            foreach (char raw in text.ToLowerInvariant())
            {
                char c = raw;
                switch (c)
                {
                    case 'ı':
                    case 'i':
                    case '1':
                    case '!':
                        c = 'i';
                        break;
                    case 'ş':
                        c = 's';
                        break;
                    case 'ğ':
                        c = 'g';
                        break;
                    case 'ü':
                        c = 'u';
                        break;
                    case 'ö':
                    case '0':
                        c = 'o';
                        break;
                    case 'ç':
                        c = 'c';
                        break;
                    case '3':
                        c = 'e';
                        break;
                    case '4':
                        c = 'a';
                        break;
                    case '5':
                        c = 's';
                        break;
                    case '7':
                        c = 't';
                        break;
                }
                if (builder.Length > 0 && builder[builder.Length - 1] == c && c != ' ')
                {
                    continue;
                }
                builder.Append(c);
            }
            return builder.ToString();
        }
    }
}
