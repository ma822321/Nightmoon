#region

using System;
using System.Collections.Generic;
using Color = System.Drawing.Color;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

#endregion

namespace Akali
{
    internal class Program
    {
        public const string ChampionName = "Akali";

        public static Orbwalking.Orbwalker Orbwalker;

        public static List<Spell> SpellList = new List<Spell>();

        public static Spell Q;
        public static Spell W;
        public static Spell E;
        public static Spell R;

        //public static SpellSlot IgniteSlot;
        public static Items.Item Hex;
        public static Items.Item Dfg;
        public static Items.Item Cutlass;

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
            Game.PrintChat("Akali by Chogart Loaded");

            Q = new Spell(SpellSlot.Q, 600);
            W = new Spell(SpellSlot.W, 700);
            E = new Spell(SpellSlot.E, 325);
            R = new Spell(SpellSlot.R, 800);

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);

             //IgniteSlot = Player.GetSpellSlot("SummonerDot");
            Hex = new Items.Item(3146, 700);
            Dfg = new Items.Item(3128, 750);
            Cutlass = new Items.Item(3144, 450);

            Config = new Menu("花边汉化-Chogart阿卡丽", ChampionName, true);

            var targetSelectorMenu = new Menu("目标 选择", "Target Selector");
            TargetSelector.AddToMenu(targetSelectorMenu);
            Config.AddSubMenu(targetSelectorMenu);

            Config.AddSubMenu(new Menu("走 砍", "Orbwalking"));

            Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalking"));

            Config.AddSubMenu(new Menu("连 招", "Combo"));
            Config.SubMenu("Combo").AddItem(new MenuItem("HEX", "使用 科技枪").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("DFG", "使用 冥火").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("Cutlass", "使用 水银弯刀").SetValue(true));
            Config.SubMenu("Combo")
                .AddItem(new MenuItem("ComboActive", "连招 按键!").SetValue(new KeyBind(32, KeyBindType.Press)));

            Config.AddSubMenu(new Menu("骚 扰", "Harass"));
            Config.SubMenu("Harass").AddItem(new MenuItem("UseQHarass", "使用 Q").SetValue(true));
            Config.SubMenu("Harass").AddItem(new MenuItem("UseEHarass", "使用 E").SetValue(true));
            Config.SubMenu("Harass")
                .AddItem(
                    new MenuItem("HarassActive", "骚扰 按键!").SetValue(new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));

            Config.AddSubMenu(new Menu("打 钱", "Farm"));
            Config.SubMenu("Farm")
                .AddItem(
                    new MenuItem("UseQFarm", "使用 Q").SetValue(
                        new StringList(new[] { "控线", "清线", "两者", "禁止" }, 2)));
            Config.SubMenu("Farm")
                .AddItem(
                    new MenuItem("UseEFarm", "使用 E").SetValue(
                        new StringList(new[] { "控线", "清线", "两者", "禁止" }, 1)));
            Config.SubMenu("Farm")
                .AddItem(
                    new MenuItem("FreezeActive", "控线 按键!").SetValue(new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));
            Config.SubMenu("Farm")
                .AddItem(
                    new MenuItem("LaneClearActive", "清线 按键!").SetValue(new KeyBind("V".ToCharArray()[0],
                        KeyBindType.Press)));

            Config.AddSubMenu(new Menu("清 野", "JungleFarm"));
            Config.SubMenu("JungleFarm").AddItem(new MenuItem("UseQJFarm", "使用 Q").SetValue(true));
            Config.SubMenu("JungleFarm").AddItem(new MenuItem("UseEJFarm", "使用 E").SetValue(true));
            Config.SubMenu("JungleFarm")
                .AddItem(
                    new MenuItem("JungleFarmActive", "清野 按键!").SetValue(new KeyBind("V".ToCharArray()[0],
                        KeyBindType.Press)));

            Config.AddSubMenu(new Menu("杂 项", "Misc"));
            Config.SubMenu("Misc").AddItem(new MenuItem("KillstealR", "R 抢人头").SetValue(false));

            
            Config.AddSubMenu(new Menu("范 围", "Drawings"));
            Config.SubMenu("Drawings")
                .AddItem(new MenuItem("QRange", "Q 范围").SetValue(new Circle(true, Color.FromArgb(255, 255, 255, 255))));
            Config.SubMenu("Drawings")
                .AddItem(
                    new MenuItem("RRange", "R 范围").SetValue(new Circle(true, Color.FromArgb(255, 255, 255, 255))));

