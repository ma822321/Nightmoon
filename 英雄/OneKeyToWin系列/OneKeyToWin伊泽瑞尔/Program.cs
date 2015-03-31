using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using System.IO;
using System.Diagnostics;
using SharpDX;
using Collision = LeagueSharp.Common.Collision;
using System.Threading;

namespace Ezreal
{
    class Program
    {
        public const string ChampionName = "Ezreal";

        //Orbwalker instance
        public static Orbwalking.Orbwalker Orbwalker;

        //Spells
        public static List<Spell> SpellList = new List<Spell>();
        public static Spell Q;
        public static Spell W;
        public static Spell E;
        public static Spell R;
        public static Spell R1;
        public static bool attackNow = true;
        //ManaMenager
        public static float qRange = 1100;
        public static float QMANA;
        public static float WMANA;
        public static float EMANA;
        public static float RMANA;
        public static bool Farm = false;
        public static bool Esmart = false;
        public static double WCastTime = 0;
        public static double OverKill = 0;
        public static double OverFarm = 0;
        public static double lag  = 0;
        public static Stopwatch stopWatch = new Stopwatch();
        //AutoPotion
        public static Items.Item Potion = new Items.Item(2003, 0);
        public static Items.Item ManaPotion = new Items.Item(2004, 0);
        public static Items.Item Youmuu = new Items.Item(3142, 0);
        public static int Muramana = 3042;
        public static int Tear = 3070;
        public static int Manamune = 3004;
        //Menu
        public static Menu Config;

        private static Obj_AI_Hero MyHero;

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }
        private static void Game_OnGameLoad(EventArgs args)
        {
            MyHero = ObjectManager.Player;
            if (MyHero.ChampionName != ChampionName) return;

            //Create the spells
            Q = new Spell(SpellSlot.Q, 1180);

            W = new Spell(SpellSlot.W, 1000);
            E = new Spell(SpellSlot.E, 475);
            R = new Spell(SpellSlot.R, 3000f);
            R1 = new Spell(SpellSlot.R, 3000f);

            Q.SetSkillshot(0.25f, 50f, 2000f, true, SkillshotType.SkillshotLine);
            W.SetSkillshot(0.25f, 80f, 1600f, false, SkillshotType.SkillshotLine);
            R.SetSkillshot(1.2f, 160f, 2000f, false, SkillshotType.SkillshotLine);
            R1.SetSkillshot(1.2f, 200f, 2000f, false, SkillshotType.SkillshotCircle);

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);
            SpellList.Add(R1);
            //Create the menu
            Config = new Menu("花边-OKTW伊泽瑞尔", ChampionName, true);
            var targetSelectorMenu = new Menu("目标 选择", "Target Selector");
            TargetSelector.AddToMenu(targetSelectorMenu);
            Config.AddSubMenu(targetSelectorMenu);

            //Orbwalker submenu
            Config.AddSubMenu(new Menu("走砍 设置", "Orbwalking"));

            //Load the orbwalker and add it to the submenu.
            Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalking"));
            Config.AddToMainMenu();


            Config.SubMenu("物品").AddItem(new MenuItem("mura", "自动 魔切").SetValue(true));
            Config.SubMenu("物品").AddItem(new MenuItem("stack", "假如蓝量充足丨自动堆叠魔切Or女神").SetValue(false));
            Config.SubMenu("物品").AddItem(new MenuItem("pots", "使用 药水").SetValue(true));

            Config.SubMenu("E 设置").AddItem(new MenuItem("AGC", "被突进自动E").SetValue(true));
            Config.SubMenu("E 设置").AddItem(new MenuItem("smartE", "智能 E 按键").SetValue(new KeyBind('t', KeyBindType.Press))); //32 == space
            Config.SubMenu("E 设置").AddItem(new MenuItem("autoE", "自动 E").SetValue(true));

            Config.SubMenu("R 设置").AddItem(new MenuItem("autoR", "自动 R").SetValue(true));
            Config.SubMenu("R 设置").AddItem(new MenuItem("Rcc", "R 状态").SetValue(true));
            Config.SubMenu("R 设置").AddItem(new MenuItem("Raoe", "R 伤害").SetValue(true));
            Config.SubMenu("R 设置").AddItem(new MenuItem("hitchanceR", "非常高命中率R").SetValue(true));
            Config.SubMenu("R 设置").AddItem(new MenuItem("useR", "手动 R 按键").SetValue(new KeyBind('t', KeyBindType.Press))); //32 == space

