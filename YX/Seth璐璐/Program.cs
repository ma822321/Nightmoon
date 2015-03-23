using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LeagueSharp.Common;
using LeagueSharp;
using LeagueSharp.Common.Data;

namespace SethLulu
{
    class Program
    {
        public const string ChampionName = "Lulu";

        public static Menu _root;
        public static Spell _q, _w, _e, _r;
        public static Orbwalking.Orbwalker Orbwalker;

        public static List<Spell> _spells = new List<Spell>();

        private static SpellSlot _igniteSlot;
        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }
        private static void Game_OnGameLoad(EventArgs args)
        {
            if (ObjectManager.Player.BaseSkinName != ChampionName) return;

            _q = new Spell(SpellSlot.Q, 925f);
            _q.SetSkillshot(0.25f, 60, 1450, false, SkillshotType.SkillshotLine);

            _w = new Spell(SpellSlot.W, 650f);
            _e = new Spell(SpellSlot.E, 650f);
            _r = new Spell(SpellSlot.R, 900f);

            _igniteSlot = ObjectManager.Player.GetSpellSlot("SummonerDot");

            _spells.Add(_q);
            _spells.Add(_w);
            _spells.Add(_e);
            _spells.Add(_r);

            _root = new Menu("花边汉化-Seth璐璐", "Lulu");
            _root.AddSubMenu(new Menu("走砍 设置", "Orbwalking"));

            Orbwalker = new Orbwalking.Orbwalker(_root.SubMenu("Orbwalking"));

            var targetSelectorMenu = new Menu("目标 选择", "Target Selector");
            TargetSelector.AddToMenu(targetSelectorMenu);
            _root.AddSubMenu(targetSelectorMenu);

            _root.AddSubMenu(new Menu("法术 设置", "SpellsM"));
            _root.SubMenu("SpellsM").AddItem(new MenuItem("Combo", "连 招"));
            _root.SubMenu("Combo").AddItem(new MenuItem("UseQCombo", "使用 Q").SetValue(true));
            _root.SubMenu("Combo").AddItem(new MenuItem("UseWCombo", "使用 W").SetValue(true));
            _root.SubMenu("Combo").AddItem(new MenuItem("UseECombo", "使用 E").SetValue(true));
            _root.SubMenu("Combo").AddItem(new MenuItem("UseIgniteCombo", "使用 点燃").SetValue(true));
            _root.SubMenu("Combo")
                .AddItem(
                    new MenuItem("ComboActive", "连招 按键!").SetValue(
                        new KeyBind(_root.Item("Orbwalk").GetValue<KeyBind>().Key, KeyBindType.Press)));
            _root.SubMenu("SpellsM").AddItem(new MenuItem("Harass", "骚 扰"));
            _root.SubMenu("Harass").AddItem(new MenuItem("UseQHarass", "使用 Q").SetValue(true));
            _root.SubMenu("Harass").AddItem(new MenuItem("UseWHarass", "使用 W").SetValue(true));
            _root.SubMenu("Harass").AddItem(new MenuItem("UseEHarass", "使用 E").SetValue(true));
            _root.SubMenu("Harass")
                .AddItem(
                    new MenuItem("HarassActive", "骚扰 (自动)!").SetValue(
                        new KeyBind("Y".ToCharArray()[0], KeyBindType.Toggle)));
            _root.SubMenu("SpellsM").AddItem(new MenuItem("Farm", "打 钱"));
            _root.SubMenu("Farm").AddItem(new MenuItem("UseQFarm", "使用 Q").SetValue(true));
            _root.SubMenu("Farm").AddItem(new MenuItem("UseEFarm", "使用 E").SetValue(true));
            _root.SubMenu("Farm")
                .AddItem(
                new MenuItem("LaneClearActive", "打钱 按键!").SetValue(
                    new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));
            _root.SubMenu("SpellsM").AddItem(new MenuItem("Killsteal", "击 杀"));
            _root.SubMenu("Killsteal").AddItem(new MenuItem("StealActive", "开 启").SetValue(true));
            if (_root.Item("StealActive").GetValue<bool>() == true)
            {
                _root.SubMenu("Killsteal").AddItem(new MenuItem("UseQSteal", "使用 Q").SetValue(true));
                _root.SubMenu("Killsteal").AddItem(new MenuItem("UseESteal", "使用 E").SetValue(true));
            }

            _root.AddSubMenu(new Menu("防御 法术", "AllyM"));
            _root.SubMenu("AllyM").AddItem(new MenuItem("DefActive", "开 启").SetValue(true));
            if (_root.Item("DefActive").GetValue<bool>())
            {
                _root.SubMenu("AllyM").AddItem(new MenuItem("ESpell", "E 设置"));
                _root.SubMenu("ESpell").AddItem(new MenuItem("AutoE", "使用E丨自己HP<%").SetValue(new Slider(50, 100, 0)));
                _root.SubMenu("ESpell").AddItem(new MenuItem("AutoEAlly", "使用E丨友军HP<%").SetValue(new Slider(50, 100, 0)));
                _root.SubMenu("AllyM").AddItem(new MenuItem("RSpell", "R 设置"));
                _root.SubMenu("RSpell").AddItem(new MenuItem("AutoR", "使用R丨自己HP<%").SetValue(new Slider(50, 100, 0)));
                _root.SubMenu("RSpell").AddItem(new MenuItem("AutoRAlly", "使用R丨友军HP<%").SetValue(new Slider(50, 100, 0)));
            }

            _root.AddSubMenu(new Menu("范围 设置", "SharpDrawer"));
            _root.SubMenu("SharpDrawer").AddItem(new MenuItem("DrawQ", "显示 Q 范围").SetValue(new Circle(true, System.Drawing.Color.FromArgb(100, 58, 90, 179))));
            _root.SubMenu("SharpDrawer").AddItem(new MenuItem("DrawW", "显示 W 范围").SetValue(new Circle(true, System.Drawing.Color.FromArgb(100, 58, 90, 179))));
            _root.SubMenu("SharpDrawer").AddItem(new MenuItem("DrawE", "显示 E 范围").SetValue(new Circle(true, System.Drawing.Color.FromArgb(100, 58, 90, 179))));
            _root.SubMenu("SharpDrawer").AddItem(new MenuItem("DrawR", "显示 R 范围").SetValue(new Circle(true, System.Drawing.Color.FromArgb(100, 58, 90, 179))));

            _root.AddSubMenu(new Menu("杂项 管理", "Misc"));
            _root.SubMenu("Misc").AddItem(new MenuItem("InterruptSpells", "打断 法术").SetValue(false));
            _root.SubMenu("InterruptSpells").AddItem(new MenuItem("RInterrupt", "使用 R").SetValue(false));
            _root.SubMenu("InterruptSpells").AddItem(new MenuItem("WInterrupt", "使用 W").SetValue(false));
            _root.SubMenu("Misc").AddItem(new MenuItem("WEGapcloser", "先W后E丨防突进").SetValue(true));

            _root.AddToMainMenu();
            Game.PrintChat("<font color='#0066FF'>鑺辫竟姹夊寲-Seth鐠愮拹 鍔犺浇鎴愬姛!</font>");

            var dmgAfterComboItem = new MenuItem("DamageAfterCombo", "显示 连招 伤害").SetValue(true);
            _root.SubMenu("SharpDrawer").AddItem(dmgAfterComboItem);
            Utility.HpBarDamageIndicator.DamageToUnit = GetComboDamage;
            Utility.HpBarDamageIndicator.Enabled = dmgAfterComboItem.GetValue<bool>();
            dmgAfterComboItem.ValueChanged += delegate (object sender, OnValueChangeEventArgs eventArgs)
            {
                Utility.HpBarDamageIndicator.Enabled = eventArgs.GetNewValue<bool>();
            };
            Game.OnGameUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Interrupter2.OnInterruptableTarget += Interrupter2_OnInterruptableTarget;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloserOnOnEnemyGapcloser;
        }

