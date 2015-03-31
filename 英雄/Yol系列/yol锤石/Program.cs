﻿using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace yol0Thresh
{
    internal class Program
    {
        private const string Revision = "1.0.0.3";
        private static readonly Obj_AI_Hero Player = ObjectManager.Player;

        private static readonly Spell _Q = new Spell(SpellSlot.Q, 1075);
        private static readonly Spell _W = new Spell(SpellSlot.W, 950);
        private static readonly Spell _E = new Spell(SpellSlot.E, 500);
        private static readonly Spell _R = new Spell(SpellSlot.R, 400);

        private static Menu Config;

        private static int qTick;
        private static int hookTick;
        private static Obj_AI_Base hookedUnit;


        private static List<Vector3> escapeSpots = new List<Vector3>();
        private static List<GameObject> soulList = new List<GameObject>();

        private static Obj_AI_Hero currentTarget
        {
            get
            {
                if (Hud.SelectedUnit != null && Hud.SelectedUnit is Obj_AI_Hero && Hud.SelectedUnit.Team != Player.Team)
                    return (Obj_AI_Hero)Hud.SelectedUnit;
                if (TargetSelector.GetSelectedTarget() != null)
                    return TargetSelector.GetSelectedTarget();
                return TargetSelector.GetTarget(qRange + 175, TargetSelector.DamageType.Physical);
            }
        }

        private static float qRange
        {
            get { return Config.SubMenu("Misc").Item("qRange").GetValue<Slider>().Value; }
        }

        public static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += OnLoad;
        }

        private static void OnLoad(EventArgs args)
        {
            if (Player.ChampionName != "Thresh")
                return;

            _Q.SetSkillshot(0.5f, 70, 1900, true, SkillshotType.SkillshotLine);
            _W.SetSkillshot(0f, 200, 1750, false, SkillshotType.SkillshotCircle);
            _E.SetSkillshot(0.3f, 60, float.MaxValue, false, SkillshotType.SkillshotLine);

            Config = new Menu("花边-yol0锤石", "Thresh", true);
            Config.AddSubMenu(new Menu("走砍", "Orbwalking"));
            xSLxOrbwalker.AddToMenu(Config.SubMenu("Orbwalking"));

            Config.AddSubMenu(new Menu("目标 选择", "Target Selector"));
            TargetSelector.AddToMenu(Config.SubMenu("Target Selector"));

            Config.AddSubMenu(new Menu("连招 设置", "Combo"));
            Config.SubMenu("Combo").AddItem(new MenuItem("useQ1", "使用 Q1").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("useQ2", "使用 Q2").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("useE", "使用 E").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("useW", "使用 W 保护Or拉队友").SetValue(true));


            Config.AddSubMenu(new Menu("骚扰 设置", "Harass"));
            Config.SubMenu("Harass").AddItem(new MenuItem("useQ1", "使用 Q1").SetValue(true));
            Config.SubMenu("Harass").AddItem(new MenuItem("useE", "使用 E").SetValue(true));
            Config.SubMenu("Harass").AddItem(new MenuItem("manaPercent", "骚扰最低蓝量比").SetValue(new Slider(40, 1, 100)));

            Config.AddSubMenu(new Menu("E 设置", "Flay"));
            Config.SubMenu("Flay").AddItem(new MenuItem("pullEnemy", "拉近 E").SetValue(new KeyBind(90, KeyBindType.Press)));
            Config.SubMenu("Flay").AddItem(new MenuItem("pushEnemy", "推走 E").SetValue(new KeyBind(88, KeyBindType.Press)));
            Config.SubMenu("Flay").AddSubMenu(new Menu("英雄 设置", "ActionToTake"));
            foreach (Obj_AI_Hero enemy in ObjectManager.Get<Obj_AI_Hero>().Where(unit => unit.Team != Player.Team))
            {
                Config.SubMenu("Flay").SubMenu("ActionToTake").AddItem(new MenuItem(enemy.ChampionName, enemy.ChampionName).SetValue(new StringList(new[] { "拉近", "推走" })));
            }

            Config.AddSubMenu(new Menu("W 设置", "Lantern"));
            Config.SubMenu("Lantern").AddItem(new MenuItem("useW", "保护队友").SetValue(true));
            Config.SubMenu("Lantern")
                .AddItem(new MenuItem("numEnemies", "附近敌人数").SetValue(new Slider(2, 1, 5)));
            Config.SubMenu("Lantern").AddItem(new MenuItem("useWCC", "队友被控放灯笼").SetValue(true));

            Config.AddSubMenu(new Menu("R 设置", "Box"));
            Config.SubMenu("Box").AddItem(new MenuItem("useR", "自动 使用 R").SetValue(true));
            Config.SubMenu("Box").AddItem(new MenuItem("minEnemies", "最小命中数").SetValue(new Slider(3, 1, 5)));

            Config.AddSubMenu(new Menu("杂项 设置", "Misc"));
            Config.SubMenu("Misc").AddItem(new MenuItem("qRange", "Q 范围").SetValue(new Slider(1075, 700, 1075)));
            Config.SubMenu("Misc").AddItem(new MenuItem("qHitChance", "Q 命中").SetValue(new StringList(new[] { "非常 高", "高", "中", "低" }, 1)));
            Config.SubMenu("Misc").AddItem(new MenuItem("dashes", "推走突进者").SetValue(true));
            Config.SubMenu("Misc").AddItem(new MenuItem("packetCasting", "使用封包").SetValue(false));

            Config.SubMenu("Misc").AddSubMenu(new Menu("突进 设置", "Gapclosers"));
            if (ObjectManager.Get<Obj_AI_Hero>().Any(unit => unit.Team != Player.Team && unit.ChampionName == "Rengar"))
            {
                Config.SubMenu("Misc").SubMenu("Gapclosers").AddItem(new MenuItem("rengarleap", "Rengar - Unseen Predator").SetValue(true));
            }
            foreach (Gapcloser spell in AntiGapcloser.Spells)
            {
                foreach (Obj_AI_Hero enemy in ObjectManager.Get<Obj_AI_Hero>().Where(unit => unit.Team != Player.Team))
                {
                    if (spell.ChampionName == enemy.ChampionName)
                    {
                        Config.SubMenu("Misc").SubMenu("Gapclosers").AddItem(new MenuItem(spell.SpellName, spell.ChampionName + " - " + spell.SpellName).SetValue(true));
                    }
                }
            }

            Config.SubMenu("Misc").AddSubMenu(new Menu("打断 技能", "InterruptSpells"));
            foreach (InterruptableSpell spell in Interrupter.Spells)
            {
                foreach (Obj_AI_Hero enemy in ObjectManager.Get<Obj_AI_Hero>().Where(unit => unit.Team != Player.Team))
                {
                    if (spell.ChampionName == enemy.ChampionName)
                    {
                        Config.SubMenu("Misc").SubMenu("InterruptSpells").AddSubMenu(new Menu(enemy.ChampionName + " - " + spell.SpellName, spell.SpellName));
                        Config.SubMenu("Misc").SubMenu("InterruptSpells").SubMenu(spell.SpellName).AddItem(new MenuItem("enabled", "启用").SetValue(true));
                        Config.SubMenu("Misc").SubMenu("InterruptSpells").SubMenu(spell.SpellName).AddItem(new MenuItem("useE", "E 打断").SetValue(true));
                        Config.SubMenu("Misc").SubMenu("InterruptSpells").SubMenu(spell.SpellName).AddItem(new MenuItem("useQ", "Q 打断").SetValue(true));
                    }
                }
            }

            Config.AddSubMenu(new Menu("击杀 设置", "KS"));
            Config.SubMenu("KS").AddItem(new MenuItem("ksQ", "Q").SetValue(false));
            Config.SubMenu("KS").AddItem(new MenuItem("ksE", "E").SetValue(false));

            Config.AddSubMenu(new Menu("显示 设置", "Draw"));
            Config.SubMenu("Draw").AddItem(new MenuItem("drawQMax", "显示 Q Max 范围").SetValue(new Circle(true, Color.Red)));
            Config.SubMenu("Draw").AddItem(new MenuItem("drawQEffective", "显示 Q 拉近").SetValue(new Circle(true, Color.Blue)));
            Config.SubMenu("Draw").AddItem(new MenuItem("drawW", "显示 W 范围").SetValue(new Circle(false, Color.Green)));
            Config.SubMenu("Draw").AddItem(new MenuItem("drawE", "显示 E 范围").SetValue(new Circle(false, Color.Aqua)));
            Config.SubMenu("Draw").AddItem(new MenuItem("drawQCol", "显示 Q 线条").SetValue(true));
            Config.SubMenu("Draw").AddItem(new MenuItem("drawTargetC", "显示 目标 (线圈)").SetValue(true));
            Config.SubMenu("Draw").AddItem(new MenuItem("drawTargetT", "显示 目标 (文本)").SetValue(true));
            Config.SubMenu("Draw").AddItem(new MenuItem("drawSouls", "显示 灵魂线圈").SetValue(new Circle(true, Color.DeepSkyBlue)));

            Config.AddSubMenu(new Menu("信息 通知", "message"));
            Config.SubMenu("message").AddItem(new MenuItem("Sprite", "yol0-锤石"));
            Config.SubMenu("message").AddItem(new MenuItem("Hanhua", "汉化:花边下丶情未央"));
            Config.SubMenu("message").AddItem(new MenuItem("qqqun", "QQ群:299606556"));

            Config.AddToMainMenu();
            Obj_AI_Base.OnProcessSpellCast += OnProcessSpell;
            Obj_AI_Base.OnPlayAnimation += OnAnimation;
            Drawing.OnDraw += OnDraw;
            Game.OnUpdate += OnGameUpdate;
            GameObject.OnCreate += OnCreateObj;
            AntiGapcloser.OnEnemyGapcloser += OnEnemyGapCloser;
            Interrupter.OnPossibleToInterrupt += OnPossibleToInterrupt;

            Game.PrintChat("<font color=\"#FF0F0\">yol0 Thresh v" + Revision + " loaded!</font>");

            Game.PrintChat("<font color=\"#1eff00\">Huabian婕㈠寲QQ缇わ細299606556</font> - <font color=\"#00BFFF\">姝¤繋鍚勪綅鐨勫姞鍏ワ紒</font>");

        }

        public static void OnGameUpdate(EventArgs args)
        {
            AutoBox();
            KS();
            Lantern();
            UpdateSouls();
            UpdateBuffs();

            /*if (Config.SubMenu("Misc").Item("dashes").GetValue<bool>())
            {
                foreach (Obj_AI_Hero enemy in ObjectManager.Get<Obj_AI_Hero>().Where(unit => unit.Team != Player.Team))
                {
                    var pIn = new PredictionInput
                    {
                        Unit = enemy,
                        Delay = 300,
                        Aoe = false,
                        Collision = false,
                        Radius = 200,
                        Range = _E.Range,
                        Speed = 2000,
                        Type = SkillshotType.SkillshotLine,
                        RangeCheckFrom = Player.ServerPosition,
                    };
                    PredictionOutput pOut = Prediction.GetPrediction(pIn);
                    float pX = Player.Position.X + (Player.Position.X - pOut.CastPosition.X);
                    float pY = Player.Position.Y + (Player.Position.Y - pOut.CastPosition.Y);
                    if (pOut.Hitchance == HitChance.Dashing && Player.Distance(pOut.CastPosition) < 125 && _E.IsReady())
                    {
                        _E.Cast(new Vector2(pX, pY), PacketCasting());
                    }
                }
            }*/

            if (xSLxOrbwalker.CurrentMode == xSLxOrbwalker.Mode.Combo)
                Combo();

            if (xSLxOrbwalker.CurrentMode == xSLxOrbwalker.Mode.Harass)
                Harass();

            if (Config.SubMenu("Flay").Item("pullEnemy").GetValue<KeyBind>().Active)
            {
                Obj_AI_Hero target = TargetSelector.GetTarget(_E.Range, TargetSelector.DamageType.Physical);
                if (target != null)
                    PullFlay(target);
            }

            if (Config.SubMenu("Flay").Item("pushEnemy").GetValue<KeyBind>().Active)
            {
                Obj_AI_Hero target = TargetSelector.GetTarget(_E.Range, TargetSelector.DamageType.Physical);
                if (target != null)
                    PushFlay(target);
            }
        }

        public static void OnDraw(EventArgs args)
        {
            if (Config.SubMenu("Draw").Item("drawQMax").GetValue<Circle>().Active && !Player.IsDead)
            {
                Utility.DrawCircle(Player.Position, _Q.Range,
                    Config.SubMenu("Draw").Item("drawQMax").GetValue<Circle>().Color);
            }

            if (Config.SubMenu("Draw").Item("drawQEffective").GetValue<Circle>().Active && !Player.IsDead)
            {
                Utility.DrawCircle(Player.Position, qRange,
                    Config.SubMenu("Draw").Item("drawQEffective").GetValue<Circle>().Color);
            }

            if (Config.SubMenu("Draw").Item("drawW").GetValue<Circle>().Active && !Player.IsDead)
            {
                Utility.DrawCircle(Player.Position, _W.Range,
                    Config.SubMenu("Draw").Item("drawW").GetValue<Circle>().Color);
            }

            if (Config.SubMenu("Draw").Item("drawE").GetValue<Circle>().Active && !Player.IsDead)
            {
                Utility.DrawCircle(Player.Position, _E.Range,
                    Config.SubMenu("Draw").Item("drawE").GetValue<Circle>().Color);
            }

            if (Config.SubMenu("Draw").Item("drawQCol").GetValue<bool>() && !Player.IsDead)
            {
                if (Player.Distance(currentTarget) < qRange + 200)
                {
                    Vector2 playerPos = Drawing.WorldToScreen(Player.Position);
                    Vector2 targetPos = Drawing.WorldToScreen(currentTarget.Position);
                    Drawing.DrawLine(playerPos, targetPos, 4,
                        _Q.GetPrediction(currentTarget, overrideRange: qRange).Hitchance < GetSelectedHitChance()
                            ? Color.Red
                            : Color.Green);
                }
            }

            if (Config.SubMenu("Draw").Item("drawTargetC").GetValue<bool>() && currentTarget.IsVisible &&
                !currentTarget.IsDead)
            {
                Utility.DrawCircle(currentTarget.Position, currentTarget.BoundingRadius + 10, Color.Red);
                Utility.DrawCircle(currentTarget.Position, currentTarget.BoundingRadius + 25, Color.Red);
                Utility.DrawCircle(currentTarget.Position, currentTarget.BoundingRadius + 45, Color.Red);
            }

            if (Config.SubMenu("Draw").Item("drawTargetT").GetValue<bool>() && !currentTarget.IsDead)
            {
                Drawing.DrawText(100, 150, Color.Red, "Current Target: " + currentTarget.ChampionName);
            }

            if (Config.SubMenu("Draw").Item("drawSouls").GetValue<Circle>().Active && !Player.IsDead)
            {
                foreach (GameObject soul in soulList.Where(s => s.IsValid))
                {
                    Utility.DrawCircle(soul.Position, 50,
                        Config.SubMenu("Draw").Item("drawSouls").GetValue<Circle>().Color);
                }
            }
        }

        public static void OnAnimation(GameObject unit, GameObjectPlayAnimationEventArgs args)
        {
            if (unit is Obj_AI_Hero)
            {
                var hero = (Obj_AI_Hero)unit;
                if (hero.Team != Player.Team)
                {
                    if (hero.ChampionName == "Rengar" && args.Animation == "Spell5" && Player.Distance(hero) <= 725)
                    {
                        if (_E.IsReady() &&
                            Config.SubMenu("Misc").SubMenu("Gapclosers").Item("rengarleap").GetValue<bool>())
                        {
                            _E.Cast(unit.Position, PacketCasting());
                        }
                    }
                }
            }
        }

        public static void OnProcessSpell(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsMe)
            {
                if (args.SData.Name == "ThreshQ")
                {
                    qTick = Environment.TickCount + 500;
                }
            }
        }

        public static void OnCreateObj(GameObject obj, EventArgs args)
        {
            if (obj.Name.Contains("ChaosMinion") && obj.Team == Player.Team)
            {
                soulList.Add(obj);
            }
        }

        public static void OnPossibleToInterrupt(Obj_AI_Base unit, InterruptableSpell spell)
        {
            if (
                Config.SubMenu("Misc")
                    .SubMenu("InterruptSpells")
                    .SubMenu(spell.SpellName)
                    .Item("enabled")
                    .GetValue<bool>())
            {
                if (
                    Config.SubMenu("Misc")
                        .SubMenu("InterruptSpells")
                        .SubMenu(spell.SpellName)
                        .Item("useE")
                        .GetValue<bool>() && _E.IsReady() &&
                    Player.Distance(unit) < _E.Range)
                {
                    if (ShouldPull((Obj_AI_Hero)unit))
                        PullFlay(unit);
                    else
                        PushFlay(unit);
                }
                else if (
                    Config.SubMenu("Misc")
                        .SubMenu("InterruptSpells")
                        .SubMenu(spell.SpellName)
                        .Item("useQ")
                        .GetValue<bool>() && _Q.IsReady() &&
                    !_Q.GetPrediction(unit).CollisionObjects.Any())
                {
                    _Q.Cast(unit, PacketCasting());
                }
            }
        }

        public static void OnEnemyGapCloser(ActiveGapcloser gapcloser)
        {
            if (_E.IsReady() &&
                Config.SubMenu("Misc").SubMenu("Gapclosers").Item(gapcloser.SpellName.ToLower()).GetValue<bool>() &&
                Player.Distance(gapcloser.Sender) < _E.Range + 100)
            {
                if (Player.Distance(gapcloser.Start) < Player.Distance(gapcloser.End))
                    PullFlay(gapcloser.Sender);
                else
                    PushFlay(gapcloser.Sender);
            }
        }

        private static void UpdateBuffs()
        {
            if (hookedUnit == null)
            {
                foreach (Obj_AI_Base obj in ObjectManager.Get<Obj_AI_Base>().Where(unit => unit.Team != Player.Team))
                {
                    if (obj.HasBuff("threshqfakeknockup"))
                    {
                        hookedUnit = obj;
                        hookTick = Environment.TickCount + 1500;
                        return;
                    }
                }
            }
            hookTick = 0;
            hookedUnit = null;
        }

        private static void UpdateSouls()
        {
            foreach (GameObject soul in soulList.Where(soul => !soul.IsValid))
            {
                soulList.Remove(soul);
            }
        }

        private static bool ShouldPull(Obj_AI_Hero unit)
        {
            return
                Config.SubMenu("Flay")
                    .SubMenu("ActionToTake")
                    .Item(unit.ChampionName)
                    .GetValue<StringList>()
                    .SelectedIndex == 0;
        }

        private static bool IsFirstQ()
        {
            return _Q.Instance.Name == "ThreshQ";
        }

        private static bool IsSecondQ()
        {
            return _Q.Instance.Name == "threshqleap";
        }

        private static bool IsImmune(Obj_AI_Base unit)
        {
            return unit.HasBuff("BlackShield") || unit.HasBuff("SivirE") || unit.HasBuff("NocturneShroudofDarkness") ||
                   unit.HasBuff("deathdefiedbuff");
        }

        private static void KS()
        {
            if (Config.SubMenu("KS").Item("ksE").GetValue<bool>())
            {
                foreach (
                    Obj_AI_Hero enemy in
                        from enemy in
                            ObjectManager.Get<Obj_AI_Hero>().Where(unit => unit.Team != Player.Team && !unit.IsDead)
                        let eDmg = Player.GetSpellDamage(enemy, SpellSlot.E)
                        where eDmg > enemy.Health && Player.Distance(enemy) <= _E.Range && _E.IsReady()
                        select enemy)
                {
                    PullFlay(enemy);
                    return;
                }
            }

            if (Config.SubMenu("KS").Item("ksQ").GetValue<bool>())
            {
                foreach (
                    Obj_AI_Hero enemy in
                        from enemy in
                            ObjectManager.Get<Obj_AI_Hero>().Where(unit => unit.Team != Player.Team && !unit.IsDead)
                        let qDmg = Player.GetSpellDamage(enemy, SpellSlot.Q)
                        where qDmg > enemy.Health && Player.Distance(enemy) <= qRange && IsFirstQ() && _Q.IsReady() &&
                              _Q.GetPrediction(enemy, overrideRange: qRange).Hitchance >= GetSelectedHitChance()
                        select enemy)
                {
                    _Q.Cast(enemy);
                    return;
                }
            }
        }

        private static bool PacketCasting()
        {
            return Config.SubMenu("Misc").Item("packetCasting").GetValue<bool>();
        }

        private static HitChance GetSelectedHitChance()
        {
            switch (Config.SubMenu("Misc").Item("qHitChance").GetValue<StringList>().SelectedIndex)
            {
                case 0:
                    return HitChance.VeryHigh;
                case 1:
                    return HitChance.High;
                case 2:
                    return HitChance.Medium;
                case 3:
                    return HitChance.Low;
            }
            return HitChance.Medium;
        }

        private static void AutoBox()
        {
            if (Config.SubMenu("Box").Item("useR").GetValue<bool>() && _R.IsReady() &&
                ObjectManager.Get<Obj_AI_Hero>()
                    .Count(unit => unit.Team != Player.Team && Player.Distance(unit) <= _R.Range) >=
                Config.SubMenu("Box").Item("minEnemies").GetValue<Slider>().Value)
            {
                _R.Cast(PacketCasting());
            }
        }

        private static void Combo()
        {
            if (Config.SubMenu("Combo").Item("useE").GetValue<bool>() && _E.IsReady() &&
                Player.Distance(currentTarget) < _E.Range &&
                (!_Q.IsReady() && Environment.TickCount > qTick || _Q.IsReady() && IsFirstQ()))
            {
                Flay(currentTarget);
            }
            else if (Config.SubMenu("Combo").Item("useQ2").GetValue<bool>() && Player.Distance(currentTarget) > _E.Range &&
                     _Q.IsReady() &&
                     Environment.TickCount >= hookTick - 500 && IsSecondQ() &&
                     ObjectManager.Get<Obj_AI_Hero>().FirstOrDefault(unit => unit.HasBuff("ThreshQ")) != null)
            {
                _Q.Cast(PacketCasting());
            }
            else if (Config.SubMenu("Combo").Item("useQ2").GetValue<bool>() &&
                     Config.SubMenu("Combo").Item("useE").GetValue<bool>() && _Q.IsReady() &&
                     _E.IsReady() && ObjectManager.Get<Obj_AI_Minion>().FirstOrDefault(unit => unit.HasBuff("ThreshQ") && unit.Distance(currentTarget) <= _E.Range) != null && IsSecondQ())
            {
                _Q.Cast(PacketCasting());
            }

            if (Config.SubMenu("Combo").Item("useQ1").GetValue<bool>() && _Q.IsReady() && IsFirstQ() &&
                !IsImmune(currentTarget))
            {
                _Q.CastIfHitchanceEquals(currentTarget, GetSelectedHitChance(), PacketCasting());
                /*if (_Q.GetPrediction(currentTarget, false, qRange).Hitchance >= GetSelectedHitChance())
                {
                    if (currentTarget.HasBuffOfType(BuffType.Slow))
                        _Q.Cast(currentTarget.ServerPosition, PacketCasting());
                    else
                        _Q.Cast(currentTarget, PacketCasting());
                }*/
            }

            if (Config.SubMenu("Lantern").Item("useW").GetValue<bool>() && _W.IsReady() && ObjectManager.Get<Obj_AI_Hero>().FirstOrDefault(unit => unit.HasBuff("ThreshQ")) != null)
            {
                Obj_AI_Hero nearAlly = GetNearAlly();
                if (nearAlly != null)
                {
                    _W.Cast(nearAlly, PacketCasting());
                }
            }
        }

        private static void Harass()
        {
            float percentManaAfterQ = 100 * ((Player.Mana - _Q.Instance.ManaCost) / Player.MaxMana);
            float percentManaAfterE = 100 * ((Player.Mana - _E.Instance.ManaCost) / Player.MaxMana);
            int minPercentMana = Config.SubMenu("Harass").Item("manaPercent").GetValue<Slider>().Value;

            if (Config.SubMenu("Harass").Item("useQ1").GetValue<bool>() && _Q.IsReady() && IsFirstQ() &&
                !IsImmune(currentTarget) && percentManaAfterQ >= minPercentMana)
            {
                if (_Q.GetPrediction(currentTarget, false, qRange).Hitchance >= GetSelectedHitChance())
                {
                    _Q.Cast(currentTarget, PacketCasting());
                }
            }
            else if (Config.SubMenu("Harass").Item("useE").GetValue<bool>() && !IsImmune(currentTarget) && _E.IsReady() &&
                     Player.Distance(currentTarget) < _E.Range && percentManaAfterE >= minPercentMana)
            {
                Flay(currentTarget);
            }
        }

        private static void Lantern()
        {
            if (Config.SubMenu("Lantern").Item("useWCC").GetValue<bool>() && GetCCAlly() != null && _W.IsReady())
            {
                _W.Cast(GetCCAlly(), PacketCasting());
                return;
            }

            if (Config.SubMenu("Lantern").Item("useW").GetValue<bool>() && GetLowAlly() != null && _W.IsReady())
            {
                if (GetLowAlly().Position.CountEnemysInRange(950) >=
                    Config.SubMenu("Lantern").Item("numEnemies").GetValue<Slider>().Value)
                {
                    _W.Cast(GetLowAlly(), PacketCasting());
                }
            }
        }

        private static Obj_AI_Hero GetCCAlly()
        {
            return
                ObjectManager.Get<Obj_AI_Hero>()
                    .Where(
                        unit =>
                            !unit.IsMe && unit.Team == Player.Team && !unit.IsDead &&
                            Player.Distance(unit) <= _W.Range + 200)
                    .FirstOrDefault(
                        ally =>
                            ally.HasBuffOfType(BuffType.Charm) || ally.HasBuffOfType(BuffType.CombatDehancer) ||
                            ally.HasBuffOfType(BuffType.Fear) || ally.HasBuffOfType(BuffType.Knockback) ||
                            ally.HasBuffOfType(BuffType.Knockup) || ally.HasBuffOfType(BuffType.Polymorph) ||
                            ally.HasBuffOfType(BuffType.Snare) || ally.HasBuffOfType(BuffType.Stun) ||
                            ally.HasBuffOfType(BuffType.Suppression) || ally.HasBuffOfType(BuffType.Taunt));
        }

        private static Obj_AI_Hero GetLowAlly()
        {
            Obj_AI_Hero lowAlly = null;
            foreach (
                Obj_AI_Hero ally in
                    ObjectManager.Get<Obj_AI_Hero>()
                        .Where(
                            unit => unit.Team == Player.Team && !unit.IsDead && Player.Distance(unit) <= _W.Range + 200)
                )
            {
                if (lowAlly == null)
                    lowAlly = ally;
                else if (!lowAlly.IsDead && ally.Health / ally.MaxHealth < lowAlly.Health / lowAlly.MaxHealth)
                    lowAlly = ally;
            }
            return lowAlly;
        }

        private static Obj_AI_Hero GetNearAlly()
        {
            if (Hud.SelectedUnit != null && Hud.SelectedUnit is Obj_AI_Hero && Hud.SelectedUnit.Team == Player.Team &&
                Player.Distance(Hud.SelectedUnit.Position) <= _W.Range + 200)
            {
                return (Obj_AI_Hero)Hud.SelectedUnit;
            }

            Obj_AI_Hero nearAlly = null;
            foreach (
                Obj_AI_Hero ally in
                    ObjectManager.Get<Obj_AI_Hero>()
                        .Where(
                            unit => unit.Team == Player.Team && !unit.IsDead && Player.Distance(unit) <= _W.Range + 200)
                )
            {
                if (nearAlly == null)
                    nearAlly = ally;
                else if (!nearAlly.IsDead && Player.Distance(ally) < Player.Distance(nearAlly))
                    nearAlly = ally;
            }
            return nearAlly;
        }

        private static void PushFlay(Obj_AI_Base unit)
        {
            if (Player.Distance(unit) <= _E.Range)
            {
                _E.Cast(unit.ServerPosition, PacketCasting());
            }
        }

        private static void PullFlay(Obj_AI_Base unit)
        {
            if (Player.Distance(unit) <= _E.Range)
            {
                float pX = Player.Position.X + (Player.Position.X - unit.Position.X);
                float pY = Player.Position.Y + (Player.Position.Y - unit.Position.Y);
                _E.Cast(new Vector2(pX, pY), PacketCasting());
            }
        }

        private static void Flay(Obj_AI_Hero unit)
        {
            if (ShouldPull(unit))
            {
                PullFlay(unit);
            }
            else
            {
                PushFlay(unit);
            }
        }
    }
}
