using BepInEx;
using HarmonyLib;
using System.Reflection;

namespace SpectatorCamera
{
    [BepInPlugin("gs.spectatorcamera", "SpectatorCamera", "1.0.0.0")]
    public class SpectatorCameraMod : BaseUnityPlugin
    {
        public void Awake()
        {
            var harmony = new Harmony("gs.spectatorcamera");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}