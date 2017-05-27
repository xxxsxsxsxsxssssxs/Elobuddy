using System;
using System.Collections.Generic;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using SharpDX;
using SharpDX.Direct3D9;
using Color = System.Drawing.Color;
using Line = EloBuddy.SDK.Rendering.Line;

namespace UtilitySharp
{
    internal class RecallTracker
    {
        public List<Recall> Recalls = new List<Recall>();

        private Font _Font;
        private Font _FontNumber;

        private CheckBox _turnOff, _recallEnemies, _recallAllies;
        private Slider _xPos, _yPos;

        public void Initialize(Menu menu)
        {
            menu.AddGroupLabel("++ Recall Tracker");
            _turnOff = menu.Add("turnoffrecalltracker", new CheckBox("Turn off recall tracker ?", false));
            menu.AddSeparator();
            _recallEnemies = menu.Add("recallEnemies", new CheckBox("Enemies recall ?"));
            _recallAllies = menu.Add("recallAllies", new CheckBox("Allies recall?", false));
            menu.AddLabel("Recall Tracker Position");
            _xPos = menu.Add("xpositionslider", new Slider("X Position?", Drawing.Width / 2, 0, Drawing.Width));
            _yPos = menu.Add("ypositionslider", new Slider("Y Position?", Drawing.Height / 2 - 100, 0, Drawing.Height));

            _Font = new Font(
                Drawing.Direct3DDevice,
                new FontDescription
                {
                    FaceName = "Tahoma",
                    Height = 18,
                    OutputPrecision = FontPrecision.Default,
                    Quality = FontQuality.ClearType,
                    Weight = FontWeight.Bold

                });

            _FontNumber = new Font(
                Drawing.Direct3DDevice,
                new FontDescription
                {
                    FaceName = "Tahoma",
                    Height = 17,
                    OutputPrecision = FontPrecision.Default,
                    Quality = FontQuality.ClearType,
                    Weight = FontWeight.Bold

                });

            Teleport.OnTeleport += Teleport_OnTeleport;
            Drawing.OnEndScene += DrawOnEnd;
        }

        private void Teleport_OnTeleport(Obj_AI_Base sender, Teleport.TeleportEventArgs args)
        {
            if (_turnOff.CurrentValue) return;

            var hero = sender as AIHeroClient;

            if (args.Type != TeleportType.Recall || hero == null) return;

            if (!_recallAllies.CurrentValue && hero.IsAlly) return;
            if (!_recallEnemies.CurrentValue && hero.IsEnemy) return;


            switch (args.Status)
            {
                case TeleportStatus.Abort:
                    foreach (var source in Recalls.Where(a => a.Unit == hero))
                    {
                        source.Abort();
                    }
                    break;
                case TeleportStatus.Start:
                    var recall = Recalls.FirstOrDefault(a => a.Unit == hero);
                    if (recall != null)
                    {
                        Recalls.Remove(recall);
                    }
                    Recalls.Add(new Recall(hero, Environment.TickCount,
                        Environment.TickCount + args.Duration, args.Duration));
                    break;
            }
        }

        public void DrawOnEnd(EventArgs args)
        {
            if (_turnOff.CurrentValue) return;

            var x = _xPos.CurrentValue;
            var y = _yPos.CurrentValue;

            var bonus = 0;
            foreach (var recall in Recalls.ToList())
            {
                Line.DrawLine(Color.GhostWhite, 18, new Vector2(x + 20, y + bonus + 33), new Vector2(x + 250, y + bonus + 33));

                Line.DrawLine(recall.IsAborted ? Color.DarkRed : BarColor(recall.PercentComplete()), 18,
                    new Vector2(x + 20, y + bonus + 33),
                    new Vector2(x + 20 + 230 * (recall.PercentComplete() / 100), y + bonus + 33));

                //Line.DrawLine(Color.Red, 18, new Vector2(x + 180, y + bonus + 33), new Vector2(x + 182, y + bonus + 33));

                _Font.DrawText(null, recall.Unit.ChampionName, x + 25, y + bonus + 23, SharpDX.Color.Black);
                _FontNumber.DrawText(null, recall.PercentComplete() + "% ", x + 203, y + bonus + 24, SharpDX.Color.Black);


                if (recall.ExpireTime < Environment.TickCount && Recalls.Contains(recall))
                {
                    Recalls.Remove(recall);
                }
                bonus += 35;
            }
        }

        private Color BarColor(float percent)
        {
            if (percent > 80)
            {
                return Color.LimeGreen;
            }
            if (percent > 60)
            {
                return Color.YellowGreen;
            }

            if (percent > 40)
            {
                return Color.Orange;
            }
            if (percent > 20)
            {
                return Color.DarkOrange;
            }
            if (percent > 1)
            {
                return Color.OrangeRed;
            }
            return Color.White;
        }

        public class Recall
        {
            public Recall(AIHeroClient unit, int recallStart, int recallEnd, int duration)
            {
                Unit = unit;
                RecallStart = recallStart;
                Duration = duration;
                RecallEnd = recallEnd;
                ExpireTime = RecallEnd + 2000;
            }

            public int RecallEnd;
            public int Duration;
            public int RecallStart;
            public int ExpireTime;
            public int CancelT;

            public AIHeroClient Unit;

            public bool IsAborted;

            public void Abort()
            {
                CancelT = Environment.TickCount;
                ExpireTime = Environment.TickCount + 2000;
                IsAborted = true;
            }

            private float Elapsed => (CancelT > 0 ? CancelT : Environment.TickCount) - RecallStart;

            public float PercentComplete()
            {
                return (float)Math.Round(Elapsed / Duration * 100) > 100 ? 100 : (float)Math.Round(Elapsed / Duration * 100);
            }
        }
    }
}