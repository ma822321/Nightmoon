using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace PerplexedLucian
{
    class SpellManager
    {
        private static Obj_AI_Hero Player = ObjectManager.Player;

        private static Spell _Q, _Q2, _W, _E, _R;

        public static Spell Q { get { return _Q; } }
        public static Spell Q2 { get { return _Q2; } }
        public static Spell W { get { return _W; } }
        public static Spell E { get { return _E; } }
        public static Spell R { get { return _R; } }

        public static float LastCastTime = 0f;

        public static void Initialize()
        {
            _Q = new Spell(SpellSlot.Q, 675);
            _Q.SetTargetted(0.5f, float.MaxValue);

            _Q2 = new Spell(SpellSlot.Q, 1100);
            _Q2.SetSkillshot(0.5f, 5f, float.MaxValue, true, SkillshotType.SkillshotLine);

            _W = new Spell(SpellSlot.W, 1000);
            _W.SetSkillshot(0.3f, 80f, 1600, true, SkillshotType.SkillshotLine);

            _E = new Spell(SpellSlot.E, 475);
            _E.SetSkillshot(0.25f, 0, float.MaxValue, false, SkillshotType.SkillshotLine);

            _R = new Spell(SpellSlot.R, 1400);
            _R.SetSkillshot(0.01f, 110, 2800f, true, SkillshotType.SkillshotLine);
        }

        public static void CastSpell(Spell spell, Obj_AI_Base target, HitChance hitChance, bool packetCast)
        {
            Obj_AI_Hero Player = ObjectManager.Player;
            if (target.IsValidTarget(spell.Range) && spell.GetPrediction(target).Hitchance >= hitChance)
            {
                spell.Cast(target, packetCast);
                LastCastTime = Environment.TickCount;
                Orbwalking.ResetAutoAttackTimer();
            }
        }

        public static void CastSpell(Spell spell, Vector3 position, bool packetCast)
        {
            spell.Cast(position, packetCast);
            LastCastTime = Environment.TickCount;
            Orbwalking.ResetAutoAttackTimer();
        }

        public static void CastSpell(Spell spell, Obj_AI_Base target, bool packetCast)
        {
            if (target.IsValidTarget(spell.Range))
            {
                spell.Cast(target, packetCast);
                LastCastTime = Environment.TickCount;
                Orbwalking.ResetAutoAttackTimer();
            }
        }

        public static void UseHealIfInDanger(double incomingDmg)
        {
            if (Config.UseHeal)
            {
                int healthToUse = (int)(Player.MaxHealth / 100) * Config.HealPct;
                if ((Player.Health - incomingDmg) <= healthToUse && !Player.InFountain())
                {
                    SpellSlot healSlot = Utility.GetSpellSlot(Player, "SummonerHeal");
                    if(healSlot != SpellSlot.Unknown)
                        Player.Spellbook.CastSpell(healSlot);
                }
            }
        }

        internal static void IgniteIfPossible()
        {
            if (Config.UseIgnite)
            {
                SpellSlot igniteSlot = Utility.GetSpellSlot(Player, "SummonerDot");
                if (igniteSlot != SpellSlot.Unknown)
                {
                    var targets = ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsValidTarget(600) && hero.IsEnemy);
                    foreach (var target in targets)
                    {
                        if (Config.IgniteMode == "Combo" && Config.Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
                            Player.Spellbook.CastSpell(igniteSlot, target);
                        else
                        {
                            double igniteDamage = Damage.GetSummonerSpellDamage(Player, target, Damage.SummonerSpell.Ignite);
                            if (target.Health < igniteDamage)
                                Player.Spellbook.CastSpell(igniteSlot, target);
                        }
                    }
                }
            }
        }
    }
}
