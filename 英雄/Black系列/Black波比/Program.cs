﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

namespace BlackPoppy
{
    internal class Program
    {
        // Generic
        public static readonly string ChampName = "Poppy";
        private static readonly Obj_AI_Hero Player = ObjectManager.Player;
        // Spells
        private static readonly List<Spell> SpellList = new List<Spell>();
        private static Spell _q, _w, _e, _r;
        private static SpellSlot _igniteSlot;
        // Menu
        public static Menu Menu;
        private static Orbwalking.Orbwalker _ow;

        public static void Main(string[] args)
        {
            // Register events
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            //Champ validation
            if (Player.ChampionName != ChampName)
            {
                return;
            }

            //Define spells
            _q = new Spell(SpellSlot.Q, 250f);
            _w = new Spell(SpellSlot.W, 250f);
            _e = new Spell(SpellSlot.E, 500f);
            _r = new Spell(SpellSlot.R, 850f);
            SpellList.AddRange(new[] {_q, _w, _e, _r});

            _igniteSlot = Player.GetSpellSlot("SummonerDot");

            // Finetune spells
            _e.SetTargetted(0.5f, 1450f);

            // Create menu
            CreateMenu();

            // Register events
            Game.OnUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;

            // Print
            Game.PrintChat(
                String.Format("<font color='#08F5F8'>blacky -</font> <font color='#FFFFFF'>{0} Loaded!</font>",
                    ChampName));
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            // Spell ranges
            foreach (var spell in SpellList)
            {
                // Regular spell ranges
                var circleEntry = Menu.Item("drawRange" + spell.Slot).GetValue<Circle>();
                if (circleEntry.Active)
                {
                    Render.Circle.DrawCircle(Player.Position, spell.Range, circleEntry.Color);
                }
            }
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            var target = TargetSelector.GetTarget(_r.Range, TargetSelector.DamageType.Magical);

            // Combo
            if (Menu.SubMenu("combo").Item("comboActive").GetValue<KeyBind>().Active)
            {
                OnCombo(target);
            }

            // Harass
            if (Menu.SubMenu("harass").Item("harassActive").GetValue<KeyBind>().Active &&
                (ObjectManager.Player.Mana/ObjectManager.Player.MaxMana*100) >
                Menu.Item("harassMana").GetValue<Slider>().Value)
            {
                OnHarass(target);
            }

            // WaveClear
            if (Menu.SubMenu("waveclear").Item("wcActive").GetValue<KeyBind>().Active &&
                (Player.Mana/Player.MaxMana*100) >
                Menu.Item("wcMana").GetValue<Slider>().Value)
            {
                WaveClear();
            }

            // Misc
            if (Menu.SubMenu("misc").Item("miscEscapeToMouse").GetValue<KeyBind>().Active)
            {
                EscapeToMouse();
            }

            // Killsteal
            Killsteal(target);
        }

        private static void OnCombo(Obj_AI_Hero target)
        {
            var comboMenu = Menu.SubMenu("combo");
            var useQ = comboMenu.Item("comboUseQ").GetValue<bool>() && _q.IsReady();
            var useW = comboMenu.Item("comboUseW").GetValue<bool>() && _w.IsReady();
            var useE = comboMenu.Item("comboUseE").GetValue<bool>() && _e.IsReady();
            var useR = comboMenu.Item("comboUseR").GetValue<bool>() && _r.IsReady();

            if (target.HasBuffOfType(BuffType.Invulnerability))
            {
                return;
            }

            if (useR && Player.Distance(target.Position) < _r.Range &&
                ObjectManager.Get<Obj_AI_Hero>().Count(hero => hero.IsValidTarget(_r.Range)) >=
                Menu.Item("comboLogicR").GetValue<Slider>().Value && Menu.Item("DontUlt" + target.BaseSkinName) != null &&
                Menu.Item("DontUlt" + target.BaseSkinName).GetValue<bool>() == false)
            {
                UltLogic();
            }

            if (useE && Player.Distance(target.Position) < _e.Range)
            {
                if (useW)
                {
                    _w.Cast(Player, Packets());
                }
                ELogic();
            }

            if (useQ && Player.Distance(target.Position) < _q.Range)
            {
                _q.Cast(Player, Packets());
            }

            if (!Menu.Item("miscIgnite").GetValue<bool>() || _igniteSlot == SpellSlot.Unknown ||
                Player.Spellbook.CanUseSpell(_igniteSlot) != SpellState.Ready)
            {
                return;
            }

            if (GetComboDamage(target) > target.Health)
            {
                Player.Spellbook.CastSpell(_igniteSlot, target);
            }
        }

