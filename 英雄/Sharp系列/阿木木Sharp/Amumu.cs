﻿using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;



namespace AmumuSharp
{
    internal class Amumu
    {
        readonly Menu _menu;

        private readonly Spell _spellQ;
        private readonly Spell _spellW;
        private readonly Spell _spellE;
        private readonly Spell _spellR;

        private bool _comboW;

        private static Orbwalking.Orbwalker _orbwalker;

        public Amumu() //add Q near mouse (range), 
        {
            if (ObjectManager.Player.ChampionName != "Amumu")
                return;

            (_menu = new Menu("花边-阿木木Sharp", "AmumuSharp", true)).AddToMainMenu();

            var targetSelectorMenu = new Menu("目标 选择", "TargetSelector");
            TargetSelector.AddToMenu(targetSelectorMenu);
            _menu.AddSubMenu(targetSelectorMenu);

            _orbwalker = new Orbwalking.Orbwalker(_menu.AddSubMenu(new Menu("走砍", "Orbwalking")));

            var comboMenu = _menu.AddSubMenu(new Menu("连招", "Combo"));
            comboMenu.AddItem(new MenuItem("comboQ" + ObjectManager.Player.ChampionName, "使用 Q").SetValue(new StringList(new[] { "禁止", "总是", "目标离开AA范围" }, 1)));
            comboMenu.AddItem(new MenuItem("comboW" + ObjectManager.Player.ChampionName, "使用 W").SetValue(true));
            comboMenu.AddItem(new MenuItem("comboE" + ObjectManager.Player.ChampionName, "使用 E").SetValue(true));
            comboMenu.AddItem(new MenuItem("comboR" + ObjectManager.Player.ChampionName, "自动 使用 R").SetValue(new Slider(3, 0, 5)));
            comboMenu.AddItem(new MenuItem("comboWPercent" + ObjectManager.Player.ChampionName, "使用 W 丨最低蓝量比 %").SetValue(new Slider(10)));

            var farmMenu = _menu.AddSubMenu(new Menu("打钱", "Farming"));
            farmMenu.AddItem(new MenuItem("farmQ" + ObjectManager.Player.ChampionName, "使用 Q").SetValue(new StringList(new[] { "禁止", "总是", "目标离开AA范围" }, 2)));
            farmMenu.AddItem(new MenuItem("farmW" + ObjectManager.Player.ChampionName, "使用 W").SetValue(true));
            farmMenu.AddItem(new MenuItem("farmE" + ObjectManager.Player.ChampionName, "使用 E").SetValue(true));
            farmMenu.AddItem(new MenuItem("farmWPercent" + ObjectManager.Player.ChampionName, "使用 W 丨最低蓝量比 %").SetValue(new Slider(20)));

            var drawMenu = _menu.AddSubMenu(new Menu("显示", "Drawing"));
            drawMenu.AddItem(new MenuItem("drawQ" + ObjectManager.Player.ChampionName, "显示 Q 范围").SetValue(new Circle(true, System.Drawing.Color.FromArgb(125, 0, 255, 0))));
            drawMenu.AddItem(new MenuItem("drawW" + ObjectManager.Player.ChampionName, "显示 W 范围").SetValue(new Circle(false, System.Drawing.Color.FromArgb(125, 0, 255, 0))));
            drawMenu.AddItem(new MenuItem("drawE" + ObjectManager.Player.ChampionName, "显示 E 范围").SetValue(new Circle(false, System.Drawing.Color.FromArgb(125, 0, 255, 0))));
            drawMenu.AddItem(new MenuItem("drawR" + ObjectManager.Player.ChampionName, "显示 R 范围").SetValue(new Circle(false, System.Drawing.Color.FromArgb(125, 0, 255, 0))));

            var miscMenu = _menu.AddSubMenu(new Menu("杂项", "Misc"));
            miscMenu.AddItem(new MenuItem("aimQ" + ObjectManager.Player.ChampionName, "Q 鼠标附近").SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Press)));
            miscMenu.AddItem(new MenuItem("packetCast", "封包").SetValue(true));

            var messageMenu = _menu.AddSubMenu(new Menu("信 息", "message"));
            messageMenu.AddItem(new MenuItem("Sprite", "Beaving-阿木木Sharp"));
            messageMenu.AddItem(new MenuItem("Hanhua", "汉化:花边下丶情未央"));
            messageMenu.AddItem(new MenuItem("qqqun", "QQ群:299606556"));

            _menu.AddItem(new MenuItem("XiaoXiongMei", "带上小胸妹无形装逼"));

            _spellQ = new Spell(SpellSlot.Q, 1080);
            _spellW = new Spell(SpellSlot.W, 300);
            _spellE = new Spell(SpellSlot.E, 350);
            _spellR = new Spell(SpellSlot.R, 550);

