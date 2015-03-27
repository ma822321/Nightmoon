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
    class Evelynn
    {
        public static Menu config;
        private static Orbwalking.Orbwalker orbwalker;
        public static readonly Obj_AI_Hero player = ObjectManager.Player;
        public static Spell Q, W, E, R;

        public Evelynn()
        {
            if (player.BaseSkinName != "Evelynn") return;
            InitMenu();
            InitEvelynn();
            Game.PrintChat("<font color='#9933FF'>Soresu </font><font color='#FFFFFF'>- Evelynn</font>");
            Game.OnUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Game_OnDraw;
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
                   LastHit();
                   break;
               default:
                   if (!minionBlock)
                   {
                   }
                   break;
           }
       }

       private void LastHit()
       {
           var target =
               ObjectManager.Get<Obj_AI_Minion>().Where(i => i.IsEnemy && !i.IsDead && Q.GetDamage(i)>i.Health).OrderBy(i => player.Distance(i)).FirstOrDefault();
           if (target.IsValid && Q.CanCast(target))
           {
               Q.Cast(config.Item("useqLH").GetValue<bool>());
           }
       }


       private void Combo()
       {
           Obj_AI_Hero target = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Magical);
           if (config.Item("useItems").GetValue<bool>()) ItemHandler.UseItems(target);
           if (target == null) return;
           bool hasIgnite = player.Spellbook.CanUseSpell(player.GetSpellSlot("SummonerDot")) == SpellState.Ready;

           if (config.Item("useq").GetValue<bool>() && Q.IsReady())
           {
               Q.Cast();
           }
           if (config.Item("usew").GetValue<bool>() && W.IsReady() && checkSlows())
           {
               W.Cast();
           }
           if (config.Item("usee").GetValue<bool>() && E.CanCast(target))
           {
               E.CastOnUnit(target);
           }
           var Ultpos =Environment.Hero.bestVectorToAoeSpell(ObjectManager.Get<Obj_AI_Hero>().Where(i=>(i.IsEnemy && R.CanCast(i))),R.Range, 250f);
           if (config.Item("user").GetValue<bool>() && R.IsReady() && config.Item("useRmin").GetValue<Slider>().Value < Ultpos.CountEnemiesInRange(250f) && R.Range > player.Distance(Ultpos))
           {
               R.Cast(Ultpos);
           }
           var ignitedmg = (float)player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite);
           if (config.Item("useIgnite").GetValue<bool>() && ignitedmg > target.Health && hasIgnite && !E.CanCast(target))
           {
               player.Spellbook.CastSpell(player.GetSpellSlot("SummonerDot"), target);
           }
       }

        private bool checkSlows()
        {
            foreach (var buff in player.Buffs)
            {
                if (buff.Name.ToLower().Contains("slow")) return true;
            }
            return false;
        }

       private void Clear()
       {
           float perc = (float)config.Item("minmana").GetValue<Slider>().Value / 100f;
           if (player.Mana < player.MaxMana * perc) return;

           if (config.Item("useqLC").GetValue<bool>() && Q.IsReady())
           {
               Q.Cast();
           }
           var target =
    ObjectManager.Get<Obj_AI_Minion>()
        .Where(i => i.Distance(player) < E.Range && i.Health < E.GetDamage(i))
        .OrderByDescending(i => i.Distance(player))
        .FirstOrDefault();
           if (config.Item("useeLC").GetValue<bool>() && E.CanCast(target))
           {
               E.CastOnUnit(target, config.Item("packets").GetValue<bool>());
           }
       }

       private void Game_OnDraw(EventArgs args)
       {
           DrawHelper.DrawCircle(config.Item("drawaa").GetValue<Circle>(), player.AttackRange);
           DrawHelper.DrawCircle(config.Item("drawqq").GetValue<Circle>(), Q.Range);
           DrawHelper.DrawCircle(config.Item("drawww").GetValue<Circle>(), W.Range);
           DrawHelper.DrawCircle(config.Item("drawee").GetValue<Circle>(), E.Range);
           DrawHelper.DrawCircle(config.Item("drawrr").GetValue<Circle>(), R.Range);
           Utility.HpBarDamageIndicator.DamageToUnit = ComboDamage;
           Utility.HpBarDamageIndicator.Enabled = config.Item("drawcombo").GetValue<bool>();
       }

       private float ComboDamage(Obj_AI_Hero hero)
       {
           double damage = 0;
           if (Q.IsReady())
           {
               damage += Damage.GetSpellDamage(player, hero, SpellSlot.Q);
           }
           if (W.IsReady())
           {
               damage += Damage.GetSpellDamage(player, hero, SpellSlot.W);
           }
           if (R.IsReady())
           {
               damage += Damage.GetSpellDamage(player, hero, SpellSlot.R);
           }

           damage += ItemHandler.GetItemsDamage(hero);

           if ((Items.HasItem(ItemHandler.Bft.Id) && Items.CanUseItem(ItemHandler.Bft.Id)) ||
                (Items.HasItem(ItemHandler.Dfg.Id) && Items.CanUseItem(ItemHandler.Dfg.Id)))
           {
               damage = (float)(damage * 1.2);
           }
           if (E.IsReady())
           {
               damage += Damage.GetSpellDamage(player, hero, SpellSlot.E);
           }
           var ignitedmg = player.GetSummonerSpellDamage(hero, Damage.SummonerSpell.Ignite);
           if (player.Spellbook.CanUseSpell(player.GetSpellSlot("summonerdot")) == SpellState.Ready && hero.Health < damage + ignitedmg)
           {
               damage += ignitedmg;
           }
           return (float)damage;
       }

       private void InitEvelynn()
        {
            Q = new Spell(SpellSlot.Q, 500);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E, 225);
            R = new Spell(SpellSlot.R, 650);
            R.SetSkillshot(R.Instance.SData.SpellCastTime, R.Instance.SData.LineWidth, R.Speed, false, SkillshotType.SkillshotCone);
        }

        private void InitMenu()
       {
           config = new Menu("花边-上单寡妇", "Evelynn", true);
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
           menuD.AddItem(new MenuItem("drawww", "显示 W 范围")).SetValue(new Circle(false, Color.FromArgb(180, 58, 100, 150)));
           menuD.AddItem(new MenuItem("drawee", "显示 E 范围")).SetValue(new Circle(false, Color.FromArgb(180, 58, 100, 150)));
           menuD.AddItem(new MenuItem("drawrr", "显示 R 范围")).SetValue(new Circle(false, Color.FromArgb(180, 58, 100, 150)));
           menuD.AddItem(new MenuItem("drawcombo", "显示连招伤害")).SetValue(true);
           config.AddSubMenu(menuD);
           // Combo Settings
           Menu menuC = new Menu("连招", "csettings");
           menuC.AddItem(new MenuItem("useq", "使用 Q")).SetValue(true);
           menuC.AddItem(new MenuItem("usew", "使用 W")).SetValue(true);
           menuC.AddItem(new MenuItem("usee", "使用 E")).SetValue(true);
           menuC.AddItem(new MenuItem("user", "使用 R")).SetValue(true);
           menuC.AddItem(new MenuItem("useRmin", "R 敌人数")).SetValue(new Slider(1, 1, 5));
           menuC.AddItem(new MenuItem("useItems", "使用物品")).SetValue(true);
           menuC.AddItem(new MenuItem("useIgnite", "使用点燃")).SetValue(true);
           config.AddSubMenu(menuC);
           // Harass Settings
           Menu menuH = new Menu("骚扰 ", "Hsettings");
           menuH.AddItem(new MenuItem("useqH", "使用 Q")).SetValue(true);
           config.AddSubMenu(menuH);
           // Lasthit Settings
           Menu menuLH = new Menu("补刀 ", "LHsettings");
           menuLH.AddItem(new MenuItem("useqLH", "使用 Q")).SetValue(true);
           config.AddSubMenu(menuLH);
           // LaneClear Settings
           Menu menuLC = new Menu("清线", "Lcsettings");
           menuLC.AddItem(new MenuItem("useqLC", "使用 Q")).SetValue(true);
           menuLC.AddItem(new MenuItem("useeLC", "使用 E")).SetValue(true);
           menuLC.AddItem(new MenuItem("minmana", "最低蓝量比")).SetValue(new Slider(1, 1, 100));
           config.AddSubMenu(menuLC);
           // Misc Settings
           Menu menuM = new Menu("杂项", "Msettings");
           menuM.AddItem(new MenuItem("useSmite", "使用惩戒")).SetValue(true);

           config.AddSubMenu(menuM);
           config.AddItem(new MenuItem("packets", "使用封包")).SetValue(false);
           config.AddToMainMenu();
       }
    }
}