        private static void OnHarass(Obj_AI_Hero target)
        {
            var harassMenu = Menu.SubMenu("harass");
            var useQ = harassMenu.Item("harassUseQ").GetValue<bool>() && _q.IsReady();

            if (useQ && Player.Distance(target.Position) < _q.Range)
            {
                _q.Cast(Player, Packets());
            }
        }

        private static void EscapeToMouse()
        {
            var miscMenu = Menu.SubMenu("misc");
            var useW = miscMenu.Item("miscUseW").GetValue<bool>() && _w.IsReady();

            if (useW)
            {
                Orbwalking.Orbwalk(null, Game.CursorPos);
                _w.Cast(Player, Packets());
            }
            else
            {
                Orbwalking.Orbwalk(null, Game.CursorPos);
            }
        }

        private static void UltLogic()
        {
            Obj_AI_Hero newtarget = null;
            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsValidTarget(_r.Range)))
            {
                if (newtarget == null)
                {
                    newtarget = hero;
                }
                else
                {
                    if (hero.Health > newtarget.Health && hero.BaseAttackDamage < newtarget.BaseAttackDamage)
                    {
                        newtarget = hero;
                    }
                }
            }
            _r.Cast(newtarget, Packets());
        }

        private static void ELogic()
        {
            foreach (
                var hero in from hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsValidTarget(_e.Range))
                    let prediction = _e.GetPrediction(hero)
                    where NavMesh.GetCollisionFlags(
                        prediction.UnitPosition.To2D()
                            .Extend(ObjectManager.Player.ServerPosition.To2D(), -300)
                            .To3D())
                        .HasFlag(CollisionFlags.Wall) || NavMesh.GetCollisionFlags(
                            prediction.UnitPosition.To2D()
                                .Extend(ObjectManager.Player.ServerPosition.To2D(),
                                    -(300/2))
                                .To3D())
                            .HasFlag(CollisionFlags.Wall)
                    select hero)
            {
                _e.Cast(hero, Packets());
            }
        }

        private static void Killsteal(Obj_AI_Hero target)
        {
            var killstealMenu = Menu.SubMenu("killsteal");
            var useQ = killstealMenu.Item("killstealUseQ").GetValue<bool>() && _q.IsReady();
            var useE = killstealMenu.Item("killstealUseE").GetValue<bool>() && _e.IsReady();

            if (target.HasBuffOfType(BuffType.Invulnerability))
            {
                return;
            }

            if (useQ && target.Distance(Player.Position) < _q.Range)
            {
                if (_q.IsKillable(target))
                {
                    _q.Cast(Player, Packets());
                }
            }

            if (!useE || !(target.Distance(Player.Position) < _e.Range))
            {
                return;
            }

            if (_e.IsKillable(target))
            {
                _e.Cast(target, Packets());
            }
        }

        private static void WaveClear()
        {
            var waveclearMenu = Menu.SubMenu("waveclear");
            var useQ = waveclearMenu.Item("wcUseQ").GetValue<bool>() && _q.IsReady();
            var useW = waveclearMenu.Item("wcUseW").GetValue<bool>() && _w.IsReady();
            var useE = waveclearMenu.Item("wcUseE").GetValue<bool>() && _e.IsReady();

            var allMinionsQ = MinionManager.GetMinions(Player.ServerPosition, _q.Range);

            if (useQ)
            {
                if (allMinionsQ.Any(minion => minion.IsValidTarget() &&
                                              _q.IsKillable(minion)))
                {
                    _q.CastOnUnit(Player, Packets());
                    return;
                }
            }

            if (useW && allMinionsQ.Count > 1)
            {
                _w.Cast(Player, Packets());
            }

            if (useE)
            {
                foreach (var minion in allMinionsQ.Where(minion => minion.IsValidTarget() &&
                                                                   HealthPrediction.GetHealthPrediction(minion,
                                                                       (int)
                                                                           (Player.Distance(minion.Position)*1000/1450)) <
                                                                   Player.GetSpellDamage(minion, SpellSlot.E)))
                {
                    _e.CastOnUnit(minion, Packets());
                    return;
                }
            }

            var jcreeps = MinionManager.GetMinions(Player.ServerPosition, _e.Range, MinionTypes.All,
                MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
            if (jcreeps.Count <= 0)
            {
                return;
            }

            var jcreep = jcreeps[0];
            if (useQ)
            {
                _q.Cast(Player, Packets());
            }

            if (useW)
            {
                _w.Cast(Player, Packets());
            }

            if (useE)
            {
                _e.Cast(jcreep, Packets());
            }
        }

        private static float GetComboDamage(Obj_AI_Base enemy)
        {
            var damage = 0d;
            if (_r.IsReady())
            {
                damage += Player.GetSpellDamage(enemy, SpellSlot.R);
            }

            if (_w.IsReady())
            {
                damage += Player.GetSpellDamage(enemy, SpellSlot.W);
            }

            if (_e.IsReady())
            {
                damage += Player.GetSpellDamage(enemy, SpellSlot.E);
            }

            if (_q.IsReady())
            {
                damage += Player.GetSpellDamage(enemy, SpellSlot.Q);
            }

            if (_igniteSlot != SpellSlot.Unknown && Player.Spellbook.CanUseSpell(_igniteSlot) == SpellState.Ready)
            {
                damage += Player.GetSummonerSpellDamage(enemy, Damage.SummonerSpell.Ignite);
            }

            return (float) damage;
        }

        private static bool Packets()
        {
            return Menu.Item("miscPacket").GetValue<bool>();
        }

        private static void CreateMenu()
        {
            Menu = new Menu("花边-Black波比", "black" + ChampName, true);

            // Target selector
            var ts = new Menu("目标 选择", "ts");
            Menu.AddSubMenu(ts);
            TargetSelector.AddToMenu(ts);

            // Orbwalker
            var orbwalk = new Menu("走砍", "orbwalk");
            Menu.AddSubMenu(orbwalk);
            _ow = new Orbwalking.Orbwalker(orbwalk);

            // Combo
            var combo = new Menu("连招", "combo");
            Menu.AddSubMenu(combo);
            combo.AddItem(new MenuItem("comboUseQ", "使用 Q").SetValue(true));
            combo.AddItem(new MenuItem("comboUseW", "使用 W").SetValue(true));
            combo.AddItem(new MenuItem("comboUseE", "使用 E").SetValue(true));
            combo.AddItem(new MenuItem("comboUseR", "使用 R").SetValue(true));
            combo.AddItem(new MenuItem("comboLogicR", "使用R丨附近敌人数").SetValue(new Slider(2, 1, 5)));
            combo.AddItem(new MenuItem("comboActive", "连招 按键!").SetValue(new KeyBind(32, KeyBindType.Press)));

            // Harass
            var harass = new Menu("骚扰", "harass");
            Menu.AddSubMenu(harass);
            harass.AddItem(new MenuItem("harassUseQ", "使用 Q").SetValue(true));
            harass.AddItem(new MenuItem("harassMana", "骚扰最低蓝量").SetValue(new Slider(40, 100, 0)));
            harass.AddItem(new MenuItem("harassActive", "骚扰 按键!").SetValue(new KeyBind('C', KeyBindType.Press)));

            // WaveClear
            var waveclear = new Menu("清线", "waveclear");
            Menu.AddSubMenu(waveclear);
            waveclear.AddItem(new MenuItem("wcUseQ", "使用 Q").SetValue(true));
            waveclear.AddItem(new MenuItem("wcUseW", "使用 W").SetValue(true));
            waveclear.AddItem(new MenuItem("wcUseE", "使用 E").SetValue(true));
            waveclear.AddItem(new MenuItem("wcMana", "清线最低蓝量").SetValue(new Slider(40, 100, 0)));
            waveclear.AddItem(new MenuItem("wcActive", "清线 按键!").SetValue(new KeyBind('V', KeyBindType.Press)));

            // Killsteal
            var killsteal = new Menu("击杀", "killsteal");
            Menu.AddSubMenu(killsteal);
            killsteal.AddItem(new MenuItem("killstealUseQ", "使用 Q").SetValue(true));
            killsteal.AddItem(new MenuItem("killstealUseE", "使用 E").SetValue(true));

            // Misc
            var misc = new Menu("杂项", "misc");
            Menu.AddSubMenu(misc);
            misc.AddItem(new MenuItem("miscPacket", "使用 封包").SetValue(true));
            misc.AddItem(new MenuItem("miscIgnite", "使用 点燃").SetValue(true));
            misc.AddItem(new MenuItem("miscEscapeToMouse", "向鼠标处逃跑").SetValue(new KeyBind('G', KeyBindType.Press)));
            misc.AddItem(new MenuItem("miscUseW", "使用 W 向鼠标处逃跑").SetValue(true));
            misc.AddItem(new MenuItem("DontUlt", "禁止R对象"));
            misc.AddItem(new MenuItem("sep0", "========="));
            foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.Team != Player.Team))
            {
                misc.AddItem(new MenuItem("DontUlt" + enemy.BaseSkinName, "就是这个傻逼: " + enemy.BaseSkinName).SetValue(false));
            }
            misc.AddItem(new MenuItem("sep1", "========="));

            //Damage after combo:
            var dmgAfterComboItem = new MenuItem("DamageAfterCombo", "显示 连招 伤害").SetValue(true);
            Utility.HpBarDamageIndicator.DamageToUnit = GetComboDamage;
            Utility.HpBarDamageIndicator.Enabled = dmgAfterComboItem.GetValue<bool>();
            dmgAfterComboItem.ValueChanged +=
                delegate(object sender, OnValueChangeEventArgs eventArgs)
                {
                    Utility.HpBarDamageIndicator.Enabled = eventArgs.GetNewValue<bool>();
                };

            // Drawings
            var drawings = new Menu("显示", "drawings");
            Menu.AddSubMenu(drawings);
            drawings.AddItem(new MenuItem("drawRangeE", "E 范围").SetValue(new Circle(false, Color.Aquamarine)));
            drawings.AddItem(new MenuItem("drawRangeR", "R 范围").SetValue(new Circle(false, Color.Aquamarine)));
            drawings.AddItem(dmgAfterComboItem);

            var messageMenu = new Menu("信 息", "message");
            Menu.AddSubMenu(messageMenu);
            messageMenu.AddItem(new MenuItem("Sprite", "Black-波比"));
            messageMenu.AddItem(new MenuItem("Hanhua", "汉化:花边下丶情未央"));
            messageMenu.AddItem(new MenuItem("qqqun", "QQ群:299606556"));

            Game.PrintChat("<font color=\"#1eff00\">Huabian婕㈠寲QQ缇わ細299606556</font> - <font color=\"#00BFFF\">姝¤繋鍚勪綅鐨勫姞鍏ワ紒</font>");


            // Finalizing
            Menu.AddToMainMenu();
        }
    }
}