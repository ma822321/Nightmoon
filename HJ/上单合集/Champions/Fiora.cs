﻿using System;
using System.Collections.Generic;
using System.Linq;
using Color = System.Drawing.Color;

using LeagueSharp;
using LeagueSharp.Common;

using UnderratedAIO.Helpers;
using Environment = UnderratedAIO.Helpers.Environment;
using Orbwalking = UnderratedAIO.Helpers.Orbwalking;

namespace UnderratedAIO.Champions
{
    class Fiora
    {
        private static Menu config;
        private static Orbwalking.Orbwalker orbwalker;
        public static readonly Obj_AI_Hero player = ObjectManager.Player;
        public static Spell Q, W, E, R;
        private static float lastQ;

        public Fiora()
        {
            if (player.BaseSkinName != "Fiora") return;
            InitMenu();
            InitFiora();
            Game.PrintChat("<font color='#9933FF'>Soresu </font><font color='#FFFFFF'>- Fiora</font>");
            Game.OnUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Game_OnDraw;
            Orbwalking.AfterAttack += AfterAttack;
            Obj_AI_Base.OnProcessSpellCast += Game_ProcessSpell;
            Jungle.setSmiteSlot();
        }


        private void Game_OnGameUpdate(EventArgs args)
        {
            bool minionBlock = false;
            foreach (Obj_AI_Minion minion in MinionManager.GetMinions(player.Position, player.AttackRange, MinionTypes.All, MinionTeam.Enemy, MinionOrderTypes.None))
            {
                if (HealthPrediction.GetHealthPrediction(minion, 3000) <= Damage.GetAutoAttackDamage(player, minion, false))
                    minionBlock = true;
            }
            if (config.Item("useSmite").GetValue<bool>() && Jungle.smiteSlot != SpellSlot.Unknown)
            {

                var target = Jungle.GetNearest(player.Position);
                bool smiteReady = ObjectManager.Player.Spellbook.CanUseSpell(Jungle.smiteSlot) == SpellState.Ready;
                if (target != null)
                {
                    Jungle.setSmiteSlot();
                    if (Jungle.smite.CanCast(target) && smiteReady && player.Distance(target.Position) <= Jungle.smite.Range && Jungle.smiteDamage() >= target.Health)
                    {

                        Jungle.CastSmite(target);
                    }
                }
            }
            switch (orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    Combo();
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    break;
                case Orbwalking.OrbwalkingMode.LaneClear:
                    Clear();
                    break;
                case Orbwalking.OrbwalkingMode.LastHit:
                    break;
                default:
                    if (!minionBlock)
                    {
                    }
                    break;
            }
        }
        private static void AfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            if (unit.IsMe && E.IsReady() && orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo && config.Item("usee").GetValue<bool>() && target.IsEnemy && target.Team != player.Team)
            {
                E.Cast(config.Item("packets").GetValue<bool>());
                Orbwalking.ResetAutoAttackTimer();
            }
        }