            Config.AddToMainMenu();

            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnGameUpdate += Game_OnGameUpdate;
            Game.PrintChat("鑺辫竟姹夊寲-Chogart闃垮崱涓戒辅鍔犺浇鎴愬姛!");
        }
        private static void Drawing_OnDraw(EventArgs args)
        {
            var menuItem = Config.Item("RRange").GetValue<Circle>();
            if (menuItem.Active) Utility.DrawCircle(Player.Position, R.Range, menuItem.Color);

            var menuItem2 = Config.Item("QRange").GetValue<Circle>();
            if (menuItem2.Active) Utility.DrawCircle(Player.Position, Q.Range, menuItem2.Color);
        }
        private static void Game_OnGameUpdate(EventArgs args)
        {
            Orbwalker.SetAttack(true);
            if (Config.Item("ComboActive").GetValue<KeyBind>().Active)
            {
                Combo();
            }
            else
            {
                if (Config.Item("HarassActive").GetValue<KeyBind>().Active)
                    Harass();

                var lc = Config.Item("LaneClearActive").GetValue<KeyBind>().Active;
                if (lc || Config.Item("FreezeActive").GetValue<KeyBind>().Active)
                    Farm(lc);

                if (Config.Item("JungleFarmActive").GetValue<KeyBind>().Active)
                    JungleFarm();
            }
            if (Config.Item("KillstealR").GetValue<bool>())
            {
                Killsteal();
            }
        }
        private static void Combo()
        {
            Obj_AI_Hero target = TargetSelector.GetTarget(R.Range, 0);
            Orbwalker.SetAttack(!R.IsReady() && !Q.IsReady() && !E.IsReady() && Geometry.Distance(Player, target) < 800f);
            bool value = Config.Item("HEX").GetValue<bool>();
            bool value2 = Config.Item("DFG").GetValue<bool>();
            bool value3 = Config.Item("Cutlass").GetValue<bool>();
            if (target != null)
            {
                if (Geometry.Distance(Player, target) <= 800f)
                {
                    if (Geometry.Distance(Player, target) >= 630f && R.IsReady())
                    {
                        R.CastOnUnit(target, true);
                        if (Q.IsReady())
                        {
                            Q.CastOnUnit(target, true);
                        }
                        if (E.IsReady())
                        {
                            E.CastOnUnit(target, true);
                        }
                    }
                    else
                    {
                        if (Q.IsReady() && Geometry.Distance(Player, target) <= 600f)
                        {
                            Q.CastOnUnit(target, true);
                            if (R.IsReady())
                            {
                                R.CastOnUnit(target, true);
                            }
                            if (E.IsReady())
                            {
                                E.CastOnUnit(target, true);
                            }
                        }
                        else
                        {
                            if (R.IsReady())
                            {
                                R.CastOnUnit(target, true);
                                if (value &&  Hex.IsReady())
                                {
                                    Hex.Cast(target);
                                }
                            }
                            if (E.IsReady())
                            {
                                E.CastOnUnit(target, true);
                            }
                            if (value2 && Dfg.IsReady())
                            {
                                Dfg.Cast(target);
                            }
                            if (value3 && Cutlass.IsReady())
                            {
                                Cutlass.Cast(target);
                            }
                        }
                    }
                }
                else
                {
                    if (Damage.GetSpellDamage(Player, target, SpellSlot.Q, 4) > target.Health)
                    {
                        Q.CastOnUnit(target, true);
                    }
                }
            }


            }
        
        private static void Harass()
        {
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);

            if (target != null)
            {
                Q.CastOnUnit(target);
            }
            if (Player.Distance(target) <= 325 && E.IsReady())
            {
                E.CastOnUnit(target);
            }
        }
        private static void Farm(bool laneClear)
        {
            if (!Orbwalking.CanMove(40)) return;
            var allMinions = MinionManager.GetMinions(Player.ServerPosition, Q.Range);
            var useQi = Config.Item("UseQFarm").GetValue<StringList>().SelectedIndex;
            var useEi = Config.Item("UseEFarm").GetValue<StringList>().SelectedIndex;
            var useQ = (laneClear && (useQi == 1 || useQi == 2)) || (!laneClear && (useQi == 0 || useQi == 2));
            var useE = (laneClear && (useEi == 1 || useEi == 2)) || (!laneClear && (useEi == 0 || useEi == 2));

            if (useQ && Q.IsReady())
            {
                foreach (var minion in allMinions)
                {
                    if (minion.IsValidTarget() &&
                        HealthPrediction.GetHealthPrediction(minion,
                            (int)(Player.Distance(minion) * 1000 / 1400)) <
                        0.75 * Damage.GetSpellDamage(Player, minion, SpellSlot.Q))
                    {
                        Q.CastOnUnit(minion);
                        return;
                    }
                }
            }
            else if (useE && E.IsReady())
            {
                foreach (var minion in allMinions)
                {
                    if (minion.IsValidTarget(E.Range) &&
                        minion.Health < 0.75 * Damage.GetSpellDamage(Player, minion, SpellSlot.E))
                    {
                        E.CastOnUnit(minion);
                        return;
                    }
                }
            }


            if (laneClear)
            {
                foreach (var minion in allMinions)
                {
                    if (useQ)
                        Q.CastOnUnit(minion);

                    if (useE)
                        E.CastOnUnit(minion);
                }
            }
        }
        private static void JungleFarm()
        {
            var mobs = MinionManager.GetMinions(Player.ServerPosition, Q.Range,
                MinionTypes.All,
                MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
            if (mobs.Count > 0)
            {
                var mob = mobs[0];
                Q.CastOnUnit(mob);
                E.CastOnUnit(mob);
            }
        }
        private static void Killsteal()
        {
            var useR = Config.Item("KillstealR").GetValue<bool>() && R.IsReady();
            if (useR)
            {
                foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsValidTarget(R.Range)))
                {
                    if (R.IsReady() && hero.Distance(ObjectManager.Player) <= R.Range &&
                        Damage.GetSpellDamage(Player, hero, SpellSlot.R) >= hero.Health)
                        R.CastOnUnit(hero, true);
                }
            }
        }
        
     }
}

