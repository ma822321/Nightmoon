using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;
using System.Reflection;
namespace PerplexedLucian
{
    class Program
    {
        static Obj_AI_Hero Player = ObjectManager.Player;
        static System.Version Version = Assembly.GetExecutingAssembly().GetName().Version;

        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        static void Game_OnGameLoad(EventArgs args)
        {
            if (Player.ChampionName != "Lucian")
                return;

            SpellManager.Initialize();
            ItemManager.Initialize();
            Config.Initialize();

            if (Updater.Outdated())
            {
                Game.PrintChat("<font color=\"#ff0000\">Perplexed Lucian is outdated! Please update to {0}!</font>", Updater.GetLatestVersion());
                return;
            }

            CustomDamageIndicator.Initialize(DamageCalc.GetDrawDamage); //Credits to Hellsing for this! Borrowed it from his Kalista assembly.

            Game.OnGameUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Orbwalking.AfterAttack += Orbwalking_AfterAttack;

            Game.PrintChat("<font color=\"#ff3300\">鑺辫竟姹夊寲 Perplexed 鍗㈤敗瀹変辅({0})</font> <font color=\"#ffffff\">鍔犺浇鎴愬姛!</font>", Version);
        }

        static void Orbwalking_AfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            if (HasPassive)
                SpellManager.LastCastTime = 0;
        }

        static void Game_OnGameUpdate(EventArgs args)
        {
            SpellManager.UseHealIfInDanger(0);
            SpellManager.IgniteIfPossible();
            ItemManager.CleanseCC();
            if (Player.IsChannelingImportantSpell())
                return;
            switch (Config.Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    Combo();
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    Harass();
                    break;
            }
        }

        private static void Combo()
        {
            if (HasPassive && Config.CheckPassive)
                return;
            ItemManager.UseOffensiveItems();
            if (Config.ComboQ && SpellManager.Q.IsReady())
            {
                var target = TargetSelector.GetTarget(SpellManager.Q.Range, TargetSelector.DamageType.Physical);
                if (target != null)
                {
                    SpellManager.CastSpell(SpellManager.Q, target, Config.UsePackets);
                    if (Config.CheckPassive)
                        return;
                }
                target = TargetSelector.GetTarget(SpellManager.Q2.Range, TargetSelector.DamageType.Physical);
                var collisions = SpellManager.Q2.GetCollision(Player.ServerPosition.To2D(), new List<Vector2>() { target.ServerPosition.To2D() });
                foreach (Obj_AI_Base collision in collisions)
                    SpellManager.CastSpell(SpellManager.Q, collision, Config.UsePackets);
            }
            if (Config.ComboW && SpellManager.W.IsReady())
            {
                var target = TargetSelector.GetTarget(SpellManager.W.Range, TargetSelector.DamageType.Physical);
                if (target != null)
                {
                    PredictionOutput prediction = SpellManager.W.GetPrediction(target);
                    switch (prediction.Hitchance)
                    {
                        case HitChance.High:
                        case HitChance.VeryHigh:
                            SpellManager.CastSpell(SpellManager.W, target, Config.UsePackets);
                            if (Config.CheckPassive)
                                return;
                            break;
                        case HitChance.Collision:
                            var collisions = prediction.CollisionObjects.Where(collision => collision.Distance(target) <= SpellManager.W.Width).ToList();
                            if (collisions.Count > 0)
                            {
                                SpellManager.CastSpell(SpellManager.W, collisions[0], Config.UsePackets);
                                if (Config.CheckPassive)
                                    return;
                            }
                            break;
                    }
                }
            }
            if (Config.ComboE && SpellManager.E.IsReady())
            {
                float range = Player.AttackRange + SpellManager.E.Range;
                var target = TargetSelector.GetTarget(range, TargetSelector.DamageType.Physical);
                if (target != null && target.IsValidTarget(range))
                {
                    if (Config.EIntoTurret || (!Config.EIntoTurret && !target.UnderTurret(true)))
                    {
                        SpellManager.CastSpell(SpellManager.E, Game.CursorPos, Config.UsePackets);
                        if (Config.CheckPassive)
                            return;
                    }
                }
            }

            if (Config.ComboR && SpellManager.R.IsReady())
            {
                var target = TargetSelector.GetTarget(SpellManager.R.Range, TargetSelector.DamageType.Physical);
                if (target != null)
                {
                    if (target.Health <= DamageCalc.GetUltDamage(target))
                    {
                        SpellManager.CastSpell(SpellManager.R, target, HitChance.High, Config.UsePackets);
                        if (Config.CheckPassive)
                            return;
                    }
                }
            }
        }

        static void Harass()
        {
            if (HasPassive && Config.CheckPassive)
                return;
            if (Config.HarassQ && SpellManager.Q.IsReady())
            {
                var target = TargetSelector.GetTarget(SpellManager.Q.Range, TargetSelector.DamageType.Physical);
                if (target != null)
                {
                    SpellManager.CastSpell(SpellManager.Q, target, Config.UsePackets);
                    if (Config.CheckPassive)
                        return;
                }
                target = TargetSelector.GetTarget(SpellManager.Q2.Range, TargetSelector.DamageType.Physical);
                var collisions = SpellManager.Q2.GetCollision(Player.ServerPosition.To2D(), new List<Vector2>() { target.ServerPosition.To2D() });
                foreach (Obj_AI_Base collision in collisions)
                    SpellManager.CastSpell(SpellManager.Q, collision, Config.UsePackets);
            }
        }

        static void Drawing_OnDraw(EventArgs args)
        {
            if (Config.DrawQ.Active)
                Render.Circle.DrawCircle(Player.Position, SpellManager.Q.Range, Config.DrawQ.Color);
            if (Config.DrawQ2.Active)
                Render.Circle.DrawCircle(Player.Position, SpellManager.Q2.Range, Config.DrawQ2.Color);
            if (Config.DrawW.Active)
                Render.Circle.DrawCircle(Player.Position, SpellManager.W.Range, Config.DrawW.Color);
            if (Config.DrawE.Active)
                Render.Circle.DrawCircle(Player.Position, SpellManager.E.Range, Config.DrawE.Color);
            if (Config.DrawR.Active)
                Render.Circle.DrawCircle(Player.Position, SpellManager.R.Range, Config.DrawR.Color);
        }

        private static bool HasPassive
        {
            get
            {
                if (Environment.TickCount - SpellManager.LastCastTime < 3000)
                    return true;
                else if (Player.HasLucianPassive())
                    return true;
                else
                    return false;
            }
        }
    }
}
