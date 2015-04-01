using System;
using LeagueSharp.Common;
using System.Linq;
using Color = System.Drawing.Color;

namespace PentakillZed {

	public class MenuHelper {
		
		public Menu menu;
		
		public MenuHelper() {
			menu = new Menu("花边-五杀zed", "PentakillZed", true);
			Menu tsMenu = menu.AddSubMenu(new Menu("目标 选择", "TS"));
			TargetSelector.AddToMenu(tsMenu);
			AddCombo();
			AddDrawing();
			AddHarass();
			//AddItems();
			AddLaneClear();
			AddLastHit();
		}
		
		public Menu getOrbwalkerMenu() {
			return menu.AddSubMenu(new Menu("走砍", "Orbwalker"));
		}
		
		public void AddCombo() {
			Menu comboMenu = menu.AddSubMenu(new Menu("连招", "Combo"));
            comboMenu.AddItem(new MenuItem("comboUseQ", "使用 Q").SetValue(true));
            comboMenu.AddItem(new MenuItem("comboUseW", "使用 W").SetValue(true));
            comboMenu.AddItem(new MenuItem("comboUseE", "使用 E").SetValue(true));
            comboMenu.AddItem(new MenuItem("comboUseR", "使用 R").SetValue(true));
            comboMenu.AddItem(new MenuItem("comboIgn", "使用 点燃丨目标可击杀").SetValue(true));
			comboMenu.AddItem(new MenuItem("comboLine", "线性连招 (需要与连招按键区分开)").SetValue(new KeyBind("X".ToCharArray()[0], KeyBindType.Press)));
		}
		
		public void AddDrawing() {			
			Menu drawingMenu = menu.AddSubMenu(new Menu("显示", "drawing"));
			drawingMenu.AddItem(new MenuItem("drawQ", "显示 Q 范围").SetValue(true));
			drawingMenu.AddItem(new MenuItem("drawW", "显示 W 范围").SetValue(true));
			drawingMenu.AddItem(new MenuItem("drawE", "显示 E 范围").SetValue(true));
			drawingMenu.AddItem(new MenuItem("drawR", "显示 R 范围").SetValue(true));
			drawingMenu.AddItem(new MenuItem("drawDmg", "显示 连招 伤害").SetValue(true));
			drawingMenu.AddItem(new MenuItem("drawShadow", "显示 影子 位置").SetValue(true));
		}
		
		public void AddHarass() {
			Menu harassMenu = menu.AddSubMenu(new Menu("骚扰", "harass"));
            harassMenu.AddItem(new MenuItem("harassUseQ", "使用 Q").SetValue(true));
            harassMenu.AddItem(new MenuItem("harassUseW", "使用 W").SetValue(true));
            harassMenu.AddItem(new MenuItem("harassUseE", "使用 E").SetValue(true));
		}
		
		public void AddItems() {
			Menu itemMenu = menu.AddSubMenu(new Menu("物品", "items"));
			itemMenu.AddItem(new MenuItem("temp", "没有使用"));
		}
		
		public void AddLaneClear() {
			Menu lcMenu = menu.AddSubMenu(new Menu("清线", "laneClear"));
            lcMenu.AddItem(new MenuItem("lcUseQ", "使用 Q").SetValue(true));
            lcMenu.AddItem(new MenuItem("lcUseE", "使用 E").SetValue(true));
			lcMenu.AddItem(new MenuItem("lcEnergyManager", "最低查克拉 (%)").SetValue(new Slider(30, 1, 100)));			
		}
		
		public void AddLastHit() {
			Menu lastHitMenu = menu.AddSubMenu(new Menu("补刀", "lastHit"));
            lastHitMenu.AddItem(new MenuItem("lastHitUseQ", "使用 Q").SetValue(true));			
		}
	}
}
