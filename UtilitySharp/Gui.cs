using System;
using System.Drawing;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Rendering;
using SharpDX;
using SharpDX.Direct3D9;
using Sprite = EloBuddy.SDK.Rendering.Sprite;
using Color = System.Drawing.Color;
using Font = System.Drawing.Font;
using Rectangle = SharpDX.Rectangle;
using Line = EloBuddy.SDK.Rendering.Line;

namespace UtilitySharp
{
    public class Gui
    {                             
        private CheckBox Show2DHud, Show3DHud, ShowSelf3D, ShowTeam3D, ShowEnemies3D;

        public Slider HudX, HudY;

        private Text HudLevel2, HudCd;

        private Sprite FlashSquare, HealSquare, ExhaustSquare, TeleportSquare, SmiteSquare, IgniteSquare, BarrierSquare, ClairvoyanceSquare, CleanseSquare, GhostSquare;

        private Sprite Flash, Heal, Exhaust, Teleport, Smite, Ignite, Barrier, Clairvoyance, Cleanse, Ghost, Ultimate;

        public Gui(Menu mainMenu)
        {
            if (!Program.EnableGui.CurrentValue)
            {
                return;
            }

            var menu = mainMenu.AddSubMenu("Gui");

            menu.AddGroupLabel("++ 2D Options");
            menu.AddSeparator();
            Show2DHud = menu.Add("Show2DHud", new CheckBox("Draw 2D HUD"));
            HudX = menu.Add("HudX", new Slider("X Offset", Drawing.Width - 310, 0, Drawing.Width));
            HudY = menu.Add("HudY", new Slider("Y Offset", Drawing.Height / 2 + 100, 0, Drawing.Height));

            menu.AddGroupLabel("++ 3D Options");
            menu.AddSeparator();
            Show3DHud = menu.Add("Show3DHud", new CheckBox("Draw 3D HUD"));
            ShowSelf3D = menu.Add("ShowSelf3D", new CheckBox("Draw My UI"));
            ShowTeam3D = menu.Add("ShowTeam3D", new CheckBox("Draw Team UI"));
            ShowEnemies3D = menu.Add("ShowEnemies3D", new CheckBox("Draw Enemy UI"));

            new RecallTracker().Initialize(menu);

            Flash = ImageDownloader.CreateSummonerSprite("Flash");
            Heal = ImageDownloader.CreateSummonerSprite("Heal");
            Exhaust = ImageDownloader.CreateSummonerSprite("Exhaust");
            Teleport = ImageDownloader.CreateSummonerSprite("Teleport");
            Ignite = ImageDownloader.CreateSummonerSprite("Ignite");
            Barrier = ImageDownloader.CreateSummonerSprite("Barrier");
            Clairvoyance = ImageDownloader.CreateSummonerSprite("Clairvoyance");
            Cleanse = ImageDownloader.CreateSummonerSprite("Cleanse");
            Ghost = ImageDownloader.CreateSummonerSprite("Ghost");
            Smite = ImageDownloader.CreateSummonerSprite("Smite");
            Ultimate = ImageDownloader.CreateSummonerSprite("r");

            FlashSquare = ImageDownloader.GetSprite("Flash");
            HealSquare = ImageDownloader.GetSprite("Heal");
            ExhaustSquare = ImageDownloader.GetSprite("Exhaust");
            TeleportSquare = ImageDownloader.GetSprite("Teleport");
            IgniteSquare = ImageDownloader.GetSprite("Ignite");
            BarrierSquare = ImageDownloader.GetSprite("Barrier");
            ClairvoyanceSquare = ImageDownloader.GetSprite("Clairvoyance");
            CleanseSquare = ImageDownloader.GetSprite("Cleanse");
            GhostSquare = ImageDownloader.GetSprite("Ghost");
            SmiteSquare = ImageDownloader.GetSprite("Smite");

            FlashSquare.Scale = new Vector2(0.4f, 0.4f);
            HealSquare.Scale = new Vector2(0.4f, 0.4f);
            ExhaustSquare.Scale = new Vector2(0.4f, 0.4f);
            TeleportSquare.Scale = new Vector2(0.4f, 0.4f);
            IgniteSquare.Scale = new Vector2(0.4f, 0.4f);
            BarrierSquare.Scale = new Vector2(0.4f, 0.4f);
            ClairvoyanceSquare.Scale = new Vector2(0.4f, 0.4f);
            CleanseSquare.Scale = new Vector2(0.4f, 0.4f);
            GhostSquare.Scale = new Vector2(0.4f, 0.4f);
            SmiteSquare.Scale = new Vector2(0.4f, 0.4f);

            HudCd = new Text(string.Empty, new Font("Tahoma", 7f, FontStyle.Regular));

        }

