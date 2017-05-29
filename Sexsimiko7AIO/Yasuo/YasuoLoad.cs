using System;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Events;

namespace Sexsimiko7AIO.Yasuo
{
    internal class YasuoLoad
    {
        static YasuoLoad()
        {
            YasuoConfig.YasuoMenuAndSpells();
            Game.OnUpdate += YasuoOnGameUpdate;
            Obj_AI_Base.OnSpellCast += YasuoOnSpellCast;
            Dash.OnDash += YasuoOnDash;
            Obj_AI_Base.OnPlayAnimation += YasuoOnPlayAnimation;
        }

        private static void YasuoOnPlayAnimation(Obj_AI_Base sender, GameObjectPlayAnimationEventArgs args)
        {
            YasuoVariables.YasuoVariableOnPlayAnimation(sender, args);
        }

        private static void YasuoOnDash(Obj_AI_Base sender, Dash.DashEventArgs e)
        {
            YasuoVariables.YasuoVariableOnDash(sender, e);
        }

        private static void YasuoOnSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            YasuoVariables.YasuoVariableOnSpellCast(sender, args);
        }

        private static void YasuoOnGameUpdate(EventArgs args)
        {
            YasuoModes.YasuoModeOnUpdate(args);
        }
    }
}
