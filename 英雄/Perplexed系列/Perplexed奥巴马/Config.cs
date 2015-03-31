using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using Color = System.Drawing.Color;

namespace PerplexedLucian
{
    class Config
    {
        public static Menu Settings = new Menu("花边-Perplexed奥巴马", "menu", true);
        public static Orbwalking.Orbwalker Orbwalker;

        public static void Initialize()
        {
            //Orbwalker
            Settings.AddSubMenu(new Menu("走砍", "orbMenu"));
            Orbwalker = new Orbwalking.Orbwalker(Settings.SubMenu("orbMenu"));
            //Target Selector
            Settings.AddSubMenu(new Menu("目标选择", "ts"));
            TargetSelector.AddToMenu(Settings.SubMenu("ts"));
            //Combo
            Settings.AddSubMenu(new Menu("连招", "menuCombo"));
            Settings.SubMenu("menuCombo").AddItem(new MenuItem("comboQ", "Q").SetValue(true));
            Settings.SubMenu("menuCombo").AddItem(new MenuItem("comboW", "W").SetValue(true));
            Settings.SubMenu("menuCombo").AddItem(new MenuItem("comboE", "E").SetValue(true));
            Settings.SubMenu("menuCombo").AddItem(new MenuItem("comboR", "R").SetValue(true));
            Settings.SubMenu("menuCombo").AddItem(new MenuItem("eIntoTurret", "塔下E").SetValue(true));
            //Harass
            Settings.AddSubMenu(new Menu("骚扰", "menuHarass"));
            Settings.SubMenu("menuHarass").AddItem(new MenuItem( "harassQ", "Q").SetValue(true));
            //Summoners
            Settings.AddSubMenu(new Menu("召唤师技能", "menuSumms"));
            Settings.SubMenu("menuSumms").AddSubMenu(new Menu("治疗", "summHeal"));
            Settings.SubMenu("menuSumms").SubMenu("summHeal").AddItem(new MenuItem("useHeal", "开 启").SetValue(true));
            Settings.SubMenu("menuSumms").SubMenu("summHeal").AddItem(new MenuItem("healPct", "使用HP %").SetValue(new Slider(35, 10, 90)));
            Settings.SubMenu("menuSumms").AddSubMenu(new Menu("点燃", "summIgnite"));
            Settings.SubMenu("menuSumms").SubMenu("summIgnite").AddItem(new MenuItem("useIgnite", "开 启").SetValue(true));
            Settings.SubMenu("menuSumms").SubMenu("summIgnite").AddItem(new MenuItem("igniteMode", "使用模式").SetValue(new StringList(new string[] { "执 行", "连 招" })));
            //Items
            Settings.AddSubMenu(new Menu("物品", "menuItems"));
            Settings.SubMenu("menuItems").AddSubMenu(new Menu("进 攻", "offItems"));
            foreach (var offItem in ItemManager.Items.Where(item => item.Type == ItemType.Offensive))
                Settings.SubMenu("menuItems").SubMenu("offItems").AddItem(new MenuItem("use" + offItem.ShortName, offItem.Name).SetValue(true));
            Settings.SubMenu("menuItems").AddSubMenu(new Menu("净 化", "cleanseItems"));
            foreach (var cleanseItem in ItemManager.Items.Where(item => item.Type == ItemType.Cleanse))
                Settings.SubMenu("menuItems").SubMenu("cleanseItems").AddItem(new MenuItem("use" + cleanseItem.ShortName, cleanseItem.Name).SetValue(true));
            //Drawing
            Settings.AddSubMenu(new Menu("范 围", "menuDrawing"));
            Settings.SubMenu("menuDrawing").AddSubMenu(new Menu("伤害 指示", "menuDamage"));
            Settings.SubMenu("menuDrawing").SubMenu("menuDamage").AddItem(new MenuItem("drawAADmg", "显示 自动 攻击 伤害").SetValue(true));
            Settings.SubMenu("menuDrawing").SubMenu("menuDamage").AddItem(new MenuItem("drawQDmg", "显示 Q 伤害").SetValue(true));
            Settings.SubMenu("menuDrawing").SubMenu("menuDamage").AddItem(new MenuItem("drawWDmg", "显示 W 伤害").SetValue(true));
            Settings.SubMenu("menuDrawing").SubMenu("menuDamage").AddItem(new MenuItem("drawRDmg", "显示 R 伤害").SetValue(true));
            Settings.SubMenu("menuDrawing").AddItem(new MenuItem("drawQ", "显示 Q 范围").SetValue(new Circle(true, Color.Yellow)));
            Settings.SubMenu("menuDrawing").AddItem(new MenuItem("drawQ2", "显示 额外Q 范围").SetValue(new Circle(true, Color.Yellow)));
            Settings.SubMenu("menuDrawing").AddItem(new MenuItem("drawW", "显示 W 范围").SetValue(new Circle(true, Color.Yellow)));
            Settings.SubMenu("menuDrawing").AddItem(new MenuItem("drawE", "显示 E 范围").SetValue(new Circle(true, Color.Yellow)));
            Settings.SubMenu("menuDrawing").AddItem(new MenuItem("drawR", "显示 R 范围").SetValue(new Circle(true, Color.Yellow)));
            //Other
            Settings.AddItem(new MenuItem("checkPassive", "检查 通知").SetValue(true));
            Settings.AddItem(new MenuItem("usePackets", "使用 封包").SetValue(true));
            //Finish
            Settings.AddSubMenu(new Menu("信 息", "message"));
            Settings.SubMenu("message").AddItem(new MenuItem("Sprite", "Perlexed伊泽瑞尔"));
            Settings.SubMenu("message").AddItem(new MenuItem("Hanhua", "汉化:花边下丶情未央"));
            Settings.SubMenu("message").AddItem(new MenuItem("qqqun", "QQ群:299606556"));


            Game.PrintChat("<font color=\"#1eff00\">Huabian婕㈠寲QQ缇わ細299606556</font> - <font color=\"#00BFFF\">姝¤繋鍚勪綅鐨勫姞鍏ワ紒</font>");


            Settings.AddToMainMenu();
        }

        public static bool ComboQ { get { return Settings.Item("comboQ").GetValue<bool>(); } }
        public static bool ComboW { get { return Settings.Item("comboW").GetValue<bool>(); } }
        public static bool ComboE { get { return Settings.Item("comboE").GetValue<bool>(); } }
        public static bool ComboR { get { return Settings.Item("comboR").GetValue<bool>(); } }
        public static bool EIntoTurret { get { return Settings.Item("eIntoTurret").GetValue<bool>(); } }

        public static bool HarassQ { get { return Settings.Item("harassQ").GetValue<bool>(); } }

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
        public static bool DrawRDmg { get { return Settings.Item("drawRDmg").GetValue<bool>(); } }
        public static Circle DrawQ { get { return Settings.Item("drawQ").GetValue<Circle>(); } }
        public static Circle DrawQ2 { get { return Settings.Item("drawQ2").GetValue<Circle>(); } }
        public static Circle DrawW { get { return Settings.Item("drawW").GetValue<Circle>(); } }
        public static Circle DrawE { get { return Settings.Item("drawE").GetValue<Circle>(); } }
        public static Circle DrawR { get { return Settings.Item("drawR").GetValue<Circle>(); } }

        public static bool CheckPassive { get { return Settings.Item("checkPassive").GetValue<bool>(); } }
        public static bool UsePackets { get { return Settings.Item("usePackets").GetValue<bool>(); } }
    }
}
