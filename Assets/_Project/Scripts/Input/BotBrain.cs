using UnityEngine;

namespace FartRafters
{
    public class BotBrain : RiderBrain
    {
        public CourseManager course;
        public BotDifficulty difficulty;

        static BotDifficulty fallback;

        RiderCommand mistake;
        float nextDecisionTime;
        float mistakeEndTime;
        float selfishEndTime;
        float noise;

        BotDifficulty Settings
        {
            get
            {
                if (difficulty != null)
                {
                    return difficulty;
                }
                if (fallback == null)
                {
                    fallback = ScriptableObject.CreateInstance<BotDifficulty>();
                }
                return fallback;
            }
        }

        public override RiderCommand GetCommand(Rider rider)
        {
            if (course == null || rider.raft == null || course.Finished)
            {
                return default;
            }

            BotDifficulty settings = Settings;
            float now = Time.time;
            if (now < mistakeEndTime)
            {
                return mistake;
            }

            BotCrew crew = BotCrew.For(rider.raft);
            RiderCommand command = crew.CommandFor(rider, settings);

            if (now >= nextDecisionTime)
            {
                float interval = settings.decisionInterval * Random.Range(0.7f, 1.3f);
                nextDecisionTime = now + interval;
                noise = settings.aimNoiseDegrees > 0f ? Random.Range(-settings.aimNoiseDegrees, settings.aimNoiseDegrees) : 0f;

                float mistakeChance = settings.mistakeChancePerSecond;
                if (CourseHazard.Contains(rider.raft.Body.position))
                {
                    mistakeChance = Mathf.Max(mistakeChance, settings.hazardMistakeChancePerSecond);
                }
                if (mistakeChance > 0f && Random.value < mistakeChance * interval)
                {
                    Vector2 random = Random.insideUnitCircle.normalized;
                    mistake = new RiderCommand(new Vector3(random.x, 0f, random.y), true);
                    mistakeEndTime = now + Random.Range(settings.mistakeDuration.x, settings.mistakeDuration.y);
                    return mistake;
                }
                if (settings.teamwork < 1f && Random.value < (1f - settings.teamwork) * 0.6f * interval)
                {
                    selfishEndTime = now + Random.Range(0.8f, 2f);
                }
            }

            if (now < selfishEndTime)
            {
                bool gasOk = !rider.Gas.Exhausted && rider.Gas.Normalized > 0.02f;
                command.gas = crew.Demand && gasOk;
            }
            if (noise != 0f && command.HasAim)
            {
                command.aim = Quaternion.AngleAxis(noise, Vector3.up) * command.aim;
            }
            return command;
        }
    }
}
