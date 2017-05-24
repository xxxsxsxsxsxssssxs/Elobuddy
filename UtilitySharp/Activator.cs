using System;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using SharpDX;
using Color = System.Drawing.Color;

namespace UtilitySharp
{
    class Activator
    {
        private AIHeroClient Hero => ObjectManager.Player;

        #region Items

        public Item QSS { get; private set; }
        public Item Scimitar { get; private set; }
        public Item Mikaels { get; private set; }
        public Item HealthPotion { get; private set; }
        public Item RefillablePotion { get; private set; }
        public Item HuntersPotion { get; private set; }
        public Item CorruptingPotion { get; private set; }
        public Item Biscuit { get; private set; }
        public Item FaceOfTheMountain { get; private set; }
        public Item Redemption { get; private set; }
        public Item Zhonyas { get; private set; }
        public Item Seraphs { get; private set; }
        public Item Locket { get; private set; }
        public Item Randuins { get; private set; }
        public Item Gunblade { get; private set; }
        public Item Botrk { get; private set; }
        public Item Cutlass { get; private set; }
        public Item Youmuus { get; private set; }
        public Item GLP800 { get; private set; }
        public Item Tiamat { get; private set; }
        public Item Ravenous { get; private set; }
        public Item Titanic { get; private set; }
        public int HumanizeDelayCleanse { get; private set; }
        public int DelayedSpellIndex { get; private set; }

        #endregion

        #region MenuItems

        private CheckBox CleanseActive,
            CleanseCharm,
            CleanseDisarm,
            CleanseFear,
            CleanseFlee,
            CleansePolymorph,
            CleanseSnare,
            CleanseStun,
            CleanseExhaust,
            CleanseSuppression,
            CleanseBlind,
            CleanseTaunt,
            CleanseTeamate1,
            CleanseTeamate2,
            CleanseTeamate3,
            CleanseTeamate4,
            CleanseTeamate5,
            FaceOfTheMountainEnabled,
            RedemptionEnabled,
            ZhonyasEnabled,
            SeraphsEnabled,
            LocketEnabled,
            RanduinsEnabled,
            TiamatEnabled,
            RavenousEnabled,
            TitanicEnabled,
            GunbladeEnabled,
            BotrkEnabled,
            CutlassEnabled,
            YoumuusEnabled,
            GLP800Enabled,
            SmiteActive,
            HealActive,
            HealTeamateActive,
            BarrierActive;

        private CheckBox SmiteRed,
            SmiteBlue,
            DrawSmiteEnabled,
            SmiteEnemies,
            SmiteEnemies2Stacks,
            IgniteKSEnable,
            IgniteInCombo,
            PotionsEnabled;

        private Slider CleanseHumanizerDelay,
            CleanseDurationMin,
            FaceOfTheMountainPercent,
            RedemptionPercent,
            SeraphsPercent,
            LocketPercent,
            HealPercent,
            HealTeamatePercent,
            BarrierPercent,
            PotionsPercent;

        private KeyBind SmiteKey;

        #endregion

        #region Teamate

        private AIHeroClient CleanseTeamate01, CleanseTeamate02, CleanseTeamate03, CleanseTeamate04, CleanseTeamate05;

        #endregion

        private AIHeroClient MikaelsTargetToCast;
        private Vector2 AutoSmiteTextPos;
        private Spell.SpellBase SMITE, HEAL, BARRIER, EXHAUST, CLEANSE, IGNITE;

