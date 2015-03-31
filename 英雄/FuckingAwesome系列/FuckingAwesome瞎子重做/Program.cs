using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace FuckingAwesomeLeeSinReborn
{
    class Program
    {
        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameStart;
        }

        public static Orbwalking.Orbwalker Orbwalker;
        public static Menu Config;

        static void Game_OnGameStart(EventArgs args)
        {
            if (ObjectManager.Player.ChampionName != "LeeSin")
            {
                Notifications.AddNotification(new Notification("not lee sin huh? wanna go m9?", 2));
                return;
            }
            Config = new Menu("花边汉化-FA瞎子重做", "you-stealing-me-src-m9?", true);

            Orbwalker = new Orbwalking.Orbwalker(Config.AddSubMenu(new Menu("走砍 设置", "Orbwalker")));
            TargetSelector.AddToMenu(Config.AddSubMenu(new Menu("目标 选择", "Target Selector")));

            var combo = Config.AddSubMenu(new Menu("连招 设置", "Combo"));
            combo.AddItem(new MenuItem("CQ", "使用 Q").SetValue(true));
            combo.AddItem(new MenuItem("smiteQ", "Q命中后使用惩戒").SetValue(false));
            combo.AddItem(new MenuItem("CE", "使用 E").SetValue(true));
            combo.AddItem(new MenuItem("CR", "使用 R (击杀)").SetValue(false));
            combo.AddItem(new MenuItem("CpassiveCheck", "被动 检测").SetValue(false));
            combo.AddItem(new MenuItem("CpassiveCheckCount", "最小 层数").SetValue(new Slider(1,1,2)));
            combo.AddItem(new MenuItem("starCombo", "连招 按键").SetValue(new KeyBind('T', KeyBindType.Press)));
            combo.AddItem(new MenuItem("starsadasCombo", "连招方式:Q->摸眼->W->R->Q2"));
            
            var harass = Config.AddSubMenu(new Menu("骚扰 设置", "Harass"));
            harass.AddItem(new MenuItem("HQ", "使用 Q").SetValue(true));
            harass.AddItem(new MenuItem("HE", "使用 E").SetValue(false));
            harass.AddItem(new MenuItem("HpassiveCheck", "被动 检测").SetValue(false));
            harass.AddItem(new MenuItem("HpassiveCheckCount", "最小 层数").SetValue(new Slider(1, 1, 2)));

            var insec = Config.AddSubMenu(new Menu("Insec 设置", "Insec"));
            insec.AddItem(new MenuItem("insecOrbwalk", "走砍 模式").SetValue(true));
            insec.AddItem(new MenuItem("clickInsec", "点击 Insec").SetValue(true));
            insec.AddItem(new MenuItem("sdgdsgsg", "先点击敌人再点击我方"));
            insec.AddItem(new MenuItem("ddfhdhdg", "最后的我方可以是塔 小兵 Or英雄"));
            insec.AddItem(new MenuItem("mouseInsec", "踢回鼠标位置").SetValue(false));
            insec.AddItem(new MenuItem("easyInsec", "简单 Insec").SetValue(true));
            insec.AddItem(new MenuItem("sdgdsgsdfdssg", "点击敌人后移动鼠标"));
            insec.AddItem(new MenuItem("ddfhdffdsdfdhdg", "(这会自动走到敌人那边)"));
            insec.AddItem(new MenuItem("q2InsecRange", "使用 Q2 丨有塔下BUFF (所有)").SetValue(true));
            insec.AddItem(new MenuItem("q1InsecRange", "使用 Q1 丨敌人在Insec范围内").SetValue(false));
            insec.AddItem(new MenuItem("flashInsec", "没眼自动R闪").SetValue(false));
            insec.AddItem(new MenuItem("insec", "Insec 按键").SetValue(new KeyBind('Y', KeyBindType.Press)));

            var autoSmite = Config.AddSubMenu(new Menu("自动 惩戒", "Auto Smite"));
            autoSmite.AddItem(new MenuItem("smiteEnabled", "惩戒 按键").SetValue(new KeyBind("M".ToCharArray()[0], KeyBindType.Toggle)));
            autoSmite.AddItem(new MenuItem("SRU_Red", "红 Buff").SetValue(true));
            autoSmite.AddItem(new MenuItem("SRU_Blue", "蓝 Buff").SetValue(true));
            autoSmite.AddItem(new MenuItem("SRU_Dragon", "小龙").SetValue(true));
            autoSmite.AddItem(new MenuItem("SRU_Baron", "大龙").SetValue(true));

            var farm = Config.AddSubMenu(new Menu("打钱 设置", "Farming"));
            farm.AddItem(new MenuItem("10010321223", "          清野"));
            farm.AddItem(new MenuItem("QJ", "使用 Q").SetValue(true));
            farm.AddItem(new MenuItem("WJ", "使用 W").SetValue(true));
            farm.AddItem(new MenuItem("EJ", "使用 E").SetValue(true));
            farm.AddItem(new MenuItem("5622546001", "          清线"));
            farm.AddItem(new MenuItem("QWC", "使用 Q").SetValue(true));
            farm.AddItem(new MenuItem("EWC", "使用 E").SetValue(true));

            var draw = Config.AddSubMenu(new Menu("范围 设置", "Draw"));
            draw.AddItem(new MenuItem("LowFPS", "低FPS模式").SetValue(false));
            draw.AddItem(new MenuItem("LowFPSMode", "低 FPS 设置").SetValue(new StringList(new []{"极端", "中等", "低"}, 2)));
            draw.AddItem(new MenuItem("DQ", "显示 Q 范围").SetValue(new Circle(false, Color.White)));
            draw.AddItem(new MenuItem("DW", "显示 W 范围").SetValue(new Circle(false, Color.White)));
            draw.AddItem(new MenuItem("DE", "显示 E 范围").SetValue(new Circle(false, Color.White)));
            draw.AddItem(new MenuItem("DR", "显示 R 范围").SetValue(new Circle(false, Color.White)));
            draw.AddItem(new MenuItem("DS", "显示 惩戒 范围").SetValue(new Circle(false, Color.PowderBlue)));
            draw.AddItem(new MenuItem("DWJ", "显示 摸眼位置").SetValue(true));
            draw.AddItem(new MenuItem("DES", "显示 逃跑 位置").SetValue(true));

            var escape = Config.AddSubMenu(new Menu("逃跑 设置", "Escape Settings"));
            escape.AddItem(new MenuItem("escapeMode", "开启 野区 逃生").SetValue(true));
            escape.AddItem(new MenuItem("Wardjump", "逃生 按键").SetValue(new KeyBind('Z', KeyBindType.Press)));
            escape.AddItem(new MenuItem("alwaysJumpMaxRange", "自动 跳到最大范围").SetValue(true));
            escape.AddItem(new MenuItem("jumpChampions", "跳到英雄上").SetValue(true));
            escape.AddItem(new MenuItem("jumpMinions", "跳到小兵上").SetValue(true));
            escape.AddItem(new MenuItem("jumpWards", "跳到眼上").SetValue(true));

            var info = Config.AddSubMenu(new Menu("信息 显示", "info"));
            info.AddItem(new MenuItem("Msddsds", "假如你喜欢这个本你可以使用paypal捐赠"));
            info.AddItem(new MenuItem("Msdsddsd", "下面是FA作者的paypal捐赠:"));
            info.AddItem(new MenuItem("Msdsadfdsd", "jayyeditsdude@gmail.com"));

            Config.AddItem(new MenuItem("Mgdgdfgsd", "版本: 0.0.1-5 BETA"));
            Config.AddItem(new MenuItem("Msd", "作者 FluxySenpai"));
            Config.AddItem(new MenuItem("M1sd", "花边汉化-FA瞎子重做!"));
            Config.AddItem(new MenuItem("M1s11d", "提示:FA瞎子重做与自带走砍键位有冲突"));
            Config.AddItem(new MenuItem("M12sd", "请自行设置"));



            Config.AddToMainMenu();
            
            CheckHandler.spells[SpellSlot.Q].SetSkillshot(0.25f, 65f, 1800f, true, SkillshotType.SkillshotLine);

            CheckHandler.Init();
            JumpHandler.Load();
            Game.OnUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnWndProc += InsecHandler.OnClick;
            AutoSmite.Init();
            Obj_AI_Base.OnProcessSpellCast += CheckHandler.Obj_AI_Hero_OnProcessSpellCast;
            Notifications.AddNotification(new Notification("Fucking Awesome Lee Sin:", 2));
            Notifications.AddNotification(new Notification("REBORN", 2));
        }

        static void Drawing_OnDraw(EventArgs args)
        {
            var lowFps = Config.Item("LowFPS").GetValue<bool>();
            var lowFpsMode = Config.Item("LowFPSMode").GetValue<StringList>().SelectedIndex + 1;
            if (Config.Item("DQ").GetValue<Circle>().Active)
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, CheckHandler.spells[SpellSlot.Q].Range, Config.Item("DQ").GetValue<Circle>().Color, lowFps ? lowFpsMode : 5);
            }
            if (Config.Item("DW").GetValue<Circle>().Active)
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, CheckHandler.spells[SpellSlot.W].Range, Config.Item("DW").GetValue<Circle>().Color , lowFps ? lowFpsMode : 5);
            }
            if (Config.Item("DE").GetValue<Circle>().Active)
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, CheckHandler.spells[SpellSlot.E].Range, Config.Item("DE").GetValue<Circle>().Color , lowFps ? lowFpsMode : 5);
            }
            if (Config.Item("DR").GetValue<Circle>().Active)
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, CheckHandler.spells[SpellSlot.R].Range, Config.Item("DR").GetValue<Circle>().Color , lowFps ? lowFpsMode : 5);
            }
            WardjumpHandler.Draw();
            InsecHandler.Draw();
        }

        static void Game_OnGameUpdate(EventArgs args)
        {
            if (CheckHandler.LastSpell + 3000 <= Environment.TickCount)
            {
                CheckHandler.PassiveStacks = 0;
            }
            if (Config.Item("starCombo").GetValue<KeyBind>().Active)
            {
                StateHandler.StarCombo();
                return;
            }
            if (Config.Item("insec").GetValue<KeyBind>().Active)
            {
                InsecHandler.DoInsec();
                return;
            }
                InsecHandler.FlashPos = new Vector3();
                InsecHandler.FlashR = false;

            if (Config.Item("Wardjump").GetValue<KeyBind>().Active)
            {
                WardjumpHandler.DrawEnabled = Config.Item("DWJ").GetValue<bool>();
                WardjumpHandler.Jump(Game.CursorPos, Config.Item("alwaysJumpMaxRange").GetValue<bool>(), true);
                return;
            }
            WardjumpHandler.DrawEnabled = false;

            switch (Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                StateHandler.Combo();
                return;
                case Orbwalking.OrbwalkingMode.LaneClear:
                StateHandler.JungleClear();
                return;
                case Orbwalking.OrbwalkingMode.Mixed:
                StateHandler.Harass();
                return;
            }
        }
    }
}
 