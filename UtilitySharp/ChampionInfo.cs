using System;
using System.Collections.Generic;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Rendering;
using SharpDX;

namespace UtilitySharp
{
    class ChampionInfo
    {
        public AIHeroClient Hero { get; set; }

        public Vector3 LastVisablePos { get; set; }
        public float LastVisableTime { get; set; }
        public Vector3 PredictedPos { get; set; }
        public Vector3 LastWayPoint { get; set; }

        public bool IsJungler { get; set; }
        public Sprite NormalSprite;
        public Sprite HudSprite;
        public Sprite MinimapSprite;
        public Sprite SquareSprite;

        public ChampionInfo(AIHeroClient hero)
        {
            Hero = hero;
            if (hero.IsEnemy)
                NormalSprite = ImageDownloader.CreateRadrarIcon(hero.ChampionName + "_Square_0", System.Drawing.Color.Red);
            else
                NormalSprite = ImageDownloader.CreateRadrarIcon(hero.ChampionName + "_Square_0", System.Drawing.Color.GreenYellow);
            SquareSprite = ImageDownloader.GetSprite(hero.ChampionName + "_Square_0");
            HudSprite = ImageDownloader.CreateRadrarIcon(hero.ChampionName + "_Square_0", System.Drawing.Color.DarkGoldenrod, 100);
            MinimapSprite = ImageDownloader.CreateMinimapSprite(hero.ChampionName + "_Square_0");
            MinimapSprite.CenterRef = new Vector3(-15f, -15f, 0f);
            LastVisableTime = Game.Time;
            LastVisablePos = hero.Position;
            PredictedPos = hero.Position;
            IsJungler = hero.Spellbook.Spells.Any(spell => spell.Name.ToLower().Contains("smite")); 

        }
    }

    class InfoLoader
    {
        public static HashSet<ChampionInfo> ChampionInfoList = new HashSet<ChampionInfo>();

        private Vector3 EnemySpawn;

        public void Load()
        {
            EnemySpawn = ObjectManager.Get<Obj_SpawnPoint>().FirstOrDefault(x => x.IsEnemy).Position;
            foreach (var hero in EntityManager.Heroes.AllHeroes)
            {
                ChampionInfoList.Add(new ChampionInfo(hero));
            }

            Game.OnUpdate += OnUpdate;
        } 

        private void OnUpdate(EventArgs args)
        {
            foreach (var extra in ChampionInfoList.Where(x => x.Hero.IsEnemy))
            {
                var enemy = extra.Hero;
                if (enemy.IsDead)
                {
                    extra.LastVisablePos = EnemySpawn;
                    extra.LastVisableTime = Game.Time;
                    extra.PredictedPos = EnemySpawn;
                    extra.LastWayPoint = EnemySpawn;
                }
                else if (enemy.IsVisible)
                {
                    extra.LastWayPoint = extra.Hero.Path.LastOrDefault();
                    extra.PredictedPos = enemy.Position.Extend(extra.LastWayPoint, 125).To3D();
                    extra.LastVisablePos = enemy.Position;
                    extra.LastVisableTime = Game.Time;
                }
            }
        }
    }
}
