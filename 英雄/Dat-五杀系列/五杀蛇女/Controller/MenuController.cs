using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;

namespace Pentakill_Cassiopeia.Controller
{
    public class MenuController
    {
        private Menu menu;

        public MenuController()
        {
            menu = new Menu("花边-Pentakill蛇女", "menu", true);
            Combo();
            Harass();
            LastHit();
            LaneClear();
            Drawings();
            Misc();
            Message();
        }

        private void Combo()
        {
            Menu comboMenu = menu.AddSubMenu(new Menu("连招", "combo"));
            comboMenu.AddItem(new MenuItem("comboUseQ", "使用 Q")).SetValue(true);
            comboMenu.AddItem(new MenuItem("comboUseW", "使用 W")).SetValue(true);
            comboMenu.AddItem(new MenuItem("comboUseE", "使用 E")).SetValue(true);
            comboMenu.AddItem(new MenuItem("comboUseR", "使用 R")).SetValue(true);
            //   comboMenu.AddItem(new MenuItem("faceOnlyR", "使用R丨目标可眩晕")).SetValue(true);
            comboMenu.AddItem(new MenuItem("minEnemies", "使用 R 最小敌人数").SetValue(new Slider(2, 1, 5)));
            comboMenu.AddItem(new MenuItem("useIgnite", "智能 点燃").SetValue(true));
        }

        private void Harass()
        {
            Menu harassMenu = menu.AddSubMenu(new Menu("骚扰", "harass"));
            harassMenu.AddItem(new MenuItem("harassUseQ", "使用 Q")).SetValue(true);
            harassMenu.AddItem(new MenuItem("harassUseW", "使用 W")).SetValue(false);
            harassMenu.AddItem(new MenuItem("harassUseE", "使用 E")).SetValue(true);
            harassMenu.AddItem(new MenuItem("harassManager", "最低蓝量比 %").SetValue(new Slider(60, 1, 100)));
        }

        private void LastHit()
        {
            Menu lastHitMenu = menu.AddSubMenu(new Menu("补刀", "lastHit"));
            lastHitMenu.AddItem(new MenuItem("lastHitUseQ", "使用 Q")).SetValue(true);
            lastHitMenu.AddItem(new MenuItem("lastHitUseE", "使用 E")).SetValue(true);
            lastHitMenu.AddItem(new MenuItem("lastHitManager", "最低蓝量比 %").SetValue(new Slider(50, 1, 100)));
        }

        private void LaneClear()
        {
            Menu laneClearMenu = menu.AddSubMenu(new Menu("清线", "laneClear"));
            laneClearMenu.AddItem(new MenuItem("laneClearUseQ", "使用 Q")).SetValue(true);
            laneClearMenu.AddItem(new MenuItem("laneClearUseW", "使用 W")).SetValue(true);
            laneClearMenu.AddItem(new MenuItem("laneClearUseE", "使用 E")).SetValue(true);
            laneClearMenu.AddItem(new MenuItem("laneClearManager", "最低蓝量比 %").SetValue(new Slider(25, 1, 100)));
        }

        private void Drawings()
        {
            Menu drawingsMenu = menu.AddSubMenu(new Menu("显示", "drawings"));
            drawingsMenu.AddItem(new MenuItem("drawQW", "显示 Q/W")).SetValue(true);
            drawingsMenu.AddItem(new MenuItem("drawE", "显示 E")).SetValue(true);
            drawingsMenu.AddItem(new MenuItem("drawR", "显示 R")).SetValue(true);
            drawingsMenu.AddItem(new MenuItem("drawDmg", "显示 伤害")).SetValue(true);
        }

        private void Misc()
        {
            menu.AddItem(new MenuItem("eDelay", "E 释放延迟(ms)")).SetValue(new Slider(75, 1, 1000));
            menu.AddItem(new MenuItem("autoLevel", "自动加点")).SetValue(true);
        }

        public Menu getOrbwalkingMenu()
        {
            return menu.AddSubMenu(new Menu("走砍设置", "orbwalkerMenu"));
        }

        public Menu getMenu()
        {
            return menu;
        }

        public void Message()
        {
            Menu messageMenu = menu.AddSubMenu(new Menu("信息", "message"));
            messageMenu.AddItem(new MenuItem("Sprite", "Pentakill蛇女"));
            messageMenu.AddItem(new MenuItem("Hanhua", "汉化:花边下丶情未央"));
            messageMenu.AddItem(new MenuItem("qqqun", "QQ群:299606556"));
        }

        public void addToMainMenu()
        {
            menu.AddToMainMenu();
        }

    }
}
