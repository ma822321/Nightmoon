using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;

namespace PerplexedLucian
{
    static class Extensions
    {
        public static bool HasLucianPassive(this Obj_AI_Hero hero)
        {
            foreach (BuffInstance buff in hero.Buffs)
            {
                if (buff.Name.ToLower() == "lucianpassivebuff")
                    return true;
            }
            return false;
        }
    }
}
