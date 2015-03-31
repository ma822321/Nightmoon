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
using SharpDX;
using Color = System.Drawing.Color;


namespace BlackKassadin
{
    public static class Program
    {
        private const string ChampionName = "Kassadin";
        private static Obj_AI_Hero _player;
        private static readonly List<Spell> SpellList = new List<Spell>();
        private static Spell _nullSphere, _netherBlade, _forcePulse, _riftWalk;
        private static Menu _menu;
        private static Orbwalking.Orbwalker _orbwalker;
        private static ManaManager _manaManager;

        private static HitChance CustomHitChance
        {
            get { return GetHitchance(); }
        }

        #region Main

        public static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        #endregion

        #region OnGameLoad

        private static void Game_OnGameLoad(EventArgs args)
        {
            _player = ObjectManager.Player;
            if (_player.ChampionName != ChampionName)
            {
                return;
            }

            _nullSphere = new Spell(SpellSlot.Q, 650f);
            _netherBlade = new Spell(SpellSlot.W, 150f);
            _forcePulse = new Spell(SpellSlot.E, 400f);
            _riftWalk = new Spell(SpellSlot.R, 500f);

            SpellList.AddRange(new[] { _nullSphere, _forcePulse, _riftWalk });

            _nullSphere.SetTargetted(0.5f, 1400f);
            _forcePulse.SetSkillshot(0.5f, 10f, float.MaxValue, false, SkillshotType.SkillshotCone);
            _riftWalk.SetSkillshot(0.5f, 150f, float.MaxValue, false, SkillshotType.SkillshotCircle);

            _manaManager = new ManaManager();

            CreateMenu();

            Game.OnUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Interrupter2.OnInterruptableTarget += OnInterruptableSpell;

            ShowNotification("BlackKassadin by blacky - Loaded", System.Drawing.Color.Crimson, 4000);
            ShowNotification("ManaManager by iJabba", System.Drawing.Color.Crimson, 4000);
        }

        #endregion

        #region OnDraw

        private static void Drawing_OnDraw(EventArgs args)
        {
            foreach (var spell in SpellList)
            {
                var circleEntry = _menu.Item("drawRange" + spell.Slot).GetValue<Circle>();
                if (circleEntry.Active && !_player.IsDead)
                {
                    Render.Circle.DrawCircle(_player.Position, spell.Range, circleEntry.Color);
                }
            }
        }

        #endregion

        #region OnGameUpdate

        private static void Game_OnGameUpdate(EventArgs args)
        {
            var target = TargetSelector.GetTarget(_riftWalk.Range, TargetSelector.DamageType.Magical);

            Killsteal();

            if (_menu.Item("useCombo").GetValue<KeyBind>().Active)
            {
                OnCombo(target);
            }

            if (_menu.Item("useHarass").GetValue<KeyBind>().Active)
            {
                OnHarass(target);
            }

            if (_menu.Item("useWC").GetValue<KeyBind>().Active)
            {
                WaveClear();
            }

            if (_menu.Item("useJC").GetValue<KeyBind>().Active)
            {
                JungleClear();
            }

            if (_menu.Item("useFlee").GetValue<KeyBind>().Active)
            {
                Flee();
            }
        }

        #endregion

        #region Combo

        private static void OnCombo(Obj_AI_Hero target)
        {
            if (_menu.Item("useRiftWalk").GetValue<bool>() && _riftWalk.IsReady() &&
                _player.Distance(target) <= _riftWalk.Range && target.IsValidTarget(_riftWalk.Range))
            {
                _riftWalk.CastIfHitchanceEquals(target, CustomHitChance);
            }

            if (_menu.Item("useNetherBlade").GetValue<bool>() && _netherBlade.IsReady() &&
                _player.Distance(target) <= _netherBlade.Range + 25)
            {
                _netherBlade.Cast();
            }

            if (_menu.Item("useNullSphere").GetValue<bool>() && _nullSphere.IsReady() &&
                _player.Distance(target) <= _nullSphere.Range)
            {
                _nullSphere.Cast(target);
            }

            if (_menu.Item("useForcePulse").GetValue<bool>() && _forcePulse.IsReady() &&
                _player.Distance(target) <= _forcePulse.Range && target.IsValidTarget(_forcePulse.Range))
            {
                _forcePulse.CastIfHitchanceEquals(target, CustomHitChance);
            }
        }

        #endregion

        #region Harass

