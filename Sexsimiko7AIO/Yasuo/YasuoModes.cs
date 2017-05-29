using System;
using System.Collections.Generic;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;

namespace Sexsimiko7AIO.Yasuo
{
    internal class YasuoModes
    {      
        public static void YasuoModeOnUpdate(EventArgs args)
        {       
            //// COMBO COMBO COMBO COMBO COMBO COMBO COMBO COMBO COMBO COMBO COMBO COMBO COMBO COMBO COMBO COMBO COMBO COMBO COMBO COMBO COMBO 
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
            {
                if (YasuoConfig.YasuoComboR.CurrentValue)
                {
                    var targets = EntityManager.Heroes.Enemies.Where(i => i.IsValidTarget(YasuoConfig.R.Range) && YasuoHelper.YasuoTargetIsOnAir(i));
                    foreach (var target in targets)
                    {
                        if (YasuoHelper.YasuoGEtRcount(target) >= YasuoConfig.YasuoComboRHits.CurrentValue)
                        {
                            YasuoConfig.R.Cast();
                        }
                        if (YasuoConfig.YasuoComboRWhiteList["combo" + target.NetworkId].CurrentValue
                            && YasuoConfig.YasuoComboRWhiteListHP["combohp" + target.NetworkId].CurrentValue > target.HealthPercent)
                        {
                            YasuoConfig.R.Cast();
                        }
                    }
                }
                if (!YasuoHelper.IsADCanCastSpell())
                    return;

                bool underturret = YasuoConfig.YasuoComboEUnderTurret.CurrentValue;
                // Q
                if (YasuoHelper.YasuoQStage() == 3)
                {
                    if (YasuoHelper.YasuoNotDashing())
                    {
                        YasuoHelper.YasuoCastQ();
                    }
                    else
                    {
                        var target = TargetSelector.GetTarget(YasuoHelper.YasuoGetQRange(), DamageType.Physical);
                        if (YasuoHelper.YasuoCastEOnUnit(target, underturret))
                        {
                            Core.DelayAction(() => Player.CastSpell(SpellSlot.Q), 100);
                        }
                    }
                }
                if (YasuoHelper.YasuoQStage() != 3 && YasuoHelper.YasuoNotDashing())
                {
                    YasuoHelper.YasuoCastQ();
                }
                else
                {
                    var target = TargetSelector.GetTarget(YasuoConfig.Q2.Range, DamageType.Physical);
                    if (target.IsValidTarget())
                    {
                        YasuoHelper.YasuoCastQ(target);
                    }
                    foreach (var hero in EntityManager.Heroes.Enemies.Where(x => x.IsValidTarget(100000)))
                    { 
                        YasuoHelper.YasuoCastQCircle(hero);
                    }
                }
                // E
                {
                    switch (YasuoConfig.YasuoComboEMode.CurrentValue)
                    {
                        case 0:
                            YasuoHelper.YasuoCastEFly(underturret);
                            break;
                        case 1:
                            YasuoHelper.YasuoCastETarget(underturret);
                            break;
                        default:
                            YasuoHelper.YasuoCastEMouse(underturret);
                            break;
                    }
                }
                //Qstack
                if (YasuoConfig.YasuoComboStackQ.CurrentValue && YasuoHelper.YasuoQStage() != 3)
                {
                    YasuoHelper.YasuoStackQ();
                }
            }
            //// HARASS HARASS HARASS HARASS HARASS HARASS HARASS HARASS HARASS HARASS HARASS HARASS HARASS HARASS HARASS HARASS HARASS HARASS HARASS
            if (Orbwalker.ActiveModesFlags == Orbwalker.ActiveModes.Harass)
            {
                if (!YasuoHelper.IsADCanCastSpell())
                    return;
                // Q
                if (YasuoHelper.YasuoQStage() != 3 && YasuoHelper.YasuoNotDashing() && YasuoConfig.YasuoHarassQ.CurrentValue)
                {
                    YasuoHelper.YasuoCastQ();
                }
                else if (YasuoHelper.YasuoQStage() == 3 && YasuoHelper.YasuoNotDashing() && YasuoConfig.YasuoHarassQ3.CurrentValue)
                {
                    var target = TargetSelector.GetTarget(YasuoConfig.Q2.Range, DamageType.Physical);
                    if (target.IsValidTarget())
                    {
                        YasuoHelper.YasuoCastQ(target);
                    }
                }
            }
            //// FARM FARM FARM FARM FARM FARM FARM FARM FARM FARM FARM FARM FARM FARM FARM FARM FARM FARM 
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LastHit))
            {
                bool underturret = YasuoConfig.YasuoFarmUnderTurret.CurrentValue;
                var Minions = EntityManager.MinionsAndMonsters.EnemyMinions.Where(i => i.IsValidTarget(YasuoConfig.E.Range));
                foreach (var minion in Minions)
                {
                    if (YasuoConfig.YasuoLastHitQ.CurrentValue && YasuoConfig.Q.IsReady() && ObjectManager.Player.GetSpellDamage(minion, SpellSlot.Q) > minion.Health)
                    {
                        YasuoHelper.YasuoCastQ(minion);
                    }
                    if (YasuoConfig.YasuoLastHitE.CurrentValue && YasuoConfig.E.IsReady() && YasuoHelper.YasuoGetEDamage(minion) > minion.Health)
                    {
                        YasuoHelper.YasuoCastEOnUnit(minion, underturret);
                    }
                }
            }

            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear))
            {
                bool underturret = YasuoConfig.YasuoFarmUnderTurret.CurrentValue;
                //lane
                if (YasuoConfig.YasuoLaneClearQ.CurrentValue && YasuoConfig.YasuoLaneClearE.CurrentValue && YasuoConfig.E.IsReady() && YasuoConfig.Q.IsReady())
                {
                    var minions = EntityManager.MinionsAndMonsters.EnemyMinions
                        .Where(x => x.IsValidTarget());
                    if (!minions.Any())
                    {
                        return;
                    }
                    foreach (var minion in minions)
                    {
                        if (YasuoHelper.YasuoGetDashEnd(minion).CountEnemyMinionsInRange(250) >= 3 && YasuoHelper.YasuoGetDashEnd(minion).CountEnemyChampionsInRange(450) == 0)
                        {
                            if (YasuoHelper.YasuoCastEOnUnit(minion, underturret))
                            {
                                Core.DelayAction(() => Player.CastSpell(SpellSlot.Q), 100);
                            }
                        }
                    }

                }
                if (YasuoConfig.YasuoLaneClearQ.CurrentValue && YasuoConfig.Q.IsReady())
                {
                    var minions = EntityManager.MinionsAndMonsters.EnemyMinions.Where(x => x.IsValidTarget(YasuoHelper.YasuoGetQRange()));
                    var objAiMinions = minions as IList<Obj_AI_Minion> ?? minions.ToList();
                    if (objAiMinions.Any() && objAiMinions.Count >= 2)
                    {
                        var minionPos = objAiMinions.Select(x => x.Position.To2D()).ToList();
                        var farm = MinionManager.GetBestLineFarmLocation(minionPos, 10, YasuoHelper.YasuoGetQRange());
                        if (farm.MinionsHit >= 2)
                        {
                            if (!YasuoHelper.YasuoNotDashing())
                                return;
                            if (YasuoHelper.YasuoQStage() == 3)
                            {
                                YasuoConfig.Q2.Cast(farm.Position);
                            }
                            YasuoConfig.Q.Cast(farm.Position);
                        }
                    }
                }
                if (YasuoConfig.YasuoLaneClearE.CurrentValue && YasuoConfig.E.IsReady())
                {
                    var minions = EntityManager.MinionsAndMonsters.EnemyMinions
                        .Where(x => x.IsEnemy && x.IsValidTarget(YasuoConfig.E.Range));
                    foreach (var minion in minions)
                    {              
                        if (YasuoHelper.YasuoGetEDamage(minion) > minion.Health)
                        {
                            YasuoHelper.YasuoCastEOnUnit(minion, underturret);
                        }
                    }
                }
            }

            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.JungleClear))
            {
                // jungle
                bool underturret = YasuoConfig.YasuoFarmUnderTurret.CurrentValue;
                if (YasuoConfig.YasuoJungleClearQ.CurrentValue || YasuoConfig.YasuoJungleClearE.CurrentValue)
                {
                    var Minions = EntityManager.MinionsAndMonsters.GetJungleMonsters(Player.Instance.Position, YasuoHelper.YasuoGetQRange());
                    foreach (var minion in Minions)
                    {
                        if (YasuoConfig.YasuoJungleClearQ.CurrentValue)
                        {
                            YasuoHelper.YasuoCastQ(minion);
                        }
                        if (YasuoConfig.YasuoJungleClearE.CurrentValue)
                        {
                            YasuoHelper.YasuoCastEOnUnit(minion, underturret);
                        }
                    }
                }
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Flee))
            {
                YasuoHelper.YasuoCastEFlee();
                if (YasuoConfig.YasuoFleeStackQ.CurrentValue)
                    YasuoHelper.YasuoStackQ();
            }

            {
                if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo) || Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Flee))
                    return;
                if (YasuoConfig.YasuoAutoQUnderTurret.CurrentValue || !Player.Instance.Position.IsUnderTurret())
                {
                    if (YasuoHelper.YasuoQStage() != 3 && YasuoConfig.YasuoAutoQ.CurrentValue)
                    {
                        YasuoHelper.YasuoCastQ();
                    }
                    if (YasuoHelper.YasuoQStage() == 3 && YasuoConfig.YasuoAutoQ3.CurrentValue)
                    {
                        YasuoHelper.YasuoCastQ();
                    }
                }
            }
        }     
    }
}