        public Activator(Menu mainMenu)
        {
            if (!Program.EnableActivator.CurrentValue)
            {
                return;
            }

            LoadSpells();

            var menu = mainMenu.AddSubMenu("Activator");

            #region Cleanse / QSS / Mikaels

            menu.AddGroupLabel("++ Cleanse / QSS / Mikaels");
            CleanseActive = menu.Add("enabled", new CheckBox("Enabled "));
            CleanseHumanizerDelay = menu.Add("CleanseHumanizerDelay", new Slider("Humanizer Delay in Tick", 0, 0, 60));
            CleanseDurationMin = menu.Add("CleanseDurationMin",
                new Slider("Minimum Duration (secs) to Cleanse", 1, 0, 3));
            menu.AddLabel("CC Filter");
            CleanseCharm = menu.Add("CleanseCharm", new CheckBox("Use on Charm"));
            CleanseDisarm = menu.Add("CleanseDisarm", new CheckBox("Use on Disarm"));
            CleanseFear = menu.Add("CleanseFear", new CheckBox("Use on Fear"));
            CleanseFlee = menu.Add("CleanseFlee", new CheckBox("Use on Flee"));
            CleansePolymorph = menu.Add("CleansePolymorph", new CheckBox("Use on Polymorph"));
            CleanseSnare = menu.Add("CleanseSnare", new CheckBox("Use on Snare"));
            CleanseStun = menu.Add("CleanseStun", new CheckBox("Use on Stun"));
            CleanseExhaust = menu.Add("CleanseExhaust", new CheckBox("Use on Exhaust"));
            CleanseSuppression = menu.Add("CleanseSuppression", new CheckBox("Use on Suppression"));
            CleanseBlind = menu.Add("CleanseBlind", new CheckBox("Use on Blind"));
            CleanseTaunt = menu.Add("CleanseTaunt", new CheckBox("Use on Taunt"));
            menu.AddLabel("Mikaels Fiter");
            AddTeamatesToCleanse(menu);

            #endregion

            #region DEFENSIVE ITEMS MENU

            menu.AddGroupLabel("++ Defensive Items");
            menu.AddLabel("Face of The Mountain");
            FaceOfTheMountainEnabled = menu.Add("FaceOfTheMountainEnabled", new CheckBox("Enabled"));
            FaceOfTheMountainPercent = menu.Add("FaceOfTheMountainPercent",
                new Slider("Use at Health %:{0}", 50, 1, 99));

            menu.AddLabel("Redemption");
            RedemptionEnabled = menu.Add("RedemptionEnabled", new CheckBox("Enabled"));
            RedemptionPercent = menu.Add("RedemptionPercent", new Slider("Use at Health %:{0}", 40, 1, 99));

            menu.AddLabel("Zhonyas Hourglass");
            ZhonyasEnabled = menu.Add("ZhonyasEnabled", new CheckBox("COMING SOON WITH DAMAGE PREDICTION"));

            menu.AddLabel("Seraphs Embrace");
            SeraphsEnabled = menu.Add("SeraphsEnabled", new CheckBox("Enabled"));
            SeraphsPercent = menu.Add("SeraphsPercent", new Slider("Use at Health %:{0}", 50, 1, 99));

            menu.AddLabel("Locket of the Iron Solari");
            LocketEnabled = menu.Add("LocketEnabled", new CheckBox("Enabled"));
            LocketPercent = menu.Add("LocketPercent", new Slider("Use at Health %:{0}", 45, 1, 99));

            menu.AddLabel("Randuins Omen");
            RanduinsEnabled = menu.Add("RanduinsEnabled", new CheckBox("Enabled"));

            #endregion

            #region OFFENSIVE ITEMS MENU

            menu.AddGroupLabel("++ Offensive Items");
            menu.AddLabel("Tiamat/Hydras");
            TiamatEnabled = menu.Add("TiamatEnabled", new CheckBox("Use Tiamat"));
            RavenousEnabled = menu.Add("RavenousEnabled", new CheckBox("Use Ravenous Hydra"));
            TitanicEnabled = menu.Add("TitanicEnabled", new CheckBox("Use Titanic Hydra"));
            menu.AddLabel("Hextech Gunblade");
            GunbladeEnabled = menu.Add("GunbladeEnabled", new CheckBox("Enabled in Combo"));
            menu.AddLabel("Blade of the Ruined King");
            BotrkEnabled = menu.Add("BotrkEnabled", new CheckBox("Enabled in Combo"));
            menu.AddLabel("Bilgewater Cutlass");
            CutlassEnabled = menu.Add("CutlassEnabled", new CheckBox("Enabled in Combo"));
            menu.AddLabel("Youmuus Ghostblade");
            YoumuusEnabled = menu.Add("YoumuusEnabled", new CheckBox("Enabled in Combo"));
            menu.AddLabel("Hextech GLP-800");
            GLP800Enabled = menu.Add("GLP800Enabled", new CheckBox("Enabled in Combo"));

            #endregion

            #region SUMMONERS MENU

            menu.AddGroupLabel("++ Summoners");

            menu.AddLabel("Summoner: Heal");
            HealActive = menu.Add("HealActive", new CheckBox("Enabled"));
            HealPercent = menu.Add("HealPercent", new Slider("Use at Health %:{0}", 30, 1, 99));
            HealTeamateActive = menu.Add("HealTeamateActive", new CheckBox("Use on Teamates"));
            HealTeamatePercent = menu.Add("HealTeamatePercent", new Slider("Use at Teamate Health %:{0}", 30, 1, 99));

            menu.AddLabel("Summoner: Barrier");
            BarrierActive = menu.Add("BarrierActive", new CheckBox("Enabled"));
            BarrierPercent = menu.Add("BarrierPercent", new Slider("Use at Health %:{0}", 30, 1, 99));

            menu.AddLabel("Summoner: Smite");
            SmiteActive = menu.Add("SmiteActive", new CheckBox("Smite Epic Monsters"));
            SmiteRed = menu.Add("SmiteRed", new CheckBox("Smite Red Buff"));
            SmiteBlue = menu.Add("SmiteBlue", new CheckBox("Smite Blue Buff"));
            SmiteKey = menu.Add("SmiteKey", new KeyBind("Toggle Key", false, KeyBind.BindTypes.PressToggle, 77));
            DrawSmiteEnabled = menu.Add("DrawSmiteEnabled", new CheckBox("Draw Auto Smite Enabled"));
            SmiteEnemies = menu.Add("SmiteEnemies", new CheckBox("Smite Enemy Champions in Combo"));
            SmiteEnemies2Stacks = menu.Add("SmiteEnemies2Stacks", new CheckBox("Only Smite Champions at 2 Stacks"));

            menu.AddLabel("Summoner: Ignite");
            IgniteKSEnable = menu.Add("IgniteKSEnable", new CheckBox("Enable Ignite KS"));
            IgniteInCombo = menu.Add("IgniteInCombo", new CheckBox("Enable Ignite in Combo"));

            #endregion

            #region POTIONS MENU

            menu.AddGroupLabel("++ Potions");
            PotionsEnabled = menu.Add("PotionsEnabled", new CheckBox("Use Potions"));
            PotionsPercent = menu.Add("PotionsPercent", new Slider("Drink at Health %:{0}", 60, 1, 99));

            #endregion
        }

        #region AddTeamatesToCleanse

        private void AddTeamatesToCleanse(Menu menu)
        {
            int indx = 0;
            foreach (var teamate in EntityManager.Heroes.Allies)
            {
                if (indx == 0)
                {
                    indx++;
                    CleanseTeamate1 = menu.Add("CleanseTeamate1", new CheckBox(teamate.ChampionName));
                    CleanseTeamate01 = teamate;
                }
                else if (indx == 1)
                {
                    indx++;
                    CleanseTeamate2 = menu.Add("CleanseTeamate2", new CheckBox(teamate.ChampionName));
                    CleanseTeamate02 = teamate;
                }
                else if (indx == 2)
                {
                    indx++;
                    CleanseTeamate3 = menu.Add("CleanseTeamate3", new CheckBox(teamate.ChampionName));
                    CleanseTeamate03 = teamate;
                }
                else if (indx == 3)
                {
                    indx++;
                    CleanseTeamate4 = menu.Add("CleanseTeamate4", new CheckBox(teamate.ChampionName));
                    CleanseTeamate04 = teamate;
                }
                else if (indx == 4)
                {
                    CleanseTeamate5 = menu.Add("CleanseTeamate5", new CheckBox(teamate.ChampionName));
                    CleanseTeamate05 = teamate;
                    return;
                }
            }
        }