        public void onDraw(EventArgs args)
        {
            var hudSpace = 0;

            foreach (var hero in InfoLoader.ChampionInfoList.Where(x => !x.Hero.IsDead))
            {   
                if (Show2DHud.CurrentValue && hero.Hero.IsEnemy)
                {
                    Sprite sprite8 = hero.HudSprite;
                    if (!hero.Hero.IsHPBarRendered)
                    {
                        sprite8.Color = Color.DimGray;
                    }
                    else
                    {
                        sprite8.Color = Color.White;
                    }
                    Vector2 vector45 = new Vector2(HudX.CurrentValue + hudSpace, HudY.CurrentValue);
                    sprite8.Scale = new Vector2(0.33f, 0.33f);
                    sprite8.Draw(vector45 + new Vector2(-10, 14.5f));
                    Vector2 vector46 = new Vector2(vector45.X - 9f, vector45.Y + 64f);
                    Vector2 vector47 = new Vector2(vector45.X - 8f + 33f + 3f, vector45.Y + 64f);
                    EloBuddy.SDK.Rendering.Line.DrawLine(Color.DarkGoldenrod, 18f, vector46, vector47);

                    Vector2 vector48 = new Vector2(vector45.X - 8f, vector45.Y + 64f);
                    Vector2 vector49 = new Vector2(vector45.X - 8f + 33f + 2f, vector45.Y + 64f);
                    EloBuddy.SDK.Rendering.Line.DrawLine(Color.Black, 16f, vector48, vector49);

                    Color color2 = Color.LimeGreen;
                    if (hero.Hero.HealthPercent < 30f)
                    {
                        color2 = Color.OrangeRed;
                    }
                    else if (hero.Hero.HealthPercent < 50f)
                    {
                        color2 = Color.DarkOrange;
                    }
                    Vector2 vector50 = new Vector2(vector45.X - 7f, vector45.Y + 60f);
                    Vector2 vector51 = new Vector2(vector45.X - 7f + 33f * hero.Hero.HealthPercent * 0.01f, vector45.Y + 60f);
                    EloBuddy.SDK.Rendering.Line.DrawLine(color2, 7f, vector50, vector51);

                    Vector2 vector52 = new Vector2(vector45.X - 7f, vector45.Y + 68f);
                    Vector2 vector53 = new Vector2(vector45.X - 7f + 33f * hero.Hero.ManaPercent * 0.01f, vector45.Y + 68f);
                    EloBuddy.SDK.Rendering.Line.DrawLine(Color.DodgerBlue, 5f, vector52, vector53);

                    var spell4 = hero.Hero.Spellbook.Spells[3];
                    var spellDataInst = hero.Hero.Spellbook.Spells[4];
                    var spellDataInst2 = hero.Hero.Spellbook.Spells[5];

                    if (spell4 != null)
                    {
                        float num49 = spell4.CooldownExpires - Game.Time;
                        Vector2 vector54 = new Vector2(vector45.X - 2f, vector45.Y - 30f);
                        Vector2 vector55 = new Vector2(vector45.X + 9f, vector45.Y - 20f);
                        Sprite sprite9 = GetSummonerIcon("r");
                        sprite9.Scale = new Vector2(0.35f, 0.35f);
                        if (hero.Hero.Level < 6)
                        {
                            sprite9.Color = Color.DimGray;
                            sprite9.Draw(vector54);
                        }
                        else if (num49 < 0f)
                        {
                            sprite9.Color = Color.White;
                            sprite9.Draw(vector54);
                        }
                        else
                        {
                            sprite9.Color = Color.DimGray;
                            sprite9.Draw(vector54);
                            DrawFontTextScreen(HudCd, MakeNiceNumber(num49), Color.White, vector55);
                        }
                    }

                    if (spellDataInst != null)
                    {
                        float num50 = spellDataInst.CooldownExpires - Game.Time;
                        Vector2 vector56 = new Vector2(vector45.X - 13f, vector45.Y - 10f);
                        Vector2 vector57 = new Vector2(vector45.X - 1f, vector45.Y + 2f);
                        Sprite sprite10 = GetSummonerIcon(spellDataInst.Name);
                        sprite10.Scale = new Vector2(0.35f, 0.35f);
                        if (num50 < 0f)
                        {
                            sprite10.Color = Color.White;
                            sprite10.Draw(vector56);
                        }
                        else
                        {
                            sprite10.Color = Color.DimGray;
                            sprite10.Draw(vector56);
                            DrawFontTextScreen(HudCd, MakeNiceNumber(num50), Color.White, vector57);
                        }
                    }

                    if (spellDataInst2 != null)
                    {
                        float num51 = spellDataInst2.CooldownExpires - Game.Time;
                        Vector2 vector58 = new Vector2(vector45.X + 9f, vector45.Y - 10f);
                        Vector2 vector59 = new Vector2(vector45.X + 22f, vector45.Y + 2f);
                        Sprite sprite11 = GetSummonerIcon(spellDataInst2.Name);
                        sprite11.Scale = new Vector2(0.35f, 0.35f);
                        if (num51 < 0f)
                        {
                            sprite11.Color = Color.White;
                            sprite11.Draw(vector58);
                        }
                        else
                        {
                            sprite11.Color = Color.DimGray;
                            sprite11.Draw(vector58);
                            DrawFontTextScreen(HudCd, MakeNiceNumber(num51), Color.White, vector59);
                        }
                    }
                    hudSpace += 45;
                }
                if (Show3DHud.CurrentValue)
                {
                    var vector6 = hero.Hero.HPBarPosition + new Vector2(-8f, -11f);
                    var spell = hero.Hero.Spellbook.GetSpell(SpellSlot.Q);
                    var spell2 = hero.Hero.Spellbook.GetSpell(SpellSlot.W);
                    var spell3 = hero.Hero.Spellbook.GetSpell(SpellSlot.E);
                    var spell4 = hero.Hero.Spellbook.GetSpell(SpellSlot.R);
                    var spellDataInst = hero.Hero.Spellbook.Spells[4];
                    var spellDataInst2 = hero.Hero.Spellbook.Spells[5];

                    if (hero.Hero.IsHPBarRendered && hero.Hero.Position.IsOnScreen() && ((ShowTeam3D.CurrentValue && (hero.Hero.IsAlly && !hero.Hero.IsMe)) ||
                                                (ShowEnemies3D.CurrentValue && hero.Hero.IsEnemy) ||
                                                (ShowSelf3D.CurrentValue && hero.Hero.IsMe)))
                    {
                        if (hero.Hero.IsAlly)
                        {
                            vector6 += new Vector2(1f, -2f);
                        }
                        if (hero.Hero.IsMe)
                        {
                            vector6 += new Vector2(25f, 0f);
                        }
                        Line.DrawLine(Color.DimGray, 9f, vector6 + new Vector2(7f, 34f), vector6 + new Vector2(115f, 34f));
                        Line.DrawLine(Color.Black, 7f, vector6 + new Vector2(8f, 34f), vector6 + new Vector2(113f, 34f));
                        float num11 = spell.CooldownExpires - Game.Time;
                        float num12 = spell2.CooldownExpires - Game.Time;
                        float num13 = spell3.CooldownExpires - Game.Time;
                        float num14 = spell4.CooldownExpires - Game.Time;
                        float num15 = (spell4.SData.CooldownTime > 15f) ? spell4.SData.CooldownTime : 90f;
                        float num16 = Math.Max(Math.Min(num11 / spell.SData.CooldownTime, 1f), 0f);
                        float num17 = Math.Max(Math.Min(num12 / spell2.SData.CooldownTime, 1f), 0f);
                        float num18 = Math.Max(Math.Min(num13 / spell3.SData.CooldownTime, 1f), 0f);
                        float num19 = Math.Max(Math.Min(num14 / num15, 1f), 0f);
                        if (spell.Level > 0)
                        {
                            Vector2 vector7 = vector6 + new Vector2(9f, 34f);
                            Line.DrawLine(num16 > 0f ? Color.Orange : Color.YellowGreen, 5f, vector7, vector6 + new Vector2(33f - 24f * num16, 34f)); 
                            if (num16 > 0f)
                            {
                                DrawFontTextScreen(HudCd, MakeNiceNumber(num11 + 1f), Color.White, vector7 + new Vector2(14f, 11f));
                            }
                        }
                        if (spell2.Level > 0)
                        {
                            Vector2 vector8 = vector6 + new Vector2(35f, 34f);
                            Line.DrawLine(num17 > 0f ? Color.Orange : Color.YellowGreen, 5f, vector8, vector6 + new Vector2(59f - 24f * num17, 34f)); 
                            if (num17 > 0f)
                            {
                                DrawFontTextScreen(HudCd, MakeNiceNumber(num12 + 1f), Color.White, vector8 + new Vector2(14f, 11f));
                            }
                        }
                        if (spell3.Level > 0)
                        {
                            Vector2 vector9 = vector6 + new Vector2(61f, 34f);
                            Line.DrawLine((num18 > 0f) ? Color.Orange : Color.YellowGreen, 5f, vector9, vector6 + new Vector2(85f - 24f * num18, 34f));
                            if (num18 > 0f)
                            {
                                DrawFontTextScreen(HudCd, MakeNiceNumber(num13 + 1f), Color.White, vector9 + new Vector2(14f, 11f));
                            }
                        }
                        if (spell4.Level > 0)
                        {
                            Vector2 vector10 = vector6 + new Vector2(87f, 34f);
                            Line.DrawLine((num19 > 0f) ? Color.Orange : Color.YellowGreen, 5f, vector10, vector6 + new Vector2(112f - 24f * num19, 34f)); 
                            if (num19 > 0f)
                            {
                                DrawFontTextScreen(HudCd, MakeNiceNumber(num14 + 1f), Color.White, vector10 + new Vector2(14f, 11f));
                            }
                        }
                        if (spellDataInst != null)
                        {
                            Sprite sprite = GetSummonerIconSquare(spellDataInst.Name);
                            Vector2 vector11 = new Vector2(-50f, 14f);
                            if (hero.Hero.IsMe)
                            {
                                vector11 = new Vector2(117f, 14f);
                            }
                            Vector2 vector12 = vector6 + vector11;
                            float num20 = spellDataInst.CooldownExpires - Game.Time;
                            if (num20 < 0f)
                            {
                                sprite.Color = Color.White;
                                sprite.Draw(vector12);
                            }
                            else
                            {
                                sprite.Color = Color.DimGray;
                                sprite.Draw(vector12);
                                DrawFontTextScreen(HudCd, MakeNiceNumber(num20), Color.White, vector12 + new Vector2(13f, 13f));
                            }
                        }
                        if (spellDataInst2 != null)
                        {
                            Sprite sprite2 = GetSummonerIconSquare(spellDataInst2.Name);
                            Vector2 vector13 = new Vector2(-22f, 14f);
                            if (hero.Hero.IsMe)
                            {
                                vector13 = new Vector2(145f, 14f);
                            }
                            Vector2 vector14 = vector6 + vector13;
                            float num21 = spellDataInst2.CooldownExpires - Game.Time;
                            if (num21 < 0f)
                            {
                                sprite2.Color = Color.White;
                                sprite2.Draw(vector14);
                            }
                            else
                            {
                                sprite2.Color = Color.DimGray;
                                sprite2.Draw(vector14);
                                DrawFontTextScreen(HudCd, MakeNiceNumber(num21), Color.White, vector14 + new Vector2(13f, 13f));
                            }
                        }
                    }
                }
            }
        }

