using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;
#if DEBUG
using UnityEngine.SceneManagement;
#endif

namespace SpectatorCamera
{
    [HarmonyPatch(typeof(Character), "checkCameraToggle")]
    public class CameraChange
    {
        static void checkCharacter(Character instance)
        {
            List<Character> remote = new List<Character>();
            ZoomCamera camera = ZoomCamera.CurrentZoomCamera.GetComponent<ZoomCamera>();
            GameState.GameMode gameMode = GameSettings.GetInstance().GameMode;
            if (gameMode != GameState.GameMode.FREEPLAY && instance.Dead && !instance.isZombie && !instance.isGhost || instance.Success && !ZoomCamera.LocalOnly)
            {
                foreach (Character character in camera.characters)
                {
                    if (camera.targets.Contains(character.transform))
                    {
                        if (character.hasAuthority) return;
                        remote.Add(character);
                    }
                }
                if (remote.Count > 1)
                {
                    camera.targets.Clear();
                    if (instance.rotateLeftDown || instance.rotateLeft) camera.targets.Add(remote[remote.Count - 1].transform);
                    else if (instance.rotateRightDown || instance.rotateRight) camera.targets.Add(remote[0].transform);
                }
                else if (remote.Count == 1)
                {
                    var i = 0;
                    for (; i < camera.characters.Count; i++) if (camera.characters[i] == remote[0]) break;
                    if (instance.rotateLeftDown || instance.rotateLeft)
                    {
                        i--;
                        if (i == -1) camera.trackAllPlayers();
                        else
                        {
                            camera.targets.Remove(camera.characters[i + 1].transform);
                            camera.targets.Add(camera.characters[i].transform);
                        }
                    }
                    else if (instance.rotateRightDown || instance.rotateRight)
                    {
                        i++;
                        if (i == camera.characters.Count) camera.trackAllPlayers();
                        else
                        {
                            camera.targets.Remove(camera.characters[i - 1].transform);
                            camera.targets.Add(camera.characters[i].transform);
                        }
                    }
                }
            }
        }
        
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            var fldRotL1 = AccessTools.Field(typeof(Character), "rotateLeftDown");
            var fldRotL2 = AccessTools.Field(typeof(Character), "rotateLeft");
            var patch = AccessTools.Method(typeof(CameraChange), nameof(checkCharacter));
            var codes = new List<CodeInstruction>(instructions);
            codes.Add(new CodeInstruction(OpCodes.Ldarg_0));
            codes.Add(new CodeInstruction(OpCodes.Call, patch));
            codes.Add(new CodeInstruction(OpCodes.Ret));
            Label jumpTrue = generator.DefineLabel();
            codes[codes.Count - 3].labels.Add(jumpTrue);
            int j = 0;
            for (var i = 0; i < codes.Count; i++)
            {
                if (CodeInstructionExtensions.LoadsField(codes[i], fldRotL1) || CodeInstructionExtensions.LoadsField(codes[i], fldRotL2))
                {
                    codes.RemoveAt(i + 1);
                    j++;
                    Debug.LogWarning("Found at " + i + ", j=" + j);
                    if (j == 2)
                    {
                        codes[i + 3] = new CodeInstruction(OpCodes.Bne_Un_S, jumpTrue);
                        break;
                    }
                    else codes[i + 3].opcode = OpCodes.Beq_S;
                }
            }
            #if DEBUG
            for (var i = 0; i < codes.Count; i++) Debug.LogWarning(codes[i].ToString());
            #endif
            return codes.AsEnumerable();
        }
    }
    
    #if DEBUG
    [HarmonyPatch(typeof(GameState), "OnGUI")]
    public class DrawInfo
    {
        static void Postfix()
        {
            if(ZoomCamera.CurrentZoomCamera == null) return;
            ZoomCamera camera = ZoomCamera.CurrentZoomCamera.GetComponent<ZoomCamera>();
            string text = "Current characters in cam: ";
            string text1 = "Current characters in cam targets: ";
            foreach (Character character in camera.characters)
            {
                if (character.AssociatedGamePlayer != null) text += character.AssociatedGamePlayer.playerName;
                else if (character.AssociatedGamePlayer != null) text += character.AssociatedLobbyPlayer.playerName;
                else continue;
                text += ", ";
                if (camera.targets.Contains(character.transform))
                {
                    text1 += character.AssociatedGamePlayer.playerName;
                    text1 += ", ";
                }
            }
            int textheight = 20;
            debugStyleBg.normal.textColor = Color.white;
            GUI.Label(new Rect(3f, (float)(Screen.height - textheight * 1) - 12.5f, 1024f, (float)textheight), text, debugStyle);
            GUI.Label(new Rect(2f, (float)(Screen.height - textheight * 1) - 12f, 1024f, (float)textheight), text, debugStyleBg);
            GUI.Label(new Rect(3f, (float)(Screen.height - textheight * 1) + 0.5f, 1024f, (float)textheight), text1, debugStyle);
            GUI.Label(new Rect(2f, (float)(Screen.height - textheight * 1), 1024f, (float)textheight), text1, debugStyleBg);
        }
        
        public static GUIStyle debugStyleBg = new GUIStyle();
        
        public static GUIStyle debugStyle = new GUIStyle();
    }
    #endif
}