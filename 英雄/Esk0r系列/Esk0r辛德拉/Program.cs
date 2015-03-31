﻿#region

using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

#endregion

namespace Syndra
{
    internal class Program
    {
        public const string ChampionName = "Syndra";

        //Orbwalker instance
        public static Orbwalking.Orbwalker Orbwalker;

        //Spells
        public static List<Spell> SpellList = new List<Spell>();

        public static Spell Q;
        public static Spell W;
        public static Spell E;
        public static Spell EQ;
        public static Spell R;

        public static SpellSlot IgniteSlot;

        public static Items.Item DFG;

        //Menu
        public static Menu Config;
        private static int QEComboT;
        private static int WEComboT;

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
            Q = new Spell(SpellSlot.Q, 790);
            W = new Spell(SpellSlot.W, 925);
            E = new Spell(SpellSlot.E, 700);
            R = new Spell(SpellSlot.R, 675);
            EQ = new Spell(SpellSlot.Q, Q.Range + 500);

            IgniteSlot = Player.GetSpellSlot("SummonerDot");

            DFG = Utility.Map.GetMap().Type == Utility.Map.MapType.TwistedTreeline ? new Items.Item(3188, 750) : new Items.Item(3128, 750);

            Q.SetSkillshot(0.6f, 125f, float.MaxValue, false, SkillshotType.SkillshotCircle);
            W.SetSkillshot(0.25f, 140f, 1600f, false, SkillshotType.SkillshotCircle);
            E.SetSkillshot(0.25f, (float)(45 * 0.5), 2500f, false, SkillshotType.SkillshotCircle);
            EQ.SetSkillshot(float.MaxValue, 55f, 2000f, false, SkillshotType.SkillshotCircle);

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);

            //Create the menu
            Config = new Menu("花边汉化-Esk0r辛德拉", ChampionName, true);

            //Orbwalker submenu
            Config.AddSubMenu(new Menu("走 砍", "Orbwalking"));

            //Add the target selector to the menu as submenu.
            var targetSelectorMenu = new Menu("目标 选择", "Target Selector");
            TargetSelector.AddToMenu(targetSelectorMenu);
            Config.AddSubMenu(targetSelectorMenu);

            //Load the orbwalker and add it to the menu as submenu.
            Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalking"));
            
