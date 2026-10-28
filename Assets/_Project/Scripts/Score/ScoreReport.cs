using System;
using System.Collections.Generic;

namespace FartRafters
{
    [Serializable]
    public class PlayerScore
    {
        public string name;
        public bool bot;
        public bool main;
        public int seat;
        public float time;
        public float safety;
        public float gas;
        public float bonus;
        public float penalties;
        public float efficiency;
        public float usefulThrust;
        public float wastedThrust;
        public float gasSeconds;
        public float damagePenalty;
        public float crocodilePenalty;
        public float causedFalls;
        public int falls;
        public int rescues;
        public int pickups;
        public int total;
        public List<Badge> badges = new List<Badge>();

        public float Base => time + safety + gas + bonus;
    }

    [Serializable]
    public class ScoreReport
    {
        public string levelId;
        public string levelName;
        public bool adventure;
        public int difficultyIndex;
        public float elapsed;
        public float par;
        public int respawns;
        public int humans;
        public int bots;
        public float difficultyFactor;
        public float crewFactor;
        public float botFactor;
        public float repeatFactor = 1f;
        public List<PlayerScore> players = new List<PlayerScore>();

        public PlayerScore Main
        {
            get
            {
                foreach (PlayerScore player in players)
                {
                    if (player.main)
                    {
                        return player;
                    }
                }
                return players.Count > 0 ? players[0] : null;
            }
        }
    }
}
