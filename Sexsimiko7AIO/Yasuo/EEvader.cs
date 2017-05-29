using System;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Menu.Values;
using SharpDX;
using Sexsimiko7AIO.Yasuo.EvadePlus;
using Sexsimiko7AIO.Yasuo.EvadePlus.SkillshotTypes;
using Color = System.Drawing.Color;

namespace Sexsimiko7AIO.Yasuo
{
    static class EEvader
    {
        public static int WallCastT;
        public static Vector2 YasuoWallCastedPos;
        public static int WDelay;
        public static GameObject Wall;
        public static Geometry.Polygon.Rectangle WallPolygon;
        private static int _resetWall;

        public static void Init()
        {
            GameObject.OnCreate += GameObject_OnCreate;
            Game.OnTick += delegate { UpdateTask(); };
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
            Drawing.OnDraw += Drawing_OnDraw;
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (WallPolygon != null)
            {
                WallPolygon.DrawPolygon(Color.AliceBlue);
            }
        }

        private static void GameObject_OnCreate(GameObject sender, EventArgs args)
        {
            if (System.Text.RegularExpressions.Regex.IsMatch(
                sender.Name, "_w_windwall.\\.troy",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase))
            {
                Wall = sender;
            }
        }

        private static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsValid && sender.Team == ObjectManager.Player.Team && args.SData.Name == "YasuoWMovingWall")
            {
                EEvader.YasuoWallCastedPos = sender.ServerPosition.To2D();
                _resetWall = Environment.TickCount + 4000;
            }
        }

        public static void UpdateTask()
        {
            if (EvadePlus.Program.Evade == null) return;
            if (_resetWall - Environment.TickCount > 3400 && Wall != null)
            {
                var level = Player.GetSpell(SpellSlot.W).Level;
                var wallWidth = (300 + 50 * level);
                var wallDirection = (Wall.Position.To2D() - YasuoWallCastedPos).Normalized().Perpendicular();
                var wallStart = Wall.Position.To2D() + wallWidth / 2 * wallDirection;
                var wallEnd = wallStart - wallWidth * wallDirection;
                WallPolygon = new Geometry.Polygon.Rectangle(wallStart, wallEnd, 75);
            }
            if (_resetWall < Environment.TickCount)
            {
                Wall = null;
                WallPolygon = null;
            }
            if (Wall != null && YasuoWallCastedPos.IsValid() && WallPolygon != null)
            {
                foreach (var activeSkillshot in EvadePlus.Program.Evade.SkillshotDetector.ActiveSkillshots
                    .Where(EvadeMenu.IsSkillshotW))
                {
                    if (WallPolygon.IsInside(activeSkillshot.GetPosition()))
                    {
                        activeSkillshot.IsValid = false;
                    }
                }
            }

            EvadePlus.Program.Evade.CacheSkillshots();

            if (EvadePlus.Program.Evade.IsHeroInDanger(Player.Instance))
            {
                if (YasuoConfig.EvadeW.CurrentValue &&
                    Player.GetSpell(SpellSlot.W).State == SpellState.Ready)
                {
                    foreach (var activeSkillshot in EvadePlus.Program.Evade.SkillshotDetector.ActiveSkillshots.Where(
                        a => EvadeMenu.IsSkillshotW(a) && Environment.TickCount - a.TimeDetected >=
                             YasuoConfig.EvadeWDelay.CurrentValue))
                    {
                        if (activeSkillshot.ToPolygon().IsInside(Player.Instance))
                        {
                            Player.CastSpell(SpellSlot.W, activeSkillshot.GetPosition());
                            WDelay = Environment.TickCount + 500;
                            return;
                        }
                    }
                }

                if (WDelay > Environment.TickCount) return;

                var poly = EvadePlus.Program.Evade.CustomPoly();

                if (YasuoConfig.EvadeE.CurrentValue &&
                    Player.GetSpell(SpellSlot.E).State == SpellState.Ready)
                {
                    foreach (
                        var source in
                        EntityManager.MinionsAndMonsters.EnemyMinions.Where(
                            a => a.Team != Player.Instance.Team && a.Distance(Player.Instance) < 475 && a.CanDash()))
                    {
                        if (source.GetDashPos().IsUnderTurret()) continue;
                        if (EvadePlus.Program.Evade.IsPointSafe(poly, source.GetDashPos().To2D()))
                        {
                            int count = 0;
                            for (int i = 0; i < 10; i += 47)
                            {
                                if (!EvadePlus.Program.Evade.IsPointSafe(poly,
                                    Player.Instance.Position.Extend(source.GetDashPos(), i)))
                                {
                                    count++;
                                }
                            }
                            if (count > 3) continue;
                            Player.CastSpell(SpellSlot.E, source);
                            break;
                        }
                    }
                    foreach (
                        var source in
                        EntityManager.Heroes.Enemies.Where(
                            a => a.IsEnemy && a.Distance(Player.Instance) < 475 && a.CanDash()))
                    {
                        if (source.GetDashPos().IsUnderTurret()) continue;
                        if (EvadePlus.Program.Evade.IsPointSafe(poly, source.GetDashPos().To2D()))
                        {
                            int count = 0;
                            for (int i = 0; i < 10; i += 47)
                            {
                                if (!EvadePlus.Program.Evade.IsPointSafe(poly,
                                    Player.Instance.Position.Extend(source.GetDashPos(), i)))
                                {
                                    count++;
                                }
                            }
                            if (count > 3) continue;
                            Player.CastSpell(SpellSlot.E, source);
                            break;
                        }
                    }
                }
            }
        }

        public static Vector3 GetDashPos(this Obj_AI_Base unit)
        {
            return Player.Instance.Position.Extend(Prediction.Position.PredictUnitPosition(unit, 250), 475).To3D();
        }

        public static bool CanDash(this Obj_AI_Base unit)
        {
            return !unit.HasBuff("YasuoDashWrapper");
        }
    }
}
