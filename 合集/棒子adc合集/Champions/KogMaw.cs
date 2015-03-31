﻿using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using SharpDX.Direct3D9;
using Color = System.Drawing.Color;

namespace Sharpshooter.Champions
{
    public static class KogMaw
    {
        static Obj_AI_Hero Player { get { return ObjectManager.Player; } }
        static Orbwalking.Orbwalker Orbwalker { get { return SharpShooter.Orbwalker; } }

        static Spell Q, W, E, R;

        static readonly int DefaltRange = 565;

        //W 1 = 695
        //W 2 = 715
        //W 3 = 735
        //W 4 = 755
        //W 5 = 775
        static int GetWActiveRange { get { return DefaltRange + 110 + (W.Level * 20); } }

        //R 1 = 1200
       // R 2 = 1500
        //R 3 = 1700
        static int GetRRange { get { return  900 + (R.Level * 300); } }

        public static void Load()
        {
            Q = new Spell(SpellSlot.Q, 900f);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E, 1200f);
            R = new Spell(SpellSlot.R);

            Q.SetSkillshot(0.25f, 60f, 1650f, true, SkillshotType.SkillshotLine);
            E.SetSkillshot(0.25f, 110f, 1400f, false, SkillshotType.SkillshotLine);
            R.SetSkillshot(1.2f, 120f, float.MaxValue, false, SkillshotType.SkillshotCircle);

            var drawDamageMenu = new MenuItem("Draw_RDamage", "显示 R 伤害", true).SetValue(true);
            var drawFill = new MenuItem("Draw_Fill", "显示 R 充能伤害", true).SetValue(new Circle(true, Color.FromArgb(90, 255, 169, 4)));

            SharpShooter.Menu.SubMenu("Combo").AddItem(new MenuItem("comboUseQ", "使用 Q", true).SetValue(true));
            SharpShooter.Menu.SubMenu("Combo").AddItem(new MenuItem("comboUseW", "使用 W", true).SetValue(true));
            SharpShooter.Menu.SubMenu("Combo").AddItem(new MenuItem("comboUseE", "使用 E", true).SetValue(true));
            SharpShooter.Menu.SubMenu("Combo").AddItem(new MenuItem("comboUseR", "使用 R", true).SetValue(true));

            SharpShooter.Menu.SubMenu("Harass").AddItem(new MenuItem("harassUseQ", "使用 Q", true).SetValue(true));
            SharpShooter.Menu.SubMenu("Harass").AddItem(new MenuItem("harassUseW", "使用 W", true).SetValue(true));
            SharpShooter.Menu.SubMenu("Harass").AddItem(new MenuItem("harassUseE", "使用 E", true).SetValue(true));
            SharpShooter.Menu.SubMenu("Harass").AddItem(new MenuItem("harassUseR", "使用 R", true).SetValue(true));
            SharpShooter.Menu.SubMenu("Harass").AddItem(new MenuItem("harassMana", "蓝量 % >", true).SetValue(new Slider(50, 0, 100)));

            SharpShooter.Menu.SubMenu("Laneclear").AddItem(new MenuItem("laneclearUseW", "使用 W", true).SetValue(false));
            SharpShooter.Menu.SubMenu("Laneclear").AddItem(new MenuItem("laneclearUseE", "使用 E", true).SetValue(true));
            SharpShooter.Menu.SubMenu("Laneclear").AddItem(new MenuItem("laneclearUseR", "使用 R", true).SetValue(true));
            SharpShooter.Menu.SubMenu("Laneclear").AddItem(new MenuItem("laneclearMana", "蓝量 % >", true).SetValue(new Slider(60, 0, 100)));

            SharpShooter.Menu.SubMenu("Jungleclear").AddItem(new MenuItem("jungleclearUseQ", "使用 Q", true).SetValue(true));
            SharpShooter.Menu.SubMenu("Jungleclear").AddItem(new MenuItem("jungleclearUseW", "使用 W", true).SetValue(true));
            SharpShooter.Menu.SubMenu("Jungleclear").AddItem(new MenuItem("jungleclearUseE", "使用 E", true).SetValue(true));
            SharpShooter.Menu.SubMenu("Jungleclear").AddItem(new MenuItem("jungleclearUseR", "使用 R", true).SetValue(true));
            SharpShooter.Menu.SubMenu("Jungleclear").AddItem(new MenuItem("jungleclearMana", "蓝量 % >", true).SetValue(new Slider(20, 0, 100)));

