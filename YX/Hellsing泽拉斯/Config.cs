using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LeagueSharp;
using LeagueSharp.Common;

using Color = System.Drawing.Color;

namespace Xerath
{
    public class Config
    {
        public const string MENU_NAME = "[Hellsing] " + Program.CHAMP_NAME;
        private static MenuWrapper _menu;

        private static Dictionary<string, MenuWrapper.BoolLink> _boolLinks = new Dictionary<string, MenuWrapper.BoolLink>();
        private static Dictionary<string, MenuWrapper.CircleLink> _circleLinks = new Dictionary<string, MenuWrapper.CircleLink>();
        private static Dictionary<string, MenuWrapper.KeyBindLink> _keyLinks = new Dictionary<string, MenuWrapper.KeyBindLink>();
        private static Dictionary<string, MenuWrapper.SliderLink> _sliderLinks = new Dictionary<string, MenuWrapper.SliderLink>();
        private static Dictionary<string, MenuWrapper.StringListLink> _stringListLinks = new Dictionary<string, MenuWrapper.StringListLink>();

        public static MenuWrapper Menu
        {
            get { return _menu; }
        }

        public static Dictionary<string, MenuWrapper.BoolLink> BoolLinks
        {
            get { return _boolLinks; }
        }
        public static Dictionary<string, MenuWrapper.CircleLink> CircleLinks
        {
            get { return _circleLinks; }
        }
        public static Dictionary<string, MenuWrapper.KeyBindLink> KeyLinks
        {
            get { return _keyLinks; }
        }
        public static Dictionary<string, MenuWrapper.SliderLink> SliderLinks
        {
            get { return _sliderLinks; }
        }
        public static Dictionary<string, MenuWrapper.StringListLink> StringListLinks
        {
            get { return _stringListLinks; }
        }

        private static void ProcessLink(string key, object value)
        {
            if (value is MenuWrapper.BoolLink)
                _boolLinks.Add(key, value as MenuWrapper.BoolLink);
            else if (value is MenuWrapper.CircleLink)
                _circleLinks.Add(key, value as MenuWrapper.CircleLink);
            else if (value is MenuWrapper.KeyBindLink)
                _keyLinks.Add(key, value as MenuWrapper.KeyBindLink);
            else if (value is MenuWrapper.SliderLink)
                _sliderLinks.Add(key, value as MenuWrapper.SliderLink);
            else if (value is MenuWrapper.StringListLink)
                _stringListLinks.Add(key, value as MenuWrapper.StringListLink);
        }

