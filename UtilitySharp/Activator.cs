using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Notifications;
using SharpDX;
using Color = System.Drawing.Color;
using EloBuddy.SDK.Rendering;
using RectangleF = SharpDX.RectangleF;
using UtilitySharp.Properties;
using SharpDX.Direct3D9;


namespace GankAlerter
{
    public class GankAlerter
    {
        private Menu menu;

        private CheckBox Enable;

        private Slider Value;

        public GankAlerter(Menu mainMenu)
        {
            menu = mainMenu.AddSubMenu("Gank Alerter");

            menu.AddGroupLabel("Draw Champions");

            Enable = menu.Add("enable", new CheckBox("Enable Gang alerter"));

            Value = menu.Add("value", new Slider("Distance to draw", 1000, 600, 1200));

            foreach (var obj in EntityManager.Heroes.Enemies)
            {
                menu.Add("draw" + obj.BaseSkinName, new CheckBox("Draw " + obj.BaseSkinName));
            }
            
            Drawing.OnDraw += Drawing_OnDraw;     
        }

        private void Drawing_OnDraw(EventArgs args)
        {
            foreach (var obj in EntityManager.Heroes.Enemies)
            {
                //Console.WriteLine(menu["draw" + obj.BaseSkinName].Cast<CheckBox>().CurrentValue);      
                if (obj != null && Enable.CurrentValue && menu["draw" + obj.BaseSkinName].Cast<CheckBox>().CurrentValue &&
                    obj.IsValidTarget(Value.CurrentValue) && Player.Instance.CountAllyChampionsInRange(Value.CurrentValue) < 2  && Player.Instance.Distance(obj) > Value.CurrentValue - Value.CurrentValue / 2)
                {
                    Drawing.DrawLine(Player.Instance.Position.WorldToScreen(), obj.Position.WorldToScreen(), 10, Color.Azure);
                    Drawing.DrawText(Player.Instance.Position.WorldToScreen().Extend(obj,15), Color.Red, string.Format("WARNING {0} incoming !", obj.BaseSkinName), 65);
                }  
            }         
        }
    }
}



namespace WardTracker
{
    public interface IWard
    {
        string FriendlyName { get; }
        string BaseSkinName { get; }
        string DetectingBuffName { get; }
        string DetectingSpellCastName { get; }
        string DetectingObjectName { get; }
        WardTracker.Ward.Type Type { get; }

