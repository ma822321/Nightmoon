using LeagueSharp.Common;

namespace Avoid
{
    public class Config
    {
        public static MenuWrapper Menu { get; private set; }

        private static MenuWrapper.BoolLink drawRangesLink;
        public static bool DrawRanges
        {
            get { return drawRangesLink.Value; }
        }

        private static MenuWrapper.KeyBindLink disableKeyLink;
        public static bool Enabled
        {
            get { return !disableKeyLink.Value.Active; }
        }

        static Config()
        {
            Menu = new MenuWrapper("花边-H7反隐形陷阱", false, false);

            var subMenu = Menu.MainMenu.AddSubMenu("避免 陷阱");
            foreach (var obj in ObjectDatabase.AvoidObjects)
            {
                obj.MenuState = subMenu.AddLinkedBool(obj.DisplayName);
            }

            subMenu = Menu.MainMenu.AddSubMenu("显示");
            drawRangesLink = subMenu.AddLinkedBool("显示 陷阱 范围");

            disableKeyLink = Menu.MainMenu.AddLinkedKeyBind("紧急情况无法避免踩陷阱按键", 'A', KeyBindType.Press);
        }
    }
}
