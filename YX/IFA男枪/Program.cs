// This file is part of LeagueSharp.Common.
// 
// LeagueSharp.Common is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// LeagueSharp.Common is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with LeagueSharp.Common.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

namespace iFuckingAwesomeGraves
{
    internal class Program
    {
        private static Orbwalking.Orbwalker _orbwalker;
        private static Menu _menu;
        private static Obj_AI_Hero _player;
        private static Spell _r2;
        // ReSharper disable once InconsistentNaming
        private static readonly Dictionary<SpellSlot, Spell> _spells = new Dictionary<SpellSlot, Spell>
        {
            { SpellSlot.Q, new Spell(SpellSlot.Q, 800f) },
            { SpellSlot.W, new Spell(SpellSlot.W, 950f) },
            { SpellSlot.E, new Spell(SpellSlot.E, 425f) },
            { SpellSlot.R, new Spell(SpellSlot.R, 1100f) }
        };

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        #region Events

        private static void Game_OnGameLoad(EventArgs args)
        {
            _player = ObjectManager.Player;
            Notifications.AddNotification(new Notification("THIS IS NOT DONE", 2));

            LoadSpells();
            CreateMenu();

            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += OnDraw;
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            if (_player.IsDead)
            {
                return;
            }
            switch (_orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    DoCombo();
                    break;
            }
        }

        private static void OnDraw(EventArgs args)
        {
            //TODO drawing
        }

        #endregion

        #region ActiveModes

        private static void DoCombo()
        {
            CastBuckshot();
            CastSmokeScreen();
            CastQuickdraw();
            CastCollateralDamage();
            CollateralDamageKs();
        }

        #endregion

        #region spell casting

        private static void CastBuckshot()
        {
            var qTarget = TargetSelector.GetTarget(_spells[SpellSlot.Q].Range, TargetSelector.DamageType.Physical);
            if (_menu.Item("useQC").GetValue<bool>() && qTarget.IsValidTarget(_spells[SpellSlot.Q].Range))
            {
                if (_spells[SpellSlot.Q].IsReady() && _spells[SpellSlot.Q].IsInRange(qTarget))
                {
                    _spells[SpellSlot.Q].CastIfHitchanceEquals(qTarget, GetCustomHitChance()); // TODO custom hitchance.
                }
            }
        }

        private static void CastSmokeScreen()
        {
            var wTarget = TargetSelector.GetTarget(_spells[SpellSlot.W].Range, TargetSelector.DamageType.Physical);
            if (_menu.Item("useWC").GetValue<bool>() && wTarget.IsValidTarget(_spells[SpellSlot.W].Range))
            {
                if (_spells[SpellSlot.W].IsReady() && _spells[SpellSlot.W].IsInRange(wTarget))
                {
                    _spells[SpellSlot.W].CastIfHitchanceEquals(wTarget, GetCustomHitChance()); // TODO custom hitchance
                }
            }
        }

        private static void CastQuickdraw()
        {
            var positionAfterE = _player.Position.Extend(Game.CursorPos, _spells[SpellSlot.E].Range);
            if (_menu.Item("useEC").GetValue<bool>() && _spells[SpellSlot.E].IsReady())
            {
                if (_menu.Item("eCheck").GetValue<bool>() && positionAfterE.UnderTurret(true))
                {
                    return;
                }
                _spells[SpellSlot.E].Cast(positionAfterE);
            }
        }

        /// <summary>
        ///     Matey, If target is killable and higher then inital R Range, then take into account the extra 800 - cone range if
        ///     the ult collides with a minion / champion / yasuo wall.
        /// </summary>
        private static void CastCollateralDamage()
        {
            var rTarget = TargetSelector.GetTarget(_spells[SpellSlot.R].Range, TargetSelector.DamageType.Physical);
            if (_menu.Item("useRC").GetValue<bool>() && rTarget.IsValidTarget(_spells[SpellSlot.R].Range))
            {
                if (_spells[SpellSlot.R].IsReady() && _spells[SpellSlot.R].IsInRange(rTarget))
                {
                    if (_spells[SpellSlot.R].GetDamage(rTarget) > rTarget.Health + 10)
                    {
                        _spells[SpellSlot.R].CastIfHitchanceEquals(rTarget, GetCustomHitChance());
                            // TODO custom hitchance
                    }
                    else
                    {
                        foreach (Obj_AI_Hero source in
                            from source in
                                HeroManager.Enemies.Where(hero => hero.IsValidTarget(_spells[SpellSlot.R].Range))
                            let prediction = _spells[SpellSlot.R].GetPrediction(source, true)
                            where
                                _player.Distance(source) <= _spells[SpellSlot.R].Range &&
                                prediction.AoeTargetsHitCount >= 3
                            select source)
                        {
                            _spells[SpellSlot.R].CastIfHitchanceEquals(source, GetCustomHitChance());
                        }
                    }
                }
            }
        }

