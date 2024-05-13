using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using VanillaTradingExpanded;
using Verse;

namespace VTE_Notification
{
    [StaticConstructorOnStartup]
    public static class Startup
    {
        static Startup() => new Harmony("localghost.vtenotification").PatchAll();
    }

    [HarmonyPatch("VanillaTradingExpanded.TradingManager", "GenerateRandomContract")]
    public class GenerateRandomContractPatch
    {
        private static void Postfix(Contract __result)
        {
            Log.Message($"{__result.FoundItemsInMap(Find.AnyPlayerHomeMap).Sum(thing => thing.stackCount)} {__result.amount}");
            if (Utility.IsTradingTerminalWorking)
                Utility.Message(
                    "NewContract",
                    "VTENotification.NewContract",
                    "VTENotification.NewContractDesc".Translate(__result.Name.Translate().Colorize(
                            VTENotification.settings.highlightCompleteContract && __result.FoundItemsInMap(Find.AnyPlayerHomeMap).Sum(thing => thing.stackCount) >= __result.amount
                                ? Color.green
                                : Color.white
                        )
                        , __result.reward, __result.reward / __result.BaseMarketValue * 100f)
                );
        }
    }

    [HarmonyPatch("VanillaTradingExpanded.TradingManager", "ProcessContracts")]
    public class ProcessContractsPatch
    {
        private static void Prefix(TradingManager __instance, Map localMap, out IEnumerable<Tuple<string, int, float>> __state)
        {
            __state = new List<Tuple<string, int, float>>();
            if (Utility.IsTradingTerminalWorking)
                __state = __instance.npcSubmittedContracts.Select(contract => Tuple.Create(contract.Name, contract.reward, contract.reward / contract.BaseMarketValue * 100f));
        }

        private static void Postfix(TradingManager __instance, ref List<Tuple<string, int, float>> __state)
        {
            if (Utility.IsTradingTerminalWorking)
            {
                var __current_state = __instance.npcSubmittedContracts.Select(
                    contract => Tuple.Create(contract.Name, contract.reward, contract.reward / contract.BaseMarketValue * 100f)
                );
                if (__state != __current_state)
                    foreach (var contract in __state.Except(__current_state))
                        Utility.Message(
                            "EndContract",
                            "VTENotification.EndContract".Translate(),
                            "VTENotification.EndContractDesc".Translate(contract.Item1, contract.Item2.ToString())
                        );
            }
        }
    }

    [HarmonyPatch("VanillaTradingExpanded.TradingManager", "CreateNews")]
    public class CreateNewsPatch
    {
        private static void Postfix(ref News __result)
        {
            if (Utility.IsTradingTerminalWorking)
                Utility.Message("News", "VTENotification.News".Translate(), __result.text);
        }
    }

    [HarmonyPatch("VanillaTradingExpanded.TradingManager", "DoPriceImpactsFromNews")]
    public class DoPriceImpactsFromNewsPatch
    {
        private static void Prefix(TradingManager __instance, out Dictionary<Company, float> __state)
        {
            __state = new Dictionary<Company, float>();
            if (Utility.IsTradingTerminalWorking)
                __state = __instance.companies.ToDictionary(company => company, company => company.currentValue);
        }

        private static void Postfix(TradingManager __instance, ref Dictionary<Company, float> __state)
        {
            if (Utility.IsTradingTerminalWorking)
                foreach (var company in __instance.companies)
                    if (company.currentValue != __state[company])
                    {
                        var rate = (company.currentValue - __state[company]) / __state[company] * 100f;
                        if (rate > 1)
                            Utility.Message("Stock", "VTENotification.StockRise".Translate(), "VTENotification.StockRiseDesc".Translate(company.name, rate));
                        if (rate < -1)
                            Utility.Message("Stock", "VTENotification.StockFall".Translate(), "VTENotification.StockFallDesc".Translate(company.name, -rate));
                    }
        }
    }
}
