using System;
using System.Collections.Generic;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using SharpDX;
using Color = System.Drawing.Color;

namespace UtilitySharp
{
    class AntiClown_Juke
    {
        public AntiClown_Juke(Menu menu)
        {
            #region InitClone&Shaco

            CurrentCloneChampions = new HashSet<AIHeroClient>();
            CurrentClones = new HashSet<Obj_AI_Base>();
            EnemyShaco = EntityManager.Heroes.Enemies.FindAll(o => o.Hero == Champion.Shaco);

            #endregion


            #region Anti Clown & Juke

            menu.AddSubMenu("Anti Clown & Juke");
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

            Game.OnTick += OnTick;
            GameObject.OnCreate += OnCreate;
            Obj_AI_Base.OnProcessSpellCast += OnProcessSpellCast;
            Drawing.OnEndScene += OnDraw;
        }

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
                                    EloBuddy.SDK.Rendering.Circle.Draw(color, JukeRevealerCircleRadius, JukeRevealerLineWidth, end);

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
    }
}
