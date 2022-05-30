using Rocket.Unturned.Player;
using SDG.Unturned;
using UnityEngine;

namespace RFJob.Patches
{
    internal static class HarmonyPatch
    {
        internal delegate void PlayerChat(UnturnedPlayer player, ref string iconUrl);
        internal static event PlayerChat OnPlayerChat;
        
        [HarmonyLib.HarmonyPatch]
        internal static class ChatPatch
        {
            [HarmonyLib.HarmonyPatch(typeof(ChatManager), "serverSendMessage")]
            [HarmonyLib.HarmonyPrefix]
            internal static void ServerSendMessage(string text, Color color, SteamPlayer fromPlayer,
                SteamPlayer toPlayer, EChatMode mode, ref string iconURL,
                bool useRichTextFormatting)
            {
                if (fromPlayer != null && fromPlayer.player != null)
                {
                    OnPlayerChat?.Invoke(UnturnedPlayer.FromPlayer(fromPlayer.player), ref iconURL);
                }
            }
        }
    }
}