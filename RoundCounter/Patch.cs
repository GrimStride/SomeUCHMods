using BepInEx;
using HarmonyLib;
using System.Reflection;

namespace RoundCounter
{
    [BepInPlugin("gs.roundcounter", "RoundCounter", "1.0.0.0")]
    public class RoundCounterMod : BaseUnityPlugin
    {
        public void Awake()
        {
            var harmony = new Harmony("gs.roundcounter");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}