﻿using System;
using System.Linq;
using System.Collections.Generic;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using SharpDX.Direct3D9;
using Color = System.Drawing.Color;
using Collision = LeagueSharp.Common.Collision;

namespace Sharpshooter.Champions
{
    public static class Kalista
    {
        static Obj_AI_Hero Player { get { return ObjectManager.Player; } }
        static Orbwalking.Orbwalker Orbwalker { get { return SharpShooter.Orbwalker; } }

        static Spell Q, W, E, R;

        public static void Load()
        {
            Q = new Spell(SpellSlot.Q, 1180f);
            W = new Spell(SpellSlot.W, 5200f);
            E = new Spell(SpellSlot.E, 1000f);
            R = new Spell(SpellSlot.R, 1400f);

            Q.SetSkillshot(0.25f, 30f, 1700f, true, SkillshotType.SkillshotLine);

            var drawDamageMenu = new MenuItem("Draw_RDamage", "显示 E伤害", true).SetValue(true);
            var drawFill = new MenuItem("Draw_Fill", "显示 E充能伤害", true).SetValue(new Circle(true, Color.FromArgb(90, 255, 169, 4)));

            SharpShooter.Menu.SubMenu("Combo").AddItem(new MenuItem("comboUseQ", "使用 Q", true).SetValue(true));
            SharpShooter.Menu.SubMenu("Combo").AddItem(new MenuItem("comboUseE", "使用 E", true).SetValue(true));

            SharpShooter.Menu.SubMenu("Harass").AddItem(new MenuItem("harassUseQ", "使用 Q", true).SetValue(true));
            SharpShooter.Menu.SubMenu("Harass").AddItem(new MenuItem("harassMana", "蓝量 % >", true).SetValue(new Slider(50, 0, 100)));

            SharpShooter.Menu.SubMenu("Laneclear").AddItem(new MenuItem("laneclearUseQ", "使用 Q", true).SetValue(true));
            SharpShooter.Menu.SubMenu("Laneclear").AddItem(new MenuItem("laneclearQnum", "小兵数 >=", true).SetValue(new Slider(3, 1, 5)));
            SharpShooter.Menu.SubMenu("Laneclear").AddItem(new MenuItem("laneclearUseE", "使用 E", true).SetValue(true));
            SharpShooter.Menu.SubMenu("Laneclear").AddItem(new MenuItem("laneclearEnum", "小兵数 >=", true).SetValue(new Slider(2, 1, 5)));
            SharpShooter.Menu.SubMenu("Laneclear").AddItem(new MenuItem("laneclearMana", "蓝量 % >", true).SetValue(new Slider(60, 0, 100)));

            SharpShooter.Menu.SubMenu("Jungleclear").AddItem(new MenuItem("jungleclearUseQ", "使用 Q", true).SetValue(true));
            SharpShooter.Menu.SubMenu("Jungleclear").AddItem(new MenuItem("jungleclearUseE", "使用 E", true).SetValue(true));
            SharpShooter.Menu.SubMenu("Jungleclear").AddItem(new MenuItem("jungleclearMana", "蓝量 % >", true).SetValue(new Slider(20, 0, 100)));

            SharpShooter.Menu.SubMenu("Misc").AddItem(new MenuItem("killsteal", "抢人头 E", true).SetValue(true));
            SharpShooter.Menu.SubMenu("Misc").AddItem(new MenuItem("mobsteal", "抢野 E", true).SetValue(true));
            SharpShooter.Menu.SubMenu("Misc").AddItem(new MenuItem("lasthitassist", "补刀 E)", true).SetValue(true));
            SharpShooter.Menu.SubMenu("Misc").AddItem(new MenuItem("soulboundsaver", "自动R", true).SetValue(true));

            SharpShooter.Menu.SubMenu("Drawings").AddItem(new MenuItem("drawingAA", "AA 范围", true).SetValue(new Circle(true, Color.FromArgb(0, 230, 255))));
            SharpShooter.Menu.SubMenu("Drawings").AddItem(new MenuItem("drawingQ", "Q 范围", true).SetValue(new Circle(true, Color.FromArgb(0, 230, 255))));
            SharpShooter.Menu.SubMenu("Drawings").AddItem(new MenuItem("drawingW", "W 范围", true).SetValue(new Circle(false, Color.FromArgb(0, 230, 255))));
            SharpShooter.Menu.SubMenu("Drawings").AddItem(new MenuItem("drawingE", "E 范围", true).SetValue(new Circle(true, Color.FromArgb(0, 230, 255))));
            SharpShooter.Menu.SubMenu("Drawings").AddItem(new MenuItem("drawingR", "R 范围", true).SetValue(new Circle(false, Color.FromArgb(0, 230, 255))));

            SharpShooter.Menu.SubMenu("Drawings").AddItem(drawDamageMenu);
            SharpShooter.Menu.SubMenu("Drawings").AddItem(drawFill);

            DamageIndicator.DamageToUnit = GetComboDamage;
            DamageIndicator.Enabled = drawDamageMenu.GetValue<bool>();
            DamageIndicator.Fill = drawFill.GetValue<Circle>().Active;
            DamageIndicator.FillColor = drawFill.GetValue<Circle>().Color;

            drawDamageMenu.ValueChanged +=
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

            Game.OnUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Obj_AI_Hero.OnProcessSpellCast += Obj_AI_Hero_OnProcessSpellCast;
            Orbwalking.OnNonKillableMinion += Orbwalking_OnNonKillableMinion;
        }

