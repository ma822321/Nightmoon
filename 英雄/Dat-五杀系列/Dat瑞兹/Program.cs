/*
 * User: GoldenGates
 * Date: 29/12/2014
 * Time: 5:33 PM
 */

/*
 Features:
 -Full combo with togglable QWER
 -Slider to adjust health to Ulti at
 -Seraph activator at slider adjustable health
 -Lane clear with togglable QE with Mana Manager
 -Last hit with togglable QE (if out of AA range) with Mana Manager
 -Mixed mode (harass/last hit) with togglable QE with Mana Manager
 -Drawing Q+WE+AA ranges
 -Auto pot at adjustable health/mana
 -Auto cage under tower when possible (optional)
 
 */

using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace DatRyze {
	class Program {
		
		static Obj_AI_Hero Player { get { return ObjectManager.Player; } }
		
		static Orbwalking.Orbwalker Orbwalker;
		static Spell Q, W, E, R;
		static Items.Item Seraph;
		static Items.Item HealthPot;
		static Items.Item ManaPot;
		static Menu Menu;
		
		public static void Main(string[] args) {
			CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
		}
		
		static void Game_OnGameLoad(EventArgs args) {
			if (Player.ChampionName != "Ryze")
				return;
			Q = new Spell(SpellSlot.Q, 625);
			W = new Spell(SpellSlot.W, 600);
			E = new Spell(SpellSlot.E, 600);
			R = new Spell(SpellSlot.R);
			Seraph = new Items.Item(3040, 0);
			HealthPot = new Items.Item(2003, 0);
			ManaPot = new Items.Item(2004, 0);
			
			Menu = new Menu("花边-Dat瑞兹", Player.ChampionName, true);
			
			Menu orbwalkerMenu = Menu.AddSubMenu(new Menu("走砍", "Orbwalker"));
			Orbwalker = new Orbwalking.Orbwalker(orbwalkerMenu);
			
			Menu tsMenu = Menu.AddSubMenu(new Menu("目标选择", "TS"));
			TargetSelector.AddToMenu(tsMenu);
			
			Menu spellsMenu = Menu.AddSubMenu(new Menu("技能", "spellsMenu"));
			
			spellsMenu.AddItem(new MenuItem("towerW", "塔下自动W").SetValue(true));
			
			Menu comboMenu = spellsMenu.AddSubMenu(new Menu("连招", "comboSpells"));
            comboMenu.AddItem(new MenuItem("comboUseQ", "使用 Q").SetValue(true));
            comboMenu.AddItem(new MenuItem("comboUseW", "使用 W").SetValue(true));
            comboMenu.AddItem(new MenuItem("comboUseE", "使用 E").SetValue(true));
            comboMenu.AddItem(new MenuItem("comboUseR", "使用 R").SetValue(true));
            comboMenu.AddItem(new MenuItem("comboSliderR", "使用 R丨Hp百分比 (%)").SetValue(new Slider(100, 1, 100)));
			
			Menu laneClearMenu = spellsMenu.AddSubMenu(new Menu("清线", "laneClearSpells"));
            laneClearMenu.AddItem(new MenuItem("laneClearUseQ", "使用 Q").SetValue(true));
            laneClearMenu.AddItem(new MenuItem("laneClearUseE", "使用 E").SetValue(true));
			laneClearMenu.AddItem(new MenuItem("laneClearManaManager", "最低蓝量比 (%)").SetValue(new Slider(40, 1, 100)));
			
			Menu lastHitMenu = spellsMenu.AddSubMenu(new Menu("补刀", "lastHitSpells"));
            lastHitMenu.AddItem(new MenuItem("lastHitUseQ", "使用 Q").SetValue(false));
            lastHitMenu.AddItem(new MenuItem("lastHitUseE", "使用 E").SetValue(false));
			lastHitMenu.AddItem(new MenuItem("lastHitManaManager", "最低蓝量比 (%)").SetValue(new Slider(40, 1, 100)));
			
			Menu mixedMenu = spellsMenu.AddSubMenu(new Menu("骚扰", "mixedSpells"));
            mixedMenu.AddItem(new MenuItem("mixedUseQ", "使用 Q").SetValue(true));
            mixedMenu.AddItem(new MenuItem("mixedUseE", "使用 E").SetValue(true));
			mixedMenu.AddItem(new MenuItem("mixedManaManager", "最低蓝量比 (%)").SetValue(new Slider(40, 1, 100)));
						
			Menu itemsMenu = Menu.AddSubMenu(new Menu("物品", "items"));
            itemsMenu.AddItem(new MenuItem("useSeraphs", "使用 炽天使之拥").SetValue(true));
			itemsMenu.AddItem(new MenuItem("seraphHealth", "Hp百分比 (%)").SetValue(new Slider(25, 1, 100)));
			itemsMenu.AddItem(new MenuItem("useHealthPot", "使用 红药").SetValue(true));
            itemsMenu.AddItem(new MenuItem("healthPotHealth", "Hp百分比 (%)").SetValue(new Slider(30, 1, 100)));
            itemsMenu.AddItem(new MenuItem("useManaPot", "使用 蓝药").SetValue(true));
            itemsMenu.AddItem(new MenuItem("manaPotMana", "Mp百分比 (%)").SetValue(new Slider(30, 1, 100)));
									
			Menu drawMenu = Menu.AddSubMenu(new Menu("显示", "drawing"));
			drawMenu.AddItem(new MenuItem("drawAA", "显示 AA 范围").SetValue(true));
			drawMenu.AddItem(new MenuItem("drawQ", "显示 Q 范围").SetValue(true));
			drawMenu.AddItem(new MenuItem("drawWE", "显示 W/E 范围").SetValue(true));

            Menu msgMenu = Menu.AddSubMenu(new Menu("显示", "message"));
            msgMenu.AddItem(new MenuItem("Sprite", "Dat瑞兹"));
            msgMenu.AddItem(new MenuItem("Hanhua", "汉化:花边下丶情未央"));
            msgMenu.AddItem(new MenuItem("qqqun", "QQ群:299606556"));

			Menu.AddToMainMenu();
			Drawing.OnDraw += Drawing_OnDraw;
			Game.OnUpdate += Game_OnGameUpdate;
			//Game.OnGameProcessPacket += Game_OnGameProcessPacket;
			Game.PrintChat("<font color ='#33FFFF'>Dat Ryze</font> by GoldenGates loaded. Enjoy!");

            Game.PrintChat("<font color=\"#1eff00\">Huabian婕㈠寲QQ缇わ細299606556</font> - <font color=\"#00BFFF\">姝¤繋鍚勪綅鐨勫姞鍏ワ紒</font>");

		}
		
		static void Game_OnGameUpdate(EventArgs args) {
			if (Player.IsDead)
				return;
			Checks();
			float manaPercentage = (Player.Mana / Player.MaxMana) * 100;
			float healthPercentage = (Player.Health / Player.MaxHealth) * 100;
			switch (Orbwalker.ActiveMode) {
				case Orbwalking.OrbwalkingMode.Combo:
					int comboRHealth = Menu.Item("comboSliderR").GetValue<Slider>().Value;
					if (Menu.Item("comboUseQ").GetValue<bool>())
						useQ(false, false);
					if (Menu.Item("comboUseW").GetValue<bool>())
						useW();
					if (Menu.Item("comboUseE").GetValue<bool>())
						useE(false, false);
					if (Menu.Item("comboUseR").GetValue<bool>() && R.IsReady() && healthPercentage < comboRHealth)
						R.Cast();		
					break;
				case Orbwalking.OrbwalkingMode.LaneClear:
					int laneClearMana = Menu.Item("laneClearManaManager").GetValue<Slider>().Value;
					if (Menu.Item("laneClearUseQ").GetValue<bool>() && manaPercentage > laneClearMana)
						useQ(true, true);
					if (Menu.Item("laneClearUseE").GetValue<bool>() && manaPercentage > laneClearMana)
						useE(true, true);
					break;
				case Orbwalking.OrbwalkingMode.LastHit:
					int lastHitMana = Menu.Item("lastHitManaManager").GetValue<Slider>().Value;
					if (Menu.Item("lastHitUseQ").GetValue<bool>() && manaPercentage > lastHitMana)
						useQ(true, false);
					if (Menu.Item("lastHitUseE").GetValue<bool>() && manaPercentage > lastHitMana)
						useE(true, false);
					break;
				case Orbwalking.OrbwalkingMode.Mixed:
					int mixedMana = Menu.Item("mixedManaManager").GetValue<Slider>().Value;
					if (Menu.Item("mixedUseQ").GetValue<bool>() && manaPercentage > mixedMana)
						useQ(false, false);
					if (Menu.Item("mixedUseE").GetValue<bool>() && manaPercentage > mixedMana)
						useE(false, false);
					break;
				
			}
			
		}
		
		static void Drawing_OnDraw(EventArgs args) {
			if (Menu.Item("drawAA").GetValue<bool>())
				Render.Circle.DrawCircle(Player.Position, 550, Color.Blue);
			if (Menu.Item("drawQ").GetValue<bool>())
				Render.Circle.DrawCircle(Player.Position, 625, Color.Orange);
			if (Menu.Item("drawWE").GetValue<bool>())
				Render.Circle.DrawCircle(Player.Position, 600, Color.HotPink);
		}
		
		static void Checks() {
			float healthPercentage = (Player.Health / Player.MaxHealth) * 100;
			float manaPercentage = (Player.Mana / Player.MaxMana) * 100;
			if (Menu.Item("useSeraphs").GetValue<bool>() && Items.HasItem(Seraph.Id) && Seraph.IsReady() && !Player.InFountain() && healthPercentage < Menu.Item("seraphHealth").GetValue<Slider>().Value && Player.CountEnemiesInRange(600) > 0)
				Seraph.Cast();			
			if (Menu.Item("useHealthPot").GetValue<bool>() && Items.HasItem(HealthPot.Id) && HealthPot.IsReady() && !Player.InFountain() && !Player.HasBuff("RegenerationPotion", true) && healthPercentage < Menu.Item("healthPotHealth").GetValue<Slider>().Value)
				HealthPot.Cast();
			if (Menu.Item("useManaPot").GetValue<bool>() && Items.HasItem(ManaPot.Id) && ManaPot.IsReady() && !Player.InFountain() && manaPercentage < Menu.Item("manaPotMana").GetValue<Slider>().Value)
				ManaPot.Cast();					
		}
				
		static void useQ(bool onMinion, bool laneClear) {
			if (!Q.IsReady())
				return;
			if (onMinion) {
				Obj_AI_Base minion = MinionManager.GetMinions(Player.Position, 625).FirstOrDefault();
				if (minion != null && laneClear && minion.IsValidTarget())
					Q.CastOnUnit(minion);
				else if (minion != null && Player.Distance(minion) > 550 && minion.IsValidTarget())
					Q.CastOnUnit(minion);
			} else {
				Obj_AI_Hero target = TargetSelector.GetTarget(625, TargetSelector.DamageType.Magical);
				if (target != null && target.IsValidTarget())
					Q.CastOnUnit(target);
			}
		}
		
		static void useW() {
			if (!W.IsReady())
				return;
			Obj_AI_Hero target = TargetSelector.GetTarget(600, TargetSelector.DamageType.Magical);
			if (target != null && target.IsValidTarget())
				W.CastOnUnit(target);
			
		}
		
		static void useE(bool onMinion, bool laneClear) {
			if (!E.IsReady())
				return;
			if (onMinion) {
				Obj_AI_Base minion = MinionManager.GetMinions(Player.Position, 600).FirstOrDefault();
				if (minion != null && laneClear && minion.IsValidTarget())
					E.CastOnUnit(minion);
				else if (minion != null && Player.Distance(minion) > 550 && minion.IsValidTarget())
					E.CastOnUnit(minion);
			} else {
				Obj_AI_Hero target = TargetSelector.GetTarget(600, TargetSelector.DamageType.Magical);
				if (target != null && target.IsValidTarget())
					E.CastOnUnit(target);
			}
		}
		
		
		//Credits to FluxySenpai
	/*	static void Game_OnGameProcessPacket(GamePacketEventArgs args) {
			if (args.PacketData[0] == Network.Packets.S2C.TowerAggro.Header && Menu.Item("towerW").GetValue<bool>()) {
				var p = Packet.S2C.TowerAggro.Decoded(args.PacketData);
				var target = ObjectManager.GetUnitByNetworkId<Obj_AI_Hero>(p.TargetNetworkId);
				if (target != null && target.IsValidTarget() && Player.Distance(target) <= W.Range) {
					//Game.PrintChat("Target Found Under Tower, Auto Caging");
					W.CastOnUnit(target);
				}
			}
		}*/
	} 
}

//http://pastie.org/9804616