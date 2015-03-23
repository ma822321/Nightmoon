using System;
using LeagueSharp;
using LeagueSharp.Common;

namespace Prince_Urgot
{
    internal class Program
    {
        private static Obj_AI_Hero Player
        {
            get { return ObjectManager.Player; }
        }

        internal static Menu UrgotConfig;
        internal static Menu TargetSelectorMenu;
        internal static Orbwalking.Orbwalker Orbwalker;

        public static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += OnGameLoad;
        }

        private static void OnGameLoad(EventArgs args)
        {
            if (Player.ChampionName != "Urgot")
            {
                return;
            }

            MainMenu();
            new SpellClass();

            Game.PrintChat("<b><font color =\"#FFFFFF\">Prince Urgot</font></b><font color =\"#FFFFFF\"> by </font><b><font color=\"#FF66FF\">Leia</font></b><font color =\"#FFFFFF\"> loaded!</font>");
            Game.PrintChat("鑺辫竟姹夊寲-Prince铻冭煿鍔犺浇鎴愬姛!");
        }

        static void MainMenu()
        {
            UrgotConfig = new Menu("花边汉化-Prince螃蟹", "Prince" + ObjectManager.Player.ChampionName, true);

            UrgotConfig.AddSubMenu(new Menu("走 砍", "Orbwalking"));
            Orbwalker = new Orbwalking.Orbwalker(UrgotConfig.SubMenu("Orbwalking"));

            TargetSelectorMenu = new Menu("目标 选择", "Common_TargetSelector");
            TargetSelector.AddToMenu(TargetSelectorMenu);
            UrgotConfig.AddSubMenu(TargetSelectorMenu);

            new ComboClass(UrgotConfig);
            new HarassClass(UrgotConfig);
            new LaneClearClass(UrgotConfig);
            new ItemClass(UrgotConfig);
            new DrawingClass(UrgotConfig);
            UrgotConfig.AddToMainMenu();
        }
    }
}