        bool Matches(Obj_AI_Base target);
        bool MatchesBuffGain(Obj_AI_Base sender, Obj_AI_BaseBuffGainEventArgs args);
        bool MatchesSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args);
        WardTracker.Ward CreateWard(AIHeroClient caster, Obj_AI_Base wardHandle);
        WardTracker.Ward CreateFakeWard(AIHeroClient caster, Vector3 position);
        WardTracker.Ward CreateFakeWard(GameObjectTeam team, Vector3 position);
    }

    public abstract class WardBase : IWard
    {
        public abstract string FriendlyName { get; }
        public abstract string BaseSkinName { get; }
        public abstract string DetectingBuffName { get; }
        public abstract string DetectingSpellCastName { get; }
        public virtual string DetectingObjectName
        {
            get { return string.Empty; }
        }
        public abstract WardTracker.Ward.Type Type { get; }

        public virtual bool Matches(Obj_AI_Base target)
        {
            return target.Type == GameObjectType.obj_AI_Minion
                   && target.BaseSkinName == BaseSkinName
                   && target.Buffs.Any(o => o.Name == DetectingBuffName);
        }

        public virtual bool MatchesBuffGain(Obj_AI_Base sender, Obj_AI_BaseBuffGainEventArgs args)
        {
            return sender.BaseSkinName == BaseSkinName
                   && args.Buff.Name == DetectingBuffName;
        }

        public virtual bool MatchesSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            return String.Equals(args.SData.Name, DetectingSpellCastName, StringComparison.CurrentCultureIgnoreCase);
        }

        public virtual WardTracker.Ward CreateWard(AIHeroClient caster, Obj_AI_Base wardHandle)
        {
            // Wards cannot have more than 5 max health
            if (wardHandle.MaxHealth <= 5)
            {
                // Validate base skin
                if (!string.IsNullOrWhiteSpace(wardHandle.BaseSkinName) && wardHandle.BaseSkinName == BaseSkinName)
                {
                    // Return the ward object
                    return new WardTracker.Ward(caster, wardHandle, wardHandle.Position, this, GetWardDuration(caster), caster.Team);
                }
            }

            return null;
        }

        public static Vector3 GetValidCastSpot(Vector3 castPosition)
        {
            var flags = castPosition.ToNavMeshCell().CollFlags;
            if (!flags.HasFlag(CollisionFlags.Wall) && !flags.HasFlag(CollisionFlags.Building))
            {
                return castPosition;
            }

            const int maxRange = 20;
            const double step = 2 * Math.PI / 20;

            var start = new Vector2(castPosition.ToNavMeshCell().GridX, castPosition.ToNavMeshCell().GridY);
            var checkedCells = new HashSet<Vector2>();

            var directions = new List<Vector2>();
            for (var theta = 0d; theta <= 2 * Math.PI + step; theta += step)
            {
                directions.Add((new Vector2((short)(start.X + Math.Cos(theta)), (short)(start.Y - Math.Sin(theta))) - start).Normalized());
            }

            var validPositions = new HashSet<Vector3>();
            for (var range = 1; range < maxRange; range++)
            {
                foreach (var direction in directions)
                {
                    var end = start + range * direction;
                    var testPoint = new Vector2((short)end.X, (short)end.Y);
                    if (checkedCells.Contains(testPoint))
                    {
                        continue;
                    }
                    checkedCells.Add(testPoint);

                    flags = new NavMeshCell((short)testPoint.X, (short)testPoint.Y).CollFlags;
                    if (!flags.HasFlag(CollisionFlags.Wall) && !flags.HasFlag(CollisionFlags.Building))
                    {
                        validPositions.Add(NavMesh.GridToWorld((short)testPoint.X, (short)testPoint.Y));
                    }
                }

                if (validPositions.Count > 0)
                {
                    return validPositions.OrderBy(o => o.Distance(start, true)).First();
                }
            }

            return castPosition;
        }

        public virtual WardTracker.Ward CreateFakeWard(AIHeroClient caster, Vector3 position)
        {
            return new WardTracker.Ward(caster, null, GetValidCastSpot(position), this, GetWardDuration(caster), caster.Team);
        }

        public virtual WardTracker.Ward CreateFakeWard(GameObjectTeam team, Vector3 position)
        {
            return new WardTracker.Ward(null, null, GetValidCastSpot(position), this, -1, team);
        }

        public int GetWardDuration(AIHeroClient caster)
        {
            switch (Type)
            {
                case WardTracker.Ward.Type.SightWard:
                    return 150;
                case WardTracker.Ward.Type.YellowTrinket:
                    return (int)Math.Ceiling(60 + 3.5 * (caster.Level - 1));
            }

            return -1;
        }
    }

    public sealed class BlueTrinket : WardBase
    {
        public override string FriendlyName
        {
            get { return "Farsight Alteration (Blue)"; }
        }
        public override string BaseSkinName
        {
            get { return "BlueTrinket"; }
        }
        public override string DetectingBuffName
        {
            get { return "relicblueward"; }
        }
        public override string DetectingSpellCastName
        {
            get { return "TrinketOrbLvl3"; }
        }
        public override string DetectingObjectName
        {
            get { return "Global_Trinket_ItemClairvoyance_Red.troy"; }
        }
        public override WardTracker.Ward.Type Type
        {
            get { return WardTracker.Ward.Type.BlueTrinket; }
        }
    }

    public sealed class ControlWard : WardBase
    {
        public override string FriendlyName
        {
            get { return "Control Ward (Pink)"; }
        }
        public override string BaseSkinName
        {
            get { return "JammerDevice"; }
        }
        public override string DetectingBuffName
        {
            get { return "JammerDevice"; }
        }
        public override string DetectingSpellCastName
        {
            get { return "JammerDevice"; }
        }
        public override WardTracker.Ward.Type Type
        {
            get { return WardTracker.Ward.Type.JammerDevice; }
        }
    }

    public sealed class SightWard : WardBase
    {
        public override string FriendlyName
        {
            get { return "Sight Ward (Green)"; }
        }
        public override string BaseSkinName
        {
            get { return "SightWard"; }
        }
        public override string DetectingBuffName
        {
            get { return "sharedwardbuff"; }
        }
        public override string DetectingSpellCastName
        {
            get { return "ItemGhostWard"; }
        }
        public override WardTracker.Ward.Type Type
        {
            get { return WardTracker.Ward.Type.SightWard; }
        }
    }

    public sealed class YellowTrinket : WardBase
    {
        public override string FriendlyName
        {
            get { return "Warding Totem (Yellow)"; }
        }
        public override string BaseSkinName
        {
            get { return "YellowTrinket"; }
        }
        public override string DetectingBuffName
        {
            get { return "sharedwardbuff"; }
        }
        public override string DetectingSpellCastName
        {
            get { return "TrinketTotemLvl1"; }
        }
        public override WardTracker.Ward.Type Type
        {
            get { return WardTracker.Ward.Type.YellowTrinket; }
        }
    }

    public sealed class WardTracker
    {
        public static Dictionary<Ward.Type, CheckBox> EnabledWards { get; private set; }
        private static readonly List<Obj_AI_Base> CreatedWards = new List<Obj_AI_Base>();
        public static Ward.PinkColors PinkColor { get; private set; }

        private static readonly HashSet<IWard> DetectableWards = new HashSet<IWard>
        {
            new BlueTrinket(),
            new SightWard(),
            new ControlWard(),
            new YellowTrinket()
        };

        public Menu Menu { get; private set; }

        private CheckBox DrawAlliedWardTime { get; set; }

        public CheckBox RenderWard { get; private set; }
        public CheckBox DrawMinimap { get; private set; }
        public CheckBox DrawHealth { get; private set; }
        public CheckBox DrawTime { get; private set; }

        public static CheckBox NotifyPlace { get; private set; }
        public static CheckBox NotifyPlacePing { get; private set; }
        public static Slider NotifyRange { get; private set; }

        private List<Ward> ActiveWards { get; set; }

        public bool ShouldLoad(bool isSpectatorMode = false)
        {
            // Always load, regardless the game mode
            return true;
        }

        public void InitializeComponent(Menu mainMenu)
        {
            // Initialize menu
            Menu = mainMenu.AddSubMenu("Ward Tracker", longTitle: "Ward Presence Tracker");

            Menu.AddGroupLabel("Information");
            Menu.AddLabel("A ward presence tracker helps you in various ways ingame.");
            Menu.AddLabel("It lets you visually see where different ward types have been placed,");
            Menu.AddLabel("even when you were not inrage or didn't see the ward on creation.");

            // Add all known ward types to the menu
            Menu.AddSeparator();
            Menu.AddLabel("You can enable ward tracking for the following ward types:");
            EnabledWards = new Dictionary<Ward.Type, CheckBox>();
            foreach (var wardType in Enum.GetValues(typeof(Ward.Type)).Cast<Ward.Type>())
            {
                EnabledWards[wardType] = Menu.Add(wardType.ToString(), new CheckBox(wardType.ToString()));
            }

            // World options
            Menu.AddSeparator();
            Menu.AddLabel("World options:");
            RenderWard = Menu.Add("renderWard", new CheckBox("Render ward on map (circle)"));
            DrawHealth = Menu.Add("drawHealth", new CheckBox("Draw remaining ward health"));
            DrawTime = Menu.Add("drawTime", new CheckBox("Draw remaining ward time"));
            if (!Bootstrap.IsSpectatorMode)
            {
                DrawAlliedWardTime = Menu.Add("drawAlliedTime", new CheckBox("Draw allied remaining ward time"));
            }

            // Minimap options
            Menu.AddSeparator();
            Menu.AddLabel("Minimap options:");
            DrawMinimap = Menu.Add("drawMinimap", new CheckBox("Draw ward icon"));
            if (!Bootstrap.IsSpectatorMode)
            {
                var pinkColor = new ComboBox("Pink ward color", Enum.GetValues(typeof(Ward.PinkColors)).Cast<Ward.PinkColors>().Select(o => o.ToString()));
                pinkColor.OnValueChange += delegate (ValueBase<int> sender, ValueBase<int>.ValueChangeArgs args) { PinkColor = (Ward.PinkColors)args.NewValue; };
                Menu.Add("pinkColor", pinkColor);
            }

            if (!Bootstrap.IsSpectatorMode)
            {
                // Notification options
                Menu.AddSeparator();
                Menu.AddLabel("Notification options:");
                NotifyPlace = Menu.Add("notifyPlaceNear", new CheckBox("Notify when enemy nearby places a ward"));
                NotifyPlacePing = Menu.Add("notifyPlaceNearPing", new CheckBox("Notify with local ping", false));
                NotifyRange = Menu.Add("notifyPlaceNearRange", new Slider("Notification altert range", 2000, 500, 5000));
            }

            // Initialize properties
            ActiveWards = new List<Ward>();

            // Get all currently known wards
            ObjectManager.Get<Obj_AI_Minion>().ToList().ForEach(o => OnCreate(o, EventArgs.Empty));

            // Listen to required events
            Game.OnTick += OnTick;
            Drawing.OnDraw += OnDraw;
            Drawing.OnEndScene += OnEndScene;
            GameObject.OnCreate += OnCreate;
            GameObject.OnDelete += OnDelete;
            Obj_AI_Base.OnPlayAnimation += OnAnimation;
            Obj_AI_Base.OnBuffGain += OnBuffGain;
            Obj_AI_Base.OnProcessSpellCast += OnProcessSpellCast;
        }

        private void OnTick(EventArgs args)
        {
            foreach (var fakeWard in ActiveWards.Where(o => o.IsFakeWard).ToArray())
            {
                switch (fakeWard.WardType)
                {
                    case Ward.Type.SightWard:
                    case Ward.Type.YellowTrinket:

                        if (fakeWard.RemainingTime <= 0)
                        {
                            // Remove timed up fake wards
                            ActiveWards.Remove(fakeWard);
                        }
                        break;
                }
            }
        }

        private void OnDraw(EventArgs args)
        {
            ActiveWards.ForEach(ward =>
            {
                // Check if ward type is enabled
                if (ward.IsEnabled)
                {
                    // Actions for invisible wards
                    if (!ward.IsVisible)
                    {
                        if (RenderWard.CurrentValue)
                        {
                            // Render a fake ward object on the map (soon)
                            ward.RenderWard();
                        }
                        if (DrawHealth.CurrentValue)
                        {
                            // Draw the remaining ward health on the map
                            ward.DrawHealth();
                        }
                    }

                    // Draw the remaining ward time for all wards
                    if (Bootstrap.IsSpectatorMode || (ward.Team.IsEnemy() || DrawAlliedWardTime.CurrentValue))
                    {
                        ward.DrawTime();
                    }
                }
            });
        }

        private void OnEndScene(EventArgs args)
        {
            if (DrawMinimap.CurrentValue)
            {
                foreach (var ward in ActiveWards.Where(o => !o.IsVisible && o.IsEnabled))
                {
                    // Draw the ward icon on the minimap for invisible wards
                    ward.DrawMinimap();
                }
            }
        }

        private void OnCreate(GameObject sender, EventArgs args)
        {
            switch (sender.Type)
            {
                case GameObjectType.obj_AI_Minion:
                {
                    var baseSender = (Obj_AI_Base)sender;

                    // Try to parse the ward
                    Ward.Type wardType;
                    if (Ward.IsWard(baseSender, out wardType))
                    {
                        // Add ward to created wards
                        CreatedWards.Add(baseSender);

                        // Validate all buffs of the ward
                        baseSender.Buffs.ForEach(o => OnBuffGain(baseSender, new Obj_AI_BaseBuffGainEventArgs(o)));
                    }
                    break;
                }
                case GameObjectType.obj_GeneralParticleEmitter:
                {
                    if (Bootstrap.IsSpectatorMode)
                    {
                        break;
                    }

                    // FoW ward places
                    var particleEmitter = (Obj_GeneralParticleEmitter)sender;

                    foreach (var ward in DetectableWards.Where(o => String.Equals(o.DetectingObjectName, particleEmitter.Name, StringComparison.CurrentCultureIgnoreCase)))
                    {
                        var pos = particleEmitter.Position.To2D().To3DWorld();

                        // Check if that position already holds a ward of that type
                        if (
                            CreatedWards.Any(
                                o => o.Team != Player.Instance.Team && String.Equals(o.BaseSkinName, ward.BaseSkinName, StringComparison.CurrentCultureIgnoreCase) && o.Position.IsInRange(pos, 50)))
                        {
                            break;
                        }
                        if (ActiveWards.Any(o => o.WardType == ward.Type && o.Team != Player.Instance.Team && o.Position.IsInRange(pos, 50)))
                        {
                            break;
                        }

                        // Create a fake ward at that position
                        ward.CreateFakeWard(Player.Instance.Team == GameObjectTeam.Order ? GameObjectTeam.Chaos : GameObjectTeam.Order, pos);
                        break;
                    }
                    break;
                }
            }
        }

        private void OnDelete(GameObject sender, EventArgs args)
        {
            // Check if the sender is a minion object
            if (sender.Type == GameObjectType.obj_AI_Minion)
            {
                // Remove it from the lists
                CreatedWards.Remove((Obj_AI_Base)sender);
                ActiveWards.RemoveAll(o => !o.IsFakeWard && o.Handle.IdEquals(sender));
            }
        }

        private void OnAnimation(Obj_AI_Base sender, GameObjectPlayAnimationEventArgs args)
        {
            // Check if sender is an active ward
            if (sender.Type == GameObjectType.obj_AI_Minion)
            {
                var ward = ActiveWards.Find(o => o.Handle.IdEquals(sender));
                if (ward != null)
                {
                    switch (args.Animation)
                    {
                        case "Death":
                            // Remove the ward
                            OnDelete(sender, EventArgs.Empty);
                            break;
                    }
                }
            }
        }

        private void OnBuffGain(Obj_AI_Base sender, Obj_AI_BaseBuffGainEventArgs args)
        {
            // Only check minion types
            if (sender.Type != GameObjectType.obj_AI_Minion)
            {
                return;
            }

            foreach (var ward in DetectableWards.Where(ward => ward.MatchesBuffGain(sender, args)))
            {
                if (CreatedWards.Contains(sender))
                {
                    // Check if there is already a fake ward at that position
                    var fakeWard = ActiveWards.Where(o => o.IsFakeWard && o.Team == sender.Team)
                        .Where(
                            o => o.Position.IsInRange(sender, 500) &&
                                 (args.Buff.EndTime - Game.Time > 1000
                                     ? o.RemainingTime < 0
                                     : Math.Abs(o.RemainingTime - (args.Buff.EndTime - Game.Time)) < 3))
                        .OrderBy(o => Math.Abs(o.RemainingTime - (args.Buff.EndTime - Game.Time)))
                        .ThenBy(o => o.Position.Distance(sender, true))
                        .FirstOrDefault();
                    if (fakeWard != null)
                    {
                        // Remove the ward from the created wards list
                        CreatedWards.Remove(sender);

                        // Turn ward into a real ward
                        fakeWard.Handle = sender;
                    }
                    else
                    {
                        // Get the caster of the ward
                        if (args.Buff.Caster != null && args.Buff.Caster.Type == GameObjectType.AIHeroClient)
                        {
                            var caster = EntityManager.Heroes.AllHeroes.Find(o => o.IdEquals(args.Buff.Caster));
                            if (caster != null)
                            {
                                // Create a new ward wrapper
                                var wardObject = ward.CreateWard(caster, sender);
                                if (wardObject != null)
                                {
                                    // Remove the ward from the created wards list
                                    CreatedWards.Remove(sender);

                                    // Add the ward to the active wards list
                                    ActiveWards.Add(wardObject);
                                }
                            }
                        }
                    }
                }

                var activeWard = ActiveWards.Find(o => o.Handle.IdEquals(sender));
                if (activeWard != null)
                {
                    // Check for active buffs
                    CheckBuffs(activeWard);
                }

                break;
            }
        }

        private void OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.Type == GameObjectType.AIHeroClient)
            {
                // Check if any detectable ward matches the spell cast
                foreach (var ward in DetectableWards.Where(ward => ward.MatchesSpellCast(sender, args)))
                {
                    ActiveWards.Add(ward.CreateFakeWard((AIHeroClient)sender, args.End));
                    break;
                }
            }
        }

        private static void CheckBuffs(Ward ward)
        {
            if (ward.RemainingTimeDelegate == null)
            {
                // Get the correct buff name (seems like this one is used for all of them)
                const string buffName = "sharedwardbuff";

                // Check for remaining time buff
                var buff = ward.Handle.Buffs.Find(o => o.Name == buffName);
                if (buff != null)
                {
                    // Apply remaining time delegate
                    ward.RemainingTimeDelegate = () => (int)Math.Ceiling(buff.EndTime - Game.Time);
                }
            }
        }

        public class Ward
        {
            public const int Radius = 65;

            private static readonly Vector2 HealthBarSize = new Vector2(64, 6);
            private static readonly RectangleF HealthBar = new RectangleF(-HealthBarSize.X / 2, -HealthBarSize.Y / 2, HealthBarSize.X, HealthBarSize.Y);

            private const int HealthBarBorderWidth = 1;
            private const int HealthBarPadding = 2;
            private static readonly Color HealthBarBackgroundColor = Color.Black;
            private static readonly Dictionary<GameObjectTeam, Tuple<Color, Color>> HealthBarColors = new Dictionary<GameObjectTeam, Tuple<Color, Color>>
            {
                { GameObjectTeam.Order, new Tuple<Color, Color>(Color.FromArgb(81, 162, 230), Color.FromArgb(40, 80, 114)) },
                { GameObjectTeam.Chaos, new Tuple<Color, Color>(Color.FromArgb(230, 104, 104), Color.FromArgb(114, 51, 51)) }
            };

            public enum Type
            {
                BlueTrinket = 3,
                SightWard = 0,
                JammerDevice = 1,
                YellowTrinket = 2
            }

            public enum PinkColors
            {
                Pink,
                Red
            }

            public static readonly TextureLoader TextureLoader = new TextureLoader();

            static Ward()
            {
                // Load the textures
                TextureLoader.Load("BlueTrinket", Resources.BlueTrinket);
                TextureLoader.Load("ControlWard", Resources.ControlWard);
                TextureLoader.Load("ControlWard_Enemy", Resources.ControlWard_Enemy);
                TextureLoader.Load("ControlWard_Friendly", Resources.ControlWard_Friendly);
                TextureLoader.Load("YellowTrinket_Enemy", Resources.YellowTrinket_Enemy);
                TextureLoader.Load("YellowTrinket_Friendly", Resources.YellowTrinket_Friendly);
            }

            #region Constructor Properties

            public AIHeroClient Caster { get; private set; }
            public Obj_AI_Base Handle { get; set; }
            public Vector3 FakePosition { get; private set; }
            public IWard WardInfo { get; private set; }
            public Type WardType
            {
                get { return WardInfo.Type; }
            }
            public int Duration { get; private set; }
            public int CreationTime { get; private set; }

            #endregion

            #region Wrapped Properties

            public Vector3 Position
            {
                get { return Handle != null ? Handle.Position : FakePosition; }
            }
            public Vector2 ScreenPosition
            {
                get { return Handle != null ? Handle.Position.WorldToScreen() : FakePosition.WorldToScreen(); }
            }
            public Vector2 MinimapPosition
            {
                get { return Handle != null ? Handle.Position.WorldToMinimap() : FakePosition.WorldToMinimap(); }
            }
            public bool IsVisible
            {
                get { return Handle != null && Handle.IsHPBarRendered; }
            }
            private GameObjectTeam _team;
            public GameObjectTeam Team
            {
                get { return Handle != null ? Handle.Team : _team; }
                set { _team = value; }
            }
            public float MaxHealth
            {
                get
                {
                    switch (WardType)
                    {
                        case Type.BlueTrinket:
                            return 1;
                        case Type.JammerDevice:
                            return 4;
                        default:
                            return 3;
                    }
                }
            }

            #endregion

            public bool IsFakeWard
            {                                                       
                get { return Handle == null; }
            }

            private EloBuddy.SDK.Rendering.Sprite MinimapSprite { get; set; }
            private Text TextHandle { get; set; }

            private Texture MinimapIconTexture
            {
                get
                {
                    switch (WardType)
                    {
                        // Blue trinkets
                        case Type.BlueTrinket:
                            return TextureLoader["BlueTrinket"];
                        // Pink wards
                        case Type.JammerDevice:
                            return (Bootstrap.IsSpectatorMode ? Team == GameObjectTeam.Order : Team.IsAlly())
                                ? TextureLoader["ControlWard_Friendly"]
                                : (Bootstrap.IsSpectatorMode
                                    ? TextureLoader["ControlWard_Enemy"]
                                    : TextureLoader[PinkColor == PinkColors.Pink ? "ControlWard" : "ControlWard_Enemy"]);
                        // All other regular wards
                        default:
                            return (Bootstrap.IsSpectatorMode ? Team == GameObjectTeam.Order : Team.IsAlly())
                                ? TextureLoader["YellowTrinket_Friendly"]
                                : TextureLoader["YellowTrinket_Enemy"];
                    }
                }
            }

            public bool IsEnabled
            {
                get { return EnabledWards.ContainsKey(WardType) && EnabledWards[WardType].CurrentValue; }
            }

            public Func<int> RemainingTimeDelegate { get; set; }
            public int RemainingTime
            {
                get
                {
                    switch (WardType)
                    {
                        case Type.SightWard:
                        case Type.YellowTrinket:
                            return
                                IsVisible
                                    ? (int)Handle.Mana
                                    : (RemainingTimeDelegate != null
                                        ? RemainingTimeDelegate()
                                        : (((CreationTime + Duration) - Core.GameTickCount) / 1000));
                    }
                    return -1;
                }
            }
            public string RemainingTimeText
            {
                get
                {
                    var time = RemainingTime;
                    return time < 0 ? null : TimeSpan.FromSeconds(time).ToString(@"m\:ss");
                }
            }

            public Color Color
            {
                get
                {
                    switch (WardType)
                    {
                        case Type.BlueTrinket:
                            return Color.CornflowerBlue;
                        case Type.SightWard:
                            return Color.LimeGreen;
                        case Type.JammerDevice:
                            return Color.DeepPink;
                        case Type.YellowTrinket:
                            return Color.Yellow;
                    }
                    return Color.Red;
                }
            }

            public Ward(AIHeroClient caster, Obj_AI_Base handle, Vector3 position, IWard wardInfo, int duration, GameObjectTeam team)
            {
                // Initialize properties
                Caster = caster;
                Handle = handle;
                FakePosition = position;
                Team = team;
                WardInfo = wardInfo;
                Duration = duration * 1000;
                CreationTime = Core.GameTickCount;

                // Initialize rendering
                MinimapSprite = new EloBuddy.SDK.Rendering.Sprite(() => MinimapIconTexture);
                TextHandle = new Text(string.Empty, new System.Drawing.Font(FontFamily.GenericSansSerif, 8, FontStyle.Regular));

                // Notify player about placement
                if (!Bootstrap.IsSpectatorMode && Team.IsEnemy() &&
                    (Player.Instance.IsInRange(Position, NotifyRange.CurrentValue) || Player.Instance.IsInRange(position, NotifyRange.CurrentValue)))
                {
                    if (NotifyPlace.CurrentValue)
                    {
                        Notifications.Show(new SimpleNotification("A ward has been placed!",
                            string.Format("{0} has placed a {1}", caster != null ? caster.ChampionName : "Unknown", WardInfo.FriendlyName)));
                    }
                    if (NotifyPlacePing.CurrentValue)
                    {
                        TacticalMap.ShowPing(PingCategory.Normal, Position, true);
                    }
                }
            }

            public void RenderWard()
            {
                // TODO: Replace with fake object rendering once it's added by finn
                Circle.Draw(Color.ToBgra(150), Radius, 3, Position);
            }

            public void DrawMinimap()
            {
                // Draw the ward icon on the minimap
                var desc = MinimapSprite.Texture.GetLevelDescription(0);
                var offset = new Vector2(-desc.Width / 2f, -desc.Height / 2f);
                MinimapSprite.Draw(MinimapPosition + offset);
            }

            public void DrawTime()
            {
                // Apply the remaining time and draw it
                var time = RemainingTimeText;
                if (time != null)
                {
                    TextHandle.TextValue = "Ward: " + time;
                    TextHandle.Position = new Vector2(ScreenPosition.X - TextHandle.Bounding.Width / 2f, ScreenPosition.Y + HealthBar.Height);
                    TextHandle.Draw();
                }
            }

            public void DrawHealth()
            {
                // Get the screen position
                var pos = ScreenPosition;

                // Calculate the outline rectangle
                var rect = new RectangleF((int)(pos.X + HealthBar.X), (int)(pos.Y + HealthBar.Y), (int)HealthBar.Width, (int)HealthBar.Height);

                // Draw the background
                Drawing.DrawLine(new Vector2(rect.Left, rect.Top + rect.Height / 2), new Vector2(rect.Right, rect.Top + rect.Height / 2), rect.Height, HealthBarBackgroundColor);

                // Get the health percent
                var percent = (Handle == null ? MaxHealth : Handle.Health) / MaxHealth;

                // Draw the percent lines
                var colors = HealthBarColors[Bootstrap.IsSpectatorMode ? Team : (Team.IsAlly() ? GameObjectTeam.Order : GameObjectTeam.Chaos)];
                var max = rect.Height - HealthBarBorderWidth * 2;
                for (var i = 0; i < max; i++)
                {
                    var start = new Vector2(rect.Left + HealthBarBorderWidth, rect.Top + HealthBarBorderWidth + i);
                    EloBuddy.SDK.Rendering.Line.DrawLine(Color.FromArgb((byte)(colors.Item1.R + (colors.Item2.R - colors.Item1.R) * (i / max)),
                        (byte)(colors.Item1.G + (colors.Item2.G - colors.Item1.G) * (i / max)),
                        (byte)(colors.Item1.B + (colors.Item2.B - colors.Item1.B) * (i / max))), 1, start, start + new Vector2((int)((rect.Width - HealthBarBorderWidth * 2) * percent), 0));
                }

                // Draw the separating lines
                var step = 1 / MaxHealth;
                var currentStep = step;
                for (var i = 0; i < MaxHealth - 1; i++)
                {
                    // Draw the separator
                    var start = new Vector2((float)(rect.Left + HealthBarBorderWidth + Math.Round(currentStep * (rect.Width - HealthBarBorderWidth * 2))), rect.Top);
                    Drawing.DrawLine(start, start + new Vector2(0, rect.Height), HealthBarPadding, Color.FromArgb(200, HealthBarBackgroundColor));

                    // Increase step
                    currentStep += step;
                }
            }

            public static bool IsWard(Obj_AI_Base obj, out Type wardType)
            {
                // Wards cannot have more than 5 max health
                if (obj.MaxHealth <= 5)
                {
                    // Validate base skin
                    if (!string.IsNullOrWhiteSpace(obj.BaseSkinName))
                    {
                        // Parse the ward type
                        return Enum.TryParse(obj.BaseSkinName, out wardType);
                    }
                }

                wardType = 0;
                return false;
            }
        }
    }

    public static partial class Extensions
    {
        public static ColorBGRA ToBgra(this Color color, byte alpha)
        {
            return new ColorBGRA(color.R, color.G, color.B, alpha);
        }

        public static ColorBGRA ToBgra(this Color color)
        {
            return new ColorBGRA(color.R, color.G, color.B, color.A);
        }
    }
}

