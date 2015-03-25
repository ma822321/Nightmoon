#region
using System;
using System.Collections;
using System.Linq;
using Color = System.Drawing.Color;
using System.Collections.Generic;
using System.Threading;

using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Veigar__The_Tiny_Master_Of_Evil.Properties;
//using KeMinimap;
#endregion

namespace Veigar__The_Tiny_Master_Of_Evil
{
    class Program
    {
        public const string ChampionName = "Veigar";
        public static bool boughtItemOne;                   //Var to check if bought starting items
        public static Obj_AI_Hero ChoosedTarget = null;
        private static int TargetLockCD;
        public static Obj_AI_Base _m = null;
        private static Obj_AI_Hero Player;
        private static Obj_AI_Hero Target = null;
        private static Obj_AI_Hero WimmTarget = null;
        private static Obj_AI_Hero WimmTargett = null;
        //private static Int32 LastSkin;
        public static int Orb = 0;
        public static int ComboStarted = 0;
        public static float Delay = 0f;
        public static float Delay1 = 0f;
        public static string Ccombo = null;
        public static float Delayy = 0f;
        public static float Delayy1 = 0f;
        public static string Cccombo = null;

        //HUD
        public static List<HUD> HUDlist = new List<HUD>
        {
            new HUD()
            {
                DisplayTextON = "閫ｆ嫑 : On", DisplayTextOFF = "閫ｆ嫑 : Off", MenuText = "鏄剧ず閫ｆ嫑", MenuComboText = "閫ｆ嫑"
            },
            new HUD()
            {
                DisplayTextON = "楱锋摼 : On", DisplayTextOFF = "楱锋摼 : Off", MenuText = "鏄剧ず 楱锋摼", MenuComboText = "楱锋摼"
            },
            new HUD()
            {
                DisplayTextON = "浣跨敤鍏ㄩ儴 : On", DisplayTextOFF = "浣跨敤鍏ㄩ儴 : Off", MenuText = "鏄剧ず 浣跨敤鍏ㄩ儴", MenuComboText = "浣跨敤鍏ㄩ儴"
            },
            new HUD()
            {
                DisplayTextON = "Q 瑁滃垁 : On", DisplayTextOFF = "Q 瑁滃垁: Off", MenuText = "鏄剧ず Q 瑁滃垁", MenuComboText = "Q 瑁滃垁"
            },
            new HUD()
            {
                DisplayTextON = "鑷嫊 鎿婃 : On", DisplayTextOFF = "鑷嫊 鎿婃 : Off", MenuText = "鏄剧ず 鑷嫊 鎿婃", MenuComboText = "鑷嫊 鎿婃"
            },
            new HUD()
            {
                DisplayTextON = "鐪╂殘 : On", DisplayTextOFF = "鐪╂殘 : Off", MenuText = "鏄剧ず 鐪╂殘", MenuComboText = "鐪╂殘"
            },
            new HUD()
            {
                DisplayTextON = "娓呯窔W : On", DisplayTextOFF = "娓呯窔W : Off", MenuText = "鏄剧ず 娓呯窔W", MenuComboText = "娓呯窔W"
            },
        };

        //Buffs
        public static List<NewBuff> buffList = new List<NewBuff>
        {
            
            new NewBuff()
            {
                MenuName = "濉炴仼 琚嫊", DisplayName = "SionPassiveZombie", Name = "sionpassivezombie"
            },
            new NewBuff()
            {
                MenuName = "鐗涢牠 澶ф嫑", DisplayName = "Trample Buff", Name = "alistartrample"
            },
            new NewBuff()
            {
                MenuName = "鍐伴偿 琚嫊", DisplayName = "rebirth", Name = "Rebirth"
            },
            new NewBuff()
            {
                MenuName = "鍔嶉瓟 琚嫊", DisplayName = "aatroxpassivedeath", Name = "AatroxPassiveReady"
            },
            new NewBuff()
            {
                MenuName = "绱厠 琚嫊", DisplayName = "zacrebirthready", Name = "ZacRebirthReady"
            },
            new NewBuff()
            {
                MenuName = "绱厠 琚嫊", DisplayName = "JudicatorIntervention", Name = "JudicatorIntervention"
            },
            new NewBuff()
            {
                MenuName = "鍐板コ 澶ф嫑", DisplayName = "lissandrarself", Name = "LissandraRSelf"
            },
            new NewBuff()
            {
                MenuName = "娉㈡瘮 澶ф嫑", DisplayName = "PoppyDiplomaticImmunity", Name = "PoppyDiplomaticImmunity"
            },
            new NewBuff()
            {
                MenuName = "锠诲瓙 澶ф嫑", DisplayName = "UndyingRage", Name = "Undying Rage"
            },
            new NewBuff()
            {
                MenuName = "甯冮殕 璀风浘", DisplayName = "braumeshieldbuff", Name = "BraumShieldRaise"
            },
            new NewBuff()
            {
                MenuName = "瀹堣 澶╀娇", DisplayName = "Guardian Angel", Name = "willrevive"
            },
        };

        public static List<NewIgnore> IgnoreList = new List<NewIgnore>
        {
            new NewIgnore()
            {
                MenuName = "濂冲", DisplayName = "BansheesVeil", Name = "bansheesveil"
            },
            new NewIgnore()
            {
                MenuName = "澶㈤瓨 璀风浘", DisplayName = "NocturneShroudofDarkness", Name = "NocturneShroudofDarknessShield"
            },
            new NewIgnore()
            {
                MenuName = "澧惤澶╀娇 璀风浘", DisplayName = "Black Shield", Name = "BlackShield"
            },
            new NewIgnore()
            {
                MenuName = "鎴扮埈濂崇 璀风浘", DisplayName = "SivirE", Name = "Spell Shield"
            },
            new NewIgnore()
            {
                MenuName = "灏忛瓪 E", DisplayName = "fizztrickslamsounddummy", Name = "fizztrickslamsounddummy"
            },
                        new NewIgnore()
            {
                MenuName = "鍚歌Ghost W", DisplayName = "VladimirSanguinePool", Name = "VladimirSanguinePool"
            },
        };

        //Spells
        public static List<Spell> SpellList = new List<Spell>();

        public static Spell Q;
        public static Spell W;
        public static Spell E;
        public static Spell R;

        //Collision
        private static int _wallCastT;
        private static Vector2 _yasuoWallCastedPos;
        private static GameObject _yasuoWall;

        //Damage
        private static Dictionary<Obj_AI_Hero, int> enemyDictionary = new Dictionary<Obj_AI_Hero, int>();
        private static Dictionary<Obj_AI_Hero, string> enemyDictionary1 = new Dictionary<Obj_AI_Hero, string>();

        //Ignite
        public static SpellSlot IgniteSlot;

        //Items
        public static Items.Item biscuit = new Items.Item(2010, 10);
        public static Items.Item HPpot = new Items.Item(2003, 10);
        public static Items.Item Flask = new Items.Item(2041, 10);

        //Mana Manager
        public static int[] qMana = { 0, 60, 65, 70, 75, 80 };
        public static int[] wMana = { 0, 70, 75, 80, 85, 90 };
        public static int[] eMana = { 0, 80, 85, 90, 95, 100 };
        public static int[] rMana = { 0, 125, 125, 125 };

        public static int ManaMode = 0;
        public static int NeededCD = 0;

        //Orbwalker instance
        private static Orbwalking.Orbwalker Orbwalker;

        //Menu
        public static Menu menu;

        private static Menu orbwalkerMenu;

        private static void Main(string[] args)
        {
            //potato
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
            var sprite = new Render.Sprite(Properties.Resources.Sprite, new Vector2(Drawing.Width * 0.83f, Drawing.Height * 0.33f));
            sprite.VisibleCondition += s => Render.OnScreen(Drawing.WorldToScreen(Player.Position)) && menu.Item("Show").GetValue<bool>();
            sprite.Scale = new Vector2(1f, 1f);
            sprite.Add();
            Game.OnUpdate += eventArgs =>
            {
                if (sprite != null && Game.ClockTime >= 50)
                {
                    sprite.Dispose();
                    sprite = null;
                }
            };
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            Player = ObjectManager.Player;

            // Check if the champion is Veigar or not.
            if (Player.BaseSkinName != ChampionName) return;

            //Initializing Spells
            Q = new Spell(SpellSlot.Q, 850);
            W = new Spell(SpellSlot.W, 900);
            E = new Spell(SpellSlot.E, 1005);
            R = new Spell(SpellSlot.R, 650);

            Q.SetSkillshot(0.25f, 70f, 1200f, false, SkillshotType.SkillshotLine);
            W.SetSkillshot(1.25f, 230f, float.MaxValue, false, SkillshotType.SkillshotCircle);
            E.SetSkillshot(.2f, 330f, float.MaxValue, false, SkillshotType.SkillshotCircle);

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);

            IgniteSlot = Player.GetSpellSlot("SummonerDot");

            //Menu
            menu = new Menu("花边汉化-Deto小法", ChampionName, true);
            orbwalkerMenu = new Menu("走 砍", "Orbwalker");

            //Target selector
            var targetSelectorMenu = new Menu("目标 选择", "Target Selector");
            TargetSelector.AddToMenu(targetSelectorMenu);
            menu.AddSubMenu(targetSelectorMenu);

            //Orbwalker
            menu.AddSubMenu(orbwalkerMenu);
            chooseOrbwalker(true);