        public static void Initialize()
        {
            // Create menu
            _menu = new MenuWrapper("花边修复-H泽拉斯完美不崩");

            // ----- Combo
            var subMenu = _menu.MainMenu.AddSubMenu("连 招");
            var subSubMenu = subMenu.AddSubMenu("使用 Q");
            ProcessLink("comboUseQ", subSubMenu.AddLinkedBool("开 启"));
            ProcessLink("comboExtraRangeQ", subSubMenu.AddLinkedSlider("Q 额外范围", 200, 0, 200));
            ProcessLink("comboUseW", subMenu.AddLinkedBool("使用 W"));
            ProcessLink("comboUseE", subMenu.AddLinkedBool("使用 E"));
            ProcessLink("comboUseR", subMenu.AddLinkedBool("使用 R", false));
            //ProcessLink("comboUseItems", subMenu.AddLinkedBool("Use items"));
            //ProcessLink("comboUseIgnite", subMenu.AddLinkedBool("Use Ignite"));
            ProcessLink("comboActive", subMenu.AddLinkedKeyBind("连招 按键", 32, KeyBindType.Press));

            // ----- Harass
            subMenu = _menu.MainMenu.AddSubMenu("骚 扰");
            subSubMenu = subMenu.AddSubMenu("使用 Q");
            ProcessLink("harassUseQ", subSubMenu.AddLinkedBool("开 启"));
            ProcessLink("harassExtraRangeQ", subSubMenu.AddLinkedSlider("Q 额外范围", 200, 0, 200));
            ProcessLink("harassUseW", subMenu.AddLinkedBool("使用 W"));
            ProcessLink("harassUseE", subMenu.AddLinkedBool("使用 E"));
            ProcessLink("harassMana", subMenu.AddLinkedSlider("骚扰丨最低蓝量比", 30));
            ProcessLink("harassActive", subMenu.AddLinkedKeyBind("骚扰 按键", 'C', KeyBindType.Press));

            // ----- WaveClear
            subMenu = _menu.MainMenu.AddSubMenu("清 线");
            ProcessLink("waveUseQ", subMenu.AddLinkedBool("使用 Q"));
            ProcessLink("waveNumQ", subMenu.AddLinkedSlider("Q丨清兵数", 3, 1, 10));
            ProcessLink("waveUseW", subMenu.AddLinkedBool("使用 W"));
            ProcessLink("waveNumW", subMenu.AddLinkedSlider("W丨清兵数", 3, 1, 10));
            ProcessLink("waveMana", subMenu.AddLinkedSlider("清线丨最低蓝量比", 30));
            ProcessLink("waveActive", subMenu.AddLinkedKeyBind("清线 按键", 'V', KeyBindType.Press));

            // ----- JungleClear
            subMenu = _menu.MainMenu.AddSubMenu("清 野");
            ProcessLink("jungleUseQ", subMenu.AddLinkedBool("使用 Q"));
            ProcessLink("jungleUseW", subMenu.AddLinkedBool("使用 W"));
            ProcessLink("jungleUseE", subMenu.AddLinkedBool("使用 E"));
            ProcessLink("jungleActive", subMenu.AddLinkedKeyBind("清野 按键", 'V', KeyBindType.Press));

            // ----- Flee
            subMenu = _menu.MainMenu.AddSubMenu("逃 跑");
            ProcessLink("fleeNothing", subMenu.AddLinkedBool("没技能就奔跑吧 骚年"));
            ProcessLink("fleeActive", subMenu.AddLinkedKeyBind("逃跑 按键", 'T', KeyBindType.Press));

            // ----- Ultimate Settings
            subMenu = _menu.MainMenu.AddSubMenu("大招 设置");
            ProcessLink("ultSettingsEnabled", subMenu.AddLinkedBool("开 启"));
            ProcessLink("ultSettingsMode", subMenu.AddLinkedStringList("模式:", new[] { "智能 目标", "脚本 设置", "鼠标 附近", "按键(自动)", "按键(鼠标附近)" }));
            ProcessLink("ultSettingsKeyPress", subMenu.AddLinkedKeyBind("手动停止", 'T', KeyBindType.Press));

            // ----- Items
            subMenu = _menu.MainMenu.AddSubMenu("物 品");
            ProcessLink("itemsOrb", subMenu.AddLinkedBool("使用蓝色小灯泡"));

            // ----- Misc
            subMenu = _menu.MainMenu.AddSubMenu("杂 项");
            ProcessLink("miscGapcloseE", subMenu.AddLinkedBool("使用 E 丨阻止突进"));
            ProcessLink("miscInterruptE", subMenu.AddLinkedBool("使用E 丨打断危险法术"));
            //ProcessLink("miscAlerter", subMenu.AddLinkedBool("R可击杀(本地提示)"));

            // ----- Single Spell Casting
            subMenu = _menu.MainMenu.AddSubMenu("单一法术目标");
            ProcessLink("castEnabled", subMenu.AddLinkedBool("开 启"));
            ProcessLink("castW", subMenu.AddLinkedKeyBind("扔 W", 'A', KeyBindType.Press));
            ProcessLink("castE", subMenu.AddLinkedKeyBind("扔 E", 'S', KeyBindType.Press));

            // ----- Drawings
            subMenu = _menu.MainMenu.AddSubMenu("范 围");
            ProcessLink("drawRangeQ", subMenu.AddLinkedCircle("Q 范围", true, Color.FromArgb(150, Color.IndianRed), SpellManager.Q.Range));
            ProcessLink("drawRangeW", subMenu.AddLinkedCircle("W 范围", true, Color.FromArgb(150, Color.PaleVioletRed), SpellManager.W.Range));
            ProcessLink("drawRangeE", subMenu.AddLinkedCircle("E 范围", true, Color.FromArgb(150, Color.IndianRed), SpellManager.E.Range));
            ProcessLink("drawRangeR", subMenu.AddLinkedCircle("R 范围", true, Color.FromArgb(150, Color.DarkRed), SpellManager.R.Range));
        }
    }
}
