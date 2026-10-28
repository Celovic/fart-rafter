using UnityEngine;

namespace FartRafters
{
    public class QuestTracker : MonoBehaviour
    {
        GameBootstrap game;
        RaftHealth health;
        RaftBoosts boosts;
        bool broken;
        float fartTime;
        int raftIndex;

        public void Setup(GameBootstrap bootstrap, int raft)
        {
            game = bootstrap;
            raftIndex = raft;
        }

        void Start()
        {
            if (game == null)
            {
                return;
            }
            if (game.course != null)
            {
                game.course.CheckpointReached += OnCheckpoint;
                game.course.RaceFinished += OnFinished;
            }
            if (game.raft != null)
            {
                health = game.raft.GetComponent<RaftHealth>();
                boosts = game.raft.Boosts;
            }
            if (health != null)
            {
                health.Broken += OnBroken;
            }
            if (boosts != null)
            {
                boosts.Collected += OnCollected;
            }
        }

        void OnDestroy()
        {
            FlushFart();
            if (game != null && game.course != null)
            {
                game.course.CheckpointReached -= OnCheckpoint;
                game.course.RaceFinished -= OnFinished;
            }
            if (health != null)
            {
                health.Broken -= OnBroken;
            }
            if (boosts != null)
            {
                boosts.Collected -= OnCollected;
            }
        }

        void Update()
        {
            if (game == null || game.PlayerRiders == null || game.course == null || !game.course.Running || game.course.Finished)
            {
                return;
            }
            foreach (Rider rider in game.PlayerRiders)
            {
                if (rider != null && rider.IsFiring && !rider.Autopilot)
                {
                    fartTime += Time.deltaTime;
                    break;
                }
            }
            if (fartTime >= 5f)
            {
                FlushFart();
            }
        }

        void FlushFart()
        {
            int seconds = Mathf.FloorToInt(fartTime);
            if (seconds > 0)
            {
                fartTime -= seconds;
                DailyQuests.Fart(seconds);
            }
        }

        void OnCheckpoint(Checkpoint checkpoint)
        {
            DailyQuests.Checkpoint();
        }

        void OnCollected(PickupKind kind)
        {
            DailyQuests.Pickup();
        }

        void OnBroken()
        {
            broken = true;
        }

        void OnFinished()
        {
            FlushFart();
            Rider main = game.PlayerRiders != null && game.PlayerRiders.Length > 0 ? game.PlayerRiders[0] : null;
            DailyQuests.Run(new RunSummary
            {
                finished = true,
                biome = game.Biome,
                adventure = game.IsAdventure,
                difficulty = game.IsAdventure ? game.DifficultyIndex : 0,
                broken = broken,
                costume = main != null ? main.Costume : 0,
                raft = raftIndex
            });
        }
    }
}