            SharpShooter.Menu.SubMenu("Drawings").AddItem(new MenuItem("drawingAA", "AA 范围", true).SetValue(new Circle(true, Color.FromArgb(0, 216, 255))));
            SharpShooter.Menu.SubMenu("Drawings").AddItem(new MenuItem("drawingQ", "Q 范围", true).SetValue(new Circle(false, Color.FromArgb(0, 216, 255))));
            SharpShooter.Menu.SubMenu("Drawings").AddItem(new MenuItem("drawingW", "W 范围", true).SetValue(new Circle(true, Color.FromArgb(0, 216, 255))));
            SharpShooter.Menu.SubMenu("Drawings").AddItem(new MenuItem("drawingE", "E 范围", true).SetValue(new Circle(false, Color.FromArgb(0, 216, 255))));
            SharpShooter.Menu.SubMenu("Drawings").AddItem(new MenuItem("drawingR", "R 范围", true).SetValue(new Circle(true, Color.FromArgb(0, 216, 255))));

            SharpShooter.Menu.SubMenu("Drawings").AddItem(drawDamageMenu);
            SharpShooter.Menu.SubMenu("Drawings").AddItem(drawFill);

            DamageIndicator.DamageToUnit = GetComboDamage;
            DamageIndicator.Enabled = drawDamageMenu.GetValue<Boolean>();
            DamageIndicator.Fill = drawFill.GetValue<Circle>().Active;
            DamageIndicator.FillColor = drawFill.GetValue<Circle>().Color;

            drawDamageMenu.ValueChanged +=
            delegate(object sender, OnValueChangeEventArgs eventArgs)
            {
                DamageIndicator.Enabled = eventArgs.GetNewValue<Boolean>();
            };

            drawFill.ValueChanged +=
            delegate(object sender, OnValueChangeEventArgs eventArgs)
            {
                DamageIndicator.Fill = eventArgs.GetNewValue<Circle>().Active;
                DamageIndicator.FillColor = eventArgs.GetNewValue<Circle>().Color;
            };

            Game.OnUpdate += Game_OnGameUpdate;
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

            if (drawingQ.Active && Q.IsReady())
                Render.Circle.DrawCircle(Player.Position, Q.Range, drawingQ.Color);

            if (drawingW.Active && W.IsReady())
                Render.Circle.DrawCircle(Player.Position, GetWActiveRange, drawingW.Color);

            if (drawingE.Active && E.IsReady())
                Render.Circle.DrawCircle(Player.Position, E.Range, drawingE.Color);

            if (drawingR.Active && R.IsReady())
                Render.Circle.DrawCircle(Player.Position, GetRRange, drawingR.Color);

        }


        static float GetComboDamage(Obj_AI_Base enemy)
        {
            return R.IsReady() ? R.GetDamage(enemy) : 0;
        }

        static void Combo()
        {
            if (!Orbwalking.CanMove(1))
                return;

            if (SharpShooter.Menu.Item("comboUseQ", true).GetValue<Boolean>() && Q.IsReady())
            {
                var Qtarget = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical, true);

                if (Q.CanCast(Qtarget) && Q.GetPrediction(Qtarget).Hitchance >= HitChance.VeryHigh)
                    Q.Cast(Qtarget);
            }

            if (SharpShooter.Menu.Item("comboUseW", true).GetValue<Boolean>() && W.IsReady())
            {
                var Wtarget = TargetSelector.GetTarget(GetWActiveRange, TargetSelector.DamageType.Physical, true);

                if (W.IsReady() && Wtarget.IsValidTarget(GetWActiveRange))
                    W.Cast();
            }

            if (SharpShooter.Menu.Item("comboUseE", true).GetValue<Boolean>() && E.IsReady())
            {
                var Etarget = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical, true);

