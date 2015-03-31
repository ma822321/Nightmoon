﻿using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using Color = System.Drawing.Color;

namespace KurisuBlitz
{
    //  _____ _ _ _                       _   
    // | __  | |_| |_ ___ ___ ___ ___ ___| |_ 
    // | __ -| | |  _|- _|  _|  _| .'|   | '_|
    // |_____|_|_|_| |___|___|_| |__,|_|_|_,_|
    //  Copyright © Kurisu Solutions 2015
   
    internal class Program
    {
        private static Menu _menu;
        private static Spell _q, _e, _r;
        private static Orbwalking.Orbwalker _orbwalker;
        private static readonly Obj_AI_Hero Me = ObjectManager.Player;
        static void Main(string[] args)
        {
            Console.WriteLine("Blitzcrank injected...");
            CustomEvents.Game.OnGameLoad += BlitzOnLoad;
        }

        private static void BlitzOnLoad(EventArgs args)
        {
            if (Me.ChampionName != "Blitzcrank")
                return;
           
            // Set spells      
            _q = new Spell(SpellSlot.Q, 1000f);
            _q.SetSkillshot(0.25f, 70f, 1800f, true, SkillshotType.SkillshotLine);

            _e = new Spell(SpellSlot.E, 150f);
            _r = new Spell(SpellSlot.R, 550f);

            // Load Menu
            _menu = new Menu("花边-Kurisu机器人", "blitz", true);

            var blitzTs = new Menu("目标选择", "tselect");
            TargetSelector.AddToMenu(blitzTs);
            _menu.AddSubMenu(blitzTs);

            var blitzOrb = new Menu("走砍", "orbwalker");
            _orbwalker = new Orbwalking.Orbwalker(blitzOrb);
            _menu.AddSubMenu(blitzOrb);

            var menuD = new Menu("显示", "drawings");
            menuD.AddItem(new MenuItem("drawQ", "显示 Q")).SetValue(new Circle(true, Color.FromArgb(150, Color.White)));
            menuD.AddItem(new MenuItem("drawR", "显示 R")).SetValue(new Circle(true, Color.FromArgb(150, Color.White)));
            menuD.AddItem(new MenuItem("drawT", "显示 目标")).SetValue(true);
            _menu.AddSubMenu(menuD);

            var spellmenu = new Menu("技能", "smenu");

            var menuQ = new Menu("Q设置", "qmenu");
            menuQ.AddItem(new MenuItem("usecomboq", "连招时使用")).SetValue(true);
            menuQ.AddItem(new MenuItem("qdashing", "Q丨突进的敌人")).SetValue(true);
            menuQ.AddItem(new MenuItem("qimmobile", "Q丨无法移动的敌人")).SetValue(true);
            menuQ.AddItem(new MenuItem("interruptq", "打断技能")).SetValue(true);
            menuQ.AddItem(new MenuItem("secureq", "击杀")).SetValue(false);
            spellmenu.AddSubMenu(menuQ);

            var menuE = new Menu("E设置", "emenu");
            menuE.AddItem(new MenuItem("usecomboe", "连招时使用")).SetValue(true);
            menuE.AddItem(new MenuItem("interrupte", "打断技能")).SetValue(true);
            menuE.AddItem(new MenuItem("securee", "击杀")).SetValue(false);
            spellmenu.AddSubMenu(menuE);

            var menuR = new Menu("R设置", "rmenu");
            menuR.AddItem(new MenuItem("usecombor", "连招时使用")).SetValue(true);
            menuR.AddItem(new MenuItem("interruptr", "打断技能")).SetValue(true);
            menuR.AddItem(new MenuItem("securer", "击杀")).SetValue(false);
            spellmenu.AddSubMenu(menuR);


            _menu.AddSubMenu(spellmenu);

            var menuM = new Menu("杂项", "bmisc");
            menuM.AddItem(new MenuItem("hitchanceq", "Q命中 1-低, 4-非常高")).SetValue(new Slider(3, 1, 4));
            menuM.AddItem(new MenuItem("dnd", "Q丨最小距离")).SetValue(new Slider(255, 0, (int)_q.Range));
            menuM.AddItem(new MenuItem("dnd2", "Q丨最大距离")).SetValue(new Slider((int)_q.Range, 0, (int)_q.Range));
            menuM.AddItem(new MenuItem("hnd", "禁用Q|生命值低于 %")).SetValue(new Slider(0));
            foreach (var obj in ObjectManager.Get<Obj_AI_Hero>().Where(obj => obj.Team != Me.Team))
            {
                menuM.AddItem(new MenuItem("dograb" + obj.ChampionName, obj.ChampionName))
                    .SetValue(new StringList(new[] { "禁止 ", "普通 ", "自动 " }, 1));
            }

            _menu.AddSubMenu(menuM);
            _menu.AddItem(new MenuItem("combokey", "连招 (按键)")).SetValue(new KeyBind(32, KeyBindType.Press));
            
            var messageM = new Menu("信息", "message");
            messageM.AddItem(new MenuItem("Sprite", "Kurisu-机器人"));
            messageM.AddItem(new MenuItem("Hanhua", "汉化:花边下丶情未央"));
            messageM.AddItem(new MenuItem("qqqun", "QQ群:299606556"));
            _menu.AddSubMenu(messageM);

            _menu.AddItem(new MenuItem("XiaoXiongMei", "带上小胸妹无形装逼"));

            _menu.AddToMainMenu();

            // events
            Drawing.OnDraw += BlitzOnDraw;
            Game.OnUpdate += BlitzOnUpdate;
            Interrupter.OnPossibleToInterrupt += BlitzOnInterruptableSpell;

            Game.PrintChat("<font color=\"#1eff00\">甯朵笂灏忚兏濡硅閫间辅</font> - <font color=\"#00BFFF\">婕㈠寲By Huabian</font>");

        }