        #endregion

        #region LoadSpells

        private void LoadSpells()
        {
            AutoSmiteTextPos = new Vector2(0, 0);
            MikaelsTargetToCast = null;
                                                                                                      
            if (Hero.Spellbook.GetSpell(SpellSlot.Summoner1).Name.ToLower().Contains("summonersmite"))
            {
                SMITE = new Spell.Targeted(SpellSlot.Summoner1, 500);
            }
            if (Hero.Spellbook.GetSpell(SpellSlot.Summoner2).Name.ToLower().Contains("summonersmite"))
            {
                SMITE = new Spell.Targeted(SpellSlot.Summoner2, 500);
            }
            HEAL = new Spell.Active(Hero.GetSpellSlotFromName("SummonerHeal"), 850);
            BARRIER = new Spell.Active(Hero.GetSpellSlotFromName("SummonerBarrier"), 0);
            EXHAUST = new Spell.Targeted(Hero.GetSpellSlotFromName("SummonerExhaust"), 650);
            CLEANSE = new Spell.Active(Hero.GetSpellSlotFromName("SummonerBoost"), 0);
            IGNITE = new Spell.Targeted(Hero.GetSpellSlotFromName("SummonerDot"), 600);

            //Cleansers
            QSS = new Item(3140);
            Scimitar = new Item(3139);
            Mikaels = new Item(3222, 600);
            //Potions
            HealthPotion = new Item(2003, 0);
            RefillablePotion = new Item(2031, 0);
            HuntersPotion = new Item(2032, 0);
            CorruptingPotion = new Item(2033, 0);
            Biscuit = new Item(2010, 0);
            //Defensives
            FaceOfTheMountain = new Item(3401, 600);
            Redemption = new Item(3107, 5500);
            Zhonyas = new Item(3157, 0);
            Seraphs = new Item(3040, 0);
            Locket = new Item(3190, 600);
            Randuins = new Item(3143, 400);
            //Offensive
            Gunblade = new Item(3146, 700);
            Botrk = new Item(3153, 550);
            Cutlass = new Item(3144, 550);
            Youmuus = new Item(3142, 0);
            GLP800 = new Item(3030, 800);
            Tiamat = new Item(3077, 400);
            Ravenous = new Item(3074, 400);
            Titanic = new Item(3748, 400);
        }

        #endregion

        #region SmiteDamage

        private int GetSmiteDamage(int PlayerLevel)
        {
            switch (PlayerLevel)
            {
                case 1:
                    return 390;
                case 2:
                    return 410;
                case 3:
                    return 430;
                case 4:
                    return 450;
                case 5:
                    return 480;
                case 6:
                    return 510;
                case 7:
                    return 540;
                case 8:
                    return 570;
                case 9:
                    return 600;
                case 10:
                    return 640;
                case 11:
                    return 680;
                case 12:
                    return 720;
                case 13:
                    return 760;
                case 14:
                    return 800;
                case 15:
                    return 850;
                case 16:
                    return 900;
                case 17:
                    return 950;
                case 18:
                    return 1000;
                default:
                    return 0;
            }
        }

        #endregion

        #region DelayCast

        private void DelayCast()
        {
            if (HumanizeDelayCleanse > 0)
            {
                HumanizeDelayCleanse--;
            } //every game tick --

            if (DelayedSpellIndex == 1 && HumanizeDelayCleanse == 0) //SUMMONER CLEANSE
            {
                CLEANSE.Cast();
                DelayedSpellIndex = 0;
            }
            if (DelayedSpellIndex == 2 && HumanizeDelayCleanse == 0) //QSS
            {
                QSS.Cast();
                DelayedSpellIndex = 0;
            }
            if (DelayedSpellIndex == 3 && HumanizeDelayCleanse == 0) //SCIMITAR
            {
                Scimitar.Cast();
                DelayedSpellIndex = 0;
            }
            if (DelayedSpellIndex == 4 && HumanizeDelayCleanse == 0) //MIKAELS
            {
                Mikaels.Cast(MikaelsTargetToCast);
                DelayedSpellIndex = 0;
                MikaelsTargetToCast = null;
            }
        }

        #endregion

        #region UseDefensives