            //Combo menu:
            Config.AddSubMenu(new Menu("连 招", "Combo"));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseQCombo", "使用 Q").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseWCombo", "使用 W").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseECombo", "使用 E").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseQECombo", "使用 QE").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseRCombo", "使用 R").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseIgniteCombo", "使用 点燃").SetValue(true));
            Config.SubMenu("Combo")
                .AddItem(
                    new MenuItem("ComboActive", "开 启!").SetValue(
                        new KeyBind(Config.Item("Orbwalk").GetValue<KeyBind>().Key, KeyBindType.Press)));

            //Harass menu:
            Config.AddSubMenu(new Menu("骚 扰", "Harass"));
            Config.SubMenu("Harass").AddItem(new MenuItem("UseQHarass", "使用 Q").SetValue(true));
            Config.SubMenu("Harass").AddItem(new MenuItem("UseWHarass", "使用 W").SetValue(false));
            Config.SubMenu("Harass").AddItem(new MenuItem("UseEHarass", "使用 E").SetValue(false));
            Config.SubMenu("Harass").AddItem(new MenuItem("UseQEHarass", "使用 QE").SetValue(false));
            Config.SubMenu("Harass")
                .AddItem(
                    new MenuItem("HarassActive", "开 启!").SetValue(
                        new KeyBind(Config.Item("Farm").GetValue<KeyBind>().Key, KeyBindType.Press)));
            Config.SubMenu("Harass")
                .AddItem(
                    new MenuItem("HarassActiveT", "自动 骚扰!").SetValue(new KeyBind("Y".ToCharArray()[0],
                        KeyBindType.Toggle)));

            //Farming menu:
            Config.AddSubMenu(new Menu("打 钱", "Farm"));
            Config.SubMenu("Farm")
                .AddItem(
                    new MenuItem("UseQFarm", "使用 Q").SetValue(
                        new StringList(new[] { "控 线", "清 线", "两 者", "都不" }, 2)));
            Config.SubMenu("Farm")
                .AddItem(
                    new MenuItem("UseWFarm", "使用 W").SetValue(
                        new StringList(new[] { "控 线", "清 线", "两 者", "都不" }, 1)));
            Config.SubMenu("Farm")
                .AddItem(
                    new MenuItem("UseEFarm", "使用 E").SetValue(
                        new StringList(new[] { "控 线", "清 线", "两 者", "都不" }, 3)));
            Config.SubMenu("Farm")
                .AddItem(
                    new MenuItem("FreezeActive", "控 线!").SetValue(
                        new KeyBind(Config.Item("Farm").GetValue<KeyBind>().Key, KeyBindType.Press)));
            Config.SubMenu("Farm")
                .AddItem(
                    new MenuItem("LaneClearActive", "清 线!").SetValue(
                        new KeyBind(Config.Item("LaneClear").GetValue<KeyBind>().Key, KeyBindType.Press)));

            //JungleFarm menu:
            Config.AddSubMenu(new Menu("清 野", "JungleFarm"));
            Config.SubMenu("JungleFarm").AddItem(new MenuItem("UseQJFarm", "使用 Q").SetValue(true));
            Config.SubMenu("JungleFarm").AddItem(new MenuItem("UseWJFarm", "使用 W").SetValue(true));
            Config.SubMenu("JungleFarm").AddItem(new MenuItem("UseEJFarm", "使用 E").SetValue(true));
            Config.SubMenu("JungleFarm")
                .AddItem(
                    new MenuItem("JungleFarmActive", "开 启!").SetValue(
                        new KeyBind(Config.Item("LaneClear").GetValue<KeyBind>().Key, KeyBindType.Press)));

            //Misc
            Config.AddSubMenu(new Menu("杂 项", "Misc"));
            Config.SubMenu("Misc").AddItem(new MenuItem("InterruptSpells", "打断 法术").SetValue(true));
            Config.SubMenu("Misc")
                .AddItem(
                    new MenuItem("CastQE", "QE 鼠标最近的人").SetValue(new KeyBind("T".ToCharArray()[0],
                        KeyBindType.Press)));
            Config.SubMenu("Misc").AddSubMenu(new Menu("不对某人使用R", "DontUlt"));

            foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.Team != Player.Team))
                Config.SubMenu("Misc")
                    .SubMenu("DontUlt")
                    .AddItem(new MenuItem("DontUlt" + enemy.BaseSkinName, enemy.BaseSkinName).SetValue(false));
            
            //Damage after combo:
            var dmgAfterComboItem =  new MenuItem("DamageAfterCombo", "显示 连招 伤害").SetValue(true);
            Utility.HpBarDamageIndicator.DamageToUnit = GetComboDamage;
            Utility.HpBarDamageIndicator.Enabled = dmgAfterComboItem.GetValue<bool>();
            dmgAfterComboItem.ValueChanged += delegate(object sender, OnValueChangeEventArgs eventArgs)
            {
                Utility.HpBarDamageIndicator.Enabled = eventArgs.GetNewValue<bool>();
            };

            //Drawings menu:
            Config.AddSubMenu(new Menu("范围", "Drawings"));
            Config.SubMenu("Drawings")
                .AddItem(new MenuItem("QRange", "Q 范围").SetValue(new Circle(false, Color.FromArgb(100, 255, 0, 255))));
            Config.SubMenu("Drawings")
                .AddItem(new MenuItem("WRange", "W 范围").SetValue(new Circle(true, Color.FromArgb(100, 255, 0, 255))));
            Config.SubMenu("Drawings")
                .AddItem(new MenuItem("ERange", "E 范围").SetValue(new Circle(false, Color.FromArgb(100, 255, 0, 255))));
            Config.SubMenu("Drawings")
                .AddItem(new MenuItem("RRange", "R 范围").SetValue(new Circle(false, Color.FromArgb(100, 255, 0, 255))));
            Config.SubMenu("Drawings")
                .AddItem(new MenuItem("QERange", "QE 范围").SetValue(new Circle(true, Color.FromArgb(100, 255, 0, 255))));
            Config.SubMenu("Drawings")
                .AddItem(dmgAfterComboItem);
            Config.AddToMainMenu();

            //Add the events we are going to use:
            Game.OnUpdate += Game_OnGameUpdate;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Hero_OnProcessSpellCast;
            Drawing.OnDraw += Drawing_OnDraw;
            Interrupter.OnPossibleToInterrupt += Interrupter_OnPossibleToInterrupt;
            Orbwalking.BeforeAttack += Orbwalking_BeforeAttack;
            Game.PrintChat(ChampionName + " Loaded!涓ㄦ眽鍖朆y 鑺辫竟");
        }

        static void Orbwalking_BeforeAttack(Orbwalking.BeforeAttackEventArgs args)
        {
            if (Config.Item("ComboActive").GetValue<KeyBind>().Active)
                args.Process = !(Q.IsReady() || W.IsReady());
        }

        private static void Interrupter_OnPossibleToInterrupt(Obj_AI_Base unit, InterruptableSpell spell)
        {
            if (!Config.Item("InterruptSpells").GetValue<bool>()) return;

            if (Player.Distance(unit) < E.Range && E.IsReady())
            {
                Q.Cast(unit.ServerPosition);
                E.Cast(unit.ServerPosition);
            }
            else if (Player.Distance(unit) < EQ.Range && E.IsReady() && Q.IsReady())
            {
                UseQE(unit);
            }
        }

        private static void Combo()
        {
            UseSpells(Config.Item("UseQCombo").GetValue<bool>(), Config.Item("UseWCombo").GetValue<bool>(),
                Config.Item("UseECombo").GetValue<bool>(), Config.Item("UseRCombo").GetValue<bool>(),
                Config.Item("UseQECombo").GetValue<bool>(), Config.Item("UseIgniteCombo").GetValue<bool>(), false);
        }

        private static void Harass()
        {
            UseSpells(Config.Item("UseQHarass").GetValue<bool>(), Config.Item("UseWHarass").GetValue<bool>(),
                Config.Item("UseEHarass").GetValue<bool>(), false, Config.Item("UseQEHarass").GetValue<bool>(), false, true);
        }

        private static void UseE(Obj_AI_Base enemy)
        {
            foreach (var orb in OrbManager.GetOrbs(true))
                if (Player.Distance(orb) < E.Range + 100)
                {
                    var startPoint = orb.To2D().Extend(Player.ServerPosition.To2D(), 100);
                    var endPoint = Player.ServerPosition.To2D()
                        .Extend(orb.To2D(), Player.Distance(orb) > 200 ? 1300 : 1000);
                    EQ.Delay = E.Delay + Player.Distance(orb) / E.Speed;
                    EQ.From = orb;
                    var enemyPred = EQ.GetPrediction(enemy);
                    if (enemyPred.Hitchance >= HitChance.High &&
                        enemyPred.UnitPosition.To2D().Distance(startPoint, endPoint, false) <
                        EQ.Width + enemy.BoundingRadius)
                    {
                        E.Cast(orb, true);
                        W.LastCastAttemptT = Environment.TickCount;
                        return;
                    }
                }
        }

        private static void UseQE(Obj_AI_Base enemy)
        {
            EQ.Delay = E.Delay + Q.Range / E.Speed;
            EQ.From = Player.ServerPosition.To2D().Extend(enemy.ServerPosition.To2D(), Q.Range).To3D();

            var prediction = EQ.GetPrediction(enemy);
            if (prediction.Hitchance >= HitChance.High)
            {
                Q.Cast(Player.ServerPosition.To2D().Extend(prediction.CastPosition.To2D(), Q.Range - 100));
                QEComboT = Environment.TickCount;
                W.LastCastAttemptT = Environment.TickCount;
            }
        }

        private static Vector3 GetGrabableObjectPos(bool onlyOrbs)
        {
            if (!onlyOrbs)
                foreach (var minion in ObjectManager.Get<Obj_AI_Minion>().Where(minion => minion.IsValidTarget(W.Range))
                    )
                    return minion.ServerPosition;

            return OrbManager.GetOrbToGrab((int)W.Range);
        }

        private static float GetComboDamage(Obj_AI_Base enemy)
        {
            var damage = 0d;

            if (Q.IsReady(420))
                damage += Player.GetSpellDamage(enemy, SpellSlot.Q);

            if (DFG.IsReady())
                damage += Player.GetItemDamage(enemy, Damage.DamageItems.Dfg) / 1.2;

            if (W.IsReady())
                damage += Player.GetSpellDamage(enemy, SpellSlot.W);

            if (E.IsReady())
                damage += Player.GetSpellDamage(enemy, SpellSlot.E);

            if (IgniteSlot != SpellSlot.Unknown && Player.Spellbook.CanUseSpell(IgniteSlot) == SpellState.Ready)
                damage += ObjectManager.Player.GetSummonerSpellDamage(enemy, Damage.SummonerSpell.Ignite);

            if (R.IsReady())
                damage += Math.Min(7, Player.Spellbook.GetSpell(SpellSlot.R).Ammo) * Player.GetSpellDamage(enemy, SpellSlot.R, 1);

            return (float)damage * (DFG.IsReady() ? 1.2f : 1);
        }

        private static void UseSpells(bool useQ, bool useW, bool useE, bool useR, bool useQE, bool useIgnite, bool isHarass)
        {
            var qTarget = TargetSelector.GetTarget(Q.Range + (isHarass ? Q.Width / 3 : Q.Width), TargetSelector.DamageType.Magical);
            var wTarget = TargetSelector.GetTarget(W.Range + W.Width, TargetSelector.DamageType.Magical);
            var rTarget = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Magical);
            var qeTarget = TargetSelector.GetTarget(EQ.Range, TargetSelector.DamageType.Magical);
            var comboDamage = rTarget != null ? GetComboDamage(rTarget) : 0;

            //Q
            if (qTarget != null && useQ)
                Q.Cast(qTarget, false, true);

            //E
            if (Environment.TickCount - W.LastCastAttemptT > Game.Ping + 150 && E.IsReady() && useE)
                foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>())
                {
                    if (enemy.IsValidTarget(EQ.Range))
                        UseE(enemy);
                }

            //W
            if (useW)
                if (Player.Spellbook.GetSpell(SpellSlot.W).ToggleState == 1 && W.IsReady() && qeTarget != null)
                {
                    //WObject
                    var gObjectPos = GetGrabableObjectPos(wTarget == null);

                    if (gObjectPos.To2D().IsValid() && Environment.TickCount - W.LastCastAttemptT > Game.Ping + 100 && Environment.TickCount - E.LastCastAttemptT > Game.Ping + 100)
                    {
                        W.Cast(gObjectPos);
                        W.LastCastAttemptT = Environment.TickCount;
                    }
                }
                else if (wTarget != null && Player.Spellbook.GetSpell(SpellSlot.W).ToggleState != 1 && W.IsReady() &&
                         Environment.TickCount - W.LastCastAttemptT > Game.Ping + 100)
                {
                    if (OrbManager.WObject(false) != null)
                    {
                        W.From = OrbManager.WObject(false).ServerPosition;
                        W.Cast(wTarget, false, true);
                    }
                }

            if (rTarget != null)
                useR = (Config.Item("DontUlt" + rTarget.BaseSkinName) != null &&
                        Config.Item("DontUlt" + rTarget.BaseSkinName).GetValue<bool>() == false) && useR;

            //DFG (and ult if ready)
            if (rTarget != null && useR && comboDamage > rTarget.Health && DFG.IsReady())
            {
                DFG.Cast(rTarget);
                if (R.IsReady())
                {
                    R.Cast(rTarget);
                }
            }

            //R
            if (rTarget != null && useR && R.IsReady() && !Q.IsReady() && !DFG.IsReady())
            {
                if (comboDamage > rTarget.Health)
                {
                    R.Cast(rTarget);
                }
            }

            //Ignite
            if (rTarget != null && useIgnite && IgniteSlot != SpellSlot.Unknown &&
                Player.Spellbook.CanUseSpell(IgniteSlot) == SpellState.Ready)
            {
                if (comboDamage > rTarget.Health)
                {
                    Player.Spellbook.CastSpell(IgniteSlot, rTarget);
                }
            }

            //QE
            if (wTarget == null && qeTarget != null && Q.IsReady() && E.IsReady() && useQE)
                UseQE(qeTarget);

            //WE
            if (wTarget == null && qeTarget != null && E.IsReady() && useE && OrbManager.WObject(true) != null)
            {
                EQ.Delay = E.Delay + Q.Range / W.Speed;
                EQ.From = Player.ServerPosition.To2D().Extend(qeTarget.ServerPosition.To2D(), Q.Range).To3D();
                var prediction = EQ.GetPrediction(qeTarget);
                if (prediction.Hitchance >= HitChance.High)
                {
                    W.Cast(Player.ServerPosition.To2D().Extend(prediction.CastPosition.To2D(), Q.Range - 100));
                    WEComboT = Environment.TickCount;
                }
            }
        }

        private static void Obj_AI_Hero_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsMe && Environment.TickCount - QEComboT < 500 &&
                (args.SData.Name == "SyndraQ"))
            {
                W.LastCastAttemptT = Environment.TickCount + 400;
                E.Cast(args.End, true);
            }

            if (sender.IsMe && Environment.TickCount - WEComboT < 500 &&
                (args.SData.Name == "SyndraW" || args.SData.Name == "syndrawcast"))
            {
                W.LastCastAttemptT = Environment.TickCount + 400;
                E.Cast(args.End, true);
            }
        }

        private static void Farm(bool laneClear)
        {
            if (!Orbwalking.CanMove(40)) return;

            var rangedMinionsQ = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range + Q.Width + 30,
                MinionTypes.Ranged);
            var allMinionsQ = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range + Q.Width + 30,
                MinionTypes.All);
            var rangedMinionsW = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, W.Range + W.Width + 30,
                MinionTypes.Ranged);
            var allMinionsW = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, W.Range + W.Width + 30,
                MinionTypes.All);

            var useQi = Config.Item("UseQFarm").GetValue<StringList>().SelectedIndex;
            var useWi = Config.Item("UseWFarm").GetValue<StringList>().SelectedIndex;
            var useEi = Config.Item("UseEFarm").GetValue<StringList>().SelectedIndex;
            var useQ = (laneClear && (useQi == 1 || useQi == 2)) || (!laneClear && (useQi == 0 || useQi == 2));
            var useW = (laneClear && (useWi == 1 || useWi == 2)) || (!laneClear && (useWi == 0 || useWi == 2));
            var useE = (laneClear && (useEi == 1 || useEi == 2)) || (!laneClear && (useEi == 0 || useEi == 2));

            if (useQ && Q.IsReady())
                if (laneClear)
                {
                    var fl1 = Q.GetCircularFarmLocation(rangedMinionsQ, Q.Width);
                    var fl2 = Q.GetCircularFarmLocation(allMinionsQ, Q.Width);

                    if (fl1.MinionsHit >= 3)
                    {
                        Q.Cast(fl1.Position);
                    }

                    else if (fl2.MinionsHit >= 2 || allMinionsQ.Count == 1)
                    {
                        Q.Cast(fl2.Position);
                    }
                }
                else
                    foreach (var minion in allMinionsQ)
                        if (!Orbwalking.InAutoAttackRange(minion) &&
                            minion.Health < 0.75 * Player.GetSpellDamage(minion, SpellSlot.Q))
                            Q.Cast(minion);

            if (useW && W.IsReady() && allMinionsW.Count > 3)
            {
                if (laneClear)
                {
                    if (Player.Spellbook.GetSpell(SpellSlot.W).ToggleState == 1)
                    {
                        //WObject
                        var gObjectPos = GetGrabableObjectPos(false);

                        if (gObjectPos.To2D().IsValid() && Environment.TickCount - W.LastCastAttemptT > Game.Ping + 150)
                        {
                            W.Cast(gObjectPos);
                        }
                    }
                    else if (Player.Spellbook.GetSpell(SpellSlot.W).ToggleState != 1)
                    {
                        var fl1 = Q.GetCircularFarmLocation(rangedMinionsW, W.Width);
                        var fl2 = Q.GetCircularFarmLocation(allMinionsW, W.Width);

                        if (fl1.MinionsHit >= 3 && W.IsInRange(fl1.Position.To3D()))
                        {
                            W.Cast(fl1.Position);
                        }

                        else if (fl2.MinionsHit >= 1 && W.IsInRange(fl2.Position.To3D()) && fl1.MinionsHit <= 2)
                        {
                            W.Cast(fl2.Position);
                        }
                    }
                }
            }
        }

        private static void JungleFarm()
        {
            var useQ = Config.Item("UseQJFarm").GetValue<bool>();
            var useW = Config.Item("UseWJFarm").GetValue<bool>();
            var useE = Config.Item("UseEJFarm").GetValue<bool>();

            var mobs = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, W.Range, MinionTypes.All,
                MinionTeam.Neutral, MinionOrderTypes.MaxHealth);

            if (mobs.Count > 0)
            {
                var mob = mobs[0];

                if (Q.IsReady() && useQ)
                {
                    Q.Cast(mob);
                }

                if (W.IsReady() && useW && Environment.TickCount - Q.LastCastAttemptT > 800)
                {
                    W.Cast(mob);
                }

                if (useE && E.IsReady())
                {
                    E.Cast(mob);
                }
            }
        }


        private static void Game_OnGameUpdate(EventArgs args)
        {
            if (Player.IsDead) return;

            //Update the R range
            R.Range = R.Level == 3 ? 750 : 675;
           
            if (Config.Item("CastQE").GetValue<KeyBind>().Active && E.IsReady() && Q.IsReady())
                foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>())
                    if (enemy.IsValidTarget(EQ.Range) && Game.CursorPos.Distance(enemy.ServerPosition) < 300)
                        UseQE(enemy);

            if (Config.Item("ComboActive").GetValue<KeyBind>().Active)
            {
                Combo();
            }
            else
            {
                if (Config.Item("HarassActive").GetValue<KeyBind>().Active ||
                    Config.Item("HarassActiveT").GetValue<KeyBind>().Active)
                    Harass();

                var lc = Config.Item("LaneClearActive").GetValue<KeyBind>().Active;
                if (lc || Config.Item("FreezeActive").GetValue<KeyBind>().Active)
                    Farm(lc);

                if (Config.Item("JungleFarmActive").GetValue<KeyBind>().Active)
                    JungleFarm();
            }
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            //Draw the ranges of the spells.
            var menuItem = Config.Item("QERange").GetValue<Circle>();
            if (menuItem.Active) Render.Circle.DrawCircle(Player.Position, EQ.Range, menuItem.Color);

            foreach (var spell in SpellList)
            {
                menuItem = Config.Item(spell.Slot + "Range").GetValue<Circle>();
                if (menuItem.Active)
                    Render.Circle.DrawCircle(Player.Position, spell.Range, menuItem.Color);
            }
            //if (OrbManager.WObject(false) != null)
            //    Render.Circle.DrawCircle(OrbManager.WObject(false).Position, 100, Color.White);
        }
    }
}
