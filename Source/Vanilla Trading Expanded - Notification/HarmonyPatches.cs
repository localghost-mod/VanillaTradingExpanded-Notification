using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using HarmonyLib;
using VanillaTradingExpanded;
using RimWorld;

namespace VTE_Notification
{
    [StaticConstructorOnStartup]
    public static class Startup
    {
        static Startup()
        {
            var harmony = new Harmony("localghost.VTENotification");
            harmony.PatchAll();
        }
    }
    [HarmonyPatch(typeof(TradingManager), "GenerateRandomContract")]
    public class GenerateRandomContractPatch
    {
        private static void Postfix(ref Contract __result)
        {
            if (!Utils.isTradingTerminalUsable()) { return; }

            Utils.Show("NewContract", "VTENotification.NewContract".Translate(), "VTENotification.NewContractDesc".Translate(__result.Name, __result.reward, __result.reward / __result.BaseMarketValue * 100f));
        }
    }
    [HarmonyPatch(typeof(TradingManager), "ProcessContracts")]
    public class ProcessContractsPatch
    {
        private static void Prefix(TradingManager __instance, Map localMap, out List<Tuple<string, int, float>> __state)
        {
            __state = new List<Tuple<string, int, float>>();
            if (!Utils.isTradingTerminalUsable()) { return; }

            foreach (var contract in __instance.npcSubmittedContracts)
            {
                __state.Add(Tuple.Create(contract.Name, contract.reward, contract.reward / contract.BaseMarketValue * 100f));
            }
        }
        private static void Postfix(TradingManager __instance, ref List<Tuple<string, int, float>> __state)
        {
            if (!Utils.isTradingTerminalUsable()) { return; }

            var __current_state = new List<Tuple<string, int, float>>();
            foreach (var contract in __instance.npcSubmittedContracts)
            {
                __current_state.Add(Tuple.Create(contract.Name, contract.reward, contract.reward / contract.BaseMarketValue * 100f));
            }
            if (!__state.Equals(__current_state))
            {
                foreach (var contract in __state.Except(__current_state))
                {
                    Utils.Show("EndContract", "VTENotification.EndContract".Translate(), "VTENotification.EndContractDesc".Translate(contract.Item1, contract.Item2.ToString()));
                }
            }
        }
    }
    [HarmonyPatch(typeof(TradingManager), "CreateNews")]
    public class CreateNewsPatch
    {
        private static void Postfix(ref News __result)
        {
            if (!Utils.isTradingTerminalUsable()) { return; }

            Utils.Show("News", "VTENotification.News".Translate(), __result.text);
        }
    }
    [HarmonyPatch(typeof(TradingManager), "DoPriceImpactsFromNews")]
    public class DoPriceImpactsFromNewsPatch
    {
        private static void Prefix(TradingManager __instance, out Dictionary<Company, float> __state)
        {
            __state = new Dictionary<Company, float>();
            if (!Utils.isTradingTerminalUsable()) { return; }

            foreach (var company in __instance.companies)
            {
                __state.Add(company, company.currentValue);
            }
        }
        private static void Postfix(TradingManager __instance, ref Dictionary<Company, float> __state)
        {
            if (!Utils.isTradingTerminalUsable()) { return; }

            foreach (var company in __instance.companies)
            {
                if (company.currentValue != __state[company])
                {
                    var rate = (company.currentValue - __state[company]) / __state[company] * 100f;
                    if (rate > 1)
                    {
                        Utils.Show("Stock", "VTENotification.StockRise".Translate(), "VTENotification.StockRiseDesc".Translate(company.name, rate));
                    }
                    else if (rate < -1)
                    {
                        Utils.Show("Stock", "VTENotification.StockFall".Translate(), "VTENotification.StockFallDesc".Translate(company.name, -rate));
                    }
                }
            }
        }
    }
}