        private void UseDefensives()
        {
            //ITEMS
            if (FaceOfTheMountain.IsOwned() && FaceOfTheMountain.IsReady() && FaceOfTheMountainEnabled.CurrentValue &&
                !(Hero.IsDead))
            {
                foreach (var teamate in EntityManager.Heroes.Allies)
                {
                    if (!(teamate.IsDead) && FaceOfTheMountain.IsInRange(teamate) &&
                        teamate.HealthPercent <= FaceOfTheMountainPercent.CurrentValue &&
                        teamate.CountEnemyChampionsInRange(600) > 0)
                    {
                        FaceOfTheMountain.Cast(teamate);
                    } //Cast on injured teamate
                }
            }

            if (Redemption.IsOwned() && Redemption.IsReady() && RedemptionEnabled.CurrentValue && !(Hero.IsDead))
            {
                foreach (var teamate in EntityManager.Heroes.Allies)
                {
                    if (!(teamate.IsDead) && Redemption.IsInRange(teamate) &&
                        teamate.HealthPercent <= RedemptionPercent.CurrentValue &&
                        teamate.CountEnemyChampionsInRange(700) > 0)
                    {
                        Redemption.Cast(teamate.Position);
                    } //Cast on injured teamate
                }
            }

            if (Seraphs.IsOwned() && Seraphs.IsReady() && SeraphsEnabled.CurrentValue && !(Hero.IsDead))
            {
                if (Hero.HealthPercent <= SeraphsPercent.CurrentValue && Hero.CountEnemyChampionsInRange(700) > 0)
                {
                    Seraphs.Cast();
                }

            }

            if (Locket.IsOwned() && Locket.IsReady() && LocketEnabled.CurrentValue && !(Hero.IsDead))
            {
                foreach (var teamate in EntityManager.Heroes.Allies)
                {
                    if (!(teamate.IsDead) && Locket.IsInRange(teamate) &&
                        teamate.HealthPercent <= LocketPercent.CurrentValue &&
                        teamate.CountEnemyChampionsInRange(600) > 0)
                    {
                        Locket.Cast();
                    } //Cast on injured teamate
                }
            }


            //HEAL
            if (HEAL != null && HEAL.IsReady() && HealActive.CurrentValue && !(Hero.IsDead))
            {
                if (Hero.HealthPercent <= HealPercent.CurrentValue && Hero.CountEnemyChampionsInRange(600) > 0)
                {
                    HEAL.Cast();
                } //Cast on self

                if (HealTeamateActive.CurrentValue)
                {
                    foreach (var teamate in EntityManager.Heroes.Allies)
                    {
                        if (!(teamate.IsDead) && Hero.Distance(teamate) <= HEAL.Range &&
                            teamate.HealthPercent <= HealTeamatePercent.CurrentValue &&
                            teamate.CountEnemyChampionsInRange(600) > 0)
                        {
                            HEAL.Cast(teamate);
                        } //Cast on injured teamate
                    }
                }
            }


            //BARRIER
            if (BARRIER != null && BARRIER.IsReady() && BarrierActive.CurrentValue && !(Hero.IsDead) &&
                Hero.HealthPercent <= BarrierPercent.CurrentValue && Hero.CountEnemyChampionsInRange(500) > 0)
            {
                BARRIER.Cast();
            }

            //Potions
            if (!(Hero.IsDead) && PotionsEnabled.CurrentValue && Hero.HealthPercent <= PotionsPercent.CurrentValue &&
                !Hero.IsInFountainRange() &&
                !(Hero.HasBuff("RegenerationPotion") || Hero.HasBuff("ItemMiniRegenPotion") ||
                  Hero.HasBuff("ItemCrystalFlaskJungle") ||
                  Hero.HasBuff("ItemCrystalFlask") || Hero.HasBuff("ItemDarkCrystalFlask")))
            {
                if (HealthPotion.IsOwned() && HealthPotion.IsReady())
                {
                    HealthPotion.Cast();
                }
                if (RefillablePotion.IsOwned() && RefillablePotion.IsReady())
                {
                    RefillablePotion.Cast();
                }
                if (HuntersPotion.IsOwned() && HuntersPotion.IsReady())
                {
                    HuntersPotion.Cast();
                }
                if (CorruptingPotion.IsOwned() && CorruptingPotion.IsReady())
                {
                    CorruptingPotion.Cast();
                }
                if (Biscuit.IsOwned() && Biscuit.IsReady() && !Hero.IsInFountainRange())
                {
                    Biscuit.Cast();
                }
            }
        }

        #endregion

        #region CheckKeyPresses

        private void CheckKeyPresses()
        {
            if (MenuGUI.IsChatOpen || Chat.IsOpen) return;

            if (SmiteKey.CurrentValue) 
            {
                SmiteActive.CurrentValue = true;
            }
            else
            {                   
                SmiteActive.CurrentValue = false;
            }
        }

        #endregion

        #region AutoSmite

        private void AutoSmite()
        {
            if (SMITE != null && SMITE.IsReady() && SmiteActive.CurrentValue)
            {   
                foreach (var minion in EntityManager.MinionsAndMonsters.GetJungleMonsters())
                {                               
                    if (minion.IsValidTarget(570) && minion.Health <= GetSmiteDamage(Hero.Level))
                    {
                        if ((minion.Name.Contains("Red") && SmiteRed.CurrentValue) ||
                            (minion.Name.Contains("Blue") && SmiteBlue.CurrentValue) ||
                            minion.Name.Contains("Dragon") || minion.Name.Contains("Rift") ||
                            minion.Name.Contains("Baron"))
                        {
                            if (SMITE.Cast(minion))
                            {
                                return;
                            }
                        }
                    }
                }
            }
        }

        #endregion

        #region TargetValidForMikaels

        private bool TargetValidForMikaels(AIHeroClient Source)
        {
            if (CleanseTeamate1 != null && CleanseTeamate1.CurrentValue &&
                CleanseTeamate01.ChampionName == Source.ChampionName)
            {
                return true;
            }
            if (CleanseTeamate2 != null && CleanseTeamate2.CurrentValue &&
                CleanseTeamate02.ChampionName == Source.ChampionName)
            {
                return true;
            }
            if (CleanseTeamate3 != null && CleanseTeamate3.CurrentValue &&
                CleanseTeamate03.ChampionName == Source.ChampionName)
            {
                return true;
            }
            if (CleanseTeamate4 != null && CleanseTeamate4.CurrentValue &&
                CleanseTeamate04.ChampionName == Source.ChampionName)
            {
                return true;
            }
            if (CleanseTeamate5 != null && CleanseTeamate5.CurrentValue &&
                CleanseTeamate05.ChampionName == Source.ChampionName)
            {
                return true;
            }
            return false;
        }

