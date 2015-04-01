﻿using LeagueSharp;
using LeagueSharp.Common;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyRyze
{
    class EasyRyze : Champion
    {
        static void Main(string[] args)
        {
            new EasyRyze();
        }

        public EasyRyze() : base("Ryze")
        {
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
        }

        protected override void InitializeSkins(ref SkinManager Skins)
        {
            Skins.Add("Ryze");
            Skins.Add("Human Ryze");
            Skins.Add("Tribal Ryze");
            Skins.Add("Uncle Ryze");
            Skins.Add("Triumphant Ryze");
            Skins.Add("Professor Ryze");
            Skins.Add("Zombie Ryze");
            Skins.Add("Dark Crystal Ryze");
            Skins.Add("Pirate Ryze");
        }
        protected override void InitializeSpells(ref SpellManager Spells)
        {
            Spell Q = new Spell(SpellSlot.Q, 625);
            Spell W = new Spell(SpellSlot.W, 600);
            Spell E = new Spell(SpellSlot.E, 600);
            Spell R = new Spell(SpellSlot.R, 625);

            Spells.Add("Q", Q);
            Spells.Add("W", W);
            Spells.Add("E", E);
            Spells.Add("R", R);
        }
        protected override void InitializeMenu()
        {
            Menu.AddSubMenu(new Menu("连 招", "Combo"));
            Menu.SubMenu("Combo").AddItem(new MenuItem("Combo_smart", "智能连招开启").SetValue(true));
            Menu.SubMenu("Combo").AddItem(new MenuItem("Combo_q", "使用 Q").SetValue(true));
            Menu.SubMenu("Combo").AddItem(new MenuItem("Combo_w", "使用 W").SetValue(true));
            Menu.SubMenu("Combo").AddItem(new MenuItem("Combo_e", "使用 E").SetValue(true));
            Menu.SubMenu("Combo").AddItem(new MenuItem("Combo_r", "可击杀时 使用 R").SetValue(true));
            Menu.SubMenu("Combo").AddItem(new MenuItem("Combo_packet", "使用 封包").SetValue(true));

            Menu.AddSubMenu(new Menu("骚 扰", "Harass"));
            Menu.SubMenu("Harass").AddItem(new MenuItem("Harass_q", "使用 Q").SetValue(true));
            Menu.SubMenu("Harass").AddItem(new MenuItem("Harass_w", "使用 W").SetValue(false));
            Menu.SubMenu("Harass").AddItem(new MenuItem("Harass_e", "使用 E").SetValue(false));
            Menu.SubMenu("Harass").AddItem(new MenuItem("Harass_packet", "使用 封包").SetValue(false));

            Menu.AddSubMenu(new Menu("自 动", "Auto"));
            Menu.SubMenu("Auto").AddItem(new MenuItem("Auto_q", "使用 Q").SetValue(true));
            Menu.SubMenu("Auto").AddItem(new MenuItem("Auto_w", "使用 W").SetValue(false));
            Menu.SubMenu("Auto").AddItem(new MenuItem("Auto_wgapcloser", "使用 W 阻止突进").SetValue(true));
            Menu.SubMenu("Auto").AddItem(new MenuItem("Auto_e", "使用 E").SetValue(false));
            Menu.SubMenu("Auto").AddItem(new MenuItem("Auto_r", "当多少敌人在附近使用 R").SetValue(new Slider(2, 1, 5)));
            Menu.SubMenu("Auto").AddItem(new MenuItem("Auto_packet", "使用 封包").SetValue(true));

            Menu.AddSubMenu(new Menu("范 围", "Drawing"));
            Menu.SubMenu("Drawing").AddItem(new MenuItem("Drawing_q", "Q 范围").SetValue(new Circle(true, Color.FromArgb(100, 0, 255, 0))));
            Menu.SubMenu("Drawing").AddItem(new MenuItem("Drawing_w", "W 范围").SetValue(new Circle(true, Color.FromArgb(100, 0, 255, 0))));
            Menu.SubMenu("Drawing").AddItem(new MenuItem("Drawing_e", "E 范围").SetValue(new Circle(false, Color.FromArgb(100, 0, 255, 0))));
            Menu.SubMenu("Drawing").AddItem(new MenuItem("Drawing_damage", "显示 连招伤害").SetValue(true));
        }

        protected override void Combo()
        {
            if (Menu.Item("Combo_r").GetValue<bool>()) CastR();

            if (Menu.Item("Combo_w").GetValue<bool>() && Menu.Item("Combo_smart").GetValue<bool>())
            {
                if (Spells.get("W").IsReady())
                {
                    Obj_AI_Hero target = TargetSelector.GetTarget(Spells.get("Q").Range, TargetSelector.DamageType.Magical);
                    if (target.Distance(Player) < 200) Spells.CastOnTarget("E", TargetSelector.DamageType.Magical, Menu.Item("Combo_packet").GetValue<bool>());
                    else if (target.Distance(Player) < Spells.get("W").Range - 100) Spells.CastOnTarget("Q", TargetSelector.DamageType.Magical, Menu.Item("Combo_packet").GetValue<bool>());
                    else if (target.Distance(Player) > Spells.get("W").Range)
                    {
                        if (Damage.GetSpellDamage(Player, target, SpellSlot.Q) >= target.Health)
                            Spells.CastOnTarget("Q", TargetSelector.DamageType.Magical, Menu.Item("Combo_packet").GetValue<bool>());
                        else
                            return;
                    }
                }
            }

            if (Menu.Item("Combo_w").GetValue<bool>()) Spells.CastOnTarget("W", TargetSelector.DamageType.Magical, Menu.Item("Combo_packet").GetValue<bool>());
            if (Menu.Item("Combo_q").GetValue<bool>()) Spells.CastOnTarget("Q", TargetSelector.DamageType.Magical, Menu.Item("Combo_packet").GetValue<bool>());
            if (Menu.Item("Combo_e").GetValue<bool>()) Spells.CastOnTarget("E", TargetSelector.DamageType.Magical, Menu.Item("Combo_packet").GetValue<bool>());
        }
        protected override void Harass()
        {
            if (Menu.Item("Harass_w").GetValue<bool>()) Spells.CastOnTarget("W", TargetSelector.DamageType.Magical, Menu.Item("Harass_packet").GetValue<bool>());
            if (Menu.Item("Harass_q").GetValue<bool>()) Spells.CastOnTarget("Q", TargetSelector.DamageType.Magical, Menu.Item("Harass_packet").GetValue<bool>());
            if (Menu.Item("Harass_e").GetValue<bool>()) Spells.CastOnTarget("E", TargetSelector.DamageType.Magical, Menu.Item("Harass_packet").GetValue<bool>());
        }
        protected override void Auto()
        {
            if (Menu.Item("Auto_w").GetValue<bool>()) Spells.CastOnTarget("W", TargetSelector.DamageType.Magical, Menu.Item("Auto_packet").GetValue<bool>());
            if (Menu.Item("Auto_q").GetValue<bool>()) Spells.CastOnTarget("Q", TargetSelector.DamageType.Magical, Menu.Item("Auto_packet").GetValue<bool>());
            if (Menu.Item("Auto_e").GetValue<bool>()) Spells.CastOnTarget("E", TargetSelector.DamageType.Magical, Menu.Item("Auto_packet").GetValue<bool>());
        }

        protected override void Draw()
        {
            DrawCircle("Drawing_q", "Q");
            DrawCircle("Drawing_w", "W");
            DrawCircle("Drawing_e", "E");

            Utility.HpBarDamageIndicator.DamageToUnit = ComboDamage;
            Utility.HpBarDamageIndicator.Enabled = Menu.Item("Drawing_damage").GetValue<bool>();
        }
        protected override void Update()
        {
            Orbwalker.SetAttack(!(Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo && (Spells.get("Q").IsReady() || Spells.get("W").IsReady() || Spells.get("E").IsReady())));

            if (Menu.Item("Auto_r").GetValue<Slider>().Value <= Utility.CountEnemysInRange(Player, (int)Spells.get("R").Range))
                Spells.get("R").Cast();
        }

        private float ComboDamage(Obj_AI_Hero hero)
        {
            float damage = 0;
            //if (Spells.get("Q").IsReady())
            if(Spells.get("Q").Level > 0)
                damage += (float)Damage.GetSpellDamage(Player, hero, SpellSlot.Q) * 1.2f;
            //if (Spells.get("W").IsReady())
            if (Spells.get("W").Level > 0)
                damage += (float)Damage.GetSpellDamage(Player, hero, SpellSlot.W) * 1.2f;
            //if (Spells.get("E").IsReady())
            if (Spells.get("E").Level > 0)
                damage += (float)Damage.GetSpellDamage(Player, hero, SpellSlot.E) * 1.2f;
            return damage;
        }

        private void CastR()
        {
            if (!Spells.get("R").IsReady()) return;

            Obj_AI_Hero target = TargetSelector.GetTarget(Spells.get("R").Range, TargetSelector.DamageType.Magical);
            if (target == null) return;
            if (ComboDamage(target) < target.Health) return;

            Spells.get("R").Cast();
        }

        void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (Menu.Item("Auto_wgapcloser").GetValue<bool>())
            {
                if (Player.Distance(gapcloser.Sender) <= Spells.get("W").Range)
                    Spells.get("W").CastOnUnit(gapcloser.Sender);
            }
        }
    }
}
