using System;
using RFJob.Enums;
using RFRocketLibrary.Hooks;
using Rocket.Core.Logging;
using Rocket.Unturned.Player;

namespace RFJob.Managers
{
    internal static class BalanceManager
    {
        internal static void AddBalance(UnturnedPlayer player, decimal amount)
        {
            try
            {
                switch (Plugin.Conf.Balance)
                {
                    case EBalance.UCONOMY:
                        UconomyHook.Deposit(player.CSteamID.m_SteamID, amount);
                        break;
                    case EBalance.EXPERIENCE:
                        player.Player.skills.ServerModifyExperience((int) amount);
                        break;
                    case EBalance.AVIECONOMY:
                        AviEconomyHook.Deposit(player, amount);
                        break;
                }

            }
            catch (Exception e)
            {
                Logger.LogError($"[{Plugin.Inst.Name}] [ERROR] BalanceManager AddBalance: " + e.Message);
                Logger.LogError($"[{Plugin.Inst.Name}] [ERROR] Details: " + (e.InnerException ?? e));
            }
        }

        internal static void DecreaseBalance(UnturnedPlayer player, decimal amount)
        {
            try
            {
                switch (Plugin.Conf.Balance)
                {
                    case EBalance.UCONOMY:
                        UconomyHook.Withdraw(player.CSteamID.m_SteamID, amount);
                        break;
                    case EBalance.EXPERIENCE:
                        player.Player.skills.ServerModifyExperience(-(int) amount);
                        break;
                    case EBalance.AVIECONOMY:
                        AviEconomyHook.Withdraw(player, amount);
                        break;
                }

            }
            catch (Exception e)
            {
                Logger.LogError($"[{Plugin.Inst.Name}] [ERROR] BalanceManager DecreaseBalance: " + e.Message);
                Logger.LogError($"[{Plugin.Inst.Name}] [ERROR] Details: " + (e.InnerException ?? e));
            }
        }

        internal static decimal GetBalance(UnturnedPlayer player)
        {
            try
            {
                switch (Plugin.Conf.Balance)
                {
                    case EBalance.UCONOMY:
                        return UconomyHook.GetBalance(player.CSteamID.m_SteamID);
                    case EBalance.EXPERIENCE:
                        return player.Player.skills.experience;
                    case EBalance.AVIECONOMY:
                        return AviEconomyHook.GetBalance(player);
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            catch (Exception e)
            {
                Logger.LogError($"[{Plugin.Inst.Name}] [ERROR] BalanceManager GetBalance: " + e.Message);
                Logger.LogError($"[{Plugin.Inst.Name}] [ERROR] Details: " + (e.InnerException ?? e));
                return 0;
            }
        }
    }
}