namespace UtilitySharp
{        
    class Activator
    {
        private AIHeroClient Hero => ObjectManager.Player;

        #region Anti Clown

        private static readonly Color CloneLineColor = Color.OrangeRed;
        private static readonly Color OriginalLineColor = Color.LimeGreen;
        private const int CloneRevealerLineWidth = 5;

        private static readonly Champion[] CloneChampions =
        {
            Champion.Leblanc,
            Champion.Shaco,
            Champion.MonkeyKing
        };
        private HashSet<AIHeroClient> CurrentCloneChampions { get; set; }
        private HashSet<Obj_AI_Base> CurrentClones { get; set; }

        private static readonly SharpDX.Color FlashColor = SharpDX.Color.Yellow;
        private static readonly SharpDX.Color ShacoColor = SharpDX.Color.Tomato;
        private const int JukeRevealerLineWidth = 5;
        private const int JukeRevealerCircleRadius = 50;
        private const int FlashRange = 425;
        private const int ShacoQRange = 400;

        private List<AIHeroClient> EnemyShaco { get; set; }
                                               
        public CheckBox ShowClones { get; private set; }
        public CheckBox JukeRevealer { get; private set; }
        public Slider JukeTimer { get; private set; }

        public void OnTick(EventArgs args)
        {
            if (CurrentClones.Count > 0)
            {
                CurrentClones.RemoveWhere(o => !o.IsValid || o.IsDead);
            }
        }