        private string MakeNiceNumber(float number)
        {
            var num = (int)number;
            if (num < 10)
                return " " + num;
            return num.ToString();
        }

        private void DrawFontTextScreen(Text a, string A, Color b, Vector2 B)
        {
            Rectangle rectangle = a.MeasureBounding(A, null, null);
            a.Draw(A, b, B - new Vector2(rectangle.Width, rectangle.Height) / 2f );
        }

        private Sprite GetSummonerIconSquare(string name)
        {
            var nameToLower = name.ToLower();
            if (nameToLower.Contains("flash"))
                return FlashSquare;
            else if (nameToLower.Contains("heal"))
                return HealSquare;
            else if (nameToLower.Contains("exhaust"))
                return ExhaustSquare;
            else if (nameToLower.Contains("teleport"))
                return TeleportSquare;
            else if (nameToLower.Contains("dot"))
                return IgniteSquare;
            else if (nameToLower.Contains("boost"))
                return CleanseSquare;
            else if (nameToLower.Contains("barrier"))
                return BarrierSquare;
            else if (nameToLower.Contains("haste"))
                return GhostSquare;
            else if (nameToLower.Contains("smite"))
                return SmiteSquare;
            else
                return ClairvoyanceSquare;

        }

        private Sprite GetSummonerIcon(string name)
        {
            var nameToLower = name.ToLower();
            if (nameToLower.Contains("flash"))
                return Flash;
            if (nameToLower.Contains("heal"))
                return Heal;
            if (nameToLower.Contains("exhaust"))
                return Exhaust;
            if (nameToLower.Contains("teleport"))
                return Teleport;
            if (nameToLower.Contains("dot"))
                return Ignite;
            if (nameToLower.Contains("boost"))
                return Cleanse;
            if (nameToLower.Contains("barrier"))
                return Barrier;
            if (nameToLower.Contains("haste"))
                return Ghost;
            if (nameToLower.Contains("smite"))
                return Smite;
            if (nameToLower.Contains("r"))
                return Ultimate;
            return Clairvoyance;
        }
    }
}
