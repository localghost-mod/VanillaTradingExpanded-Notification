using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace VTE_Notification
{
    public enum MessageType
    {
        Hidden,
        Message,
        NeutralLetter,
        PositiveLetter
    }

    public class Settings : ModSettings
    {
        static readonly List<string> keys = new List<string> { "NewContract", "EndContract", "News", "Stock" };
        public Dictionary<string, MessageType> messageTypes = keys.ToDictionary(key => key, key => MessageType.Hidden);
        public bool highlightCompleteContract;

        public void DoWindowContents(Rect inRect)
        {
            var height = 28f;
            var ls = new Listing_Standard();
            ls.Begin(inRect);
            foreach (var key in keys)
            {
                var rowRect = ls.GetRect(height);
                var row = new WidgetRow(rowRect.x, rowRect.y, UIDirection.RightThenDown, ls.ColumnWidth);
                row.Label($"VTENotification.{key}".Translate());
                var rowRight = new WidgetRow(ls.ColumnWidth, row.FinalY, UIDirection.LeftThenDown);
                if (rowRight.ButtonText($"VTENotification.{Enum.GetName(typeof(MessageType), messageTypes.GetValueSafe(key))}".Translate(), fixedWidth: 120))
                    Find.WindowStack.Add(
                        new FloatMenu(
                            Enum.GetValues(typeof(MessageType))
                                .Cast<MessageType>()
                                .Select(
                                    type => new FloatMenuOption($"VTENotification.{Enum.GetName(typeof(MessageType), type)}".Translate(), () => messageTypes.SetOrAdd(key, type))
                                )
                                .ToList()
                        )
                    );
            }
            {
                var rowRect = ls.GetRect(height);
                var row = new WidgetRow(rowRect.x, rowRect.y, UIDirection.RightThenDown, ls.ColumnWidth);
                row.Label("Transmog.DisplayAllStyles".Translate());
                var rowRight = new WidgetRow(ls.ColumnWidth, row.FinalY, UIDirection.LeftThenDown);
                if (rowRight.ButtonIcon(highlightCompleteContract ? Widgets.CheckboxOnTex : Widgets.CheckboxOffTex))
                    highlightCompleteContract = !highlightCompleteContract;
            }
            ls.End();
        }
        public override void ExposeData()
        {
            Scribe_Collections.Look(ref messageTypes, "messageTypes");
            Scribe_Values.Look(ref highlightCompleteContract, "highlightCompleteContract");
        }
    }

    class VTENotification : Mod
    {
        public static Settings settings;

        public VTENotification(ModContentPack content)
            : base(content) => settings = GetSettings<Settings>();

        public override void DoSettingsWindowContents(Rect inRect) => settings.DoWindowContents(inRect);

        public override string SettingsCategory() => "VTENotification".Translate();
    }
}
