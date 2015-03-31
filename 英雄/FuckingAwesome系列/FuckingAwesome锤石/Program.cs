﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using Color = System.Drawing.Color;

namespace FuckingAwesomeThresh
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameStart;
        }

        public static Orbwalking.Orbwalker Orbwalker;
        public static Menu Config;

        private static void Game_OnGameStart(EventArgs args)
        {
            if (ObjectManager.Player.ChampionName != "Thresh")
            {
                Notifications.AddNotification(new Notification("not thresh sin huh? wanna go m9?", 2));
                return;
            }
            Config = new Menu("花边汉化-FA锤石", "you-stealing-me-src-m29?", true);

            Orbwalker = new Orbwalking.Orbwalker(Config.AddSubMenu(new Menu("走砍 设置", "Orbwalker")));
            TargetSelector.AddToMenu(Config.AddSubMenu(new Menu("目标 选择", "Target Selector")));

            var combo = Config.AddSubMenu(new Menu("连招 设置", "Combo"));
            combo.AddItem(new MenuItem("CQ", "使用 Q").SetValue(true));
            combo.AddItem(new MenuItem("CW", "使用 W").SetValue(true));
            combo.AddItem(new MenuItem("CE", "使用 E").SetValue(true));
            combo.AddItem(new MenuItem("CR", "使用 R").SetValue(false));

            var allyMenu = combo.AddSubMenu(new Menu("W 优先设置 (排列)", "WPrior"));
            allyMenu.AddItem(
                new MenuItem("kepe", "1是最低优先权"));

            foreach (var ally in ObjectManager.Get<Obj_AI_Hero>().Where(a => a.IsAlly))
            {
                allyMenu.AddItem(
                    new MenuItem("priority" + ally.ChampionName, ally.ChampionName).SetValue(new Slider(5, 1, 5)));
            }

            var harass = Config.AddSubMenu(new Menu("骚扰 设置", "Harass"));
            harass.AddItem(new MenuItem("HQ", "使用 Q").SetValue(true));
            harass.AddItem(new MenuItem("HE", "使用 E").SetValue(false));

            var draw = Config.AddSubMenu(new Menu("范围 设置", "Draw"));
            draw.AddItem(new MenuItem("DQ", "显示 Q 范围").SetValue(new Circle(false, Color.White)));
            draw.AddItem(new MenuItem("DW", "显示 W 范围").SetValue(new Circle(false, Color.White)));
            draw.AddItem(new MenuItem("DE", "显示 E 范围").SetValue(new Circle(false, Color.White)));
            draw.AddItem(new MenuItem("DR", "显示 R 范围").SetValue(new Circle(false, Color.White)));
            draw.AddItem(new MenuItem("DES", "显示 逃跑 位置").SetValue(true));

            var escape = Config.AddSubMenu(new Menu("杂项 设置", "Misc"));
            escape.AddItem(new MenuItem("LanternAlly", "自动扔灯笼给友军").SetValue(true));
            escape.AddItem(new MenuItem("Escape", "逃跑 按键").SetValue(new KeyBind('Z', KeyBindType.Press)));

            var keys = Config.AddSubMenu(new Menu("按键 设置", "Kays"));
            keys.AddItem(new MenuItem("engageCombo", "W 按键").SetValue(new KeyBind('M', KeyBindType.Press)));
            keys.AddItem(new MenuItem("pull", "拉人 按键").SetValue(new KeyBind('T', KeyBindType.Press)));
            keys.AddItem(new MenuItem("push", "推走 按键").SetValue(new KeyBind('Y', KeyBindType.Press)));
            keys.AddItem(new MenuItem("flashPull", "闪现拉人 按键").SetValue(new KeyBind('K', KeyBindType.Press)));
            keys.AddItem(new MenuItem("shieldAlly", "保护 友军").SetValue(new KeyBind('L', KeyBindType.Press)));

            var info = Config.AddSubMenu(new Menu("信息 显示", "info"));
            info.AddItem(new MenuItem("Msddsds", "假如你喜欢FA锤石可以利用 paypal 捐赠"));
            info.AddItem(new MenuItem("Msdsddsd", "你可以把钱转到这个帐号:"));
            info.AddItem(new MenuItem("Msdsadfdsd", "jayyeditsdude@gmail.com"));

            Config.AddItem(new MenuItem("Mgdgdfgsd", "版本: 0.0.1-2 BETA"));
            Config.AddItem(new MenuItem("Msd", "作者: FluxySenpai"));
            Config.AddItem(new MenuItem("M1sd", "花边汉化-FA锤石!"));


            Config.AddToMainMenu();

            CheckHandler.Spells[SpellSlot.Q].SetSkillshot(0.5f, 70f, 1900, true, SkillshotType.SkillshotLine);

            Game.OnUpdate += Game_OnUpdate;
            CheckHandler.Init();

            Notifications.AddNotification(new Notification("Fucking Awesome Thresh", 2));

        }

        static void Game_OnUpdate(EventArgs args)
        {
            switch (Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    ComboStateHandler.Combo();
                    break;
            }
            if (Config.Item("engageCombo").GetValue<KeyBind>().Active) ComboStateHandler.EngageCombo();
            if (Config.Item("pull").GetValue<KeyBind>().Active) ComboStateHandler.Pull();
            if (Config.Item("push").GetValue<KeyBind>().Active) ComboStateHandler.Push();
            if (Config.Item("flashPull").GetValue<KeyBind>().Active) ComboStateHandler.FlashPull();
        }
    }
}
