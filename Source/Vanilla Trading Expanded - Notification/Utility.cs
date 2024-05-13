using System.Linq;
using RimWorld;
using VanillaTradingExpanded;
using Verse;

namespace VTE_Notification
{
    static class Utility
    {
        public static bool IsTradingTerminalWorking =>
            Find.AnyPlayerHomeMap.listerBuildings.allBuildingsColonist.Any(building => building is Building_TradingTerminal terminal && terminal.CanUseTerminalNow);

        public static void Message(string key, string label, string text)
        {
            var type = VTENotification.settings.messageTypes[key];
            if (type == MessageType.Message)
                Messages.Message(text, MessageTypeDefOf.NeutralEvent);
            if (type == MessageType.NeutralLetter)
                Find.LetterStack.ReceiveLetter(label, text, LetterDefOf.NeutralEvent);
            if (type == MessageType.PositiveLetter)
                Find.LetterStack.ReceiveLetter(label, text, LetterDefOf.PositiveEvent);
        }
    }
}
