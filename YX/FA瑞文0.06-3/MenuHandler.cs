using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp.Common;
using SharpDX;

namespace FuckingAwesomeRiven
{
    class MenuHandler
    {

        public static Orbwalking.Orbwalker Orbwalker;
        public static Menu Config;
        public static List<jumpPosition> j = new List<jumpPosition>();

        public static void initMenu()
        {
            Config = new Menu("花边私人汉化-FA瑞文", "KappaChino", true);

            Orbwalker = new Orbwalking.Orbwalker(Config.AddSubMenu(new Menu("走 砍", "OW")));
            TargetSelector.AddToMenu(Config.AddSubMenu(new Menu("目标 选择", "Target Selector")));

            var combo = Config.AddSubMenu(new Menu("连招 设置", "Combo"));
            combo.AddItem(new MenuItem("xdxdxdxd", "普通 连招"));

            var enabledCombos = combo.AddSubMenu(new Menu("击杀 连招", "Killable Combos"));
            enabledCombos.AddItem(new MenuItem("HaveR", "有R 连招方式"));
            enabledCombos.AddItem(new MenuItem("QWR2KS", "Q - W - R2").SetValue(true));
            enabledCombos.AddItem(new MenuItem("QR2KS", "Q - R2").SetValue(true));
            enabledCombos.AddItem(new MenuItem("WR2KS", "W - R2").SetValue(true));
            enabledCombos.AddItem(new MenuItem("NoR", "没有R 连招方式"));
            enabledCombos.AddItem(new MenuItem("QWKS", "Q - W").SetValue(true));
            enabledCombos.AddItem(new MenuItem("QKS", "Q").SetValue(true));
            enabledCombos.AddItem(new MenuItem("WKS", "W").SetValue(true));

            combo.AddItem(new MenuItem("CQ", "使用 Q").SetValue(true));
            combo.AddItem(new MenuItem("QAA", "Q+AA 模式").SetValue(new StringList(new[] { "Q -> AA", "AA -> Q" })));
            combo.AddItem(new MenuItem("UseQ-GC2", "使用Q丨突进").SetValue(false));
            combo.AddItem(new MenuItem("Use R2", "使用 R2").SetValue(true));
            combo.AddItem(new MenuItem("CW", "使用 W").SetValue(true));
            combo.AddItem(new MenuItem("CE", "使用 E").SetValue(true));
            combo.AddItem(new MenuItem("UseE-AA", "假如敌人离开攻击范围丨仅使用E").SetValue(true));
            combo.AddItem(new MenuItem("UseE-GC", "使用E丨突进").SetValue(true));
            combo.AddItem(new MenuItem("CR", "使用 R").SetValue(true));
            combo.AddItem(new MenuItem("CRNO", "使用 R2丨最小敌人数").SetValue(new Slider(2, 1, 5)));
            combo.AddItem(new MenuItem("forcedR", "连招强行使用R").SetValue(new KeyBind('T', KeyBindType.Toggle, false)));
            combo.AddItem(new MenuItem("CR2", "使用 R2").SetValue(true));
            combo.AddItem(new MenuItem("magnet", "黏住 目标").SetValue(false));
            combo.AddItem(new MenuItem("Baofalz", "爆发 连招"));
            //combo.AddItem(new MenuItem("BFl", "Use Flash").SetValue(false));取消爆发连招中闪现连招 上版本bug导致无法正常使用爆发连招
            combo.AddItem(new MenuItem("shyCombo", "ShyCombo(恶心连招)").SetValue(true));
            combo.AddItem(new MenuItem("kyzerCombo", "Kyzer Q3 Combo(第三下Q连招)").SetValue(true));

            var farm = Config.AddSubMenu(new Menu("打钱 设置", "Farming"));
            farm.AddItem(new MenuItem("Budao", "补刀 设置"));
            farm.AddItem(new MenuItem("QLH", "使用 Q").SetValue(true));
            farm.AddItem(new MenuItem("WLH", "使用 W").SetValue(true));
            farm.AddItem(new MenuItem("QY", "清野 设置"));
            farm.AddItem(new MenuItem("QJ", "使用 Q").SetValue(true));
            farm.AddItem(new MenuItem("WJ", "使用 W").SetValue(true));
            farm.AddItem(new MenuItem("EJ", "使用 E").SetValue(true));
            farm.AddItem(new MenuItem("QX", "清线 设置"));
            farm.AddItem(new MenuItem("QWC", "使用 Q").SetValue(true));
            farm.AddItem(new MenuItem("QWC-LH", "Q 补刀").SetValue(true));
            farm.AddItem(new MenuItem("QWC-AA", "Q+AA").SetValue(true));
            farm.AddItem(new MenuItem("WWC", "使用 W").SetValue(true));

            var cancels = Config.AddSubMenu(new Menu("消除 动画", "autoCancels"));
            cancels.AddItem(new MenuItem("autoCancelR1", "R1").SetValue(false));
            cancels.AddItem(new MenuItem("autoCancelR2", "R2").SetValue(false));
            cancels.AddItem(new MenuItem("autoCancelT", "提亚马特 Or 九头蛇").SetValue(true));
            cancels.AddItem(new MenuItem("autoCancelE", "E").SetValue(false));

            var draw = Config.AddSubMenu(new Menu("范围 设置", "Draw"));

            draw.AddItem(new MenuItem("DQ", "显示 Q 范围").SetValue(new Circle(false, System.Drawing.Color.White)));
            draw.AddItem(new MenuItem("DW", "显示 W 范围").SetValue(new Circle(false, System.Drawing.Color.White)));
            draw.AddItem(new MenuItem("DE", "显示 E 范围").SetValue(new Circle(false, System.Drawing.Color.White)));
            draw.AddItem(new MenuItem("DR", "显示 R 范围").SetValue(new Circle(false, System.Drawing.Color.White)));
            draw.AddItem(new MenuItem("DBC", "显示 爆发连招 范围").SetValue(new Circle(false, System.Drawing.Color.White)));
            draw.AddItem(new MenuItem("DD", "显示伤害 [瞬间]").SetValue(new Circle(false, System.Drawing.Color.White)));

            var misc = Config.AddSubMenu(new Menu("杂项 设置", "Misc"));
            misc.AddItem(new MenuItem("bonusCancelDelay", "消除 额外延迟 (ms)").SetValue(new Slider(0, 0, 500)));
            misc.AddItem(new MenuItem("keepQAlive", "保持Q 激活状态").SetValue(true));
            misc.AddItem(new MenuItem("QFlee", "Q 逃跑").SetValue(true));
            misc.AddItem(new MenuItem("EFlee", "E 逃跑").SetValue(true));

            var Keybindings = Config.AddSubMenu(new Menu("按键 设置", "KB"));
            Keybindings.AddItem(new MenuItem("normalCombo", "普通 连招").SetValue(new KeyBind(32, KeyBindType.Press)));
            Keybindings.AddItem(new MenuItem("burstCombo", "爆发 连招").SetValue(new KeyBind('M', KeyBindType.Press)));
            Keybindings.AddItem(new MenuItem("jungleCombo", "清野 按键").SetValue(new KeyBind('C', KeyBindType.Press)));
            Keybindings.AddItem(new MenuItem("waveClear", "清线 按键").SetValue(new KeyBind('C', KeyBindType.Press)));
            Keybindings.AddItem(new MenuItem("lastHit", "补刀 按键").SetValue(new KeyBind('X', KeyBindType.Press)));
            Keybindings.AddItem(new MenuItem("flee", "逃跑 按键").SetValue(new KeyBind('Z', KeyBindType.Press)));

            EvadeUtils.AutoE.init();

            Antispells.init();

            var Info = Config.AddSubMenu(new Menu("信息 显示", "info"));
            Info.AddItem(new MenuItem("Msddsds", "假如你喜欢FA瑞文可以利用 paypal 捐赠"));
            Info.AddItem(new MenuItem("Msdsddsd", "你可以把钱转到这个帐号:"));
            Info.AddItem(new MenuItem("Msdsadfdsd", "jayyeditsdude@gmail.com"));
            Info.AddItem(new MenuItem("debug", "调试 模式")).SetValue(false);
            Info.AddItem(new MenuItem("logPos", "日志 位置").SetValue(false));
            Info.AddItem(new MenuItem("printPos", "显示 位置").SetValue(false));
            Info.AddItem(new MenuItem("clearPrevious", "清除 日志").SetValue(false));
            Info.AddItem(new MenuItem("clearCurrent", "清楚 错误").SetValue(false));
            Info.AddItem(new MenuItem("drawCirclesforTest", "显示 线圈").SetValue(false));

            Config.AddItem(new MenuItem("Mgdgdfgsd", "版本: 0.0.6-3 BETA"));
            Config.AddItem(new MenuItem("Msd", "作者: FluxySenpai"));
            Config.AddItem(new MenuItem("M1sd", "花边私人汉化-FA瑞文!"));

            Config.AddToMainMenu();
        }

        public static bool getMenuBool(String s)
        {
            return Config.Item(s).GetValue<bool>();
        }
    }
}
