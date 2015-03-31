using System;
using System.Drawing;
using System.Drawing.Printing;
using LeagueSharp;
using LeagueSharp.Common;
using _Damage = LeagueSharp.Common.Damage;
using SharpDX;
using Color = System.Drawing.Color;

namespace DMGinc
{
    class Program
    {

        #region some var's
        public static Obj_AI_Hero Player = ObjectManager.Player;
        private static Obj_AI_Hero _target;
        private static Menu _menu;
        public static Spell Q;
        public static Spell W;
        public static Spell E;
        public static Spell R;
        private static MenuItem drawFill;
        private static MenuItem drawLine;
        public delegate float DamageToUnitDelegate(Obj_AI_Hero hero);
        public static Color Color = Color.Lime;
        public static Color FillColor = Color.Goldenrod;
        public static bool Fill = true;
        public static bool Enabled = true;
        private static DamageToUnitDelegate _damageToUnit;
        private static readonly Render.Text Text = new Render.Text(
            0, 0, "", 11, new ColorBGRA(255, 0, 0, 255), "monospace");

        private static float _percentpenetrationarmor;
        private static float _percentpenetrationmagic;
        #endregion

        static void Main(string[] args)
        {

            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;

        }

        static void Game_OnGameLoad(EventArgs args)
        {

            #region sayings

            //Game.PrintChat("<font color=\"#AF7AFF\"><b>Damage Indicator</b></font> for " + Player.ChampionName + " - Loaded");
            Notifications.AddNotification("Damage Indicator for " + Player.ChampionName, 10, true);

            #endregion

            #region Menushit
            _menu = new Menu("花边-伤害显示", "damage.Indicator", true);
            var Drawings = new Menu("显示", "drawings");
            _menu.AddSubMenu(Drawings);
            drawFill = new MenuItem("Draw_Fill", "显示 连招伤害 (满)", true).SetValue(new Circle(true, Color.FromArgb(90, 255, 169, 4)));
            drawLine = new MenuItem("Draw_Line", "显示 连招伤害 (线条)", true).SetValue(new Circle(true, Color.FromArgb(90, 255, 169, 4)));
            Drawings.AddItem(drawFill);
            Drawings.AddItem(drawLine);
            Game.PrintChat("<font color=\"#1eff00\">Huabian婕㈠寲QQ缇わ細299606556</font> - <font color=\"#00BFFF\">姝¤繋鍚勪綅鐨勫姞鍏ワ紒</font>");
            _menu.AddToMainMenu();
            #endregion

            #region Spells
            Q = new Spell(SpellSlot.Q);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E);
            R = new Spell(SpellSlot.R);
            #endregion

            Drawing.OnDraw += Drawing_OnDraw;

        }

        private static void Drawing_OnDraw(EventArgs args)
        {

            if (Player.IsDead != true)
            {
                _percentpenetrationarmor = ((1 - Player.PercentArmorPenetrationMod) + 1);
                _percentpenetrationmagic = ((1 - Player.PercentMagicPenetrationMod) + 1);
                DamageIndicator.DamageToUnit = GetComboDamage;
                DamageIndicator.Enabled = true;
                DamageIndicator.Fill = drawFill.GetValue<Circle>().Active;
                DamageIndicator.FillColor = drawFill.GetValue<Circle>().Color;
                DamageIndicator.Color = drawLine.GetValue<Circle>().Color;
                DamageIndicator.ColorFill = drawLine.GetValue<Circle>().Active;
            }
            
        }

        private static float GetComboDamage(Obj_AI_Base enemy)
        {
            // do nothing is there no enemy
            if (enemy == null)
                return 0;

            // reset values
            double damage = 0d;

            // Check Q Spell and Calc
            #region Q Spell Calc
            if (Q.IsReady())
            {
                if (Q.DamageType == TargetSelector.DamageType.Magical)
                {
                    damage +=
                        Player.CalcDamage(enemy, _Damage.DamageType.Magical, Player.GetSpellDamage(enemy, SpellSlot.Q)) * _percentpenetrationmagic;
                }
                else if (Q.DamageType == TargetSelector.DamageType.Physical)
                {
                    damage += 
                        Player.CalcDamage(enemy, _Damage.DamageType.Physical, Player.GetSpellDamage(enemy, SpellSlot.Q)) * _percentpenetrationarmor;
                }
                else if (Q.DamageType == TargetSelector.DamageType.True)
                {
                    damage += 
                        Player.CalcDamage(enemy, _Damage.DamageType.True, Player.GetSpellDamage(enemy, SpellSlot.Q));
                }
            }
            #endregion

            // Check W Spell and Calc
            #region W Spell Calc
            if (W.IsReady())
            {
                if (W.DamageType == TargetSelector.DamageType.Magical)
                {
                    damage += 
                        Player.CalcDamage(enemy, _Damage.DamageType.Magical, Player.GetSpellDamage(enemy, SpellSlot.W)) * _percentpenetrationmagic;
                }
                else if (W.DamageType == TargetSelector.DamageType.Physical)
                {
                    damage += 
                        Player.CalcDamage(enemy, _Damage.DamageType.Physical, Player.GetSpellDamage(enemy, SpellSlot.W)) * _percentpenetrationarmor;
                }
                else if (W.DamageType == TargetSelector.DamageType.True)
                {
                    damage += Player.CalcDamage(enemy, _Damage.DamageType.True, Player.GetSpellDamage(enemy, SpellSlot.W));
                }
            }
            #endregion

            // Check E Spell and Calc
            #region E Spell Calc
            if (E.IsReady())
            {
                if (E.DamageType == TargetSelector.DamageType.Magical)
                {
                    damage += Player.CalcDamage(enemy, _Damage.DamageType.Magical, Player.GetSpellDamage(enemy, SpellSlot.E)) * _percentpenetrationmagic;
                }
                else if (E.DamageType == TargetSelector.DamageType.Physical)
                {
                    damage += Player.CalcDamage(enemy, _Damage.DamageType.Physical, Player.GetSpellDamage(enemy, SpellSlot.E)) * _percentpenetrationarmor;
                }
                else if (E.DamageType == TargetSelector.DamageType.True)
                {
                    damage += Player.CalcDamage(enemy, _Damage.DamageType.True, Player.GetSpellDamage(enemy, SpellSlot.E));
                }
            }
            #endregion

            // Check R Spell and Calc
            #region R Spell Calc
            if (R.IsReady())
            {
                if (R.DamageType == TargetSelector.DamageType.Magical)
                {
                    damage += Player.CalcDamage(enemy, _Damage.DamageType.Magical, Player.GetSpellDamage(enemy, SpellSlot.R)) * _percentpenetrationmagic;
                }
                else if (R.DamageType == TargetSelector.DamageType.Physical)
                {
                    damage += Player.CalcDamage(enemy, _Damage.DamageType.Physical, Player.GetSpellDamage(enemy, SpellSlot.R)) * _percentpenetrationarmor;
                }
                else if (R.DamageType == TargetSelector.DamageType.True)
                {
                    damage += Player.CalcDamage(enemy, _Damage.DamageType.True, Player.GetSpellDamage(enemy, SpellSlot.R));
                }
            }
            #endregion

            //damage += Player.CalcDamage(enemy, _Damage.DamageType.Physical, Player.GetAutoAttackDamage(enemy)); // no need for Autoattack Damage 

            damage = ActiveItems.CalcDamage(enemy, damage); // active Items thanks xSlaice :)

            return (float)damage;

        }
     }
}