            //Keys & Combo Related
            menu.AddSubMenu(new Menu("按 键", "Keys"));
            menu.SubMenu("Keys");
            menu.SubMenu("Keys").AddItem(new MenuItem("Combo", "智能 连招").SetValue(new KeyBind(32, KeyBindType.Press, false)));
            menu.SubMenu("Keys").AddItem(new MenuItem("AllInActive", "使用 所有 技能").SetValue(new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));
            menu.SubMenu("Keys").AddItem(new MenuItem("HarassActive", "骚 扰").SetValue(new KeyBind("Z".ToCharArray()[0], KeyBindType.Press)));
            menu.SubMenu("Keys").AddItem(new MenuItem("Stun Closest Enemy", "眩晕附近敌人").SetValue(new KeyBind("A".ToCharArray()[0], KeyBindType.Press, false)));
            menu.SubMenu("Keys").AddItem(new MenuItem("LastHitQQ", "补刀 Q[自动]").SetValue(new KeyBind("J".ToCharArray()[0], KeyBindType.Toggle)));
            menu.SubMenu("Keys").AddItem(new MenuItem("LastHitQ", "补刀 Q[按键]").SetValue(new KeyBind("I".ToCharArray()[0], KeyBindType.Press, false)));
            menu.SubMenu("Keys").AddItem(new MenuItem("LastHitWW", "清线 W").SetValue(new KeyBind("K".ToCharArray()[0], KeyBindType.Press, false)));
            menu.SubMenu("Keys").AddItem(new MenuItem("JungleActive", "清野").SetValue(new KeyBind("H".ToCharArray()[0], KeyBindType.Press, false)));
            menu.SubMenu("Keys").AddItem(new MenuItem("ExtraNeeded", "显示 额外伤害").SetValue(new KeyBind("N".ToCharArray()[0], KeyBindType.Toggle, true)));
            menu.SubMenu("Keys").AddItem(new MenuItem("InfoTable", "显示 信息[FPS]").SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Toggle)));

            //Drawings menu:
            menu.AddSubMenu(new Menu("范 围", "Drawings"));
            menu.SubMenu("Drawings").SubMenu("HUD Settings").AddItem(new MenuItem("HUDdisplay", "显示HUD").SetValue(true));
            menu.SubMenu("Drawings").SubMenu("HUD Settings").AddItem(new MenuItem("HUDX", "X 坐标").SetValue(new Slider(67, 0, 100)));
            menu.SubMenu("Drawings").SubMenu("HUD Settings").AddItem(new MenuItem("HUDY", "Y 坐标").SetValue(new Slider(86, 0, 100)));
            foreach (var hud in HUDlist.Where(hud => hud.MenuText != "Display KS Status" && hud.MenuText != "Display Stun Closest Status" && hud.MenuText != "Display LaneClear Status"))
                menu.SubMenu("Drawings").SubMenu("HUD Settings").AddItem(new MenuItem("U" + hud.MenuText, hud.MenuText).SetValue(true));
            foreach (var hud in HUDlist.Where(hud => hud.MenuText == "Display KS Status" || hud.MenuText == "Display Stun Closest Status" || hud.MenuText == "Display LaneClear Status"))
                menu.SubMenu("Drawings").SubMenu("HUD Settings").AddItem(new MenuItem("U" + hud.MenuText, hud.MenuText).SetValue(false));

            menu.SubMenu("Drawings").AddItem(new MenuItem("QRange", "Q,R 范围").SetValue(new Circle(true, Color.FromArgb(255, 0, 255, 0))));
            menu.SubMenu("Drawings").AddItem(new MenuItem("WRange", "W 范围").SetValue(new Circle(false, Color.FromArgb(100, 255, 0, 255))));
            menu.SubMenu("Drawings").AddItem(new MenuItem("ERange", "E 范围").SetValue(new Circle(true, Color.FromArgb(255, 255, 0, 255))));
            menu.SubMenu("Drawings").AddItem(new MenuItem("MinionMarker", "标记 Q 可击杀的小兵").SetValue(new Circle(true, Color.Green)));
            menu.SubMenu("Drawings").AddItem(new MenuItem("TText", "线圈 标记目标").SetValue(true));
            menu.SubMenu("Drawings").AddItem(new MenuItem("LText", "显示 锁定 目标[血条]").SetValue(true));
            menu.SubMenu("Drawings").AddItem(new MenuItem("ExtraNeeded1", "增加 血条 指示 E/N").SetValue(true));
            menu.SubMenu("Drawings").AddItem(new MenuItem("OptimalCombo", "显示 最强 连招[FPS]").SetValue(false));

            //Mana Manager menu:
            menu.AddSubMenu(new Menu("蓝量 管理", "manam"));
            menu.SubMenu("manam").AddItem(new MenuItem("manaStatus", "显示 蓝量 状态").SetValue(true));
            menu.SubMenu("manam").AddItem(new MenuItem("wusage", "清线丨最低蓝量").SetValue(new Slider(0)));
            menu.SubMenu("manam").AddItem(new MenuItem("qusage", "Q补刀丨最低蓝量").SetValue(new Slider(0)));
            menu.SubMenu("manam").AddItem(new MenuItem("husage", "骚扰丨最低蓝量").SetValue(new Slider(0)));
            menu.SubMenu("manam").AddItem(new MenuItem("SaveEW", "保存蓝使用E(清线)").SetValue(false));
            menu.SubMenu("manam").AddItem(new MenuItem("SaveE", "Save Mana for E(Q farm)").SetValue(false));
            menu.SubMenu("manam").AddItem(new MenuItem("SaveEH", "保存蓝使用E(骚扰)").SetValue(false));

            //Misc menu:
            menu.AddSubMenu(new Menu("额 外", "Other"));
            menu.SubMenu("Other").AddSubMenu(new Menu("自动 W 设置", "wsets"));
            menu.SubMenu("Other").SubMenu("wsets").AddItem(new MenuItem("Wimm", "开启 自动 W丨目标有debuff").SetValue(true));
            menu.SubMenu("Other").SubMenu("wsets").AddItem(new MenuItem("Wimmz", "使用 W丨不受伤害").SetValue(true));
            menu.SubMenu("Other").SubMenu("wsets").AddItem(new MenuItem("DontWimm", "禁止 自动 W丨连招时").SetValue(true));
            menu.SubMenu("Other").AddItem(new MenuItem("StunUnderTower", "敌人在塔下丨自动眩晕").SetValue(true));
            menu.SubMenu("Other").AddItem(new MenuItem("UseInt", "使用 E丨打断技能").SetValue(true));
            menu.SubMenu("Other").AddItem(new MenuItem("UseGap", "使用 E丨阻止突进").SetValue(true));
            menu.SubMenu("Other").AddItem(new MenuItem("PotOnIGN", "被点燃丨使用红药").SetValue(true));
            menu.SubMenu("Other").AddItem(new MenuItem("buystart", "购买 开始物品").SetValue(new KeyBind("P".ToCharArray()[0], KeyBindType.Press, false)));
            menu.SubMenu("Other").AddItem(new MenuItem("Reset", "取消 锁定目标").SetValue(new KeyBind("L".ToCharArray()[0], KeyBindType.Press, false)));
            menu.SubMenu("Other").AddItem(new MenuItem("packet", "使用 封包").SetValue(true));
            menu.SubMenu("Other").AddItem(new MenuItem("Show", "显示 脚本").SetValue(true));
            //menu.SubMenu("Other").AddItem(new MenuItem("skinch", "启用 换肤").SetValue(false));
            //menu.SubMenu("Other").AddItem(new MenuItem("skinechm", "皮肤 更换").SetValue(new Slider(5, 1, 8)));

            //Notifications
            menu.AddSubMenu(new Menu("通 知", "notes"));
            menu.SubMenu("notes").AddItem(new MenuItem("noteactive", "开启 文本提示").SetValue(true));
            menu.SubMenu("notes").AddItem(new MenuItem("notems", "显示你的 ms < 目标 ms[连招]").SetValue(true));
            menu.SubMenu("notes").AddItem(new MenuItem("notebuffs", "显示 目标被禁止的buff").SetValue(false));
            menu.SubMenu("notes").AddItem(new MenuItem("notetenacity", "显示  E 眩晕 < W 延迟").SetValue(true));


            //Farm menu:
            menu.AddSubMenu(new Menu("打 钱", "Farm"));
            menu.SubMenu("Farm").AddItem(new MenuItem("dontfarm", "禁止Q丨在连招时").SetValue(true));
            menu.SubMenu("Farm").AddItem(new MenuItem("OnlySiege", "只补大车刀").SetValue(false));
            menu.SubMenu("Farm").AddItem(new MenuItem("WAmount", "使用W丨最低小兵数").SetValue(new Slider(3, 1, 7)));
            menu.SubMenu("Farm").AddItem(new MenuItem("FarmMove", "鼠标附近指示").SetValue(new StringList(new[] { "禁止", "清线", "Q 补刀", "清线 & Q 补刀" }, 0)));

            //Jungle Farm menu:
            menu.AddSubMenu(new Menu("清 野", "Jungle Clear"));
            menu.SubMenu("Jungle Clear").AddItem(new MenuItem("UseAAJungle", "使用 AA").SetValue(true));
            menu.SubMenu("Jungle Clear").AddItem(new MenuItem("UseQJungle", "使用 Q").SetValue(true));
            menu.SubMenu("Jungle Clear").AddItem(new MenuItem("UseWJungle", "使用 W").SetValue(true));
            menu.SubMenu("Jungle Clear").AddItem(new MenuItem("UseEJungle", "使用 E").SetValue(true));

            //Auto KS
            menu.AddSubMenu(new Menu("自动击杀", "AutoKS"));
            menu.SubMenu("AutoKS").AddItem(new MenuItem("UseQKS", "使用 Q").SetValue(true));
            menu.SubMenu("AutoKS").AddItem(new MenuItem("UseWKS", "使用 W").SetValue(false));
            menu.SubMenu("AutoKS").AddItem(new MenuItem("UseRKS", "使用 R").SetValue(false));
            menu.SubMenu("AutoKS").AddItem(new MenuItem("UseIGNKS", "使用 点燃").SetValue(false));
            menu.SubMenu("AutoKS").AddItem(new MenuItem("DisableKS", "禁止击杀丨连招时").SetValue(false));
            menu.SubMenu("AutoKS").AddItem(new MenuItem("AutoKST", "击杀 (自动)!").SetValue(new KeyBind("U".ToCharArray()[0], KeyBindType.Toggle, true)));

            //Harass menu:
            menu.AddSubMenu(new Menu("骚 扰", "Harass"));
            menu.SubMenu("Harass").AddItem(new MenuItem("HarassMode", "选择骚扰模式").SetValue(new StringList(new[] { "E+W+Q", "E+W", "Q", "禁止" }, 0)));
            menu.SubMenu("Harass").AddItem(new MenuItem("WaitW", "先W后Q").SetValue(false));

            //Combo menu:
            menu.AddSubMenu(new Menu("连招丨切入", "Combo"));
            menu.SubMenu("Combo").AddItem(new MenuItem("LockTargets", "鼠标点击目标锁定").SetValue(true));
            menu.SubMenu("Combo").AddItem(new MenuItem("DontEShields", "目标有法术护盾丨禁止E").SetValue(true));
            menu.SubMenu("Combo").AddItem(new MenuItem("ToOrb", "[Test功能]连招走砍").SetValue(false));
            menu.SubMenu("Combo").AddItem(new MenuItem("CastMode", "释放EW顺序").SetValue(new StringList(new[] { "先E后W", "先W后E", }, 0)));
            menu.SubMenu("Combo").AddSubMenu(new Menu("智能 连招 设置", "MainCombo"));
            menu.SubMenu("Combo").SubMenu("MainCombo").SubMenu("假如连招时W打中").AddItem(new MenuItem("ComboWaitMode", "选择 模式:").SetValue(new StringList(new[] { "等待W命中", "不等待命中", }, 0)));
            menu.SubMenu("Combo").SubMenu("MainCombo").SubMenu("假如连招时W打中").AddItem(new MenuItem("IgnoreQ", "跟上Q").SetValue(false));
            menu.SubMenu("Combo").SubMenu("MainCombo").SubMenu("假如连招时W打中").AddItem(new MenuItem("IgnoreR", "跟上R").SetValue(false));
            menu.SubMenu("Combo").SubMenu("MainCombo").SubMenu("假如连招时W打中").AddItem(new MenuItem("IgnoreIGN", "跟上点燃").SetValue(false));

            menu.SubMenu("Combo").SubMenu("MainCombo").AddSubMenu(new Menu("击杀目标 血量比例 %", "op"));
            foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.Team != Player.Team))
                menu.SubMenu("Combo").SubMenu("MainCombo").SubMenu("op").AddItem(new MenuItem("op" + enemy.BaseSkinName, enemy.BaseSkinName).SetValue(new Slider(0)));

            menu.SubMenu("Combo").SubMenu("MainCombo").AddSubMenu(new Menu("忽 略", "il"));
            foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.Team != Player.Team))
                menu.SubMenu("Combo").SubMenu("MainCombo").SubMenu("il").AddItem(new MenuItem("il" + enemy.BaseSkinName, enemy.BaseSkinName).SetValue(false));
            menu.SubMenu("Combo").SubMenu("MainCombo").AddItem(new MenuItem("Priority", "Saving priority").SetValue(new StringList(new[] { "点燃 > R", "R > 点燃" }, 0)));
            menu.SubMenu("Combo").SubMenu("MainCombo").AddItem(new MenuItem("ComboMode", "Combo config for unkillable targets").SetValue(new StringList(new[] { "禁止", "选择 骚扰模式", "Q", "E+W", "E+W+Q", }, 0)));
            menu.SubMenu("Combo").AddSubMenu(new Menu("目标被点燃禁止R", "DontRIGN"));
            foreach (var buff in buffList)
                menu.SubMenu("Combo").SubMenu("DontRIGN").AddItem(new MenuItem("dont" + buff.Name, buff.MenuName).SetValue(true));
            foreach (var buff in IgnoreList.Where(buff => buff.MenuName != "Sivir Shield" && buff.MenuName != "Fizz E" && buff.MenuName != "Vladimir W"))
                menu.SubMenu("Combo").SubMenu("DontRIGN").AddItem(new MenuItem("dont" + buff.Name, buff.MenuName).SetValue(true));
            menu.SubMenu("Combo").SubMenu("DontRIGN").AddItem(new MenuItem("YasuoWall", "亚索 风墙").SetValue(true));

            menu.AddToMainMenu();

            //if (menu.Item("skinch").GetValue<bool>())
            //{
            //    GenModelPacket(Player.ChampionName, menu.Item("skinchm").GetValue<Slider>().Value);
            //    LastSkin = menu.Item("skinchm").GetValue<Slider>().Value;
            //}

            //Events
            GameObject.OnCreate += OnCreate;
            GameObject.OnDelete += OnDelete;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
            Game.OnUpdate += Game_OnGameUpdate;
            Game.OnWndProc += Game_OnWndProc;
            GameObject.OnCreate += TowerAttackOnCreate;
            Drawing.OnDraw += Drawing_OnDraw;
            Interrupter2.OnInterruptableTarget += Interrupter_OnPosibleToInterrupt;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Game.PrintChat("Veigar, The Tiny Master Of Evil Loaded! Made by DedToto");
        }

        public static void CastQ(Obj_AI_Hero target)
        {
            var prediction = Q.GetPrediction(target, true);
            var minions = prediction.CollisionObjects.Count(thing => thing.IsMinion);

            if (minions > 1)
            {
                return;
            }

            Q.Cast(prediction.CastPosition, Packets());
        }

        public static void CastQ(Obj_AI_Base target)
        {
            var prediction = Q.GetPrediction(target, true);
            var minions = prediction.CollisionObjects.Count(thing => thing.IsMinion);

            if (minions > 2)
            {
                return;
            }

            Q.Cast(prediction.CastPosition, Packets());
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            #region ComboShetOnUpdate
            if (Delay != 0f)
            {
                //int I = Environment.TickCount;
                //Drawing.DrawText(Player.HPBarPosition.X + 55, Player.HPBarPosition.Y + 50, System.Drawing.Color.LightGreen, "Combo:" + Ccombo + "(" + (Environment.TickCount - Delay1) + "/" + Delay + ")");

                if (Ccombo.Contains("IGN"))
                {
                    int I = 0;
                    if (Ccombo.Contains("E")) I += 200;
                    if (Ccombo.Contains("W")) I += 1400;
                    if (Ccombo.Contains("Q")) I += 600;
                    if (Ccombo.Contains("R")) I += 600;
                    if (Environment.TickCount - Delay1 > I && Player.Spellbook.CanUseSpell(IgniteSlot) == SpellState.Ready)
                    {
                        Delay = 0f;
                        Delay1 = 0f;
                        Ccombo = null;
                        I = 0;
                    }
                }
                if (Environment.TickCount - Delay1 > Delay)
                {
                    Delay = 0f;
                    Delay1 = 0f;
                    Ccombo = null;
                }
            }
            #endregion

            #region OnUpdate
            if (ChoosedTarget != null && ChoosedTarget.IsDead || !menu.Item("LockTargets").GetValue<bool>())
                ChoosedTarget = null;

            Target = GetTarget();

            var point = Player.ServerPosition + 300 * (Game.CursorPos.To2D() - Player.ServerPosition.To2D()).Normalized().To3D();

            //check if player is dead
            if (Player.IsDead) return;

            if (menu.Item("Reset").GetValue<KeyBind>().Active)
            {
                ChoosedTarget = null;
            }

            if (menu.Item("PotOnIGN").GetValue<bool>())
            {
                AutoHP();
            }

            if (menu.Item("buystart").GetValue<KeyBind>().Active)
            {
                BuyItems();
            }

            if (!menu.Item("Combo").GetValue<KeyBind>().Active)
            {
                if (menu.Item("JungleActive").GetValue<KeyBind>().Active)
                {
                    JungleFarm();
                }

                if (menu.Item("HarassActive").GetValue<KeyBind>().Active)
                {
                    if (Player.Mana / Player.MaxMana * 100 < menu.Item("husage").GetValue<Slider>().Value) return;
                    if (menu.Item("SaveEH").GetValue<bool>() && !HasMana(false, false, true, false)) return;
                    Harass();
                }

                if (menu.Item("LastHitWW").GetValue<KeyBind>().Active)
                {
                    if (Player.Mana / Player.MaxMana * 100 < menu.Item("wusage").GetValue<Slider>().Value) return;
                    if (menu.Item("SaveEW").GetValue<bool>() && !HasMana(false, false, true, false)) return;
                    lastHitW();
                    if (menu.Item("FarmMove").GetValue<StringList>().SelectedIndex == 1 || menu.Item("FarmMove").GetValue<StringList>().SelectedIndex == 3) if (Player.ServerPosition.Distance(Game.CursorPos) > 55) Player.IssueOrder(GameObjectOrder.MoveTo, point);
                }

                if (menu.Item("AllInActive").GetValue<KeyBind>().Active)
                {
                    AllIn();
                }

                if (menu.Item("Stun Closest Enemy").GetValue<KeyBind>().Active)
                {
                    //if (Player.ServerPosition.Distance(Game.CursorPos) > 55 && !KeMinimap.Minimap.MouseOver) Player.IssueOrder(GameObjectOrder.MoveTo, point);
                    //if (E.IsReady())
                    //{
                    //    castE(GetNearestEnemy(Player));
                    //}
                    //if(Target != null)
                    //{
                    //    CastQ(Target);
                    //}
                    if (E.IsReady())
                    {
                        var targets = ObjectManager.Get<Obj_AI_Hero>().Where(h => h.IsValidTarget(E.Range + E.Width / 2)).OrderBy(h => h.Distance(Player, true));

                        if (targets.Count() > 0)
                        {
                            foreach (var target in targets)
                            {
                                castE(target);
                            }
                        }
                    }
                    if (menu.Item("ToOrb").GetValue<bool>()) if (Player.ServerPosition.Distance(Game.CursorPos) > 80) Player.IssueOrder(GameObjectOrder.MoveTo, point);
                }
            }
            else
            {
                Combo();
                //if (menu.Item("ToOrb").GetValue<bool>()) if (Orb == 2) xSLx_Orbwalker.xSLxOrbwalker.Orbwalk(Game.CursorPos, Target); else if (Orb == 1) Orbwalking.Orbwalk(Target, Game.CursorPos);
            }

            if (menu.Item("AutoKST").GetValue<KeyBind>().Active)
            {
                AutoKS();
            }

            if (menu.Item("manaStatus").GetValue<bool>())
            {
                ManaMode = manaCheck().Item1;
                NeededCD = manaCheck().Item2;
            }

            if (menu.Item("ExtraNeeded").GetValue<KeyBind>().Active)
            {
                enemyDictionary = ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.IsEnemy && enemy.IsValidTarget()).ToDictionary(enemy => enemy, enemy => GetExtraNeeded(enemy).Item1);
            }

            if (menu.Item("InfoTable").GetValue<KeyBind>().Active || menu.Item("OptimalCombo").GetValue<bool>())
            {
                enemyDictionary1 = ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.IsEnemy && enemy.IsValidTarget() && menu.Item("il" + enemy.BaseSkinName).GetValue<bool>() == false).ToDictionary(enemy => enemy, enemy => GetBestCombo(enemy, "Table"));
            }

            if (menu.Item("LastHitQQ").GetValue<KeyBind>().Active || menu.Item("LastHitQ").GetValue<KeyBind>().Active)
            {
                if (Player.Mana / Player.MaxMana * 100 < menu.Item("qusage").GetValue<Slider>().Value) return;
                if (menu.Item("SaveE").GetValue<bool>() && !HasMana(false, false, true, false)) return;
                if (menu.Item("AllInActive").GetValue<KeyBind>().Active || menu.Item("Stun Closest Enemy").GetValue<KeyBind>().Active || menu.Item("HarassActive").GetValue<KeyBind>().Active || menu.Item("Combo").GetValue<KeyBind>().Active && menu.Item("dontfarm").GetValue<bool>()) return;
                if (menu.Item("OnlySiege").GetValue<bool>())
                {
                    _m = MinionManager.GetMinions(Q.Range, MinionTypes.All, MinionTeam.Enemy, MinionOrderTypes.MaxHealth).FirstOrDefault(m => m.BaseSkinName.Contains("Siege") && m.Health < ((Player.GetSpellDamage(m, SpellSlot.Q))) && HealthPrediction.GetHealthPrediction(m, (int)(Player.Distance(m, false) / Q.Speed), (int)(Q.Delay * 1000 + Game.Ping / 2)) > 0);
                }
                else
                {
                    _m = MinionManager.GetMinions(Q.Range, MinionTypes.All, MinionTeam.Enemy, MinionOrderTypes.MaxHealth).FirstOrDefault(m => m.Health < ((Player.GetSpellDamage(m, SpellSlot.Q))) && HealthPrediction.GetHealthPrediction(m, (int)(Player.Distance(m, false) / Q.Speed), (int)(Q.Delay * 1000 + Game.Ping / 2)) > 0);
                }
                lastHit();
                if (menu.Item("FarmMove").GetValue<StringList>().SelectedIndex == 2 || menu.Item("FarmMove").GetValue<StringList>().SelectedIndex == 3) if (Player.ServerPosition.Distance(Game.CursorPos) > 55) Player.IssueOrder(GameObjectOrder.MoveTo, point);
            }

            if (menu.Item("Wimm").GetValue<bool>())
            {
                WimmTarget = ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.IsEnemy && enemy.IsValidTarget()).FirstOrDefault(m => m.IsValidTarget(E.Range) && m.Buffs.Where(b => b.IsActive && Game.Time < b.EndTime && b.Name != "VeigarStun" && (b.Type == BuffType.Charm || b.Type == BuffType.Knockback || b.Type == BuffType.Stun || b.Type == BuffType.Suppression || b.Type == BuffType.Snare)).Aggregate(0f, (current, buff) => Math.Max(current, buff.EndTime)) - Game.Time >= W.Delay);
                if (WimmTarget != null && !WimmTarget.HasBuff("VeigarStun") && !menu.Item("Stun Closest Enemy").GetValue<KeyBind>().Active)
                    W.Cast(WimmTarget);
            }
            if (menu.Item("Wimm").GetValue<bool>() && menu.Item("Wimmz").GetValue<bool>())
            {
                WimmTargett = ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.IsEnemy && enemy.IsValidTarget()).FirstOrDefault(m => m.IsValidTarget(E.Range) && m.Buffs.Where(b => b.IsActive && Game.Time < b.EndTime && b.Name == "zhonyasringshield").Aggregate(0f, (current, buff) => Math.Max(current, buff.EndTime)) - Game.Time < W.Delay);
                if (WimmTargett != null && WimmTargett.Buffs.Where(b => b.Name == "zhonyasringshield").Aggregate(0f, (current, buff) => Math.Max(current, buff.EndTime)) - Game.Time > W.Delay - 1f)
                {
                    W.Cast(WimmTargett);
                }
            }
            #endregion
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            //Drawing.DrawText(Player.HPBarPosition.X - 30, Player.HPBarPosition.Y + 10, System.Drawing.Color.Red, "E<W" + Player.PercentTenacityCharacterMod);

            //check if player is dead
            if (Player.IsDead) return;

            if (menu.Item("noteactive").GetValue<bool>())
            {
                var y = 0f;
                foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.IsEnemy && enemy.IsValidTarget()))
                {
                    if (menu.Item("notebuffs").GetValue<bool>() && menu.Item("Combo").GetValue<KeyBind>().Active)
                    {
                        //Game.PrintChat("actually");
                        if (HasBuffs(enemy))
                        {
                            foreach (var buff in buffList.Where(buff => enemy.HasBuff(buff.DisplayName) || enemy.HasBuff(buff.Name)))
                            {
                                Game.PrintChat("actually" + buff.MenuName);
                                Drawing.DrawText(enemy.HPBarPosition.X + 9, enemy.HPBarPosition.Y + 37 + y, System.Drawing.Color.Red, "" + buff.MenuName);
                                y += 11f;
                            }
                        }
                    }
                    if (menu.Item("notetenacity").GetValue<bool>())
                    {
                        if (!tenacitycheck(enemy))
                        {
                            Drawing.DrawText(enemy.HPBarPosition.X + 143, enemy.HPBarPosition.Y + 20, System.Drawing.Color.Red, "E<W");
                        }
                    }
                }
            }

            if (Target != null && Target.IsVisible)
            {
                if (ChoosedTarget != null && menu.Item("LockTargets").GetValue<bool>())
                {
                    if (menu.Item("TText").GetValue<bool>()) Render.Circle.DrawCircle(ChoosedTarget.Position, 75, Color.LightGreen);
                    Drawing.DrawText(Player.HPBarPosition.X + 55, Player.HPBarPosition.Y + 25, System.Drawing.Color.LightGreen, "Lock:" + ChoosedTarget.ChampionName);
                }
                else
                {
                    if (menu.Item("TText").GetValue<bool>()) Render.Circle.DrawCircle(Target.Position, 75, Color.Red);
                }
            }
            else if (Target != null)
            {
                if (ChoosedTarget != null && menu.Item("LockTargets").GetValue<bool>())
                {
                    if (menu.Item("LText").GetValue<bool>()) Drawing.DrawText(Player.HPBarPosition.X + 55, Player.HPBarPosition.Y + 25, System.Drawing.Color.LightGreen, "Lock:" + ChoosedTarget.ChampionName);
                }
            }

            //Draw Extra or Needed for kill damage
            if (menu.Item("ExtraNeeded").GetValue<KeyBind>().Active)
            {
                foreach (Obj_AI_Hero enemy in enemyDictionary.Keys)
                {
                    if (enemy.IsVisible && !enemy.IsDead)
                    {
                        string Text = GetExtraNeeded(enemy).Item2;
                        float ENdamage = 0f;
                        if (enemyDictionary[enemy] >= 1000)
                        {
                            int Integer = enemyDictionary[enemy];
                            float Floater = Integer;
                            ENdamage = (float)Math.Round((Floater / 1000), 1);
                        }
                        else
                        {
                            ENdamage = enemyDictionary[enemy];
                        }
                        if (Text == "Extra:")
                        {
                            if (enemyDictionary[enemy] >= 1000)
                                Drawing.DrawText(enemy.HPBarPosition.X + 7, enemy.HPBarPosition.Y + 1, Color.Red, "Killable(" + ENdamage + "k+)");
                            else
                                Drawing.DrawText(enemy.HPBarPosition.X + 7, enemy.HPBarPosition.Y + 1, Color.Red, "Killable(" + ENdamage + ")");
                            if (menu.Item("ExtraNeeded1").GetValue<bool>())
                            {
                                var hpBarPos = enemy.HPBarPosition;

                                hpBarPos.X += 45;
                                hpBarPos.Y += 18;

                                var combodamage = GetComboDamage(enemy, "Cunts", true, true, true, true, true);

                                var PercentHPleftAfterCombo = (enemy.Health - combodamage) / enemy.MaxHealth;
                                var PercentHPleft = enemy.Health / enemy.MaxHealth;
                                if (PercentHPleftAfterCombo < 0) PercentHPleftAfterCombo = 0;

                                double comboXPos = hpBarPos.X - 36 + (107 * PercentHPleftAfterCombo);
                                double currentHpxPos = hpBarPos.X - 36 + (107 * PercentHPleft);

                                var barcolor = Color.FromArgb(100, 220, 0, 0);
                                var barcolorline = Color.WhiteSmoke;

                                Drawing.DrawLine(
                                    (float)comboXPos, hpBarPos.Y, (float)comboXPos, hpBarPos.Y + 5, 1, barcolorline);
                                var diff = currentHpxPos - comboXPos;
                                for (var i = 0; i < diff; i++)
                                {
                                    Drawing.DrawLine(
                                        (float)comboXPos + i, hpBarPos.Y + 2, (float)comboXPos + i,
                                        hpBarPos.Y + 10, 1, barcolor);
                                }
                            }
                        }
                        else
                        {
                            if (enemyDictionary[enemy] >= 1000)
                                Drawing.DrawText(enemy.HPBarPosition.X + 7, enemy.HPBarPosition.Y + 1, Color.White, "Unkillable(" + ENdamage + "k+)");
                            else
                                Drawing.DrawText(enemy.HPBarPosition.X + 7, enemy.HPBarPosition.Y + 1, Color.White, "Unkillable(" + ENdamage + ")");
                            if (menu.Item("ExtraNeeded1").GetValue<bool>())
                            {
                                var hpBarPos = enemy.HPBarPosition;

                                hpBarPos.X += 45;
                                hpBarPos.Y += 18;

                                var combodamage = GetComboDamage(enemy, "Cunts", true, true, true, true, true);

                                var PercentHPleftAfterCombo = (enemy.Health - combodamage) / enemy.MaxHealth;
                                var PercentHPleft = enemy.Health / enemy.MaxHealth;
                                if (PercentHPleftAfterCombo < 0) PercentHPleftAfterCombo = 0;

                                double comboXPos = hpBarPos.X - 36 + (107 * PercentHPleftAfterCombo);
                                double currentHpxPos = hpBarPos.X - 36 + (107 * PercentHPleft);

                                var barcolor = Color.FromArgb(100, 0, 220, 0);
                                var barcolorline = Color.WhiteSmoke;

                                Drawing.DrawLine(
                                    (float)comboXPos, hpBarPos.Y, (float)comboXPos, hpBarPos.Y + 5, 1, barcolorline);
                                var diff = currentHpxPos - comboXPos;
                                for (var i = 0; i < diff; i++)
                                {
                                    Drawing.DrawLine(
                                        (float)comboXPos + i, hpBarPos.Y + 2, (float)comboXPos + i,
                                        hpBarPos.Y + 10, 1, barcolor);
                                }
                            }
                        }
                    }
                }
            }

            if (menu.Item("OptimalCombo").GetValue<bool>())
            {
                foreach (Obj_AI_Hero enemy in enemyDictionary1.Keys)
                {
                    if (enemy.IsVisible)
                    {
                        if (!enemy.IsDead)
                        {
                            Drawing.DrawText(enemy.HPBarPosition.X + 7, enemy.HPBarPosition.Y + 40, System.Drawing.Color.White, enemyDictionary1[enemy]);
                        }
                    }
                }
            }

            if (menu.Item("LastHitQQ").GetValue<KeyBind>().Active || menu.Item("LastHitQ").GetValue<KeyBind>().Active)
            {
                var MinMar = menu.Item("MinionMarker").GetValue<Circle>();
                if (_m != null)
                    Render.Circle.DrawCircle(_m.Position, 75, MinMar.Color);
            }

            #region Indicators
            if (menu.Item("HUDdisplay").GetValue<bool>())
            {
                float X = (float)menu.Item("HUDX").GetValue<Slider>().Value / 100;
                float Y = (float)menu.Item("HUDY").GetValue<Slider>().Value / 100;
                foreach (var hud in HUDlist.Where(hud => "U" + hud.MenuText == menu.Item("U" + hud.MenuText).Name && menu.Item("U" + hud.MenuText).GetValue<bool>()))
                {
                    if (menu.Item(hud.MenuComboText).GetValue<KeyBind>().Active)
                        Drawing.DrawText(Drawing.Width * X, Drawing.Height * Y, System.Drawing.Color.Yellow, hud.DisplayTextON);
                    else
                        Drawing.DrawText(Drawing.Width * X, Drawing.Height * Y, System.Drawing.Color.DarkRed, hud.DisplayTextOFF);
                    Y = Y + 0.02f;
                }
            }
            #endregion
            #region Mana Status
            if (menu.Item("manaStatus").GetValue<bool>())
            {
                if (ManaMode != 0)
                {
                    Vector2 wts = Drawing.WorldToScreen(Player.Position);
                    if (ManaMode == 1)
                    {
                        Drawing.DrawText(wts[0] - 30, wts[1], Color.White, ("Q(" + NeededCD + ")s"));
                    }
                    else if (ManaMode == 2)
                    {
                        Drawing.DrawText(wts[0] - 30, wts[1], Color.White, ("E+W+Q(" + NeededCD + ")s"));
                    }
                    else if (ManaMode == 3)
                    {
                        Drawing.DrawText(wts[0] - 30, wts[1], Color.White, ("E+W+Q+R(" + NeededCD + ")s"));
                    }
                }
            }
            #endregion
            #region InfoTable
            if (menu.Item("InfoTable").GetValue<KeyBind>().Active)
            {
                var x = Drawing.Width * 0.85f;
                var y = Drawing.Height * 0.61f;
                Drawing.DrawText(x, y - 20f, System.Drawing.Color.White, "~INFO TABLE~");
                //Applies the function to all enemy heroes
                foreach (Obj_AI_Hero enemy in enemyDictionary1.Keys)
                {
                    if (!enemy.IsDead)
                    {
                        float ENNdamage = 0f;
                        if (GetExtraNeeded(enemy).Item1 >= 1000)
                        {
                            int Integer = GetExtraNeeded(enemy).Item1;
                            float Floater = Integer;
                            ENNdamage = (float)Math.Round((Floater / 1000), 1);
                        }
                        else
                        {
                            ENNdamage = GetExtraNeeded(enemy).Item1;
                        }
                        if (enemyDictionary1[enemy] == "Unkillable")
                        {
                            if (GetExtraNeeded(enemy).Item1 >= 1000)
                            {
                                Drawing.DrawText(x, y, System.Drawing.Color.White, enemy.ChampionName + ":" + enemyDictionary1[enemy] + "(" + ENNdamage + "k+)");
                            }
                            else
                            {
                                Drawing.DrawText(x, y, System.Drawing.Color.White, enemy.ChampionName + ":" + enemyDictionary1[enemy] + "(" + GetExtraNeeded(enemy).Item1 + ")");
                            }
                        }
                        else
                        {
                            Drawing.DrawText(x, y, System.Drawing.Color.White, enemy.ChampionName + ":" + enemyDictionary1[enemy] + "(" + GetExtraNeeded(enemy).Item1 + ")");
                        }
                    }
                    else
                    {
                        Drawing.DrawText(x, y, System.Drawing.Color.White, enemy.ChampionName + ":" + "Dead"); //Enemy is dead
                    }
                    y = y + 20f;
                }
            }
            //    foreach (Obj_AI_Hero enemy in enemyDictionary1.Keys)
            //    {
            //        if (!enemy.IsDead)
            //        {
            //            if (enemyDictionary1[enemy] == "Unkillable")
            //            {
            //                Drawing.DrawText(x, y, System.Drawing.Color.White, enemy.ChampionName + ":" + enemyDictionary1[enemy]);
            //            }
            //            else
            //            {
            //                Drawing.DrawText(x, y, System.Drawing.Color.White, enemy.ChampionName + ":" + enemyDictionary1[enemy]);
            //            }
            //        }
            //        else
            //        {
            //            Drawing.DrawText(x, y, System.Drawing.Color.White, enemy.ChampionName + ":" + "Dead"); //Enemy is dead
            //        }
            //        y = y + 20f;
            //    }
            //}
            #endregion
            foreach (Spell spell in SpellList)
            {
                var menuItem = menu.Item(spell.Slot + "Range").GetValue<Circle>();
                if (menuItem.Active)
                    Render.Circle.DrawCircle(Player.Position, spell.Range, menuItem.Color);
            }
        }

        //Automatic Kill Steal
        private static void AutoKS()
        {
            if (menu.Item("AllInActive").GetValue<KeyBind>().Active || menu.Item("Stun Closest Enemy").GetValue<KeyBind>().Active || menu.Item("HarassActive").GetValue<KeyBind>().Active || menu.Item("Combo").GetValue<KeyBind>().Active && menu.Item("DisableKS").GetValue<bool>()) return;
            foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.IsEnemy && !enemy.IsDead && enemy.IsValidTarget() && Player.Distance(enemy.Position) <= NeededRange(menu.Item("UseQKS").GetValue<bool>(), menu.Item("UseWKS").GetValue<bool>(), menu.Item("UseWKS").GetValue<bool>(), menu.Item("UseRKS").GetValue<bool>(), menu.Item("UseIGNKS").GetValue<bool>()) && Player.Distance(enemy.Position) <= E.Range))
            {
                if (GetComboDamage(enemy, "Cunts", true, false, false, false, false) > enemy.Health && menu.Item("UseQKS").GetValue<bool>())
                {
                    if (HasMana(true, false, false, false))
                    {
                        UseSpells(enemy, "Source", true, false, false, false, false);
                    }
                }
                else if (GetComboDamage(enemy, "Cunts", false, true, false, false, false) > enemy.Health && menu.Item("UseWKS").GetValue<bool>())
                {
                    if (HasMana(false, true, false, false))
                    {
                        UseSpells(enemy, "NE", false, true, false, false, false);
                    }
                }
                else if (GetComboDamage(enemy, "Cunts", true, false, false, false, false) > enemy.Health && menu.Item("UseQKS").GetValue<bool>() && menu.Item("UseDFGKS").GetValue<bool>())
                {
                    if (HasMana(true, false, false, false))
                    {
                        UseSpells(enemy, "Source", true, false, false, false, false);
                    }
                }
                else if (GetComboDamage(enemy, "Cunts", false, false, false, true, false) > enemy.Health && menu.Item("UseRKS").GetValue<bool>())
                {
                    if (HasMana(false, false, false, true))
                    {
                        UseSpells(enemy, "Source", false, false, false, true, false);
                    }
                }
                else if (GetComboDamage(enemy, "Cunts", false, false, false, true, false) > enemy.Health && menu.Item("UseRKS").GetValue<bool>() && menu.Item("UseDFGKS").GetValue<bool>())
                {
                    if (HasMana(false, false, false, true))
                    {
                        UseSpells(enemy, "Source", true, false, false, false, false);
                    }
                }
                else if (GetComboDamage(enemy, "Cunts", false, false, false, false, true) > enemy.Health && menu.Item("UseIGNKS").GetValue<bool>())
                {
                    UseSpells(enemy, "Source", false, false, false, false, true);
                }
                else if (GetComboDamage(enemy, "Cunts", menu.Item("UseQKS").GetValue<bool>(), menu.Item("UseWKS").GetValue<bool>(), false, menu.Item("UseRKS").GetValue<bool>(), menu.Item("UseIGNKS").GetValue<bool>()) > enemy.Health)
                {
                    UseSpells(enemy, "Source", menu.Item("UseQKS").GetValue<bool>(), menu.Item("UseWKS").GetValue<bool>(), false, menu.Item("UseRKS").GetValue<bool>(), menu.Item("UseIGNKS").GetValue<bool>());
                }
            }
        }

        private static int NeededRange(bool A, bool B, bool C, bool D, bool F)
        {
            bool AR = Exists(true, false, false, false, false);
            bool BR = Exists(false, true, false, false, false);
            bool CR = Exists(false, false, true, false, false);
            bool DR = Exists(false, false, false, true, false);
            bool FR = Exists(false, false, false, false, true);

            int NeededRangee = 0;
            if (F && FR)
                NeededRangee = 600;
            else if (D && DR)
                NeededRangee = 650;
            else if (A && AR)
                NeededRangee = 650;
            else if (B && BR)
                NeededRangee = 900;
            else if (C && CR)
                NeededRangee = 1050;
            return NeededRangee;
        }

        //Harass Combo(Independent of CD and target HP)
        private static void Harass()
        {
            if (menu.Item("HarassMode").GetValue<StringList>().SelectedIndex == 0) UseSpells(Target, "EWQHarass", true, true, true, false, false);
            else if (menu.Item("HarassMode").GetValue<StringList>().SelectedIndex == 1) UseSpells(Target, "EWHarass", false, true, true, false, false);
            else if (menu.Item("HarassMode").GetValue<StringList>().SelectedIndex == 2) UseSpells(Target, "QHarass", true, false, false, false, false);
        }

        //Use All Available Spells Combo(Independent of CD and target HP)
        private static void AllIn()
        {
            if (menu.Item("ToOrb").GetValue<bool>()) if (Orb == 2) Orbwalking.Orbwalk(Target, Game.CursorPos); else if (Orb == 1) Orbwalking.Orbwalk(Target, Game.CursorPos);
            UseSpells(Target, "AllIn", true, true, true, true, true);
        }

        //The Main Smart Combo that chooses the most efficient combo that will ensure the kill
        private static void Combo()
        {
            if (Target != null && Player.Distance(Target.Position) <= E.Range && menu.Item("il" + Target.BaseSkinName).GetValue<bool>() == false)
            {
                string TheCombo = null;

                TheCombo = GetBestCombo(Target, "Comboing");

                if (TheCombo != null && Target.MoveSpeed >= Player.MoveSpeed && menu.Item("notems").GetValue<bool>()) Drawing.DrawText(Player.HPBarPosition.X + 9, Player.HPBarPosition.Y + 37, System.Drawing.Color.LightGreen, Target.ChampionName + "" + "MS is higher" + "(" + Math.Round((Target.MoveSpeed - Player.MoveSpeed)) + ")");

                if (TheCombo == "E+Q" && HasMana(true, false, true, false)) //E+Q
                    UseSpells(Target, "N", true, false, true, false, false);
                else if (TheCombo == "Q" && HasMana(true, false, false, false)) //Q
                    UseSpells(Target, "N", true, false, false, false, false);
                else if (TheCombo == "E+W" && HasMana(false, true, true, false)) //E+W
                    UseSpells(Target, "NE", false, true, true, false, false);
                else if (TheCombo == "W" && HasMana(false, true, false, false)) //W
                    UseSpells(Target, "NE", false, true, false, false, false);
                else if (TheCombo == "E+W+Q" && HasMana(true, true, true, false)) //E+W+Q
                    UseSpells(Target, "NE", true, true, true, false, false);
                else if (TheCombo == "|DFG|E+W+Q" && HasMana(true, true, true, false)) //DFG+E+W+Q
                    UseSpells(Target, "NE", true, true, true, false, false);
                else if (TheCombo == "|DFG|E+W" && HasMana(false, true, true, false)) //DFG+E+W
                    UseSpells(Target, "NE", false, true, true, false, false);
                else if (TheCombo == "|DFG|Q" && HasMana(true, false, false, false)) //DFG+Q
                    UseSpells(Target, "N", true, false, false, false, false);
                else if (TheCombo == "E+W+R" && HasMana(false, true, true, true)) //E+W+R
                    UseSpells(Target, "NE", false, true, true, true, false);
                else if (TheCombo == "E+W+R+Q" && HasMana(true, true, true, true)) //E+W+R+Q
                    UseSpells(Target, "NE", true, true, true, true, false);
                else if (TheCombo == "E+R" && HasMana(false, false, true, true)) //E+R
                    UseSpells(Target, "N", false, false, true, true, false);
                else if (TheCombo == "R" && HasMana(false, false, false, true)) //R
                    UseSpells(Target, "N", false, false, false, true, false);
                else if (TheCombo == "|DFG|E+R+Q" && HasMana(true, false, true, true)) //DFG+E+R+Q
                    UseSpells(Target, "N", true, false, true, true, false);
                else if (TheCombo == "|DFG|E+W+R" && HasMana(false, true, true, true)) //DFG+E+W+R
                    UseSpells(Target, "NE", false, true, true, true, false);
                else if (TheCombo == "|DFG|E+W+R+Q" && HasMana(true, true, true, true)) //DFG+E+W+R+Q
                    UseSpells(Target, "NE", true, true, true, true, false);
                else if (TheCombo == "|DFG|R" && HasMana(false, false, false, true)) //DFG+R
                    UseSpells(Target, "N", false, false, false, true, false);
                else if (TheCombo == "E+Q+IGN" && HasMana(true, false, true, false)) //E+Q+IGN
                    UseSpells(Target, "N", true, false, true, false, true);
                else if (TheCombo == "E+W+IGN" && HasMana(false, true, true, false)) //E+W+IGN
                    UseSpells(Target, "NE", false, true, true, false, true);
                else if (TheCombo == "E+W+Q+IGN" && HasMana(true, true, true, false)) //E+W+Q+IGN
                    UseSpells(Target, "NE", true, true, true, false, true);
                else if (TheCombo == "E+W+R+Q+IGN" && HasMana(true, true, true, true)) //E+W+R+Q+IGN
                    UseSpells(Target, "NE", true, true, true, true, true);
                else if (TheCombo == "|DFG|E+Q+IGN" && HasMana(true, false, true, false)) //DFG+E+Q+IGN
                    UseSpells(Target, "N", true, false, true, false, true);
                else if (TheCombo == "|DFG|E+W+IGN" && HasMana(false, true, true, false)) //DFG+E+W+IGN
                    UseSpells(Target, "NE", false, true, true, false, true);
                else if (TheCombo == "|DFG|E+W+Q+IGN" && HasMana(true, true, true, false)) //DFG+E+W+Q+IGN
                    UseSpells(Target, "NE", true, true, true, false, true);
                else if (TheCombo == "|DFG|E+W+R+Q+IGN" && HasMana(true, true, true, true)) //DFG+E+W+R+Q+IGN
                    UseSpells(Target, "NE", true, true, true, true, true);
                else if (TheCombo == "IGN" && HasMana(false, false, false, false)) //IGN
                    UseSpells(Target, "N", false, false, false, false, true);
                else if (TheCombo == "Unkillable" && HasMana(false, false, false, false)) //Unkillable
                    if (menu.Item("ComboMode").GetValue<StringList>().SelectedIndex == 0)
                        return;
                    else if (menu.Item("ComboMode").GetValue<StringList>().SelectedIndex == 1)
                        Harass();
                    else if (menu.Item("ComboMode").GetValue<StringList>().SelectedIndex == 2)
                        UseSpells(Target, "N", true, false, false, false, false);
                    else if (menu.Item("ComboMode").GetValue<StringList>().SelectedIndex == 3)
                        UseSpells(Target, "NE", false, true, true, false, false);
                    else if (menu.Item("ComboMode").GetValue<StringList>().SelectedIndex == 4)
                        UseSpells(Target, "NE", true, true, true, false, false);
            }
        }

        //Uses selected abilities
        private static void UseSpells(Obj_AI_Hero T, string Source, bool QQ, bool WW, bool EE, bool RR, bool IGNN)
        {

            if (Player.Distance(T, true) < Math.Pow(NeededRange(QQ, WW, EE, RR, IGNN), 2) && Player.Distance(T, true) > Math.Pow(NeededRange(QQ, WW, EE, RR, IGNN), 2)) ObjectManager.Player.IssueOrder(GameObjectOrder.MoveTo, T);

            if (menu.Item("CastMode").GetValue<StringList>().SelectedIndex == 0)
            {
                if (EE && T != null)
                {
                    if (Player.Distance(T.Position) <= E.Range)
                    {
                        if (E.IsReady() && !IsImmune(T) || !menu.Item("DontEShields").GetValue<bool>())
                        {
                            castE((Obj_AI_Hero)T);
                        }
                    }
                }

                if (WW && T != null)
                {
                    if (W.IsReady())
                    {
                        var pred = W.GetPrediction(T);
                        if (Source == "NE" && menu.Item("ComboWaitMode").GetValue<StringList>().SelectedIndex == 0)
                        {
                            if (pred.Hitchance == HitChance.Immobile || T.Buffs.Where(b => b.IsActive && Game.Time < b.EndTime && (b.Type == BuffType.Charm || b.Type == BuffType.Knockback || b.Type == BuffType.Stun || b.Type == BuffType.Suppression || b.Type == BuffType.Snare)).Aggregate(0f, (current, buff) => Math.Max(current, buff.EndTime)) - Game.Time >= W.Delay && W.IsReady())
                                W.Cast(T.ServerPosition, Packets());
                        }
                        else
                        {
                            if (pred.Hitchance == HitChance.Immobile || T.Buffs.Where(b => b.IsActive && Game.Time < b.EndTime && (b.Type == BuffType.Charm || b.Type == BuffType.Knockback || b.Type == BuffType.Stun || b.Type == BuffType.Suppression || b.Type == BuffType.Snare)).Aggregate(0f, (current, buff) => Math.Max(current, buff.EndTime)) - Game.Time >= W.Delay && W.IsReady())
                                W.Cast(T.ServerPosition, Packets());
                        }
                    }
                }
            }
            else if (menu.Item("CastMode").GetValue<StringList>().SelectedIndex == 1)
            {
                if (WW && T != null)
                {
                    if (W.IsReady())
                    {
                        var pred = W.GetPrediction(T);
                        if (EE)
                        {
                            if (E.IsReady() && pred.Hitchance == HitChance.VeryHigh) W.Cast(T.ServerPosition, Packets());
                        }
                        else
                        {
                            if (pred.Hitchance == HitChance.VeryHigh) W.Cast(T.ServerPosition, Packets());
                        }
                    }
                }

                if (EE && T != null)
                {
                    if (Player.Distance(T.Position) <= E.Range)
                    {
                        if (E.IsReady() && !IsImmune(T) || !menu.Item("DontEShields").GetValue<bool>())
                        {
                            if (WW)
                            {
                                if (!W.IsReady()) castE((Obj_AI_Hero)T);
                            }
                            else
                            {
                                castE((Obj_AI_Hero)T);
                            }
                        }
                    }
                }
            }

            if (RR && T != null)
            {
                if (R.IsReady() && !HasBuffs(T) && DetectCollision(T, R.Delay))
                {
                    if (Source == "NE" && menu.Item("ComboWaitMode").GetValue<StringList>().SelectedIndex == 0)
                    {
                        if (menu.Item("CastMode").GetValue<StringList>().SelectedIndex == 0)
                        {
                            if (menu.Item("IgnoreR").GetValue<bool>() || !W.IsReady())
                                R.CastOnUnit(T, Packets());
                        }
                        else
                        {
                            if (T.Buffs.Where(b => b.IsActive && Game.Time < b.EndTime && (b.Type == BuffType.Charm || b.Type == BuffType.Knockback || b.Type == BuffType.Stun || b.Type == BuffType.Suppression || b.Type == BuffType.Snare)).Aggregate(0f, (current, buff) => Math.Max(current, buff.EndTime)) - Game.Time >= W.Delay)
                                R.CastOnUnit(T, Packets());
                        }
                    }
                    else
                    {
                        R.CastOnUnit(T, Packets());
                    }

                }
            }

            if (QQ && T != null && !HasBuffs(T) && DetectCollision(T, Q.Delay))
            {
                if (Source == "NE" && menu.Item("ComboWaitMode").GetValue<StringList>().SelectedIndex == 0)
                {
                    if (menu.Item("CastMode").GetValue<StringList>().SelectedIndex == 0)
                    {
                        if (menu.Item("IgnoreQ").GetValue<bool>() || !W.IsReady())
                        {
                            CastQ(T);
                        }
                    }
                    else
                    {
                        if (T.Buffs.Where(b => b.IsActive && Game.Time < b.EndTime && (b.Type == BuffType.Charm || b.Type == BuffType.Knockback || b.Type == BuffType.Stun || b.Type == BuffType.Suppression || b.Type == BuffType.Snare)).Aggregate(0f, (current, buff) => Math.Max(current, buff.EndTime)) - Game.Time >= W.Delay)
                            CastQ(T);
                    }
                }
                else if (Source != "EWQHarass" && Source != "QHarass")
                {
                    CastQ(T);
                }
                else if (Source == "EWQHarass")
                {
                    if (!menu.Item("WaitW").GetValue<bool>() || !W.IsReady())
                        CastQ(T);
                }
                else if (Source == "QHarass")
                {
                    if (Player.Distance(T.Position) <= Q.Range)
                        CastQ(T);
                }

            }

            if (IGNN && T != null && IgniteSlot != SpellSlot.Unknown)
            {
                if (Player.Spellbook.CanUseSpell(IgniteSlot) == SpellState.Ready && !HasBuffs(T))
                {
                    if (Source == "NE" && menu.Item("ComboWaitMode").GetValue<StringList>().SelectedIndex == 0)
                    {
                        if (menu.Item("CastMode").GetValue<StringList>().SelectedIndex == 0)
                        {
                            if (menu.Item("IgnoreIGN").GetValue<bool>() || !W.IsReady())
                                Player.Spellbook.CastSpell(IgniteSlot, T);
                        }
                        else
                        {
                            if (T.Buffs.Where(b => b.IsActive && Game.Time < b.EndTime && (b.Type == BuffType.Charm || b.Type == BuffType.Knockback || b.Type == BuffType.Stun || b.Type == BuffType.Suppression || b.Type == BuffType.Snare)).Aggregate(0f, (current, buff) => Math.Max(current, buff.EndTime)) - Game.Time >= W.Delay)
                                Player.Spellbook.CastSpell(IgniteSlot, T);
                        }
                    }
                    else
                    {
                        Player.Spellbook.CastSpell(IgniteSlot, T);
                    }
                }
            }
        }

        //The Main function that decides which combo to use and what is the needed cooldown
        private static string GetBestCombo(Obj_AI_Hero x, string Source)
        {
            string BestCombo = null;

            var op = menu.Item("op" + x.BaseSkinName).GetValue<Slider>().Value * .01 + 1;

            if (GetComboDamage(x, Source, true, false, true, false, false) > x.Health * op) //E+Q
            {
                if (HasMana(true, false, true, false))
                {
                    BestCombo = "E+Q";
                }
            }
            else if (GetComboDamage(x, Source, true, false, false, false, false) > x.Health * op) //Q
            {
                if (HasMana(true, false, false, false))
                {
                    BestCombo = "Q";
                }
            }
            else if (GetComboDamage(x, Source, false, true, true, false, false) > x.Health * op) //E+W
            {
                if (HasMana(false, true, true, false))
                {
                    BestCombo = "E+W";
                }
            }
            else if (GetComboDamage(x, Source, false, true, false, false, false) > x.Health * op) //W
            {
                if (HasMana(false, true, false, false))
                {
                    BestCombo = "W";
                }
            }
            else if (GetComboDamage(x, Source, true, true, true, false, false) > x.Health * op) //E+W+Q
            {
                if (HasMana(true, true, true, false))
                {
                    BestCombo = "E+W+Q";
                }
            }
            if (menu.Item("Priority").GetValue<StringList>().SelectedIndex == 0)
            {
                if (GetComboDamage(x, Source, false, true, true, true, false) > x.Health * op) //E+W+R
                {
                    if (HasMana(false, true, true, true))
                    {
                        BestCombo = "E+W+R";
                    }
                }
                else if (GetComboDamage(x, Source, true, true, true, true, false) > x.Health * op) //E+W+R+Q
                {
                    if (HasMana(true, true, true, true))
                    {
                        BestCombo = "E+W+R+Q";
                    }
                }
                else if (GetComboDamage(x, Source, false, false, true, true, false) > x.Health * op) //E+R
                {
                    if (HasMana(false, false, true, true))
                    {
                        BestCombo = "E+R";
                    }
                }
                else if (GetComboDamage(x, Source, false, false, false, true, false) > x.Health * op) //R
                {
                    if (HasMana(false, false, false, true))
                    {
                        BestCombo = "R";
                    }
                }
                else if (GetComboDamage(x, Source, true, false, true, false, true) > x.Health * op) //E+Q+IGN
                {
                    if (HasMana(true, false, true, false))
                    {
                        BestCombo = "E+Q+IGN";
                    }
                }
                else if (GetComboDamage(x, Source, false, true, true, false, true) > x.Health * op) //E+W+IGN
                {
                    if (HasMana(false, true, true, false))
                    {
                        BestCombo = "E+W+IGN";
                    }
                }
                else if (GetComboDamage(x, Source, true, true, true, false, true) > x.Health * op) //E+W+Q+IGN
                {
                    if (HasMana(true, true, true, false))
                    {
                        BestCombo = "E+W+Q+IGN";
                    }
                }
                else if (GetComboDamage(x, Source, true, true, true, true, true) > x.Health * op) //E+W+R+Q+IGN
                {
                    if (HasMana(true, true, true, true))
                    {
                        BestCombo = "E+W+R+Q+IGN";
                    }
                }
                else if (GetComboDamage(x, Source, false, false, false, false, true) > x.Health * op)  //IGN
                {
                    if (HasMana(false, false, false, false))
                    {
                        BestCombo = "IGN";
                    }
                }
            }
            else if (menu.Item("Priority").GetValue<StringList>().SelectedIndex == 1)
            {
                if (GetComboDamage(x, Source, true, false, true, false, true) > x.Health * op) //E+Q+IGN
                {
                    if (HasMana(true, false, true, false))
                    {
                        BestCombo = "E+Q+IGN";
                    }
                }
                else if (GetComboDamage(x, Source, false, true, true, false, true) > x.Health * op) //E+W+IGN
                {
                    if (HasMana(false, true, true, false))
                    {
                        BestCombo = "E+W+IGN";
                    }
                }
                else if (GetComboDamage(x, Source, true, true, true, false, true) > x.Health * op) //E+W+Q+IGN
                {
                    if (HasMana(true, true, true, false))
                    {
                        BestCombo = "E+W+Q+IGN";
                    }
                }
                else if (GetComboDamage(x, Source, true, true, true, true, true) > x.Health * op) //E+W+R+Q+IGN
                {
                    if (HasMana(true, true, true, true))
                    {
                        BestCombo = "E+W+R+Q+IGN";
                    }
                }
                else if (GetComboDamage(x, Source, false, false, false, false, true) > x.Health * op)  //IGN
                {
                    if (HasMana(false, false, false, false))
                    {
                        BestCombo = "IGN";
                    }
                }
                else if (GetComboDamage(x, Source, false, true, true, true, false) > x.Health * op) //E+W+R
                {
                    if (HasMana(false, true, true, true))
                    {
                        BestCombo = "E+W+R";
                    }
                }
                else if (GetComboDamage(x, Source, true, true, true, true, false) > x.Health * op) //E+W+R+Q
                {
                    if (HasMana(true, true, true, true))
                    {
                        BestCombo = "E+W+R+Q";
                    }
                }
                else if (GetComboDamage(x, Source, false, false, true, true, false) > x.Health * op) //E+R
                {
                    if (HasMana(false, false, true, true))
                    {
                        BestCombo = "E+R";
                    }
                }
                else if (GetComboDamage(x, Source, false, false, false, true, false) > x.Health * op) //R
                {
                    if (HasMana(false, false, false, true))
                    {
                        BestCombo = "R";
                    }
                }
            }

            if (BestCombo == null) //Not Killable
            {
                BestCombo = "Unkillable";
            }

            if (x.IsDead)
            {
                Ccombo = null;
                Cccombo = null;
            }

            if (Source == "Comboing")
            {
                if (Ccombo == null && BestCombo != "Unkillable")
                {
                    Delay = CastTime(BestCombo);
                    Delay1 = Environment.TickCount;
                    Ccombo = BestCombo;
                }
            }

            if (Source == "KS")
            {
                if (Cccombo == null && BestCombo != "Unkillable")
                {
                    Delayy = CastTime(BestCombo);
                    Delayy1 = Environment.TickCount;
                    Cccombo = BestCombo;
                }
            }

            if (Source != "KS" && Source != "Comboing")
            {
                if (!x.IsVisible)
                    BestCombo = "N/A";
            }

            if (Source == "Comboing" || Source == "KS")
            {
                if (Source == "Comboing")
                {
                    if (Ccombo != null)
                        return Ccombo;
                    else
                        return BestCombo;
                }
                else
                {
                    if (Cccombo != null)
                        return Cccombo;
                    else
                        return BestCombo;
                }
            }
            else
            {
                return BestCombo;
            }

        }

        //Jungle Farm
        public static void JungleFarm()
        {
            if (menu.Item("UseAAJungle").GetValue<bool>())
            {
                var AAminion = MinionManager.GetMinions(525, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth).FirstOrDefault();
                if (AAminion != null)
                {
                    Player.IssueOrder(GameObjectOrder.AttackUnit, AAminion);
                }
            }

            if (menu.Item("UseEJungle").GetValue<bool>() && E.IsReady())
            {
                var minion = MinionManager.GetMinions(E.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth).FirstOrDefault();
                if (minion != null)
                    castE(minion);
            }

            if (menu.Item("UseQJungle").GetValue<bool>() && Q.IsReady())
            {
                var targetClear = MinionManager.GetMinions(Q.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth).FirstOrDefault();
                var targetFarm = MinionManager.GetMinions(Q.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth).FirstOrDefault(m => m.Health < (Player.GetSpellDamage(m, SpellSlot.Q)));

                if (targetFarm != null)
                {
                    Q.Cast(targetFarm, Packets());
                }
                else if (targetClear != null)
                {
                    Q.Cast(targetClear, Packets());
                }
            }

            if (menu.Item("UseWJungle").GetValue<bool>() && W.IsReady())
            {
                var JungleWMinions = MinionManager.GetMinions(Player.Position, W.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
                List<Vector2> minionerinos2 =
         (from minions in JungleWMinions select minions.Position.To2D()).ToList();
                var ePos2 = MinionManager.GetBestCircularFarmLocation(minionerinos2, W.Width, W.Range).Position;

                if (ePos2.Distance(Player.Position.To2D()) < W.Range && MinionManager.GetBestCircularFarmLocation(minionerinos2, W.Width, W.Range).MinionsHit > 0)
                {
                    W.Cast(ePos2, Packets());
                }
            }
        }

        //Q last Hitting
        public static void lastHit()
        {
            if (!Orbwalking.CanMove(40)) return;
            if (Q.IsReady())
            {
                if (_m != null)
                    CastQ(_m);
            }
        }

        //W Lane Crearing
        public static void lastHitW()
        {
            var laneMinions = MinionManager.GetMinions(Player.Position, W.Range, MinionTypes.All, MinionTeam.Enemy, MinionOrderTypes.MaxHealth);
            List<Vector2> minionerinos2 =
     (from minions in laneMinions select minions.Position.To2D()).ToList();
            var ePos2 = MinionManager.GetBestCircularFarmLocation(minionerinos2, W.Width, W.Range).Position;

            if (ePos2.Distance(Player.Position.To2D()) < W.Range && MinionManager.GetBestCircularFarmLocation(minionerinos2, W.Width, W.Range).MinionsHit >= menu.Item("WAmount").GetValue<Slider>().Value)
            {
                W.Cast(ePos2, Packets());
            }
        }

        //        //Calculates the damage from selected abilities
        private static float GetComboDamage(Obj_AI_Base enemy, string source, bool A, bool B, bool C, bool D, bool F)
        {
            double damage = 0d;
            double cmana = 0d;

            if (W.IsReady() && B)
            {
                cmana += Player.Spellbook.GetSpell(SpellSlot.W).ManaCost;
                if (source == "Comboing" || source == "KS" || source == "Table")
                {
                    if (!C)
                        if (cmana <= Player.Mana) damage += Player.GetSpellDamage(enemy, SpellSlot.W);
                        else cmana -= Player.Spellbook.GetSpell(SpellSlot.W).ManaCost;
                    else if (E.IsReady())
                        if (cmana <= Player.Mana) damage += Player.GetSpellDamage(enemy, SpellSlot.W);
                        else cmana -= Player.Spellbook.GetSpell(SpellSlot.W).ManaCost;
                }
                else
                {
                    if (cmana <= Player.Mana) damage += Player.GetSpellDamage(enemy, SpellSlot.W);
                    else cmana -= Player.Spellbook.GetSpell(SpellSlot.W).ManaCost;
                }
            }

            if (R.IsReady() && D)
            {
                cmana += Player.Spellbook.GetSpell(SpellSlot.R).ManaCost;
                if (source == "Comboing" || source == "KS" || source == "Table")
                {
                    if (!C)
                        if (cmana <= Player.Mana) damage += Player.GetSpellDamage(enemy, SpellSlot.R);
                        else cmana -= Player.Spellbook.GetSpell(SpellSlot.R).ManaCost;
                    else if (E.IsReady())
                        if (cmana <= Player.Mana) damage += Player.GetSpellDamage(enemy, SpellSlot.R);
                        else cmana -= Player.Spellbook.GetSpell(SpellSlot.R).ManaCost;
                }
                else
                {
                    if (cmana <= Player.Mana) damage += Player.GetSpellDamage(enemy, SpellSlot.R);
                    else cmana -= Player.Spellbook.GetSpell(SpellSlot.R).ManaCost;
                }
            }

            if (Q.IsReady() && A)
            {
                cmana += Player.Spellbook.GetSpell(SpellSlot.Q).ManaCost;
                if (source == "Comboing" || source == "KS" || source == "Table")
                {
                    if (!C)
                        if (cmana <= Player.Mana) damage += Player.GetSpellDamage(enemy, SpellSlot.Q);
                        else cmana -= Player.Spellbook.GetSpell(SpellSlot.Q).ManaCost;
                    else if (E.IsReady())
                        if (cmana <= Player.Mana) damage += Player.GetSpellDamage(enemy, SpellSlot.Q);
                        else cmana -= Player.Spellbook.GetSpell(SpellSlot.Q).ManaCost;
                }
                else
                {
                    if (cmana <= Player.Mana) damage += Player.GetSpellDamage(enemy, SpellSlot.Q);
                    else cmana -= Player.Spellbook.GetSpell(SpellSlot.Q).ManaCost;
                }
            }

            if (IgniteSlot != SpellSlot.Unknown && Player.Spellbook.CanUseSpell(IgniteSlot) == SpellState.Ready && F)
            {
                if (source == "Comboing" || source == "KS" || source == "Table")
                {
                    if (!C)
                        damage += ObjectManager.Player.GetSummonerSpellDamage(enemy, Damage.SummonerSpell.Ignite);
                    else if (E.IsReady())
                        damage += ObjectManager.Player.GetSummonerSpellDamage(enemy, Damage.SummonerSpell.Ignite);
                }
                else
                {
                    damage += ObjectManager.Player.GetSummonerSpellDamage(enemy, Damage.SummonerSpell.Ignite);
                }
            }
            //}

            if (Items.HasItem(3155, (Obj_AI_Hero)enemy))
            {
                damage = damage - 250;
            }

            if (Items.HasItem(3156, (Obj_AI_Hero)enemy))
            {
                damage = damage - 400;
            }
            return (float)damage - 20;
        }

        //Checks if you have enough mana for casting selected abilities
        public static bool HasMana(bool A, bool B, bool C, bool D)
        {
            int QMana = 0;
            if (A) QMana = qMana[Q.Level];
            int WMana = 0;
            if (B) WMana = wMana[W.Level];
            int EMana = 0;
            if (C) EMana = eMana[E.Level];
            int RMana = 0;
            if (D) RMana = rMana[R.Level];
            int together = QMana + WMana + EMana + RMana;
            if (Player.Mana >= together)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        //Returns the time in seconds needed to regen mana for Q,E+W+Q,E+W+Q+R combos.
        public static Tuple<int, int> manaCheck()
        {
            int QMana = qMana[Q.Level];
            int EWQMana = qMana[Q.Level] + wMana[W.Level] + eMana[E.Level];
            int EWQRMana = qMana[Q.Level] + wMana[W.Level] + eMana[E.Level] + rMana[R.Level];
            int end = 0;
            int EndValue = 0;

            if (Player.Mana < QMana)
            {
                if (Q.Level != 0)
                {
                    double ManaRegen = ObjectManager.Player.PARRegenRate;
                    double NeededMana = qMana[Q.Level] - Player.Mana;
                    EndValue = (int)Math.Round(NeededMana / ManaRegen);
                    end = 1;
                }
            }

            else if (Player.Mana < EWQMana)
            {
                if (Q.Level != 0 && W.Level != 0 && E.Level != 0)
                {
                    double ManaRegen = ObjectManager.Player.PARRegenRate;
                    double NeededMana = qMana[Q.Level] + wMana[W.Level] + eMana[E.Level] - Player.Mana;
                    EndValue = (int)Math.Round(NeededMana / ManaRegen);
                    end = 2;
                }
            }

            else if (Player.Mana < EWQRMana)
            {
                if (Q.Level != 0 && W.Level != 0 && E.Level != 0 && R.Level != 0)
                {
                    double ManaRegen = ObjectManager.Player.PARRegenRate;
                    double NeededMana = qMana[Q.Level] + wMana[W.Level] + eMana[E.Level] + rMana[R.Level] - Player.Mana;
                    EndValue = (int)Math.Round(NeededMana / ManaRegen);
                    end = 3;
                }
            }

            return Tuple.Create(end, EndValue);
        }

        ////Check if needed abilities are ready
        //public static bool UpCD(bool A, bool B, bool C, bool D, bool EEE, bool F)
        //{
        //    bool AR = Q.IsReady();
        //    bool BR = W.IsReady();
        //    bool CR = E.IsReady();
        //    bool DR = R.IsReady();
        //    bool ER = Dfg.IsReady();
        //    bool FR = IgniteSlot != SpellSlot.Unknown && Player.Spellbook.CanUseSpell(IgniteSlot) == SpellState.Ready;

        //    if (A && !AR && Q.Level > 0)
        //    {
        //        return false;
        //    }
        //    else if (B && !BR && W.Level > 0)
        //    {
        //        return false;
        //    }
        //    else if (C && !CR && E.Level > 0)
        //    {
        //        return false;
        //    }
        //    else if (D && !DR && R.Level > 0)
        //    {
        //        return false;
        //    }
        //    else if (EEE && !ER && IgniteSlot == SpellSlot.Unknown)
        //    {
        //        return false;
        //    }
        //    else if (F && !FR && DFGSlot == SpellSlot.Unknown)
        //    {
        //        return false;
        //    }
        //    return true;
        //}

        ////Calculates the biggest needed cooldown for performing combo
        //public static double UpCDD(bool A, bool B, bool C, bool D, bool EEE, bool F)
        //{
        //    bool AR = Q.IsReady();
        //    bool BR = W.IsReady();
        //    bool CR = E.IsReady();
        //    bool DR = R.IsReady();
        //    bool ER = Dfg.IsReady();
        //    bool FR = IgniteSlot != SpellSlot.Unknown && Player.Spellbook.CanUseSpell(IgniteSlot) == SpellState.Ready;

        //    double timeleft = 0f;
        //    if (A && !AR)
        //        if ((Math.Round(Q.Instance.CooldownExpires - Game.Time)) > timeleft)
        //            timeleft = Math.Round(Q.Instance.CooldownExpires - Game.Time);
        //    if (B && !BR)
        //        if ((Math.Round(W.Instance.CooldownExpires - Game.Time)) > timeleft)
        //            timeleft = Math.Round(W.Instance.CooldownExpires - Game.Time);
        //    if (C && !CR)
        //        if ((Math.Round(E.Instance.CooldownExpires - Game.Time)) > timeleft)
        //            timeleft = Math.Round(E.Instance.CooldownExpires - Game.Time);
        //    if (D && !DR)
        //        if ((Math.Round(R.Instance.CooldownExpires - Game.Time)) > timeleft)
        //            timeleft = (Math.Round(R.Instance.CooldownExpires - Game.Time));
        //    if (EEE && !ER)
        //        if (Math.Round((ObjectManager.Player.Spellbook.GetSpell(DFGSlot).CooldownExpires - Game.Time)) > timeleft)
        //            timeleft = Math.Round((ObjectManager.Player.Spellbook.GetSpell(DFGSlot).CooldownExpires - Game.Time));
        //    if (F && !FR)
        //        if (Math.Round((ObjectManager.Player.Spellbook.GetSpell(IgniteSlot).CooldownExpires - Game.Time)) > timeleft)
        //            timeleft = Math.Round((ObjectManager.Player.Spellbook.GetSpell(IgniteSlot).CooldownExpires - Game.Time));
        //    return Math.Round(timeleft);
        //}

        //Checks if needed abilities are leveled up/exist
        public static bool Exists(bool A, bool B, bool C, bool D, bool F)
        {
            int x = 0;

            if (A && Q.Level == 0)
                x++;
            if (B && W.Level == 0)
                x++;
            if (C && E.Level == 0)
                x++;
            if (D && R.Level == 0)
                x++;
            if (F && IgniteSlot == SpellSlot.Unknown)
                x++;

            if (x > 0) return false;
            else return true;
        }

        //Returns how much time will you spend on casting spells
        public static float CastTime(Obj_AI_Base enemy, bool A, bool B, bool C, bool D, bool EEE, bool F)
        {
            float time = 0f;
            if (A) time += 0.6f * 1000;
            if (B) time += 1.2f * 1000;
            if (C) time += 0.2f * 1000;
            if (D) time += 0.6f * 1000;
            if (F) time += 1f * 1000;
            return time;
        }

        //Returns how much time will you spend on casting spells
        public static float CastTime(string combo)
        {
            float time = 0f;
            if (combo.Contains("Q")) time += 0.6f * 1000;
            if (combo.Contains("W")) time += 1.2f * 1000;
            if (combo.Contains("E")) time += 0.2f * 1000;
            if (combo.Contains("R")) time += 0.6f * 1000;
            if (combo.Contains("IGN")) time += 3f * 1000;
            return time;
        }

        //Tenacity and stun time check
        public static bool tenacitycheck(Obj_AI_Hero target)
        {
            if (E.Level == 0 || W.Level == 0) return true;
            if (Items.HasItem(3721, (Obj_AI_Hero)target) || Items.HasItem(3111, (Obj_AI_Hero)target) || Items.HasItem(3170, (Obj_AI_Hero)target) || Items.HasItem(3172, (Obj_AI_Hero)target))
            {
                var time = 1.5f + 0.25f * E.Level - 0.25f;
                time = time * 0.75f;
                if (time < W.Delay)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            else
            {
                return true;
            }
        }

        //Returns how much more damage you need to deal to the target to ensure the kill OR how much you will overdamage to them
        private static Tuple<int, string> GetExtraNeeded(Obj_AI_Hero target)
        {
            var combodamage = GetComboDamage(target, "Cunts", true, true, true, true, true);
            float Damage = 0f;
            int OutPut = 0;
            string Text = null;

            if (combodamage > target.Health)
            {
                Damage = (float)Math.Round(combodamage - target.Health);
                OutPut = (int)Damage;
                Text = "Extra:";
            }
            else
            {
                Damage = (float)Math.Round(target.Health - combodamage);
                OutPut = (int)Damage;
                Text = "Need:";
            }
            if (!target.IsVisible)
            {
                Text = "N/A";
            }
            if (target.IsDead)
            {
                Text = "Dead";
            }

            return Tuple.Create(OutPut, Text);
        }

        //Left Click Target Locker
        public static void Game_OnWndProc(WndEventArgs args)
        {
            if (args.Msg != (uint)WindowsMessages.WM_LBUTTONDOWN)
            {
                return;
            }

            if (menu.Item("LockTargets").GetValue<bool>())
            {
                foreach (var objAiHero in from hero in ObjectManager.Get<Obj_AI_Hero>()
                                          where hero.IsValidTarget()
                                          select hero
                                              into h
                                              orderby h.Distance(Game.CursorPos) descending
                                              select h
                                                  into enemy
                                                  where enemy.Distance(Game.CursorPos) < 150f
                                                  select enemy)
                {
                    if (objAiHero != null && objAiHero.IsVisible && !objAiHero.IsDead)
                    {
                        if (Environment.TickCount + Game.Ping - TargetLockCD > 200)
                        {
                            if (ChoosedTarget == null)
                            {
                                ChoosedTarget = objAiHero;
                            }
                            else
                            {
                                if (ChoosedTarget.Name == objAiHero.Name)
                                {
                                    ChoosedTarget = null;
                                }
                                else
                                {
                                    ChoosedTarget = objAiHero;
                                }
                            }
                            TargetLockCD = Environment.TickCount;
                        }
                    }
                }
            }
        }

        //Check if packet casting is turned ON/OFF
        public static bool Packets()
        {
            return menu.Item("packet").GetValue<bool>();
        }

        //Gets Current Target
        private static Obj_AI_Hero GetTarget()
        {
            Obj_AI_Hero Target = null;
            if (ChoosedTarget == null)
            {
                Target = TargetSelector.GetTarget(1050, TargetSelector.DamageType.Magical);
            }
            else
            {
                Target = ChoosedTarget;
            }
            return Target;
        }

        //Checks if the target will be affected by spell
        public static bool IsImmune(Obj_AI_Hero target)
        {
            foreach (var buff in IgnoreList)
            {
                if (target.HasBuff(buff.DisplayName) || target.HasBuff(buff.Name)) return true;
            }
            return false;
        }

        //Checks if the target will be killed by spell
        public static bool HasBuffs(Obj_AI_Hero target)
        {
            foreach (var buff in buffList)
            {
                if (target.HasBuff(buff.DisplayName) || target.HasBuff(buff.Name))
                {
                    if (menu.Item("dont" + buff.Name).GetValue<bool>())
                        return true;
                }
            }
            return false;
        }

        //E CAST(UNIT)
        public static void castE(Obj_AI_Base target)
        {
            PredictionOutput pred = Prediction.GetPrediction(target, E.Delay);
            Vector2 castVec = pred.UnitPosition.To2D() -
                              Vector2.Normalize(pred.UnitPosition.To2D() - Player.Position.To2D()) * E.Width;

            if (pred.Hitchance >= HitChance.High && E.IsReady())
            {
                E.Cast(castVec, false);
            }
        }

        //E CAST(UNIT)
        public static void castE(Obj_AI_Hero target)
        {
            PredictionOutput pred = Prediction.GetPrediction(target, E.Delay);
            Vector2 castVec = pred.UnitPosition.To2D() -
                              Vector2.Normalize(pred.UnitPosition.To2D() - Player.Position.To2D()) * E.Width;

            if (pred.Hitchance >= HitChance.High && E.IsReady())
            {
                E.Cast(castVec, false);
            }
        }

        //E CAST(COORDINATES)
        public static void castE(Vector3 pos)
        {
            Vector2 castVec = pos.To2D() -
                              Vector2.Normalize(pos.To2D() - Player.Position.To2D()) * E.Width;

            if (E.IsReady())
            {
                E.Cast(castVec, Packets());
            }
        }

        //E CAST(COORDINATES)
        public static void castE(Vector2 pos)
        {
            Vector2 castVec = pos;

            if (E.IsReady())
            {
                E.Cast(castVec, Packets());
            }
        }

        //Get Nearest Enemy Hero around "unit"
        public static Obj_AI_Hero GetNearestEnemy(Obj_AI_Hero unit)
        {
            return ObjectManager.Get<Obj_AI_Hero>()
                .Where(x => x.IsEnemy && x.IsValid && Player.Distance(x.Position) <= E.Range)
                .OrderBy(x => unit.ServerPosition.Distance(x.ServerPosition))
                .FirstOrDefault();
        }

        //Uses Items
        public static void useItem(int id, Obj_AI_Hero target = null)
        {
            if (Items.HasItem(id) && Items.CanUseItem(id))
            {
                Items.UseItem(id, target);
            }
        }

        //Loads standart or xSLx Orbwalker
        private static void chooseOrbwalker(bool mode)
        {
            if (mode)
            {
                Orbwalker = new Orbwalking.Orbwalker(orbwalkerMenu);
                Orb = 1;
                Game.PrintChat("Regular Orbwalker Loaded");
            }
        }

        //private static void GenModelPacket(string champ, int skinId)
        //{
        //     Packet.S2C.UpdateModel.Encoded(new Packet.S2C.UpdateModel.Struct(Player.NetworkId, skinId, champ))
        //        .Process();
        //}

        //Automatically uses Health Pots when ignite/morde buff is on you
        public static void AutoHP()
        {
            if (Player.HasBuff("summonerdot") || Player.HasBuff("MordekaiserChildrenOfTheGrave"))
            {
                if (!Utility.InFountain(Player))

                    if (Items.HasItem(biscuit.Id) && Items.CanUseItem(biscuit.Id) && !Player.HasBuff("ItemMiniRegenPotion"))
                    {
                        biscuit.Cast(Player);
                    }
                    else if (Items.HasItem(HPpot.Id) && Items.CanUseItem(HPpot.Id) && !Player.HasBuff("RegenerationPotion") && !Player.HasBuff("Health Potion"))
                    {
                        HPpot.Cast(Player);
                    }
                    else if (Items.HasItem(Flask.Id) && Items.CanUseItem(Flask.Id) && !Player.HasBuff("ItemCrystalFlask"))
                    {
                        Flask.Cast(Player);
                    }
            }
        }
        //hole
        //Buys listed items(Gets called once in a game in fountain when you have 475 gold)
        public static void BuyItems()
        {
            if (Utility.InFountain(Player) && ObjectManager.Player.Gold == 475 && !boughtItemOne)
            {
                ObjectManager.Player.BuyItem(LeagueSharp.ItemId.Warding_Totem_Trinket);
                ObjectManager.Player.BuyItem(LeagueSharp.ItemId.Faerie_Charm);
                ObjectManager.Player.BuyItem(LeagueSharp.ItemId.Health_Potion);
                ObjectManager.Player.BuyItem(LeagueSharp.ItemId.Health_Potion);
                ObjectManager.Player.BuyItem(LeagueSharp.ItemId.Health_Potion);
                ObjectManager.Player.BuyItem(LeagueSharp.ItemId.Health_Potion);
                ObjectManager.Player.BuyItem(LeagueSharp.ItemId.Health_Potion);
                ObjectManager.Player.BuyItem(LeagueSharp.ItemId.Mana_Potion);
                ObjectManager.Player.BuyItem(LeagueSharp.ItemId.Mana_Potion);
                ObjectManager.Player.BuyItem(LeagueSharp.ItemId.Mana_Potion);
                boughtItemOne = true;
            }
        }

        //Uses E on the end point of enemy Gapclosers
        public static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (!menu.Item("UseGap").GetValue<bool>()) return;
            if (E.IsReady() && gapcloser.Sender.IsValidTarget(E.Range))
                castE((Vector3)gapcloser.End);
        }

        //Interrupts Dangerous enemy spells with E
        private static void Interrupter_OnPosibleToInterrupt(Obj_AI_Hero unit, Interrupter2.InterruptableTargetEventArgs spell)
        {
            if (!menu.Item("UseInt").GetValue<bool>()) return;

            if (Player.Distance(unit.Position) < E.Range && unit != null && E.IsReady())
            {
                castE((Obj_AI_Hero)unit);
            }
        }

        private static void TowerAttackOnCreate(GameObject sender, EventArgs args)
        {
            if (!E.IsReady() || !menu.Item("StunUnderTower").GetValue<bool>())
            {
                return;
            }

            if (sender.IsValid<Obj_SpellMissile>())
            {
                var missile = (Obj_SpellMissile)sender;

                // Ally Turret -> Enemy Hero
                if (missile.SpellCaster.IsValid<Obj_AI_Turret>() && missile.SpellCaster.IsAlly &&
                    missile.Target.IsValid<Obj_AI_Hero>() && missile.Target.IsEnemy)
                {
                    var turret = (Obj_AI_Turret)missile.SpellCaster;
                    if (Player.Distance(missile.Target.Position) < 1050)
                        castE((Obj_AI_Hero)missile.Target);
                }
            }
        }

        //private static void ObjSpellMissileOnOnCreate(GameObject sender, EventArgs args)
        //{
        //    if (!sender.IsValid || !(sender is Obj_SpellMissile))
        //    {
        //        return; //not sure if needed
        //    }

        //    var missile = (Obj_SpellMissile)sender;
        //    var unit = missile.SpellCaster;
        //    if (!unit.IsValid || (unit.Team == ObjectManager.Player.Team))
        //    {
        //        return;
        //    }
        //    var missilePosition = missile.Position.To2D();
        //    var unitPosition = missile.StartPosition.To2D();
        //    var endPos = missile.EndPosition.To2D();
        //    if (Player.Distance(missile.Position) <= E.Range)
        //    {
        //        castE(endPos);
        //    }

        //}

        private static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!sender.IsValid || !menu.Item("YasuoWall").GetValue<bool>() || sender.Team == ObjectManager.Player.Team || args.SData.Name != "YasuoWMovingWall" || args.SData.Name != "YasuoWMovingWall")
                return;
            _wallCastT = Environment.TickCount;
            _yasuoWallCastedPos = sender.ServerPosition.To2D();
        }

        private static void OnCreate(GameObject obj, EventArgs args)
        {
            if (Player.Distance(obj.Position) > 1500 || !ObjectManager.Get<Obj_AI_Hero>().Any(h => h.ChampionName == "Yasuo" && h.IsEnemy && h.IsVisible && !h.IsDead)) return;
            //Yasuo Wall
            if (obj.IsValid &&
                System.Text.RegularExpressions.Regex.IsMatch(
                    obj.Name, "_w_windwall.\\.troy",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase))
            {
                _yasuoWall = obj;
            }
        }

        private static void OnDelete(GameObject obj, EventArgs args)
        {
            if (Player.Distance(obj.Position) > 1500 || !ObjectManager.Get<Obj_AI_Hero>().Any(h => h.ChampionName == "Yasuo" && h.IsEnemy && h.IsVisible && !h.IsDead)) return;
            //Yasuo Wall
            if (obj.IsValid && System.Text.RegularExpressions.Regex.IsMatch(
                obj.Name, "_w_windwall.\\.troy",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase))
            {
                _yasuoWall = null;
            }
        }

        private static bool DetectCollision(GameObject target, float x)
        {
            if (_yasuoWall == null || !ObjectManager.Get<Obj_AI_Hero>().Any(h => h.ChampionName == "Yasuo" && h.IsEnemy && h.IsVisible && !h.IsDead)) return true;

            var level = _yasuoWall.Name.Substring(_yasuoWall.Name.Length - 6, 1);
            var wallWidth = (300 + 50 * Convert.ToInt32(level));
            var wallDirection = (_yasuoWall.Position.To2D() - _yasuoWallCastedPos).Normalized().Perpendicular();
            var wallStart = _yasuoWall.Position.To2D() + ((int)(wallWidth / 2)) * wallDirection;
            var wallEnd = wallStart - wallWidth * wallDirection;

            var intersection = wallStart.Intersection(wallEnd, Player.Position.To2D(), target.Position.To2D());

            return !intersection.Point.IsValid() || !(Environment.TickCount + Game.Ping + x - _wallCastT < 4000);

        }
    }
}