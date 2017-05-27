using System;
using System.IO;
using EloBuddy;
using EloBuddy.Sandbox;
using EloBuddy.SDK;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Rendering;

namespace UtilitySharp
{
    class Program
    {
        private static Menu mainMenu;
        public static CheckBox EnableActivator, EnableGui, EnableWardTracker, EnableAntiClownJuke,EnableGankAlerter;
        public static readonly TextureLoader TextureLoader = new TextureLoader();

        public static string ConfigFolderPath = Path.Combine(SandboxConfig.DataDirectory, "UtilitySharp");

        static void Main(string[] args)
        {
            Loading.OnLoadingComplete += Loading_OnLoadingComplete;
        }

        private static void Loading_OnLoadingComplete(System.EventArgs args)
        {
            //Initialize Menus
            mainMenu = MainMenu.AddMenu("Utility PRO#", "sexsimiko7");

            EnableActivator = mainMenu.Add("activator",new CheckBox("Enable Activator"));

            EnableGui = mainMenu.Add("EnableGui", new CheckBox("Enable Gui"));

            EnableWardTracker = mainMenu.Add("EnableWardTracker", new CheckBox("Ward Tracker"));

            EnableAntiClownJuke = mainMenu.Add("EnableAntiClownJuke", new CheckBox("Anti Clown & Juke"));

            EnableGankAlerter = mainMenu.Add("EnableGankAlerter", new CheckBox("Gank Alerter"));

            _activator = null;
            _gui = null;
            _wardTracker = null;
            _antiClownJuke = null;
            _gankAlerter = null;

            Directory.CreateDirectory(ConfigFolderPath);

            Chat.Print("<b>Utility PRO<font color=\"#FFFFFF\">#</font> loaded.</font></b>");
            LoadEvents();
        }

        private static void LoadEvents()
        {
            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Orbwalker.OnPostAttack += Orbwalker_OnPostAttack;
            Obj_AI_Base.OnBuffGain += Obj_AI_Base_OnBuffGain;
            Teleport.OnTeleport += Teleport_OnTeleport;
            Game.OnTick += OnTick;
            Drawing.OnEndScene += OnEndScene;
            GameObject.OnCreate += OnCreate;
            new InfoLoader().Load();
        }

        private static void OnCreate(GameObject sender, EventArgs args)
        {
            
        }

        private static void OnEndScene(EventArgs args)
        {
            _gui?.onDraw(args);
        }

        private static void OnTick(EventArgs args)
        {
            
        }

        private static void Teleport_OnTeleport(Obj_AI_Base sender, Teleport.TeleportEventArgs args)
        {
             
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
        private static Gui _gui;
        private static WardTracker _wardTracker;
        private static AntiClown_Juke _antiClownJuke;
        private static GankAlerter _gankAlerter;
        private static void Game_OnUpdate(System.EventArgs args)
        {
            //ACTIVATOR
            if (EnableActivator.CurrentValue)
            {
                if (_activator == null)
                    _activator = new Activator(mainMenu);
            }

            //GUI
            if (EnableGui.CurrentValue)
            {
                if (_gui == null)
                    _gui = new Gui(mainMenu);
            }

            if (EnableWardTracker.CurrentValue)
            {
                if(_wardTracker == null)
                    _wardTracker = new WardTracker(mainMenu);
            }

            if (EnableAntiClownJuke.CurrentValue)
            {
                if(_antiClownJuke == null)
                    _antiClownJuke = new AntiClown_Juke(mainMenu);
            }

            if (EnableGankAlerter.CurrentValue)
            {
                if(_gankAlerter == null)
                    _gankAlerter = new GankAlerter(mainMenu);
            }
             
            _activator?.OnGameUpdate();
        }
    }
}