        private static void OnHarass(Obj_AI_Hero target)
        {
            if (!target.IsValidTarget(_nullSphere.Range) || !_manaManager.CanHarass())
            {
                return;
            }

            var pred = _forcePulse.GetPrediction(target);
            if (_menu.Item("useForcePulseHarass").GetValue<bool>() && _forcePulse.IsReady() &&
                target.IsValidTarget(_forcePulse.Range) && _player.Distance(target.Position) <= _forcePulse.Range &&
                pred.Hitchance >= CustomHitChance)
            {
                _forcePulse.Cast(pred.CastPosition);
            }

            if (_menu.Item("useNullSphereHarass").GetValue<bool>() && _nullSphere.IsReady() &&
                _player.Distance(target.Position) <= _nullSphere.Range)
            {
                _nullSphere.Cast(target);
            }
        }

        #endregion

        #region WaveClear

        private static void WaveClear()
        {
            List<Obj_AI_Base> allMinionsQ = MinionManager.GetMinions(
                _player.ServerPosition, _nullSphere.Range, MinionTypes.All, MinionTeam.NotAlly);
            List<Obj_AI_Base> allMinionsW = MinionManager.GetMinions(
                _player.ServerPosition, _netherBlade.Range, MinionTypes.All, MinionTeam.NotAlly);
            List<Obj_AI_Base> allMinionsE = MinionManager.GetMinions(
                _player.ServerPosition, _forcePulse.Range, MinionTypes.All, MinionTeam.NotAlly);
            List<Obj_AI_Base> allMinionsR = MinionManager.GetMinions(
                _player.ServerPosition, _riftWalk.Range, MinionTypes.All, MinionTeam.NotAlly);

            if (!allMinionsQ[0].IsValidTarget(_nullSphere.Range) || !allMinionsW[0].IsValidTarget(_netherBlade.Range) ||
                !allMinionsE[0].IsValidTarget(_forcePulse.Range) || !allMinionsR[0].IsValidTarget(_riftWalk.Range) ||
                !_manaManager.CanLaneclear())
            {
                return;
            }

            if (_menu.Item("useNullSphereWC").GetValue<bool>() && allMinionsQ.Count > 0 &&
                allMinionsQ[0].IsValidTarget(_nullSphere.Range) && _nullSphere.IsReady())
            {
                _nullSphere.Cast(allMinionsQ[0]);
            }

            if (_menu.Item("useNetherBladeWC").GetValue<bool>() && allMinionsW.Count > 0 &&
                allMinionsW[0].IsValidTarget(_netherBlade.Range) && _netherBlade.IsReady())
            {
                _netherBlade.Cast();
            }

            if (_menu.Item("useForcePulseWC").GetValue<bool>() && allMinionsE.Count > 2 &&
                allMinionsE[0].IsValidTarget(_forcePulse.Range) && _forcePulse.IsReady())
            {
                _forcePulse.Cast(allMinionsE[0]);
            }

            if (_menu.Item("useRiftWalkWC").GetValue<bool>() && allMinionsR.Count > 3 &&
                allMinionsR[0].IsValidTarget(_riftWalk.Range) && _riftWalk.IsReady())
            {
                _riftWalk.Cast(allMinionsR[0]);
            }
        }

        #endregion

        #region JungleClear

        private static void JungleClear()
        {
            List<Obj_AI_Base> allMinionsQ = MinionManager.GetMinions(
                _player.ServerPosition, _nullSphere.Range, MinionTypes.All, MinionTeam.Neutral);
            List<Obj_AI_Base> allMinionsW = MinionManager.GetMinions(
                _player.ServerPosition, _netherBlade.Range, MinionTypes.All, MinionTeam.Neutral);
            List<Obj_AI_Base> allMinionsE = MinionManager.GetMinions(
                _player.ServerPosition, _forcePulse.Range, MinionTypes.All, MinionTeam.Neutral);
            List<Obj_AI_Base> allMinionsR = MinionManager.GetMinions(
                _player.ServerPosition, _riftWalk.Range, MinionTypes.All, MinionTeam.Neutral);

            if (!allMinionsQ[0].IsValidTarget(_nullSphere.Range) || !allMinionsW[0].IsValidTarget(_netherBlade.Range) ||
                !allMinionsE[0].IsValidTarget(_forcePulse.Range) || !allMinionsR[0].IsValidTarget(_riftWalk.Range) ||
                !_manaManager.CanLaneclear())
            {
                return;
            }

            if (_menu.Item("useNullSphereJC").GetValue<bool>() && allMinionsQ.Count > 0 &&
                allMinionsQ[0].IsValidTarget(_nullSphere.Range) && _nullSphere.IsReady())
            {
                _nullSphere.Cast(allMinionsQ[0]);
            }

            if (_menu.Item("useNetherBladeJC").GetValue<bool>() && allMinionsW.Count > 0 &&
                allMinionsW[0].IsValidTarget(_netherBlade.Range) && _netherBlade.IsReady())
            {
                _netherBlade.Cast();
            }

            if (_menu.Item("useForcePulseJC").GetValue<bool>() && allMinionsE.Count > 1 &&
                allMinionsE[0].IsValidTarget(_forcePulse.Range) && _forcePulse.IsReady())
            {
                _forcePulse.Cast(allMinionsE[0]);
            }

            if (_menu.Item("useRiftWalkJC").GetValue<bool>() && allMinionsR.Count > 2 &&
                allMinionsR[0].IsValidTarget(_riftWalk.Range) && _riftWalk.IsReady())
            {
                _riftWalk.Cast(allMinionsR[0]);
            }
        }

