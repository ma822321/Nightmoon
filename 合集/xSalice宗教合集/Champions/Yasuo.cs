﻿using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace xSaliceReligionAIO.Champions
{
    class Yasuo : Champion
    {
        public Yasuo()
        {
            SetSpells();
            LoadMenu();
        }

        private void SetSpells()
        {
            Q = new Spell(SpellSlot.Q, 475);
            Q.SetSkillshot(0.4f, 20f, float.MaxValue, false, SkillshotType.SkillshotLine);

            Q2 = new Spell(SpellSlot.Q, 1050);
            Q2.SetSkillshot(0.5f, 90f, 1500f, true, SkillshotType.SkillshotLine);

            W = new Spell(SpellSlot.W, 400);

            E = new Spell(SpellSlot.E, 475);
            E.SetSkillshot(.1f, 350f, float.MaxValue, false, SkillshotType.SkillshotCircle);

            R = new Spell(SpellSlot.R, 1200);
        }

        private void LoadMenu()
        {
            var key = new Menu("按 键", "Key");
            {
                key.AddItem(new MenuItem("ComboActive", "连招 按键!", true).SetValue(new KeyBind(32, KeyBindType.Press)));
                key.AddItem(new MenuItem("HarassActive", "骚扰 按键!", true).SetValue(new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));
                key.AddItem(new MenuItem("HarassActiveT", "骚扰 (自动)!", true).SetValue(new KeyBind("N".ToCharArray()[0], KeyBindType.Toggle)));
                key.AddItem(new MenuItem("LaneClearActive", "打钱 按键!", true).SetValue(new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));
                key.AddItem(new MenuItem("LastHit", "补刀 按键", true).SetValue(new KeyBind("A".ToCharArray()[0], KeyBindType.Press)));
                key.AddItem(new MenuItem("Flee", "E 逃跑", true).SetValue(new KeyBind("Z".ToCharArray()[0], KeyBindType.Press)));
                //add to menu
                menu.AddSubMenu(key);
            }

            var spellMenu = new Menu("技能 设置", "SpellMenu");
            {
                var qMenu = new Menu("Q", "QMenu");
                {
                    qMenu.AddItem(new MenuItem("Q_Auto", "自动 Q", true).SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Toggle)));
                    qMenu.AddItem(new MenuItem("Q_Auto_third", "自动 3Q", true).SetValue(true));
                    qMenu.AddItem(new MenuItem("Q_UnderTower", "塔下自动 Q", true).SetValue(false));
                    qMenu.AddItem(new MenuItem("Q_Stack", "自动 3Q (切换)", true).SetValue(new KeyBind("I".ToCharArray()[0], KeyBindType.Toggle)));
                    qMenu.AddItem(new MenuItem("Q_thirdE", "先 E->3Q再Q", true).SetValue(true));
                    spellMenu.AddSubMenu(qMenu);
                }

                var wMenu = new Menu("W", "WMenu");
                {
                    //wind wall
                    var dangerous = new Menu("躲避 危险法术", "Dodge Dangerous");
                    {
                        SpellDatabase.CreateSpellDatabase();
                        foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsEnemy))
                        {
                            dangerous.AddSubMenu(new Menu(hero.ChampionName, hero.ChampionName));

                            var q = SpellDatabase.Spells.FirstOrDefault(x => x.ChampionName == hero.ChampionName && x.Slot == SpellSlot.Q);
                            if (q != null)
                                dangerous.SubMenu(hero.ChampionName).AddItem(new MenuItem(q.MissileSpellName + "W_Wall", q.MissileSpellName, true).SetValue(false));
                            else
                                dangerous.SubMenu(hero.ChampionName).AddItem(new MenuItem(hero.Spellbook.GetSpell(SpellSlot.Q).Name + "W_Wall", hero.Spellbook.GetSpell(SpellSlot.Q).Name, true).SetValue(false));
                            
                            var w = SpellDatabase.Spells.FirstOrDefault(x => x.ChampionName == hero.ChampionName && x.Slot == SpellSlot.W);
                            if (w != null)
                                dangerous.SubMenu(hero.ChampionName).AddItem(new MenuItem(w.MissileSpellName + "W_Wall", w.MissileSpellName, true).SetValue(false));
                            else
                                dangerous.SubMenu(hero.ChampionName).AddItem(new MenuItem(hero.Spellbook.GetSpell(SpellSlot.W).Name + "W_Wall", hero.Spellbook.GetSpell(SpellSlot.W).Name, true).SetValue(false));

                            var e = SpellDatabase.Spells.FirstOrDefault(x => x.ChampionName == hero.ChampionName && x.Slot == SpellSlot.E);
                            if (e != null)
                                dangerous.SubMenu(hero.ChampionName).AddItem(new MenuItem(e.MissileSpellName + "W_Wall", e.MissileSpellName, true).SetValue(false));
                            else
                                dangerous.SubMenu(hero.ChampionName).AddItem(new MenuItem(hero.Spellbook.GetSpell(SpellSlot.E).Name + "W_Wall", hero.Spellbook.GetSpell(SpellSlot.E).Name, true).SetValue(false));

                            var r = SpellDatabase.Spells.FirstOrDefault(x => x.ChampionName == hero.ChampionName && x.Slot == SpellSlot.R);
                            if (r != null)
                                dangerous.SubMenu(hero.ChampionName).AddItem(new MenuItem(r.MissileSpellName + "W_Wall", r.MissileSpellName, true).SetValue(false));
                            else
                                 dangerous.SubMenu(hero.ChampionName).AddItem(new MenuItem(hero.Spellbook.GetSpell(SpellSlot.R).Name + "W_Wall", hero.Spellbook.GetSpell(SpellSlot.R).Name, true).SetValue(false));

                        }
                        wMenu.AddSubMenu(dangerous);
                    }
                    spellMenu.AddSubMenu(wMenu);
                }

                var eMenu = new Menu("E", "EMenu");
                {
                    eMenu.AddItem(new MenuItem("E_Min_Dist", "E 最小距离", true).SetValue(new Slider(250, 1, 475)));
                    //e Evade
                    var dangerous = new Menu("躲避 法术", "Dodge Spells");
                    {
                        foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsEnemy))
                        {
                            dangerous.AddSubMenu(new Menu(hero.ChampionName, hero.ChampionName));

                            var q = SpellDatabase.Spells.FirstOrDefault(x => x.ChampionName == hero.ChampionName && x.Slot == SpellSlot.Q);
                            if (q != null)
                                dangerous.SubMenu(hero.ChampionName).AddItem(new MenuItem(q.MissileSpellName + "E", q.MissileSpellName, true).SetValue(false));
                            else
                                dangerous.SubMenu(hero.ChampionName).AddItem(new MenuItem(hero.Spellbook.GetSpell(SpellSlot.Q).Name + "E", hero.Spellbook.GetSpell(SpellSlot.Q).Name, true).SetValue(false));

                            var w = SpellDatabase.Spells.FirstOrDefault(x => x.ChampionName == hero.ChampionName && x.Slot == SpellSlot.W);
                            if (w != null)
                                dangerous.SubMenu(hero.ChampionName).AddItem(new MenuItem(w.MissileSpellName + "E", w.MissileSpellName, true).SetValue(false));
                            else
                                dangerous.SubMenu(hero.ChampionName).AddItem(new MenuItem(hero.Spellbook.GetSpell(SpellSlot.W).Name + "E", hero.Spellbook.GetSpell(SpellSlot.W).Name, true).SetValue(false));

                            var e = SpellDatabase.Spells.FirstOrDefault(x => x.ChampionName == hero.ChampionName && x.Slot == SpellSlot.E);
                            if (e != null)
                                dangerous.SubMenu(hero.ChampionName).AddItem(new MenuItem(e.MissileSpellName + "E", e.MissileSpellName, true).SetValue(false));
                            else
                                dangerous.SubMenu(hero.ChampionName).AddItem(new MenuItem(hero.Spellbook.GetSpell(SpellSlot.E).Name + "E", hero.Spellbook.GetSpell(SpellSlot.E).Name, true).SetValue(false));

                            var r = SpellDatabase.Spells.FirstOrDefault(x => x.ChampionName == hero.ChampionName && x.Slot == SpellSlot.R);
                            if (r != null)
                                dangerous.SubMenu(hero.ChampionName).AddItem(new MenuItem(r.MissileSpellName + "E", r.MissileSpellName, true).SetValue(false));
                            else
                                dangerous.SubMenu(hero.ChampionName).AddItem(new MenuItem(hero.Spellbook.GetSpell(SpellSlot.R).Name + "E", hero.Spellbook.GetSpell(SpellSlot.R).Name, true).SetValue(false));
                        }
                        eMenu.AddSubMenu(dangerous);
                    }
                    eMenu.AddItem(new MenuItem("E_GapClose", "使用 E丨突进", true).SetValue(true));
                    eMenu.AddItem(new MenuItem("E_Turret", "禁止塔下自动E", true).SetValue(new KeyBind("H".ToCharArray()[0], KeyBindType.Toggle)));
                    spellMenu.AddSubMenu(eMenu);
                }

                var rMenu = new Menu("R", "RMenu");
                {
                    rMenu.AddItem(new MenuItem("R_If_Killable", "R丨敌人可被击杀", true).SetValue(true));
                    rMenu.AddItem(new MenuItem("delayR", "延迟R击杀敌人", true).SetValue(true));
                    rMenu.AddItem(new MenuItem("R_MEC", "自动 R丨敌人数>=, 0 =关闭", true).SetValue(new Slider(3, 0, 5)));
                    spellMenu.AddSubMenu(rMenu);
                }
                //add to menu
                menu.AddSubMenu(spellMenu);
            }

            var combo = new Menu("连 招", "Combo");
            {
                combo.AddItem(new MenuItem("UseQCombo", "使用 Q", true).SetValue(true));
                combo.AddItem(new MenuItem("qHit", "Q 命中", true).SetValue(new Slider(2, 1, 3)));
                combo.AddItem(new MenuItem("UseECombo", "使用 E", true).SetValue(true));
                combo.AddItem(new MenuItem("UseRCombo", "使用 R", true).SetValue(true));
                combo.AddItem(new MenuItem("ComboR_MEC", "R 敌人数", true).SetValue(new Slider(3, 1, 5)));
                //add to menu
                menu.AddSubMenu(combo);
            }
            var harass = new Menu("骚 扰", "Harass");
            {
                harass.AddItem(new MenuItem("UseQHarass", "使用 Q", true).SetValue(true));
                harass.AddItem(new MenuItem("qHit2", "Q 命中", true).SetValue(new Slider(2, 1, 3)));
                harass.AddItem(new MenuItem("UseEHarass", "使用 E", true).SetValue(true));
                //add to menu
                menu.AddSubMenu(harass);
            }
            var farm = new Menu("打 钱", "Farming");
            {
                farm.AddItem(new MenuItem("UseQLast", "使用 Q 补刀", true).SetValue(true));
                farm.AddItem(new MenuItem("UseELast", "使用 E 补刀", true).SetValue(true));
                farm.AddItem(new MenuItem("UseQFarm", "使用 Q 清线", true).SetValue(true));
                farm.AddItem(new MenuItem("UseQ3Farm", "使用 Q3 清线", true).SetValue(true));
                farm.AddItem(new MenuItem("UseEFarm", "使用 E 清线", true).SetValue(true));
                farm.AddItem(new MenuItem("E_UnderTower_Farm", "E回塔底", true).SetValue(false));
                farm.AddItem(new MenuItem("LaneClear_useQ_minHit", "使用 Q(清兵数)", true).SetValue(new Slider(2, 1, 6)));
                //add to menu
                menu.AddSubMenu(farm);
            }

            var misc = new Menu("杂 项", "Misc");
            {
                misc.AddItem(new MenuItem("smartKS", "使用 智能击杀系统", true).SetValue(true));
                misc.AddItem(new MenuItem("Interrupt", "打断 法术", true).SetValue(true));
                misc.AddItem(new MenuItem("predMode", "预判 模式, off =快, on =准", true).SetValue(true));
                menu.AddSubMenu(misc);
            }

            var drawMenu = new Menu("范围", "Drawing");
            {
                drawMenu.AddItem(new MenuItem("Draw_Disabled", "禁止 所有 范围", true).SetValue(false));
                drawMenu.AddItem(new MenuItem("Draw_Q", "显示 Q", true).SetValue(true));
                drawMenu.AddItem(new MenuItem("Draw_Q2", "显示 Q 额外", true).SetValue(true));
                drawMenu.AddItem(new MenuItem("Draw_E", "显示 E", true).SetValue(true));
                drawMenu.AddItem(new MenuItem("Draw_R", "显示 R", true).SetValue(true));
                drawMenu.AddItem(new MenuItem("Draw_AutoQ", "显示 模式", true).SetValue(true));

                MenuItem drawComboDamageMenu = new MenuItem("Draw_ComboDamage", "显示 连招 伤害", true).SetValue(true);
                MenuItem drawFill = new MenuItem("Draw_Fill", "显示 满连招 伤害", true).SetValue(new Circle(true, Color.FromArgb(90, 255, 169, 4)));
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
            if (target == null)
                return 0;

            double comboDamage = 0;

            if (Q.IsReady())
                comboDamage += Player.GetSpellDamage(target, SpellSlot.Q) * 2;

            if (E.IsReady())
                comboDamage += Player.GetSpellDamage(target, SpellSlot.E) * 1.5;

            if (R.IsReady())
                comboDamage += Player.GetSpellDamage(target, SpellSlot.R);

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
            var itemTarget = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Physical);
            var dmg = GetComboDamage(itemTarget);

            if (useE)
                Cast_E();

            if (useQ)
                Cast_Q(source);

            if (useR)
                Cast_R(dmg);

            //items
            if (source == "Combo")
            {
                if (itemTarget != null)
                {
                    ActiveItems.Target = itemTarget;

                    //see if killable
                    if (dmg > itemTarget.Health - 50)
                        ActiveItems.KillableTarget = true;

                    ActiveItems.UseTargetted = true;
                }
            }
        }

        private void Cast_Q(string source)
        {
            if (!Q.IsReady() || Environment.TickCount - E.LastCastAttemptT < 250)
                return;

            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);

            if (target != null)
                if (menu.Item("Q_thirdE", true).GetValue<bool>() && E.IsReady() && CanCastE(target))
                    return;

            if (!ThirdQ() && target != null && target.IsValidTarget(Q.Range))
            {
                if (!menu.Item("predMode", true).GetValue<bool>())
                {
                    if (Player.Distance(target) < 150)
                        Q.Cast(target.ServerPosition, packets());
                    else
                        Q.Cast(target, packets());
                }
                else
                {
                    CastBasicSkillShot(Q, Q.Range, TargetSelector.DamageType.Physical, GetHitchance(source));
                }
            }
            else if(ThirdQ())
            {
                var target2 = TargetSelector.GetTarget(Q2.Range, TargetSelector.DamageType.Physical);
                if (!target2.IsValidTarget(Q2.Range))
                    return;

                if (!menu.Item("predMode", true).GetValue<bool>())
                {
                    Q2.Cast(target2, packets());
                }
                else
                {
                    CastBasicSkillShot(Q2, Q2.Range, TargetSelector.DamageType.Physical, GetHitchance(source));
                }
            }
        }

        private void Cast_E()
        {
            var target = TargetSelector.GetTarget(Q2.Range, TargetSelector.DamageType.Physical);

            if (target == null || !E.IsReady() || !CanCastE(target))
                return;

            if (E.IsKillable(target) && Player.Distance(target) < E.Range + target.BoundingRadius && EturretCheck(target))
                E.CastOnUnit(target, packets());

            //EQ3
            if (ThirdQ() && Player.ServerPosition.To2D().Distance(target.ServerPosition.To2D()) < E.Range)
            {
                if (EturretCheck(target))
                {
                    E.CastOnUnit(target);
                    Utility.DelayAction.Add(200, () => Q.Cast(target, packets()));
                    return;
                }
            }

            if (Player.ServerPosition.To2D().Distance(target.ServerPosition.To2D()) <= menu.Item("E_Min_Dist", true).GetValue<Slider>().Value)
                return;

            //gapclose
            if (menu.Item("E_GapClose", true).GetValue<bool>())
            { 
                var allMinionQ = MinionManager.GetMinions(Player.ServerPosition, E.Range, MinionTypes.All, MinionTeam.NotAlly);

                if (allMinionQ.Count > 0)
                {
                    Obj_AI_Base bestMinion = allMinionQ[0];
                    Vector3 bestVec = Player.ServerPosition + Vector3.Normalize(bestMinion.ServerPosition - Player.ServerPosition) * 475;

                    foreach (var minion in allMinionQ.Where(CanCastE))
                    {
                        var dashVec = Player.ServerPosition + Vector3.Normalize(minion.ServerPosition - Player.ServerPosition) * 475;

                        if (target.Distance(Player) > target.Distance(bestVec) - 50 && target.Distance(bestVec) > target.Distance(dashVec))
                        {
                            bestMinion = minion;
                            bestVec = dashVec;
                        }
                    }
                    if (target.Distance(Player) > target.Distance(bestVec) - 50 && bestMinion != null)
                    {
                        if (EturretCheck(bestMinion))
                        {
                            E.CastOnUnit(bestMinion, packets());
                            return;
                        }
                    }
                }
            }

            if (Q.IsReady() && Player.Distance(target) > menu.Item("E_Min_Dist", true).GetValue<Slider>().Value &&
                Player.Distance(target) < E.Range)
            {
                if (EturretCheck(target))
                {
                    E.CastOnUnit(target, packets());
                    Utility.DelayAction.Add(200, () => Q.Cast(target, packets()));
                    return;
                }
            }

            if (Player.ServerPosition.To2D().Distance(target.ServerPosition.To2D()) > menu.Item("E_Min_Dist", true).GetValue<Slider>().Value && Player.Distance(target) < E.Range + target.BoundingRadius) 
                if(EturretCheck(target))
                E.CastOnUnit(target, packets());
        }

        private bool EturretCheck(Obj_AI_Base target)
        {
            var dashCheck = menu.Item("E_Turret", true).GetValue<KeyBind>().Active;

            if (dashCheck)
            {
                var dashVec = Player.ServerPosition + Vector3.Normalize(target.ServerPosition - Player.ServerPosition)*475;
                return !dashVec.UnderTurret(true);
            }
            return true;
        }

        private void Cast_R(float dmg)
        {
            if (!R.IsReady())
                return;

            int hit = 0;
            foreach (var target in ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsValidTarget(R.Range) && isKnockedUp(x)))
            {
                hit = 1;
                if (dmg > target.Health && menu.Item("R_If_Killable", true).GetValue<bool>())
                {
                    if (menu.Item("delayR", true).GetValue<bool>())
                    {
                        Utility.DelayAction.Add((int)(BuffDurationLeft(target)*1000 - 200), () => R.Cast(packets()));
                        R.LastCastAttemptT = Environment.TickCount;
                    }
                    else
                    {
                        R.Cast(packets());
                    }
                }

                hit += ObjectManager.Get<Obj_AI_Hero>().Count(x => x.ChampionName != target.ChampionName && target.Distance(x) < 400 && isKnockedUp(x));
            }

            if (hit >= menu.Item("ComboR_MEC", true).GetValue<Slider>().Value)
                R.Cast();
        }

        private void Cast_MecR()
        {
            if (!R.IsReady())
                return;

            if (menu.Item("R_MEC", true).GetValue<Slider>().Value == 0)
                return;

            int hit = 1;
            foreach (var target in ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsValidTarget(R.Range) && isKnockedUp(x)))
            {
                hit += ObjectManager.Get<Obj_AI_Hero>().Count(x => x.ChampionName != target.ChampionName && target.Distance(x) < 400 && isKnockedUp(x));
            }       

            if (hit >= menu.Item("R_MEC", true).GetValue<Slider>().Value)
                R.Cast();
        }

        private bool isKnockedUp(Obj_AI_Hero x)
        {
            return (x.HasBuffOfType(BuffType.Knockup) || x.HasBuffOfType(BuffType.Knockback) || x.HasBuff("yasuoq3mis"));
        }

        private float BuffDurationLeft(Obj_AI_Hero target)
        {
            var firstOrDefault = target.Buffs.FirstOrDefault(buff => buff.Type.Equals(BuffType.Knockback) || buff.Type.Equals(BuffType.Knockup));
            if (firstOrDefault != null)
                return firstOrDefault.EndTime - Game.Time;

            return 0;
        }

        private void AutoQ()
        {
            if (!Q.IsReady() || !menu.Item("Q_Auto", true).GetValue<KeyBind>().Active || Environment.TickCount - E.LastCastAttemptT < 250 + Game.Ping)
                return;

            if (!ThirdQ())
            {
                var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);
                if (!target.IsValidTarget(Q.Range))
                    return;

                if (menu.Item("Q_UnderTower", true).GetValue<bool>() && target.UnderTurret(true))
                    return;

                if (!menu.Item("predMode", true).GetValue<bool>())
                {
                    if (Player.Distance(target) < 150)
                        Q.Cast(target.ServerPosition, packets());
                    else
                        Q.Cast(target, packets());
                }
                else
                {
                    CastBasicSkillShot(Q, Q.Range, TargetSelector.DamageType.Physical, GetHitchance("Combo"), menu.Item("Q_UnderTower", true).GetValue<bool>());
                }
            }
            else if (ThirdQ())
            {
                var target = TargetSelector.GetTarget(Q2.Range, TargetSelector.DamageType.Physical);
                if (!target.IsValidTarget(Q2.Range))
                    return;

                if (!menu.Item("predMode", true).GetValue<bool>())
                {
                    if (menu.Item("Q_UnderTower", true).GetValue<bool>() && target.UnderTurret(true))
                        return;

                    Q2.Cast(target, packets());
                }
                else
                {
                    CastBasicSkillShot(Q2, Q2.Range, TargetSelector.DamageType.Physical, GetHitchance("Combo"), menu.Item("Q_UnderTower", true).GetValue<bool>());
                }
            }
        }

        private void StackQ()
        {
            if (!Q.IsReady() || !menu.Item("Q_Stack", true).GetValue<KeyBind>().Active || ThirdQ())
                return;

            var minions = MinionManager.GetMinions(Player.ServerPosition, Q.Range, MinionTypes.All, MinionTeam.NotAlly);
            var enemies = ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsValidTarget(Q.Range) && !x.UnderTurret(true)).ToList();

            if (minions.Count > 0)
                Q.Cast(minions[0], packets());

            if (enemies.Count > 0)
                Q.Cast(enemies[0], packets());
        }


        private void Escape()
        {
            var minion = MinionManager.GetMinions(Player.ServerPosition, E.Range, MinionTypes.All, MinionTeam.NotAlly);

            foreach (var m in minion.Where(x=> CanCastE(x) && x.Distance(Game.CursorPos) < 500).OrderBy(x => x.Distance(Game.CursorPos)))
            {
                var dash = Player.ServerPosition + Vector3.Normalize(m.ServerPosition - Player.ServerPosition) * 475;

                if (Player.Distance(Game.CursorPos) > Player.Distance(dash) - 200)
                {
                    E.CastOnUnit(m, packets());
                    return;
                }
            }
        }
        private void SmartKs()
        {
            
            if (!menu.Item("smartKS", true).GetValue<bool>())
                return;

            foreach (Obj_AI_Hero target in ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsValidTarget(Q2.Range) && !x.HasBuffOfType(BuffType.Invulnerability)).OrderByDescending(GetComboDamage))
            {
                if (target != null)
                {
                    //E + Q
                    if (Player.Distance(target.ServerPosition) <= E.Range && (Player.GetSpellDamage(target, SpellSlot.E) + Player.GetSpellDamage(target, SpellSlot.Q)) >
                        target.Health + 20)
                    {
                        if (E.IsReady() && Q.IsReady() && EturretCheck(target))
                        {
                            E.Cast(target, packets());
                            Obj_AI_Hero target1 = target;
                            Utility.DelayAction.Add(200, () => Q.Cast(target1.ServerPosition));
                            return;
                        }
                    }

                    //Q
                    if ((Player.GetSpellDamage(target, SpellSlot.Q)) > target.Health + 20)
                    {
                        if (!ThirdQ() && target.IsValidTarget(Q.Range))
                        {
                            Q.Cast(target, packets());
                        }
                        else if(ThirdQ() && target.IsValidTarget(Q2.Range))
                        {
                            Q.Cast(target, packets());
                        }
                    }

                    //E
                    if (Player.Distance(target.ServerPosition) <= E.Range && (Player.GetSpellDamage(target, SpellSlot.E)) > target.Health + 20 && EturretCheck(target))
                    {
                        if (E.IsReady())
                        {
                            E.Cast(target, packets());
                            return;
                        }
                    }
                }
            }
        }

        private void LastHit()
        {
            var allMinionsQ = MinionManager.GetMinions(Player.ServerPosition, Q.Range, MinionTypes.All, MinionTeam.NotAlly);
            var allMinionsQ2 = MinionManager.GetMinions(Player.ServerPosition, Q2.Range, MinionTypes.All, MinionTeam.NotAlly);
            var allMinionsE = MinionManager.GetMinions(Player.ServerPosition, E.Range, MinionTypes.All, MinionTeam.NotAlly);

            var useQ = menu.Item("UseQLast", true).GetValue<bool>();
            var useE = menu.Item("UseELast", true).GetValue<bool>();


            if (useQ && Q.IsReady())
            {
                if (!ThirdQ())
                {
                    foreach (var minion in allMinionsQ)
                    {
                        if (Q.IsKillable(minion))
                            Q.Cast(minion, packets());
                    }
                }
                else
                {
                    foreach (var minion in allMinionsQ2)
                    {
                        if (Q.IsKillable(minion))
                            Q.Cast(minion, packets());
                    }
                }
            }

            if (useE && E.IsReady())
            {
                foreach (var minion in allMinionsE.Where(CanCastE))
                {
                    var dashVec = Player.ServerPosition + Vector3.Normalize(minion.ServerPosition - Player.ServerPosition) * 475;
                    if (!menu.Item("E_UnderTower_Farm", true).GetValue<bool>() && dashVec.UnderTurret(true))
                        continue;

                    var predHealth = HealthPrediction.GetHealthPrediction(minion, (int)(Player.Distance(minion) * 1000 / 2000));

                    if (predHealth <= Player.GetSpellDamage(minion, SpellSlot.E))
                        E.CastOnUnit(minion, packets());
                }
            }

        }

        private void Farm()
        {
            var allMinionsQ = MinionManager.GetMinions(Player.ServerPosition, Q.Range, MinionTypes.All, MinionTeam.NotAlly);
            var allMinionsQ2 = MinionManager.GetMinions(Player.ServerPosition, Q2.Range, MinionTypes.All, MinionTeam.NotAlly);
            var allMinionsE = MinionManager.GetMinions(Player.ServerPosition, E.Range, MinionTypes.All, MinionTeam.NotAlly);

            var useQ = menu.Item("UseQFarm", true).GetValue<bool>();
            var useE = menu.Item("UseEFarm", true).GetValue<bool>();
            var useQ3 = menu.Item("UseQ3Farm", true).GetValue<bool>();

            var min = menu.Item("LaneClear_useQ_minHit", true).GetValue<Slider>().Value;

            if (useQ && useE && Q.IsReady() && E.IsReady())
            {
                foreach (var minion in allMinionsE.Where(CanCastE))
                {
                    var dashVec = Player.ServerPosition + Vector3.Normalize(minion.ServerPosition - Player.ServerPosition) * 475;
                    var count = MinionManager.GetMinions(dashVec, 300, MinionTypes.All, MinionTeam.NotAlly).Count - 1;
                    
                    if (!menu.Item("E_UnderTower_Farm", true).GetValue<bool>() && dashVec.UnderTurret(true))
                        continue;

                    if (count >= min)
                    {
                        E.CastOnUnit(minion, packets());
                        Obj_AI_Base minion1 = minion;
                        Utility.DelayAction.Add(200, () => Q.Cast(minion1.ServerPosition, packets()));
                    }
                }
            }

            if (useQ && Q.IsReady())
            {
                if (!ThirdQ())
                {
                    var pred = Q.GetLineFarmLocation(allMinionsQ);

                    if (pred.MinionsHit >= min)
                        Q.Cast(pred.Position, packets());
                }
                else if(useQ3)
                {
                    var pred = Q.GetLineFarmLocation(allMinionsQ2);

                    if (pred.MinionsHit >= min)
                        Q.Cast(pred.Position, packets());
                }
            }

            if (useE && E.IsReady())
            {
                foreach (var minion in allMinionsE.Where(CanCastE))
                {
                    var dashVec = Player.ServerPosition + Vector3.Normalize(minion.ServerPosition - Player.ServerPosition) * 475;
                    if (!menu.Item("E_UnderTower_Farm", true).GetValue<bool>() && dashVec.UnderTurret(true))
                        continue;

                    var predHealth = HealthPrediction.GetHealthPrediction(minion, (int)(Player.Distance(minion) * 1000 / 2000));

                    if (predHealth <= Player.GetSpellDamage(minion, SpellSlot.E))
                        E.CastOnUnit(minion, packets());
                }
            }
        }

        private bool ThirdQ()
        {
            return Player.HasBuff("YasuoQ3W");
        }

        private bool CanCastE(Obj_AI_Base target)
        {
            return !target.HasBuff("YasuoDashWrapper");
        }

        protected override void Interrupter_OnPosibleToInterrupt(Obj_AI_Hero unit, Interrupter2.InterruptableTargetEventArgs spell)
        {
            if (spell.DangerLevel < Interrupter2.DangerLevel.Medium || unit.IsAlly || !Q.IsReady() || !ThirdQ() || !menu.Item("Interrupt", true).GetValue<bool>())
                return;

            if (unit.IsValidTarget(E.Range))
            {
                E.CastOnUnit(unit, packets());
                Utility.DelayAction.Add(200, () => Q.Cast(unit.Position));
            }
            else if(unit.IsValidTarget(Q2.Range))
            {
                Q2.Cast(unit);
            }
        }

        private Obj_SpellMissile _windWall;
        private Obj_SpellMissile _eSlide;

        protected override void GameObject_OnCreate(GameObject sender, EventArgs args2)
        {
            if (!(sender is Obj_SpellMissile) || !sender.IsValid)
                return;
            var args = (Obj_SpellMissile)sender;

            if (sender.Name != "missile")
            {
                if (menu.Item(args.SData.Name + "E", true) != null)
                {
                    if (menu.Item(args.SData.Name + "E", true).GetValue<bool>() && E.IsReady())
                    {
                        //Game.PrintChat("RAWR1");
                        _eSlide = args;
                        var minion = MinionManager.GetMinions(Player.ServerPosition, E.Range, MinionTypes.All,
                            MinionTeam.NotAlly);
                        if (Player.Distance(_eSlide.Position) < 800)
                        {
                            foreach (var m in minion.Where(CanCastE))
                            {
                                if (IsPassableE(m))
                                {
                                    E.CastOnUnit(m, packets());
                                    E.LastCastAttemptT = Environment.TickCount;
                                    _eSlide = null;
                                    return;
                                }
                            }
                        }
                    }
                }

                //Game.PrintChat(args.SData.Name);
                if (menu.Item(args.SData.Name + "W_Wall", true) != null)
                {
                    if (menu.Item(args.SData.Name + "W_Wall", true).GetValue<bool>() && W.IsReady())
                    {
                        //Game.PrintChat("RAWR1");
                        _windWall = args;

                        if (_windWall != null && W.IsReady())
                        {
                            if (Player.Distance(_windWall.Position) < 200)
                            {
                                W.Cast(_windWall.Position, packets());

                                var vec = Player.ServerPosition - (_windWall.Position - Player.ServerPosition)*50;

                                Player.IssueOrder(GameObjectOrder.MoveTo, vec);
                                _windWall = null;
                                _eSlide = null;
                            }
                        }
                    }
                }

            }
        }

        protected override void GameObject_OnDelete(GameObject sender, EventArgs args2)
        {
            if (!(sender is Obj_SpellMissile) || !sender.IsValid)
                return;
            var args = (Obj_SpellMissile)sender;

            if (sender.Name != "missile")
            {
                if (menu.Item(args.SData.Name + "W_Wall", true) != null)
                {
                    if (menu.Item(args.SData.Name + "W_Wall", true).GetValue<bool>())
                    {
                        _windWall = null;
                    }
                }
                if (menu.Item(args.SData.Name + "E", true) != null)
                {
                    if (menu.Item(args.SData.Name + "E", true).GetValue<bool>())
                    {
                        //Game.PrintChat("RAWR1");
                        _eSlide = null;
                    }
                }
            }
        }

        protected override void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base unit, GameObjectProcessSpellCastEventArgs args)
        {
            if (unit.IsEnemy && (unit is Obj_AI_Hero))
            {
                if (Player.Distance(args.End) > W.Range)
                    return;

                if (menu.Item(args.SData.Name + "E", true) != null) 
                { 
                    if (menu.Item(args.SData.Name + "E", true).GetValue<bool>() && (Player.Distance(args.Start) < 1000 || Player.Distance(args.End) < 1000))
                    {
                        var minion = MinionManager.GetMinions(Player.ServerPosition, E.Range, MinionTypes.All, MinionTeam.NotAlly);

                        foreach (var m in minion.Where(CanCastE))
                        {
                            if (IsPassableE(m))
                            {
                                E.CastOnUnit(m, packets());
                                E.LastCastAttemptT = Environment.TickCount;
                                return;
                            }
                        }

                    }
                }

                if (menu.Item(args.SData.Name + "W_Wall", true) != null)
                {
                    if (menu.Item(args.SData.Name + "W_Wall", true).GetValue<bool>() && W.IsReady() &&
                        (Player.Distance(args.Start) < 1000 || Player.Distance(args.End) < 1000))
                    {
                        W.Cast(args.Start, packets());

                        var vec = Player.ServerPosition - (args.Start - Player.ServerPosition)*50;

                        Player.IssueOrder(GameObjectOrder.MoveTo, vec);
                        return;
                    }
                }

            }

            if (unit.IsMe)
            {
                SpellSlot castedSlot = ObjectManager.Player.GetSpellSlot(args.SData.Name);

                if (castedSlot == SpellSlot.E)
                {
                    E.LastCastAttemptT = Environment.TickCount;
                }
            }
        }

        protected override void Game_OnGameUpdate(EventArgs args)
        {
            //check if player is dead
            if (Player.IsDead) return;

            if (_windWall != null && W.IsReady())
            {
                if (Player.Distance(_windWall.Position) < 400)
                {
                    //Game.PrintChat("RAWR");
                    W.Cast(_windWall.Position, packets());

                    var vec = Player.ServerPosition - (_windWall.Position - Player.ServerPosition) * 50;

                    Player.IssueOrder(GameObjectOrder.MoveTo, vec);
                    _windWall = null;
                }
            }

            //rmec
            Cast_MecR();

            //smart ks
            SmartKs();

            if (menu.Item("Flee", true).GetValue<KeyBind>().Active)
            {
                Escape();
            }
            else if (menu.Item("ComboActive", true).GetValue<KeyBind>().Active)
            {
                Combo();
            }
            else
            {
                //auto Q harass
                AutoQ();

                if (menu.Item("LastHit", true).GetValue<KeyBind>().Active)
                    LastHit();

                if (menu.Item("LaneClearActive", true).GetValue<KeyBind>().Active)
                    Farm();

                if (menu.Item("HarassActiveT", true).GetValue<KeyBind>().Active)
                    Harass();

                if (menu.Item("HarassActive", true).GetValue<KeyBind>().Active)
                    Harass();
            }

            //stack Q
            StackQ();
        }

        private bool IsPassableE(Obj_AI_Base m)
        {
            for (int i = 238; i <= 475; i += 238) { 
                var dashVec = Player.ServerPosition + Vector3.Normalize(m.ServerPosition - Player.ServerPosition) * i;
            
                Object[] obj = VectorPointProjectionOnLineSegment(dashVec.To2D(), _eSlide.Position.To2D(), _eSlide.EndPosition.To2D());
                var isOnseg = (bool)obj[2];

                var pointLine = (Vector2)obj[1];
                if (isOnseg || dashVec.UnderTurret(true) || dashVec.Distance(pointLine.To3D()) < _eSlide.SData.LineWidth + 20)
                {
                    return false;
                }
            }
            return true;
        }

        protected override void Drawing_OnDraw(EventArgs args)
        {
            if (menu.Item("Draw_Disabled", true).GetValue<bool>())
                return;

            if (menu.Item("Draw_Q", true).GetValue<bool>() && !ThirdQ())
                if (Q.Level > 0)
                    Render.Circle.DrawCircle(Player.Position, Q.Range, Q.IsReady() ? Color.Green : Color.Red);

            if (menu.Item("Draw_Q2", true).GetValue<bool>() && ThirdQ())
                if (Q.Level > 0)
                    Render.Circle.DrawCircle(Player.Position, Q2.Range, Q.IsReady() ? Color.Green : Color.Red);

            if (menu.Item("Draw_E", true).GetValue<bool>())
                if (E.Level > 0)
                    Render.Circle.DrawCircle(Player.Position, E.Range, E.IsReady() ? Color.Green : Color.Red);

            if (menu.Item("Draw_R", true).GetValue<bool>())
                if (R.Level > 0)
                    Render.Circle.DrawCircle(Player.Position, R.Range, R.IsReady() ? Color.Green : Color.Red);

            if (menu.Item("Draw_AutoQ", true).GetValue<bool>())
            {
                Vector2 wts = Drawing.WorldToScreen(Player.Position);
                if (menu.Item("Q_Auto", true).GetValue<KeyBind>().Active)
                    Drawing.DrawText(wts[0] - 20, wts[1], Color.White, "Auto Q Enabled");
                else
                    Drawing.DrawText(wts[0] - 20, wts[1], Color.Red, "Auto Q Disabled");

                if(menu.Item("E_Turret", true).GetValue<KeyBind>().Active)
                    Drawing.DrawText(wts[0] - 20, wts[1] - 20, Color.White, "Don't E Turret On");
                else
                    Drawing.DrawText(wts[0] - 20, wts[1]- 20, Color.Red, "Don't E Turret off");
            }
        }

    }
}