        private static float GetComboDamage(Obj_AI_Base t)
        {
            var fComboDamage = 0d;
            if (_q.IsReady())
                fComboDamage += ObjectManager.Player.GetSpellDamage(t, SpellSlot.Q);
            if (_r.IsReady())
                fComboDamage += ObjectManager.Player.GetSpellDamage(t, SpellSlot.R);
            if (_w.IsReady())
                fComboDamage += ObjectManager.Player.GetSpellDamage(t, SpellSlot.W);
            if (_e.IsReady())
                fComboDamage += ObjectManager.Player.GetSpellDamage(t, SpellSlot.E);
            if (_igniteSlot != SpellSlot.Unknown &&
            ObjectManager.Player.Spellbook.CanUseSpell(_igniteSlot) == SpellState.Ready)
                fComboDamage += ObjectManager.Player.GetSummonerSpellDamage(t, Damage.SummonerSpell.Ignite);

            return (float)fComboDamage;
        }

        private static void AntiGapcloserOnOnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (_root.Item("WEGapcloser").GetValue<bool>())
            {
                if (ObjectManager.Player.Distance(gapcloser.Sender) < 650f)
                {
                    if (_w.IsReady() && _e.IsReady())
                    {
                        _w.CastOnUnit(gapcloser.Sender);
                        _e.CastOnUnit(ObjectManager.Player);
                    }
                }

                else
                {
                    if (_e.IsReady() && ObjectManager.Player.Distance(gapcloser.Sender) < _e.Range)
                    { _e.CastOnUnit(gapcloser.Sender); }
                }

                if (_w.IsReady() && ObjectManager.Player.Distance(gapcloser.Sender) < _w.Range)
                { _w.CastOnUnit(gapcloser.Sender); }
            }
        }

        private static void Interrupter2_OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (_root.Item("InterruptSpells").GetValue<bool>())
            {
                if (_root.Item("RInterrupt").GetValue<bool>())
                {
                    var allys = HeroManager.Allies;
                    if (ObjectManager.Player.Distance(sender) < 100f && _r.IsReady())
                    { _r.CastOnUnit(ObjectManager.Player); }

                    foreach (var ally in allys)
                    {
                        if (ObjectManager.Player.Distance(sender) > _r.Range)
                        {
                            if (ally.Distance(sender) < 100f && ObjectManager.Player.Distance(ally) < _r.Range && _r.IsReady())
                            { _r.CastOnUnit(ally); }
                        }
                    }
                }

                if (_root.Item("WInterrupt").GetValue<bool>())
                {
                    if (ObjectManager.Player.Distance(sender) < _w.Range && _w.IsReady())
                    { _w.CastOnUnit(sender); }
                }
            }
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (_root.Item("DrawQ").GetValue<Circle>().Active)
            { Render.Circle.DrawCircle(ObjectManager.Player.Position, _q.Range, _root.Item("DrawQ").GetValue<Circle>().Color); }
            if (_root.Item("DrawW").GetValue<Circle>().Active)
            { Render.Circle.DrawCircle(ObjectManager.Player.Position, _w.Range, _root.Item("DrawW").GetValue<Circle>().Color); }
            if (_root.Item("DrawE").GetValue<Circle>().Active)
            { Render.Circle.DrawCircle(ObjectManager.Player.Position, _e.Range, _root.Item("DrawE").GetValue<Circle>().Color); }
            if (_root.Item("DrawR").GetValue<Circle>().Active)
            { Render.Circle.DrawCircle(ObjectManager.Player.Position, _r.Range, _root.Item("DrawR").GetValue<Circle>().Color); }
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            if (_root.Item("ComboActive").GetValue<KeyBind>().Active)
            {
                var target = TargetSelector.GetTarget(1000f, TargetSelector.DamageType.Magical);
                if (target != null)
                {
                    if (_root.Item("UseQCombo").GetValue<bool>())
                    {
                        if (ObjectManager.Player.Distance(target) < _q.Range && _q.IsReady())
                        { _q.Cast(target, true); }
                        else
                        {
                            if (ObjectManager.Player.Distance(target) > _q.Range && _q.IsReady() && _e.IsReady())
                            {
                                var minions = MinionManager.GetMinions(_e.Range, MinionTypes.All, MinionTeam.NotAlly);

                                foreach (var minion in minions)
                                {
                                    if (ObjectManager.Player.Distance(minion) < _e.Range && ObjectManager.Player.Distance(target) < 650f + 925f)
                                    {
                                        _e.CastOnUnit(minion);
                                        _q.Cast(target, true);
                                    }
                                }
                            }
                        }
                    }
                    if (_root.Item("UseECombo").GetValue<bool>())
                    {
                        if (ObjectManager.Player.Distance(target) < _e.Range && _e.IsReady())
                        { _e.CastOnUnit(target); }
                    }

                    if (_root.Item("UseIgniteCombo").GetValue<bool>())
                    {
                        if (_igniteSlot != SpellSlot.Unknown && ObjectManager.Player.Spellbook.CanUseSpell(_igniteSlot) == SpellState.Ready)
                        { ObjectManager.Player.Spellbook.CastSpell(_igniteSlot, target); }
                    }

                    if (_root.Item("UseWCombo").GetValue<bool>())
                    {
                        if (ObjectManager.Player.Distance(target) < _w.Range && _w.IsReady())
                        { _w.CastOnUnit(target); }
                    }                                       
                }
            }

            if (_root.Item("HarassActive").GetValue<KeyBind>().Active)
            {
                var hTarget = TargetSelector.GetTarget(1000f, TargetSelector.DamageType.Magical);
                if (hTarget != null)
                {
                    if (ObjectManager.Player.Distance(hTarget) < ObjectManager.Player.AttackRange)
                    { ObjectManager.Player.IssueOrder(GameObjectOrder.AttackUnit, hTarget); }

                    if (_root.Item("UseQHarass").GetValue<bool>())
                    {
                        if (ObjectManager.Player.Distance(hTarget) < _q.Range && _q.IsReady())
                        { _q.Cast(hTarget); }
                        else
                        {
                            if (ObjectManager.Player.Distance(hTarget) > _q.Range && _q.IsReady() && _e.IsReady())
                            {
                                var minions = MinionManager.GetMinions(_e.Range, MinionTypes.All, MinionTeam.NotAlly);

                                foreach (var minion in minions)
                                {
                                    if (ObjectManager.Player.Distance(minion) < _e.Range && ObjectManager.Player.Distance(hTarget) < 650f + 925f)
                                    {
                                        _e.CastOnUnit(minion);
                                        _q.Cast(hTarget, true);
                                    }
                                }
                            }
                        }
                    }

                    if (_root.Item("UseEHarass").GetValue<bool>())
                    {
                        if (ObjectManager.Player.Distance(hTarget) < _e.Range && _e.IsReady())
                        { _e.CastOnUnit(hTarget); }
                    }

                    if (_root.Item("UseWHarass").GetValue<bool>())
                    {
                        if (ObjectManager.Player.Distance(hTarget) < _w.Range && _w.IsReady())
                        { _w.CastOnUnit(hTarget); }
                    }
                }
            }

            if (_root.Item("DefActive").GetValue<bool>())
            {
                // E spell
                var AllyE =
                    ObjectManager.Get<Obj_AI_Base>()
                        .Where(x => !x.IsEnemy)
                        .Where(x => !x.IsDead)
                        .Where(x => x.Distance(ObjectManager.Player.Position) <= _e.Range)
                        .Where(x => x.Health <= x.MaxHealth * (_root.Item("AutoEAlly").GetValue<Slider>().Value / 100));

                var PlayerE = ObjectManager.Player.MaxHealth / 100 * _root.Item("AutoE").GetValue<Slider>().Value;
                if (ObjectManager.Player.Health <= PlayerE)
                { _e.CastOnUnit(ObjectManager.Player); }
                else
                {
                    if (AllyE != null)
                    { _e.CastOnUnit((Obj_AI_Base)AllyE); }
                }

                // R Spell
                var AllyR =
                    ObjectManager.Get<Obj_AI_Base>()
                        .Where(x => !x.IsEnemy)
                        .Where(x => !x.IsDead)
                        .Where(x => x.Distance(ObjectManager.Player.Position) <= _r.Range)
                        .Where(x => x.Health <= x.MaxHealth * (_root.Item("AutoRAlly").GetValue<Slider>().Value / 100));
                var PlayerR = ObjectManager.Player.MaxHealth / 100 * _root.Item("AutoR").GetValue<Slider>().Value;
                if (ObjectManager.Player.Health <= PlayerR)
                { _r.CastOnUnit(ObjectManager.Player); }
                else
                {
                    if (AllyR != null)
                    { _r.CastOnUnit((Obj_AI_Base)AllyR); }
                }
            }

            if (_root.Item("StealActive").GetValue<bool>())
            {
                if (_root.Item("UseQSteal").GetValue<bool>())
                {
                    var target = TargetSelector.GetTarget(1000f, TargetSelector.DamageType.Magical);
                    var dmgQ = ObjectManager.Player.GetSpellDamage(target, SpellSlot.Q);

                    if (ObjectManager.Player.Distance(target) <= _q.Range && _q.IsReady())
                    {
                        if (dmgQ >= target.Health + 35)
                        { _q.Cast(target); }
                    }
                    else
                    {
                        if (ObjectManager.Player.Distance(target) <= _q.Range + _e.Range && _q.IsReady() && _e.IsReady())
                        {
                            var minions = MinionManager.GetMinions(_e.Range, MinionTypes.All, MinionTeam.NotAlly);
                            foreach (var minion in minions)
                            {
                                _e.CastOnUnit(minion);
                                _q.Cast(target, true);
                            }
                        }
                    }
                }

                if (_root.Item("UseESteal").GetValue<bool>())
                {
                    var target = TargetSelector.GetTarget(_e.Range, TargetSelector.DamageType.Magical);
                    var dmgE = ObjectManager.Player.GetSpellDamage(target, SpellSlot.E);
                    if (ObjectManager.Player.Distance(target) <= _e.Range && _e.IsReady())
                    {
                        if (dmgE >= target.Health + 35)
                        { _e.CastOnUnit(target); }
                    }

                }
            }

            if (_root.Item("LaneClearActive").GetValue<KeyBind>().Active)
            {
                foreach (
                    var minion in
                        ObjectManager.Get<Obj_AI_Minion>()
                            .Where(minion => minion.Team != ObjectManager.Player.Team)
                    )
                {
                    if (_root.Item("UseQFarm").GetValue<bool>())
                    {
                        if (ObjectManager.Player.Distance(minion) <= _q.Range && _q.IsReady())
                        { _q.Cast(minion); }
                    }
                    if(_root.Item("UseEFarm").GetValue<bool>())
                    {
                        if (ObjectManager.Player.Distance(minion) <= _e.Range && _e.IsReady())
                        { _e.CastOnUnit(minion); }
                    }
                }
            }
        }
    }
}
