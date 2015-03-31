using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LeagueSharp;
using LeagueSharp.Common;

using Color = System.Drawing.Color;

namespace Veigar
{
    public class Config
    {
        public const string MENU_NAME = "花边-H7小法";
        private static MenuWrapper _menu;

        private static Dictionary<string, MenuWrapper.BoolLink> _boolLinks = new Dictionary<string, MenuWrapper.BoolLink>();
        private static Dictionary<string, MenuWrapper.CircleLink> _circleLinks = new Dictionary<string, MenuWrapper.CircleLink>();
        private static Dictionary<string, MenuWrapper.KeyBindLink> _keyLinks = new Dictionary<string, MenuWrapper.KeyBindLink>();
        private static Dictionary<string, MenuWrapper.SliderLink> _sliderLinks = new Dictionary<string, MenuWrapper.SliderLink>();

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
        }

        public static void Initialize()
        {
            // Create menu
            _menu = new MenuWrapper(MENU_NAME);

            // Combo
            var subMenu = _menu.MainMenu.AddSubMenu("连招");
            ProcessLink("comboUseQ", subMenu.AddLinkedBool("使用 Q"));
            ProcessLink("comboUseE", subMenu.AddLinkedBool("使用 E"));
            ProcessLink("comboUseR", subMenu.AddLinkedBool("使用 R"));
            ProcessLink("comboUseItems", subMenu.AddLinkedBool("使用 物品"));
            ProcessLink("comboUseIgnite", subMenu.AddLinkedBool("使用 点燃"));
            ProcessLink("comboActive", subMenu.AddLinkedKeyBind("连招按键", 32, KeyBindType.Press));

            // Harass
            subMenu = _menu.MainMenu.AddSubMenu("骚扰");
            ProcessLink("harassUseQ", subMenu.AddLinkedBool("使用 Q"));
            ProcessLink("harassUseW", subMenu.AddLinkedBool("使用 W"));
            ProcessLink("harassMana", subMenu.AddLinkedSlider("最低蓝量比(%)", 30));
            ProcessLink("harassActive", subMenu.AddLinkedKeyBind("骚扰按键", 'C', KeyBindType.Press));

            // WaveClear
            subMenu = _menu.MainMenu.AddSubMenu("清线");
            ProcessLink("waveUseQ", subMenu.AddLinkedBool("使用 Q (堆叠)"));
            ProcessLink("waveUseW", subMenu.AddLinkedBool("使用 W"));
            ProcessLink("waveNumW", subMenu.AddLinkedSlider("W 命中", 3, 1, 10));
            ProcessLink("waveMana", subMenu.AddLinkedSlider("最低蓝量比(%)", 30));
            ProcessLink("waveActive", subMenu.AddLinkedKeyBind("清线按键", 'V', KeyBindType.Press));

            // JungleClear
            subMenu = _menu.MainMenu.AddSubMenu("清野");
            ProcessLink("jungleUseQ", subMenu.AddLinkedBool("使用 Q"));
            ProcessLink("jungleUseW", subMenu.AddLinkedBool("使用 W"));
            ProcessLink("jungleActive", subMenu.AddLinkedKeyBind("清野按键", 'V', KeyBindType.Press));

            // Flee
            subMenu = _menu.MainMenu.AddSubMenu("逃跑");
            ProcessLink("fleeUseE", subMenu.AddLinkedBool("使用 E"));
            ProcessLink("fleeActive", subMenu.AddLinkedKeyBind("逃跑按键", 'T', KeyBindType.Press));

            // Items
            subMenu = _menu.MainMenu.AddSubMenu("物品");
            ProcessLink("itemsDfg", subMenu.AddLinkedBool("使用冥火"));

            // Misc
            subMenu = _menu.MainMenu.AddSubMenu("杂项");
            ProcessLink("miscFarmQActive", subMenu.AddLinkedKeyBind("打钱Q堆叠按键", 'A', KeyBindType.Toggle, true));
            ProcessLink("miscFarmQ", subMenu.AddLinkedKeyBind("打钱禁止Q堆叠", 32, KeyBindType.Press));
            ProcessLink("miscStunW", subMenu.AddLinkedBool("目标被眩晕自动W"));

            // Drawings
            subMenu = _menu.MainMenu.AddSubMenu("显示");
            ProcessLink("drawRangeQ", subMenu.AddLinkedCircle("Q 范围", true, Color.FromArgb(150, Color.IndianRed), SpellManager.Q.Range));
            ProcessLink("drawRangeW", subMenu.AddLinkedCircle("W 范围", true, Color.FromArgb(150, Color.Azure), SpellManager.W.Range));
            ProcessLink("drawRangeE", subMenu.AddLinkedCircle("E 范围", false, Color.FromArgb(150, Color.IndianRed), SpellManager.E.Range));
            ProcessLink("drawRangeR", subMenu.AddLinkedCircle("R 范围", false, Color.FromArgb(150, Color.Azure), SpellManager.R.Range));
        }
    }
}
