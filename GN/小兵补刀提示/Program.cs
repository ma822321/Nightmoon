using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace MinionHPBar
{
    internal class Program
    {
        public static Obj_AI_Base Player = ObjectManager.Player;
        public static Menu Menu;

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            Menu = new Menu("花边汉化-DZ191小兵HP", "MinionHPBar", true);
            Menu.AddSubMenu(new Menu("线 圈", "Drawing"));

            //Drawings Menu
            Menu.SubMenu("Drawing")
                .AddItem(new MenuItem("DrawLines", "显示 血条").SetValue(new Circle(true, Color.Black)));
            Menu.SubMenu("Drawing")
                .AddItem(new MenuItem("LastHitH", "补刀 提示").SetValue(new Circle(true, Color.Lime)));
            Menu.SubMenu("Drawing").AddItem(new MenuItem("Thick", "厚度").SetValue(new Slider(2, 1, 3)));
            Menu.SubMenu("Drawing")
                .AddItem(new MenuItem("NearDead", "附近可击杀 线圈").SetValue(new Circle(true, Color.Green)));
            Menu.SubMenu("Drawing")
                .AddItem(new MenuItem("1Hit", "补刀 线圈").SetValue(new Circle(true, Color.Coral)));
            //Range Menu
            Menu.AddSubMenu(new Menu("范 围", "Range"));
            Menu.SubMenu("Range").AddItem(new MenuItem("DrRange", "显示 范围").SetValue(new Slider(850, 450, 1200)));
            Menu.AddToMainMenu();

            //Credits
            Game.PrintChat("MinionHPBars by DZ191");
            Game.PrintChat("鑺辫竟姹夊寲-Dz191灏忓叺琛€鏉¤ˉ鍒€鎻愮ず 鍔犺浇鎴愬姛!");
            //Events
            Drawing.OnDraw += Drawing_OnDraw;
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (!Menu.Item("DrawLines").GetValue<Circle>().Active)
            {
                return;
            }

            var minionList = MinionManager.GetMinions(
                Player.Position, Menu.Item("DrRange").GetValue<Slider>().Value, MinionTypes.All, MinionTeam.Enemy,
                MinionOrderTypes.MaxHealth);
            foreach (
                var minion in
                    minionList.Where(minion => minion.IsValidTarget(Menu.Item("DrRange").GetValue<Slider>().Value)))
            {
                var attackToKill = Math.Ceiling(minion.MaxHealth / Player.GetAutoAttackDamage(minion, true));
                var hpBarPosition = minion.HPBarPosition;
                var barWidth = minion.IsMelee() ? 75 : 80; //Width for melee and ranged
                if (minion.HasBuff("turretshield", true)) //Cannon minion
                {
                    barWidth = 70;
                }

                var barDistance = barWidth / attackToKill;
                for (var i = 0; i < attackToKill; i++)
                {
                    if (i != 0)
                    {
                        Drawing.DrawLine(
                            new Vector2(hpBarPosition.X + 45 + (float) (barDistance) * i, hpBarPosition.Y + 18),
                            new Vector2(hpBarPosition.X + 45 + ((float) (barDistance) * i), hpBarPosition.Y + 23),
                            Menu.Item("Thick").GetValue<Slider>().Value,
                            ((minion.Health <= Player.GetAutoAttackDamage(minion, true) &&
                              Menu.Item("LastHitH").GetValue<Circle>().Active)
                                ? Menu.Item("LastHitH").GetValue<Circle>().Color
                                : Menu.Item("DrawLines").GetValue<Circle>().Color));
                    }
                }

                //Based on LXOrbwalker. Thanks Lexxes
                if (Menu.Item("1Hit").GetValue<Circle>().Active &&
                    minion.Health <= Player.GetAutoAttackDamage(minion, true))
                {
                    Utility.DrawCircle(
                        minion.Position, minion.BoundingRadius, Menu.Item("1Hit").GetValue<Circle>().Color);
                }
                else if (Menu.Item("NearDead").GetValue<Circle>().Active &&
                         minion.Health <= Player.GetAutoAttackDamage(minion, true) * 2)
                {
                    Utility.DrawCircle(
                        minion.Position, minion.BoundingRadius, Menu.Item("NearDead").GetValue<Circle>().Color);
                }
            }
        }
    }
}