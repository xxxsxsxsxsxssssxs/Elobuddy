using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Constants;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using SharpDX;

namespace Sexsimiko7AIO.Lucian
{
    internal class Lucian
    {
        private static AIHeroClient Hero => Player.Instance;
        private static Spell.Skillshot QExtended, W, E, R;
        private static Spell.Targeted Q;

        private static CheckBox QInCombo,
            WInCombo,
            EInCombo,
            SafeEEnabled,
            EAntiMele,
            RInCombo,
            RLock,
            HarrasQ,
            ExtendedQHarassLC,
            ExtendedQHarassMixed,
            ExtendedQHarassCombo,
            QSpellFarm,
            WSpellFarm,
            ESpellFarm,
            QInJGClear,
            WInJGClear,
            EInJGClear;

        private static Slider RInComboPercentHP,
            RInComboDist,
            HarassQMana,
            ExtendedQHarassLCMana,
            ExtendedQHarassMixedMana,
            ExtendedQHarassComboMana,
            MinimumMinionsToQ,
            QFarmMana,
            WFarmMana,
            EFarmMana,
            QJGMana,
            WJGMana,
            EJGMana;

        private static KeyBind SpellFarmEnabled;

        protected static bool HasAnyOrbwalkerFlags
            =>
            (Orbwalker.ActiveModesFlags &
             (Orbwalker.ActiveModes.Combo | Orbwalker.ActiveModes.Harass | Orbwalker.ActiveModes.LaneClear |
              Orbwalker.ActiveModes.LastHit | Orbwalker.ActiveModes.JungleClear | Orbwalker.ActiveModes.Flee)) != 0;

        static Lucian()
        {
            Q = new Spell.Targeted(SpellSlot.Q, 675);
            QExtended = new Spell.Skillshot(SpellSlot.Q, 900, SkillShotType.Linear, 400, 1600, 65);
            W = new Spell.Skillshot(SpellSlot.W, 900, SkillShotType.Linear, 250, 1600, 80);
            E = new Spell.Skillshot(SpellSlot.E, 425, SkillShotType.Linear, 250, 1000, 425);
            R = new Spell.Skillshot(SpellSlot.R, 1200, SkillShotType.Linear, 50, 2800, 110);

            var main = MainMenu.AddMenu("S7AIO Lucian", "mikolucian");

            var comboMenu = main.AddSubMenu("++ Combo Menu");
            QInCombo = comboMenu.Add("QInCombo", new CheckBox("Use Q in Combo"));
            WInCombo = comboMenu.Add("WInCombo", new CheckBox("Use W in Combo"));
            EInCombo = comboMenu.Add("EInCombo", new CheckBox("Use E in Combo"));
            SafeEEnabled = comboMenu.Add("SafeEEnabled", new CheckBox("Enable E-To-Max-Attack-Range"));
            EAntiMele = comboMenu.Add("EAntiMele", new CheckBox("Enable Anti-Melee E"));
            RInCombo = comboMenu.Add("RInCombo", new CheckBox("Use R in Combo"));
            RInComboPercentHP = comboMenu.Add("RInComboPercentHP", new Slider("Use R on Enemy %HP", 35, 1));
            RInComboDist = comboMenu.Add("RInComboDist", new Slider("Use R Max Range", 1000, 100, 1200));
            RLock = comboMenu.Add("RLock", new CheckBox("Lock to R Target"));

            var harassMenu = main.AddSubMenu("++ Harass");
            HarrasQ = harassMenu.Add("HarrasQ", new CheckBox("Use Q for Harass"));
            HarassQMana = harassMenu.Add("HarassQMana", new Slider("Min Mana % to Q Harass", 40));
            harassMenu.AddSeparator();
            ExtendedQHarassLC = harassMenu.Add("ExtendedQHarassLC", new CheckBox("Use Extended Q Harass in LaneClear"));
            ExtendedQHarassLCMana = harassMenu.Add("ExtendedQHarassLCMana", new Slider("-> Min Mana %{0}", 40));
            harassMenu.AddSeparator();
            ExtendedQHarassMixed = harassMenu.Add("ExtendedQHarassMixed",
                new CheckBox("Use Extended Q Harass in Harass"));
            ExtendedQHarassMixedMana = harassMenu.Add("ExtendedQHarassMixedMana", new Slider("-> Min Mana %{0}", 40));
            harassMenu.AddSeparator();
            ExtendedQHarassCombo = harassMenu.Add("ExtendedQHarassCombo",
                new CheckBox("Use Extended Q Harass in Combo"));
            ExtendedQHarassComboMana = harassMenu.Add("ExtendedQHarassComboMana", new Slider("-> Min Mana %{0}", 40));

            var laneclearMenu = main.AddSubMenu("++ Lane Clear");
            SpellFarmEnabled = laneclearMenu.Add("SpellFarmEnabled",
                new KeyBind("Enable Spell Farming", false, KeyBind.BindTypes.PressToggle, 'M'));
            QSpellFarm = laneclearMenu.Add("QSpellFarm", new CheckBox("Use Q to Farm"));
            MinimumMinionsToQ = laneclearMenu.Add("MinimumMinionsToQ", new Slider("Minimum Minions to Q", 4, 1, 8));
            QFarmMana = laneclearMenu.Add("QFarmMana", new Slider("Q Farm Min Mana %{0}", 50));
            laneclearMenu.AddSeparator();
            WSpellFarm = laneclearMenu.Add("WSpellFarm", new CheckBox("Use W to Farm", false));
            WFarmMana = laneclearMenu.Add("WFarmMana", new Slider("W Farm Min Mana %{0}", 70));
            laneclearMenu.AddSeparator();
            ESpellFarm = laneclearMenu.Add("ESpellFarm", new CheckBox("Use E to Farm", false));
            EFarmMana = laneclearMenu.Add("EFarmMana", new Slider("E Farm Min Mana %{0}", 50));
            laneclearMenu.AddSeparator();
            QInJGClear = laneclearMenu.Add("QInJGClear", new CheckBox("Use Q in JG Clear"));
            QJGMana = laneclearMenu.Add("QJGMana", new Slider("Q JG Clear Min Mana %{0}", 20));
            laneclearMenu.AddSeparator();
            WInJGClear = laneclearMenu.Add("WInJGClear", new CheckBox("Use W in JG Clear"));
            WJGMana = laneclearMenu.Add("WJGMana", new Slider("W JG Clear Min Mana %{0}", 20));
            laneclearMenu.AddSeparator();
            EInJGClear = laneclearMenu.Add("EInJGClear", new CheckBox("Use E in JG Clear"));
            EJGMana = laneclearMenu.Add("EJGMana", new Slider("E JG Clear Min Mana %{0}", 50));
            laneclearMenu.AddSeparator();


            Game.OnUpdate += Game_OnUpdate;
            Obj_AI_Base.OnSpellCast += Obj_AI_Base_OnSpellCast;
            Obj_AI_Base.OnLevelUp += Obj_AI_Base_OnLevelUp;
            Obj_AI_Base.OnPlayAnimation += (sender, args) =>
            {
                if (sender.IsMe && ((args.Animation == "Spell1") || (args.Animation == "Spell2") || (args.Animation == "Spell3")) && HasAnyOrbwalkerFlags)
                {
                    Player.ForceIssueOrder(GameObjectOrder.MoveTo, Game.CursorPos, false);
                }
            };
        }

