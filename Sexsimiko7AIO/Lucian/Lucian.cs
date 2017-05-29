using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;

namespace Sexsimiko7AIO.Lucian
{
    internal static class Lucian
    {
        public static Spell.Skillshot QExtended, W, E, R;
        public static Spell.Targeted Q;
        public static CheckBox QInCombo, WInCombo, EInCombo, SafeEEnabled, EAntiMele, RInCombo, RLock;
        public static Slider RInComboPercentHP, RInComboDist;
        static Lucian()
        {
            Q = new Spell.Targeted(SpellSlot.Q, 675);
            QExtended = new Spell.Skillshot(SpellSlot.Q, 900, SkillShotType.Linear, 400, 1600, 65);
            W = new Spell.Skillshot(SpellSlot.W, 900, SkillShotType.Linear, 250, 1600, 80);
            E = new Spell.Skillshot(SpellSlot.E, 425, SkillShotType.Linear, 250, 1000, 425);
            R = new Spell.Skillshot(SpellSlot.R, 1200, SkillShotType.Linear, 50, 2800, 110);

            var main = MainMenu.AddMenu("Sexsimiko7AIO Lucian", "mikolucian");

            var comboMenu = main.AddSubMenu("++ Combo Menu");
            QInCombo = comboMenu.Add("QInCombo", new CheckBox("Use Q in Combo"));
            WInCombo = comboMenu.Add("WInCombo", new CheckBox("Use W in Combo"));
            EInCombo = comboMenu.Add("EInCombo", new CheckBox("Use E in Combo"));
            SafeEEnabled = comboMenu.Add("SafeEEnabled", new CheckBox("Enable E-To-Max-Attack-Range"));
            EAntiMele = comboMenu.Add("EAntiMele", new CheckBox("Enable Anti-Melee E"));
            RInCombo = comboMenu.Add("RInCombo", new CheckBox("Use R in Combo"));



            Obj_AI_Base.OnLevelUp += Obj_AI_Base_OnLevelUp;
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
