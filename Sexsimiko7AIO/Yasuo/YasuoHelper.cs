
using System;
using System.Collections.Generic;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using System.Linq;
using SharpDX;

namespace Sexsimiko7AIO.Yasuo
{
    public class YasuoHelper
    {
        public static bool IsADCanCastSpell(bool anymode = false)
        {
            if (Orbwalker.ActiveModesFlags == Orbwalker.ActiveModes.Combo || Orbwalker.ActiveModesFlags == Orbwalker.ActiveModes.Harass || anymode)
            {
                if (ObjectManager.Player.Position.CountEnemyChampionsInRange(ObjectManager.Player.AttackRange + ObjectManager.Player.BoundingRadius * 2) == 0)
                    return true;
                return Orbwalker.CanMove && !Orbwalker.CanAutoAttack;
            }
            return Orbwalker.CanMove;
        }

        public static bool YasuoIsDashing()
        {
            return Environment.TickCount - YasuoVariables.YasuoLastETick <= 420 - Game.Ping;
        }

        public static bool YasuoNotDashing()
        {
            return Environment.TickCount - YasuoVariables.YasuoLastETick > 500 - Game.Ping;
        }

        public static int YasuoQStage()
        {
            if (ObjectManager.Player.Spellbook.GetSpell(SpellSlot.Q).Level == 0) return 1;
            var name = ObjectManager.Player.Spellbook.GetSpell(SpellSlot.Q).Name;
            var Q1 = "YasuoQW";
            var Q2 = "YasuoQ2W";
            return name == Q1 ? 1 : name == Q2 ? 2 : 3;
        }

        public static bool YasuoCastQ(Obj_AI_Base target)
       {
           if (!YasuoNotDashing())
               return false;
           if (YasuoQStage() == 3)
           {
               return YasuoConfig.Q2.CastMinimumHitchance(target, YasuoConfig.YasuoComboQPercent.CurrentValue);
           }
           return YasuoConfig.Q.Cast(target);
       }

        public static bool YasuoCastQ()
        {
            if (!YasuoNotDashing())
                return false;
            var enemies = EntityManager.Heroes.Enemies.OrderBy(x => x.Health).Where(x => x.IsValidTarget(100000));
            foreach (var target in enemies)
            {
                if (YasuoQStage() == 3)
                {
                    if (YasuoConfig.Q2.CastMinimumHitchance(target, YasuoConfig.YasuoComboQPercent.CurrentValue)) return true;
                }
                if (YasuoConfig.Q.Cast(target)) return true;
            }
            return false;
        }

        public static bool YasuoCastQCircle(AIHeroClient target)
        {
            if (!YasuoIsDashing())
                return false;
            var pred = Prediction.Position.PredictUnitPosition(target,
                (YasuoVariables.YasuoDashData.EndTick - Environment.TickCount - Game.Ping) / 1000);
            if (pred.Distance(YasuoVariables.YasuoDashData.EndPos) <= 100 + target.BoundingRadius)
            {
                return YasuoConfig.Q.Cast(target);
            }
            return false;
        }

        public static float YasuoGetQRange()
        {
            return YasuoQStage() == 3 ? YasuoConfig.Q2.Range : YasuoConfig.Q.Range;
        }

        public static List<Obj_AI_Base> YasuoAllETargets(bool checkbuff = true)
        {
            var targets = new List<Obj_AI_Base>();
            targets.AddRange(EntityManager.Heroes.Enemies.Where(x => x.IsValidTarget(YasuoConfig.E.Range) && !x.IsDead));
            targets.AddRange(EntityManager.MinionsAndMonsters.Minions.Where(x => x.IsEnemy && !x.IsDead && !x.IsWard() && x.IsValidTarget(YasuoConfig.E.Range) && !x.Name.Contains("barrel")));
            if (checkbuff)
            {
                targets.RemoveAll(x => x.HasBuff("YasuoDashWrapper"));
            }
            return targets;
        }

        public static Vector2 YasuoGetDashEnd(Obj_AI_Base target)
        {
            var pred = Prediction.Position.PredictUnitPosition(target, 0);
            return ObjectManager.Player.Position.Extend(pred, 475);
        }

        public static void YasuoCastETarget(bool Underturret)
        {
            if (!IsADCanCastSpell())
                return;
            var target = TargetSelector.GetTarget(YasuoConfig.E.Range + 200, DamageType.Physical);
            if (target.IsValidTarget())
            {
                float range = ObjectManager.Player.AttackRange + ObjectManager.Player.BoundingRadius + target.BoundingRadius;
                var etargets = Underturret
                    ? YasuoAllETargets()
                    : YasuoAllETargets().Where(x => !YasuoGetDashEnd(x).IsUnderTurret(true) && !x.IsDead)
                        .Where(i => Prediction.Position.PredictUnitPosition(target, 450).Distance(YasuoGetDashEnd(i)) <=
                                    range || target.Distance(YasuoGetDashEnd(i)) <= range).ToList();
                if (etargets.Any())
                {
                    var etarget = etargets.FirstOrDefault();
                    YasuoConfig.E.Cast(etarget);
                }
            }
        }