        public void OnCreate(GameObject sender, EventArgs args)
        {                                      
            if (CurrentCloneChampions.Count > 0)
            {                                              
                if (sender.IsEnemy)
                {                                                   
                    var baseObject = sender as Obj_AI_Base;
                    if (baseObject != null)
                    {                                                                 
                        if (CurrentCloneChampions.Any(cloneChamp => baseObject.Name == cloneChamp.Name))
                        {                                                   
                            CurrentClones.Add(baseObject);
                        }
                    }
                }
            }
        }

        public static Vector3 GetValidCastSpot(Vector3 castPosition)
        {                                                                   
            var flags = castPosition.ToNavMeshCell().CollFlags;
            if (!flags.HasFlag(CollisionFlags.Wall) && !flags.HasFlag(CollisionFlags.Building))
            {
                return castPosition;
            }

            const int maxRange = 20;
            const double step = 2 * Math.PI / 20;

            var start = new Vector2(castPosition.ToNavMeshCell().GridX, castPosition.ToNavMeshCell().GridY);
            var checkedCells = new HashSet<Vector2>();
                                 
            var directions = new List<Vector2>();
            for (var theta = 0d; theta <= 2 * Math.PI + step; theta += step)
            {
                directions.Add((new Vector2((short)(start.X + Math.Cos(theta)), (short)(start.Y - Math.Sin(theta))) - start).Normalized());
            }

            var validPositions = new HashSet<Vector3>();
            for (var range = 1; range < maxRange; range++)
            {                                                             
                foreach (var direction in directions)
                {                                
                    var end = start + range * direction;
                    var testPoint = new Vector2((short)end.X, (short)end.Y);
                    if (checkedCells.Contains(testPoint))
                    {
                        continue;
                    }
                    checkedCells.Add(testPoint);
                                                          
                    flags = new NavMeshCell((short)testPoint.X, (short)testPoint.Y).CollFlags;
                    if (!flags.HasFlag(CollisionFlags.Wall) && !flags.HasFlag(CollisionFlags.Building))
                    {                                                  
                        validPositions.Add(NavMesh.GridToWorld((short)testPoint.X, (short)testPoint.Y));
                    }
                }

                if (validPositions.Count > 0)
                {                                                               
                    return validPositions.OrderBy(o => o.Distance(start, true)).First();
                }
            }
                                         
            return castPosition;
        }
         
