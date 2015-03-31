using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using Color = System.Drawing.Color;

namespace PerplexedNidalee
{
    class Config
    {
        public static Menu Settings = new Menu("花边-Perplexed豹女", "menu", true);
        public static Orbwalking.Orbwalker Orbwalker;

        public static void Initialize()
        {
            //Orbwalker
            {
                Settings.AddSubMenu(new Menu("走砍", "orbMenu"));
                Orbwalker = new Orbwalking.Orbwalker(Settings.SubMenu("orbMenu"));
            }
            //Target Selector
            {
                Settings.AddSubMenu(new Menu("目标选择", "ts"));
                TargetSelector.AddToMenu(Settings.SubMenu("ts"));
            }
            //Combo
            {
                Settings.AddSubMenu(new Menu("连招", "menuCombo"));
                Settings.SubMenu("menuCombo").AddSubMenu(new Menu("人形态", "menuComboHuman"));
                Settings.SubMenu("menuCombo").AddSubMenu(new Menu("豹形态", "menuComboCougar"));
                //Human
                {
                    Settings.SubMenu("menuCombo").SubMenu("menuComboHuman").AddItem(new MenuItem("comboJavelin", "Q").SetValue(true));
                    Settings.SubMenu("menuCombo").SubMenu("menuComboHuman").AddItem(new MenuItem("comboBushwhack", "W").SetValue(true));
                }
                //Cougar
                {
                    Settings.SubMenu("menuCombo").SubMenu("menuComboCougar").AddItem(new MenuItem("comboTakedown", "Q").SetValue(true));
                    Settings.SubMenu("menuCombo").SubMenu("menuComboCougar").AddItem(new MenuItem("comboPounce", "W").SetValue(true));
                    Settings.SubMenu("menuCombo").SubMenu("menuComboCougar").AddItem(new MenuItem("comboSwipe", "E").SetValue(true));
                }
            }
            //Harass
            {
                Settings.AddSubMenu(new Menu("骚扰", "menuHarass"));
                Settings.SubMenu("menuHarass").AddSubMenu(new Menu("人形态", "menuHarassHuman"));
                Settings.SubMenu("menuHarass").AddSubMenu(new Menu("豹形态", "menuHarassCougar"));
                //Human
                {
                    Settings.SubMenu("menuHarass").SubMenu("menuHarassHuman").AddItem(new MenuItem("harassJavelin", "Q").SetValue(true));
                    Settings.SubMenu("menuHarass").SubMenu("menuHarassHuman").AddItem(new MenuItem("harassBushwhack", "W").SetValue(false));
                }
                //Cougar
                {
                    Settings.SubMenu("menuHarass").SubMenu("menuHarassCougar").AddItem(new MenuItem("harassTakedown", "Q").SetValue(true));
                    Settings.SubMenu("menuHarass").SubMenu("menuHarassCougar").AddItem(new MenuItem("harassPounce", "W").SetValue(true));
                    Settings.SubMenu("menuHarass").SubMenu("menuHarassCougar").AddItem(new MenuItem("harassSwipe", "E").SetValue(true));
                }
                //Mana
                Settings.SubMenu("menuHarass").AddItem(new MenuItem("harassManaPct", "骚扰最低蓝量比 %").SetValue(new Slider(70, 10, 90)));
            }
            //Heal
            {
                Settings.AddSubMenu(new Menu("E设置", "menuHealing"));
                Settings.SubMenu("menuHealing").AddItem(new MenuItem("enableHeal", "启用").SetValue(true));
                Settings.SubMenu("menuHealing").AddItem(new MenuItem("healSelf", "奶自己").SetValue(true));
                Settings.SubMenu("menuHealing").AddItem(new MenuItem("healAllies", "奶友军").SetValue(true));
                Settings.SubMenu("menuHealing").AddItem(new MenuItem("healPct", "奶人丨目标Hp百分比").SetValue(new Slider(50, 10, 90)));
                Settings.SubMenu("menuHealing").AddItem(new MenuItem("healManaPct", "最低蓝量比 %").SetValue(new Slider(50, 10, 90)));
            }
            //Lane Clear
            {
                Settings.AddSubMenu(new Menu("清线", "menuLaneClear"));
                Settings.SubMenu("menuLaneClear").AddSubMenu(new Menu("人形态", "menuLaneHuman"));
                Settings.SubMenu("menuLaneClear").AddSubMenu(new Menu("豹形态", "menuLaneCougar"));
                //Human
                {
                    Settings.SubMenu("menuLaneClear").SubMenu("menuLaneHuman").AddItem(new MenuItem("laneJavelin", "Q").SetValue(false));
                    Settings.SubMenu("menuLaneClear").SubMenu("menuLaneHuman").AddItem(new MenuItem("laneBushwhack", "W").SetValue(false));
                }
                //Cougar
                {
                    Settings.SubMenu("menuLaneClear").SubMenu("menuLaneCougar").AddItem(new MenuItem("laneTakedown", "Q").SetValue(true));
                    Settings.SubMenu("menuLaneClear").SubMenu("menuLaneCougar").AddItem(new MenuItem("lanePounce", "W").SetValue(true));
                    Settings.SubMenu("menuLaneClear").SubMenu("menuLaneCougar").AddItem(new MenuItem("laneSwipe", "E").SetValue(true));
                }
                //Mana
                Settings.SubMenu("menuLaneClear").AddItem(new MenuItem("laneManaPct", "最低蓝量比%").SetValue(new Slider(70, 10, 90)));
            }
            //Jungle Clear
            {
                Settings.AddSubMenu(new Menu("清野", "menuJungleClear"));
                Settings.SubMenu("menuJungleClear").AddSubMenu(new Menu("人形态", "menuJungleHuman"));
                Settings.SubMenu("menuJungleClear").AddSubMenu(new Menu("豹形态", "menuJungleCougar"));
                //Human
                {
                    Settings.SubMenu("menuJungleClear").SubMenu("menuJungleHuman").AddItem(new MenuItem("jungleJavelin", "Q").SetValue(true));
                    Settings.SubMenu("menuJungleClear").SubMenu("menuJungleHuman").AddItem(new MenuItem("jungleBushwhack", "W").SetValue(false));
                }
                //Cougar
                {
                    Settings.SubMenu("menuJungleClear").SubMenu("menuJungleCougar").AddItem(new MenuItem("jungleTakedown", "Q").SetValue(true));
                    Settings.SubMenu("menuJungleClear").SubMenu("menuJungleCougar").AddItem(new MenuItem("junglePounce", "W").SetValue(true));
                    Settings.SubMenu("menuJungleClear").SubMenu("menuJungleCougar").AddItem(new MenuItem("jungleSwipe", "E").SetValue(true));
                }
                //Mana
                Settings.SubMenu("menuJungleClear").AddItem(new MenuItem("jungleManaPct", "最低蓝量比 %").SetValue(new Slider(70, 10, 90)));
            }
            //Summoners
            {
                Settings.AddSubMenu(new Menu("召唤师技能", "menuSumms"));
                //Heal
                {
                    Settings.SubMenu("menuSumms").AddSubMenu(new Menu("治疗", "summHeal"));
                    Settings.SubMenu("menuSumms").SubMenu("summHeal").AddItem(new MenuItem("useHeal", "启用").SetValue(true));
                    Settings.SubMenu("menuSumms").SubMenu("summHeal").AddItem(new MenuItem("summHealPct", "最低Hp比").SetValue(new Slider(35, 10, 90)));
                }
                //Ignite
                {
                    Settings.SubMenu("menuSumms").AddSubMenu(new Menu("点燃", "summIgnite"));
                    Settings.SubMenu("menuSumms").SubMenu("summIgnite").AddItem(new MenuItem("useIgnite", "启用").SetValue(true));
                    Settings.SubMenu("menuSumms").SubMenu("summIgnite").AddItem(new MenuItem("igniteMode", "使用点燃对象").SetValue(new StringList(new string[] { "Execution", "Combo" })));
                }
            }
            //Items
            {
                Settings.AddSubMenu(new Menu("物品", "menuItems"));
                //Offensive
                {
                    Settings.SubMenu("menuItems").AddSubMenu(new Menu("进攻", "offItems"));
                    foreach (var offItem in ItemManager.Items.Where(item => item.Type == ItemType.Offensive))
                        Settings.SubMenu("menuItems").SubMenu("offItems").AddItem(new MenuItem("use" + offItem.ShortName, offItem.Name).SetValue(true));
                }
                //Defensive
                {
                    Settings.SubMenu("menuItems").AddSubMenu(new Menu("防御", "defItems"));
                    foreach (var defItem in ItemManager.Items.Where(item => item.Type == ItemType.Defensive))
                    {
                        Settings.SubMenu("menuItems").SubMenu("defItems").AddSubMenu(new Menu(defItem.Name, "menu" + defItem.ShortName));
                        Settings.SubMenu("menuItems").SubMenu("defItems").SubMenu("menu" + defItem.ShortName).AddItem(new MenuItem("use" + defItem.ShortName, "启用").SetValue(true));
                        Settings.SubMenu("menuItems").SubMenu("defItems").SubMenu("menu" + defItem.ShortName).AddItem(new MenuItem("pctHealth" + defItem.ShortName, "Hp百分比").SetValue(new Slider(35, 10, 90)));
                    }
                }
                //Cleanse
                {
                    Settings.SubMenu("menuItems").AddSubMenu(new Menu("净化", "cleanseItems"));
                    foreach (var cleanseItem in ItemManager.Items.Where(item => item.Type == ItemType.Cleanse))
                        Settings.SubMenu("menuItems").SubMenu("cleanseItems").AddItem(new MenuItem("use" + cleanseItem.ShortName, cleanseItem.Name).SetValue(true));
                }
            }
            //Anti-Gapcloser
            {
                Settings.AddSubMenu(new Menu("反突进", "menuGapcloser"));
                Settings.SubMenu("menuGapcloser").AddItem(new MenuItem("dodgePounce", "自动W").SetValue(true));
            }
            //Drawing
            {
                Settings.AddSubMenu(new Menu("显示", "menuDrawing"));
                Settings.SubMenu("menuDrawing").AddItem(new MenuItem("drawJavelin", "人类Q范围").SetValue(new Circle(true, Color.Yellow)));
            }
            //Finish
            {
                Settings.AddSubMenu(new Menu("信 息", "message"));
                Settings.SubMenu("message").AddItem(new MenuItem("Sprite", "Perlexed伊泽瑞尔"));
                Settings.SubMenu("message").AddItem(new MenuItem("Hanhua", "汉化:花边下丶情未央"));
                Settings.SubMenu("message").AddItem(new MenuItem("qqqun", "QQ群:299606556"));
            }

            Game.PrintChat("<font color=\"#1eff00\">Huabian婕㈠寲QQ缇わ細299606556</font> - <font color=\"#00BFFF\">姝¤繋鍚勪綅鐨勫姞鍏ワ紒</font>");


            Settings.AddToMainMenu();
        }

