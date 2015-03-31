using System;
using System.Collections.Generic;
using System.Drawing;
using FuckingAwesomeRiven.EvadeUtils;
using LeagueSharp.Common;

namespace FuckingAwesomeRiven
{
    internal class MenuHandler
    {
        public static Orbwalking.Orbwalker Orbwalker;
        public static Menu Config;
        public static List<JumpPosition> J = new List<JumpPosition>();

        public static void InitMenu()
        {
            Config = new Menu("花边汉化-FA瑞文", "KappaChino", true);

            Orbwalker = new Orbwalking.Orbwalker(Config.AddSubMenu(new Menu("走 砍", "OW")));
            TargetSelector.AddToMenu(Config.AddSubMenu(new Menu("目标 选择", "Target Selector")));

            var combo = Config.AddSubMenu(new Menu("连招 设置", "Combo"));
            combo.AddItem(new MenuItem("xdxdxdxd", "普通 连招"));

            combo.AddItem(new MenuItem("QAA", "QA 模式").SetValue(new StringList(new[] {"Q -> AA", "AA -> Q"})));
            var gcM = combo.AddSubMenu(new Menu("突进 连招", "Gapcloser Combos"));
            gcM.AddItem(new MenuItem("CEWHQ", "E->W->九头蛇->Q").SetValue(true));
            gcM.AddItem(new MenuItem("CQWH", "Q->W->九头蛇").SetValue(true));
            gcM.AddItem(new MenuItem("CEHQ", "E->九头蛇->Q").SetValue(true));
            gcM.AddItem(new MenuItem("CEW", "E->W").SetValue(true));

            var r1Combo = combo.AddSubMenu(new Menu("R1 设置", "R1 Combos"));
            r1Combo.AddItem(new MenuItem("CREWHQ", "R->E->W->九头蛇->Q").SetValue(true));
            r1Combo.AddItem(new MenuItem("CREWH", "R->E->W->九头蛇").SetValue(true));
            r1Combo.AddItem(new MenuItem("CREAAHQ", "R->E->AA->九头蛇->Q").SetValue(true));
            r1Combo.AddItem(new MenuItem("CRWAAHQ", "R->W->AA->九头蛇-Q").SetValue(true));
            r1Combo.AddItem(new MenuItem("CR1CC", "R").SetValue(true));

            var r2Combo = combo.AddSubMenu(new Menu("R2 设置", "R2 Combos"));
            r2Combo.AddItem(new MenuItem("CR2WQ", "R2->W->Q").SetValue(true));
            r2Combo.AddItem(new MenuItem("CR2W", "R2->W").SetValue(true));
            r2Combo.AddItem(new MenuItem("CR2Q", "R2->Q").SetValue(true));
            r2Combo.AddItem(new MenuItem("CR2CC", "R2").SetValue(true));

            combo.AddItem(new MenuItem("CQ", "使用 Q").SetValue(true));
            combo.AddItem(new MenuItem("CW", "使用 W").SetValue(true));
            combo.AddItem(new MenuItem("CE", "使用 E").SetValue(true));
            combo.AddItem(new MenuItem("CRnote", "提示:下面的是是否启用该连招方式"));
            combo.AddItem(new MenuItem("CR", "使用 R1").SetValue(true));
            combo.AddItem(new MenuItem("CR2", "使用 R2").SetValue(true));

            var harass = Config.AddSubMenu(new Menu("骚扰 设置", "Harass"));
            harass.AddItem(new MenuItem("fdsf", "骚扰 连招"));
            harass.AddItem(new MenuItem("HQ3AAWE", "Q三下->AA->W->E回来").SetValue(true));
            harass.AddItem(new MenuItem("HQAA3WE", "(Q->A3下->W->E回来").SetValue(true));
            harass.AddItem(new MenuItem("sdffsdf", ""));
            harass.AddItem(new MenuItem("HQ", "使用 Q").SetValue(true));
            harass.AddItem(new MenuItem("HW", "使用 W").SetValue(true));
            harass.AddItem(new MenuItem("HE", "使用 E 回来| 默认禁止").SetValue(false));

            var burst = Config.AddSubMenu(new Menu("爆发 连招", "Burst Combos"));
            burst.AddItem(new MenuItem("shyCombo", "-- 天马行空").SetValue(true));
            burst.AddItem(new MenuItem("shyComboinfo1", "E->R->闪现->AA->九头蛇->W->R2->Q3"));
            burst.AddItem(new MenuItem("shyComboinfo2", "启用方式:按住爆发按键(Q1 Q2时)"));
            burst.AddItem(new MenuItem("kyzerCombo", "-- 行云流水").SetValue(true));
            burst.AddItem(new MenuItem("kyzerComboinfo1", "E->R->闪现->Q->AA->九头蛇->W->R2->Q"));
            burst.AddItem(new MenuItem("kyzerComboinfo2", "启用方式:按住爆发按键 Q3"));
            burst.AddItem(new MenuItem("flashlessBurst", "-- 无闪现爆发").SetValue(true));
            burst.AddItem(new MenuItem("flashlessBurst1", "E->R->W->Hydra->AA->R2->Q"));
            burst.AddItem(new MenuItem("flashlessBurst2", "启用方式:按住爆发按键 Q1Q2Q3均可"));

            var farm = Config.AddSubMenu(new Menu("打钱 设置", "Farming"));
            farm.AddItem(new MenuItem("fnjdsjkn", "          补刀"));
            farm.AddItem(new MenuItem("QLH", "使用 Q").SetValue(true));
            farm.AddItem(new MenuItem("WLH", "使用 W").SetValue(true));
            farm.AddItem(new MenuItem("10010321223", "          清野"));
            farm.AddItem(new MenuItem("QJ", "使用 Q").SetValue(true));
            farm.AddItem(new MenuItem("WJ", "使用 W").SetValue(true));
            farm.AddItem(new MenuItem("EJ", "使用 E").SetValue(true));
            farm.AddItem(new MenuItem("5622546001", "          清线"));
            farm.AddItem(new MenuItem("QWC", "使用 Q").SetValue(true));
            farm.AddItem(new MenuItem("QWC-LH", "   Q 补刀").SetValue(true));
            farm.AddItem(new MenuItem("QWC-AA", "   Q -> AA").SetValue(true));
            farm.AddItem(new MenuItem("WWC", "使用 W").SetValue(true));

            var draw = Config.AddSubMenu(new Menu("范围 设置", "Draw"));
            draw.AddItem(new MenuItem("DALL", "关闭 线圈显示").SetValue(false));
            draw.AddItem(new MenuItem("DQ", "显示 Q 范围").SetValue(new Circle(false, Color.White)));
            draw.AddItem(new MenuItem("DW", "显示 W 范围").SetValue(new Circle(false, Color.White)));
            draw.AddItem(new MenuItem("DE", "显示 E 范围").SetValue(new Circle(false, Color.White)));
            draw.AddItem(new MenuItem("DR", "显示 R 范围").SetValue(new Circle(false, Color.White)));
            draw.AddItem(new MenuItem("DBC", "显示 爆发 连招 范围").SetValue(new Circle(false, Color.White)));
            draw.AddItem(new MenuItem("DER", "显示 R2 范围").SetValue(new Circle(true, Color.White)));
            draw.AddItem(new MenuItem("DD", "显示 伤害 [瞬间]").SetValue(new Circle(false, Color.White)));

            var misc = Config.AddSubMenu(new Menu("杂项 设置", "Misc"));
            misc.AddItem(new MenuItem("bonusCancelDelay", "金钱获得延迟 (ms)").SetValue(new Slider(0, 0, 500)));
            misc.AddItem(new MenuItem("keepQAlive", "保持 Q 激活状态").SetValue(true));
            misc.AddItem(new MenuItem("QFlee", "Q 逃跑").SetValue(true));
            misc.AddItem(new MenuItem("EFlee", "E 逃跑").SetValue(true));

            var keyBindings = Config.AddSubMenu(new Menu("按键 设置", "KB"));
            keyBindings.AddItem(
                new MenuItem("normalCombo", "普通 连招").SetValue(new KeyBind(32, KeyBindType.Press)));
            keyBindings.AddItem(new MenuItem("burstCombo", "爆发 连招").SetValue(new KeyBind('M', KeyBindType.Press)));
            keyBindings.AddItem(
                new MenuItem("jungleCombo", "清野 按键").SetValue(new KeyBind('C', KeyBindType.Press)));
            keyBindings.AddItem(new MenuItem("waveClear", "清线 按键").SetValue(new KeyBind('C', KeyBindType.Press)));
            keyBindings.AddItem(new MenuItem("lastHit", "补刀 按键").SetValue(new KeyBind('X', KeyBindType.Press)));
            keyBindings.AddItem(new MenuItem("harass", "骚扰 按键").SetValue(new KeyBind('V', KeyBindType.Press)));
            keyBindings.AddItem(new MenuItem("flee", "逃跑 按键").SetValue(new KeyBind('Z', KeyBindType.Press)));
            keyBindings.AddItem(
                new MenuItem("forcedR", "连招时自动R 按键").SetValue(
                    new KeyBind('T', KeyBindType.Toggle)));

            Config.AddSubMenu(new Menu("突进 技能", "Anti Spells"));

            AutoE.Init();

            Antispells.Init();

            var info = Config.AddSubMenu(new Menu("信息 显示", "info"));
            info.AddItem(new MenuItem("Msddsds", "假如你喜欢FA瑞文可以利用 paypal 捐赠"));
            info.AddItem(new MenuItem("Msdsddsd", "你可以把钱转到这个帐号:"));
            info.AddItem(new MenuItem("Msdsadfdsd", "jayyeditsdude@gmail.com"));
            info.AddItem(new MenuItem("debug", "调试 模式")).SetValue(false);
            info.AddItem(new MenuItem("logPos", "日志 位置").SetValue(false));
            info.AddItem(new MenuItem("printPos", "显示 位置").SetValue(false));
            info.AddItem(new MenuItem("clearPrevious", "清除 日志").SetValue(false));
            info.AddItem(new MenuItem("clearCurrent", "清除 错误").SetValue(false));
            info.AddItem(new MenuItem("drawCirclesforTest", "显示 线圈").SetValue(false));

            Config.AddItem(new MenuItem("streamMouse", "鼠标跟随模式").SetValue(false));
            Config.AddItem(new MenuItem("Mgdgdfgsd", "版本: 0.0.9.0 BETA"));
            Config.AddItem(new MenuItem("Msd", "作者 FluxySenpai"));
            Config.AddItem(new MenuItem("M1sd", "花边汉化-FA瑞文!"));
            Config.AddItem(new MenuItem("M1s11d", "提示:FA瑞文与自带走砍键位有冲突"));
            Config.AddItem(new MenuItem("M12sd", "请自行设置"));

            Config.AddToMainMenu();
        }

        public static bool GetMenuBool(String s)
        {
            return Config.Item(s).GetValue<bool>();
        }
    }
}