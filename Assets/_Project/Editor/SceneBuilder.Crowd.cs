using System.Collections.Generic;
using FartRafters.EditorTools.Sculpt;
using UnityEngine;

namespace FartRafters.EditorTools
{
    public static partial class SceneBuilder
    {
        static void ApplySpectators(CourseTheme theme)
        {
            var looks = new List<SpectatorLook>();
            for (int i = 0; i < SpectatorSculptor.People.Length; i++)
            {
                looks.Add(BakeSpectator("Person" + i, SpectatorSculptor.Person(SpectatorSculptor.People[i]), false));
            }
            looks.Add(BakeSpectator("Dog", SpectatorSculptor.Dog(), true));
            looks.Add(BakeSpectator("Sheep", SpectatorSculptor.Sheep(), true));
            looks.Add(BakeSpectator("Duck", SpectatorSculptor.Duck(), true));
            theme.spectators = looks.ToArray();

            theme.signMesh = SaveMesh(SurfaceNets.Build(SpectatorSculptor.Sign(), 0.014f, "Sign"), "Crowd");
            theme.snackMeshes = new[]
            {
                SaveMesh(SurfaceNets.Build(SpectatorSculptor.BeanBowl(), 0.008f, "SnackBeans"), "Crowd"),
                SaveMesh(SurfaceNets.Build(SpectatorSculptor.PastirmaBread(), 0.008f, "SnackPastirma"), "Crowd"),
                SaveMesh(SurfaceNets.Build(SpectatorSculptor.Simit(), 0.007f, "SnackSimit"), "Crowd")
            };

            theme.fishMesh = SaveMesh(SurfaceNets.Build(CritterSculptor.Fish(), 0.01f, "Fish"), "Props");
            theme.duckMesh = SaveMesh(SurfaceNets.Build(CritterSculptor.Duck(), 0.01f, "Duck"), "Props");
            theme.butterflyWingMesh = SaveMesh(SurfaceNets.Build(CritterSculptor.ButterflyWing(new Color(0.98f, 0.62f, 0.15f)), 0.006f, "ButterflyWing"), "Props");
            theme.turtleMesh = SaveMesh(SurfaceNets.Build(CritterSculptor.Turtle(), 0.012f, "Turtle"), "Props");

            string crowd = Root + "/Audio/Crowd";
            theme.crowdBoo = LoadClips(crowd, "boo_");
            theme.crowdCheer = LoadClips(crowd, "cheer_");
            theme.crowdClap = LoadClips(crowd, "clap_");
            theme.crowdChant = LoadClips(crowd, "chant_");
            theme.crowdChew = LoadClips(crowd, "chew_");
        }

        static SpectatorLook BakeSpectator(string name, AvatarSculpture sculpture, bool animal)
        {
            return new SpectatorLook
            {
                name = name,
                animal = animal,
                body = SaveMesh(SurfaceNets.Build(sculpture.body, 0.026f, "Crowd_" + name + "_Body"), "Crowd"),
                head = SaveMesh(SurfaceNets.Build(sculpture.head, 0.018f, "Crowd_" + name + "_Head"), "Crowd"),
                armLeft = SaveMesh(SurfaceNets.Build(sculpture.armLeft, 0.02f, "Crowd_" + name + "_ArmLeft"), "Crowd"),
                armRight = SaveMesh(SurfaceNets.Build(sculpture.armRight, 0.02f, "Crowd_" + name + "_ArmRight"), "Crowd"),
                eye = SaveMesh(SurfaceNets.Build(sculpture.eye, 0.014f, "Crowd_" + name + "_Eye"), "Crowd"),
                headPivot = sculpture.headPivot,
                shoulder = sculpture.shoulder,
                eyePosition = sculpture.eyePosition
            };
        }
    }
}
