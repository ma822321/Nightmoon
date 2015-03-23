﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using System.IO;
using SharpDX;
using Collision = LeagueSharp.Common.Collision;
namespace Caitlyn
{
    class Program
    {
        public const string ChampionName = "Caitlyn";

        //Orbwalker instance
        public static Orbwalking.Orbwalker Orbwalker;
        //Spells
        public static List<Spell> SpellList = new List<Spell>();
        public static Spell Q;
        public static Spell Qc;
        public static Spell W;
        public static Spell E;
        public static Spell R;
        public static Spell R1;
        //ManaMenager
        public static float QMANA;
        public static float WMANA;
        public static float EMANA;
        public static float RMANA;
        public static bool Farm = false;
        public static double WCastTime = 0;
        //AutoPotion
        public static Items.Item Potion = new Items.Item(2003, 0);
        public static Items.Item ManaPotion = new Items.Item(2004, 0);
        public static Items.Item Youmuu = new Items.Item(3142, 0);

        //Menu
        public static Menu Config;

        private static Obj_AI_Hero Player;

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }
        private static void Game_OnGameLoad(EventArgs args)
        {
            Player = ObjectManager.Player;
            if (Player.BaseSkinName != ChampionName) return;

            //Create the spells
            Q = new Spell(SpellSlot.Q, 1280);
            Qc = new Spell(SpellSlot.Q, 1100);
            W = new Spell(SpellSlot.W, 800);
            E = new Spell(SpellSlot.E, 980);
            R = new Spell(SpellSlot.R, 3000);
            R1 = new Spell(SpellSlot.R, 3000f);

            Q.SetSkillshot(0.65f, 90f, 2200f, false, SkillshotType.SkillshotLine);
            Qc.SetSkillshot(0.65f, 90f, 2200f, true, SkillshotType.SkillshotLine);
            W.SetSkillshot(1.5f, 1f, 1750f, false, SkillshotType.SkillshotCircle);
            E.SetSkillshot(0.25f, 80f, 1600f, true, SkillshotType.SkillshotLine);
            R1.SetSkillshot(0.7f, 200f, 1500f, false, SkillshotType.SkillshotCircle);

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);
            SpellList.Add(R1);
            //Create the menu
            Config = new Menu("花边汉化-一键女警", ChampionName, true);

            var targetSelectorMenu = new Menu("目标 选择", "Target Selector");
            TargetSelector.AddToMenu(targetSelectorMenu);
            Config.AddSubMenu(targetSelectorMenu);

            //Orbwalker submenu
            Config.AddSubMenu(new Menu("走砍 设置", "Orbwalking"));

            //Load the orbwalker and add it to the submenu.
            Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalking"));
            Config.AddToMainMenu();
            Config.AddItem(new MenuItem("noti", "显示 通知").SetValue(true));
            Config.AddItem(new MenuItem("pots", "使用 药水").SetValue(true));
            Config.AddItem(new MenuItem("autoR", "自动 R").SetValue(true));
            Config.AddItem(new MenuItem("autoQ", "自动 Q").SetValue(true));
            Config.AddItem(new MenuItem("Hit", "Q 命中").SetValue(new Slider(2, 3, 0)));
            Config.AddItem(new MenuItem("useR", "手动 R 按键").SetValue(new KeyBind('t', KeyBindType.Press))); //32 == space
            #region E
            Config.SubMenu("E Config").AddItem(new MenuItem("autoE", "自动 E").SetValue(true));
            Config.SubMenu("E Config").AddItem(new MenuItem("opsE", "连招时 使用W").SetValue(true));
            Config.SubMenu("E Config").AddItem(new MenuItem("AGC", "使用E 防突进").SetValue(true));
            Config.SubMenu("E Config").AddItem(new MenuItem("useE", "进攻 E 按键").SetValue(new KeyBind('t', KeyBindType.Press)));
            Config.AddItem(new MenuItem("debug", "Debug").SetValue(false));
            #endregion
            //Add the events we are going to use:
            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnGameUpdate += Game_OnGameUpdate;
            Orbwalking.BeforeAttack += BeforeAttack;
            Orbwalking.AfterAttack += afterAttack;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
            Game.PrintChat("<font color=\"#00BFFF\">鑺遍倞灏忓眿鍑哄搧 </font> One Key To Win - Caitlyn V1.6 <font color=\"#FFFFFF\">鍔犺級 鎴愬姛!</font>");
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            ManaMenager();
            if (Orbwalker.ActiveMode.ToString() == "Mixed" || Orbwalker.ActiveMode.ToString() == "LaneClear" || Orbwalker.ActiveMode.ToString() == "LastHit")
                Farm = true;
            else
                Farm = false;

