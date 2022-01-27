using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Threading;
using HarmonyLib;
using UnityEngine;
using GameEvent;

namespace ColorPicker
{
    [HarmonyPatch(typeof(GameState), "OnGUI")]
    public class ColorChoose
    {
        public const int
        CC_RGBINIT = 0x00000001,
        CC_FULLOPEN = 0x00000002;
        
        [DllImport("comdlg32.dll", CharSet = CharSet.Auto)]
        public static extern bool ChooseColor([In, Out] CHOOSECOLOR cc);
        
        public delegate IntPtr WndProc(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public class CHOOSECOLOR {
            public int      lStructSize = Marshal.SizeOf<CHOOSECOLOR>();
            public IntPtr   hwndOwner;
            public IntPtr   hInstance;
            public int      rgbResult;
            public IntPtr   lpCustColors;
            public int      Flags;
            public IntPtr   lCustData = IntPtr.Zero;
            public WndProc?  lpfnHook;
            public string?   lpTemplateName;
        }
        
        public static void LaunchColorDlg()
        {
            CHOOSECOLOR cc = new CHOOSECOLOR();
            int rgbCurrent = lastColor;
            IntPtr custColorPtr = Marshal.AllocCoTaskMem(64);
            Marshal.Copy(custColors, 0, custColorPtr, 16);
            cc.lStructSize = Marshal.SizeOf<CHOOSECOLOR>();
            cc.hwndOwner = new IntPtr(0);
            cc.rgbResult = rgbCurrent;
            cc.Flags = (CC_RGBINIT | CC_FULLOPEN);
            cc.lpCustColors = custColorPtr;
            if (ChooseColor(cc))
            {
                byte[] intBytes = BitConverter.GetBytes(cc.rgbResult);
                string hex = "#";
                hex += BitConverter.ToString(intBytes).Replace("-", "").Remove(6, 2);
                Color colorFinal;
                if(ColorUtility.TryParseHtmlString(hex, out colorFinal))
                {
                    if(toggleGamma) colorFinal = colorFinal.gamma;
                    else colorFinal = colorFinal.linear;
                    GameEventManager.SendEvent(new SetpieceColorChangeEvent(colorFinal));
                    lastColor = cc.rgbResult;
                    Marshal.Copy(custColorPtr, custColors, 0, 16);
                }
            }
            dlgLaunched = false;
        }
        
        static void Postfix()
        {
            if (canShow)
            {
                var backgroundColor = GUI.backgroundColor;
                GUI.backgroundColor = new Color(0.1764706f, 0.1764706f, 0.1764706f);
                string gammaText =  "Gamma " + (toggleGamma ? "ON" : "OFF");
                if (GUI.Button(new Rect((float)(Screen.width - 80f), (float)(Screen.height - 40), 80f, 40f), gammaText, guiStyle))
                    toggleGamma = !toggleGamma;
                GUI.backgroundColor = new Color(0.02315f, 0.20863f, 0.40724f);
                if (GUI.Button(new Rect((float)(Screen.width - 168f), (float)(Screen.height - 40), 84f, 40f), "Custom color", guiStyle) && !dlgLaunched)
                {
                    dlgLaunched = true;
                    Thread job = new Thread(LaunchColorDlg);
                    job.Start();
                }
            }
        }
        
        public static bool canShow = false;
        
        private static readonly Texture2D backgroundTexture = Texture2D.whiteTexture;
        
        public static GUIStyle guiStyle = new GUIStyle() {alignment = TextAnchor.MiddleCenter,
                                                            normal = new GUIStyleState {background = backgroundTexture, textColor = Color.white}};
        
        private static bool dlgLaunched = false;
        
        private static bool toggleGamma = false;
        
        private static int lastColor = 0xffffff;
        
        private static int[] custColors = new int[] {0xffffff, 0xffffff, 0xffffff, 0xffffff, 0xffffff,
                                                    0xffffff, 0xffffff, 0xffffff, 0xffffff, 0xffffff,
                                                    0xffffff, 0xffffff, 0xffffff, 0xffffff, 0xffffff, 0xffffff};
    }
    
    [HarmonyPatch(typeof(InventoryBook), "Show")]
    public class OnInvShow
    {
        static void Postfix(bool OpenSound)
        {
            GameState.GameMode gameMode = GameSettings.GetInstance().GameMode;
            if (OpenSound && gameMode == GameState.GameMode.FREEPLAY) ColorChoose.canShow = true;
            else if(ColorChoose.canShow) ColorChoose.canShow = false;
        }
    }
    
    [HarmonyPatch(typeof(InventoryBook), "Hide")]
    public class OnInvHide
    {
        static void Postfix()
        {
            if (ColorChoose.canShow) ColorChoose.canShow = false;
        }
    }
    
    [HarmonyPatch(typeof(InventoryBook), "TurnOffScreens")]
    public class OnOffScreens
    {
        static void Postfix(bool ShowBook)
        {
            if (ShowBook && !ColorChoose.canShow) ColorChoose.canShow = true;
        }
    }
    
    [HarmonyPatch(typeof(InventoryBook), "TurnScreenOn")]
    public class OnOnScreens
    {
        static void Check()
        {
            if (ColorChoose.canShow) ColorChoose.canShow = false;
        }
        
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var patch = AccessTools.Method(typeof(OnOnScreens), nameof(Check));
            var codes = new List<CodeInstruction>(instructions);
            for (var i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Ldstr)
                {
                    if(System.Object.ReferenceEquals(codes[i].operand, "On"))
                    {
                        codes.Insert(i + 3, new CodeInstruction(OpCodes.Call, patch));
                        break;
                    }
                }
            }
            return codes.AsEnumerable();
        }
    }
}