        #endregion

        #region KillSteal

        private static void Killsteal()
        {
            foreach (Obj_AI_Hero target in
                ObjectManager.Get<Obj_AI_Hero>()
                    .Where(hero => hero.IsValidTarget(_riftWalk.Range) && !hero.HasBuffOfType(BuffType.Invulnerability))
                )
            {
                var forcePulseDmg = _player.GetSpellDamage(target, SpellSlot.E);
                var nullSphereDmg = _player.GetSpellDamage(target, SpellSlot.Q);
                var riftWalkDmg = _player.GetSpellDamage(target, SpellSlot.R);
                if (_menu.Item("killstealUseForcePulse").GetValue<bool>() && target.Health <= forcePulseDmg)
                {
                    _forcePulse.CastIfHitchanceEquals(target, CustomHitChance);
                }

                if (_menu.Item("killstealUseNullSphere").GetValue<bool>() && target.Health <= nullSphereDmg)
                {
                    _nullSphere.Cast(target);
                }

                if (_menu.Item("killstealUseRiftWalk").GetValue<bool>() && target.Health <= riftWalkDmg)
                {
                    _riftWalk.CastIfHitchanceEquals(target, CustomHitChance);
                }
            }
        }

        #endregion

        #region Flee

        private static void Flee()
        {
            var rOnPlayer = RiftWalkCount();
            var keepStacks = _menu.Item("miscRiftWalkStacks").GetValue<Slider>().Value;
            if (_riftWalk.IsReady() && rOnPlayer < keepStacks)
            {
                Orbwalking.Orbwalk(null, Game.CursorPos);
                _riftWalk.Cast(Game.CursorPos);
            }
            else
            {
                Orbwalking.Orbwalk(null, Game.CursorPos);
            }
        }

        #endregion

        #region Interrupt

        private static void OnInterruptableSpell(Obj_AI_Hero unit, Interrupter2.InterruptableTargetEventArgs spell)
        {
            if (_menu.Item("inter").GetValue<bool>() && _nullSphere.IsReady() &&
                _player.Distance(unit.Position) <= _nullSphere.Range)
            {
                _nullSphere.Cast(unit);
            }
        } 
        #endregion

        #region CreateMenu

