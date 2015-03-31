using System.Drawing;
using LeagueSharp;
using LeagueSharp.Common;

namespace UniversalMinimapHack
{
    public class Menu : LeagueSharp.Common.Menu
    {
        private readonly MenuItem _iconOpacity;
        private readonly MenuItem _slider;
        private readonly MenuItem _ssCircle;
        private readonly MenuItem _ssCircleColor;
        private readonly MenuItem _ssCircleSize;
        private readonly MenuItem _ssFallbackPing;
        private readonly MenuItem _ssTimerEnabler;
        private readonly MenuItem _ssTimerMin;
        private readonly MenuItem _ssTimerMinPing;
        private readonly MenuItem _ssTimerOffset;
        private readonly MenuItem _ssTimerSize;

        public Menu() : base("花边-Universal消失提示", "UniversalMinimapHack", true)
        {
            _slider = new MenuItem("scale", "图标大小 % (F5重载)").SetValue(new Slider(20));
            _iconOpacity = new MenuItem("opacity", "图标透明度 % (F5重载)").SetValue(new Slider(70));
            _ssTimerEnabler = new MenuItem("enableSS", "启用").SetValue(true);
            _ssTimerSize = new MenuItem("sizeSS", "消失文本大小(F5重载)").SetValue(new Slider(15));
            _ssTimerOffset = new MenuItem("offsetSS", "消失文本提示").SetValue(new Slider(15, -50, +50));
            _ssTimerMin = new MenuItem("minSS", "多少秒后显示").SetValue(new Slider(30, 1, 180));
            _ssTimerMinPing = new MenuItem("minPingSS", "多少秒后提示").SetValue(new Slider(30, 5, 180));
            _ssFallbackPing = new MenuItem("fallbackSS", "撤退信号提示 (本地)").SetValue(false);
            AddItem(new MenuItem("", "[自定义]"));
            AddItem(_slider);
            AddItem(_iconOpacity);
            var ssMenu = new LeagueSharp.Common.Menu("消失 时间", "ssTimer");
            ssMenu.AddItem(_ssTimerEnabler);
            ssMenu.AddItem(new MenuItem("1", "--- [额外] ---"));
            ssMenu.AddItem(_ssTimerMin);
            ssMenu.AddItem(_ssFallbackPing);
            ssMenu.AddItem(_ssTimerMinPing);
            ssMenu.AddItem(new MenuItem("2", "--- [自定义] ---"));
            ssMenu.AddItem(_ssTimerSize);
            ssMenu.AddItem(_ssTimerOffset);
            var ssCircleMenu = new LeagueSharp.Common.Menu("消失 线圈", "ccCircles");
            _ssCircle = new MenuItem("ssCircle", "启用").SetValue(true);
            _ssCircleSize = new MenuItem("ssCircleSize", "最大线圈大小").SetValue(new Slider(7000, 500, 15000));
            _ssCircleColor = new MenuItem("ssCircleColor", "线圈 颜色").SetValue(Color.Green);
            ssCircleMenu.AddItem(_ssCircle);
            ssCircleMenu.AddItem(_ssCircleSize);
            ssCircleMenu.AddItem(_ssCircleColor);
            Game.PrintChat("<font color=\"#1eff00\">Huabian婕㈠寲QQ缇わ細299606556</font> - <font color=\"#00BFFF\">姝¤繋鍚勪綅鐨勫姞鍏ワ紒</font>");
            AddSubMenu(ssMenu);
            AddSubMenu(ssCircleMenu);
            AddToMainMenu();
        }

        public float IconScale
        {
            get { return _slider.GetValue<Slider>().Value / 100f; }
        }

        public float IconOpacity
        {
            get { return _iconOpacity.GetValue<Slider>().Value / 100f; }
        }

        // ReSharper disable once InconsistentNaming
        public bool SSTimer
        {
            get { return _ssTimerEnabler.GetValue<bool>(); }
        }

        // ReSharper disable once InconsistentNaming
        public int SSTimerSize
        {
            get { return _ssTimerSize.GetValue<Slider>().Value; }
        }

        // ReSharper disable once InconsistentNaming
        public int SSTimerOffset
        {
            get { return _ssTimerOffset.GetValue<Slider>().Value; }
        }

        // ReSharper disable once InconsistentNaming
        public int SSTimerStart
        {
            get { return _ssTimerMin.GetValue<Slider>().Value; }
        }

        // ReSharper disable once InconsistentNaming
        public bool Ping
        {
            get { return _ssFallbackPing.GetValue<bool>(); }
        }

        // ReSharper disable once InconsistentNaming
        public int MinPing
        {
            get { return _ssTimerMinPing.GetValue<Slider>().Value; }
        }

        // ReSharper disable once InconsistentNaming
        public bool SSCircle
        {
            get { return _ssCircle.GetValue<bool>(); }
        }

        // ReSharper disable once InconsistentNaming
        public int SSCircleSize
        {
            get { return _ssCircleSize.GetValue<Slider>().Value; }
        }

        // ReSharper disable once InconsistentNaming
        public Color SSCircleColor
        {
            get { return _ssCircleColor.GetValue<Color>(); }
        }
    }
}