            if (ObjectManager.Player.Mana > RMANA + WMANA && W.IsReady())
            {
                foreach (var Object in ObjectManager.Get<Obj_AI_Base>().Where(Obj => Obj.Distance(Player.ServerPosition) < W.Range && Obj.Team != Player.Team && Obj.HasBuff("teleport_target", true)))
                {
                    W.Cast(Object.Position, true);
                    debug("W telport");
                }
            }

            if (E.IsReady())
            {
                ManaMenager();
                var t = TargetSelector.GetTarget(E.Range - 100, TargetSelector.DamageType.Physical);

                var t2 = TargetSelector.GetTarget(1100, TargetSelector.DamageType.Physical);
                if (t.IsValidTarget() && Config.Item("autoE").GetValue<bool>())
                {
                    var eDmg = E.GetDamage(t);
                    float predictedHealth = HealthPrediction.GetHealthPrediction(t, (int)(R.Delay + (Player.Distance(t.ServerPosition) / Q.Speed) * 1000));
                    double Qdmg = Q.GetDamage(t);
                    if (Qdmg > predictedHealth)
                        Qdmg = getQdmg(t);
                    if ( Qdmg + eDmg > t.Health 
                        && Qdmg < t.Health && ObjectManager.Player.Mana > EMANA + QMANA && Q.IsReady()
                        && t.Position.Distance(ObjectManager.Player.ServerPosition) > t.Position.Distance(ObjectManager.Player.Position)
                        && ObjectManager.Player.Position.Distance(t.ServerPosition) < ObjectManager.Player.Position.Distance(t.Position))

                    {
                        E.Cast(t, true);
                        debug("E + Q combo");
                    }
                    else if (
                         ObjectManager.Player.Mana > RMANA + EMANA
                        && ObjectManager.Player.CountEnemiesInRange(200) > 0
                        && ObjectManager.Player.Position.Extend(Game.CursorPos, 400).CountEnemiesInRange(500) < 3
                        && t2.Position.Distance(Game.CursorPos) > t2.Position.Distance(ObjectManager.Player.Position))
                    {

                        var position = ObjectManager.Player.ServerPosition - (Game.CursorPos - ObjectManager.Player.ServerPosition);
                        E.Cast(position, true);
                        debug("E mele escape");
                    }
                    
                    else if (ObjectManager.Player.Mana > RMANA + EMANA && GetRealDistance(t) < 500 && ObjectManager.Player.Health < ObjectManager.Player.MaxHealth * 0.3)
                        E.Cast(t, true);
                }
                if (Config.Item("useE").GetValue<KeyBind>().Active)
                {
                    var position = ObjectManager.Player.ServerPosition - (Game.CursorPos - ObjectManager.Player.ServerPosition);
                    E.Cast(position, true);
                }
            }

