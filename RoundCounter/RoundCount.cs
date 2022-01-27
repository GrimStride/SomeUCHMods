using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;

namespace RoundCounter
{
    [HarmonyPatch(typeof(GameControl), "ToPlaceMode")]
    public class DisplayRound
    {
        static void ShowRound()
        {
            GameState.GameMode gameMode = GameSettings.GetInstance().GameMode;
            if (gameMode == GameState.GameMode.CREATIVE || gameMode == GameState.GameMode.PARTY)
            {
                string text = I2.Loc.ScriptLocalization.RuleBook.RoundSingular.Substring(4);
                int num = LobbyManager.instance.CurrentGameController.RoundNumber + 1;
                UserMessageManager.Instance.UserMessage(char.ToUpperInvariant(text[0]) + text.Substring(1) + " " + num,
                    1f, UserMessageManager.UserMsgPriority.hi, false);
            }
        }
        
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var patch = AccessTools.Method(typeof(DisplayRound), nameof(ShowRound));
            var codes = new List<CodeInstruction>(instructions);
            for (var i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Ldc_I4_3)
                {
                    codes.Insert(i, new CodeInstruction(OpCodes.Call, patch));
                    break;
                }
            }
            return codes.AsEnumerable();
        }
    }
}