        #endregion

        #region Combo

        private void Combo()
        {

            //Smite Enemies
            if (SMITE != null && SMITE.IsReady() && SmiteEnemies.CurrentValue)
            {
                //Console.WriteLine(SMITE.Handle.Cooldown);
                if ((SmiteEnemies2Stacks.CurrentValue && SMITE.Handle.Cooldown == 0) || !SmiteEnemies2Stacks.CurrentValue)
                {
                    if (TargetSelector.SelectedTarget != null && TargetSelector.SelectedTarget.IsValidTarget() && !TargetSelector.SelectedTarget.IsDead && (Hero.Position - TargetSelector.SelectedTarget.Position).Length() < 500)
                    {
                        SMITE.Cast(TargetSelector.SelectedTarget);
                    }
                    else
                    {
                        var target = TargetSelector.GetTarget(500, DamageType.True);

                        if (target == null) return;

                        SMITE.Cast(target);
                    }
                }
            }

            //Ravenous
            if (RavenousEnabled.CurrentValue && Ravenous.IsOwned() && Ravenous.IsReady() && !Hero.IsDead)
            {
                if (Hero.CountEnemyChampionsInRange(400) > 0)
                {
                    Ravenous.Cast();
                }
            }
            //Tiamat
            if (TiamatEnabled.CurrentValue && Tiamat.IsOwned() && Tiamat.IsReady() && !Hero.IsDead)
            {
                if (Hero.CountEnemyChampionsInRange(400) > 0)
                {
                    Tiamat.Cast();
                }
            }
            //RANDUINS
            if (Randuins.IsOwned() && Randuins.IsReady() && RanduinsEnabled.CurrentValue && !(Hero.IsDead))
            {
                if (Hero.CountEnemyChampionsInRange(400) > 0)
                {
                    Randuins.Cast();
                }
            }
            //Gunblade
            if (Gunblade.IsOwned() && Gunblade.IsReady() && GunbladeEnabled.CurrentValue && !(Hero.IsDead))
            {
                if (TargetSelector.SelectedTarget != null && TargetSelector.SelectedTarget.IsValidTarget() &&
                    !(TargetSelector.SelectedTarget.IsDead) && Gunblade.IsInRange(TargetSelector.SelectedTarget))
                {
                    Gunblade.Cast(TargetSelector.SelectedTarget);
                }
                else
                {
                    Gunblade.Cast(TargetSelector.GetTarget(700, DamageType.Mixed));
                }
            }
            //Botrk
            if (Botrk.IsOwned() && Botrk.IsReady() && BotrkEnabled.CurrentValue && !(Hero.IsDead))
            {
                if (TargetSelector.SelectedTarget != null && TargetSelector.SelectedTarget.IsValidTarget() &&
                    !(TargetSelector.SelectedTarget.IsDead) && Botrk.IsInRange(TargetSelector.SelectedTarget))
                {
                    Botrk.Cast(TargetSelector.SelectedTarget);
                }
                else
                {
                    Botrk.Cast(TargetSelector.GetTarget(550, DamageType.Mixed));
                }
            }
            //Cutlass
            if (Cutlass.IsOwned() && Cutlass.IsReady() && CutlassEnabled.CurrentValue && !(Hero.IsDead))
            {
                if (TargetSelector.SelectedTarget != null && TargetSelector.SelectedTarget.IsValidTarget() &&
                    !(TargetSelector.SelectedTarget.IsDead) && Cutlass.IsInRange(TargetSelector.SelectedTarget))
                {
                    Cutlass.Cast(TargetSelector.SelectedTarget);
                }
                else
                {
                    Cutlass.Cast(TargetSelector.GetTarget(550, DamageType.Mixed));
                }
            }
            //Youmuus
            if (Youmuus.IsOwned() && Youmuus.IsReady() && YoumuusEnabled.CurrentValue && !(Hero.IsDead))
            {
                if (Hero.CountEnemyChampionsInRange(500) > 0)
                {
                    Youmuus.Cast();
                }
            }
            //GLP800
            if (GLP800.IsOwned() && GLP800.IsReady() && GLP800Enabled.CurrentValue && !(Hero.IsDead))
            {
                if (TargetSelector.SelectedTarget != null && TargetSelector.SelectedTarget.IsValidTarget() &&
                    !(TargetSelector.SelectedTarget.IsDead) && GLP800.IsInRange(TargetSelector.SelectedTarget))
                {
                    GLP800.Cast(TargetSelector.SelectedTarget);
                }
                else
                {
                    GLP800.Cast(TargetSelector.GetTarget(800, DamageType.Mixed));
                }
            }
            //IGNITE
            if (IGNITE != null && (IgniteInCombo.CurrentValue || IgniteKSEnable.CurrentValue))
            {
                if (IGNITE.IsReady())
                {
                    var BadDudes = EntityManager.Heroes.Enemies;
                    foreach (var BadDude in BadDudes)
                    {
                        //Console.WriteLine(Hero.GetSummonerSpellDamage(BadDude, DamageLibrary.SummonerSpells.Ignite));
                        if (BadDude != null && !BadDude.IsDead && Hero.Distance(BadDude) <= 600 && BadDude.Health <=
                            Hero.GetSummonerSpellDamage(BadDude, DamageLibrary.SummonerSpells.Ignite))
                        {
                            IGNITE.Cast(BadDude);
                        }
                    }
                }
            }
        }

        #endregion

        #region OnGameUpdate

        public void OnGameUpdate()
        {
            UseDefensives();
            DelayCast();
            CheckKeyPresses();
            AutoSmite();

            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
            {
                Combo();
            }
        }

        #endregion

        #region OnRender

