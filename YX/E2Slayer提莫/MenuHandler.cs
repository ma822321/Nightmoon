#region

using LeagueSharp;
using LeagueSharp.Common;

using Color = System.Drawing.Color;


#endregion

namespace Teemo___The_Satan_Yordle
{
    public class MenuHandler
    {


        private static Obj_AI_Hero Player = ObjectManager.Player;
        private static readonly Menu Menu = Program.Menu;

        private static Menu _combo;
        private static MenuItem _comboq, _combow, _combor, _comboActive;

        private static Menu _harass;
        private static MenuItem _harassq, _harassw, _harassr, _harassMana, _harassActive, _harassToggle;

        private static Menu _lineclear;
        private static MenuItem _lineclearq, _lineclearw, _lineclearr, _lineclearActive;

        private static Menu _rsetting;
        private static MenuItem _shroomh, _shroomm, _shroomType;

        private static Menu _misc;
        private static MenuItem _packet, _gabcloser;

        private static Menu _drawing;
        private static MenuItem _drwaingq, _drwaingr, _drwaingshroomh, _drwaingshroomm, _drwaingshroomV;

        
        public static void MenuHandlerRun()
        {



      
            _combo = (new Menu("连 招", "Combo"));
            _comboq = (new MenuItem("UseQ", "使用 Q").SetValue(true));
            _combow = (new MenuItem("UseW", "使用 W").SetValue(true));
            _combor = (new MenuItem("UseR", "使用 R").SetValue(true));
            _comboActive = (new MenuItem("ComboActive", "连招 按键").SetValue(new KeyBind(32, KeyBindType.Press)));

            _harass = (new Menu("骚 扰", "Harass"));
            _harassq = (new MenuItem("UseQH", "使用 Q").SetValue(true));
            _harassw = (new MenuItem("UseWH", "使用 W").SetValue(false));
            _harassr = (new MenuItem("UseRH", "使用 R").SetValue(true));
            _harassMana = (new MenuItem("HarassMana", "骚扰最低蓝量比").SetValue(new Slider(0, 0, 100)));
            _harassActive = (new MenuItem("HarassActive", "骚扰 (按键)").SetValue(new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));
            _harassToggle = (new MenuItem("HarassActiveT", "骚扰 (自动)").SetValue(new KeyBind("Y".ToCharArray()[0], KeyBindType.Toggle, false)));

            _lineclear = (new Menu("清 线", "LaneClear"));
            _lineclearq = (new MenuItem("UseQL", "使用 Q").SetValue(false));
            _lineclearw = (new MenuItem("UseWL", "使用 W").SetValue(false));
            _lineclearr = (new MenuItem("UseRL", "使用 R").SetValue(false));
            _lineclearActive = (new MenuItem("LaneClearA", "清线 按键").SetValue(new KeyBind("X".ToCharArray()[0], KeyBindType.Press)));

            _rsetting = (new Menu("R 设置", "Rsettings"));
            _shroomh = (new MenuItem("ShroomH", "自动 使用 R (高优先级)").SetValue(true));
            _shroomm = (new MenuItem("ShroomM", "自动 使用 R (中优先级)").SetValue(true));
            _shroomType = (new MenuItem("ShroomOn", "自动 使用 R").SetValue(new StringList(new[] { "一直", "连招时", "禁止" }, 0)));

            _misc = (new Menu("杂 项", "Misc"));
            _packet = (new MenuItem("Packets", "使用 封包").SetValue(false));
            _gabcloser = (new MenuItem("GapQ", "使用 Q 丨被突进").SetValue(true));

            _drawing = new Menu("范 围", "Drawing");
            _drwaingq = new MenuItem("DrawQ", "Q 范围").SetValue(new Circle(false, Color.Lime));
            _drwaingr = new MenuItem("DrawR", "R 范围").SetValue(new Circle(false, Color.Azure));
            _drwaingshroomh = new MenuItem("ShroomH1", "蘑菇(高优先级)").SetValue(true);
            _drwaingshroomm = new MenuItem("ShroomM1", "蘑菇(中优先级)").SetValue(true);
            _drwaingshroomV = new MenuItem("ShroomV", "蘑菇 视觉范围").SetValue(new Slider(1500, 4000, 1000));

           
            //Combo
            Menu.AddSubMenu(_combo);
            Menu.SubMenu("Combo").AddItem(_comboq);
            Menu.SubMenu("Combo").AddItem(_combow);
            Menu.SubMenu("Combo").AddItem(_combor);
            Menu.SubMenu("Combo").AddItem(_comboActive);

            
            //Harass
            Menu.AddSubMenu(_harass);
            Menu.SubMenu("Harass").AddItem(_harassq);
            Menu.SubMenu("Harass").AddItem(_harassw);
            Menu.SubMenu("Harass").AddItem(_harassr);
            Menu.SubMenu("Harass").AddItem(_harassMana);
            Menu.SubMenu("Harass").AddItem(_harassActive);
            Menu.SubMenu("Harass").AddItem(_harassToggle);
              
            Menu.AddSubMenu(_lineclear);
            Menu.SubMenu("LaneClear").AddItem(_lineclearq);
            Menu.SubMenu("LaneClear").AddItem(_lineclearActive);

            
            Menu.AddSubMenu(new Menu("物 品", "Items"));
            Menu.SubMenu("Items").AddItem(new MenuItem("DFG1", "冥火").SetValue(true));
            Menu.SubMenu("Items").AddItem(new MenuItem("Cutlass1", "水银刀").SetValue(true));
            Menu.SubMenu("Items").AddItem(new MenuItem("Hextech1", "科技枪").SetValue(true));
            Menu.SubMenu("Items").AddItem(new MenuItem("Frostclaim1", "冰霜女王的指令").SetValue(true));
            Menu.SubMenu("Items").AddItem(new MenuItem("Botrk1", "破败").SetValue(true));
            Menu.SubMenu("Items").AddItem(new MenuItem("Youmuus1", "幽梦").SetValue(true));
           
            

            //R settings
            Menu.AddSubMenu(_rsetting);
            Menu.SubMenu("Rsettings").AddItem(_shroomh);
            Menu.SubMenu("Rsettings").AddItem(_shroomm);
            Menu.SubMenu("Rsettings").AddItem(_shroomType);


            //Misc
            Menu.AddSubMenu(_misc);
               Menu.SubMenu("Misc").AddItem(_packet);
              Menu.SubMenu("Misc").AddItem(_gabcloser);


              //Drawings
              Menu.AddSubMenu(_drawing);
              Menu.SubMenu("Drawing").AddItem(_drwaingq);
              Menu.SubMenu("Drawing").AddItem(_drwaingr);

              Menu.SubMenu("Drawing").AddItem(_drwaingshroomh);
              Menu.SubMenu("Drawing").AddItem(_drwaingshroomm);
              Menu.SubMenu("Drawing").AddItem(_drwaingshroomV);


           


        }
    }
}
