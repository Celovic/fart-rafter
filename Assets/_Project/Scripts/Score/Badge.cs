using System;
using System.Collections.Generic;

namespace FartRafters
{
    [Serializable]
    public class Badge
    {
        public string title;
        public string description;
        public bool praise;

        public Badge(string title, string description, bool praise)
        {
            this.title = title;
            this.description = description;
            this.praise = praise;
        }
    }

    public static class Badges
    {
        const int MaxPerPlayer = 2;
        const float ThrustFloor = 200f;

        struct Rule
        {
            public Badge badge;
            public Func<PlayerScore, float> value;
            public bool highest;
            public Func<PlayerScore, Context, bool> eligible;
        }

        static readonly Rule[] Rules =
        {
            new Rule { badge = new Badge("Kaptan Kuru Fasulye", "Ekibin en yüksek puanı", true), value = p => p.total, highest = true, eligible = (p, r) => p.total > 0 },
            new Rule { badge = new Badge("Fasulye Profesörü", "Gazını en verimli kullanan", true), value = p => p.efficiency, highest = true, eligible = (p, r) => p.usefulThrust + p.wastedThrust > ThrustFloor },
            new Rule { badge = new Badge("Cankurtaran Simit", "Suya düşenleri geri aldı", true), value = p => p.rescues, highest = true, eligible = (p, r) => p.rescues > 0 },
            new Rule { badge = new Badge("Timsah Fısıldayan", "Timsahlar onu sevdi, kimse yakalanmadı", true), value = p => p.usefulThrust, highest = true, eligible = (p, r) => r.crocodiles && r.catches == 0 },
            new Rule { badge = new Badge("Buz Pateni", "Buzda hiç çarpmadan kaydı", true), value = p => p.usefulThrust, highest = true, eligible = (p, r) => r.ice && r.iceHits == 0 },
            new Rule { badge = new Badge("Pürüzsüz Pastırma", "En az ceza alan", true), value = p => -p.penalties, highest = true, eligible = (p, r) => p.penalties < 3f },
            new Rule { badge = new Badge("Soğan Gücü", "Eklentileri kapan", true), value = p => p.pickups, highest = true, eligible = (p, r) => p.pickups > 0 },
            new Rule { badge = new Badge("Jet Fasulye", "Salı en çok ileri iten", true), value = p => p.usefulThrust, highest = true, eligible = (p, r) => p.usefulThrust > ThrustFloor },
            new Rule { badge = new Badge("Islak Çorap", "Nehri en çok ziyaret eden", false), value = p => p.falls, highest = true, eligible = (p, r) => p.falls > 0 },
            new Rule { badge = new Badge("Timsah Atıştırmalığı", "Timsahın favori menüsü", false), value = p => p.crocodilePenalty, highest = true, eligible = (p, r) => p.crocodilePenalty > 0.5f },
            new Rule { badge = new Badge("Sal Sallayan", "Arkadaşlarını yüzmeye gönderdi", false), value = p => p.causedFalls, highest = true, eligible = (p, r) => p.causedFalls > 0.1f },
            new Rule { badge = new Badge("Kaya Mıknatısı", "Kayalar onu hiç bırakmadı", false), value = p => p.damagePenalty, highest = true, eligible = (p, r) => p.damagePenalty > 1f },
            new Rule { badge = new Badge("Ters Rüzgâr", "Gazı hep yanlış yöne", false), value = p => p.efficiency, highest = false, eligible = (p, r) => p.usefulThrust + p.wastedThrust > ThrustFloor && p.efficiency < 0.6f },
            new Rule { badge = new Badge("Gaz Kaçağı", "En çok boşa osuran", false), value = p => p.wastedThrust, highest = true, eligible = (p, r) => p.wastedThrust > ThrustFloor * 2f },
            new Rule { badge = new Badge("Demir Atan", "Gazı bile kokmadı", false), value = p => -p.gasSeconds, highest = true, eligible = (p, r) => r.players.Count > 1 && p.gasSeconds < r.TeamGas * 0.1f }
        };

        public static void Assign(ScoreReport report, bool crocodiles, int catches, Biome biome = Biome.River, bool ice = false, int iceHits = 0)
        {
            var context = new Context(report, crocodiles, catches) { ice = ice, iceHits = iceHits };
            foreach (Rule rule in Rules)
            {
                float best = rule.highest ? float.MinValue : float.MaxValue;
                foreach (PlayerScore player in report.players)
                {
                    if (!rule.eligible(player, context))
                    {
                        continue;
                    }
                    float value = rule.value(player);
                    best = rule.highest ? Math.Max(best, value) : Math.Min(best, value);
                }
                if (best == float.MinValue || best == float.MaxValue)
                {
                    continue;
                }
                foreach (PlayerScore player in report.players)
                {
                    if (player.badges.Count >= MaxPerPlayer || !rule.eligible(player, context))
                    {
                        continue;
                    }
                    if (Math.Abs(rule.value(player) - best) < 0.0001f)
                    {
                        player.badges.Add(ForBiome(rule.badge, biome));
                    }
                }
            }
        }

        static Badge ForBiome(Badge badge, Biome biome)
        {
            if (biome == Biome.River || biome == Biome.Desert || biome == Biome.Swamp || !badge.title.StartsWith("Timsah"))
            {
                return badge;
            }
            if (biome == Biome.Lava || biome == Biome.Night)
            {
                bool lava = biome == Biome.Lava;
                string name = lava ? "Semender" : "Yayın Balığı";
                string plural = lava ? "Semenderler" : "Yayın balıkları";
                string genitive = lava ? "Semenderin" : "Yayın balığının";
                return new Badge(badge.title.Replace("Timsah", name), badge.description.Replace("Timsahlar", plural).Replace("Timsahın", genitive), badge.praise);
            }
            if (biome == Biome.Fun)
            {
                return badge.title == "Timsah Atıştırmalığı" ? new Badge("Diken Yastığı", "Dikenli silindirlerin favorisi", badge.praise) : new Badge("Diken Fısıldayan", "Silindirler ona hiç dokunmadı", badge.praise);
            }
            bool polar = biome == Biome.Polar;
            string title = badge.title == "Timsah Atıştırmalığı" && !polar ? "Köpek Balığı Yemi" : badge.title.Replace("Timsah", polar ? "Orka" : "Köpek Balığı");
            string description = badge.description.Replace("Timsahlar", polar ? "Orkalar" : "Köpek balıkları").Replace("Timsahın", polar ? "Orkanın" : "Köpek balığının");
            return new Badge(title, description, badge.praise);
        }

        public class Context
        {
            public bool ice;
            public int iceHits;
            public readonly List<PlayerScore> players;
            public readonly bool crocodiles;
            public readonly int catches;
            public readonly float TeamGas;

            public Context(ScoreReport report, bool crocodiles, int catches)
            {
                players = report.players;
                this.crocodiles = crocodiles;
                this.catches = catches;
                foreach (PlayerScore player in report.players)
                {
                    TeamGas += player.gasSeconds;
                }
            }
        }
    }
}