        public static void YasuoCastEFly(bool Underturret)
        {
            if (!IsADCanCastSpell())
                return;
            foreach (var target in EntityManager.Heroes.Enemies.Where(x => x.IsValidTarget(10000) && !x.IsDead))
            {
                float range = ObjectManager.Player.AttackRange + ObjectManager.Player.BoundingRadius + target.BoundingRadius;
                var etargets = Underturret
                    ? YasuoAllETargets()
                    : YasuoAllETargets().Where(i => !YasuoGetDashEnd(i).IsUnderTurret(true) && !i.IsDead)
                        .Where(i2 => Prediction.Position.PredictUnitPosition(target, 450)
                                         .Distance(YasuoGetDashEnd(i2)) <= range ||
                                     target.Distance(YasuoGetDashEnd(i2)) <= range);
                var objAiBases = etargets as Obj_AI_Base[] ?? etargets.ToArray();
                if (objAiBases.Any())
                {
                    var etarget = objAiBases.FirstOrDefault();
                    YasuoConfig.E.Cast(etarget);
                }
            }
        }

        public static void YasuoCastEMouse(bool Underturret)
        {
            if (!IsADCanCastSpell())
                return;
            var etargets = Underturret
                ? YasuoAllETargets()
                : YasuoAllETargets().Where(i => !YasuoGetDashEnd(i).IsUnderTurret(true) && !i.IsDead)
                    .Where(i2 => YasuoGetDashEnd(i2).Distance(Game.CursorPos) <
                                 ObjectManager.Player.Distance(Game.CursorPos - 100));
            var objAiBases = etargets as IList<Obj_AI_Base> ?? etargets.ToList();
            var aiBases = objAiBases.OrderBy(i => YasuoGetDashEnd(i).Distance(Game.CursorPos));
            if (aiBases.Any())
            {
                var Etarget = aiBases.FirstOrDefault();
                YasuoConfig.E.Cast(Etarget);
            }
        }

        public static void YasuoCastEFlee()
        {
            var etargets = YasuoAllETargets().Where(i2 => YasuoGetDashEnd(i2).Distance(Game.CursorPos) <
                                                          ObjectManager.Player.Distance(Game.CursorPos - 100) && !i2.IsDead);
            var objAiBases = etargets as IList<Obj_AI_Base> ?? etargets.ToList();
            var aiBases = objAiBases.OrderBy(i => YasuoGetDashEnd(i).Distance(Game.CursorPos));
            if (aiBases.Any())
            {
                var etarget = aiBases.FirstOrDefault();
                YasuoConfig.E.Cast(etarget);
            }
        }

        public static bool YasuoCastEOnUnit(Obj_AI_Base target, bool Underturret)
        {
            if (!YasuoGetDashEnd(target).IsUnderTurret(true) || Underturret)
            {
                YasuoConfig.E.Cast(target);
                return true;
            }
            return false;
        }

        public static void YasuoStackQ()
        {
            if (!YasuoNotDashing())
                return;
            if (ObjectManager.Player.Position.CountEnemyChampionsInRange(YasuoConfig.E.Range) == 0 && YasuoQStage() != 3)
            {
                foreach (var target in YasuoAllETargets(false))
                {
                    if (YasuoConfig.Q.Cast(target))
                        return;
                }
            }
        }

        public static bool YasuoTargetIsOnAir(AIHeroClient target)
        {
            return target.HasBuffOfType(BuffType.Knockback) || target.HasBuffOfType(BuffType.Knockup);
        }

        public static int YasuoGEtRcount(AIHeroClient target)
        {
            return EntityManager.Heroes.Enemies.Count(x => x.IsValidTarget(YasuoConfig.R.Range) && x.Distance(target) <= 400 && YasuoTargetIsOnAir(x));
        }

        public static float YasuoGetEDamage(Obj_AI_Base target)
        {
            var stacksPassive = Player.Instance.Buffs.Find(b => b.DisplayName.Equals("YasuoDashScalar"));
            var stacks = 1 + 0.25 * ((stacksPassive != null) ? stacksPassive.Count : 0);
            return Player.Instance.CalculateDamageOnUnit(target, DamageType.Magical,
                (float)(new double[] { 60, 70, 80, 90, 100 }[Player.GetSpell(SpellSlot.E).Level - 1] + 0.2 * ObjectManager.Player.FlatPhysicalDamageMod * stacks
                        + 0.6 * (Player.Instance.FlatMagicDamageMod)));
        }
    }
}