        private static void BlitzOnInterruptableSpell(Obj_AI_Base unit, InterruptableSpell spell)
        {
            if (_menu.Item("interruptq").GetValue<bool>())
            {
                var prediction = _q.GetPrediction(unit);
                if (prediction.Hitchance >= HitChance.Low)
                {
                    _q.Cast(prediction.CastPosition);
                }
            }

            if (_menu.Item("interruptr").GetValue<bool>())
            {
                if (unit.Distance(Me.ServerPosition, true) <= _r.RangeSqr)
                {
                    _r.Cast();
                }
            }

            if (_menu.Item("interrupte").GetValue<bool>())
            {
                if (unit.Distance(Me.ServerPosition, true) <= _e.RangeSqr)
                {
                    _e.CastOnUnit(Me);
                    Me.IssueOrder(GameObjectOrder.AttackUnit, unit);
                }
            }
        }

        private static void BlitzOnDraw(EventArgs args)
        {
            var target = TargetSelector.GetTarget(_q.Range * 2, TargetSelector.DamageType.Physical);

            if (!Me.IsDead)
            {
                var rcircle = _menu.Item("drawR").GetValue<Circle>();
                var qcircle = _menu.Item("drawQ").GetValue<Circle>();

                if (qcircle.Active)
                    Render.Circle.DrawCircle(Me.Position, _q.Range, qcircle.Color);

                if (rcircle.Active)
                    Render.Circle.DrawCircle(Me.Position, _r.Range, qcircle.Color);

                if (target.IsValidTarget(_q.Range * 2) && _menu.Item("drawT").GetValue<bool>())
                    Render.Circle.DrawCircle(target.Position, target.BoundingRadius - 30, Color.Yellow, 3);
            }
        }

        private static void BlitzOnUpdate(EventArgs args)
        {
            // kill secure
            Secure(_menu.Item("secureq").GetValue<bool>(), _menu.Item("securee").GetValue<bool>(),
                   _menu.Item("securer").GetValue<bool>());

            // auto grab
            AutoCast(_menu.Item("qdashing").GetValue<bool>(),
                     _menu.Item("qimmobile").GetValue<bool>());

            if ((int) (Me.Health/Me.MaxHealth*100) >= _menu.Item("hnd").GetValue<Slider>().Value)
            {
                if (_menu.Item("combokey").GetValue<KeyBind>().Active)
                {
                    Combo(_menu.Item("usecomboq").GetValue<bool>(),
                          _menu.Item("usecomboe").GetValue<bool>());
                }
            }
        }

