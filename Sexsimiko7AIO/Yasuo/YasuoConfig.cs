using System.Collections.Generic;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;

namespace Sexsimiko7AIO.Yasuo
{
    internal class YasuoConfig
    {
        public static Spell.Skillshot Q, Q2;
        public static Spell.SpellBase E, R;

        public static Dictionary<string, CheckBox> YasuoComboRWhiteList = new Dictionary<string, CheckBox>();

        public static Dictionary<string, Slider> YasuoComboRWhiteListHP = new Dictionary<string, Slider>();

        public static CheckBox YasuoComboStackQ,
            YasuoComboEUnderTurret,
            YasuoComboR,
            YasuoHarassQ,
            YasuoHarassQ3,
            YasuoLaneClearQ,
            YasuoLaneClearE,
            YasuoJungleClearQ,
            YasuoJungleClearE,
            YasuoLastHitQ,
            YasuoLastHitE,
            YasuoFarmUnderTurret,
            YasuoAutoQ,
            YasuoAutoQ3,
            YasuoAutoQUnderTurret,
            YasuoFleeStackQ;

        public static Slider YasuoComboRHits;

        public static ComboBox YasuoComboEMode;

        public static void YasuoMenuAndSpells()
        {   
            Q = new Spell.Skillshot(SpellSlot.Q, 450, SkillShotType.Circular, 300, int.MaxValue, 10);
            Q2 = new Spell.Skillshot(SpellSlot.Q, 1050, SkillShotType.Linear, 500, 1200, 10);
            E = new Spell.Targeted(SpellSlot.E, 475);
            R = new Spell.Active(SpellSlot.R, 1200);

            var mainMenu = MainMenu.AddMenu("Sexsimiko7AIO Yasuo", "sexsimiko7aioyasuo");
            var comboMenu = mainMenu.AddSubMenu("Combo Settings");
            YasuoComboStackQ = comboMenu.Add("YasuoComboStackQ", new CheckBox("Stack Q"));
            YasuoComboEMode = comboMenu.Add("YasuoComboEMode", new ComboBox("E Mode", 0, "Fly", "Stick", "Mouse"));
            YasuoComboEUnderTurret = comboMenu.Add("YasuoComboEUnderTurret", new CheckBox("E Under Turret"));
            YasuoComboR = comboMenu.Add("YasuoComboR", new CheckBox("R"));
            YasuoComboRHits = comboMenu.Add("YasuoComboRHits", new Slider("R if will hit", 3, 1, 5));
            foreach (var enemy in EntityManager.Heroes.Enemies)
            {
                var checkboxtext = "R on " + enemy.ChampionName + " (" + enemy.Name + ")";
                YasuoComboRWhiteList["combo" + enemy.NetworkId] =
                    comboMenu.Add("combo" + enemy.NetworkId, new CheckBox(checkboxtext));
                YasuoComboRWhiteListHP["combohp" + enemy.NetworkId] =
                    comboMenu.Add("combohp" + enemy.NetworkId, new Slider("if %hp <", 101, 0, 101));
            }

            var harasMenu = mainMenu.AddSubMenu("Harass Settings");
            YasuoHarassQ = harasMenu.Add("YasuoHarassQ", new CheckBox("Q "));
            YasuoHarassQ3 = harasMenu.Add("YasuoHarassQ3", new CheckBox("Q3 "));

            var farmMenu = mainMenu.AddSubMenu("Farm Settings");
            YasuoLaneClearQ = farmMenu.Add("YasuoLaneClearQ", new CheckBox("LaneClear Q"));
            YasuoLaneClearE = farmMenu.Add("YasuoLaneClearE", new CheckBox("LaneClear E"));
            YasuoJungleClearQ = farmMenu.Add("YasuoJungleClearQ", new CheckBox("Jungle Q"));
            YasuoJungleClearE = farmMenu.Add("YasuoJungleClearE", new CheckBox("Jungle E"));
            YasuoLastHitQ = farmMenu.Add("YasuoLastHitQ", new CheckBox("LastHit Q"));
            YasuoLastHitE = farmMenu.Add("YasuoLastHitE", new CheckBox("LastHit E"));
            YasuoFarmUnderTurret = farmMenu.Add("YasuoFarmUnderTurret", new CheckBox("E Under Turret", false));

            var autoMenu = mainMenu.AddSubMenu("Auto Settings");
            YasuoAutoQ = autoMenu.Add("YasuoAutoQ", new CheckBox("Q "));
            YasuoAutoQ3 = autoMenu.Add("YasuoAutoQ3", new CheckBox("Q3 "));
            YasuoAutoQUnderTurret = autoMenu.Add("YasuoAutoQUnderTurret", new CheckBox("Under Turret", false));

            var fleeMenu = mainMenu.AddSubMenu("Flee Settings");
            YasuoFleeStackQ = fleeMenu.Add("YasuoFleeStackQ", new CheckBox("Stack Q"));
        }
    }
}