        public void OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.Type == GameObjectType.AIHeroClient)
            {
                var hero = (AIHeroClient)sender;
                if (hero.IsEnemy)
                {                    
                    if (JukeRevealer.CurrentValue)
                    {
                        switch (args.SData.Name)
                        {
                            case "SummonerFlash":
                            case "Deceive":
                                                                    
                                var color = FlashColor;
                                var start = args.Start;
                                var end = start.IsInRange(args.End, FlashRange) ? args.End : start.Extend(args.End, FlashRange).To3DWorld();

                                if (args.SData.Name == "Deceive")
                                {
                                    if (EnemyShaco.Any(o => o.IdEquals(hero)))
                                    {                      
                                        color = ShacoColor;
                                        end = start.IsInRange(args.End, ShacoQRange) ? args.End : start.Extend(args.End, ShacoQRange).To3DWorld();
                                    }
                                    else
                                    {                    
                                        break;
                                    }
                                }
                                                                              
                                end = GetValidCastSpot(end);
                                                          
                                DrawingDraw drawJuke = bla =>
                                {                       
                                    Circle.Draw(color, JukeRevealerCircleRadius, JukeRevealerLineWidth, end);

                                    EloBuddy.SDK.Rendering.Line.DrawLine(Color.FromArgb(color.A, color.R, color.G, color.B), JukeRevealerLineWidth, start,
                                        start.Extend(end, start.Distance(end) - JukeRevealerCircleRadius).To3D((int)end.Z));
                                };
                                                     
                                Drawing.OnDraw += drawJuke;
                                                                                          
                               if (JukeTimer.CurrentValue > 1)
                                {
                                    Core.DelayAction(() =>
                                    {
                                        if (hero.IsHPBarRendered)
                                        {
                                            Drawing.OnDraw -= drawJuke;
                                        }
                                    }, 1000);
                                }
                                                                  
                                Core.DelayAction(() => { Drawing.OnDraw -= drawJuke; }, JukeTimer.CurrentValue * 1000);

                                break;
                        }
                    }
                }
            }
        }

        public static RectangleF GetScreenBoudingRectangle(GameObject obj)
        {
            int minX = 0, maxX = 0, minY = 0, maxY = 0;

            foreach (var corner in obj.BBox.GetCorners())
            {
                var pos = corner.WorldToScreen();
                var x = (int)Math.Round(pos.X);
                var y = (int)Math.Round(pos.Y);
                                                   
                if (minX == 0 || x < minX)
                {
                    minX = x;
                }
                else if (maxX == 0 || x > maxX)
                {
                    maxX = x;
                }
                                                   
                if (minY == 0 || y < minY)
                {
                    minY = y;
                }
                else if (maxY == 0 || y > maxY)
                {
                    maxY = y;
                }
            }

            return new RectangleF(minX, minY, maxX - minX, maxY - minY);
        }

        public void OnDraw(EventArgs args)
        {
            if (ShowClones != null && ShowClones.CurrentValue)
            {                                                             
                foreach (var clone in CurrentClones.Where(o => o.IsVisible && o.IsHPBarRendered))
                {                                            
                    var cloneBounding = GetScreenBoudingRectangle(clone);
                                               
                    var size = Math.Min(cloneBounding.Width, cloneBounding.Height);
                    var halfSize = size / 2;

                    EloBuddy.SDK.Rendering.Line.DrawLine(CloneLineColor, CloneRevealerLineWidth,
                        cloneBounding.Center - halfSize,
                        cloneBounding.Center + halfSize);
                    EloBuddy.SDK.Rendering.Line.DrawLine(CloneLineColor, CloneRevealerLineWidth,
                        cloneBounding.Center + new Vector2(-halfSize, halfSize),
                        cloneBounding.Center + new Vector2(halfSize, -halfSize));
                                                                 
                    var realChamp = EntityManager.Heroes.Enemies.Find(o => o.Name == clone.Name);
                    if (realChamp.IsVisible && realChamp.IsHPBarRendered)
                    {                                                
                        var champBounding = GetScreenBoudingRectangle(realChamp);
                                                
                        size = Math.Min(champBounding.Width, champBounding.Height) / 2;
                        halfSize = size / 2;

                        EloBuddy.SDK.Rendering.Line.DrawLine(OriginalLineColor, CloneRevealerLineWidth,
                            champBounding.TopLeft + new Vector2(0, halfSize),
                            champBounding.TopLeft,
                            champBounding.TopLeft + new Vector2(halfSize, 0));

                        EloBuddy.SDK.Rendering.Line.DrawLine(OriginalLineColor, CloneRevealerLineWidth,
                            champBounding.TopRight + new Vector2(-halfSize, 0),
                            champBounding.TopRight,
                            champBounding.TopRight + new Vector2(0, halfSize));

                        EloBuddy.SDK.Rendering.Line.DrawLine(OriginalLineColor, CloneRevealerLineWidth,
                            champBounding.BottomRight + new Vector2(0, -halfSize),
                            champBounding.BottomRight,
                            champBounding.BottomRight + new Vector2(-halfSize, 0));

                        EloBuddy.SDK.Rendering.Line.DrawLine(OriginalLineColor, CloneRevealerLineWidth,
                            champBounding.BottomLeft + new Vector2(halfSize, 0),
                            champBounding.BottomLeft,
                            champBounding.BottomLeft + new Vector2(0, -halfSize));
                    }
                }
            }
        }
        
          
        #endregion
           
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
            PotionsEnabled;

        private Slider CleanseHumanizerDelay,
            CleanseDurationMin,
            FaceOfTheMountainPercent,
            RedemptionPercent,
            SeraphsPercent,
            ZhonyasValue,
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

            #region InitClone&Shaco

            CurrentCloneChampions = new HashSet<AIHeroClient>();
            CurrentClones = new HashSet<Obj_AI_Base>();
            EnemyShaco = EntityManager.Heroes.Enemies.FindAll(o => o.Hero == Champion.Shaco);  
            
            #endregion


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
            ZhonyasEnabled = menu.Add("ZhonyasEnabled", new CheckBox("Enable Zhonya Usage"));
            ZhonyasValue = menu.Add("ZhonyasValue", new Slider("Zhonya Healthpercent Value", 20));

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
            
            #endregion

            #region POTIONS MENU

            menu.AddGroupLabel("++ Potions");
            PotionsEnabled = menu.Add("PotionsEnabled", new CheckBox("Use Potions"));
            PotionsPercent = menu.Add("PotionsPercent", new Slider("Drink at Health %:{0}", 60, 1, 99));

            #endregion

            #region Anti Clown & Juke

            menu.AddGroupLabel("1. Clone Revealer");
            menu.AddLabel("Reveals the fake enemy champions with a cross, like Shaco clone.");
            if (EntityManager.Heroes.Enemies.Any(o => CloneChampions.Contains(o.Hero)))
            {
                ShowClones = menu.Add("crossClone", new CheckBox("Enabled"));

                foreach (var cloneChamp in EntityManager.Heroes.Enemies.Where(o => CloneChampions.Contains(o.Hero)))
                {
                    // Add clone champ to the current clone champs
                    CurrentCloneChampions.Add(cloneChamp);
                }
            }
            else
            {
                menu.AddLabel(string.Format(" - No clone champions in this match! ({0})", string.Join(" or ", CloneChampions)));
            }
            menu.AddSeparator();

            menu.AddGroupLabel("2. Juke Revealer");
            menu.AddLabel("Reveals jukes, like flashing into brushes or Shaco Q");
            JukeRevealer = menu.Add("juke", new CheckBox("Enabled"));
            JukeTimer = menu.Add("jukeTimer", new Slider("Show juke direction for {0} seconds", 3, 1, 10));
            menu.AddLabel("Note: Once I'm able to check if the team has vision on the end position");
            menu.AddLabel("I will avoid always drawing the spell and instead only draw when there is no vision.");


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

        #region IgniteDamage

        public float GetCustomIgniteDamage()
        {
            switch (Player.Instance.Level)
            {
                case 0:
                {
                    return 70;
                }
                case 2:
                {
                    return 90;
                }
                case 3:
                {
                    return 110;
                }
                case 4:
                {
                    return 130;
                }
                case 5:
                {
                    return 150;
                }
                case 6:
                {
                    return 170;
                }
                case 7:
                {
                    return 190;
                }
                case 8:
                {
                    return 210;
                }
                case 9:
                {
                    return 230;
                }
                case 10:
                {
                    return 250;
                }
                case 11:
                {
                    return 270;
                }
                case 12:
                {
                    return 290;
                }
                case 13:
                {
                    return 310;
                }
                case 14:
                {
                    return 330;
                }
                case 15:
                {
                    return 350;
                }
                case 16:
                {
                    return 370;
                }
                case 17:
                {
                    return 390;
                }
                case 18:
                {
                    return 410;
                }
                default:
                {
                    return 0;  
                }  
            }   
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

            if (Zhonyas.IsOwned() && Zhonyas.IsReady() && ZhonyasEnabled.CurrentValue &&
                Hero.HealthPercent <= ZhonyasValue.CurrentValue && !Hero.IsInShopRange())
            {
                Zhonyas.Cast();
            }

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
            if (IGNITE != null && IgniteKSEnable.CurrentValue)
            {
                if (IGNITE.IsReady())
                {
                    //var BadDudes = EntityManager.Heroes.Enemies;
                    //foreach (var BadDude in BadDudes)
                    //{
                    //    //Console.WriteLine(Hero.GetSummonerSpellDamage(BadDude, DamageLibrary.SummonerSpells.Ignite));
                    //    if (BadDude != null && BadDude.IsValidTarget(600) && BadDude.Health <=
                    //        Hero.GetSummonerSpellDamage(BadDude, DamageLibrary.SummonerSpells.Ignite))
                    //    {
                    //        IGNITE.Cast(BadDude);
                    //    }
                    //}

                    var target = TargetSelector.SelectedTarget != null
                        ? TargetSelector.SelectedTarget
                        : TargetSelector.GetTarget(600, DamageType.True);

                    if (target != null && target.IsValidTarget())
                    {
                        Console.WriteLine(GetCustomIgniteDamage());
                        if (target.TotalShieldHealth() < GetCustomIgniteDamage())
                        {
                            Console.WriteLine("Kek Success");
                            IGNITE.Cast(target);
                            return;
                        }
                        
                        Console.WriteLine("Not yet m8");
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
