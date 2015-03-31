using System;
using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace KurisuMorgana
{
    internal class Program
    {
        private static Menu _menu;
        private static Spell _q, _w, _e, _r;
        private static Orbwalking.Orbwalker _orbwalker;
        private static readonly Obj_AI_Hero Me = ObjectManager.Player;
        static void Main(string[] args)
        {
            Console.WriteLine("Morgana injected...");
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            if (Me.ChampionName != "Morgana")
                return;

            // set spells
            _q = new Spell(SpellSlot.Q, 1175f);
            _q.SetSkillshot(0.25f, 72f, 1400f, true, SkillshotType.SkillshotLine);

            _w = new Spell(SpellSlot.W, 900f);
            _w.SetSkillshot(0.25f, 175f, 1200f, false, SkillshotType.SkillshotCircle);

            _e = new Spell(SpellSlot.E, 750f);
            _r = new Spell(SpellSlot.R, 600f);

            _menu = new Menu("花边-Kurisu莫甘娜", "morgana", true);

            var orbmenu = new Menu("走 砍", "orbwalker");
            _orbwalker = new Orbwalking.Orbwalker(orbmenu);
            _menu.AddSubMenu(orbmenu);

            var tsmenu = new Menu("目标 选择", "selector");
            TargetSelector.AddToMenu(tsmenu);
            _menu.AddSubMenu(tsmenu);

            var drmenu = new Menu("范 围", "drawings");
            drmenu.AddItem(new MenuItem("drawq", "显示 Q 范围")).SetValue(true);
            drmenu.AddItem(new MenuItem("draww", "显示 W 范围")).SetValue(true);
            drmenu.AddItem(new MenuItem("drawe", "显示 E 范围")).SetValue(true);
            drmenu.AddItem(new MenuItem("drawr", "显示 R 范围")).SetValue(true);
            drmenu.AddItem(new MenuItem("drawkill", "显示 击杀")).SetValue(true);
            drmenu.AddItem(new MenuItem("drawtarg", "显示 目标 线圈")).SetValue(true);
            drmenu.AddItem(new MenuItem("debugdmg", "调试 模式")).SetValue(false);
            _menu.AddSubMenu(drmenu);

            var spellmenu = new Menu("法术 设置", "spells");

            var menuQ = new Menu("Q", "qmenu");
            menuQ.AddItem(new MenuItem("hitchanceq", "Q 命中")).SetValue(new Slider(3, 1, 4));
            menuQ.AddItem(new MenuItem("useqcombo", "连招时使用")).SetValue(true);
            menuQ.AddItem(new MenuItem("useharassq", "骚扰时使用")).SetValue(true);
            menuQ.AddItem(new MenuItem("useqanti", "被突进时使用")).SetValue(true);
            menuQ.AddItem(new MenuItem("useqauto", "目标被束缚时使用")).SetValue(true);
            menuQ.AddItem(new MenuItem("useqdash", "被碰撞时使用")).SetValue(true);
            spellmenu.AddSubMenu(menuQ);

            var menuW = new Menu("W", "wmenu");
            menuW.AddItem(new MenuItem("hitchancew", "W 命中")).SetValue(new Slider(2, 1, 4));
            menuW.AddItem(new MenuItem("usewcombo", "连招时使用")).SetValue(true);
            menuW.AddItem(new MenuItem("useharassw", "骚扰时使用")).SetValue(true);
            menuW.AddItem(new MenuItem("usewauto", "目标被束缚时使用")).SetValue(true);
            menuW.AddItem(new MenuItem("waitfor", "等待Q CD好再使用")).SetValue(true);
            menuW.AddItem(new MenuItem("calcw", "计算 标记")).SetValue(new Slider(6, 3, 10));
            spellmenu.AddSubMenu(menuW);

            var menuE = new Menu("E", "emenu");
            menuE.AddItem(new MenuItem("eco", "检查小兵碰撞")).SetValue(false);
            menuE.AddItem(new MenuItem("eco2", "检查英雄碰撞")).SetValue(false);

            // create menu per ally
            var allyMenu = new Menu("使用目标", "useonwho");
            foreach (var frn in ObjectManager.Get<Obj_AI_Hero>().Where(x => x.Team == Me.Team))
                allyMenu.AddItem(new MenuItem("useon" + frn.ChampionName, frn.ChampionName)).SetValue(true);              

            menuE.AddSubMenu(allyMenu);
   
            foreach (var ene in ObjectManager.Get<Obj_AI_Hero>().Where(x => x.Team != Me.Team))
            {
                // create menu per enemy
                var champMenu = new Menu(ene.ChampionName, "cm" + ene.NetworkId);

                // check if spell is supported in lib
                foreach (var lib in KurisuLib.CCList.Where(x => x.HeroName == ene.ChampionName))
                {
                    var skillMenu = new Menu(lib.Slot + " - " + lib.SpellMenuName, "技能: " + lib.SDataName);
                    skillMenu.AddItem(new MenuItem(lib.SDataName + "on", "启用")).SetValue(true);
                    skillMenu.AddItem(new MenuItem(lib.SDataName + "wait", "等待碰撞(禁用)")).SetValue(false);
                    skillMenu.AddItem(new MenuItem(lib.SDataName + "pr", "预判"))
                        .SetValue(new Slider(lib.DangerLevel, 1, 5));
                    champMenu.AddSubMenu(skillMenu);
                }

                menuE.AddSubMenu(champMenu);
            }

            spellmenu.AddSubMenu(menuE);

            var menuR = new Menu("R", "rmenu");
            menuR.AddItem(new MenuItem("usercombo", "启 用")).SetValue(true);
            menuR.AddItem(new MenuItem("rkill", "连招时使用丨假如可击杀")).SetValue(true);
            menuR.AddItem(new MenuItem("rcount", "连招时使用丨敌人数>= ")).SetValue(new Slider(3, 1, 5));
            menuR.AddItem(new MenuItem("useautor", "使用大招丨敌人数>= ")).SetValue(new Slider(4, 2, 5));
            spellmenu.AddSubMenu(menuR);

            spellmenu.AddItem(new MenuItem("harassmana", "骚扰 最低蓝量比")).SetValue(new Slider(55, 0, 99));
            _menu.AddSubMenu(spellmenu);

            var menuM = new Menu("杂项", "morgmisc");
            foreach (var obj in ObjectManager.Get<Obj_AI_Hero>().Where(obj => obj.Team != Me.Team))
            {
                menuM.AddItem(new MenuItem("dobind" + obj.ChampionName, obj.ChampionName))
                    .SetValue(new StringList(new[] { "禁止 Q ", "普通 Q ", "自动 Q" }, 1));
            }

            _menu.AddSubMenu(menuM);

            _menu.AddItem(new MenuItem("combokey", "连招 (按键)")).SetValue(new KeyBind(32, KeyBindType.Press));
            _menu.AddItem(new MenuItem("harasskey", "骚扰 (按键)")).SetValue(new KeyBind('C', KeyBindType.Press));

            var messageM = new Menu("信息", "message");
            messageM.AddItem(new MenuItem("Sprite", "Kurisu-莫甘娜"));
            messageM.AddItem(new MenuItem("Hanhua", "汉化:花边下丶情未央"));
            messageM.AddItem(new MenuItem("qqqun", "QQ群:299606556"));
            _menu.AddSubMenu(messageM);

            _menu.AddItem(new MenuItem("XiaoXiongMei", "带上小胸妹无形装逼"));

            _menu.AddToMainMenu();


            Game.PrintChat("<font color=\"#1eff00\">甯朵笂灏忚兏濡硅閫间辅</font> - <font color=\"#00BFFF\">婕㈠寲By Huabian</font>");

            // events
            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnUpdate += Game_OnGameUpdate;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;

            try
            {
                Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception thrown KurisuMorgana: (BlackShield: {0})", e);
            }
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            if (!Me.IsValidTarget(300, false))
            {
                return;
            }

            CheckDamage(TargetSelector.GetTarget(_q.Range + 10, TargetSelector.DamageType.Magical));


            AutoCast(_menu.Item("useqdash").GetValue<bool>(),
                     _menu.Item("useqauto").GetValue<bool>(),
                     _menu.Item("usewauto").GetValue<bool>());

            if (_menu.Item("combokey").GetValue<KeyBind>().Active)
            {
                Combo(_menu.Item("useqcombo").GetValue<bool>(),
                      _menu.Item("usewcombo").GetValue<bool>(), 
                      _menu.Item("usercombo").GetValue<bool>());
            }

            if (_menu.Item("harasskey").GetValue<KeyBind>().Active)
            {
                Harass(_menu.Item("useharassq").GetValue<bool>(),
                       _menu.Item("useharassw").GetValue<bool>());
            }
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (Me.IsValidTarget(300, false))
            {
                var ticks = _menu.Item("calcw").GetValue<Slider>().Value;

                if (_menu.Item("drawq").GetValue<bool>())
                    Render.Circle.DrawCircle(Me.Position, _q.Range + 10, System.Drawing.Color.White, 3);
                if (_menu.Item("draww").GetValue<bool>())
                    Render.Circle.DrawCircle(Me.Position, _w.Range, System.Drawing.Color.White, 3);
                if (_menu.Item("drawe").GetValue<bool>())
                    Render.Circle.DrawCircle(Me.Position, 750f, System.Drawing.Color.White, 3);
                if (_menu.Item("drawr").GetValue<bool>())
                    Render.Circle.DrawCircle(Me.Position, _r.Range + 10, System.Drawing.Color.White, 3);

                var target = TargetSelector.GetTarget(_q.Range + 10, TargetSelector.DamageType.Magical);
                if (target.IsValidTarget(_q.Range + 10))
                {
                    if (_menu.Item("drawtarg").GetValue<bool>())
                    {
                        Render.Circle.DrawCircle(target.Position, target.BoundingRadius - 50, System.Drawing.Color.Yellow, 6);                       
                    }

                    if (_menu.Item("drawkill").GetValue<bool>())
                    {
                        var wts = Drawing.WorldToScreen(target.Position);
                        if (_ma*3 + _mi + _mq + _guise >= target.Health)
                            Drawing.DrawText(wts[0] - 20, wts[1] + 20, System.Drawing.Color.LimeGreen, "Q Kill!");
                        else if (_ma*3 + _mi + _mw * ticks + _guise >= target.Health)
                            Drawing.DrawText(wts[0] - 20, wts[1] + 20, System.Drawing.Color.LimeGreen, "W Kill!");
                        else if (_mq + _mw * ticks + _ma * 3 + _mi + _guise >= target.Health)
                            Drawing.DrawText(wts[0] - 20, wts[1] + 20, System.Drawing.Color.LimeGreen, "Q + W Kill!");
                        else if (_mq + _mw * ticks + _ma * 3 + _mi + _mr + _guise >= target.Health)
                            Drawing.DrawText(wts[0] - 20, wts[1] + 20, System.Drawing.Color.LimeGreen, "Q + R + W Kill!");
                        else if (_mq + _mw * ticks + _ma * 3 + _mr + _mi + _guise < target.Health)
                            Drawing.DrawText(wts[0] - 20, wts[1] + 20, System.Drawing.Color.LimeGreen, "Cant Kill");
                    }

                    if (_menu.Item("debugdmg").GetValue<bool>())
                    {
                        var wts = Drawing.WorldToScreen(target.Position);
                        Drawing.DrawText(wts[0] - 75, wts[1] + 40, System.Drawing.Color.Yellow,
                                "Combo Damage: " + (float)(_ma * 3 + _mq + _mw * ticks + _mi + _mr + _guise));
                    }
                }
            }
        }

        private static void Combo(bool useq, bool usew, bool user)
        {
            if (useq && _q.IsReady())
            {
                var qtarget = TargetSelector.GetTargetNoCollision(_q);
                if (qtarget.IsValidTarget(_q.Range + 10))
                {
                    var poutput = _q.GetPrediction(qtarget);
                    if (poutput.Hitchance >= (HitChance) _menu.Item("hitchanceq").GetValue<Slider>().Value + 2)
                    {
                        _q.Cast(poutput.CastPosition);
                    }
                }
            }

            if (usew && _w.IsReady())
            {             
                var wtarget = TargetSelector.GetTarget(_w.Range + 10, TargetSelector.DamageType.Magical);            
                if (wtarget.IsValidTarget(_w.Range))
                {
                    var poutput = _w.GetPrediction(wtarget);
                    if (poutput.Hitchance >= (HitChance)_menu.Item("hitchancew").GetValue<Slider>().Value + 2)
                    {
                        if (!_menu.Item("waitfor").GetValue<bool>() ||
                            _mw*_menu.Item("calcw").GetValue<Slider>().Value >= wtarget.Health)
                        {
                            _w.Cast(poutput.CastPosition);
                        }
                    }                  
                }
            }

            if (user && _r.IsReady())
            {
                var ticks = _menu.Item("calcw").GetValue<Slider>().Value;
                var rtarget = TargetSelector.GetTarget(_r.Range, TargetSelector.DamageType.Magical);
                if (rtarget.IsValidTarget(_r.Range) && _menu.Item("rkill").GetValue<bool>())
                {
                    if (_mr + _mq + _mw * ticks + _ma * 3 + _mi + _guise >= rtarget.Health)
                    {
                        if (rtarget.Health > _mr + _ma * 3 + _mw * 3 &&
                           !rtarget.IsZombie)
                        {
                            if (_e.IsReady()) _e.CastOnUnit(Me);
                            _r.Cast();
                        }
                    }

                    if (Me.CountEnemiesInRange(_r.Range) >= _menu.Item("rcount").GetValue<Slider>().Value)
                    {
                        if (_e.IsReady())
                            _e.CastOnUnit(Me);

                        _r.Cast();
                    }
                }
            }
        }

        private static void Harass(bool useq, bool usew)
        {
            if (useq && _q.IsReady())
            {
                var qtarget = TargetSelector.GetTargetNoCollision(_q);
                if (qtarget.IsValidTarget(_q.Range - 300))
                {
                    var poutput = _q.GetPrediction(qtarget);
                    if (poutput.Hitchance >= (HitChance)_menu.Item("hitchanceq").GetValue<Slider>().Value + 2)
                    {
                        if ((int)(Me.Mana / Me.MaxMana * 100) >= _menu.Item("harassmana").GetValue<Slider>().Value)
                            _q.Cast(poutput.CastPosition);
                    }
                }
            }

            if (usew && _w.IsReady())
            {
                var wtarget = TargetSelector.GetTarget(_w.Range + 10, TargetSelector.DamageType.Magical);
                if (wtarget.IsValidTarget(_w.Range))
                {
                    var poutput = _w.GetPrediction(wtarget);
                    if (poutput.Hitchance >= (HitChance)_menu.Item("hitchancew").GetValue<Slider>().Value + 2)
                    {
                        if ((int)(Me.Mana / Me.MaxMana * 100) >= _menu.Item("harassmana").GetValue<Slider>().Value)
                            _w.Cast(poutput.CastPosition);
                    }
                }           
            }
        }

        private static void AutoCast(bool dashing, bool immobile, bool soil)
        {
            if (_q.IsReady())
            {
                var itarget =
                    ObjectManager.Get<Obj_AI_Hero>()
                        .FirstOrDefault(h => h.IsEnemy && h.Distance(Me.ServerPosition, true) <= _q.RangeSqr);

                if (itarget.IsValidTarget(_q.Range))
                {
                    if (dashing && _menu.Item("dobind" + itarget.ChampionName).GetValue<StringList>().SelectedIndex == 2)
                        _q.CastIfHitchanceEquals(itarget, HitChance.Dashing);

                    if (immobile && _menu.Item("dobind" + itarget.ChampionName).GetValue<StringList>().SelectedIndex == 2)
                        _q.CastIfHitchanceEquals(itarget, HitChance.Immobile);
                }
            }

            if (_w.IsReady() && soil)
            {
                var itarget =
                    ObjectManager.Get<Obj_AI_Hero>()
                        .FirstOrDefault(h => h.IsEnemy && h.Distance(Me.ServerPosition, true) <= _w.RangeSqr);

                if (itarget.IsValidTarget(_w.Range))
                    _w.CastIfHitchanceEquals(itarget, HitChance.Immobile);          
            }

            if (_r.IsReady())
            {
                if (Me.CountEnemiesInRange(_r.Range) >= _menu.Item("useautor").GetValue<Slider>().Value)
                {
                    if (_e.IsReady())
                        _e.CastOnUnit(Me);
                    _r.Cast();
                }           
            }
        }  

        private static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (gapcloser.Sender.IsValidTarget(_q.Range + 10))
            {
                if (_menu.Item("useqanti").GetValue<bool>())
                {
                    var poutput = _q.GetPrediction(gapcloser.Sender);
                    if (poutput.Hitchance >= HitChance.Low)
                    {
                        _q.Cast(poutput.CastPosition);
                    }
                }
            }
        }

        private static float _mq, _mw, _mr;
        private static float _ma, _mi, _guise;
        private static void CheckDamage(Obj_AI_Base target)
        {
            if (target == null)
            {
                return;
            }

            var qready = Me.Spellbook.CanUseSpell(SpellSlot.Q) == SpellState.Ready;
            var wready = Me.Spellbook.CanUseSpell(SpellSlot.W) == SpellState.Ready;
            var rready = Me.Spellbook.CanUseSpell(SpellSlot.R) == SpellState.Ready;
            var iready = Me.Spellbook.CanUseSpell(Me.GetSpellSlot("summonerdot")) == SpellState.Ready;

            _ma = (float) Me.GetAutoAttackDamage(target);
            _mq = (float) (qready ? Me.GetSpellDamage(target, SpellSlot.Q) : 0);
            _mw = (float) (wready ? Me.GetSpellDamage(target, SpellSlot.W) : 0);
            _mr = (float) (rready ? Me.GetSpellDamage(target, SpellSlot.R) : 0);
            _mi = (float) (iready ? Me.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite) : 0);

            _guise = (float) (Items.HasItem(3151)
                ? Me.GetItemDamage(target, Damage.DamageItems.LiandrysTorment)
                : 0);
        }

        internal static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.Type != Me.Type || !_e.IsReady() || !sender.IsEnemy) 
                return;

            var attacker = ObjectManager.Get<Obj_AI_Hero>().First(x => x.NetworkId == sender.NetworkId);
            foreach (var ally in ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsValidTarget(_e.Range, false)))
            {
                var detectRange = ally.ServerPosition + (args.End - ally.ServerPosition).Normalized() * ally.Distance(args.End);
                if (detectRange.Distance(ally.ServerPosition) > ally.AttackRange - ally.BoundingRadius)
                    continue;

                // no linq cus i say :p
                foreach (var lib in KurisuLib.CCList)
                {
                    if (lib.HeroName == attacker.ChampionName && lib.Slot == attacker.GetSpellSlot(args.SData.Name))
                    {
                        var timeTillHit = lib.Delay +
                                          (int)(1000*attacker.Distance(ally.ServerPosition/args.SData.MissileSpeed));

                        if (_menu.Item(lib.SDataName + "on").GetValue<bool>() && _menu.Item("useon" + ally.ChampionName).GetValue<bool>())
                        {
                            if (_menu.Item(lib.SDataName + "wait").GetValue<bool>())
                            {
                                Utility.DelayAction.Add(timeTillHit, () => _e.CastOnUnit(ally));
                            }

                            else
                            {
                                _e.CastOnUnit(ally);
                            }
                        }
                    }
                }
            }
        }
    }
}
