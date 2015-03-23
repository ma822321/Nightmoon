using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using Color = System.Drawing.Color;

namespace PerplexedEzreal
{
    class Config
    {
        public static Menu Settings = new Menu("花边汉化-Perplexed伊泽瑞尔", "menu", true);
        public static Orbwalking.Orbwalker Orbwalker;

        public static string[] Marksmen = { "Kalista", "Jinx", "Lucian", "Quinn", "Draven",  "Varus", "Graves", "Vayne", "Caitlyn",
                                                                    "Urgot", "Ezreal", "KogMaw", "Ashe", "MissFortune", "Tristana", "Teemo", "Sivir",
                                                                    "Twitch", "Corki"};

        public static void Initialize()
        {
            //Orbwalker
            Settings.AddSubMenu(new Menu("走 砍", "orbMenu"));
            Orbwalker = new Orbwalking.Orbwalker(Settings.SubMenu("orbMenu"));
            //Target Selector
            Settings.AddSubMenu(new Menu("目标 选择", "ts"));
            TargetSelector.AddToMenu(Settings.SubMenu("ts"));
            //Combo
            Settings.AddSubMenu(new Menu("连 招", "menuCombo"));
            Settings.SubMenu("menuCombo").AddItem(new MenuItem("comboQ", "Q").SetValue(true));
            Settings.SubMenu("menuCombo").AddItem(new MenuItem("comboW", "W").SetValue(true));
            //Harass
            Settings.AddSubMenu(new Menu("骚 扰", "menuHarass"));
            Settings.SubMenu("menuHarass").AddItem(new MenuItem("harassQ", "Q").SetValue(true));
            Settings.SubMenu("menuHarass").AddItem(new MenuItem("harassW", "W").SetValue(true));
            //Last Hit
            Settings.AddSubMenu(new Menu("补 刀", "menuLastHit"));
            Settings.SubMenu("menuLastHit").AddItem(new MenuItem("lastHitQ", "Q").SetValue(true));
            //Auto
            Settings.AddSubMenu(new Menu("自 动", "menuAuto"));
            Settings.SubMenu("menuAuto").AddItem(new MenuItem("toggleAuto", "自动 目标").SetValue(new KeyBind("H".ToCharArray()[0], KeyBindType.Toggle, true)));
            Settings.SubMenu("menuAuto").AddSubMenu(new Menu("英雄 名单", "autoChamps"));
            foreach (Obj_AI_Hero hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsValid && hero.IsEnemy))
                Settings.SubMenu("menuAuto").SubMenu("autoChamps").AddItem(new MenuItem("auto" + hero.ChampionName, hero.ChampionName).SetValue(true));
            Settings.SubMenu("menuAuto").AddItem(new MenuItem("autoQ", "Q").SetValue(true));
            Settings.SubMenu("menuAuto").AddItem(new MenuItem("autoW", "W").SetValue<bool>(false));
            Settings.SubMenu("menuAuto").AddItem(new MenuItem("manaER", "保存蓝量使用 E/R").SetValue(true));
            Settings.SubMenu("menuAuto").AddItem(new MenuItem("autoTurret", "塔下自动使用技能").SetValue<bool>(false));
            //Ultimate
            Settings.AddSubMenu(new Menu("大招 设置", "menuUlt"));
            Settings.SubMenu("menuUlt").AddItem(new MenuItem("ultLowest", "低血量 自动R").SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Press)));
            Settings.SubMenu("menuUlt").AddItem(new MenuItem("ks", "抢人头 自动R").SetValue(true));
            Settings.SubMenu("menuUlt").AddItem(new MenuItem("ultRange", "大招 范围").SetValue<Slider>(new Slider(1000, 1000, 5000)));
            //Summoners
            Settings.AddSubMenu(new Menu("召唤师 技能", "menuSumms"));
            Settings.SubMenu("menuSumms").AddSubMenu(new Menu("治疗", "summHeal"));
            Settings.SubMenu("menuSumms").SubMenu("summHeal").AddItem(new MenuItem("useHeal", "开 启").SetValue(true));
            Settings.SubMenu("menuSumms").SubMenu("summHeal").AddItem(new MenuItem("healPct", "使用HP %").SetValue(new Slider(35, 10, 90)));
            Settings.SubMenu("menuSumms").AddSubMenu(new Menu("点燃", "summIgnite"));
            Settings.SubMenu("menuSumms").SubMenu("summIgnite").AddItem(new MenuItem("useIgnite", "开 启").SetValue(true));
            Settings.SubMenu("menuSumms").SubMenu("summIgnite").AddItem(new MenuItem("igniteMode", "使用模式").SetValue(new StringList(new string[] { "执 行", "连 招" })));
            //Items
            Settings.AddSubMenu(new Menu("物 品", "menuItems"));
            Settings.SubMenu("menuItems").AddSubMenu(new Menu("进 攻", "offItems"));
            foreach (var offItem in ItemManager.Items.Where(item => item.Type == ItemType.Offensive))
                Settings.SubMenu("menuItems").SubMenu("offItems").AddItem(new MenuItem("use" + offItem.ShortName, offItem.Name).SetValue(true));
            Settings.SubMenu("menuItems").AddSubMenu(new Menu("防 守", "defItems"));
            foreach (var defItem in ItemManager.Items.Where(item => item.Type == ItemType.Defensive))
            {
                Settings.SubMenu("menuItems").SubMenu("defItems").AddSubMenu(new Menu(defItem.Name, "菜单:" + defItem.ShortName));
                Settings.SubMenu("menuItems").SubMenu("defItems").SubMenu("menu" + defItem.ShortName).AddItem(new MenuItem("use" + defItem.ShortName, "开 启").SetValue(true));
                Settings.SubMenu("menuItems").SubMenu("defItems").SubMenu("menu" + defItem.ShortName).AddItem(new MenuItem("pctHealth" + defItem.ShortName, "使用HP %").SetValue(new Slider(35, 10, 90)));
            }
            Settings.SubMenu("menuItems").AddSubMenu(new Menu("净 化", "cleanseItems"));
            foreach (var cleanseItem in ItemManager.Items.Where(item => item.Type == ItemType.Cleanse))
                Settings.SubMenu("menuItems").SubMenu("cleanseItems").AddItem(new MenuItem("use" + cleanseItem.ShortName, cleanseItem.Name).SetValue(true));
            //Drawing
            Settings.AddSubMenu(new Menu("范 围", "menuDrawing"));
            Settings.SubMenu("menuDrawing").AddSubMenu(new Menu("Damage Indicator", "范围 菜单"));
            Settings.SubMenu("menuDrawing").SubMenu("menuDamage").AddItem(new MenuItem("drawAADmg", "自动攻击 范围").SetValue(true));
            Settings.SubMenu("menuDrawing").SubMenu("menuDamage").AddItem(new MenuItem("drawQDmg", "显示 Q 范围").SetValue(true));
            Settings.SubMenu("menuDrawing").SubMenu("menuDamage").AddItem(new MenuItem("drawWDmg", "显示 W 范围").SetValue(true));
            Settings.SubMenu("menuDrawing").SubMenu("menuDamage").AddItem(new MenuItem("drawEDmg", "显示 E 范围").SetValue(true));
            Settings.SubMenu("menuDrawing").SubMenu("menuDamage").AddItem(new MenuItem("drawRDmg", "显示 R 范围").SetValue(true));
            Settings.SubMenu("menuDrawing").AddItem(new MenuItem("drawQ", "显示 Q 范围").SetValue(new Circle(true, Color.Yellow)));
            Settings.SubMenu("menuDrawing").AddItem(new MenuItem("drawW", "显示 W 范围").SetValue(new Circle(true, Color.Yellow)));
            Settings.SubMenu("menuDrawing").AddItem(new MenuItem("drawR", "显示 R 范围").SetValue(new Circle(true, Color.Yellow)));
            //Other
            Settings.AddItem(new MenuItem("dmgMode", "伤害 模式").SetValue(new StringList(new string[] { "AD", "AP" })));
            Settings.AddItem(new MenuItem("recallBlock", "打断 回城").SetValue(true));
            Settings.AddItem(new MenuItem("usePackets", "使用 封包").SetValue(true));
            //Finish
            Settings.AddToMainMenu();
        }

        public static bool ComboQ { get { return Settings.Item("comboQ").GetValue<bool>(); } }
        public static bool ComboW { get { return Settings.Item("comboW").GetValue<bool>(); } }

        public static bool HarassQ { get { return Settings.Item("harassQ").GetValue<bool>(); } }
        public static bool HarassW { get { return Settings.Item("harassW").GetValue<bool>(); } }

        public static bool LastHitQ { get { return Settings.Item("lastHitQ").GetValue<bool>(); } }

        public static KeyBind UltLowest { get { return Settings.Item("ultLowest").GetValue<KeyBind>(); } }
        public static bool KillSteal { get { return Settings.Item("ks").GetValue<bool>(); } }
        public static int UltRange { get { return Settings.Item("ultRange").GetValue<Slider>().Value; } }

        public static KeyBind ToggleAuto { get { return Settings.Item("toggleAuto").GetValue<KeyBind>(); } }
        public static bool ShouldAuto(string championName)
        {
            return Settings.Item("auto" + championName).GetValue<bool>();
        }
        public static bool AutoQ { get { return Settings.Item("autoQ").GetValue<bool>(); } }
        public static bool AutoW { get { return Settings.Item("autoW").GetValue<bool>(); } }
        public static bool ManaER { get { return Settings.Item("manaER").GetValue<bool>(); } }
        public static bool AutoTurret { get { return Settings.Item("autoTurret").GetValue<bool>(); } }

        public static bool UseHeal { get { return Settings.Item("useHeal").GetValue<bool>(); } }
        public static int HealPct { get { return Settings.Item("healPct").GetValue<Slider>().Value; } }
        public static bool UseIgnite { get { return Settings.Item("useIgnite").GetValue<bool>(); } }
        public static string IgniteMode { get { return Settings.Item("igniteMode").GetValue<StringList>().SelectedValue; } }

        public static bool ShouldUseItem(string shortName)
        {
            return Settings.Item("use" + shortName).GetValue<bool>();
        }
        public static int UseOnPercent(string shortName)
        {
            return Settings.Item("pctHealth" + shortName).GetValue<Slider>().Value;
        }

        public static bool DrawAADmg { get { return Settings.Item("drawAADmg").GetValue<bool>(); } }
        public static bool DrawQDmg { get { return Settings.Item("drawQDmg").GetValue<bool>(); } }
        public static bool DrawWDmg { get { return Settings.Item("drawWDmg").GetValue<bool>(); } }
        public static bool DrawEDmg { get { return Settings.Item("drawEDmg").GetValue<bool>(); } }
        public static bool DrawRDmg { get { return Settings.Item("drawRDmg").GetValue<bool>(); } }
        public static bool DrawQ { get { return Settings.Item("drawQ").GetValue<Circle>().Active; } }
        public static bool DrawW { get { return Settings.Item("drawW").GetValue<Circle>().Active; } }
        public static bool DrawR { get { return Settings.Item("drawR").GetValue<Circle>().Active; } }

        public static string DamageMode { get { return Settings.Item("dmgMode").GetValue<StringList>().SelectedValue; } }
        public static bool RecallBlock { get { return Settings.Item("recallBlock").GetValue<bool>(); } }
        public static bool UsePackets { get { return Settings.Item("usePackets").GetValue<bool>(); } }
    }
}