        static void Game_OnGameUpdate(EventArgs args)
        {
            if (Player.IsDead)
                return;

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
                Combo();

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed)
                Harass();

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear)
            {
                Laneclear();
                Jungleclear();
            }

            Killsteal();
            Mobsteal();
        }

        static void Drawing_OnDraw(EventArgs args)
        {
            if (Player.IsDead)
                return;

            var drawingAA = SharpShooter.Menu.Item("drawingAA", true).GetValue<Circle>();
            var drawingQ = SharpShooter.Menu.Item("drawingQ", true).GetValue<Circle>();
            var drawingW = SharpShooter.Menu.Item("drawingW", true).GetValue<Circle>();
            var drawingE = SharpShooter.Menu.Item("drawingE", true).GetValue<Circle>();
            var drawingR = SharpShooter.Menu.Item("drawingR", true).GetValue<Circle>();

            if (drawingAA.Active)
                Render.Circle.DrawCircle(Player.Position, Orbwalking.GetRealAutoAttackRange(Player), drawingAA.Color);

            if (Q.IsReady() && drawingQ.Active)
                Render.Circle.DrawCircle(Player.Position, Q.Range, drawingQ.Color);

            if (W.IsReady() && drawingW.Active)
                Render.Circle.DrawCircle(Player.Position, W.Range, drawingW.Color);

            if (E.IsReady() && drawingE.Active)
                Render.Circle.DrawCircle(Player.Position, E.Range, drawingE.Color);

            if (R.IsReady() && drawingR.Active)
                Render.Circle.DrawCircle(Player.Position, R.Range, drawingR.Color);
        }

        static void Obj_AI_Hero_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsMe && args.SData.Name == "KalistaExpungeWrapper")
                    Utility.DelayAction.Add(250, Orbwalking.ResetAutoAttackTimer);

            if (SharpShooter.Menu.Item("soulboundsaver", true).GetValue<Boolean>() && R.IsReady())
            {
                if (sender.Type == GameObjectType.obj_AI_Hero && sender.IsEnemy)
                {
                    var soulboundhero = HeroManager.Allies.FirstOrDefault(hero => hero.HasBuff("kalistacoopstrikeally", true) && args.Target.NetworkId == hero.NetworkId && hero.HealthPercentage() <= 15);

                    if (R.CanCast(soulboundhero))
                        R.Cast();
                }
            }
        }

        static void Killsteal()
        {
            if (!SharpShooter.Menu.Item("killsteal", true).GetValue<Boolean>() || !E.IsReady())
                return;

            var target = HeroManager.Enemies.FirstOrDefault(x => !x.HasBuffOfType(BuffType.Invulnerability) && !x.HasBuffOfType(BuffType.SpellShield) && E.CanCast(x) && (x.Health + (x.HPRegenRate / 2)) <= E.GetDamage(x));

            if (E.CanCast(target))
                E.Cast();
        }

        static void Mobsteal()
        {
            if (!SharpShooter.Menu.Item("mobsteal", true).GetValue<Boolean>() || !E.IsReady())
                return;

            var Mob = MinionManager.GetMinions(Player.ServerPosition, E.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth).FirstOrDefault(x => x.Health + (x.HPRegenRate / 2) <= E.GetDamage(x));

            if (E.CanCast(Mob))
                E.Cast();

            var Minion = MinionManager.GetMinions(Player.ServerPosition, E.Range, MinionTypes.All, MinionTeam.Enemy, MinionOrderTypes.MaxHealth).FirstOrDefault(x => x.Health <= E.GetDamage(x) && (x.SkinName.ToLower().Contains("siege") || x.SkinName.ToLower().Contains("super")));

            if (E.CanCast(Minion))
                E.Cast();
        }

        static float GetComboDamage(Obj_AI_Base enemy)
        {
            float damage = 0;

            if (E.IsReady())
                damage += E.GetDamage(enemy);

            return damage;
        }

        static List<Obj_AI_Base> Q_GetCollisionMinions(Obj_AI_Hero source, Vector3 targetposition)
        {
            var input = new PredictionInput
            {
                Unit = source,
                Radius = Q.Width,
                Delay = Q.Delay,
                Speed = Q.Speed,
            };

            input.CollisionObjects[0] = CollisionableObjects.Minions;

            return Collision.GetCollision(new List<Vector3> { targetposition }, input).OrderBy(obj => obj.Distance(source, false)).ToList();
        }

        static void Orbwalking_OnNonKillableMinion(AttackableUnit minion)
        {
            if (!SharpShooter.Menu.Item("lasthitassist", true).GetValue<Boolean>())
                return;

            if (E.CanCast((Obj_AI_Base)minion) && minion.Health <= E.GetDamage((Obj_AI_Base)minion))
                E.Cast();
        }

        static void Combo()
        {
            if (!Orbwalking.CanMove(1))
                return;

            if (SharpShooter.Menu.Item("comboUseQ", true).GetValue<Boolean>())
            {
                var Qtarget = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical, true);

                if (Q.CanCast(Qtarget) && Q.GetPrediction(Qtarget).Hitchance >= HitChance.VeryHigh && !Player.IsWindingUp && !Player.IsDashing())
                    Q.Cast(Qtarget);
            }

            if (SharpShooter.Menu.Item("comboUseE", true).GetValue<Boolean>() && E.IsReady())
            {
                var Minion = MinionManager.GetMinions(Player.ServerPosition, E.Range, MinionTypes.All, MinionTeam.Enemy).Where(x => x.Health <= E.GetDamage(x)).OrderBy(x => x.Health).FirstOrDefault();
                var Target = HeroManager.Enemies.Where(x => E.CanCast(x) && E.GetDamage(x) >= 1 && !x.HasBuffOfType(BuffType.Invulnerability) && !x.HasBuffOfType(BuffType.SpellShield)).OrderByDescending(x => E.GetDamage(x)).FirstOrDefault();

                if (Target.Health <= E.GetDamage(Target) || (E.CanCast(Minion) && E.CanCast(Target)))
                    E.Cast();
            }
        }

        static void Harass()
        {
            if (!Orbwalking.CanMove(1) || !(Player.ManaPercentage() > SharpShooter.Menu.Item("harassMana", true).GetValue<Slider>().Value))
                return;

            if (SharpShooter.Menu.Item("harassUseQ", true).GetValue<Boolean>())
            {
                var Qtarget = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical, true);

                if (Q.CanCast(Qtarget) && Q.GetPrediction(Qtarget).Hitchance >= HitChance.VeryHigh && !Player.IsWindingUp && !Player.IsDashing())
                    Q.Cast(Qtarget);
            }
        }

        static void Laneclear()
        {
            if (!Orbwalking.CanMove(1) || !(Player.ManaPercentage() > SharpShooter.Menu.Item("laneclearMana", true).GetValue<Slider>().Value))
                return;

            var Minions = MinionManager.GetMinions(Player.ServerPosition, E.Range, MinionTypes.All, MinionTeam.Enemy);

            if (Minions.Count <= 0)
                return;

            if (SharpShooter.Menu.Item("laneclearUseQ", true).GetValue<Boolean>() && Q.IsReady())
            {
                //-------------------------------------------------------------------------------------------------------------------------------
                foreach (var minion in Minions.Where(x => x.Health <= Q.GetDamage(x)))
                {
                    var killcount = 0;

                    foreach (var colminion in Q_GetCollisionMinions(Player, Player.ServerPosition.Extend(minion.ServerPosition, Q.Range)))
                    {
                        if (colminion.Health <= Q.GetDamage(colminion))
                            killcount++;
                        else
                            break;
                    }

                    if (killcount >= SharpShooter.Menu.Item("laneclearQnum", true).GetValue<Slider>().Value)
                    {
                        if (!Player.IsWindingUp && !Player.IsDashing())
                        { 
                            Q.Cast(minion);
                            break;
                        }
                    }
                }
                //-------------------------------------------------------------------------------------------------------------------------------
            }

            if (SharpShooter.Menu.Item("laneclearUseE", true).GetValue<Boolean>() && E.IsReady())
            {
                var minionkillcount = 0;

                foreach (var Minion in Minions.Where(x => E.CanCast(x) && x.Health <= E.GetDamage(x))){minionkillcount++;}

                if (minionkillcount >= SharpShooter.Menu.Item("laneclearEnum", true).GetValue<Slider>().Value)
                    E.Cast();
            }
        }

        static void Jungleclear()
        {
            if (!Orbwalking.CanMove(1) || !(Player.ManaPercentage() > SharpShooter.Menu.Item("jungleclearMana", true).GetValue<Slider>().Value))
                return;

            var Mobs = MinionManager.GetMinions(Player.ServerPosition, Orbwalking.GetRealAutoAttackRange(Player) + 100, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);

            if (Mobs.Count <= 0)
                return;

            if (SharpShooter.Menu.Item("jungleclearUseQ", true).GetValue<Boolean>() && Q.CanCast(Mobs[0]))
                Q.Cast(Mobs[0]);

            if (SharpShooter.Menu.Item("jungleclearUseE", true).GetValue<Boolean>() && E.CanCast(Mobs[0]))
            {
                if (Mobs[0].Health + (Mobs[0].HPRegenRate/2) <= E.GetDamage(Mobs[0]))
                    E.Cast();
            }
        }
    }
}