        public static void CollateralDamageKs()
        {
            foreach (var target in _player.Position.GetEnemiesInRange(1900).Where(e => e.IsValidTarget()))
            {
                if (target.Distance(_player) < _spells[SpellSlot.R].Range &&
                    _spells[SpellSlot.R].GetDamage(target) > target.Health)
                {
                    _spells[SpellSlot.R].Cast(target);
                    return;
                }
                if (R2Damage(target) < target.Health)
                {
                    return;
                }
                var pred = _r2.GetPrediction(target);
                if (
                    pred.CollisionObjects.Count(
                        a => a.IsEnemy && a.IsValid<Obj_AI_Hero>() && _player.Distance(a) < 1100) > 0)
                {
                    _r2.Cast(pred.CastPosition);
                }
                else
                {
                    foreach (var target2 in _player.Position.GetEnemiesInRange(1100).Where(e => e.IsValidTarget()))
                    {
                        var sector =
                            new Geometry.Sector(
                                _spells[SpellSlot.R].GetPrediction(target2).UnitPosition.To2D(),
                                _player.Position.To2D()
                                    .Extend(
                                        _spells[SpellSlot.R].GetPrediction(target2).UnitPosition.To2D(),
                                        _player.Distance(_spells[SpellSlot.R].GetPrediction(target2).UnitPosition) + 100),
                                60 * (float) Math.PI / 180, 800).ToPolygon();
                        if (!sector.IsOutside(target2.Position.To2D()))
                        {
                            _r2.Cast(_r2.GetPrediction(target2).CastPosition);
                        }
                    }
                }
            }
        }

        #endregion

        #region menu and spells

        private static void LoadSpells()
        {
            _spells[SpellSlot.Q].SetSkillshot(
                0.26f, 10f * 2 * (float) Math.PI / 180, 1950f, false, SkillshotType.SkillshotCone);
            _spells[SpellSlot.W].SetSkillshot(0.30f, 250f, 1650f, false, SkillshotType.SkillshotCircle);
            _spells[SpellSlot.R].SetSkillshot(0.22f, 150f, 2100f, true, SkillshotType.SkillshotLine);
            _r2 = new Spell(SpellSlot.R, 1900);
            _r2.SetSkillshot(0.22f, 150f, 2100f, true, SkillshotType.SkillshotLine);
        }

        private static void CreateMenu()
        {
            _menu = new Menu("花边汉化-IFA男枪", "ifag", true);
            //LOL iJabba = iFag = iFuckingAwesomeGraves, your a CUNT L0L

            TargetSelector.AddToMenu(_menu.AddSubMenu(new Menu("目标 选择", "Target Selector")));

            _orbwalker = new Orbwalking.Orbwalker(_menu.AddSubMenu(new Menu("走砍 设置", "Orbwalker")));

            var comboMenu = new Menu("连招 设置", "com.ifag.combo");
            {
                comboMenu.AddItem(new MenuItem("useQC", "使用 Q").SetValue(true));
                comboMenu.AddItem(new MenuItem("useWC", "使用 W").SetValue(true));
                comboMenu.AddItem(new MenuItem("useEC", "使用 E").SetValue(true));
                comboMenu.AddItem(new MenuItem("useRC", "使用 R").SetValue(true));
                _menu.AddSubMenu(comboMenu);
            }

            var harassMenu = new Menu("骚扰 设置", "com.ifag.harass");
            {
                harassMenu.AddItem(new MenuItem("useQH", "使用 Q").SetValue(true));
                harassMenu.AddItem(new MenuItem("useWH", "使用 W").SetValue(true));
                _menu.AddSubMenu(harassMenu);
                //TODO harass mana
            }

            var farmMenu = new Menu("打钱 设置", "com.ifag.farm");
            {
                var laneclearMenu = new Menu("清线 设置", "com.ifag.farm.lc");
                {
                    laneclearMenu.AddItem(new MenuItem("useQLC", "使用 Q").SetValue(false));
                }
                var lasthitMenu = new Menu("补刀 设置", "com.ifag.farm.lh");
                {
                    lasthitMenu.AddItem(new MenuItem("useQLH", "使用 Q").SetValue(false));
                }
                farmMenu.AddSubMenu(laneclearMenu);
                farmMenu.AddSubMenu(lasthitMenu);
                _menu.AddSubMenu(farmMenu);
            }

            var miscMenu = new Menu("杂项 设置", "com.ifag.misc");
            {
                miscMenu.AddItem(new MenuItem("eCheck", "安全时E回塔下"));
                miscMenu.AddItem(
                    new MenuItem("hitchance", "命中率").SetValue(
                        new StringList(new[] { "低", "中", "高", "非常高" }, 2)));
                _menu.AddSubMenu(miscMenu);
            }


            _menu.AddToMainMenu();
        }

        private static double R2Damage(Obj_AI_Hero target)
        {
            if (_spells[SpellSlot.R].Level == 0)
            {
                return 0;
            }
            return _player.CalcDamage(
                target, Damage.DamageType.Physical,
                new double[] { 200, 320, 440 }[_spells[SpellSlot.R].Level - 1] + 1.2 * _player.FlatPhysicalDamageMod);
        }

        private static HitChance GetCustomHitChance()
        {
            switch (_menu.Item("hitchance").GetValue<StringList>().SelectedIndex)
            {
                case 0:
                    return HitChance.Low;
                case 1:
                    return HitChance.Medium;
                case 2:
                    return HitChance.High;
                case 3:
                    return HitChance.VeryHigh;
                default:
                    return HitChance.High;
            }
        }

        #endregion
    }
}