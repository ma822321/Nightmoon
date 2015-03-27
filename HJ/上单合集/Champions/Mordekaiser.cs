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
    class Mordekaiser
    {
        public static Menu config;
        private static Orbwalking.Orbwalker orbwalker;
        public static readonly Obj_AI_Hero player = ObjectManager.Player;
        public static Spell Q, W, E, R;
        public static bool hasGhost = false;
        public static bool GhostDelay = false;
        public static int GhostRange = 2200;

        public Mordekaiser()
        {
            if (player.BaseSkinName != "Mordekaiser") return;
            InitMenu();
            InitMordekaiser();
            Game.PrintChat("<font color='#9933FF'>Soresu </font><font color='#FFFFFF'>- Mordekaiser</font>");
            Game.OnUpdate += Game_OnGameUpdate;
            Orbwalking.AfterAttack += AfterAttack;
            Drawing.OnDraw += Game_OnDraw;
        }

       private void Game_OnGameUpdate(EventArgs args)
       {
           bool minionBlock = false;
           foreach (Obj_AI_Minion minion in MinionManager.GetMinions(player.Position, player.AttackRange, MinionTypes.All, MinionTeam.Enemy, MinionOrderTypes.None))
           {
               if (HealthPrediction.GetHealthPrediction(minion, 3000) <= Damage.GetAutoAttackDamage(player, minion, false))
                   minionBlock = true;
           }   
           switch (orbwalker.ActiveMode)
           {
               case Orbwalking.OrbwalkingMode.Combo:
                   Combo();
                   break;
               case Orbwalking.OrbwalkingMode.Mixed:
                   if (!minionBlock)
                   {
                       Harass();
                   }
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
           if (unit.IsMe && Q.IsReady() && ((orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo && config.Item("useq").GetValue<bool>() && target.IsEnemy && target.Team != player.Team) || (config.Item("useqLC").GetValue<bool>() && Jungle.GetNearest(player.Position).Distance(player.Position) < player.AttackRange + 30)))
           {
               Q.Cast(config.Item("packets").GetValue<bool>());
               Orbwalking.ResetAutoAttackTimer();
               //player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
               return;
           }
           if (unit.IsMe && Q.IsReady() && orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear && config.Item("useqLC").GetValue<bool>() && Environment.Minion.countMinionsInrange(player.Position, 600f)>1)
           {
               Q.Cast(config.Item("packets").GetValue<bool>());
               Orbwalking.ResetAutoAttackTimer();
               //player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
           }
       }
       private void Combo()
       {
           Obj_AI_Hero target = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Magical);
           if (config.Item("useItems").GetValue<bool>()) ItemHandler.UseItems(target);
           if (target == null) return;
           var combodmg = ComboDamage(target);
           bool hasIgnite = player.Spellbook.CanUseSpell(player.GetSpellSlot("SummonerDot")) == SpellState.Ready;
           if (config.Item("usew").GetValue<bool>())
           {
               var wTarget = Environment.Hero.mostEnemyAtFriend(player, W.Range, 250f);
               if(wTarget!=null) W.Cast(wTarget, config.Item("packets").GetValue<bool>());
           }
           if (config.Item("usee").GetValue<bool>() && E.CanCast(target))
           {
               E.Cast(target.Position, config.Item("packets").GetValue<bool>());
           }
           if (config.Item("user").GetValue<bool>() && !MordeGhost && (!config.Item("ultDef").GetValue<bool>() || (config.Item("ultDef").GetValue<bool>() && CombatHelper.HasDef(target))) && (player.Distance(target.Position) <= 400f || (R.CanCast(target) && target.Health < 250f && Environment.Hero.countChampsAtrangeA(player.Position, 600f) >= 1)) && !config.Item("ult" + target.SkinName).GetValue<bool>() && combodmg > target.Health)
           {
               R.CastOnUnit(target, config.Item("packets").GetValue<bool>());
           }
           if (config.Item("useIgnite").GetValue<bool>() && combodmg > target.Health && hasIgnite)
           {
               player.Spellbook.CastSpell(player.GetSpellSlot("SummonerDot"), target);
           }
           if (MordeGhost && !GhostDelay)
           {
               var Gtarget = TargetSelector.GetTarget(GhostRange, TargetSelector.DamageType.Magical);
               switch (config.Item("ghostTarget").GetValue<StringList>().SelectedIndex)
               {
                   case 0:
                       Gtarget = TargetSelector.GetTarget(GhostRange, TargetSelector.DamageType.Magical);
                       break;
                   case 1:
                       Gtarget = ObjectManager.Get<Obj_AI_Hero>().Where(i => i.IsEnemy && !i.IsDead && player.Distance(i) <= GhostRange).OrderBy(i => i.Health).FirstOrDefault();
                       break;
                   case 2:
                       Gtarget = ObjectManager.Get<Obj_AI_Hero>().Where(i => i.IsEnemy && !i.IsDead && player.Distance(i) <= GhostRange).OrderBy(i => player.Distance(i)).FirstOrDefault();
                       break;
                   default:
                       break;
               }
               if (Gtarget.IsValid)
               {

                   R.CastOnUnit(Gtarget, config.Item("packets").GetValue<bool>());
                   GhostDelay = true;
                   Utility.DelayAction.Add(1000, () => GhostDelay = false);
               }
           }
       }
       private static bool MordeGhost
       {
           get { return player.Spellbook.GetSpell(SpellSlot.R).Name == "mordekaisercotgguide"; }
       }
       private void Harass()
       {
           Obj_AI_Hero target = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Magical);
           if (target == null) return;
           if (config.Item("useeH").GetValue<bool>() && E.CanCast(target))
           {
               E.Cast(target, config.Item("packets").GetValue<bool>());
           }
       }

       private void Clear()
       {
          var bestpos = Environment.Minion.bestVectorToAoeFarm(player.Position,E.Range-20f,50f);
           if (config.Item("useeLC").GetValue<bool>() && W.IsReady() && player.Distance(bestpos) <= W.Range && bestpos.IsValid())
           {
               E.Cast(bestpos, config.Item("packets").GetValue<bool>());
           }
           if (config.Item("usewLC").GetValue<bool>() && W.IsReady() && Environment.Minion.countMinionsInrange(player.Position,250f)>1)
           {
               W.Cast(player, config.Item("packets").GetValue<bool>());
           }
       }

       private void Game_OnDraw(EventArgs args)
       {
           DrawHelper.DrawCircle(config.Item("drawaa").GetValue<Circle>(), player.AttackRange);
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
           if (E.IsReady())
           {
               damage += Damage.GetSpellDamage(player, hero, SpellSlot.E);
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

           var ignitedmg = player.GetSummonerSpellDamage(hero, Damage.SummonerSpell.Ignite);
           if (player.Spellbook.CanUseSpell(player.GetSpellSlot("summonerdot")) == SpellState.Ready && hero.Health < damage + ignitedmg)
           {
               damage += ignitedmg;
           }
           return (float)damage;
       }

       private void InitMordekaiser()
        {
            Q = new Spell(SpellSlot.Q, player.AttackRange);
            W = new Spell(SpellSlot.W, 750);
            E = new Spell(SpellSlot.E, 650);
            E.SetSkillshot(E.Instance.SData.SpellCastTime, E.Instance.SData.LineWidth, E.Speed, false, SkillshotType.SkillshotCone);
            R = new Spell(SpellSlot.R, 850);
        }

        private void InitMenu()
       {
           config = new Menu("花边-上单金属", "Mordekaiser", true);
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
           menuD.AddItem(new MenuItem("drawaa", "显示 AA 范围")).SetValue(new Circle(false, Color.FromArgb(180, 109, 111, 126)));
           menuD.AddItem(new MenuItem("drawww", "显示 W 范围")).SetValue(new Circle(false, Color.FromArgb(180, 109, 111, 126)));
           menuD.AddItem(new MenuItem("drawee", "显示 E 范围")).SetValue(new Circle(false, Color.FromArgb(180, 109, 111, 126)));
           menuD.AddItem(new MenuItem("drawrr", "显示 R 范围")).SetValue(new Circle(false, Color.FromArgb(180, 109, 111, 126)));
           menuD.AddItem(new MenuItem("drawcombo", "显示连招伤害")).SetValue(true);
           config.AddSubMenu(menuD);
           // Combo Settings
           Menu menuC = new Menu("连招", "csettings");
           menuC.AddItem(new MenuItem("useq", "使用 Q")).SetValue(true);
           menuC.AddItem(new MenuItem("usew", "使用 W")).SetValue(true);
           menuC.AddItem(new MenuItem("usee", "使用 E")).SetValue(true);
           menuC.AddItem(new MenuItem("user", "使用 R")).SetValue(true);
           menuC.AddItem(new MenuItem("ultDef", "有净化.屏障禁止R")).SetValue(true);
           menuC.AddItem(new MenuItem("useItems", "使用物品")).SetValue(true);
           menuC.AddItem(new MenuItem("useIgnite", "使用点燃")).SetValue(true);
           config.AddSubMenu(menuC);
           // Harass Settings
           Menu menuH = new Menu("骚扰 ", "Hsettings");
           menuH.AddItem(new MenuItem("useeH", "使用 E")).SetValue(true);
           config.AddSubMenu(menuH);
           // LaneClear Settings
           Menu menuLC = new Menu("刷兵", "Lcsettings");
           menuLC.AddItem(new MenuItem("useqLC", "使用 Q")).SetValue(true);
           menuLC.AddItem(new MenuItem("usewLC", "使用 W")).SetValue(true);
           menuLC.AddItem(new MenuItem("useeLC", "使用 E")).SetValue(true);
           config.AddSubMenu(menuLC);
           // Misc Settings
           Menu menuM = new Menu("杂项", "Msettings");
           menuM.AddItem(new MenuItem("ghostTarget", "R幽灵优先目标")).SetValue(new StringList(new[] { "手动选择", "残血", "附近" }, 0));

           config.AddSubMenu(menuM);
           var sulti = new Menu("禁止R ", "dontult");
           foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsEnemy))
           {
                   sulti.AddItem(new MenuItem("ult" + hero.SkinName, hero.SkinName)).SetValue(false);
           }
           config.AddSubMenu(sulti);
           config.AddItem(new MenuItem("packets", "使用封包")).SetValue(false);
           config.AddToMainMenu();
       }
    }
}
