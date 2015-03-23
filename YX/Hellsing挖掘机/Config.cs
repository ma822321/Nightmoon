using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LeagueSharp;
using LeagueSharp.Common;

using Color = System.Drawing.Color;

namespace Rekt_Sai
{
    public class Config
    {
        public const string MENU_NAME = "花边汉化-Hellsing挖掘机";
        public static MenuWrapper Menu { get; private set; }

        public static Dictionary<string, MenuWrapper.BoolLink> BoolLinks { get; private set; }
        public static Dictionary<string, MenuWrapper.CircleLink> CircleLinks { get; private set; }
        public static Dictionary<string, MenuWrapper.KeyBindLink> KeyLinks { get; private set; }
        public static Dictionary<string, MenuWrapper.SliderLink> SliderLinks { get; private set; }
        public static Dictionary<string, MenuWrapper.StringListLink> StringListLinks { get; private set; }

        private static void ProcessLink(string key, object value)
        {
            if (value is MenuWrapper.BoolLink)
                BoolLinks.Add(key, value as MenuWrapper.BoolLink);
            else if (value is MenuWrapper.CircleLink)
                CircleLinks.Add(key, value as MenuWrapper.CircleLink);
            else if (value is MenuWrapper.KeyBindLink)
                KeyLinks.Add(key, value as MenuWrapper.KeyBindLink);
            else if (value is MenuWrapper.SliderLink)
                SliderLinks.Add(key, value as MenuWrapper.SliderLink);
            else if (value is MenuWrapper.StringListLink)
                StringListLinks.Add(key, value as MenuWrapper.StringListLink);
        }

        static Config()
        {
            Menu = new MenuWrapper(MENU_NAME);

            BoolLinks = new Dictionary<string, MenuWrapper.BoolLink>();
            CircleLinks = new Dictionary<string, MenuWrapper.CircleLink>();
            KeyLinks = new Dictionary<string, MenuWrapper.KeyBindLink>();
            SliderLinks = new Dictionary<string, MenuWrapper.SliderLink>();
            StringListLinks = new Dictionary<string, MenuWrapper.StringListLink>();

            SetupMenu();
        }

        private static void SetupMenu()
        {
            // ----- Combo
            var subMenu = Menu.MainMenu.AddSubMenu("连 招");
            ProcessLink("comboUseQ", subMenu.AddLinkedBool("使用 Q"));
            ProcessLink("comboUseW", subMenu.AddLinkedBool("使用 W"));
            ProcessLink("comboUseE", subMenu.AddLinkedBool("使用 E"));
            ProcessLink("comboUseQBurrow", subMenu.AddLinkedBool("使用 Q (潜地)"));
            ProcessLink("comboUseEBurrow", subMenu.AddLinkedBool("使用 E (潜地)"));
            ProcessLink("comboUseItems", subMenu.AddLinkedBool("使用 物品"));
            ProcessLink("comboUseIgnite", subMenu.AddLinkedBool("使用 点燃"));
            ProcessLink("comboUseSmite", subMenu.AddLinkedBool("使用 惩戒 (CD好了)"));
            ProcessLink("comboActive", subMenu.AddLinkedKeyBind("按 键", 32, KeyBindType.Press));

            // ----- Harass
            subMenu = Menu.MainMenu.AddSubMenu("骚 扰");
            ProcessLink("harassUseQ", subMenu.AddLinkedBool("使用 Q"));
            ProcessLink("harassUseE", subMenu.AddLinkedBool("使用 E"));
            ProcessLink("harassUseQBurrow", subMenu.AddLinkedBool("使用 Q (潜地)"));
            ProcessLink("harassUseItems", subMenu.AddLinkedBool("使用 物品"));
            ProcessLink("harassActive", subMenu.AddLinkedKeyBind("按 键", 'C', KeyBindType.Press));

            // ----- WaveClear
            subMenu = Menu.MainMenu.AddSubMenu("清 线");
            ProcessLink("waveUseQ", subMenu.AddLinkedBool("使用 Q"));
            ProcessLink("waveNumQ", subMenu.AddLinkedSlider("使用Q丨附近小兵", 2, 1, 10));
            ProcessLink("waveUseE", subMenu.AddLinkedBool("使用 E"));
            ProcessLink("waveUseQBurrow", subMenu.AddLinkedBool("使用 Q (潜地)"));
            ProcessLink("waveUseItems", subMenu.AddLinkedBool("使用 items"));
            ProcessLink("waveActive", subMenu.AddLinkedKeyBind("按 键", 'V', KeyBindType.Press));

            // ----- JungleClear
            subMenu = Menu.MainMenu.AddSubMenu("清 野");
            ProcessLink("jungleUseQ", subMenu.AddLinkedBool("使用 Q"));
            ProcessLink("jungleUseW", subMenu.AddLinkedBool("使用 W"));
            ProcessLink("jungleUseE", subMenu.AddLinkedBool("使用 E"));
            ProcessLink("jungleUseQBurrow", subMenu.AddLinkedBool("使用 Q (潜地)"));
            ProcessLink("jungleUseItems", subMenu.AddLinkedBool("使用 物品"));
            ProcessLink("jungleActive", subMenu.AddLinkedKeyBind("按 键", 'V', KeyBindType.Press));

            // ----- Flee
            subMenu = Menu.MainMenu.AddSubMenu("逃 跑");
            ProcessLink("fleeNothing", subMenu.AddLinkedBool("技能CD"));
            ProcessLink("fleeActive", subMenu.AddLinkedKeyBind("按 键", 'T', KeyBindType.Press));

            // ----- Items
            subMenu = Menu.MainMenu.AddSubMenu("物 品");
            ProcessLink("itemsTiamat", subMenu.AddLinkedBool("使用 提亚马特"));
            ProcessLink("itemsHydra", subMenu.AddLinkedBool("使用 九头蛇"));
            ProcessLink("itemsCutlass", subMenu.AddLinkedBool("使用 水银刀"));
            ProcessLink("itemsBotrk", subMenu.AddLinkedBool("使用 破败"));
            ProcessLink("itemsRanduin", subMenu.AddLinkedBool("使用 兰顿"));

            // ----- Drawings
            subMenu = Menu.MainMenu.AddSubMenu("范 围");
            ProcessLink("drawRangeQ", subMenu.AddLinkedCircle("Q 范围 (潜地)", true, Color.FromArgb(150, Color.IndianRed), SpellManager.QBurrowed.Range));
            ProcessLink("drawRangeE", subMenu.AddLinkedCircle("E 范围 (潜地)", true, Color.FromArgb(150, Color.Azure), SpellManager.EBurrowed.Range));
        }
    }
}
