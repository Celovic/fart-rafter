using System;
using System.Text;

namespace FartRafters
{
    public static class NameGenerator
    {
        static readonly string[] Adjectives =
        {
            "Neşeli", "Pofuduk", "Zıp Zıp", "Minik", "Cesur", "Uykucu", "Hızlı", "Gülen", "Şakacı", "Meraklı", "Islak", "Pırıl Pırıl", "Sevimli", "Çılgın", "Sakin", "Dalgın"
        };

        static readonly string[] Nouns =
        {
            "Su Aygırı", "Ayı", "Kurbağa", "Nohut", "Ördek", "Zürafa", "Kaplumbağa", "Balık", "Fasulye", "Simit", "Soğan", "Timsah", "Kuzu", "Sal", "Kelebek", "Tembel Hayvan"
        };

        static readonly string[] Funny =
        {
            "Fasulye Fırtınası", "Gaz Kaptanı", "Pastırma Pilotu", "Soğan Sultanı", "Nohut Ninja", "Timsah Kaçkını", "Kokulu Kaptan", "Turbo Fasulye", "Acı Biber Roketi", "Sal Sallayan Sultan",
            "Gaz Pedalı", "Kuru Fasulye Kralı", "Pastırma Paşa", "Osuruk Ustası", "Dümen Fasulye", "Rüzgâr Gülü", "Nehir Roketi", "Gazlı Kaptan", "Kürek Çekmeyen", "Fıs Fıs Kaptan"
        };

        public static string ChildName(Random random)
        {
            return Adjectives[random.Next(Adjectives.Length)] + " " + Nouns[random.Next(Nouns.Length)] + " " + random.Next(10, 1000);
        }

        public static string FunnyName(Random random)
        {
            return Funny[random.Next(Funny.Length)];
        }

        public static bool IsGenerated(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return false;
            }
            foreach (string adjective in Adjectives)
            {
                if (!name.StartsWith(adjective + " "))
                {
                    continue;
                }
                foreach (string noun in Nouns)
                {
                    string prefix = adjective + " " + noun + " ";
                    if (name.StartsWith(prefix) && int.TryParse(name.Substring(prefix.Length), out _))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static string ToServiceName(string display)
        {
            var builder = new StringBuilder();
            foreach (char c in display.Trim())
            {
                if (char.IsLetterOrDigit(c))
                {
                    builder.Append(c);
                }
                else if ((c == ' ' || c == '_') && builder.Length > 0 && builder[builder.Length - 1] != '_')
                {
                    builder.Append('_');
                }
            }
            string result = builder.ToString().Trim('_');
            return result.Length > 40 ? result.Substring(0, 40) : result;
        }

        public static string FromServiceName(string service)
        {
            if (string.IsNullOrEmpty(service))
            {
                return "";
            }
            int hash = service.LastIndexOf('#');
            string body = hash > 0 ? service.Substring(0, hash) : service;
            return body.Replace('_', ' ');
        }
    }
}