            _spellQ.SetSkillshot(.25f, 90, 2000, true, SkillshotType.SkillshotLine);  //check delay
            _spellW.SetSkillshot(0f, _spellW.Range, float.MaxValue, false, SkillshotType.SkillshotCircle); //correct
            _spellE.SetSkillshot(.5f, _spellE.Range, float.MaxValue, false, SkillshotType.SkillshotCircle); //check delay
            _spellR.SetSkillshot(.25f, _spellR.Range, float.MaxValue, false, SkillshotType.SkillshotCircle); //check delay

            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnUpdate += Game_OnUpdate;

            Game.PrintChat("<font color=\"#1eff00\">甯朵笂灏忚兏濡硅閫间辅</font> - <font color=\"#00BFFF\">婕㈠寲By Huabian</font>");
        }

        void Game_OnUpdate(EventArgs args)
        {
            AutoUlt();

            if (_menu.Item("aimQ" + ObjectManager.Player.ChampionName).GetValue<KeyBind>().Active)
                CastQ(Program.Helper.EnemyTeam.Where(x => x.IsValidTarget(_spellQ.Range) && x.Distance(Game.CursorPos) < 400).OrderBy(x => x.Distance(Game.CursorPos)).FirstOrDefault());

            switch (_orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    Combo();
                    break;
                case Orbwalking.OrbwalkingMode.LaneClear:
                    LaneClear();
                    break;
                default:
                    RegulateWState();
                    break;
            }
        }

        void AutoUlt()
        {
            var comboR = _menu.Item("comboR" + ObjectManager.Player.ChampionName).GetValue<Slider>().Value;

            if (comboR > 0 && _spellR.IsReady())
            {
                int enemiesHit = 0;
                int killableHits = 0;

                foreach (Obj_AI_Hero enemy in Program.Helper.EnemyTeam.Where(x => x.IsValidTarget(_spellR.Range)))
                {
                    var prediction = Prediction.GetPrediction(enemy, _spellR.Delay);

                    if (prediction != null && prediction.UnitPosition.Distance(ObjectManager.Player.ServerPosition) <= _spellR.Range)
                    {
                        enemiesHit++;

                        if (ObjectManager.Player.GetSpellDamage(enemy, SpellSlot.W) >= enemy.Health)
                            killableHits++;
                    }
                }

                if (enemiesHit >= comboR || (killableHits >= 1 && ObjectManager.Player.Health / ObjectManager.Player.MaxHealth <= 0.1))
                    CastR();
            }
        }

        void CastE(Obj_AI_Base target)
        {
            if (!_spellE.IsReady() || target == null || !target.IsValidTarget())
                return;

            if (_spellE.GetPrediction(target).UnitPosition.Distance(ObjectManager.Player.ServerPosition) <= _spellE.Range)
                _spellE.CastOnUnit(ObjectManager.Player);
        }

        public float GetManaPercent()
        {
            return (ObjectManager.Player.Mana / ObjectManager.Player.MaxMana) * 100f;
        }

        public bool PacketsNoLel()
        {
            return _menu.Item("packetCast").GetValue<bool>();
        }

        void Combo()
        {
            var comboQ = _menu.Item("comboQ" + ObjectManager.Player.ChampionName).GetValue<StringList>().SelectedIndex;
            var comboW = _menu.Item("comboW" + ObjectManager.Player.ChampionName).GetValue<bool>();
            var comboE = _menu.Item("comboE" + ObjectManager.Player.ChampionName).GetValue<bool>();
            var comboR = _menu.Item("comboR" + ObjectManager.Player.ChampionName).GetValue<Slider>().Value;

            if (comboQ > 0 && _spellQ.IsReady())
            {
                if (_spellR.IsReady() && comboR > 0) //search unit that provides most targets hit by ult. prioritize hero target unit
                {
                    int maxTargetsHit = 0;
                    Obj_AI_Base unitMostTargetsHit = null;

                    foreach (Obj_AI_Base unit in ObjectManager.Get<Obj_AI_Base>().Where(x => x.IsValidTarget(_spellQ.Range) && _spellQ.GetPrediction(x).Hitchance >= HitChance.High)) //causes troubles?
                    {
                        int targetsHit = unit.CountEnemiesInRange((int)_spellR.Range); //unitposition might not reflect where you land with Q

                        if (targetsHit > maxTargetsHit || (unitMostTargetsHit != null && targetsHit >= maxTargetsHit && unit.Type == GameObjectType.obj_AI_Hero))
                        {
                            maxTargetsHit = targetsHit;
                            unitMostTargetsHit = unit;
                        }
                    }

                    if (maxTargetsHit >= comboR)
                        CastQ(unitMostTargetsHit);
                }

                Obj_AI_Base target = TargetSelector.GetTarget(_spellQ.Range, TargetSelector.DamageType.Magical);

                if (target != null)
                    if (comboQ == 1 || (comboQ == 2 && !Orbwalking.InAutoAttackRange(target)))
                        CastQ(target);
            }

            if (comboW && _spellW.IsReady())
            {
                var target = TargetSelector.GetTarget(_spellW.Range, TargetSelector.DamageType.Magical);

                if (target != null)
                {
                    var enoughMana = GetManaPercent() >= _menu.Item("comboWPercent" + ObjectManager.Player.ChampionName).GetValue<Slider>().Value;

                    if (ObjectManager.Player.Spellbook.GetSpell(SpellSlot.W).ToggleState == 1)
                    {
                        if (ObjectManager.Player.Distance(target.ServerPosition) <= _spellW.Range && enoughMana)
                        {
                            _comboW = true;
                            _spellW.Cast();
                        }
                    }
                    else if (!enoughMana)
                        RegulateWState(true);
                }
                else
                    RegulateWState();
            }

            if (comboE && _spellE.IsReady())
                CastE(Program.Helper.EnemyTeam.OrderBy(x => x.Distance(ObjectManager.Player)).FirstOrDefault());
        }

        void LaneClear()
        {
            var farmQ = _menu.Item("farmQ" + ObjectManager.Player.ChampionName).GetValue<StringList>().SelectedIndex;
            var farmW = _menu.Item("farmW" + ObjectManager.Player.ChampionName).GetValue<bool>();
            var farmE = _menu.Item("farmE" + ObjectManager.Player.ChampionName).GetValue<bool>();

            List<Obj_AI_Base> minions;

            if (farmQ > 0 && _spellQ.IsReady())
            {
                Obj_AI_Base minion = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, _spellQ.Range, MinionTypes.All, MinionTeam.NotAlly, MinionOrderTypes.MaxHealth).FirstOrDefault(x => _spellQ.GetPrediction(x).Hitchance >= HitChance.Medium);

                if (minion != null)
                    if (farmQ == 1 || (farmQ == 2 && !Orbwalking.InAutoAttackRange(minion)))
                        CastQ(minion, HitChance.Medium);
            }

            if (farmE && _spellE.IsReady())
            {
                minions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, _spellE.Range, MinionTypes.All, MinionTeam.NotAlly);
                CastE(minions.OrderBy(x => x.Distance(ObjectManager.Player)).FirstOrDefault());
            }

            if (!farmW || !_spellW.IsReady())
                return;
            _comboW = false;

            minions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, _spellW.Range, MinionTypes.All, MinionTeam.NotAlly);

            bool anyJungleMobs = minions.Any(x => x.Team == GameObjectTeam.Neutral);

            var enoughMana = GetManaPercent() > _menu.Item("farmWPercent" + ObjectManager.Player.ChampionName).GetValue<Slider>().Value;

            if (enoughMana && ((minions.Count >= 3 || anyJungleMobs) && ObjectManager.Player.Spellbook.GetSpell(SpellSlot.W).ToggleState == 1))
                _spellW.Cast();
            else if (!enoughMana || ((minions.Count <= 2 && !anyJungleMobs) && ObjectManager.Player.Spellbook.GetSpell(SpellSlot.W).ToggleState == 2))
                RegulateWState(!enoughMana);
        }

        void RegulateWState(bool ignoreTargetChecks = false)
        {
            if (!_spellW.IsReady() || ObjectManager.Player.Spellbook.GetSpell(SpellSlot.W).ToggleState != 2)
                return;

            var target = TargetSelector.GetTarget(_spellW.Range, TargetSelector.DamageType.Magical);
            var minions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, _spellW.Range, MinionTypes.All, MinionTeam.NotAlly);

            if (!ignoreTargetChecks && (target != null || (!_comboW && minions.Count != 0)))
                return;

            _spellW.Cast();
            _comboW = false;
        }

        void CastQ(Obj_AI_Base target, HitChance hitChance = HitChance.High)
        {
            if (!_spellQ.IsReady())
                return;
            if (target == null || !target.IsValidTarget())
                return;

            _spellQ.CastIfHitchanceEquals(target, hitChance);
        }

        void CastR()
        {
            if (!_spellR.IsReady())
                return;
            _spellR.Cast();
        }

        void Drawing_OnDraw(EventArgs args)
        {
            if (!ObjectManager.Player.IsDead)
            {
                var drawQ = _menu.Item("drawQ" + ObjectManager.Player.ChampionName).GetValue<Circle>();
                var drawW = _menu.Item("drawW" + ObjectManager.Player.ChampionName).GetValue<Circle>();
                var drawE = _menu.Item("drawE" + ObjectManager.Player.ChampionName).GetValue<Circle>();
                var drawR = _menu.Item("drawR" + ObjectManager.Player.ChampionName).GetValue<Circle>();

                if (drawQ.Active)
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, _spellQ.Range, drawQ.Color);

                if (drawW.Active)
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, _spellW.Range, drawW.Color);

                if (drawE.Active)
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, _spellE.Range, drawE.Color);

                if (drawR.Active)
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, _spellR.Range, drawR.Color);
            }
        }
    }
}