        public void OnRender()
        {
            if (SMITE != null && DrawSmiteEnabled.CurrentValue)
            {
                AutoSmiteTextPos = Drawing.WorldToScreen(Hero.Position);
                var text = SmiteActive.CurrentValue ? "AUTOSMITE ON" : "AUTOSMITE OFF";
                Drawing.DrawText(AutoSmiteTextPos[0] - 20, AutoSmiteTextPos[1] + 70, Color.White, text);
            }
        }

        #endregion

        #region OnOrbwalkAfterAttack

        public void OnOrbwalkAfterAttack(AttackableUnit target)
        {
            //Titanic
            if (TitanicEnabled.CurrentValue && Titanic.IsOwned() && Titanic.IsReady() && !Hero.IsDead)
            {
                Titanic.Cast();
            }
        }

        #endregion

        #region OnBuffAdd
        public void OnBuffAdd(Obj_AI_Base Source, Obj_AI_BaseBuffGainEventArgs args)
        {
            //GRender->Notification(Vec4(255, 255, 255, 255), 0, "%s", GBuffData->GetBuffName(BuffData));

            if (args.Buff.EndTime - args.Buff.StartTime >= CleanseDurationMin.CurrentValue)
            {
                //CLEANSE
                if (CLEANSE != null && CLEANSE.IsReady() && !(Hero.IsDead))
                {
                    if (Source == Hero && CleanseActive.CurrentValue && !Source.IsDead)
                    {
                        if (Source.HasBuffOfType(BuffType.Charm) && CleanseCharm.CurrentValue)
                        {
                            if (HumanizeDelayCleanse == 0)
                            {
                                HumanizeDelayCleanse = CleanseHumanizerDelay.CurrentValue;
                            }
                            DelayedSpellIndex = 1;
                        }
                        else if (Source.HasBuffOfType(BuffType.Blind) && CleanseBlind.CurrentValue)
                        {
                            if (HumanizeDelayCleanse == 0)
                            {
                                HumanizeDelayCleanse = CleanseHumanizerDelay.CurrentValue;
                            }
                            DelayedSpellIndex = 1;
                        }
                        else if (Source.HasBuffOfType(BuffType.Disarm) && CleanseDisarm.CurrentValue)
                        {
                            if (HumanizeDelayCleanse == 0)
                            {
                                HumanizeDelayCleanse = CleanseHumanizerDelay.CurrentValue;
                            }
                            DelayedSpellIndex = 1;
                        }
                        else if (Source.HasBuffOfType(BuffType.Fear) && CleanseFear.CurrentValue)
                        {
                            if (HumanizeDelayCleanse == 0)
                            {
                                HumanizeDelayCleanse = CleanseHumanizerDelay.CurrentValue;
                            }
                            DelayedSpellIndex = 1;
                        }
                        else if (Source.HasBuffOfType(BuffType.Flee) && CleanseFlee.CurrentValue)
                        {
                            if (HumanizeDelayCleanse == 0)
                            {
                                HumanizeDelayCleanse = CleanseHumanizerDelay.CurrentValue;
                            }
                            DelayedSpellIndex = 1;
                        }
                        else if (Source.HasBuffOfType(BuffType.Polymorph) && CleansePolymorph.CurrentValue)
                        {
                            if (HumanizeDelayCleanse == 0)
                            {
                                HumanizeDelayCleanse = CleanseHumanizerDelay.CurrentValue;
                            }
                            DelayedSpellIndex = 1;
                        }
                        else if (Source.HasBuffOfType(BuffType.Snare) && CleanseSnare.CurrentValue)
                        {
                            if (HumanizeDelayCleanse == 0)
                            {
                                HumanizeDelayCleanse = CleanseHumanizerDelay.CurrentValue;
                            }
                            DelayedSpellIndex = 1;
                        }
                        else if (Source.HasBuffOfType(BuffType.Taunt) && CleanseTaunt.CurrentValue)
                        {
                            if (HumanizeDelayCleanse == 0)
                            {
                                HumanizeDelayCleanse = CleanseHumanizerDelay.CurrentValue;
                            }
                            DelayedSpellIndex = 1;
                        }
                        else if (Source.HasBuff("SummonerExhaust") && CleanseExhaust.CurrentValue)
                        {
                            if (HumanizeDelayCleanse == 0)
                            {
                                HumanizeDelayCleanse = CleanseHumanizerDelay.CurrentValue;
                            }
                            DelayedSpellIndex = 1;
                        }
                        else if (Source.HasBuffOfType(BuffType.Stun) && CleanseStun.CurrentValue)
                        {
                            if (HumanizeDelayCleanse == 0)
                            {
                                HumanizeDelayCleanse = CleanseHumanizerDelay.CurrentValue;
                            }
                            DelayedSpellIndex = 1;
                        }
                    }
                }

                //QSS
                if (Source == Hero && !Source.IsDead && CleanseActive.CurrentValue &&
                    (QSS.IsOwned() || Scimitar.IsOwned()))
                {
                    if (Source.HasBuffOfType(BuffType.Charm) && CleanseCharm.CurrentValue)
                    {
                        if (QSS.IsOwned() && QSS.IsReady())
                        {
                            if (HumanizeDelayCleanse == 0)
                            {
                                HumanizeDelayCleanse = CleanseHumanizerDelay.CurrentValue;
                            }
                            DelayedSpellIndex = 2;
                        }
                        else if (Scimitar.IsOwned() && Scimitar.IsReady())
                        {
                            if (HumanizeDelayCleanse == 0)
                            {
                                HumanizeDelayCleanse = CleanseHumanizerDelay.CurrentValue;
                            }
                            DelayedSpellIndex = 3;
                        }
                    }
                    else if (Source.HasBuffOfType(BuffType.Blind) && CleanseBlind.CurrentValue)
                    {
                        if (QSS.IsOwned() && QSS.IsReady())
                        {
                            if (HumanizeDelayCleanse == 0)
                            {
                                HumanizeDelayCleanse = CleanseHumanizerDelay.CurrentValue;
                            }
                            DelayedSpellIndex = 2;
                        }
                        else if (Scimitar.IsOwned() && Scimitar.IsReady())
                        {
                            if (HumanizeDelayCleanse == 0)
                            {
                                HumanizeDelayCleanse = CleanseHumanizerDelay.CurrentValue;
                            }
                            DelayedSpellIndex = 3;
                        }
                    }
                    else if (Source.HasBuffOfType(BuffType.Disarm) && CleanseDisarm.CurrentValue)
                    {
                        if (QSS.IsOwned() && QSS.IsReady())
                        {
                            if (HumanizeDelayCleanse == 0)
                            {
                                HumanizeDelayCleanse = CleanseHumanizerDelay.CurrentValue;
                            }
                            DelayedSpellIndex = 2;
                        }
                        else if (Scimitar.IsOwned() && Scimitar.IsReady())
                        {
                            if (HumanizeDelayCleanse == 0)
                            {
                                HumanizeDelayCleanse = CleanseHumanizerDelay.CurrentValue;
                            }
                            DelayedSpellIndex = 3;
                        }
                    }
                    else if (Source.HasBuffOfType(BuffType.Fear) && CleanseFear.CurrentValue)
                    {
                        if (QSS.IsOwned() && QSS.IsReady())
                        {
                            if (HumanizeDelayCleanse == 0)
                            {
                                HumanizeDelayCleanse = CleanseHumanizerDelay.CurrentValue;
                            }
                            DelayedSpellIndex = 2;
                        }
                        else if (Scimitar.IsOwned() && Scimitar.IsReady())
                        {
                            if (HumanizeDelayCleanse == 0)
                            {
                                HumanizeDelayCleanse = CleanseHumanizerDelay.CurrentValue;
                            }
                            DelayedSpellIndex = 3;
                        }
                    }
                    else if (Source.HasBuffOfType(BuffType.Flee) && CleanseFlee.CurrentValue)
                    {
                        if (QSS.IsOwned() && QSS.IsReady())
                        {
                            if (HumanizeDelayCleanse == 0)
                            {
                                HumanizeDelayCleanse = CleanseHumanizerDelay.CurrentValue;
                            }
                            DelayedSpellIndex = 2;
                        }
                        else if (Scimitar.IsOwned() && Scimitar.IsReady())
                        {
                            if (HumanizeDelayCleanse == 0)
                            {
                                HumanizeDelayCleanse = CleanseHumanizerDelay.CurrentValue;
                            }
                            DelayedSpellIndex = 3;
                        }
                    }
                    else if (Source.HasBuffOfType(BuffType.Polymorph) && CleansePolymorph.CurrentValue)
                    {
                        if (QSS.IsOwned() && QSS.IsReady())
                        {
                            if (HumanizeDelayCleanse == 0)
                            {
                                HumanizeDelayCleanse = CleanseHumanizerDelay.CurrentValue;
                            }
                            DelayedSpellIndex = 2;
                        }
                        else if (Scimitar.IsOwned() && Scimitar.IsReady())
                        {
                            if (HumanizeDelayCleanse == 0)
                            {
                                HumanizeDelayCleanse = CleanseHumanizerDelay.CurrentValue;
                            }
                            DelayedSpellIndex = 3;
                        }
                    }
                    else if (Source.HasBuffOfType(BuffType.Snare) && CleanseSnare.CurrentValue)
                    {
                        if (QSS.IsOwned() && QSS.IsReady())
                        {
                            if (HumanizeDelayCleanse == 0)
                            {
                                HumanizeDelayCleanse = CleanseHumanizerDelay.CurrentValue;
                            }
                            DelayedSpellIndex = 2;
                        }
                        else if (Scimitar.IsOwned() && Scimitar.IsReady())
                        {
                            if (HumanizeDelayCleanse == 0)
                            {
                                HumanizeDelayCleanse = CleanseHumanizerDelay.CurrentValue;
                            }
                            DelayedSpellIndex = 3;
                        }
                    }
                    else if (Source.HasBuffOfType(BuffType.Taunt) && CleanseTaunt.CurrentValue)
                    {
                        if (QSS.IsOwned() && QSS.IsReady())
                        {
                            if (HumanizeDelayCleanse == 0)
                            {
                                HumanizeDelayCleanse = CleanseHumanizerDelay.CurrentValue;
                            }
                            DelayedSpellIndex = 2;
                        }
                        else if (Scimitar.IsOwned() && Scimitar.IsReady())
                        {
                            if (HumanizeDelayCleanse == 0)
                            {
                                HumanizeDelayCleanse = CleanseHumanizerDelay.CurrentValue;
                            }
                            DelayedSpellIndex = 3;
                        }
                    }
                    else if (Source.HasBuff("SummonerExhaust") && CleanseExhaust.CurrentValue)
                    {
                        if (QSS.IsOwned() && QSS.IsReady())
                        {
                            if (HumanizeDelayCleanse == 0)
                            {
                                HumanizeDelayCleanse = CleanseHumanizerDelay.CurrentValue;
                            }
                            DelayedSpellIndex = 2;
                        }
                        else if (Scimitar.IsOwned() && Scimitar.IsReady())
                        {
                            if (HumanizeDelayCleanse == 0)
                            {
                                HumanizeDelayCleanse = CleanseHumanizerDelay.CurrentValue;
                            }
                            DelayedSpellIndex = 3;
                        }
                    }
                    else if (Source.HasBuffOfType(BuffType.Stun) && CleanseStun.CurrentValue)
                    {
                        if (QSS.IsOwned() && QSS.IsReady())
                        {
                            if (HumanizeDelayCleanse == 0)
                            {
                                HumanizeDelayCleanse = CleanseHumanizerDelay.CurrentValue;
                            }
                            DelayedSpellIndex = 2;
                        }
                        else if (Scimitar.IsOwned() && Scimitar.IsReady())
                        {
                            if (HumanizeDelayCleanse == 0)
                            {
                                HumanizeDelayCleanse = CleanseHumanizerDelay.CurrentValue;
                            }
                            DelayedSpellIndex = 3;
                        }
                    }
                    else if (Source.HasBuffOfType(BuffType.Suppression) && CleanseSuppression.CurrentValue)
                    {
                        if (QSS.IsOwned() && QSS.IsReady())
                        {
                            if (HumanizeDelayCleanse == 0)
                            {
                                HumanizeDelayCleanse = CleanseHumanizerDelay.CurrentValue;
                            }
                            DelayedSpellIndex = 2;
                        }
                        else if (Scimitar.IsOwned() && Scimitar.IsReady())
                        {
                            if (HumanizeDelayCleanse == 0)
                            {
                                HumanizeDelayCleanse = CleanseHumanizerDelay.CurrentValue;
                            }
                            DelayedSpellIndex = 3;
                        }
                    }
                }

                //Mikael's
                if (Mikaels.IsOwned() && Mikaels.IsReady() && !(Hero.IsDead) && TargetValidForMikaels(Source as AIHeroClient) &&
                    Hero.Distance(Source) <= 600 && !Source.IsEnemy && CleanseActive.CurrentValue &&
                    !Source.IsDead)
                {
                    if (Source.HasBuffOfType(BuffType.Charm) && CleanseCharm.CurrentValue)
                    {
                        if (HumanizeDelayCleanse == 0)
                        {
                            HumanizeDelayCleanse = CleanseHumanizerDelay.CurrentValue;
                        }
                        DelayedSpellIndex = 4;
                        MikaelsTargetToCast = Source as AIHeroClient;
                    }
                    else if (Source.HasBuffOfType(BuffType.Blind) && CleanseBlind.CurrentValue)
                    {
                        if (HumanizeDelayCleanse == 0)
                        {
                            HumanizeDelayCleanse = CleanseHumanizerDelay.CurrentValue;
                        }
                        DelayedSpellIndex = 4;
                        MikaelsTargetToCast = Source as AIHeroClient;
                    }
                    else if (Source.HasBuffOfType(BuffType.Disarm) && CleanseDisarm.CurrentValue)
                    {
                        if (HumanizeDelayCleanse == 0)
                        {
                            HumanizeDelayCleanse = CleanseHumanizerDelay.CurrentValue;
                        }
                        DelayedSpellIndex = 4;
                        MikaelsTargetToCast = Source as AIHeroClient;
                    }
                    else if (Source.HasBuffOfType(BuffType.Fear) && CleanseFear.CurrentValue)
                    {
                        if (HumanizeDelayCleanse == 0)
                        {
                            HumanizeDelayCleanse = CleanseHumanizerDelay.CurrentValue;
                        }
                        DelayedSpellIndex = 4;
                        MikaelsTargetToCast = Source as AIHeroClient;
                    }
                    else if (Source.HasBuffOfType(BuffType.Flee) && CleanseFlee.CurrentValue)
                    {
                        if (HumanizeDelayCleanse == 0)
                        {
                            HumanizeDelayCleanse = CleanseHumanizerDelay.CurrentValue;
                        }
                        DelayedSpellIndex = 4;
                        MikaelsTargetToCast = Source as AIHeroClient;
                    }
                    else if (Source.HasBuffOfType(BuffType.Polymorph) && CleansePolymorph.CurrentValue)
                    {
                        if (HumanizeDelayCleanse == 0)
                        {
                            HumanizeDelayCleanse = CleanseHumanizerDelay.CurrentValue;
                        }
                        DelayedSpellIndex = 4;
                        MikaelsTargetToCast = Source as AIHeroClient;
                    }
                    else if (Source.HasBuffOfType(BuffType.Snare) && CleanseSnare.CurrentValue)
                    {
                        if (HumanizeDelayCleanse == 0)
                        {
                            HumanizeDelayCleanse = CleanseHumanizerDelay.CurrentValue;
                        }
                        DelayedSpellIndex = 4;
                        MikaelsTargetToCast = Source as AIHeroClient;
                    }
                    else if (Source.HasBuffOfType(BuffType.Taunt) && CleanseTaunt.CurrentValue)
                    {
                        if (HumanizeDelayCleanse == 0)
                        {
                            HumanizeDelayCleanse = CleanseHumanizerDelay.CurrentValue;
                        }
                        DelayedSpellIndex = 4;
                        MikaelsTargetToCast = Source as AIHeroClient;
                    }
                    else if (Source.HasBuff("SummonerExhaust") && CleanseExhaust.CurrentValue)
                    {
                        if (HumanizeDelayCleanse == 0)
                        {
                            HumanizeDelayCleanse = CleanseHumanizerDelay.CurrentValue;
                        }
                        DelayedSpellIndex = 4;
                        MikaelsTargetToCast = Source as AIHeroClient;
                    }
                    else if (Source.HasBuffOfType(BuffType.Stun) && CleanseStun.CurrentValue)
                    {
                        if (HumanizeDelayCleanse == 0)
                        {
                            HumanizeDelayCleanse = CleanseHumanizerDelay.CurrentValue;
                        }
                        DelayedSpellIndex = 4;
                        MikaelsTargetToCast = Source as AIHeroClient;
                    }
                }
            }
        }
        #endregion
    }
}
