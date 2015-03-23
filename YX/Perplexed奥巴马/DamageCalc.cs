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
    class DamageCalc
    {
        private static Obj_AI_Hero Player = ObjectManager.Player;

        public static float GetComboDamage(Obj_AI_Hero target)
        {
            double dmg = GetAADamage(target);

            if (SpellManager.Q.IsReady())
                dmg += Player.GetSpellDamage(target, SpellSlot.Q);
            if (SpellManager.W.IsReady())
                dmg += Player.GetSpellDamage(target, SpellSlot.W);

            dmg += GetUltDamage(target);

            return (float)dmg;
        }

        public static float GetDrawDamage(Obj_AI_Hero target)
        {
            double dmg = Config.DrawAADmg ? GetAADamage(target) : 0;

            if (SpellManager.Q.IsReady() && Config.DrawQDmg)
                dmg += Player.GetSpellDamage(target, SpellSlot.Q);
            if (SpellManager.W.IsReady() && Config.DrawWDmg)
                dmg += Player.GetSpellDamage(target, SpellSlot.W);

            dmg += Config.DrawRDmg ? GetUltDamage(target) : 0;

            return (float)dmg;
        }

        public static float GetAADamage(Obj_AI_Hero target)
        {
            double dmg = Player.GetAutoAttackDamage(target);
            if (Player.HasLucianPassive())
            {
                float modifier = 0.3f;
                if (Player.Level > 6 && Player.Level < 13)
                    modifier = 0.4f;
                else
                    modifier = 0.5f;
                dmg += Player.GetAutoAttackDamage(target) * modifier;
            }
            return (float)dmg;
        }

        public static float GetUltDamage(Obj_AI_Hero target)
        {
            Spell R = SpellManager.R;
            if (!R.IsReady())
                return 0f;
            double ultDamage = Player.GetSpellDamage(target, SpellSlot.R);
            float attackSpeed = Player.AttackSpeedMod;
            int ultLevel = Player.GetSpell(SpellSlot.R).Level;
            double shotsPerLevel = ultLevel == 1 ? 7.5 : ultLevel == 2 ? 9 : 10.5;
            int shots = (int) (7.5 + (shotsPerLevel * attackSpeed));
            return (float) ultDamage * shots;
        }
    }
}
