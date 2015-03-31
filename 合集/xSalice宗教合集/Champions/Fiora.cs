﻿using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace xSaliceReligionAIO.Champions
{
    class Fiora : Champion
    {
        public Fiora()
        {
            SetSpells();
            LoadMenu();
        }

        private void SetSpells()
        {
            Q = new Spell(SpellSlot.Q, 600);

            W = new Spell(SpellSlot.W);

            E = new Spell(SpellSlot.E);

            R = new Spell(SpellSlot.R, 400);
        }

        private void LoadMenu()
        {
            //key
            var key = new Menu("按 键", "Key");
            {
                key.AddItem(new MenuItem("ComboActive", "连招 按键!",true).SetValue(new KeyBind(32, KeyBindType.Press)));
                key.AddItem(new MenuItem("HarassActive", "骚扰 按键!",true).SetValue(new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));
                key.AddItem(new MenuItem("LaneClearActive", "打钱 按键!",true).SetValue(new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));
                key.AddItem(new MenuItem("LastHitKey", "补刀 按键!",true).SetValue(new KeyBind("A".ToCharArray()[0], KeyBindType.Press)));
                key.AddItem(new MenuItem("Combo_Switch", "切换模式 按键",true).SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Press)));
                //add to menu
                menu.AddSubMenu(key);
            }

            var spellMenu = new Menu("技能 设置", "SpellMenu");
            {
                var qMenu = new Menu("Q", "QMenu");
                {
                    qMenu.AddItem(new MenuItem("Q_Min_Distance", "Q 最小距离",true).SetValue(new Slider(300, 0, 600)));
                    qMenu.AddItem(new MenuItem("Q_Gap_Close", "Q小兵突进对方",true).SetValue(true));
                    spellMenu.AddSubMenu(qMenu);
                }
                var wMenu = new Menu("W", "WMenu");
                {
                    wMenu.AddItem(new MenuItem("W_Incoming", "W总是反弹对面伤害",true).SetValue(true));
                    wMenu.AddItem(new MenuItem("W_Tower", "塔下禁止开W", true).SetValue(true));
                    wMenu.AddItem(new MenuItem("W_minion", "W反弹小兵",true).SetValue(false));
                    spellMenu.AddSubMenu(wMenu);
                }
                var eMenu = new Menu("E", "EMenu");
                {
                    eMenu.AddItem(new MenuItem("E_Reset", "E丨重置普攻",true).SetValue(true));
                    spellMenu.AddSubMenu(eMenu);
                }

                var rMenu = new Menu("R", "RMenu");
                {
                    rMenu.AddItem(new MenuItem("R_If_HP", "R丨HP <=",true).SetValue(new Slider(20)));

                    //evading spells
                    var dangerous = new Menu("躲避 危险技能", "Dodge Dangerous");
                    {
                        foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsEnemy))
                        {
                            dangerous.AddSubMenu(new Menu(hero.ChampionName, hero.ChampionName));
                            dangerous.SubMenu(hero.ChampionName).AddItem(new MenuItem(hero.Spellbook.GetSpell(SpellSlot.Q).Name + "R_Dodge", hero.Spellbook.GetSpell(SpellSlot.Q).Name,true).SetValue(false));
                            dangerous.SubMenu(hero.ChampionName).AddItem(new MenuItem(hero.Spellbook.GetSpell(SpellSlot.W).Name + "R_Dodge", hero.Spellbook.GetSpell(SpellSlot.W).Name,true).SetValue(false));
                            dangerous.SubMenu(hero.ChampionName).AddItem(new MenuItem(hero.Spellbook.GetSpell(SpellSlot.E).Name + "R_Dodge", hero.Spellbook.GetSpell(SpellSlot.E).Name,true).SetValue(false));
                            dangerous.SubMenu(hero.ChampionName).AddItem(new MenuItem(hero.Spellbook.GetSpell(SpellSlot.R).Name + "R_Dodge", hero.Spellbook.GetSpell(SpellSlot.R).Name,true).SetValue(false));
                        }
                        rMenu.AddSubMenu(dangerous);
                    }

                    spellMenu.AddSubMenu(rMenu);
                }

                menu.AddSubMenu(spellMenu);
            }

            var combo = new Menu("连 招", "Combo");
            {
                combo.AddItem(new MenuItem("selected", "自动 锁定 目标",true).SetValue(true));
                combo.AddItem(new MenuItem("Combo_mode", "连招 模式",true).SetValue(new StringList(new[] { "正常", "Q-AA-Q-AA-R" })));
                combo.AddItem(new MenuItem("UseQCombo", "使用 Q", true).SetValue(true));
                combo.AddItem(new MenuItem("UseWCombo", "使用 W", true).SetValue(true));
                combo.AddItem(new MenuItem("UseECombo", "使用 E", true).SetValue(true));
                combo.AddItem(new MenuItem("UseRCombo", "使用 R", true).SetValue(true));
                //add to menu
                menu.AddSubMenu(combo);
            }

            var harass = new Menu("骚 扰", "Harass");
            {
                harass.AddItem(new MenuItem("UseQHarass", "使用 Q", true).SetValue(true));
                harass.AddItem(new MenuItem("UseWHarass", "使用 W", true).SetValue(false));
                harass.AddItem(new MenuItem("UseEHarass", "使用 E", true).SetValue(true));
                AddManaManagertoMenu(harass, "Harass", 30);
                //add to menu
                menu.AddSubMenu(harass);
            }

            var lastHit = new Menu("补 刀", "Lasthit");
            {
                lastHit.AddItem(new MenuItem("UseQLastHit", "使用 Q", true).SetValue(true));
                AddManaManagertoMenu(lastHit, "Lasthit", 30);
                //add to menu
                menu.AddSubMenu(lastHit);
            }

            var farm = new Menu("清 线", "LaneClear");
            {
                farm.AddItem(new MenuItem("UseQFarm", "使用 Q", true).SetValue(true));
                farm.AddItem(new MenuItem("UseQFarm_Tower", "禁止 塔下Q",true).SetValue(true));
                farm.AddItem(new MenuItem("UseEFarm", "使用 E", true).SetValue(true));
                AddManaManagertoMenu(farm, "LaneClear", 30);
                //add to menu
                menu.AddSubMenu(farm);
            }
            var misc = new Menu("杂 项", "Misc");
            {
                misc.AddItem(new MenuItem("smartKS", "使用 智能击杀系统", true).SetValue(true));
                //add to menu
                menu.AddSubMenu(misc);
            }
            var drawMenu = new Menu("范 围", "Drawing");
            {
                drawMenu.AddItem(new MenuItem("Draw_Disabled", "禁止 所有 范围",true).SetValue(false));
                drawMenu.AddItem(new MenuItem("Draw_Q", "显示 Q 范围",true).SetValue(true));
                drawMenu.AddItem(new MenuItem("Draw_E", "显示 E 范围",true).SetValue(true));
                drawMenu.AddItem(new MenuItem("Draw_R", "显示 R 范围",true).SetValue(true));
                drawMenu.AddItem(new MenuItem("Draw_R_Killable", "标记 R可击杀目标",true).SetValue(true));

                MenuItem drawComboDamageMenu = new MenuItem("Draw_ComboDamage", "显示 连招 伤害",true).SetValue(true);
                MenuItem drawFill = new MenuItem("Draw_Fill", "显示 满连招 伤害",true).SetValue(new Circle(true, Color.FromArgb(90, 255, 169, 4)));
                drawMenu.AddItem(drawComboDamageMenu);
                drawMenu.AddItem(drawFill);
                DamageIndicator.DamageToUnit = GetComboDamage;
                DamageIndicator.Enabled = drawComboDamageMenu.GetValue<bool>();
                DamageIndicator.Fill = drawFill.GetValue<Circle>().Active;
                DamageIndicator.FillColor = drawFill.GetValue<Circle>().Color;
                drawComboDamageMenu.ValueChanged +=
                    delegate(object sender, OnValueChangeEventArgs eventArgs)
                    {
                        DamageIndicator.Enabled = eventArgs.GetNewValue<bool>();
                    };
                drawFill.ValueChanged +=
                    delegate(object sender, OnValueChangeEventArgs eventArgs)
                    {
                        DamageIndicator.Fill = eventArgs.GetNewValue<Circle>().Active;
                        DamageIndicator.FillColor = eventArgs.GetNewValue<Circle>().Color;
                    };
                //add to menu
                menu.AddSubMenu(drawMenu);
            }
        }

        private float GetComboDamage(Obj_AI_Base target)
        {
            double comboDamage = 0;

            if (Q.IsReady())
                comboDamage += Player.GetSpellDamage(target, SpellSlot.Q)*2;

            if (W.IsReady())
                comboDamage += Player.GetSpellDamage(target, SpellSlot.W);

            if (R.IsReady())
                comboDamage += Player.GetSpellDamage(target, SpellSlot.R) / countEnemiesNearPosition(target.ServerPosition, R.Range);

            comboDamage = ActiveItems.CalcDamage(target, comboDamage);

            return (float)(comboDamage + Player.GetAutoAttackDamage(target) * 3);
        }

        private void Combo()
        {
            UseSpells(menu.Item("UseQCombo", true).GetValue<bool>(),
                menu.Item("UseECombo", true).GetValue<bool>(), menu.Item("UseRCombo", true).GetValue<bool>(), "Combo");
        }

        private void Harass()
        {
            UseSpells(menu.Item("UseQHarass", true).GetValue<bool>(),
                menu.Item("UseEHarass", true).GetValue<bool>(), false, "Harass");
        }

        private void UseSpells(bool useQ, bool useE, bool useR, string source)
        {
            if (source == "Harass" && !HasMana("Harass"))
                return;

            if (useQ)
                Cast_Q();

            //items
            if (source == "Combo")
            {
                var itemTarget = TargetSelector.GetTarget(750, TargetSelector.DamageType.Physical);
                if (itemTarget != null)
                {
                    var dmg = GetComboDamage(itemTarget);
                    ActiveItems.Target = itemTarget;

                    //see if killable
                    if (dmg > itemTarget.Health - 50)
                        ActiveItems.KillableTarget = true;

                    ActiveItems.UseTargetted = true;
                }
            }

            if (useE && !menu.Item("E_Reset", true).GetValue<bool>())
                E.Cast(packets());

            if (useR)
                Cast_R();

        }

        private void Lasthit()
        {
            if (menu.Item("UseQLastHit", true).GetValue<bool>() && HasMana("Lasthit"))
                Cast_Q_Last_Hit();
        }

        private void Farm()
        {
            if (!HasMana("LaneClear"))
                return;

            List<Obj_AI_Base> allMinionsE = MinionManager.GetMinions(Player.ServerPosition, Player.AttackRange + Player.BoundingRadius,
                MinionTypes.All, MinionTeam.NotAlly);

            var useQ = menu.Item("UseQFarm", true).GetValue<bool>();
            var useE = menu.Item("UseEFarm", true).GetValue<bool>();

            if (useQ)
                Cast_Q_Last_Hit();

            if (useE && allMinionsE.Count > 0 && E.IsReady())
                E.Cast();

        }

        private void SmartKs()
        {
            if (!menu.Item("smartKS", true).GetValue<bool>())
                return;
            foreach (Obj_AI_Hero target in ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsValidTarget(Q.Range) && !x.IsDead && !x.HasBuffOfType(BuffType.Invulnerability)))
            {
                //Q *2
                if (Player.GetSpellDamage(target, SpellSlot.Q)*2 > target.Health && Player.Distance(target) < Q.Range && Q.IsReady())
                {
                    Q.CastOnUnit(target, packets());
                    return;
                }
                //Q
                if (Player.GetSpellDamage(target, SpellSlot.Q) > target.Health && Player.Distance(target) < Q.Range && Q.IsReady())
                {
                    Q.CastOnUnit(target, packets());
                    return;
                }
            }
        }

        private void Cast_Q()
        {
            var target = TargetSelector.GetTarget(Q.Range * 2, TargetSelector.DamageType.Physical);

            if (GetTargetFocus(Q.Range) != null)
                target = GetTargetFocus(Q.Range);

            int mode = menu.Item("Combo_mode", true).GetValue<StringList>().SelectedIndex;
            if (mode == 0)
            {
                if (Q.IsReady() && target != null)
                {
                    if (Q.IsKillable(target))
                        Q.CastOnUnit(target, packets());

                    if (Player.GetSpellDamage(target, SpellSlot.Q)*2 > target.Health)
                        Q.CastOnUnit(target, packets());

                    if (Environment.TickCount - Q.LastCastAttemptT > 3800)
                        Q.CastOnUnit(target, packets());

                    var minDistance = menu.Item("Q_Min_Distance", true).GetValue<Slider>().Value;

                    if (Player.Distance(target) > Q.Range && menu.Item("Q_Gap_Close", true).GetValue<bool>())
                    {
                        var allMinionQ = MinionManager.GetMinions(Player.ServerPosition, Q.Range, MinionTypes.All,
                            MinionTeam.NotAlly);

                        Obj_AI_Base bestMinion = allMinionQ[0];

                        foreach (var minion in allMinionQ)
                        {
                            if (target.Distance(minion) < Q.Range && Player.Distance(minion) < Q.Range &&
                                target.Distance(minion) < target.Distance(Player))
                                if (target.Distance(minion) < target.Distance(bestMinion))
                                    bestMinion = minion;
                        }
                    }

                    if (Player.Distance(target) > minDistance &&
                        Player.Distance(target) < Q.Range + target.BoundingRadius)
                    {
                        Q.CastOnUnit(target, packets());
                    }
                }
            }
            else if (mode == 1)//Ham mode
            {
                if (target == null)
                    return;

                if (Q.IsReady() && Environment.TickCount - Q.LastCastAttemptT > 4000 && Player.Distance(target) < Q.Range && Player.Distance(target) > Player.AttackRange)
                    Q.CastOnUnit(target, packets());
            }
        }

        private void Cast_Q_Last_Hit()
        {
            var allMinionQ = MinionManager.GetMinions(Player.ServerPosition, Q.Range + Player.BoundingRadius, MinionTypes.All, MinionTeam.NotAlly);

            if (allMinionQ.Count > 0 && Q.IsReady())
            {

                foreach (var minion in allMinionQ)
                {
                    double dmg = Player.GetSpellDamage(minion, SpellSlot.Q);

                    if (dmg > minion.Health + 35)
                    {
                        if (menu.Item("UseQFarm_Tower", true).GetValue<bool>())
                        {
                            if (!Utility.UnderTurret(minion, true))
                            {
                                Q.Cast(minion, packets());
                                return;
                            }
                        }
                        else
                            Q.Cast(minion, packets());
                    }
                }
            }
        }

        private void Cast_R()
        {
            var target = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Physical);

            var range = R.Range;
            if (GetTargetFocus(range) != null)
                target = GetTargetFocus(range);

            if (target != null && R.IsReady())
            {
                if (Player.GetSpellDamage(target, SpellSlot.R)/
                    countEnemiesNearPosition(target.ServerPosition, R.Range) >
                    target.Health - Player.GetAutoAttackDamage(target)*2)
                    R.CastOnUnit(target, packets());

                var rHpValue = menu.Item("R_If_HP", true).GetValue<Slider>().Value;
                if (GetHealthPercent() <= rHpValue)
                    R.CastOnUnit(target, packets());
            }
            
        }

        private int _lasttick;
        private void ModeSwitch()
        {
            int mode = menu.Item("Combo_mode", true).GetValue<StringList>().SelectedIndex;
            int lasttime = Environment.TickCount - _lasttick;

            if (menu.Item("Combo_Switch", true).GetValue<KeyBind>().Active && lasttime > Game.Ping)
            {
                if (mode == 0)
                {
                    menu.Item("Combo_mode",true).SetValue(new StringList(new[] { "Normal", "Q-AA-Q-AA-Ult" }, 1));
                    _lasttick = Environment.TickCount + 300;
                }
                else if (mode == 1)
                {
                    menu.Item("Combo_mode",true).SetValue(new StringList(new[] { "Normal", "Q-AA-Q-AA-Ult" }));
                    _lasttick = Environment.TickCount + 300;
                }
            }
        }

        private Obj_AI_Base dodgeHero;

        protected override void Game_OnGameUpdate(EventArgs args)
        {
            //check if player is dead
            if (Player.IsDead) return;

            SmartKs();

            ModeSwitch();

            if(Environment.TickCount - R.LastCastAttemptT < 750)
                R.Cast(dodgeHero, packets());

            if (menu.Item("ComboActive", true).GetValue<KeyBind>().Active)
            {
                Combo();
            }
            else
            {
                if (menu.Item("LaneClearActive", true).GetValue<KeyBind>().Active)
                    Farm();

                if (menu.Item("LastHitKey", true).GetValue<KeyBind>().Active)
                    Lasthit();

                if (menu.Item("HarassActive", true).GetValue<KeyBind>().Active)
                    Harass();
            }
        }

        protected override void AfterAttack(AttackableUnit unit, AttackableUnit mytarget)
        {
            var target = (Obj_AI_Base) mytarget;
            if (unit.IsMe)
            {
                if ((menu.Item("ComboActive", true).GetValue<KeyBind>().Active || menu.Item("HarassActive", true).GetValue<KeyBind>().Active )
                    && (target is Obj_AI_Hero))
                {
                    if (menu.Item("E_Reset", true).GetValue<bool>() && E.IsReady())
                        E.Cast();

                    int mode = menu.Item("Combo_mode", true).GetValue<StringList>().SelectedIndex;
                    if (mode == 1)
                    {
                        Q.CastOnUnit(target, packets());

                        if(QSpell.State == SpellState.Cooldown && R.IsReady())
                            R.CastOnUnit(target, packets());
                    }
                }
            }
        }

        protected override void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base unit, GameObjectProcessSpellCastEventArgs args)
        {
            if (unit.IsMe)
            {
                SpellSlot castedSlot = Player.GetSpellSlot(args.SData.Name);

                if (castedSlot == SpellSlot.Q)
                {
                    Q.LastCastAttemptT = Environment.TickCount;
                }
            }

            if (unit.IsMe)
                return;

            if (xSLxOrbwalker.IsAutoAttack(args.SData.Name) && args.Target.IsMe && Player.Distance(args.End) < 450)
            {
                if (menu.Item("W_Incoming", true).GetValue<bool>() ||
                    (menu.Item("ComboActive", true).GetValue<KeyBind>().Active && E.IsReady() && menu.Item("UseWCombo", true).GetValue<bool>()) ||
                    (menu.Item("HarassActive", true).GetValue<KeyBind>().Active && menu.Item("UseWHarass", true).GetValue<bool>()))
                {
                    if (!menu.Item("W_minion", true).GetValue<bool>() && !(unit is Obj_AI_Hero))
                        return;

                    if (menu.Item("W_Tower", true).GetValue<bool>() && Player.UnderTurret(true))
                        return;

                    W.Cast(packets());
                }
            }

            if (unit.IsEnemy && (unit is Obj_AI_Hero))
            {
                if (Player.Distance(args.End) > R.Range || !R.IsReady())
                    return;

                if (menu.Item(args.SData.Name + "R_Dodge", true).GetValue<bool>() && args.SData.Name == "SyndraR")
                {
                    Utility.DelayAction.Add(150, () => R.CastOnUnit(unit, packets()));
                    return;
                }

                if (menu.Item(args.SData.Name + "R_Dodge", true).GetValue<bool>())
                {
                    //Game.PrintChat("RAWR");
                    R.Cast(unit, packets());
                    dodgeHero = unit;
                    R.LastCastAttemptT = Environment.TickCount;
                }
            }
        }

        protected override void Drawing_OnDraw(EventArgs args)
        {
            if (menu.Item("Draw_Disabled", true).GetValue<bool>())
                return;

            if (menu.Item("Draw_Q", true).GetValue<bool>())
                if (Q.Level > 0)
                    Render.Circle.DrawCircle(Player.Position, Q.Range, Q.IsReady() ? Color.Green : Color.Red);

            if (menu.Item("Draw_E", true).GetValue<bool>())
                if (E.Level > 0)
                    Render.Circle.DrawCircle(Player.Position, E.Range, E.IsReady() ? Color.Green : Color.Red);

            if (menu.Item("Draw_R", true).GetValue<bool>())
                if (R.Level > 0)
                    Render.Circle.DrawCircle(Player.Position, R.Range, R.IsReady() ? Color.Green : Color.Red);

            if (menu.Item("Draw_R_Killable", true).GetValue<bool>() && R.IsReady())
            {
                foreach (var target in ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsValidTarget(5000) && !x.IsDead && x.IsEnemy).OrderBy(x => x.Health))
                {
                    Vector2 wts = Drawing.WorldToScreen(target.Position);
                    if (Player.GetSpellDamage(target, SpellSlot.R) / countEnemiesNearPosition(target.ServerPosition, R.Range) > target.Health)
                    {
                        Drawing.DrawText(wts[0] - 20, wts[1], Color.White, "KILL!!!");

                    }
                }
            }

            Vector2 wts2 = Drawing.WorldToScreen(Player.Position);
            int mode = menu.Item("Combo_mode", true).GetValue<StringList>().SelectedIndex;
            if (mode == 0)
                Drawing.DrawText(wts2[0] - 20, wts2[1], Color.White, "Normal");
            else if (mode == 1)
                Drawing.DrawText(wts2[0] - 20, wts2[1], Color.White, "Q-AA-Q-AA-Ult");
        }
    }
}
