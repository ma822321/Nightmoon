#region

using System;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Threading;
using LeagueSharp;
using LeagueSharp.Common;

#endregion

namespace Kayle
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                CustomEvents.Game.OnGameLoad += delegate
                {
                    var onGameLoad = new Thread(Game_OnGameLoad);
                    onGameLoad.Start();
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private static void Game_OnGameLoad()
        {
            if (Variable.Player.ChampionName != Variable.ChampionName) return;

            Variable.Q = new Spell(SpellSlot.Q, 650f);
            Variable.W = new Spell(SpellSlot.W, 900f);
            Variable.E = new Spell(SpellSlot.E, 625f);
            Variable.R = new Spell(SpellSlot.R, 900f);

            Variable.IgniteSlot = Variable.Player.GetSpellSlot("summonerdot");

            Variable.Dfg = Utility.Map.GetMap().Type == Utility.Map.MapType.TwistedTreeline
                ? new Items.Item(3188, 750)
                : new Items.Item(3128, 750);

            Variable.SpellList.Add(Variable.Q);
            Variable.SpellList.Add(Variable.W);
            Variable.SpellList.Add(Variable.E);
            Variable.SpellList.Add(Variable.R);

            Variable.Config = new Menu("花边汉化-RoachXD凯尔", Variable.ChampionName, true);

            Variable.Config.AddSubMenu(new Menu("走 砍", "Orbwalking"));

            var tsMenu = new Menu("目标 选择", "Target Selector");
            TargetSelector.AddToMenu(tsMenu);
            Variable.Config.AddSubMenu(tsMenu);

            Variable.Orbwalker = new Orbwalking.Orbwalker(Variable.Config.SubMenu("Orbwalking"));

            Variable.Config.AddSubMenu(new Menu("连 招", "Combo"));
            Variable.Config.SubMenu("Combo").AddItem(new MenuItem("UseQC", "使用 Q").SetValue(true));
            Variable.Config.SubMenu("Combo").AddItem(new MenuItem("UseWC", "使用 W").SetValue(false));
            Variable.Config.SubMenu("Combo").AddItem(new MenuItem("UseEC", "使用 E").SetValue(true));
            Variable.Config.SubMenu("Combo").AddItem(new MenuItem("UseIgniteC", "使用 点燃").SetValue(true));
            Variable.Config.SubMenu("Combo")
                .AddItem(new MenuItem("CMinions", "目标离开攻击范围自动A小兵溅射").SetValue(true));
            Variable.Config.SubMenu("Combo").AddItem(new MenuItem("ComboActive", "连招 按键!").SetValue(
                new KeyBind(Variable.Config.Item("Orbwalk").GetValue<KeyBind>().Key, KeyBindType.Press)));

            Variable.Config.AddSubMenu(new Menu("骚 扰", "Harass"));
            Variable.Config.SubMenu("Harass").AddItem(new MenuItem("UseQH", "使用 Q").SetValue(true));
            Variable.Config.SubMenu("Harass").AddItem(new MenuItem("UseWH", "使用 W").SetValue(false));
            Variable.Config.SubMenu("Harass").AddItem(new MenuItem("UseEH", "使用 E").SetValue(false));
            Variable.Config.SubMenu("Harass")
                .AddItem(new MenuItem("HMinions", "目标离开攻击范围自动A小兵溅射").SetValue(true));
            Variable.Config.SubMenu("Harass").AddItem(new MenuItem("HarassActive", "骚扰 (按键)!").SetValue(
                new KeyBind(Variable.Config.Item("Farm").GetValue<KeyBind>().Key, KeyBindType.Press)));
            Variable.Config.SubMenu("Harass").AddItem(new MenuItem("HarassActiveT", "骚扰 (自动)!").SetValue(
                new KeyBind("T".ToCharArray()[0], KeyBindType.Toggle)));

            Variable.Config.AddSubMenu(new Menu("打 钱", "Farm"));
            Variable.Config.SubMenu("Farm").AddItem(new MenuItem("UseQF", "使用 Q").SetValue(
                new StringList(new[] {"Freeze", "LaneClear", "Both", "No"}, 1)));
            Variable.Config.SubMenu("Farm").AddItem(new MenuItem("UseEF", "使用 E").SetValue(
                new StringList(new[] {"Freeze", "LaneClear", "Both", "No"}, 2)));
            Variable.Config.SubMenu("Farm").AddItem(new MenuItem("FreezeActive", "控 线!").SetValue(
                new KeyBind(Variable.Config.Item("Farm").GetValue<KeyBind>().Key, KeyBindType.Press)));
            Variable.Config.SubMenu("Farm").AddItem(new MenuItem("LaneClearActive", "清 线!").SetValue(
                new KeyBind(Variable.Config.Item("LaneClear").GetValue<KeyBind>().Key, KeyBindType.Press)));

            Variable.Config.AddSubMenu(new Menu("清 野", "JungleFarm"));
            Variable.Config.SubMenu("JungleFarm").AddItem(new MenuItem("UseQJ", "使用 Q").SetValue(true));
            Variable.Config.SubMenu("JungleFarm").AddItem(new MenuItem("UseEJ", "使用 E").SetValue(true));
            Variable.Config.SubMenu("JungleFarm").AddItem(new MenuItem("JungleFarmActive", "清野 (按键)!").SetValue(
                new KeyBind(Variable.Config.Item("LaneClear").GetValue<KeyBind>().Key, KeyBindType.Press)));

            Variable.Config.AddSubMenu(new Menu("大 招", "Ultimate"));
            Variable.Config.SubMenu("Ultimate").AddSubMenu(new Menu("给友军", "Allies"));
            foreach (var ally in ObjectManager.Get<Obj_AI_Hero>()
                .Where(ally => ally.IsAlly))
                Variable.Config.SubMenu("Ultimate")
                    .SubMenu("Allies")
                    .AddItem(new MenuItem("Ult" + ally.ChampionName, ally.ChampionName)
                        .SetValue(ally.ChampionName == Variable.Player.ChampionName));
            Variable.Config.SubMenu("Ultimate")
                .AddItem(new MenuItem("UltMinHP", "最低HP百分比").SetValue(new Slider(20, 1)));

            Variable.Config.AddSubMenu(new Menu("治 疗", "Heal"));
            Variable.Config.SubMenu("Heal").AddSubMenu(new Menu("友 军", "Allies"));
            foreach (var ally in ObjectManager.Get<Obj_AI_Hero>()
                .Where(ally => ally.IsAlly))
            {
                Variable.Config.SubMenu("Heal")
                    .SubMenu("Allies")
                    .AddItem(new MenuItem("Heal" + ally.ChampionName, ally.ChampionName)
                        .SetValue(ally.ChampionName == Variable.Player.ChampionName));
            }
            Variable.Config.SubMenu("Heal")
                .AddItem(new MenuItem("HealMinHP", "最低HP百分比").SetValue(new Slider(40, 1)));


            Variable.Config.AddSubMenu(new Menu("杂 项", "Misc"));
            Variable.Config.SubMenu("Misc").AddItem(new MenuItem("UsePackets", "使用 封包").SetValue(true));
            Variable.Config.SubMenu("Misc").AddItem(new MenuItem("SupportMode", "辅助 模式").SetValue(false));

            var comboDmg = new MenuItem("ComboDamage", "显示 连招 伤害").SetValue(true);
            Utility.HpBarDamageIndicator.DamageToUnit = Internal.ComboDamage;
            Utility.HpBarDamageIndicator.Enabled = comboDmg.GetValue<bool>();
            comboDmg.ValueChanged +=
                delegate(object sender, OnValueChangeEventArgs eventArgs)
                {
                    Utility.HpBarDamageIndicator.Enabled = eventArgs.GetNewValue<bool>();
                };

            Variable.Config.AddSubMenu(new Menu("范 围", "Drawings"));
            Variable.Config.SubMenu("Drawings").AddItem(new MenuItem("QRange", "Q 范围").SetValue(
                new Circle(false, Color.FromArgb(100, 255, 0, 255))));
            Variable.Config.SubMenu("Drawings").AddItem(new MenuItem("WRange", "W 范围").SetValue(
                new Circle(true, Color.FromArgb(100, 255, 0, 255))));
            Variable.Config.SubMenu("Drawings").AddItem(new MenuItem("ERange", "E 范围").SetValue(
                new Circle(true, Color.FromArgb(100, 255, 0, 255))));
            Variable.Config.SubMenu("Drawings").AddItem(new MenuItem("RRange", "R 范围").SetValue(
                new Circle(false, Color.FromArgb(100, 255, 0, 255))));
            Variable.Config.SubMenu("Drawings").AddItem(comboDmg);

            Variable.Config.AddToMainMenu();

            Game.PrintChat("<font color=\"#00BFFF\">鑺辫竟姹夊寲-RoachXD鍑皵</font> - <font color=\"#FFFFFF\">鍔犺浇鎴愬姛!</font>");

            Game.OnGameUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            if (Variable.Player.IsDead)
            {
                return;
            }

            if ((((Obj_AI_Base) Variable.Orbwalker.GetTarget()).IsMinion))
            {
                Variable.Orbwalker.SetAttack(!Variable.Config.Item("SupportMode").GetValue<bool>());
            }

            if (Variable.Config.Item("ComboActive").GetValue<KeyBind>().Active)
            {
                Internal.Combo();
            }
            else
            {
                if (Variable.Config.Item("HarassActive").GetValue<KeyBind>().Active ||
                    Variable.Config.Item("HarassActiveT").GetValue<KeyBind>().Active)
                {
                    Internal.Harass();
                }

                var laneClear = Variable.Config.Item("LaneClearActive").GetValue<KeyBind>().Active;
                if ((laneClear || Variable.Config.Item("FreezeActive").GetValue<KeyBind>().Active) &&
                    !Variable.Config.Item("SupportMode").GetValue<bool>())
                {
                    Internal.Farm(laneClear);
                }

                if (Variable.Config.Item("JungleFarmActive").GetValue<KeyBind>().Active)
                {
                    Internal.JungleFarm();
                }
            }

            Internal.Ultimate();
            Internal.Heal();
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            Internal.DrawCircles();

            var target = TargetSelector.GetTarget(Variable.W.Range, TargetSelector.DamageType.Magical);
            var eDamage = 20 + ((Variable.E.Level - 1)*10) + (Variable.Player.BaseAbilityDamage*0.25);
            if (Variable.Config.Item("ComboDamage").GetValue<bool>())
            {
                Drawing.DrawText(target.Position.X, target.Position.Y, Color.White,
                    ((target.Health - Internal.ComboDamage(target))/
                     (Variable.RighteousFuryActive ? (eDamage) : (Variable.Player.GetAutoAttackDamage(target))))
                        .ToString(
                            CultureInfo.InvariantCulture));
            }
        }
    }
}