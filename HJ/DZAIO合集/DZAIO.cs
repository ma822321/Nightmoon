using System;
using System.Collections.Generic;
using System.Reflection;
using DZAIO;
using DZAIO.Champions;
using DZAIO.Utility;
using DZAIO.Utility.Drawing;
using DZAIO.Utility.Helpers;
using LeagueSharp;
using LeagueSharp.Common;

namespace DZAIO
{
    class DZAIO
    {
        /**
         ____ _____   _    ___ ___  
        |  _ \__  /  / \  |_ _/ _ \ 
        | | | |/ /  / _ \  | | | | |
        | |_| / /_ / ___ \ | | |_| |
        |____/____/_/   \_\___\___/ 
         */
                            
        public static Dictionary<String, Func<IChampion>> ChampList = new Dictionary<string, Func<IChampion>>
        { 
           {"Jinx",() => new Jinx()},
           {"Graves",() => new Graves()},
           {"Zilean",() => new Zilean()},
           {"Lux",() => new Lux()},
           {"Cassiopeia",() => new Cassiopeia()},
           {"Caitlyn", () => new Caitlyn()}
        };
        public static Menu Config { get; set; }
        public static Orbwalking.Orbwalker Orbwalker { get; set; }
        public static Obj_AI_Hero Player { get; set; }
        public static IChampion CurrentChampion { get; set; }

        public static bool IsDebug = false;
        public static int Revision = 7;

        public static void OnLoad(EventArgs args)
        {
            Player = ObjectManager.Player;
            Config = new Menu("花边-DzAio合集", "dzaio", true);
            TargetSelector.AddToMenu(Config.SubMenu("目标选择"));
            Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("走砍"));
            Game.PrintChat("<b><font color='#FF0000'>[DZAIO]</font></b><font color='#FFFFFF'> DZAIO Loaded!</font> v{0}", Assembly.GetExecutingAssembly().GetName().Version);
            
            if (ChampList.ContainsKey(Player.ChampionName))
            {
                CurrentChampion = ChampList[Player.ChampionName].Invoke();
                CurrentChampion.OnLoad(Config);
                CurrentChampion.SetUpSpells();
                CurrentChampion.RegisterEvents();
                ItemManager.OnLoad(Config);
                Game.PrintChat("<b><font color='#FF0000'>[DZAIO] </font></b><font color='#FFFFFF'>Loaded</font> <b><font color='#FF0000'>{0}</font></b> plugin! <font color='#FFFFFF'> Have fun! </font>", Player.ChampionName);
            }
            Game.PrintChat("<b><font color='#FF0000'>[DZAIO] </font></b><font color='#FFFFFF'>Special credits to:</font>  <b><font color='#FF0000'>Hellsing</font></b>, <b><font color='#FF0000'>ChewyMoon</font></b> ");

            Cleanser.OnLoad();
            ChatHook.OnLoad();
            DebugHelper.OnLoad();
            NotificationDrawing.OnLoad();
            PotionManager.OnLoad(Config);
            Config.AddItem(new MenuItem("dzaio.hpdraw.disable", "禁用显示伤害").SetValue(false));
            var aboutMenu = new Menu(Player.ChampionName + " - 关于", "dzaio.about");
            {
                aboutMenu.AddItem(new MenuItem("dzaio.about.line1", "作者： Asuna/DZ191"));
                aboutMenu.AddItem(new MenuItem("dzaio.about.line2", "v." + Assembly.GetExecutingAssembly().GetName().Version));
                aboutMenu.AddItem(new MenuItem("dzaio.about.line4", "如果你喜欢"));
                aboutMenu.AddItem(new MenuItem("dzaio.about.line5", "请捐赠:"));
                aboutMenu.AddItem(new MenuItem("dzaio.about.line6", "dz1917@yahoo.it"));
            }
            Config.AddSubMenu(aboutMenu);

            Config.AddToMainMenu();
        }
    }
}
