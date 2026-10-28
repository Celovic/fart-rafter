using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace FartRafters.EditorTools
{
    public static class CourseAudit
    {
        public static void Run()
        {
            GameCatalog catalog = AssetDatabase.LoadAssetAtPath<GameCatalog>("Assets/_Project/Data/GameCatalog.asset");
            var report = new StringBuilder();
            int totalBad = 0;
            int totalChecked = 0;
            foreach (AdventureDifficulty source in catalog.difficulties)
            {
                for (int crew = 1; crew <= 4; crew++)
                {
                    AdventureDifficulty difficulty = CrewScaling.Apply(source, crew);
                    int bad = 0;
                    int count = 0;
                    float worst = float.MaxValue;
                    float worstCluster = float.MaxValue;
                    for (int seed = 1; seed <= 250; seed++)
                    {
                        LevelDefinition level = CourseGenerator.Generate(difficulty, seed * 7919 + crew, CrewScaling.Intensity(crew));
                        float[] distances = Distances(level);
                        var sweepers = new List<Vector3>();
                        foreach (ObstacleSpec obstacle in level.obstacles)
                        {
                            sweepers.Add(obstacle.position);
                        }
                        foreach (SpecialSpec special in level.specials)
                        {
                            sweepers.Add(special.position);
                        }
                        foreach (Vector3 position in sweepers)
                        {
                            count++;
                            float tight = Tightest(level, distances, position);
                            worst = Mathf.Min(worst, tight);
                            if (tight < CourseGenerator.SweeperClearance - 0.01f)
                            {
                                bad++;
                            }
                        }
                        foreach (RockSpec rock in level.rocks)
                        {
                            count++;
                            float rt = Tightest(level, distances, rock.position);
                            if (rt < CourseGenerator.ClusterClearance - 0.01f)
                            {
                                bad++;
                                worstCluster = Mathf.Min(worstCluster, rt);
                            }
                        }
                        foreach (BarrelSpec barrel in level.barrels)
                        {
                            count++;
                            float bt = Tightest(level, distances, barrel.position);
                            if (bt < CourseGenerator.ClusterClearance - 0.01f)
                            {
                                bad++;
                                worstCluster = Mathf.Min(worstCluster, bt);
                            }
                        }
                    }
                    totalBad += bad;
                    totalChecked += count;
                    report.AppendLine(source.displayName + " crew " + crew + ": checked " + count + " bad " + bad + " tightestSweeper " + worst.ToString("0.0") + " worstBadCluster " + (worstCluster < 1000f ? worstCluster.ToString("0.0") : "-"));
                }
            }
            report.AppendLine("TOTAL checked " + totalChecked + " bad " + totalBad);
            Directory.CreateDirectory("Logs");
            File.WriteAllText("Logs/course_audit.txt", report.ToString());
            Debug.Log(report.ToString());
            if (Application.isBatchMode)
            {
                EditorApplication.Exit(totalBad == 0 ? 0 : 2);
            }
        }

        static float[] Distances(LevelDefinition level)
        {
            var distances = new float[level.NodeCount];
            for (int i = 1; i < level.NodeCount; i++)
            {
                distances[i] = distances[i - 1] + Vector3.Distance(level.Center(i - 1), level.Center(i));
            }
            return distances;
        }

        static float Tightest(LevelDefinition level, float[] distances, Vector3 position)
        {
            int nearest = 0;
            float best = float.MaxValue;
            for (int i = 0; i < level.NodeCount; i++)
            {
                Vector3 delta = level.Center(i) - position;
                delta.y = 0f;
                float d = delta.sqrMagnitude;
                if (d < best)
                {
                    best = d;
                    nearest = i;
                }
            }
            float min = float.MaxValue;
            for (int i = 0; i < level.NodeCount; i++)
            {
                if (Mathf.Abs(distances[i] - distances[nearest]) <= CourseGenerator.NarrowWindow - 1f)
                {
                    min = Mathf.Min(min, level.Width(i));
                }
            }
            return min;
        }
    }
}