        private void Combo()
        {
            Obj_AI_Hero target = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Physical);
            if (config.Item("useItems").GetValue<bool>()) ItemHandler.UseItems(target);
            if (target == null) return;
            bool hasIgnite = player.Spellbook.CanUseSpell(player.GetSpellSlot("SummonerDot")) == SpellState.Ready;
            if (config.Item("useq").GetValue<bool>() && Q.CanCast(target) && lastQ.Equals(0))
            {
                Q.CastOnUnit(target, config.Item("packets").GetValue<bool>());
                lastQ = System.Environment.TickCount;
            }
            if (config.Item("useq").GetValue<bool>() && Q.CanCast(target) && !lastQ.Equals(0))
            {
                var time = System.Environment.TickCount - lastQ;
                if (time>3500f || player.Distance(target)>350f || Q.GetDamage(target)>target.Health)
                {
                    Q.CastOnUnit(target, config.Item("packets").GetValue<bool>());
                    lastQ = 0;
                }
            }
            if (config.Item("user").GetValue<bool>() && R.CanCast(target) && GetRDamage(target) > target.Health * 1.3 && NeedToUlt(target))
            {
                R.CastOnUnit(target, config.Item("packets").GetValue<bool>());
            }
            if (config.Item("useIgnite").GetValue<bool>() && hasIgnite && ComboDamage(target) > target.Health)
            {
                player.Spellbook.CastSpell(player.GetSpellSlot("SummonerDot"), target);
            }
        }

        private bool NeedToUlt(Obj_AI_Hero target)
        {
            if (player.Distance(target) > 300f && !Q.CanCast(target))
            {
                return true;
            }
            if (player.UnderTurret(true))
            {
                return true;
            }
            if (player.Health<target.Health && player.Health<player.MaxHealth/2)
            {
                return true;
            }
            return false;
        }

        public static void Game_ProcessSpell(Obj_AI_Base hero, GameObjectProcessSpellCastEventArgs args)
        {
            String spellName = args.SData.Name;
            if (W.IsReady() && spellName.Contains("Attack") && config.Item("autoW").GetValue<bool>() && args.Target.IsMe && args.Start.CountEnemiesInRange(40f)>=1)
            {
                W.Cast(config.Item("packets").GetValue<bool>());
            }

            if (!config.Item("dodgeWithR").GetValue<bool>()) return;
                if (spellName == "CurseofTheSadMummy")
                {
                    if (player.Distance(hero.Position) <= 600f)
                    {
                        R.Cast(TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Physical));
                    }
                }
                if (spellName == "EnchantedCrystalArrow")
                {
                    if (player.Distance(hero.Position) <= 400f)
                    {
                        R.Cast(TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Physical));
                    }
                }
                if (spellName == "EnchantedCrystalArrow" || spellName == "EzrealTrueshotBarrage" || spellName == "JinxR" || spellName == "sejuaniglacialprison")
                {
                    if (player.Distance(hero.Position) <= 400f)
                    {
                        R.Cast(TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Physical));
                    }
                }
                if (spellName == "InfernalGuardian" || spellName == "UFSlash")
                {
                    if (player.Distance(args.End) <= 270f)
                    {
                        R.Cast(TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Physical));
                    }
                }
                if (spellName == "BlindMonkRKick" || spellName == "syndrar" || spellName == "VeigarPrimordialBurst" || spellName == "AlZaharNetherGrasp")
                {
                    if (args.Target.IsMe)
                    {
                        R.Cast(TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Physical));
                    }
                }
                if (spellName == "BusterShot" || spellName == "ViR")
                {
                    if (args.Target.IsMe || player.Distance(args.Target.Position) <= 50f)
                    {
                        R.Cast(TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Physical));
                    }
                }
                if (spellName == "GalioIdolOfDurand")
                {
                    if (player.Distance(hero.Position) <= 600f)
                    {
                        R.Cast(TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Physical));
                    }
                }
        }
        private void Clear()
        {
            float perc = (float)config.Item("minmana").GetValue<Slider>().Value / 100f;
            if (player.Mana < player.MaxMana * perc) return;

            var target =
                ObjectManager.Get<Obj_AI_Minion>()
                     .Where(i => i.Distance(player) < Q.Range && i.Health < Q.GetDamage(i))
                     .OrderByDescending(i => i.Distance(player))
                     .FirstOrDefault();
            if (config.Item("useqLC").GetValue<bool>() && Q.CanCast(target))
            {
                Q.CastOnUnit(target, config.Item("packets").GetValue<bool>());
            }
            if (config.Item("useeLC").GetValue<bool>() && Environment.Minion.countMinionsInrange(player.Position,Q.Range)>3)
            {
                E.Cast(config.Item("packets").GetValue<bool>());
            }
        }

        private void Game_OnDraw(EventArgs args)
        {
            DrawHelper.DrawCircle(config.Item("drawaa").GetValue<Circle>(), player.AttackRange);
            DrawHelper.DrawCircle(config.Item("drawqq").GetValue<Circle>(), Q.Range);
            DrawHelper.DrawCircle(config.Item("drawrr").GetValue<Circle>(), R.Range);
            Utility.HpBarDamageIndicator.DamageToUnit = ComboDamage;
            Utility.HpBarDamageIndicator.Enabled = config.Item("drawcombo").GetValue<bool>();
        }

        private float ComboDamage(Obj_AI_Hero hero)
        {
            double damage = 0;
            if (Q.IsReady())
            {
                damage += Damage.GetSpellDamage(player, hero, SpellSlot.Q)*2;
            }
            if (W.IsReady())
            {
                damage += Damage.GetSpellDamage(player, hero, SpellSlot.W);
            }
            if (R.IsReady())
            {
                damage += (float) GetRDamage(hero);
            }
                damage += ItemHandler.GetItemsDamage(hero);
            var ignitedmg = player.GetSummonerSpellDamage(hero, Damage.SummonerSpell.Ignite);
            if (player.Spellbook.CanUseSpell(player.GetSpellSlot("summonerdot")) == SpellState.Ready && hero.Health < damage + ignitedmg)
            {
                damage += ignitedmg;
            }
            return (float)damage;
        }

        private float fioraRSingle(Obj_AI_Hero target)
        {
            return (float)Damage.CalcDamage(
                player, target, Damage.DamageType.Physical,
                new float[3] { 125f, 255f, 385f }[R.Level - 1] + 0.9f * player.FlatPhysicalDamageMod);
        }

        private double GetRDamage(Obj_AI_Hero target)
        {
            float singleR = fioraRSingle(target);
              if (target.CountEnemiesInRange(400) == 1)
                {
                    return Damage.GetSpellDamage(player, target, SpellSlot.R);
                }
                if (target.CountEnemiesInRange(400) == 2)
                {
                    return singleR + (singleR*0.4)*2;
                }
                    return singleR + singleR*0.4;
        }
        private void InitFiora()
        {
            Q = new Spell(SpellSlot.Q, 600);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E);
            R = new Spell(SpellSlot.R, 400);
        }

        private void InitMenu()
        {
            config = new Menu("花边-上单剑姬", "Fiora", true);
            // Target Selector
            Menu menuTS = new Menu("目标选择", "tselect");
            TargetSelector.AddToMenu(menuTS);
            config.AddSubMenu(menuTS);

            // Orbwalker
            Menu menuOrb = new Menu("走砍", "orbwalker");
            orbwalker = new Orbwalking.Orbwalker(menuOrb);
            config.AddSubMenu(menuOrb);

            // 显示 settings
            Menu menuD = new Menu("范围 ", "dsettings");
            menuD.AddItem(new MenuItem("drawaa", "显示 AA 范围")).SetValue(new Circle(false, Color.FromArgb(180, 58, 100, 150)));
            menuD.AddItem(new MenuItem("drawqq", "显示 Q 范围")).SetValue(new Circle(false, Color.FromArgb(180, 58, 100, 150)));
            menuD.AddItem(new MenuItem("drawrr", "显示 R 范围")).SetValue(new Circle(false, Color.FromArgb(180, 58, 100, 150)));
            menuD.AddItem(new MenuItem("drawcombo", "显示连招伤害")).SetValue(true);
            config.AddSubMenu(menuD);
            // Combo Settings
            Menu menuC = new Menu("连招", "csettings");
            menuC.AddItem(new MenuItem("useq", "使用 Q")).SetValue(true);
            menuC.AddItem(new MenuItem("usew", "使用 W")).SetValue(true);
            menuC.AddItem(new MenuItem("usee", "使用 E")).SetValue(true);
            menuC.AddItem(new MenuItem("user", "使用 R")).SetValue(true);
            menuC.AddItem(new MenuItem("dodgeWithR", "使用 R丨躲技能")).SetValue(true);
            menuC.AddItem(new MenuItem("useItems", "使用物品")).SetValue(true);
            menuC.AddItem(new MenuItem("useIgnite", "使用点燃")).SetValue(true);
            config.AddSubMenu(menuC);
            // LaneClear Settings
            Menu menuLC = new Menu("清线", "Lcsettings");
            menuLC.AddItem(new MenuItem("useqLC", "使用 Q")).SetValue(true);
            menuLC.AddItem(new MenuItem("useeLC", "使用 E")).SetValue(true);
            menuLC.AddItem(new MenuItem("minmana", "最低蓝量比")).SetValue(new Slider(1, 1, 100));
            config.AddSubMenu(menuLC);
            // Misc Settings
            Menu menuM = new Menu("杂项", "Msettings");
            menuM.AddItem(new MenuItem("autoW", "自动W+AA")).SetValue(true);
            menuM.AddItem(new MenuItem("useSmite", "使用惩戒")).SetValue(true);

            config.AddSubMenu(menuM);
            config.AddItem(new MenuItem("packets", "使用封包")).SetValue(false);
            config.AddToMainMenu();
        }
    }
}

