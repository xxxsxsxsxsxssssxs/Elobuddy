using System;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;

namespace UtilitySharp
{
    class Program
    {
        private static Menu mainMenu;
        public static CheckBox EnableActivator;
        static void Main(string[] args)
        {
            Loading.OnLoadingComplete += Loading_OnLoadingComplete;
        }

        private static void Loading_OnLoadingComplete(System.EventArgs args)
        {
            mainMenu = MainMenu.AddMenu("Utility PRO#", "sexsimiko7");
            EnableActivator = mainMenu.Add("activator",new CheckBox("Enable Activator"));

            _activator = null;

            Chat.Print("<b>Utility PRO<font color=\"#FFFFFF\">#</font> loaded.</font></b>");
            LoadEvents();
            new GankAlerter.GankAlerter(mainMenu);
            new WardTracker.WardTracker().InitializeComponent(mainMenu);
        }

        private static void LoadEvents()
        {
            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Orbwalker.OnPostAttack += Orbwalker_OnPostAttack;
            Obj_AI_Base.OnBuffGain += Obj_AI_Base_OnBuffGain;

            #region Clown&Juke

            Game.OnTick += OnTick;
            GameObject.OnCreate += OnCreate;
            Obj_AI_Base.OnProcessSpellCast += OnProcessSpellCast;
            Drawing.OnEndScene += OnDraw;

            #endregion

        }

        private static void OnTick(EventArgs args)
        {
           _activator?.OnTick(args);
        }

        private static void OnCreate(GameObject sender, EventArgs args)
        {
            _activator?.OnCreate(sender ,args);
        }

        private static void OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            _activator?.OnProcessSpellCast(sender, args);
        }

        private static void OnDraw(EventArgs args)
        {
            _activator?.OnDraw(args);
        }

        private static void Obj_AI_Base_OnBuffGain(Obj_AI_Base sender, Obj_AI_BaseBuffGainEventArgs args)
        {
            _activator?.OnBuffAdd(sender, args);
        }

        private static void Orbwalker_OnPostAttack(AttackableUnit target, System.EventArgs args)
        {  
            _activator?.OnOrbwalkAfterAttack(target);
        }

        private static void Drawing_OnDraw(System.EventArgs args)
        { 
            _activator?.OnRender();
        }

        private static Activator _activator;
        private static void Game_OnUpdate(System.EventArgs args)
        {               
            if (EnableActivator.CurrentValue)
            {
                if (_activator == null)
                    _activator = new Activator(mainMenu);
            }
            _activator?.OnGameUpdate();
        }
    }
}
