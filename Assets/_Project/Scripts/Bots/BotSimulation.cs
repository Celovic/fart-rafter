using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FartRafters
{
    public class BotSimulation : MonoBehaviour
    {
        public static int? SeedOverride { get; private set; }
        static bool requested;

        public static bool Running => requested;
        static int botLevel = 3;
        static int difficulty = 3;
        static int botCount = 3;
        static int biomeChoice;
        static int raftChoice;
        static float timeLimit = 900f;
        static float speedUp = 4f;

        CourseManager course;
        RaftHealth health;
        float damage;
        int caught;
        int broken;
        int hits;
        int falls;
        int eliminated;
        float nextReport;
        float nextTrace;
        readonly System.Collections.Generic.Dictionary<Rider, float> firingTime = new System.Collections.Generic.Dictionary<Rider, float>();
        bool done;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            string[] args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "-botsim":
                        requested = true;
                        break;
                    case "-botlevel":
                        botLevel = int.Parse(args[i + 1]);
                        break;
                    case "-simdifficulty":
                        difficulty = int.Parse(args[i + 1]);
                        break;
                    case "-simbots":
                        botCount = int.Parse(args[i + 1]);
                        break;
                    case "-simseed":
                        SeedOverride = int.Parse(args[i + 1]);
                        break;
                    case "-simraft":
                        raftChoice = int.Parse(args[i + 1]);
                        break;
                    case "-simbiome":
                        biomeChoice = int.Parse(args[i + 1]);
                        break;
                    case "-noexpert":
                        BotCrew.ExpertDisabled = true;
                        break;
                    case "-simlimit":
                        timeLimit = float.Parse(args[i + 1]);
                        break;
                }
            }
            if (!requested)
            {
                return;
            }
            AudioListener.volume = 0f;
            AudioListener.pause = true;
            GameSelection.CurrentMode = GameSelection.Mode.Adventure;
            GameSelection.DifficultyIndex = difficulty;
            GameSelection.PlayerCount = 1;
            GameSelection.BotCount = botCount;
            GameSelection.BotLevelIndex = botLevel;
            GameSelection.BiomeChoice = biomeChoice;
            GameSelection.RaftIndex = raftChoice;
            SceneManager.sceneLoaded += OnLoaded;
            if (SceneManager.GetActiveScene().name != GameSelection.GameScene)
            {
                SceneManager.LoadScene(GameSelection.GameScene);
            }
            else
            {
                OnLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
            }
        }

        static void OnLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == GameSelection.GameScene && FindFirstObjectByType<BotSimulation>() == null)
            {
                new GameObject("BotSimulation").AddComponent<BotSimulation>();
            }
        }

        void Start()
        {
            Time.timeScale = speedUp;
            course = FindFirstObjectByType<CourseManager>();
            if (course != null && course.raft != null)
            {
                course.raft.gameObject.AddComponent<BotSimulationContacts>();
            }
            health = course != null && course.raft != null ? course.raft.GetComponent<RaftHealth>() : null;
            if (health != null)
            {
                health.Damaged += amount => { damage += amount; hits++; if (hits <= 12) { Trace("hit " + amount.ToString("F0")); } };
                health.Caught += () => { caught++; Trace("caught ice=" + IceZone.Amount(course.raft.transform.position).ToString("F2")); TracePredators(); };
                health.Broken += () => broken++;
            }
            Rider.StateChanged += OnRider;
            if (course != null)
            {
                course.RaceFinished += () => Report("FINISHED");
            }
            GameBootstrap bootstrap = FindFirstObjectByType<GameBootstrap>();
            RiverPlanner planner = bootstrap != null ? RiverPlanner.For(bootstrap.Level) : null;
            if (planner != null)
            {
                foreach (RockSpec rock in bootstrap.Level.rocks)
                {
                    float along = planner.Project(new Vector2(rock.position.x, rock.position.z), out float lateral, out _);
                    if (along < 160f)
                    {
                        Debug.Log("[BotSim] rock along=" + along.ToString("F1") + " lat=" + lateral.ToString("F1") + " r=" + rock.radius.ToString("F2") + " half=" + planner.HalfWidthAt(along).ToString("F1"));
                    }
                }
            }
            Debug.Log("[BotSim] start level=" + (bootstrap != null && bootstrap.Level != null ? bootstrap.Level.displayName : "-") + " bots=" + botCount + " botLevel=" + botLevel + " biome=" + (bootstrap != null ? bootstrap.Biome.ToString() : "-") + " ice=" + (bootstrap != null && bootstrap.Level != null ? bootstrap.Level.ice.Length : 0) + " pools=" + (bootstrap != null && bootstrap.Level != null ? bootstrap.Level.whirlpools.Length : 0));
        }

        void OnDestroy()
        {
            Rider.StateChanged -= OnRider;
        }

        void OnRider(Rider rider, RiderEvent change)
        {
            if (change == RiderEvent.Fell)
            {
                falls++;
            }
            else if (change == RiderEvent.Eliminated)
            {
                eliminated++;
            }
        }

        void Update()
        {
            if (done || course == null)
            {
                return;
            }
            float elapsed = Time.timeSinceLevelLoad;
            foreach (Rider rider in course.raft.riders)
            {
                if (rider != null && rider.IsFiring)
                {
                    firingTime.TryGetValue(rider, out float fired);
                    firingTime[rider] = fired + Time.deltaTime;
                }
            }
            if (elapsed >= nextTrace)
            {
                nextTrace = elapsed + (elapsed < 120f ? 2f : 10f);
                Trace("trace");
                firingTime.Clear();
            }
            if (elapsed >= nextReport)
            {
                nextReport = elapsed + 30f;
                Debug.Log("[BotSim] t=" + elapsed.ToString("F0") + " checkpoint=" + course.LastCheckpoint + "/" + course.CheckpointGoal + " speed=" + course.raft.Body.linearVelocity.magnitude.ToString("F1") + " health=" + (health != null ? health.Health.ToString("F0") : "-") + " h=" + CourseProfile.Height(course.raft.Body.position.x, course.raft.Body.position.z).ToString("F2") + " y=" + course.raft.Body.position.y.ToString("F2") + " slope=" + CourseProfile.Gradient(course.raft.Body.position.x, course.raft.Body.position.z).ToString("F3"));
            }
            if (elapsed > timeLimit)
            {
                Report("TIMEOUT");
            }
        }

        void TracePredators()
        {
            Vector3 raft = course.raft.transform.position;
            string line = "";
            foreach (MovingObstacle mover in MovingObstacle.Active)
            {
                float distance = Vector3.Distance(mover.transform.position, raft);
                if (distance < 12f)
                {
                    line += " [" + mover.kind + " d=" + distance.ToString("F1") + " pos=" + mover.transform.position.x.ToString("F1") + "," + mover.transform.position.z.ToString("F1") + " origin=" + mover.Origin.x.ToString("F1") + "," + mover.Origin.z.ToString("F1") + " axis=" + mover.TravelAxis.x.ToString("F2") + "," + mover.TravelAxis.z.ToString("F2") + " reach=" + mover.Reach.ToString("F1") + "]";
                }
            }
            Debug.Log("[BotSim] predators raft=" + raft.x.ToString("F1") + "," + raft.z.ToString("F1") + line);
        }

        void Trace(string label)
        {
            BotCrew crew = course.raft.GetComponent<BotCrew>();
            Vector3 position = course.raft.Body.position;
            string plan = "";
            if (crew != null && crew.Planner != null)
            {
                float along = crew.Planner.Project(new Vector2(position.x, position.z), out float lateral, out int segment);
                float half = crew.Planner.HalfWidthAt(along);
                float targetAlong = crew.Planner.Project(new Vector2(crew.Target.x, crew.Target.z), out float targetLateral, out _);
                plan = " along=" + along.ToString("F1") + " lat=" + lateral.ToString("F1") + "/" + half.ToString("F1") + " seg=" + segment + " tAlong=" + targetAlong.ToString("F1") + " tLat=" + targetLateral.ToString("F1") + " danger=" + crew.Danger.ToString("F0") + " cur=" + crew.Planner.CurrentAt(position).magnitude.ToString("F1") + " mode=" + crew.ModeName + " firing=" + crew.FiringCount + " demand=" + crew.Demand;
            }
            Vector3 velocity = course.raft.Body.linearVelocity;
            string riders = "";
            if (crew != null && crew.Planner != null)
            {
                float here = crew.Planner.Project(new Vector2(position.x, position.z), out _, out _);
                foreach (MovingObstacle crocodile in MovingObstacle.Active)
                {
                    Vector3 c = crocodile.transform.position;
                    float along = crew.Planner.Project(new Vector2(c.x, c.z), out float lateral, out _);
                    if (Mathf.Abs(along - here) < 25f)
                    {
                        riders += " <croc d=" + (along - here).ToString("F1") + " lat=" + lateral.ToString("F1") + " r=" + crocodile.Radius.ToString("F1") + "/" + crocodile.HalfWidth.ToString("F1") + " reach=" + crocodile.Reach.ToString("F1") + " v=" + crocodile.speed.ToString("F1") + ">";
                    }
                }
            }
            foreach (Collider near in Physics.OverlapSphere(position, 4f, ~0, QueryTriggerInteraction.Ignore))
            {
                if (near.attachedRigidbody == course.raft.Body)
                {
                    continue;
                }
                Vector3 closest = near.ClosestPoint(position);
                riders += " {" + (near.transform.parent != null ? near.transform.parent.name + "/" : "") + near.name + " " + Vector3.Distance(closest, position).ToString("F1") + "}";
            }
            foreach (Rider rider in course.raft.riders)
            {
                if (rider != null && rider.isActiveAndEnabled && rider.IsBot)
                {
                    float angle = crew != null ? Vector3.Angle(Vector3.ProjectOnPlane(rider.transform.forward, Vector3.up), crew.Aim) : 0f;
                    firingTime.TryGetValue(rider, out float fired);
                    riders += " [" + rider.Gas.Normalized.ToString("F2") + (rider.IsFiring ? "F" : "-") + (rider.Gas.Exhausted ? "X" : "") + angle.ToString("F0") + " f" + fired.ToString("F1") + "]";
                }
            }
            Debug.Log("[BotSim] " + label + " t=" + Time.timeSinceLevelLoad.ToString("F1") + " pos=" + position.x.ToString("F1") + "," + position.z.ToString("F1") + " vel=" + velocity.x.ToString("F1") + "," + velocity.z.ToString("F1") + " wp=" + course.WaypointIndex + plan + riders);
        }

        void Report(string outcome)
        {
            if (done)
            {
                return;
            }
            done = true;
            Debug.Log("[BotSim] contacts " + BotSimulationContacts.Summary());
            Debug.Log("[BotSim] RESULT " + outcome + " time=" + course.ElapsedTime.ToString("F1") + " checkpoint=" + course.LastCheckpoint + "/" + course.CheckpointGoal + " respawns=" + course.RespawnCount + " hits=" + hits + " damage=" + damage.ToString("F0") + " caught=" + caught + " broken=" + broken + " falls=" + falls + " eliminated=" + eliminated);
            Application.Quit();
        }
    }
}
