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
    public static class Ashe
    {
        static Obj_AI_Hero Player { get { return ObjectManager.Player; } }
        static Orbwalking.Orbwalker Orbwalker { get { return SharpShooter.Orbwalker; } }

        static Spell Q, W, E, R;

        static Obj_AI_Hero rTarget;

        static bool QisActive { get { return Player.HasBuff("FrostShot", true); } }

        public static void Load()
        {
            Q = new Spell(SpellSlot.Q);
            W = new Spell(SpellSlot.W, 1200f);
            E = new Spell(SpellSlot.E);
            R = new Spell(SpellSlot.R, 2500f);

            W.SetSkillshot(0.25f, 24.32f * (float)(Math.PI / 180), 902f, true, SkillshotType.SkillshotCone);
            R.SetSkillshot(0.25f, 130f, 1600f, false, SkillshotType.SkillshotLine);

            SharpShooter.Menu.SubMenu("Combo").AddItem(new MenuItem("comboUseQ", "使用 Q", true).SetValue(true));
            SharpShooter.Menu.SubMenu("Combo").AddItem(new MenuItem("comboUseW", "使用 W", true).SetValue(true));
            SharpShooter.Menu.SubMenu("Combo").AddItem(new MenuItem("comboUseR", "使用 R", true).SetValue(true));

            SharpShooter.Menu.SubMenu("Harass").AddItem(new MenuItem("harassUseQ", "使用 Q", true).SetValue(true));
            SharpShooter.Menu.SubMenu("Harass").AddItem(new MenuItem("harassUseW", "使用 W", true).SetValue(true));
            SharpShooter.Menu.SubMenu("Harass").AddItem(new MenuItem("harassMana", "蓝量 % >", true).SetValue(new Slider(50, 0, 100)));

            SharpShooter.Menu.SubMenu("Laneclear").AddItem(new MenuItem("laneclearUseW", "使用 W", true).SetValue(true));
            SharpShooter.Menu.SubMenu("Laneclear").AddItem(new MenuItem("laneclearMana", "蓝量 % >", true).SetValue(new Slider(60, 0, 100)));

            SharpShooter.Menu.SubMenu("Jungleclear").AddItem(new MenuItem("jungleclearUseW", "使用 W", true).SetValue(true));
            SharpShooter.Menu.SubMenu("Jungleclear").AddItem(new MenuItem("jungleclearMana", "蓝量 % >", true).SetValue(new Slider(20, 0, 100)));

            SharpShooter.Menu.SubMenu("Misc").AddItem(new MenuItem("antigapcloser", "阻止突进", true).SetValue(true));
            SharpShooter.Menu.SubMenu("Misc").AddItem(new MenuItem("autointerrupt", "打断法术", true).SetValue(true));
            SharpShooter.Menu.SubMenu("Misc").AddItem(new MenuItem("AutoR", "自动R丨目标被控制", true).SetValue(true));

            SharpShooter.Menu.SubMenu("Drawings").AddItem(new MenuItem("drawingAA", "AA 范围", true).SetValue(new Circle(true, Color.DodgerBlue)));
            SharpShooter.Menu.SubMenu("Drawings").AddItem(new MenuItem("drawingW", "W 范围", true).SetValue(new Circle(true, Color.DodgerBlue)));
            SharpShooter.Menu.SubMenu("Drawings").AddItem(new MenuItem("drawingE", "E 范围", true).SetValue(new Circle(true, Color.DodgerBlue)));
            SharpShooter.Menu.SubMenu("Drawings").AddItem(new MenuItem("drawingR", "R 范围", true).SetValue(new Circle(true, Color.DodgerBlue)));

            Game.OnGameUpdate += Game_OnGameUpdate;
            Orbwalking.BeforeAttack += Orbwalking_BeforeAttack;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Interrupter2.OnInterruptableTarget += Interrupter2_OnInterruptableTarget;
            Drawing.OnDraw += Drawing_OnDraw;
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

            AutoR();
        }

        static void Drawing_OnDraw(EventArgs args)
        {
            if (Player.IsDead)
                return;

            var drawingAA = SharpShooter.Menu.Item("drawingAA", true).GetValue<Circle>();
            var drawingW = SharpShooter.Menu.Item("drawingW", true).GetValue<Circle>();
            var drawingE = SharpShooter.Menu.Item("drawingE", true).GetValue<Circle>();
            var drawingR = SharpShooter.Menu.Item("drawingR", true).GetValue<Circle>();

            if (drawingAA.Active)
                Render.Circle.DrawCircle(Player.Position, Orbwalking.GetRealAutoAttackRange(Player), drawingAA.Color);

            if (W.IsReady() && drawingW.Active)
                Render.Circle.DrawCircle(Player.Position, W.Range, drawingW.Color);

            if (E.IsReady() && drawingE.Active)
            {
                Render.Circle.DrawCircle(Player.Position, 1750 + (E.Level * 750), drawingE.Color);
            }
                
            if (R.IsReady() && drawingR.Active)
                Render.Circle.DrawCircle(Player.Position, Orbwalking.GetRealAutoAttackRange(Player)+50, drawingR.Color);
        }

        static void Orbwalking_BeforeAttack(Orbwalking.BeforeAttackEventArgs args)
        {
            if (args.Target is Obj_AI_Hero && !QisActive)
            {
                if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo && SharpShooter.Menu.Item("comboUseQ", true).GetValue<Boolean>())
                    Q.Cast();

                if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed && SharpShooter.Menu.Item("harassUseQ", true).GetValue<Boolean>())
                    Q.Cast();
            }

            if (!(args.Target is Obj_AI_Hero) && QisActive)
                Q.Cast();

        }

        static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (!SharpShooter.Menu.Item("antigapcloser", true).GetValue<Boolean>() || Player.IsDead)
                return;

            if (gapcloser.Sender.IsValidTarget(1000))
            {
                if (R.CanCast(gapcloser.Sender))
                    R.Cast(gapcloser.Sender);

                Render.Circle.DrawCircle(gapcloser.Sender.Position, gapcloser.Sender.BoundingRadius, Color.Gold, 5);
                var targetpos = Drawing.WorldToScreen(gapcloser.Sender.Position);
                Drawing.DrawText(targetpos[0] - 40, targetpos[1] + 20, Color.Gold, "Gapcloser");
            }
        }

        static void Interrupter2_OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (!SharpShooter.Menu.Item("autointerrupt", true).GetValue<Boolean>() || Player.IsDead)
                return;

            if (sender.IsValidTarget(1500))
            {
                if (R.CanCast(sender))
                    R.Cast(sender);

                Render.Circle.DrawCircle(sender.Position, sender.BoundingRadius, Color.Gold, 5);
                var targetpos = Drawing.WorldToScreen(sender.Position);
                Drawing.DrawText(targetpos[0] - 40, targetpos[1] + 20, Color.Gold, "Interrupt");
            }
        }

        static void AutoR()
        {
            if (!SharpShooter.Menu.Item("AutoR", true).GetValue<Boolean>())
                return;

            foreach (Obj_AI_Hero target in HeroManager.Enemies.Where(x => x.IsValidTarget(R.Range)))
            {
                if (R.CanCast(target) && UnitIsImmobileUntil(target) >= R.Delay + (Player.Distance(target, false) / R.Speed) && CollisionCheck(Player, target, R.Width))
                    R.Cast(target);
            }
        }

        static double UnitIsImmobileUntil(Obj_AI_Base unit)
        {
            var result =
                unit.Buffs.Where(
                    buff =>
                        buff.IsActive && Game.Time <= buff.EndTime &&
                        (buff.Type == BuffType.Charm || buff.Type == BuffType.Knockup || buff.Type == BuffType.Stun ||
                         buff.Type == BuffType.Suppression || buff.Type == BuffType.Snare))
                    .Aggregate(0d, (current, buff) => Math.Max(current, buff.EndTime));
            return (result - Game.Time);
        }

        static bool CollisionCheck(Obj_AI_Hero source, Obj_AI_Hero target, float width)
        {
            var input = new PredictionInput
            {
                Radius = width,
                Unit = source,
            };

            input.CollisionObjects[0] = CollisionableObjects.Heroes;
            input.CollisionObjects[1] = CollisionableObjects.YasuoWall;

            return !Collision.GetCollision(new List<Vector3> { target.ServerPosition }, input).Where(x => x.NetworkId != x.NetworkId).Any();
        }

        static void Combo()
        {
            if (!Orbwalking.CanMove(1))
                return;

            if (SharpShooter.Menu.Item("comboUseW", true).GetValue<Boolean>())
            {
                var Wtarget = TargetSelector.GetTarget(W.Range, TargetSelector.DamageType.Physical, true);

                if(W.CanCast(Wtarget) && W.GetPrediction(Wtarget).Hitchance >= HitChance.VeryHigh)
                    W.Cast(Wtarget);
            }
                
            if (SharpShooter.Menu.Item("comboUseR", true).GetValue<Boolean>())
            {
                var Rtarget = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Physical, true);

                if (R.IsReady() && Rtarget.IsValidTarget(Orbwalking.GetRealAutoAttackRange(Player) + 50) && !Rtarget.HasBuffOfType(BuffType.SpellImmunity) && R.GetPrediction(Rtarget).Hitchance >= HitChance.VeryHigh && CollisionCheck(Player, Rtarget, R.Width))
                    R.Cast(Rtarget);
            }
                
        }

        static void Harass()
        {
            if (!Orbwalking.CanMove(1) || !(Player.ManaPercentage() > SharpShooter.Menu.Item("harassMana", true).GetValue<Slider>().Value))
                return;

            if (SharpShooter.Menu.Item("harassUseW", true).GetValue<Boolean>())
            {
                var Wtarget = TargetSelector.GetTarget(W.Range, TargetSelector.DamageType.Physical, true);

                if(W.CanCast(Wtarget) && W.GetPrediction(Wtarget).Hitchance >= HitChance.VeryHigh)
                    W.Cast(Wtarget);
            }
        }

        static void Laneclear()
        {
            if (!Orbwalking.CanMove(1) || !(Player.ManaPercentage() > SharpShooter.Menu.Item("laneclearMana", true).GetValue<Slider>().Value))
                return;

            var Minions = MinionManager.GetMinions(Player.ServerPosition, W.Range, MinionTypes.All, MinionTeam.Enemy);

            if (Minions.Count <= 0)
                return;

            if (W.IsReady() && SharpShooter.Menu.Item("laneclearUseW", true).GetValue<Boolean>())
            {
                var Farmloc = W.GetLineFarmLocation(Minions);

                if (Farmloc.MinionsHit >= 3)
                    W.Cast(Farmloc.Position);
            }
        }

        static void Jungleclear()
        {
            if (!Orbwalking.CanMove(1) || !(Player.ManaPercentage() > SharpShooter.Menu.Item("jungleclearMana", true).GetValue<Slider>().Value))
                return;

            var Mobs = MinionManager.GetMinions(Player.ServerPosition, Orbwalking.GetRealAutoAttackRange(Player) + 100, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);

            if (Mobs.Count < 1)
                return;

            if (W.IsReady() && SharpShooter.Menu.Item("jungleclearUseW", true).GetValue<Boolean>())
                W.Cast(Mobs[0].Position);
        }
    }
}
