﻿using DZAIO.Utility.Helpers;
using LeagueSharp;
using LeagueSharp.Common;

namespace DZAIO.Utility.DamagePrediction
{
    internal class DamagePrediction
    {
        public delegate void OnKillableDelegate(Obj_AI_Hero sender,Obj_AI_Hero target,SpellData sData);
        public static event OnKillableDelegate OnSpellWillKill;

        static DamagePrediction()
        {
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
        }

        private static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!(sender is Obj_AI_Hero) || !(args.Target is Obj_AI_Hero))
                return;
            var senderH = sender as Obj_AI_Hero;
            var targetH = args.Target as Obj_AI_Hero;
            var damage = Orbwalking.IsAutoAttack(args.SData.Name)?sender.GetAutoAttackDamage(targetH):GetDamage(senderH,targetH, senderH.GetSpellSlot(args.SData.Name));

            //DebugHelper.AddEntry("Damage to "+targetH.ChampionName+" from spell "+args.SData.Name+" -> "+senderH.ChampionName+" ("+senderH.GetSpellSlot(args.SData.Name)+")",damage.ToString());

            if (damage > targetH.Health + 20)
            {
                if (OnSpellWillKill != null)
                {
                    OnSpellWillKill(senderH, targetH,args.SData);
                }
            }
        }

        static float GetDamage(Obj_AI_Hero hero,Obj_AI_Hero target,SpellSlot slot)
        {
            return (float)hero.GetSpellDamage(target, slot);
        }
    }
}
