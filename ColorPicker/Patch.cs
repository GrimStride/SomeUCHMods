using BepInEx;
using HarmonyLib;
using System.Reflection;

namespace ColorPicker
{
    [BepInPlugin("gs.colorpicker", "ColorPicker", "1.0.0.0")]
    public class ColorPickerMod : BaseUnityPlugin
    {
        public void Awake()
        {
            var harmony = new Harmony("gs.colorpicker");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}