            Config.SubMenu("显示").AddItem(new MenuItem("noti", "显示 通知").SetValue(false));
            Config.SubMenu("显示").AddItem(new MenuItem("qRange", "Q 范围").SetValue(false));
            Config.SubMenu("显示").AddItem(new MenuItem("wRange", "W 范围").SetValue(false));
            Config.SubMenu("显示").AddItem(new MenuItem("eRange", "E 范围").SetValue(false));
            Config.SubMenu("显示").AddItem(new MenuItem("onlyRdy", "显示R可击杀目标").SetValue(true));
            Config.SubMenu("显示").AddItem(new MenuItem("orb", "走砍 目标").SetValue(true));
            Config.SubMenu("显示").AddItem(new MenuItem("qTarget", "Q 目标").SetValue(true));
            Config.SubMenu("显示").AddItem(new MenuItem("semi", "手动 R 目标").SetValue(false));

            Config.AddItem(new MenuItem("farmQ", "Q 补刀").SetValue(true));
            Config.AddItem(new MenuItem("haras", "打钱时骚扰敌人").SetValue(true));
            Config.AddItem(new MenuItem("wPush", "W 友军 (推塔时)").SetValue(true));
            Config.AddItem(new MenuItem("noob", "青铜 虐菜模式").SetValue(false));
            Config.AddItem(new MenuItem("Hit", "技能 命中").SetValue(new Slider(3, 3, 0)));
            Config.AddItem(new MenuItem("debug", "调试 模式").SetValue(false));
            //Add the events we are going to use:
            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnUpdate += Game_OnGameUpdate;
            Orbwalking.BeforeAttack += BeforeAttack;
            Orbwalking.AfterAttack += afterAttack;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Game.PrintChat("<font color=\"#008aff\">E</font>zreal full automatic AI ver 2.6.0 <font color=\"#000000\">by sebastiank1</font> - <font color=\"#00BFFF\">Loaded</font>");
        }
        #region Farm
        public static void farmQ()
        {  
            
            var t = TargetSelector.GetTarget(Q.Range , TargetSelector.DamageType.Physical);

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear)
            {
                var mobs = MinionManager.GetMinions(MyHero.ServerPosition, 800, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
                if (mobs.Count > 0)
                {
                    var mob = mobs[0];
                    Q.Cast(mob, true);
                }
            }

            var minions = MinionManager.GetMinions(MyHero.ServerPosition, Q.Range, MinionTypes.All, MinionTeam.Enemy, MinionOrderTypes.MaxHealth);
            foreach (var minion in minions.Where(minion => !Orbwalking.InAutoAttackRange(minion)))
            {
                if (!Orbwalking.InAutoAttackRange(minion))
                {
                    if (minion.Health < Q.GetDamage(minion) && minion.Health > minion.GetAutoAttackDamage(minion))
                        Q.Cast(minion);
                }
            }
            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear && (!t.IsValidTarget() || MyHero.UnderTurret(false)) && (Game.Time - GetPassiveTime() > -1.5 || ((!E.IsReady() || !R.IsReady()) && MyHero.Mana > MyHero.MaxMana * 0.6)))
            {
                foreach (var minion in minions)
                {
                    if ( minion.Health > 3 * minion.GetAutoAttackDamage(minion))
                        Q.Cast(minion);
                }
            }
        }

        public static bool haras()
        {
            if (Config.Item("haras").GetValue<bool>())
                return true;
            var allMinions = MinionManager.GetMinions(MyHero.ServerPosition, MyHero.AttackRange, MinionTypes.All);
            var haras = true;
            foreach (var minion in allMinions)
            {
                if (minion.Health < MyHero.GetAutoAttackDamage(minion) * 1.4 && Orbwalking.InAutoAttackRange(minion))
                    haras = false;
            }
            if (haras)
                return true;
            else
                return false;
        }
        #endregion

        private static void Game_OnGameUpdate(EventArgs args)
        {  
            ManaMenager();

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed || Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear || Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LastHit)
                Farm = true;
            else
                Farm = false;

            if (Orbwalker.GetTarget() == null)
                attackNow = true;
            if (E.IsReady())
            {
                if (Config.Item("smartE").GetValue<KeyBind>().Active )
                    Esmart = true;
                if (Esmart && MyHero.Position.Extend(Game.CursorPos, E.Range).CountEnemiesInRange(500) < 4)
                    E.Cast(MyHero.Position.Extend(Game.CursorPos, E.Range), true);
            }
            else
                Esmart = false;

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo && E.IsReady() && Config.Item("autoE").GetValue<bool>())
            {
                ManaMenager();
                var t2 = TargetSelector.GetTarget(950, TargetSelector.DamageType.Physical);
                var t = TargetSelector.GetTarget(1400, TargetSelector.DamageType.Physical);

                if (E.IsReady() && MyHero.Mana > RMANA + EMANA
                    && MyHero.CountEnemiesInRange(260) > 0
                    && MyHero.Position.Extend(Game.CursorPos, E.Range).CountEnemiesInRange(500) < 3
                    && t.Position.Distance(Game.CursorPos) > t.Position.Distance(MyHero.Position))
                {

                    E.Cast(MyHero.Position.Extend(Game.CursorPos, E.Range), true);
                }
                else if (MyHero.Health > MyHero.MaxHealth * 0.4
                    && !MyHero.UnderTurret(true)
                    && (Game.Time - OverKill > 0.4)

                     && MyHero.Position.Extend(Game.CursorPos, E.Range).CountEnemiesInRange(700) < 3)
                {
                    if (t.IsValidTarget()
                     && MyHero.Mana > QMANA + EMANA + WMANA
                     && t.Position.Distance(Game.CursorPos) + 300 < t.Position.Distance(MyHero.Position)
                     && Q.IsReady()
                     && Q.GetDamage(t) + E.GetDamage(t) > t.Health
                     && !Orbwalking.InAutoAttackRange(t)
                     && Q.WillHit(MyHero.Position.Extend(Game.CursorPos, E.Range), Q.GetPrediction(t).UnitPosition)
                         )
                    {
                        E.Cast(MyHero.Position.Extend(Game.CursorPos, E.Range), true);
                        debug("E kill Q");
                    }
                    else if (t2.IsValidTarget()
                     && t2.Position.Distance(Game.CursorPos) + 300 < t2.Position.Distance(MyHero.Position)
                     && MyHero.Mana > EMANA + RMANA
                     && MyHero.GetAutoAttackDamage(t2) + E.GetDamage(t2) > t2.Health
                     && !Orbwalking.InAutoAttackRange(t2))
                    {
                        var position = MyHero.Position.Extend(Game.CursorPos, E.Range);
                        if (W.IsReady())
                            W.Cast(position, true);
                        E.Cast(position, true);
                        debug("E kill aa");
                        OverKill = Game.Time;
                    }
                    else if (t.IsValidTarget()
                     && MyHero.Mana > QMANA + EMANA + WMANA
                     && t.Position.Distance(Game.CursorPos) + 300 < t.Position.Distance(MyHero.Position)
                     && W.IsReady()
                     && W.GetDamage(t) + E.GetDamage(t) > t.Health
                     && !Orbwalking.InAutoAttackRange(t)
                     && Q.WillHit(MyHero.Position.Extend(Game.CursorPos, E.Range), Q.GetPrediction(t).UnitPosition)
                         )
                    {
                        E.Cast(MyHero.Position.Extend(Game.CursorPos, E.Range), true);
                        debug("E kill W");
                    }
                }
            }

            if (Q.IsReady())
            {
                //Q.Cast(MyHero);
                ManaMenager();
                if (Config.Item("mura").GetValue<bool>())
                {
                    int Mur = Items.HasItem(Muramana) ? 3042 : 3043;
                    if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo && Items.HasItem(Mur) && Items.CanUseItem(Mur) && MyHero.Mana > RMANA + EMANA + QMANA + WMANA)
                    {
                        if (!MyHero.HasBuff("Muramana"))
                            Items.UseItem(Mur);
                    }
                    else if (MyHero.HasBuff("Muramana") && Items.HasItem(Mur) && Items.CanUseItem(Mur))
                        Items.UseItem(Mur);
                }
                bool cast = false;
                bool wait = false;

                foreach (var target in ObjectManager.Get<Obj_AI_Hero>().Where(target => target.IsValidTarget(Q.Range) && Q.GetDamage(target) > target.Health))
                {
                    cast = true;
                    wait = true;
                    PredictionOutput output = R.GetPrediction(target);
                    Vector2 direction = output.CastPosition.To2D() - MyHero.Position.To2D();
                    direction.Normalize();
                    List<Obj_AI_Hero> enemies = ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsEnemy && x.IsValidTarget()).ToList();
                    foreach (var enemy in enemies)
                    {
                        if (enemy.SkinName == target.SkinName || !cast)
                            continue;
                        PredictionOutput prediction = R.GetPrediction(enemy);
                        Vector3 predictedPosition = prediction.CastPosition;
                        Vector3 v = output.CastPosition - MyHero.ServerPosition;
                        Vector3 w = predictedPosition - MyHero.ServerPosition;
                        double c1 = Vector3.Dot(w, v);
                        double c2 = Vector3.Dot(v, v);
                        double b = c1 / c2;
                        Vector3 pb = MyHero.ServerPosition + ((float)b * v);
                        float length = Vector3.Distance(predictedPosition, pb);
                        if (length < (Q.Width + enemy.BoundingRadius) && MyHero.Distance(predictedPosition) < MyHero.Distance(target.ServerPosition))
                            cast = false;
                    }
                    if (cast && target.IsValidTarget(qRange))
                    {
                        Q.Cast(target, true);
                        debug("Q ks");
                    }
                }
                var t = TargetSelector.GetTarget(qRange, TargetSelector.DamageType.Physical);
                if (MyHero.CountEnemiesInRange(900) == 0)
                    t = TargetSelector.GetTarget(qRange, TargetSelector.DamageType.Physical);
                else
                    t = TargetSelector.GetTarget(900, TargetSelector.DamageType.Physical);

                if (t.IsValidTarget() && !wait )
                {
                    var qDmg = Q.GetDamage(t);
                    var wDmg = W.GetDamage(t);
                    if (qDmg * 3 > t.Health && Config.Item("noob").GetValue<bool>() && t.CountAlliesInRange(800) > 1)
                        debug("Q noob mode");
                    else if (t.IsValidTarget(W.Range) && qDmg + wDmg > t.Health)
                        Q.Cast(t, true);
                    else if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo && MyHero.Mana > RMANA + QMANA)
                        CastSpell(Q, t, Config.Item("Hit").GetValue<Slider>().Value);
                    else if ((Farm && MyHero.Mana > RMANA + EMANA + QMANA + WMANA) && !MyHero.UnderTurret(true) && haras())
                    {
                        foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.IsValidTarget(Q.Range)))
                        {
                            CastSpell(Q, enemy, Config.Item("Hit").GetValue<Slider>().Value);
                        }
                    }

                    else if ((Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo || Farm) && MyHero.Mana > RMANA + QMANA + EMANA)
                    {
                        foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.IsValidTarget(Q.Range)))
                        {
                            if (enemy.HasBuffOfType(BuffType.Stun) || enemy.HasBuffOfType(BuffType.Snare) ||
                             enemy.HasBuffOfType(BuffType.Charm) || enemy.HasBuffOfType(BuffType.Fear) ||
                             enemy.HasBuffOfType(BuffType.Taunt) || enemy.HasBuffOfType(BuffType.Slow) || enemy.HasBuff("Recall"))
                            {
                                Q.Cast(enemy, true);
                            }
                        }
                    }
                }
                if (Farm && attackNow && Config.Item("farmQ").GetValue<bool>() && MyHero.Mana > RMANA + EMANA + WMANA + QMANA * 3)
                {
                    farmQ();
                }
                else if (!Farm && Config.Item("stack").GetValue<bool>() && !MyHero.HasBuff("Recall") && MyHero.Mana > MyHero.MaxMana * 0.95 && Orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.Combo && !t.IsValidTarget() && (Items.HasItem(Tear) || Items.HasItem(Manamune)))
                {
                    Q.Cast(MyHero);
                }

            }
            if (W.IsReady() && attackNow)
            {
                //W.Cast(MyHero);
                ManaMenager();
                var t = TargetSelector.GetTarget(W.Range, TargetSelector.DamageType.Physical);
                if (t.IsValidTarget())
                {
                    var qDmg = Q.GetDamage(t);
                    var wDmg = W.GetDamage(t);
                    if (wDmg > t.Health)
                        CastSpell(W, t, Config.Item("Hit").GetValue<Slider>().Value);
                    else if (wDmg + qDmg > t.Health && Q.IsReady())
                        CastSpell(W, t, Config.Item("Hit").GetValue<Slider>().Value);
                    else if (qDmg * 2 > t.Health && Config.Item("noob").GetValue<bool>() && t.CountAlliesInRange(800) > 1)
                        debug("W noob mode");
                    else if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo && MyHero.Mana > RMANA + WMANA + EMANA + QMANA)
                        CastSpell(W, t, Config.Item("Hit").GetValue<Slider>().Value);
                    else if (Farm && !MyHero.UnderTurret(true) && (MyHero.Mana > MyHero.MaxMana * 0.8 || W.Level > Q.Level) && MyHero.Mana > RMANA + WMANA + EMANA + QMANA + WMANA)
                        CastSpell(W, t, Config.Item("Hit").GetValue<Slider>().Value);
                    else if ((Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo || Farm) && MyHero.Mana > RMANA + WMANA + EMANA)
                    {
                        foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.IsValidTarget(W.Range)))
                        {
                            if (enemy.HasBuffOfType(BuffType.Stun) || enemy.HasBuffOfType(BuffType.Snare) ||
                             enemy.HasBuffOfType(BuffType.Charm) || enemy.HasBuffOfType(BuffType.Fear) ||
                             enemy.HasBuffOfType(BuffType.Taunt) || enemy.HasBuffOfType(BuffType.Slow) || enemy.HasBuff("Recall"))
                            {
                                W.Cast(enemy, true);
                            }
                        }
                    }
                }
            }
            PotionMenager();
            var tr = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Physical);
            
            if (Config.Item("useR").GetValue<KeyBind>().Active && tr.IsValidTarget())
            {
                R.CastIfWillHit(tr, 2, true);
                R1.Cast(tr, true, true);
            }
            if (R.IsReady() && Config.Item("autoR").GetValue<bool>() && MyHero.CountEnemiesInRange(800) == 0 && (Game.Time - OverKill > 0.6))
            {
                foreach (var target in ObjectManager.Get<Obj_AI_Hero>())
                {
                    if (target.IsValidTarget(R.Range) &&
                        !target.HasBuffOfType(BuffType.PhysicalImmunity) &&
                        !target.HasBuffOfType(BuffType.SpellImmunity) &&
                        !target.HasBuffOfType(BuffType.SpellShield))
                    {
                        float predictedHealth = HealthPrediction.GetHealthPrediction(target, (int)(R.Delay + (MyHero.Distance(target.ServerPosition) / R.Speed) * 1000));
                        double Rdmg = R.GetDamage(target);
                        if (Rdmg > predictedHealth)
                            Rdmg = getRdmg(target);
                        var qDmg = Q.GetDamage(target);
                        var wDmg = W.GetDamage(target);
                        if (target.IsValidTarget(R.Range) && Rdmg > predictedHealth && target.CountAlliesInRange(400) == 0 )
                        {
                            castR(target);
                            debug("R normal");
                        }
                        else if (Rdmg > predictedHealth && target.HasBuff("Recall"))
                        {
                            R.Cast(target, true, true);
                            debug("R recall");
                        }
                        else if ((target.HasBuffOfType(BuffType.Stun) || target.HasBuffOfType(BuffType.Snare) ||
                         target.HasBuffOfType(BuffType.Charm) || target.HasBuffOfType(BuffType.Fear) ||
                         target.HasBuffOfType(BuffType.Taunt)) && Config.Item("Rcc").GetValue<bool>() &&
                            target.IsValidTarget(qRange + E.Range) && Rdmg * 1.8 > predictedHealth)
                        {
                                R.Cast(target, true);
                        }
                        else if (target.IsValidTarget(R.Range) && Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo && Config.Item("Raoe").GetValue<bool>())
                        {
                            R.CastIfWillHit(target, 3, true);
                        }
                        else if (target.IsValidTarget(Q.Range + E.Range) && Rdmg + qDmg + wDmg > predictedHealth && Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo && Config.Item("Raoe").GetValue<bool>())
                        {
                            R.CastIfWillHit(target, 2, true);
                        }
                    }
                }
            }
        }

        private static void CastSpell(Spell QWER, Obj_AI_Hero target, int HitChanceNum)
        {
            //HitChance 0 - 3
            // example CastSpell(Q, target, 3);
            if (HitChanceNum == 0)
                QWER.Cast(target, true);
            else if (HitChanceNum == 1)
                QWER.CastIfHitchanceEquals(target, HitChance.VeryHigh, true);
            else if (HitChanceNum == 2)
            {
                if (QWER.Delay < 0.3)
                    QWER.CastIfHitchanceEquals(target, HitChance.Dashing, true);
                QWER.CastIfHitchanceEquals(target, HitChance.Immobile, true);
                QWER.CastIfWillHit(target, 2, true);
                if (target.Path.Count() < 2)
                QWER.CastIfHitchanceEquals(target, HitChance.VeryHigh, true);
            }
            else if (HitChanceNum == 3)
            {
                if (QWER.Delay < 0.3)
                    QWER.CastIfHitchanceEquals(target, HitChance.Dashing, true);
                QWER.CastIfHitchanceEquals(target, HitChance.Immobile, true);
                QWER.CastIfWillHit(target, 2, true);
                if (target.HasBuffOfType(BuffType.Slow) || target.HasBuffOfType(BuffType.Stun) || target.HasBuffOfType(BuffType.Snare) || target.HasBuffOfType(BuffType.Charm) || target.HasBuffOfType(BuffType.Fear) || target.HasBuffOfType(BuffType.Taunt))
                    QWER.CastIfHitchanceEquals(target, HitChance.High, true);

                List<Vector2> waypoints = target.GetWaypoints();
                float SiteToSite = ((target.MoveSpeed * QWER.Delay) + (MyHero.Distance(target.ServerPosition) / QWER.Speed)) * 6 - QWER.Width;
                float BackToFront = ((target.MoveSpeed * QWER.Delay) + (MyHero.Distance(target.ServerPosition) / QWER.Speed)) * 2;
                if (MyHero.Distance(waypoints.Last<Vector2>().To3D()) < SiteToSite || MyHero.Distance(target.Position) < SiteToSite)
                {
                    if (target.IsFacing(MyHero))
                    {
                        if (MyHero.Distance(target.ServerPosition) < QWER.Range - ((target.MoveSpeed * QWER.Delay) + (MyHero.Distance(target.ServerPosition) / QWER.Speed)))
                            QWER.CastIfHitchanceEquals(target, HitChance.High, true);
                    }
                    else
                    {
                        QWER.CastIfHitchanceEquals(target, HitChance.High, true);
                    }
                }
                else if (target.Path.Count() < 2
                    && (target.ServerPosition.Distance(waypoints.Last<Vector2>().To3D()) > SiteToSite
                    || Math.Abs(MyHero.Distance(waypoints.Last<Vector2>().To3D()) - MyHero.Distance(target.Position)) > BackToFront
                    || target.Position == target.ServerPosition))
                {

                    if (target.IsFacing(MyHero))
                    {
                        if (MyHero.Distance(target.ServerPosition) < QWER.Range - ((target.MoveSpeed * QWER.Delay) + (MyHero.Distance(target.ServerPosition) / QWER.Speed)))
                            QWER.CastIfHitchanceEquals(target, HitChance.High, true);
                    }
                    else
                    {
                        QWER.CastIfHitchanceEquals(target, HitChance.High, true);
                    }
                }
            }
        }

        private static void castR(Obj_AI_Hero target)
        {
            if (Config.Item("hitchanceR").GetValue<bool>())
            {
                List<Vector2> waypoints = target.GetWaypoints();
                if (target.Path.Count() < 2 &&  (MyHero.Distance(waypoints.Last<Vector2>().To3D()) - MyHero.Distance(target.Position)) > 400)
                {
                    R.CastIfHitchanceEquals(target, HitChance.High, true);
                }
            }
            else
                R.Cast(target, true);
        }

        public static void debug(string msg)
        {
            if (Config.Item("debug").GetValue<bool>())
                Game.PrintChat(msg);
        }

        private static double getRdmg(Obj_AI_Hero target)
        {
            var rDmg = R.GetDamage(target);
            var dmg = 0;
            PredictionOutput output = R.GetPrediction(target);
            Vector2 direction = output.CastPosition.To2D() - MyHero.Position.To2D();
            direction.Normalize();
            List<Obj_AI_Hero> enemies = ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsEnemy && x.IsValidTarget()).ToList();
            foreach (var enemy in enemies)
            {
                PredictionOutput prediction = R.GetPrediction(enemy);
                Vector3 predictedPosition = prediction.CastPosition;
                Vector3 v = output.CastPosition - MyHero.ServerPosition;
                Vector3 w = predictedPosition - MyHero.ServerPosition;
                double c1 = Vector3.Dot(w, v);
                double c2 = Vector3.Dot(v, v);
                double b = c1 / c2;
                Vector3 pb = MyHero.ServerPosition + ((float)b * v);
                float length = Vector3.Distance(predictedPosition, pb);
                if (length < (R.Width + 100 + enemy.BoundingRadius / 2) && MyHero.Distance(predictedPosition) < MyHero.Distance(target.ServerPosition))
                    dmg++;
            }
            var allMinionsR = MinionManager.GetMinions(MyHero.ServerPosition, R.Range, MinionTypes.All);
            foreach (var minion in allMinionsR)
            {
                PredictionOutput prediction = R.GetPrediction(minion);
                Vector3 predictedPosition = prediction.CastPosition;
                Vector3 v = output.CastPosition - MyHero.ServerPosition;
                Vector3 w = predictedPosition - MyHero.ServerPosition;
                double c1 = Vector3.Dot(w, v);
                double c2 = Vector3.Dot(v, v);
                double b = c1 / c2;
                Vector3 pb = MyHero.ServerPosition + ((float)b * v);
                float length = Vector3.Distance(predictedPosition, pb);
                if (length < (R.Width + 100 + minion.BoundingRadius / 2) && MyHero.Distance(predictedPosition) < MyHero.Distance(target.ServerPosition))
                    dmg++;
            }
            //if (Config.Item("debug").GetValue<bool>())
            //    Game.PrintChat("R collision" + dmg);
            if (dmg > 7)
                return rDmg * 0.7;
            else
                return rDmg - (rDmg * 0.1 * dmg);
        }

        private static void afterAttack(AttackableUnit unit, AttackableUnit target)
        {
            if (!unit.IsMe)
                return;
            attackNow = true;
        }

        static void BeforeAttack(Orbwalking.BeforeAttackEventArgs args)
        {
            attackNow = false;
            if (Config.Item("mura").GetValue<bool>())
            {
                int Mur = Items.HasItem(Muramana) ? 3042 : 3043;
                if (args.Target.IsEnemy && args.Target.IsValid<Obj_AI_Hero>() && Items.HasItem(Mur) && Items.CanUseItem(Mur) && MyHero.Mana > RMANA + EMANA + QMANA + WMANA)
                {
                    if (!MyHero.HasBuff("Muramana"))
                        Items.UseItem(Mur);
                }
                else if (MyHero.HasBuff("Muramana") && Items.HasItem(Mur) && Items.CanUseItem(Mur))
                    Items.UseItem(Mur);
            }
            if (W.IsReady() && Config.Item("wPush").GetValue<bool>() && args.Target.IsValid<Obj_AI_Turret>() && MyHero.Mana > RMANA + EMANA + QMANA + WMANA + WMANA + RMANA)
            {
                foreach (var ally in ObjectManager.Get<Obj_AI_Hero>())
                {
                    if (!ally.IsMe && ally.IsAlly && ally.Distance(MyHero.Position) < 600)
                        W.Cast(ally);
                }
            }
        }

        private static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (Config.Item("AGC").GetValue<bool>() && E.IsReady() && MyHero.Mana > RMANA + EMANA && MyHero.Position.Extend(Game.CursorPos, E.Range).CountEnemiesInRange(400) < 3)
            {
                var Target = (Obj_AI_Hero)gapcloser.Sender;
                if (Target.IsValidTarget(E.Range))
                {
                    E.Cast(MyHero.Position.Extend(Game.CursorPos, E.Range), true);
                    debug("E AGC");
                }
            }
            return;
        }


        public static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base unit, GameObjectProcessSpellCastEventArgs args)
        {
            var target = args.Target as Obj_AI_Hero;
            if (target.IsEnemy)
            {
                var dmg = unit.GetSpellDamage(target, args.SData.Name);
                double HpLeft = target.Health - dmg;
                if (HpLeft < 0 && target.IsValidTarget() && target.IsValidTarget(R.Range))
                {
                    OverKill = Game.Time;
                    debug("OverKill detection " + target.ChampionName);
                }
                if (target.IsValidTarget(Q.Range) && Q.IsReady())
                {
                    var qDmg = Q.GetDamage(target);
                    if (qDmg > HpLeft && HpLeft > 0)
                    {
                        Q.Cast(target, true);
                        debug("Q ks OPS");
                    }
                }
                if ( target.IsValidTarget(W.Range) && W.IsReady())
                {
                    var wDmg = W.GetDamage(target);
                    if (wDmg > HpLeft && HpLeft > 0)
                    {
                        W.Cast(target, true);
                        debug("W ks OPS");
                    }
                }
                if ( Config.Item("autoR").GetValue<bool>() && target.IsValidTarget(R.Range) && R.IsReady() && MyHero.CountEnemiesInRange(700) == 0)
                {
                    double rDmg = getRdmg(target);
                    if (rDmg > HpLeft && HpLeft > 0 && target.CountAlliesInRange(500) == 0)
                    {
                        debug("R OPS");
                        castR(target);
                    }
                }
            }
        }

        private static float GetRealDistance(GameObject target)
        {
            return MyHero.ServerPosition.Distance(target.Position) + MyHero.BoundingRadius +
                   target.BoundingRadius;
        }

        public static void ManaMenager()
        {
            QMANA = Q.Instance.ManaCost;
            WMANA = W.Instance.ManaCost;
            EMANA = E.Instance.ManaCost;


            if (!R.IsReady())
                RMANA = QMANA - MyHero.Level * 2;
            else
                RMANA = R.Instance.ManaCost; ;

            if (Farm)
                RMANA = RMANA + MyHero.CountEnemiesInRange(2500) * 20;

            if (MyHero.Health < MyHero.MaxHealth * 0.2)
            {
                QMANA = 0;
                WMANA = 0;
                EMANA = 0;
                RMANA = 0;
            }
        }

        public static void PotionMenager()
        {
            if (Config.Item("pots").GetValue<bool>() && !MyHero.InFountain() && !MyHero.HasBuff("Recall"))
            {
                if (Potion.IsReady() && !MyHero.HasBuff("RegenerationPotion", true))
                {
                    if (MyHero.CountEnemiesInRange(700) > 0 && MyHero.Health + 200 < MyHero.MaxHealth)
                        Potion.Cast();
                    else if (MyHero.Health < MyHero.MaxHealth * 0.6)
                        Potion.Cast();
                }
                if (ManaPotion.IsReady() && !MyHero.HasBuff("FlaskOfCrystalWater", true))
                {
                    if (MyHero.CountEnemiesInRange(1200) > 0 && MyHero.Mana < RMANA + WMANA + EMANA + RMANA)
                        ManaPotion.Cast();
                }
            }
        }

        private static float GetPassiveTime()
        {
            return
                MyHero.Buffs.OrderByDescending(buff => buff.EndTime - Game.Time)
                    .Where(buff => buff.Name == "ezrealrisingspellforce")
                    .Select(buff => buff.EndTime)
                    .FirstOrDefault();
        }
        private static void Drawing_OnDraw(EventArgs args)
        {

            if (Config.Item("qRange").GetValue<bool>())
            {
                if (Config.Item("onlyRdy").GetValue<bool>() && Q.IsReady())
                    if (Q.IsReady())
                        Render.Circle.DrawCircle(MyHero.Position, Q.Range, System.Drawing.Color.Cyan);
                else
                    Render.Circle.DrawCircle(MyHero.Position, Q.Range, System.Drawing.Color.Cyan);
            }
            if (Config.Item("wRange").GetValue<bool>())
            {
                if (Config.Item("onlyRdy").GetValue<bool>() && W.IsReady())
                    if (Q.IsReady())
                        Render.Circle.DrawCircle(MyHero.Position, W.Range, System.Drawing.Color.Orange);
                    else
                        Render.Circle.DrawCircle(MyHero.Position, W.Range, System.Drawing.Color.Orange);
            }
            if (Config.Item("eRange").GetValue<bool>())
            {
                if (Config.Item("onlyRdy").GetValue<bool>() && E.IsReady())
                    if (Q.IsReady())
                        Render.Circle.DrawCircle(MyHero.Position, E.Range, System.Drawing.Color.Yellow);
                    else
                        Render.Circle.DrawCircle(MyHero.Position, E.Range, System.Drawing.Color.Yellow);
            }
            if (Config.Item("orb").GetValue<bool>())
            {
                var orbT = Orbwalker.GetTarget();
                if (orbT.IsValidTarget())
                    Render.Circle.DrawCircle(orbT.Position, 100, System.Drawing.Color.Pink);
            }
            if (Config.Item("semi").GetValue<bool>() && R.IsReady())
            {
                var t = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Physical);
                if (t.IsValidTarget())
                    Render.Circle.DrawCircle(t.Position, 100, System.Drawing.Color.Red);
            }


            if (Config.Item("noti").GetValue<bool>())
            {
                var t = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Physical);
                if (t.IsValidTarget() && R.IsReady())
                {
                    float predictedHealth = HealthPrediction.GetHealthPrediction(t, (int)(R.Delay + (MyHero.Distance(t.ServerPosition) / R.Speed) * 1000));
                    double rDamage = R.GetDamage(t);
                    if (rDamage > predictedHealth)
                        rDamage = getRdmg(t);
                    if (rDamage > predictedHealth)
                    {
                        Drawing.DrawText(Drawing.Width * 0.1f, Drawing.Height * 0.5f, System.Drawing.Color.Red, "Ult can kill: " + t.ChampionName + " have: " + t.Health + "hp");
                        Render.Circle.DrawCircle(t.ServerPosition, 200, System.Drawing.Color.Red);
                    }

                } 
                var target = TargetSelector.GetTarget(1500, TargetSelector.DamageType.Physical);

                if (target.IsValidTarget())
                {
                    if (Config.Item("debug").GetValue<bool>())
                    {
                        float range = 0;
                        if (MyHero.Distance(target.ServerPosition) < MyHero.Distance(target.Position))
                            range = qRange - (target.MoveSpeed * Q.Delay) + (MyHero.Distance(target.ServerPosition) / Q.Speed);
                        else
                            range = qRange;

                        Drawing.DrawText(Drawing.Width * 0.1f, Drawing.Height * 0.4f, System.Drawing.Color.Red, "Q range: " + (Orbwalking.LastAATick));
                        
                        List <Vector2> waypoints = target.GetWaypoints();
                        Render.Circle.DrawCircle(waypoints.Last<Vector2>().To3D(), 100, System.Drawing.Color.Red);

                        Render.Circle.DrawCircle(target.Position.Extend(waypoints.Last<Vector2>().To3D(),400), 100, System.Drawing.Color.Blue);
                       
                    }

                    if (Config.Item("qTarget").GetValue<bool>())
                        Render.Circle.DrawCircle(target.ServerPosition, 100, System.Drawing.Color.Cyan);
                    if (Q.GetDamage(target) > target.Health)
                    {
                        Render.Circle.DrawCircle(target.ServerPosition, 200, System.Drawing.Color.Red);
                        Drawing.DrawText(Drawing.Width * 0.1f, Drawing.Height * 0.4f, System.Drawing.Color.Red, "Q kill: " + t.ChampionName + " have: " + t.Health + "hp");
                    }
                    else if (Q.GetDamage(target) + W.GetDamage(target) > target.Health)
                    {
                        Render.Circle.DrawCircle(target.ServerPosition, 200, System.Drawing.Color.Red);
                        Drawing.DrawText(Drawing.Width * 0.1f, Drawing.Height * 0.4f, System.Drawing.Color.Red, "Q + W kill: " + t.ChampionName + " have: " + t.Health + "hp");
                    }
                    else if (Q.GetDamage(target) + W.GetDamage(target) + E.GetDamage(target) > target.Health)
                    {
                        Render.Circle.DrawCircle(target.ServerPosition, 200, System.Drawing.Color.Red);
                        Drawing.DrawText(Drawing.Width * 0.1f, Drawing.Height * 0.4f, System.Drawing.Color.Red, "Q + W + E kill: " + t.ChampionName + " have: " + t.Health + "hp");
                    }
                }
            }
        }
    }
}