                if (E.CanCast(Etarget) && E.GetPrediction(Etarget).Hitchance >= HitChance.VeryHigh)
                    E.Cast(Etarget);
            }

            if (SharpShooter.Menu.Item("comboUseR", true).GetValue<Boolean>() && R.IsReady())
            {
                var Rtarget = TargetSelector.GetTarget(GetRRange, TargetSelector.DamageType.Magical, true);
                var Rpred = R.GetPrediction(Rtarget);

                if (R.IsReady() && Rtarget.IsValidTarget(GetRRange) && Player.ServerPosition.Distance(Rpred.CastPosition, false) < GetRRange && Rpred.Hitchance >= HitChance.High)
                    R.Cast(Rpred.CastPosition);
            }
        }

        static void Harass()
        {
            if (!Orbwalking.CanMove(1) || !(Player.ManaPercentage() > SharpShooter.Menu.Item("harassMana", true).GetValue<Slider>().Value))
                return;

            if (SharpShooter.Menu.Item("harassUseQ", true).GetValue<Boolean>() && Q.IsReady())
            {
                var Qtarget = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical, true);

                if (Q.CanCast(Qtarget) && Q.GetPrediction(Qtarget).Hitchance >= HitChance.VeryHigh)
                {
                    Q.Cast(Qtarget);
                    return;
                }
            }

            if (SharpShooter.Menu.Item("harassUseW", true).GetValue<Boolean>() && W.IsReady())
            {
                var Wtarget = TargetSelector.GetTarget(GetWActiveRange, TargetSelector.DamageType.Physical, true);

                if (W.IsReady() && Wtarget.IsValidTarget(GetWActiveRange))
                    W.Cast();
            }

            if (SharpShooter.Menu.Item("harassUseE", true).GetValue<Boolean>() && E.IsReady())
            {
                var Etarget = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical, true);

                if (E.CanCast(Etarget) && E.GetPrediction(Etarget).Hitchance >= HitChance.VeryHigh)
                {
                    E.Cast(Etarget);
                    return;
                }
            }

            if (SharpShooter.Menu.Item("harassUseR", true).GetValue<Boolean>() && R.IsReady())
            {
                var Rtarget = TargetSelector.GetTarget(GetRRange, TargetSelector.DamageType.Magical, true);
                var Rpred = R.GetPrediction(Rtarget);

                if (R.IsReady() && Rtarget.IsValidTarget(GetRRange) && Player.ServerPosition.Distance(Rpred.CastPosition, false) < GetRRange && Rpred.Hitchance >= HitChance.VeryHigh)
                {
                    R.Cast(Rpred.CastPosition);
                    return;
                }
            }
        }

        static void Laneclear()
        {
            var Minions = MinionManager.GetMinions(Player.ServerPosition, 1700, MinionTypes.All, MinionTeam.Enemy);

            if (Minions.Count <= 0)
                return;

            if (SharpShooter.Menu.Item("laneclearUseW", true).GetValue<Boolean>() && W.IsReady())
            {
                if (Minions.Where(x => x.IsValidTarget(GetWActiveRange + 100)).Count() >= 3)
                    W.Cast();
            }

            if (!Orbwalking.CanMove(1) || !(Player.ManaPercentage() > SharpShooter.Menu.Item("laneclearMana", true).GetValue<Slider>().Value))
                return;

            if (SharpShooter.Menu.Item("laneclearUseE", true).GetValue<Boolean>() && E.IsReady())
            {
                var Eloc = E.GetLineFarmLocation(Minions.Where(x => x.IsValidTarget(E.Range)).ToList());

                if (Eloc.MinionsHit >= 5)
                {
                    E.Cast(Eloc.Position);
                    return;
                }
            }

            if (SharpShooter.Menu.Item("laneclearUseR", true).GetValue<Boolean>() && R.IsReady())
            {
                var Rloc = R.GetCircularFarmLocation(Minions.Where(x => x.IsValidTarget(GetRRange)).ToList());

                if (Rloc.MinionsHit >= 4)
                {
                    R.Cast(Rloc.Position);
                    return;
                }
            }
        }

        static void Jungleclear()
        {
            var Mobs = MinionManager.GetMinions(Player.ServerPosition, Orbwalking.GetRealAutoAttackRange(Player) + 100, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);

            if (Mobs.Count <= 0)
                return;

            if (SharpShooter.Menu.Item("jungleclearUseW", true).GetValue<Boolean>() && W.IsReady())
            {
                W.Cast();
                return;
            }

            if (!Orbwalking.CanMove(1) || !(Player.ManaPercentage() > SharpShooter.Menu.Item("jungleclearMana", true).GetValue<Slider>().Value))
                return;

            if (SharpShooter.Menu.Item("jungleclearUseQ", true).GetValue<Boolean>() && Q.IsReady())
            {
                Q.Cast(Mobs[0]);
                return;
            }

            if (SharpShooter.Menu.Item("jungleclearUseE", true).GetValue<Boolean>() && E.IsReady())
            {
                E.Cast(Mobs[0]);
                return;
            }

            if (SharpShooter.Menu.Item("jungleclearUseR", true).GetValue<Boolean>() && R.IsReady())
            {
                R.Cast(Mobs[0]);
                return;
            }
        }
    }
}
