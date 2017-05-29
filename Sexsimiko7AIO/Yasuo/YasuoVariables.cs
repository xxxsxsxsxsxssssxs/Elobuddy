using System;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Events;

namespace Sexsimiko7AIO.Yasuo
{
    internal class YasuoVariables
    {
        public static int YasuoLastETick;
        public static Dash.DashEventArgs YasuoDashData;

        public static void YasuoVariableOnSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            var caster = sender as AIHeroClient;
            if (caster != ObjectManager.Player)
                return;
            if (args.Slot == SpellSlot.E)
            {
                YasuoLastETick = Environment.TickCount;
            }
        }

        public static void YasuoVariableOnDash(Obj_AI_Base sender, Dash.DashEventArgs e)
        {
            var source = sender as AIHeroClient;

            if (source != ObjectManager.Player)
                return;

            YasuoDashData = e;
        }

        public static void YasuoVariableOnPlayAnimation(Obj_AI_Base sender, GameObjectPlayAnimationEventArgs args)
        {
            var source = sender as AIHeroClient;
            if (source != ObjectManager.Player)
                return;
            if (args.Animation.Contains("Spell4"))
            {
                Orbwalker.DisableAttacking = true;
                Core.DelayAction(() => Orbwalker.DisableAttacking = false, 500);
            }
        }
    }
}