        private static void Obj_AI_Base_OnSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (Orbwalker.ActiveModesFlags == Orbwalker.ActiveModes.Combo && sender.IsMe && args.Target is AIHeroClient)
            {
                var target = (AIHeroClient) args.Target;

                if (target == null) return;

                if (CastE(target))
                {
                    Orbwalker.ResetAutoAttack();
                    return;
                }
                if (QInCombo.CurrentValue && Q.Cast((Obj_AI_Base)args.Target))
                {
                    return;
                }
                if (WInCombo.CurrentValue && W.Cast((Obj_AI_Base) args.Target))
                {
                    return;
                }
            }

            if (SpellFarmEnabled.CurrentValue && Hero.ManaPercent > EFarmMana.CurrentValue && Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear) && sender.IsMe && args.IsAutoAttack() && ((Obj_AI_Base) args.Target).IsMinion)
            {
                if (ESpellFarm.CurrentValue && E.Cast((Vector3) Hero.Position.Extend(Game.CursorPos, 10)))
                {
                    Orbwalker.ResetAutoAttack();
                    return;
                }
                if (QSpellFarm.CurrentValue && Hero.ManaPercent > QFarmMana.CurrentValue)
                {
                    var Qfarm = EntityManager.MinionsAndMonsters.GetLaneMinions(
                        EntityManager.UnitTeam.Enemy,
                        Hero.Position,
                        Q.Range);
                    if (Qfarm == null)
                        return;

                    foreach (var objAiMinion in from objAiMinion in Qfarm
                        let rectangle = new Geometry.Polygon.Rectangle(Player.Instance.Position,
                            Player.Instance.Position.Extend(objAiMinion, Player.Instance.Distance(objAiMinion) > 900 ? 900 - Player.Instance.Distance(objAiMinion) : 900).To3D(), 20)
                        let count = Qfarm.Count(minion => new Geometry.Polygon.Circle(minion.Position, objAiMinion.BoundingRadius).Points.Any(rectangle.IsInside))
                        where count >= MinimumMinionsToQ.CurrentValue
                        select objAiMinion)
                    {
                        Q.Cast(objAiMinion);
                    }
                }
                if (WSpellFarm.CurrentValue && Hero.ManaPercent > WFarmMana.CurrentValue && W.Cast((Obj_AI_Base) args.Target)) return;
            }

