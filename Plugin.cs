using System;
using System.Collections.Generic;
using HarmonyLib;
using RFJob.DatabaseManagers;
using RFJob.Enums;
using RFJob.EventListeners;
using RFJob.Models;
using RFRocketLibrary.Enum;
using RFRocketLibrary.Helpers;
using RFRocketLibrary.Utils;
using Rocket.API;
using Rocket.API.Collections;
using Rocket.Core.Plugins;
using Rocket.Unturned;
using Rocket.Unturned.Chat;
using SDG.Unturned;
using UnityEngine;
using HarmonyPatch = RFJob.Patches.HarmonyPatch;
using Logger = Rocket.Core.Logging.Logger;

namespace RFJob
{
    public class Plugin : RocketPlugin<Configuration>
    {
        private static int Major = 1;
        private static int Minor = 0;
        private static int Patch = 0;
        
        public static Plugin Inst;
        public static Configuration Conf;
        internal static Color MsgColor;

        internal HashSet<Job> PublicJobs;
        internal HashSet<Job> PrivateJobs;
        internal Dictionary<Job, HashSet<ulong>> PrivateJobRequest;
        internal Dictionary<ulong, DateTime> JoinJobCooldown;

        private Harmony _harmony;
        private const string HarmonyId = "RFJobs.Patches";

        protected override void Load()
        {
            Inst = this;
            Conf = Configuration.Instance;
            if (Conf.Enabled)
            {
                MsgColor = UnturnedChat.GetColorFromName(Conf.MessageColor, Color.green);
                
                DependencyUtil.Load(EDependency.NewtonsoftJson);
                DependencyUtil.Load(EDependency.SystemRuntimeSerialization);
                
                PlayerDataManager.Initialize();

                _harmony = new Harmony(HarmonyId);
                _harmony.PatchAll();
                
                U.Events.OnPlayerConnected += PlayerEvent.OnConnected;
                U.Events.OnPlayerDisconnected += PlayerEvent.OnDisconnected;
                HarmonyPatch.OnPlayerChat += PlayerEvent.OnPlayerChat;
                Level.onPostLevelLoaded += ServerEvent.OnPostLevelLoaded;
                
                if (Level.isLoaded)
                    ServerEvent.OnPostLevelLoaded(0);
            }
            else
                Logger.LogWarning($"[{Name}] Plugin: DISABLED");

            Logger.LogWarning($"[{Name}] Plugin loaded successfully!");
            Logger.LogWarning($"[{Name}] {Name} v{Major}.{Minor}.{Patch}");
            Logger.LogWarning($"[{Name}] Made with 'rice' by RiceField Plugins!");
        }
        protected override void Unload()
        {
            if (Conf.Enabled)
            {
                _harmony.UnpatchAll(HarmonyId);
                
                U.Events.OnPlayerConnected -= PlayerEvent.OnConnected;
                U.Events.OnPlayerDisconnected -= PlayerEvent.OnDisconnected;
                HarmonyPatch.OnPlayerChat -= PlayerEvent.OnPlayerChat;
                Level.onPostLevelLoaded -= ServerEvent.OnPostLevelLoaded;
                
                StopAllCoroutines();
            }
            
            Conf = null;
            Inst = null;

            Logger.LogWarning($"[{Name}] Plugin unloaded successfully!");
        }
        public override TranslationList DefaultTranslations => new()
        {            
            {$"{EResponse.ANNOUNCE_JOB_JOIN}", "{0} has joined {1} job!"},
            {$"{EResponse.CONSOLE_NOT_ALLOWED}", "Console is not allowed to do this command!"},
            {$"{EResponse.HAS_JOB}", "You can only have 1 job at a time!"},
            {$"{EResponse.INVALID_PARAMETER}", "Invalid parameter! Usage: {0}"},
            {$"{EResponse.JOB_ACCEPT_SUCCESS}", "Successfully accepted {0} job application!"},
            {$"{EResponse.JOB_ADD_SUCCESS}", "Successfully added {0} to {1} job!"},
            {$"{EResponse.JOB_APPLY_LEADER}", "{0} has submitted an application to join {1} job!"},
            {$"{EResponse.JOB_APPLY_SUCCESS}", "Successfully submitted application for {0} job! Please wait for the leader to accept!"},
            {$"{EResponse.JOB_CLEAR_SUCCESS}", "Successfully cleared {0} job members!"},
            {$"{EResponse.JOB_FIRE_SUCCESS}", "Successfully fired {0} from their job!"},
            {$"{EResponse.JOB_FULL}", "This job is full!"},
            {$"{EResponse.JOB_JOIN_COOLDOWN}", "You are under join job cooldown! Remaining time: {0}s"},
            {$"{EResponse.JOB_JOIN_SUCCESS}", "Successfully joined {0} job!"},
            {$"{EResponse.JOB_KICK_SUCCESS}", "Successfully kicked {0} from {0} job!"},
            {$"{EResponse.JOB_LEAVE_SUCCESS}", "Successfully resigned from {0} job!"},
            {$"{EResponse.JOB_LIST_END_PRIVATE}", "This is the end of private jobs list."},
            {$"{EResponse.JOB_LIST_END_PUBLIC}", "This is the end of public jobs list."},
            {$"{EResponse.JOB_LIST_NEXT}", "Next page: \"/job list {0} {1}\""},
            {$"{EResponse.JOB_LIST}", "{0}"},
            {$"{EResponse.JOB_NO_PERMISSION}", "You don't have permission to join this job!"},
            {$"{EResponse.JOB_NOT_FOUND}", "Job not found!"},
            {$"{EResponse.JOB_REMOVE_SUCCESS}", "Successfully removed {0} from {0} job!"},
            {$"{EResponse.NO_JOB}", "You don't have any job!"},
            {$"{EResponse.NOT_LEADER}", "You are not a leader of this job!"},
            {$"{EResponse.PLAYER_HAS_JOB}", "This player already joined a job!"},
            {$"{EResponse.PLAYER_IS_MEMBER}", "Player is already a member of this job!"},
            {$"{EResponse.PLAYER_NO_JOB}", "This player does not have any job!"},
            {$"{EResponse.PLAYER_NOT_APPLY}", "This player does not have application for this job!"},
            {$"{EResponse.PLAYER_NOT_FOUND}", "Player not found!"},
            {$"{EResponse.PLAYER_NOT_MEMBER}", "This player is not a member of {0} job!"},
            {$"{EResponse.SALARY}", "It's pay time! You earned ${0} from your job!"},
        };

        internal static string TranslateRich(string s, params object[] objects)
        {
            return Inst.Translate(s, objects).Replace("-=", "<").Replace("=-", ">");
        }

        internal static void Say(IRocketPlayer rPlayer, string s)
        {
            ChatHelper.Say(rPlayer, s, MsgColor, Conf.MessageIconUrl);
        }

        internal static void Say(string s)
        {
            ChatHelper.Broadcast(s, MsgColor, Conf.MessageIconUrl);
        }
    }
}