        //Combo
        public static bool ComboJavelin { get { return Settings.Item("comboJavelin").GetValue<bool>(); } }
        public static bool ComboBushwhack { get { return Settings.Item("comboBushwhack").GetValue<bool>(); } }
        public static bool ComboTakedown { get { return Settings.Item("comboTakedown").GetValue<bool>(); } }
        public static bool ComboPounce { get { return Settings.Item("comboPounce").GetValue<bool>(); } }
        public static bool ComboSwipe { get { return Settings.Item("comboSwipe").GetValue<bool>(); } }
        
        //Harass
        public static bool HarassJavelin { get { return Settings.Item("harassJavelin").GetValue<bool>(); } }
        public static bool HarassBushwhack { get { return Settings.Item("harassBushwhack").GetValue<bool>(); } }
        public static bool HarassTakedown { get { return Settings.Item("harassTakedown").GetValue<bool>(); } }
        public static bool HarassPounce { get { return Settings.Item("harassPounce").GetValue<bool>(); } }
        public static bool HarassSwipe { get { return Settings.Item("harassBushwhack").GetValue<bool>(); } }
        public static int HarassManaPct { get { return Settings.Item("harassManaPct").GetValue<Slider>().Value; } }

        //Heal
        public static bool EnableHeal { get { return Settings.Item("enableHeal").GetValue<bool>(); } }
        public static bool HealSelf { get { return Settings.Item("healSelf").GetValue<bool>(); } }
        public static bool HealAllies { get { return Settings.Item("healAllies").GetValue<bool>(); } }
        public static int HealPct { get { return Settings.Item("healPct").GetValue<Slider>().Value; } }
        public static int HealManaPct { get { return Settings.Item("healManaPct").GetValue<Slider>().Value; } }