            if (Q.IsReady())
            {
                ManaMenager();
                var t = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);
                if (t.IsValidTarget())
                {
                    
                    float predictedHealth = HealthPrediction.GetHealthPrediction(t, (int)(R.Delay + (Player.Distance(t.ServerPosition) / Q.Speed) * 1000));
                    double Qdmg = Q.GetDamage(t);
                    if (Qdmg > predictedHealth)
                        Qdmg = getQdmg(t);
                    if (GetRealDistance(t) > bonusRange() + 150 && Qdmg > predictedHealth && ObjectManager.Player.CountEnemiesInRange(400) == 0)
                    {
                        Q.Cast(t, true);
                        debug("Q KS");
                    }
                    else if (Orbwalker.ActiveMode.ToString() == "Combo" && ObjectManager.Player.Mana > RMANA + QMANA + EMANA + 10 && ObjectManager.Player.CountEnemiesInRange(bonusRange() + 100 + t.BoundingRadius) == 0 && !Config.Item("autoQ").GetValue<bool>())
                    {
                        castQ(t);
                        debug("Q combo");
                    }
                    if (Q.IsReady() && (Orbwalker.ActiveMode.ToString() == "Combo" || Farm) && ObjectManager.Player.Mana > RMANA + QMANA && ObjectManager.Player.CountEnemiesInRange(bonusRange()) == 0 && ObjectManager.Player.CountEnemiesInRange(bonusRange() + 60) == 0)
                    {
                        foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.IsValidTarget(Q.Range)))
                        {
                            if (enemy.HasBuffOfType(BuffType.Stun) || enemy.HasBuffOfType(BuffType.Snare) ||
                             enemy.HasBuffOfType(BuffType.Charm) || enemy.HasBuffOfType(BuffType.Fear) ||
                             enemy.HasBuffOfType(BuffType.Taunt) || enemy.HasBuffOfType(BuffType.Slow) || enemy.HasBuff("Recall"))
                            {
                                Q.CastIfHitchanceEquals(enemy, HitChance.High, true);
                                debug("Q cc");
                            }
                        }
                    }
                    if (Q.IsReady() && Orbwalker.ActiveMode.ToString() == "Combo" && ObjectManager.Player.Mana > RMANA + QMANA + EMANA && ObjectManager.Player.CountEnemiesInRange(bonusRange() + 100) == 0)
                        Q.CastIfWillHit(t, 2, true);
                    if (Q.IsReady() && Farm && ObjectManager.Player.Mana > RMANA + EMANA + WMANA + QMANA  && ObjectManager.Player.CountEnemiesInRange(bonusRange() + 100) == 0)
                    {
                        debug("Q farm");
                        Q.CastIfWillHit(t, 2, true);
                        if(ObjectManager.Player.Mana > ObjectManager.Player.MaxMana * 0.9)
                            castQ(t);
                        else if (t.Path.Count() == 1)
                        {
                            if (Q.IsReady() && ObjectManager.Player.Mana > RMANA + EMANA + WMANA + QMANA + QMANA )
                                Qc.CastIfHitchanceEquals(t, HitChance.VeryHigh, true);

                        }
                    }
                }
            }

            if (R.IsReady() && Config.Item("autoR").GetValue<bool>() && !ObjectManager.Player.UnderTurret(true))
            {
                bool cast = false;
                foreach (var target in ObjectManager.Get<Obj_AI_Hero>().Where(target => target.IsValidTarget(500 * R.Level + 1500)))
                {
                    if (target.IsValidTarget() && (Game.Time - WCastTime > 1) &&
                        !target.HasBuffOfType(BuffType.PhysicalImmunity) && !target.HasBuffOfType(BuffType.SpellImmunity) && !target.HasBuffOfType(BuffType.SpellShield))
                    {
                        float predictedHealth = HealthPrediction.GetHealthPrediction(target, (int)(R.Delay + (Player.Distance(target.ServerPosition) / R.Speed) * 1000));
                        var Rdmg = R.GetDamage(target);
                        if (Rdmg > predictedHealth && GetRealDistance(target) > bonusRange() + 400 + target.BoundingRadius && target.CountAlliesInRange(500) == 0 && Orbwalker.GetTarget() == null)
                        {
                            cast = true;
                            PredictionOutput output = R.GetPrediction(target);
                            Vector2 direction = output.CastPosition.To2D() - Player.Position.To2D();
                            direction.Normalize();
                            List<Obj_AI_Hero> enemies = ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsEnemy && x.IsValidTarget()).ToList();
                            foreach (var enemy in enemies)
                            {
                                if (enemy.SkinName == target.SkinName || !cast)
                                    continue;
                                PredictionOutput prediction = R.GetPrediction(enemy);
                                Vector3 predictedPosition = prediction.CastPosition;
                                Vector3 v = output.CastPosition - Player.ServerPosition;
                                Vector3 w = predictedPosition - Player.ServerPosition;
                                double c1 = Vector3.Dot(w, v);
                                double c2 = Vector3.Dot(v, v);
                                double b = c1 / c2;
                                Vector3 pb = Player.ServerPosition + ((float)b * v);
                                float length = Vector3.Distance(predictedPosition, pb);
                                if (length < (400 + enemy.BoundingRadius) && Player.Distance(predictedPosition) < Player.Distance(target.ServerPosition))
                                    cast = false;
                            }
                            if (cast && target.IsValidTarget() && target.CountEnemiesInRange(500) == 1)
                                R.Cast(target, true);
                        }
                    }
                }
            }
            PotionMenager();
        }
        private static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (Config.Item("AGC").GetValue<bool>() && E.IsReady() && ObjectManager.Player.Mana > RMANA + EMANA)
            {
                var Target = (Obj_AI_Hero)gapcloser.Sender;
                if (Target.IsValidTarget(E.Range) && ObjectManager.Player.Position.Extend(Game.CursorPos, 400).CountEnemiesInRange(500) < 3)
                    E.Cast(Target, true);
                return;
            }
            return;
        }
        public static Obj_AI_Base getMinion()
        {
            var mobs = MinionManager.GetMinions(Player.ServerPosition, 2000, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
            if (mobs.Count > 0 )
            {
                var mob = mobs[0];
                return mob;
            }
            return null;
        }



        private static void afterAttack(AttackableUnit unit, AttackableUnit target)
        {
            if (ObjectManager.Player.Mana > RMANA + WMANA && W.IsReady())
            {
                var t = TargetSelector.GetTarget(W.Range + 300, TargetSelector.DamageType.Physical);
                foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.IsValidTarget(W.Range) && W.IsReady()))
                {
                    if (enemy.HasBuffOfType(BuffType.Stun) || enemy.HasBuffOfType(BuffType.Snare) ||
                         enemy.HasBuffOfType(BuffType.Charm) || enemy.HasBuffOfType(BuffType.Fear) ||
                         enemy.HasBuffOfType(BuffType.Taunt) || enemy.HasBuffOfType(BuffType.Suppression) ||
                         enemy.IsStunned || enemy.HasBuff("Recall"))
                        W.Cast(enemy, true);
                    else if (enemy.HasBuffOfType(BuffType.Slow) && t.Path.Count() > 1)
                        W.CastIfHitchanceEquals(enemy, HitChance.VeryHigh, true);
                }
            }

        }

        static void BeforeAttack(Orbwalking.BeforeAttackEventArgs args)
        {

        }

        public static void UseItem(int id, Obj_AI_Hero target = null)
        {
            if (Items.HasItem(id) && Items.CanUseItem(id))
            {
                Items.UseItem(id, target);
            }
        }

        public static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base unit, GameObjectProcessSpellCastEventArgs args)
        {
            double ShouldUse = ShouldUseE(args.SData.Name);

            if (Config.Item("opsE").GetValue<bool>() && unit.Team != ObjectManager.Player.Team && ShouldUse >= 0f && unit.IsValidTarget(W.Range))
                W.Cast(unit.ServerPosition, true);
            if (unit.IsMe && (args.SData.Name == "CaitlynPiltoverPeacemaker" || args.SData.Name == "CaitlynEntrapment"))
            {
                WCastTime = Game.Time;
            }
            foreach (var target in ObjectManager.Get<Obj_AI_Hero>())
            {
                if (args.Target.NetworkId == target.NetworkId && args.Target.IsEnemy)
                {
                    var dmg = unit.GetSpellDamage(target, args.SData.Name);
                    double HpLeft = target.Health - dmg;
                    if (GetRealDistance(target) > bonusRange() + 60 && target.IsValidTarget(Q.Range) && Q.IsReady())
                    {
                        var qDmg = getQdmg(target);
                        if (qDmg > HpLeft && HpLeft > 0)
                        {
                            Q.Cast(target, true);
                        }
                    }

                }
            }
        }

        private static double getQdmg(Obj_AI_Hero target)
        {
            var qDmg = Q.GetDamage(target);
            var dmg = 0;
            PredictionOutput output = Q.GetPrediction(target);
            Vector2 direction = output.CastPosition.To2D() - Player.Position.To2D();
            direction.Normalize();
            List<Obj_AI_Hero> enemies = ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsEnemy && x.IsValidTarget()).ToList();
            foreach (var enemy in enemies)
            {
                PredictionOutput prediction = Q.GetPrediction(enemy);
                Vector3 predictedPosition = prediction.CastPosition;
                Vector3 v = output.CastPosition - Player.ServerPosition;
                Vector3 w = predictedPosition - Player.ServerPosition;
                double c1 = Vector3.Dot(w, v);
                double c2 = Vector3.Dot(v, v);
                double b = c1 / c2;
                Vector3 pb = Player.ServerPosition + ((float)b * v);
                float length = Vector3.Distance(predictedPosition, pb);
                if (length < (Q.Width + 100 + enemy.BoundingRadius / 2) && Player.Distance(predictedPosition) < Player.Distance(target.ServerPosition))
                    dmg++;
            }
            var allMinionsR = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range, MinionTypes.All);
            foreach (var minion in allMinionsR)
            {
                PredictionOutput prediction = Q.GetPrediction(minion);
                Vector3 predictedPosition = prediction.CastPosition;
                Vector3 v = output.CastPosition - Player.ServerPosition;
                Vector3 w = predictedPosition - Player.ServerPosition;
                double c1 = Vector3.Dot(w, v);
                double c2 = Vector3.Dot(v, v);
                double b = c1 / c2;
                Vector3 pb = Player.ServerPosition + ((float)b * v);
                float length = Vector3.Distance(predictedPosition, pb);
                if (length < (Q.Width + 100 + minion.BoundingRadius / 2) && Player.Distance(predictedPosition) < Player.Distance(target.ServerPosition))
                    dmg++;
            }
            //if (Config.Item("debug").GetValue<bool>())
            //    Game.PrintChat("R collision" + dmg);
            if (dmg > 5)
                return qDmg * 0.5;
            else
                return qDmg - (qDmg * 0.1 * dmg);

        }

        public static void debug(string msg)
        {
            if (Config.Item("debug").GetValue<bool>())
                Game.PrintChat(msg);
        }
        private static void castQ(Obj_AI_Hero target)
        {
            if (Config.Item("Hit").GetValue<Slider>().Value == 0)
                Q.Cast(target, true);
            else if (Config.Item("Hit").GetValue<Slider>().Value == 1)
                Q.CastIfHitchanceEquals(target, HitChance.VeryHigh, true);
            else if (Config.Item("Hit").GetValue<Slider>().Value == 2 && target.Path.Count() < 2)
                Q.CastIfHitchanceEquals(target, HitChance.VeryHigh, true);
            else if (Config.Item("Hit").GetValue<Slider>().Value == 3 && target.Path.Count() < 2 && Math.Abs(ObjectManager.Player.Distance(target.ServerPosition) - ObjectManager.Player.Distance(target.Position)) > 25)
                Q.CastIfHitchanceEquals(target, HitChance.VeryHigh, true);
        }

        public static double ShouldUseE(string SpellName)
        {
            if (SpellName == "ThreshQ")
                return 0;
            if (SpellName == "KatarinaR")
                return 0;
            if (SpellName == "AlZaharNetherGrasp")
                return 0;
            if (SpellName == "GalioIdolOfDurand")
                return 0;
            if (SpellName == "LuxMaliceCannon")
                return 0;
            if (SpellName == "MissFortuneBulletTime")
                return 0;
            if (SpellName == "RocketGrabMissile")
                return 0;
            if (SpellName == "CaitlynPiltoverPeacemaker")
                return 0;
            if (SpellName == "EzrealTrueshotBarrage")
                return 0;
            if (SpellName == "InfiniteDuress")
                return 0;
            if (SpellName == "VelkozR")
                return 0;
            return -1;
        }

        private static float GetRealRange(GameObject target)
        {
            return 680f + ObjectManager.Player.BoundingRadius + target.BoundingRadius;
        }

        public static float bonusRange()
        {
            return 720f + ObjectManager.Player.BoundingRadius;
        }

        private static float GetRealDistance(GameObject target)
        {
            return ObjectManager.Player.ServerPosition.Distance(target.Position) + ObjectManager.Player.BoundingRadius +
                   target.BoundingRadius;
        }

        public static void ManaMenager()
        {
            QMANA = Q.Instance.ManaCost;
            WMANA = W.Instance.ManaCost;
            EMANA = E.Instance.ManaCost;
            if (!R.IsReady())
                RMANA = QMANA - ObjectManager.Player.Level * 2;
            else
                RMANA = R.Instance.ManaCost;

            RMANA = RMANA + (ObjectManager.Player.CountEnemiesInRange(2500) * 20);

            if (ObjectManager.Player.Health < ObjectManager.Player.MaxHealth * 0.2)
            {
                QMANA = 0;
                WMANA = 0;
                EMANA = 0;
                RMANA = 0;
            }
        }

        public static void PotionMenager()
        {
            if (Config.Item("pots").GetValue<bool>() && !ObjectManager.Player.InFountain() && !ObjectManager.Player.HasBuff("Recall"))
            {
                if (Potion.IsReady() && !ObjectManager.Player.HasBuff("RegenerationPotion", true))
                {
                    if (ObjectManager.Player.CountEnemiesInRange(700) > 0 && ObjectManager.Player.Health + 200 < ObjectManager.Player.MaxHealth)
                        Potion.Cast();
                    else if (ObjectManager.Player.Health < ObjectManager.Player.MaxHealth * 0.6)
                        Potion.Cast();
                }
                if (ManaPotion.IsReady() && !ObjectManager.Player.HasBuff("FlaskOfCrystalWater", true))
                {
                    if (ObjectManager.Player.CountEnemiesInRange(1200) > 0 && ObjectManager.Player.Mana < RMANA + WMANA + EMANA)
                        ManaPotion.Cast();
                }
            }
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            var minion = getMinion();
            if (minion.IsValidTarget())
            {
                //Render.Circle.DrawCircle(minion.Position, 100, System.Drawing.Color.Red);
                //Game.PrintChat("range: " + ObjectManager.Player.Distance(minion.Position));
               // Game.PrintChat("bount: " + minion.BoundingRadius);
                
            }
            if (Config.Item("noti").GetValue<bool>())
            {
                var t = TargetSelector.GetTarget(500 * R.Level + 1500, TargetSelector.DamageType.Physical);
                float predictedHealth = HealthPrediction.GetHealthPrediction(t, (int)(R.Delay + (Player.Distance(t.ServerPosition) / R.Speed) * 1000));
                if (t.IsValidTarget() && R.IsReady())
                {
                    var rDamage = R.GetDamage(t);
                    if (rDamage > predictedHealth)
                        Drawing.DrawText(Drawing.Width * 0.1f, Drawing.Height * 0.5f, System.Drawing.Color.Red, "Ult can kill: " + t.ChampionName + " have: " + t.Health + "hp");
                    if (Config.Item("useR").GetValue<KeyBind>().Active)
                    {
                        R.Cast(t, true);
                    }
                }
                
                var tw = TargetSelector.GetTarget(W.Range, TargetSelector.DamageType.Physical);
                if (tw.IsValidTarget())
                {
                    var qDmg = W.GetDamage(tw);
                    if (qDmg > tw.Health)
                    {
                        Drawing.DrawText(Drawing.Width * 0.1f, Drawing.Height * 0.4f, System.Drawing.Color.Red, "W can kill: " + t.ChampionName + " have: " + t.Health + "hp");
                    }
                }
            }
        }
    }
}