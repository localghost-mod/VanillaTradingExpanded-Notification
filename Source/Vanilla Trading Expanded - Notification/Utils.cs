using Verse;
using XmlExtensions;
using RimWorld;
using VanillaTradingExpanded;
using System.Linq;

namespace VTE_Notification
{
    public static class Utils
    {
        public static string modId = "VTENotification";
        public static bool isTradingTerminalUsable()
        {
            var localMap = Find.AnyPlayerHomeMap;
            var useableTerminals = localMap.listerBuildings.allBuildingsColonist.Where(building => building is Building_TradingTerminal && (building as Building_TradingTerminal).CanUseTerminalNow);
            return useableTerminals.Any();
        }
        public static bool isHidden(string key)
        {
            return SettingsManager.GetSetting(modId, key) != "Hidden";
        }
        public static void Show(string key, string label, string text)
        {
            var value = SettingsManager.GetSetting(modId, key);
            if (value == "Message")
            {
                Messages.Message(text, MessageTypeDefOf.NeutralEvent);
            }
            else if (value == "LetterNeutral")
            {
                Find.LetterStack.ReceiveLetter(label, text, LetterDefOf.NeutralEvent);
            }
            else if (value == "LetterPositive")
            {
                Find.LetterStack.ReceiveLetter(label, text, LetterDefOf.PositiveEvent);
            }
        }
    }
}
