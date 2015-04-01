/*
 * User: GoldenGates
 * Date: 2015-01-19
 * Time: 8:27 PM
 */
using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace DatRenekton
{
    class Program
    {
        static Obj_AI_Hero Player { get { return ObjectManager.Player; } }

        static Orbwalking.Orbwalker Orbwalker;
        static Spell Q, W, E, R;
        static Items.Item Tiamat;
        static Items.Item Hydra;
        static Menu Menu;
        const string rageBuffName = "renektonrageready";
        const string wBuffName = "renektonpreexecute";
        const string e2BuffName = "renektonsliceanddicedelay";
        const string rBuffName = "renektonreignofthetyrant";

        public static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        static void Game_OnGameLoad(EventArgs args)
        {
            if (Player.ChampionName != "Renekton")
                return;
            Q = new Spell(SpellSlot.Q, 225);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E, 450);
            R = new Spell(SpellSlot.R);
            Tiamat = new Items.Item((int)ItemId.Tiamat_Melee_Only, 420);
            Hydra = new Items.Item((int)ItemId.Ravenous_Hydra_Melee_Only, 420);

            Menu = new Menu("花边-Dat鳄鱼", Player.ChampionName, true);

            Menu orbwalkerMenu = Menu.AddSubMenu(new Menu("走砍", "Orbwalker"));
            Orbwalker = new Orbwalking.Orbwalker(orbwalkerMenu);

            Menu tsMenu = Menu.AddSubMenu(new Menu("目标选择", "TS"));
            TargetSelector.AddToMenu(tsMenu);

            Menu spellsMenu = Menu.AddSubMenu(new Menu("技能", "spellsMenu"));

            Menu comboMenu = spellsMenu.AddSubMenu(new Menu("连招", "comboSpells"));
            comboMenu.AddItem(new MenuItem("comboUseQ", "使用 Q").SetValue(true));
            comboMenu.AddItem(new MenuItem("comboUseW", "使用 W").SetValue(true));
            comboMenu.AddItem(new MenuItem("comboUseE", "使用 E").SetValue(true));
            comboMenu.AddItem(new MenuItem("comboUseR", "使用 R").SetValue(true));
            comboMenu.AddItem(new MenuItem("comboSliderR", "使用 R丨当前Hp百分比 (%)").SetValue(new Slider(30, 1, 100)));

            Menu laneClearMenu = spellsMenu.AddSubMenu(new Menu("清线", "laneClearSpells"));
            laneClearMenu.AddItem(new MenuItem("laneClearUseQ", "使用 Q").SetValue(true));

            Menu mixedMenu = spellsMenu.AddSubMenu(new Menu("骚扰", "mixedSpells"));
            mixedMenu.AddItem(new MenuItem("mixedUseQ", "使用 Q").SetValue(true));
            mixedMenu.AddItem(new MenuItem("mixedUseW", "使用 W").SetValue(true));

            Menu drawMenu = Menu.AddSubMenu(new Menu("显示", "drawing"));
            drawMenu.AddItem(new MenuItem("drawQ", "显示 Q 范围").SetValue(true));
            drawMenu.AddItem(new MenuItem("drawIt", "显示 Balls (危险点)").SetValue(false));

            Menu msgMenu = Menu.AddSubMenu(new Menu("显示", "message"));
            msgMenu.AddItem(new MenuItem("Sprite", "Dat鳄鱼"));
            msgMenu.AddItem(new MenuItem("Hanhua", "汉化:花边下丶情未央"));
            msgMenu.AddItem(new MenuItem("qqqun", "QQ群:299606556"));

            Menu.AddToMainMenu();
            Drawing.OnDraw += Drawing_OnDraw;
            Orbwalking.AfterAttack += Orbwalking_AfterAttack;
            Game.OnUpdate += Game_OnGameUpdate;
            Game.PrintChat("<font color ='#33FFFF'>Dat Renekton</font> by GoldenGates loaded, enjoy! Best used with an activator and evader!");

            Game.PrintChat("<font color=\"#1eff00\">Huabian婕㈠寲QQ缇わ細299606556</font> - <font color=\"#00BFFF\">姝¤繋鍚勪綅鐨勫姞鍏ワ紒</font>");

        }

        static void Game_OnGameUpdate(EventArgs args)
        {
            if (Player.IsDead)
                return;
            Checks();
            Obj_AI_Hero target = TargetSelector.GetTarget(450, TargetSelector.DamageType.Physical);
            switch (Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    if (target != null)
                    {
                        if (Player.HealthPercent < Menu.Item("comboSliderR").GetValue<Slider>().Value && R.IsReady())
                            R.Cast();
                        if (Menu.Item("comboUseQ").GetValue<bool>())
                            useQ(target);
                        if (Menu.Item("comboUseE").GetValue<bool>())
                        {
                            if (Player.Distance(target.Position) > 225)
                                useE(target);
                        }
                    }
                    break;
                case Orbwalking.OrbwalkingMode.LaneClear:
                    Obj_AI_Base minion = MinionManager.GetMinions(Player.Position, 225).FirstOrDefault();
                    if (Menu.Item("laneClearUseQ").GetValue<bool>())
                        useQ(minion);
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    if (Menu.Item("mixedUseQ").GetValue<bool>())
                        useQ(target);
                    break;
            }

        }

        static void Drawing_OnDraw(EventArgs args)
        {
            if (Menu.Item("drawQ").GetValue<bool>())

                Render.Circle.DrawCircle(Player.Position, Player.BoundingRadius + 225, Color.Orange);
            if (Menu.Item("drawIt").GetValue<bool>())
            {
                Render.Circle.DrawCircle(new Vector3(Player.Position.X - 125, Player.Position.Y, Player.Position.Z), 100, Color.Red);
                Render.Circle.DrawCircle(new Vector3(Player.Position.X + 75, Player.Position.Y, Player.Position.Z), 100, Color.Red);

            }
        }

        static void Orbwalking_AfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
            {
                if (Menu.Item("comboUseW").GetValue<bool>() && W.IsReady() && target.IsEnemy)
                    W.Cast();
                if (Tiamat.IsOwned() && Player.Distance(target) < Tiamat.Range && Tiamat.IsReady())
                    Tiamat.Cast();
                if (Hydra.IsOwned() && Player.Distance(target) < Hydra.Range && Hydra.IsReady())
                    Hydra.Cast();
            }
        }

        static void Checks()
        {
        }

        static void useQ(Obj_AI_Base unit)
        {
            if (!Q.IsReady())
                return;
            if (unit != null && Player.Distance(unit.Position) < 225)
            {
                Q.Cast();
            }

        }


        static void useE(Obj_AI_Base target)
        {
            Obj_AI_Base minion = MinionManager.GetMinions(Player.Position, 225).FirstOrDefault();
            if (!E.IsReady())
                return;
            if (target != null && Player.Distance(target.Position) < E.Range)
            {
                E.Cast(target.Position);
            }
            else if (target != null && Player.Distance(target.Position) < 800 && minion != null && Player.Distance(minion.Position) < E.Range && target.Distance(minion.Position) < E.Range)
            {
                E.Cast(minion.Position);
                if (Player.HasBuff(e2BuffName, true, true))
                    E.Cast(target.Position);
            }
        }

    }
}