        private static void CreateMenu()
        {
            _menu = new Menu("花边-Black卡萨丁", "black" + ChampionName, true);

            var targetSelectorMenu = new Menu("目标 选择", "ts");
            _menu.AddSubMenu(targetSelectorMenu);
            TargetSelector.AddToMenu(targetSelectorMenu);

            var orbwalkingMenu = new Menu("走 砍", "orbwalk");
            _menu.AddSubMenu(orbwalkingMenu);
            _orbwalker = new Orbwalking.Orbwalker(orbwalkingMenu);

            var keybindings = new Menu("按键", "keybindings");
            {
                keybindings.AddItem(new MenuItem("useCombo", "连招").SetValue(new KeyBind(32, KeyBindType.Press)));
                keybindings.AddItem(new MenuItem("useHarass", "骚扰").SetValue(new KeyBind('C', KeyBindType.Press)));
                keybindings.AddItem(new MenuItem("useWC", "清线").SetValue(new KeyBind('V', KeyBindType.Press)));
                keybindings.AddItem(new MenuItem("useJC", "清野").SetValue(new KeyBind('V', KeyBindType.Press)));
                keybindings.AddItem(new MenuItem("useFlee", "逃跑").SetValue(new KeyBind('G', KeyBindType.Press)));
                _menu.AddSubMenu(keybindings);
            }

            var combo = new Menu("连招", "combo");
            {
                combo.AddItem(new MenuItem("useNullSphere", "使用 Q").SetValue(true));
                combo.AddItem(new MenuItem("useNetherBlade", "使用 W").SetValue(true));
                combo.AddItem(new MenuItem("useForcePulse", "使用 E").SetValue(true));
                combo.AddItem(new MenuItem("useRiftWalk", "使用 R").SetValue(true));
                _menu.AddSubMenu(combo);
            }

            var harass = new Menu("骚扰", "harass");
            {
                harass.AddItem(new MenuItem("useNullSphereHarass", "使用 Q").SetValue(true));
                harass.AddItem(new MenuItem("useForcePulseHarass", "使用 E").SetValue(false));
                _menu.AddSubMenu(harass);
            }

            var waveclear = new Menu("清线", "waveclear");
            {
                waveclear.AddItem(new MenuItem("useNullSphereWC", "使用 Q").SetValue(true));
                waveclear.AddItem(new MenuItem("useNetherBladeWC", "使用 W").SetValue(true));
                waveclear.AddItem(new MenuItem("useForcePulseWC", "使用 E").SetValue(true));
                waveclear.AddItem(new MenuItem("useRiftWalkWC", "使用 R").SetValue(false));
                _menu.AddSubMenu(waveclear);
            }

            var jungleclear = new Menu("清野", "jungleclear");
            {
                jungleclear.AddItem(new MenuItem("useNullSphereJC", "使用 Q").SetValue(true));
                jungleclear.AddItem(new MenuItem("useNetherBladeJC", "使用 W").SetValue(true));
                jungleclear.AddItem(new MenuItem("useForcePulseJC", "使用 E").SetValue(true));
                jungleclear.AddItem(new MenuItem("useRiftWalkJC", "使用 R").SetValue(false));
                _menu.AddSubMenu(jungleclear);
            }

            var killsteal = new Menu("击杀", "killsteal");
            {
                killsteal.AddItem(new MenuItem("killstealUseNullSphere", "使用 Q").SetValue(true));
                killsteal.AddItem(new MenuItem("killstealUseForcePulse", "使用 E").SetValue(true));
                killsteal.AddItem(new MenuItem("killstealUseRiftWalk", "使用 R").SetValue(false));
                _menu.AddSubMenu(killsteal);
            }

            _manaManager.AddToMenu(ref _menu);

            var misc = new Menu("杂项", "misc");
            {
                misc.AddItem(new MenuItem("inter", "使用 Q 打断技能").SetValue(true));
                misc.AddItem(new MenuItem("miscRiftWalkStacks", "R堆叠输不超过"))
                    .SetValue(new Slider(3, 1, 4));
                misc.AddItem(new MenuItem("hitChanceSetting", "命中率").SetValue(new StringList(new[] { "低", "中", "高", "非常高" }, 3)));
                _menu.AddSubMenu(misc);
            }

            var drawings = new Menu("显示", "drawings");
            {
                drawings.AddItem(new MenuItem("drawRangeQ", "Q 范围").SetValue(new Circle(true, Color.Aquamarine)));
                drawings.AddItem(new MenuItem("drawRangeE", "E 范围").SetValue(new Circle(false, Color.Aquamarine)));
                drawings.AddItem(new MenuItem("drawRangeR", "R 范围").SetValue(new Circle(false, Color.Aquamarine)));
                _menu.AddSubMenu(drawings);
            }

            var messageMenu = new Menu("信 息", "message");
            {
                messageMenu.AddItem(new MenuItem("Sprite", "Black-卡萨丁"));
                messageMenu.AddItem(new MenuItem("Hanhua", "汉化:花边下丶情未央"));
                messageMenu.AddItem(new MenuItem("qqqun", "QQ群:299606556"));
                _menu.AddSubMenu(messageMenu);
            }

            Game.PrintChat("<font color=\"#1eff00\">Huabian婕㈠寲QQ缇わ細299606556</font> - <font color=\"#00BFFF\">姝¤繋鍚勪綅鐨勫姞鍏ワ紒</font>");

            _menu.AddToMainMenu();
        }

        #endregion

        #region Notifications Credits to Beaving.
        public static Notification ShowNotification(string message, System.Drawing.Color color, int duration = -1, bool dispose = true)
        {
            var notif = new Notification(message).SetTextColor(color);
            Notifications.AddNotification(notif);
            if (dispose)
            {
                Utility.DelayAction.Add(duration, () => notif.Dispose());
            }
            return notif;
        }
        #endregion

        #region GetHitChance

        private static HitChance GetHitchance()
        {
            switch (_menu.Item("hitChanceSetting").GetValue<StringList>().SelectedIndex)
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
                    return HitChance.Medium;
            }
        }

        #endregion

        #region RiftWalkCount

        private static int RiftWalkCount()
        {
            var buff = ObjectManager.Player.Buffs.FirstOrDefault(buff1 => buff1.Name.Equals("RiftWalk"));
            return buff != null ? buff.Count : 0;
        }

        #endregion
    }
}