            if (SpellFarmEnabled.CurrentValue && Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.JungleClear) && sender.IsMe && args.IsAutoAttack() && ((Obj_AI_Base)args.Target).IsMonster)
            {
                if (EInJGClear.CurrentValue && Hero.ManaPercent > EJGMana.CurrentValue && E.Cast((Vector3) Hero.Position.Extend(Game.CursorPos, 10)))
                {
                    Orbwalker.ResetAutoAttack();
                    return;
                }
                if (QInJGClear.CurrentValue && Hero.ManaPercent > QJGMana.CurrentValue && args.Target.IsValid && Q.Cast((Obj_AI_Base) args.Target)) return;
                if (WInJGClear.CurrentValue && Hero.ManaPercent > WJGMana.CurrentValue && args.Target.IsValid && W.Cast((Obj_AI_Base) args.Target)) return;
            }
        }

        private static void Game_OnUpdate(System.EventArgs args)
        {
            if (RInCombo.CurrentValue && R.IsReady() && !Hero.HasBuff("lucianr"))
            {
                var Target = TargetSelector.GetTarget(R.Range, DamageType.Physical);

                if (Target != null && !Hero.Position.IsInRange(Target.Position, 500))
                {
                    var flDistance = (Hero.Position - Target.Position).Length();

                    if (Target.HealthPercent < RInComboPercentHP.CurrentValue && flDistance < RInComboDist.CurrentValue)
                        if (R.Cast(Target)) return;
                }
            }
            if (Hero.HasBuff("lucianr"))
            {
                Orbwalker.DisableAttacking = true;
            }
            else
            {
                Orbwalker.DisableAttacking = false;
            }

            if (RLock.CurrentValue && Hero.HasBuff("lucianr") && Orbwalker.ActiveModesFlags == Orbwalker.ActiveModes.Combo)
            {
                var LockTar = TargetSelector.GetTarget(R.Range, DamageType.Physical);

                if (LockTar != null && LockTar.IsValidTarget() && Hero.IsFacing(LockTar))
                {
                    var flDist = (Hero.Position - LockTar.Position).Length();
                    var flDistToCursor = (LockTar.Position - Game.CursorPos).Length();
                    if (flDistToCursor > flDist)
                        flDist += flDist / 3;
                    else
                        flDist -= flDist / 3;

                    var pos = Prediction.Position.PredictUnitPosition(LockTar, 110);
                    if (Player.IssueOrder(GameObjectOrder.MoveTo, (Vector3) pos.Extend(Hero.Position, flDist))) return;
                }
            }

            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear))
                LaneClear();

            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass))
                if (ExtendedQHarassMixed.CurrentValue && Hero.ManaPercent > ExtendedQHarassMixedMana.CurrentValue)
                    if (CastExtendedQ()) return;

            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
            {
                if (ExtendedQHarassCombo.CurrentValue && Hero.ManaPercent > ExtendedQHarassComboMana.CurrentValue)
                {
                    if (CastExtendedQ())
                    {
                        return;
                    }
                }
                var target = TargetSelector.GetTarget(W.Range, DamageType.Physical);

                if (target == null) return;

                if (WInCombo.CurrentValue && W.Cast(target))
                {
                    return;
                }
            }

            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear) || Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass))
                if (!Hero.HasBuff("lucianr"))
                    if (HarrasQ.CurrentValue && Hero.ManaPercent > HarassQMana.CurrentValue)
                    {
                        var target = TargetSelector.GetTarget(675, DamageType.Physical);
                        if (target == null) return;
                        if (Q.Cast(target)) return;
                    }
        }

        private static void LaneClear()
        {
            if (ExtendedQHarassLC.CurrentValue && Hero.ManaPercent > ExtendedQHarassLCMana.CurrentValue)
                if (CastExtendedQ()) return;

            if (SpellFarmEnabled.CurrentValue && Hero.ManaPercent > QFarmMana.CurrentValue)
            {
                if (QSpellFarm.CurrentValue)
                {
                    var Qfarm = EntityManager.MinionsAndMonsters.GetLaneMinions(
                        EntityManager.UnitTeam.Enemy,
                        Hero.Position,
                        Q.Range);
                    if (Qfarm == null)
                        return;

                    foreach (var objAiMinion in from objAiMinion in Qfarm
                                                let rectangle = new Geometry.Polygon.Rectangle(Player.Instance.Position,
                            Player.Instance.Position.Extend(objAiMinion, Player.Instance.Distance(objAiMinion) > 900 ? 900 - Player.Instance.Distance(objAiMinion) : 900).To3D(), 20)
                        let count = Qfarm.Count(minion => new Geometry.Polygon.Circle(minion.Position, objAiMinion.BoundingRadius).Points.Any(rectangle.IsInside))
                        where count >= MinimumMinionsToQ.CurrentValue
                        select objAiMinion)
                    {
                        Q.Cast(objAiMinion);
                    }

                }
            }
        }

        private static bool CastExtendedQ()
        {
            if (!Hero.HasBuff("lucianr"))
            {
                var QTar = TargetSelector.GetTarget(900, DamageType.Physical);
                if (QTar != null)
                {
                    var FuturePos = Prediction.Position.PredictUnitPosition(QTar, Q.CastDelay);

                    var ExtendedCastPos = Hero.Position.Extend(FuturePos, 1000);
                    foreach (var Minion in EntityManager.MinionsAndMonsters.EnemyMinions)
                    {
                        if (Minion.IsValidTarget(675))
                            if ((Hero.Position.Extend(Minion.Position, 1000) - ExtendedCastPos).Length() < 65)
                                if (Q.Cast(Minion)) return true;
                    }
                }
            }
            return false;
        }

        private static bool CastE(AIHeroClient Target)
        {
            if (!E.IsReady() || !EInCombo.CurrentValue) return false;

            var Radius2 = Hero.AttackRange + Hero.BoundingRadius;

            if (SafeEEnabled.CurrentValue && Target.IsFacing(Hero))
            {
                var CCI = Hero.Position.To2D().CirclesIntersection(200, Target.Position.To2D(), Radius2);
                if (CCI.Any())
                { 
                    foreach (var vector2 in CCI.Distinct().ToArray())
                    {
                        if (E.Cast((Vector3) Hero.Position.Extend(vector2, 200))) return true;
                    }
                }
            }

            if (EAntiMele.CurrentValue)
            {
                if (Target.IsFacing(Hero) && Hero.Position.IsInRange(Target.Position, 250))
                    if (E.Cast((Vector3) Target.Position
                        .Extend(Hero.Position, Hero.AttackRange + Hero.BoundingRadius))) return true;
            }

            if (Target.IsFacing(Hero) && Hero.IsFacing(Target))
            {
                var NewPosition = Hero.Position.Extend(Game.CursorPos, 10);
                if (Target.Position.IsInRange(NewPosition,
                    Hero.AttackRange + Hero.BoundingRadius))
                    if (E.Cast((Vector3) NewPosition)) return true;
            }

            if (E.Cast(Game.CursorPos)) return true;

            return false;
        }



        private static void Obj_AI_Base_OnLevelUp(Obj_AI_Base sender, Obj_AI_BaseLevelUpEventArgs args)
        {
            if (sender.IsMe)
            {
                switch (args.Level)
                {
                    case 1:
                        Q.CastDelay = 400;
                        break;
                    case 2:
                        Q.CastDelay = 390;
                        break;
                    case 3:
                        Q.CastDelay = 380;
                        break;
                    case 4:
                        Q.CastDelay = 370;
                        break;
                    case 5:
                        Q.CastDelay = 360;
                        break;
                    case 6:
                        Q.CastDelay = 360;
                        break;
                    case 7:
                        Q.CastDelay = 350;
                        break;
                    case 8:
                        Q.CastDelay = 340;
                        break;
                    case 9:
                        Q.CastDelay = 330;
                        break;
                    case 10:
                        Q.CastDelay = 320;
                        break;
                    case 11:
                        Q.CastDelay = 310;
                        break;
                    case 12:
                        Q.CastDelay = 300;
                        break;
                    case 13:
                        Q.CastDelay = 290;
                        break;
                    case 14:
                        Q.CastDelay = 290;
                        break;
                    case 15:
                        Q.CastDelay = 280;
                        break;
                    case 16:
                        Q.CastDelay = 270;
                        break;
                    case 17:
                        Q.CastDelay = 260;
                        break;
                    case 18:
                        Q.CastDelay = 250;
                        break;
                }
                QExtended.CastDelay = Q.CastDelay;
            }
        }
    }
}