        private static void AutoCast(bool dashing, bool immobile)
        {
            if (_q.IsReady())
            {
                var itarget =
                    ObjectManager.Get<Obj_AI_Hero>()
                        .FirstOrDefault(h => h.IsEnemy && h.Distance(Me.ServerPosition, true) <= _q.RangeSqr);

                if (itarget.IsValidTarget(_q.Range))
                {
                    if (dashing && _menu.Item("dograb" + itarget.ChampionName).GetValue<StringList>().SelectedIndex == 2)
                        if (itarget.Distance(Me.ServerPosition) > _menu.Item("dnd").GetValue<Slider>().Value)
                            _q.CastIfHitchanceEquals(itarget, HitChance.Dashing);

                    if (immobile && _menu.Item("dograb" + itarget.ChampionName).GetValue<StringList>().SelectedIndex == 2)
                        if (itarget.Distance(Me.ServerPosition) > _menu.Item("dnd").GetValue<Slider>().Value)
                         _q.CastIfHitchanceEquals(itarget, HitChance.Immobile);
                }
            }

            if (_r.IsReady())
            {
                var rtarget =
                    ObjectManager.Get<Obj_AI_Hero>()
                    .FirstOrDefault(h => h.IsEnemy && h.Distance(Me.ServerPosition, true) <= _r.RangeSqr);

                if (rtarget.IsValidTarget(_r.Range) && _menu.Item("usecombor").GetValue<bool>())
                {
                    if (!_e.IsReady() && rtarget.HasBuffOfType(BuffType.Knockup))
                        _r.Cast();
                }            
            }
        }

        private static void Combo(bool useq, bool usee)
        {
            if (useq && _q.IsReady())
            {
                var qtarget = TargetSelector.GetTargetNoCollision(_q);
                if (qtarget.IsValidTarget(_q.Range))
                {
                    var poutput = _q.GetPrediction(qtarget);
                    if (poutput.Hitchance >= (HitChance) _menu.Item("hitchanceq").GetValue<Slider>().Value + 2)
                    {
                        if (qtarget.Distance(Me.ServerPosition) > _menu.Item("dnd").GetValue<Slider>().Value)
                        {
                            if (_menu.Item("dograb" + qtarget.ChampionName).GetValue<StringList>().SelectedIndex != 0) 
                                _q.Cast(poutput.CastPosition);
                        }
                    }
                }
            }

            if (usee && _e.IsReady())
            {
                var etarget = TargetSelector.GetTarget(250, TargetSelector.DamageType.Physical);
                if (etarget.IsValidTarget(_e.Range + 100))
                {
                    if (_menu.Item("usecomboe").GetValue<bool>() && !_q.IsReady())
                        _e.CastOnUnit(Me);                   
                }
            }
        }

        private static void Secure(bool useq, bool usee, bool user)
        {
            if (user && _r.IsReady())
            {
                var rtarget = ObjectManager.Get<Obj_AI_Hero>().FirstOrDefault(h => h.IsEnemy);
                if (rtarget.IsValidTarget(_r.Range))
                {
                    if (Me.GetSpellDamage(rtarget, SpellSlot.R) >= rtarget.Health)
                        _r.Cast();
                }
            }

            if (usee && _e.IsReady())
            {
                var etarget = ObjectManager.Get<Obj_AI_Hero>().FirstOrDefault(h => h.IsEnemy);
                if (etarget.IsValidTarget(_e.Range))
                {
                    if (Me.GetSpellDamage(etarget, SpellSlot.E) >= etarget.Health)
                        _e.CastOnUnit(Me);
                }
            }

            if (useq && _q.IsReady())
            {
                var qtarget = ObjectManager.Get<Obj_AI_Hero>().FirstOrDefault(h => h.IsEnemy);
                if (qtarget.IsValidTarget(_q.Range))
                {
                    if (Me.GetSpellDamage(qtarget, SpellSlot.Q) >= qtarget.Health)
                    {
                        var poutput = _q.GetPrediction(qtarget);
                        if (poutput.Hitchance >= HitChance.Medium)
                        {
                            if (qtarget.Distance(Me.ServerPosition, true) >
                                Math.Pow(_menu.Item("dnd2").GetValue<Slider>().Value, 2))
                            {
                                _q.Cast(poutput.CastPosition);
                            }
                        }
                    }
                }
            }
        }
    }
}