        //Lane Clear
        public static bool LaneJavelin { get { return Settings.Item("laneJavelin").GetValue<bool>(); } }
        public static bool LaneBushwhack { get { return Settings.Item("laneBushwhack").GetValue<bool>(); } }
        public static bool LaneTakedown { get { return Settings.Item("laneTakedown").GetValue<bool>(); } }
        public static bool LanePounce { get { return Settings.Item("lanePounce").GetValue<bool>(); } }
        public static bool LaneSwipe { get { return Settings.Item("laneSwipe").GetValue<bool>(); } }
        public static int LaneManaPct { get { return Settings.Item("laneManaPct").GetValue<Slider>().Value; } }

        //Jungle Clear
        public static bool JungleJavelin { get { return Settings.Item("jungleJavelin").GetValue<bool>(); } }
        public static bool JungleBushwhack { get { return Settings.Item("jungleBushwhack").GetValue<bool>(); } }
        public static bool JungleTakedown { get { return Settings.Item("jungleTakedown").GetValue<bool>(); } }
        public static bool JunglePounce { get { return Settings.Item("junglePounce").GetValue<bool>(); } }
        public static bool JungleSwipe { get { return Settings.Item("jungleSwipe").GetValue<bool>(); } }
        public static int JungleManaPct { get { return Settings.Item("jungleManaPct").GetValue<Slider>().Value; } }

        //Summoners
        public static bool UseSummHeal { get { return Settings.Item("useHeal").GetValue<bool>(); } }
        public static int SummHealPct { get { return Settings.Item("summHealPct").GetValue<Slider>().Value; } }
        public static bool UseIgnite { get { return Settings.Item("useIgnite").GetValue<bool>(); } }
        public static string IgniteMode { get { return Settings.Item("igniteMode").GetValue<StringList>().SelectedValue; } }

        //Items
        public static bool ShouldUseItem(string shortName)
        {
            return Settings.Item("use" + shortName).GetValue<bool>();
        }
        public static int UseOnPercent(string shortName)
        {
            return Settings.Item("pctHealth" + shortName).GetValue<Slider>().Value;
        }
        
        //Anti-Gapcloser
        public static bool DodgeWithPounce { get { return Settings.Item("dodgePounce").GetValue<bool>(); } }

        //Drawing
        public static Circle DrawJavelin { get { return Settings.Item("drawJavelin").GetValue<Circle>(); } }
    }
}
