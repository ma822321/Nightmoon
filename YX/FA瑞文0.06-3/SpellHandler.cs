﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using LeagueSharp.Common.Data;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using CH = FuckingAwesomeRiven.CheckHandler;

namespace FuckingAwesomeRiven
{
    public static class SpellHandler
    {
        public static Obj_AI_Hero Player { get { return ObjectManager.Player; } }

        public static Dictionary<SpellSlot, Spell> _spells = new Dictionary<SpellSlot, Spell>()
        {
            {SpellSlot.Q, new Spell(SpellSlot.Q, 300)},
            {SpellSlot.W, new Spell(SpellSlot.W, 250)},
            {SpellSlot.E, new Spell(SpellSlot.E, 325)},
            {SpellSlot.R, new Spell(SpellSlot.R, 900)}
        };

        public enum summonerSpell {Flash, Ignite, Smite}

        public static Dictionary<summonerSpell, SpellSlot> SummonerDictionary =
            new Dictionary<summonerSpell, SpellSlot>()
            {
                { summonerSpell.Flash, Player.GetSpellSlot("SummonerFlash") },
                { summonerSpell.Ignite, Player.GetSpellSlot("SummonerDot") },
            };

        public static int QRange { get { return CheckHandler.RState ? 325 : 300; } }
        public static int WRange { get { return CheckHandler.RState ? 270 : 250; } }

        public static void CastQ(Obj_AI_Base target = null)
        {
            if (!_spells[SpellSlot.Q].IsReady()) return;
            if (target != null)
            {
                _spells[SpellSlot.Q].Cast(target.Position, true);
            }
            else
            {
                _spells[SpellSlot.Q].Cast(Game.CursorPos, true);
            }
        }

        public static void CastW(Obj_AI_Hero target = null)
        {
            if (!_spells[SpellSlot.W].IsReady()) return;
            if (target.IsValidTarget(WRange))
            {
                castItems(target);
               _spells[SpellSlot.W].Cast();
            }
            if (target == null)
            {
                _spells[SpellSlot.W].Cast();
            }
        }

        public static void CastE(Vector3 pos)
        {
            if (!_spells[SpellSlot.E].IsReady())
                return;
            _spells[SpellSlot.E].Cast(pos);
        }

        public static void CastR()
        {
            _spells[SpellSlot.R].Cast();
        }

        public static void CastR2(Obj_AI_Base target)
        {
            var r2 = new Spell(SpellSlot.R, 900);
            r2.SetSkillshot(0.25f, 45, 1200, false, SkillshotType.SkillshotCone);
            r2.Cast(target);
        }

        public static void castFlash(Vector3 pos)
        {
            if (!SummonerDictionary[summonerSpell.Flash].IsReady())
                return;
            Player.Spellbook.CastSpell(SummonerDictionary[summonerSpell.Flash], pos);
        }

        public static void castIgnite(Obj_AI_Hero target)
        {
            if (!SummonerDictionary[summonerSpell.Ignite].IsReady())
                return;
            Player.Spellbook.CastSpell(SummonerDictionary[summonerSpell.Ignite], target);
        }


        public static void castItems(Obj_AI_Base target)
        {
            if (!target.IsValidTarget())
                return;
            if (!target.IsValid<Obj_AI_Hero>() && target.IsValidTarget(300))
            {
                if (ItemData.Tiamat_Melee_Only.GetItem().IsReady())
                    ItemData.Tiamat_Melee_Only.GetItem().Cast();
                if (ItemData.Ravenous_Hydra_Melee_Only.GetItem().IsReady())
                    ItemData.Ravenous_Hydra_Melee_Only.GetItem().Cast();
            }
            else
            {
                if (target.IsValidTarget(300))
                {
                    if (ItemData.Tiamat_Melee_Only.GetItem().IsReady())
                        ItemData.Tiamat_Melee_Only.GetItem().Cast();
                    if (ItemData.Ravenous_Hydra_Melee_Only.GetItem().IsReady())
                        ItemData.Ravenous_Hydra_Melee_Only.GetItem().Cast();
                }
                if (ItemData.Youmuus_Ghostblade.GetItem().IsReady())
                    ItemData.Youmuus_Ghostblade.GetItem().Cast();
            }
        }

        public static void animCancel(Obj_AI_Base target)
        {
                if(!CheckHandler.CanMove) return;
                var pos2 = Player.Position.Extend(Game.CursorPos, 100);
                if (target.IsValidTarget()) pos2 = Player.Position.Extend(target.Position, 100);
                Player.IssueOrder(GameObjectOrder.MoveTo, pos2);
        }


        public static void Orbwalk(Obj_AI_Base target = null)
        {
            if (MenuHandler.Config.Item("normalCombo").GetValue<KeyBind>().Active)
            {
                MenuHandler.Orbwalker.SetAttack(false);
                MenuHandler.Orbwalker.SetMovement(false);
            }
            else
            {
                MenuHandler.Orbwalker.SetMovement(true);
                MenuHandler.Orbwalker.SetAttack(true);
            }
            if (CH.CanMove)
            {
                Player.IssueOrder(GameObjectOrder.MoveTo, (target.IsValidTarget(600) && MenuHandler.Config.Item("magnet").GetValue<bool>() && !(target is Obj_AI_Minion) ? Player.Position.Extend(target.Position, Player.Distance(target) - 20) : Game.CursorPos));
                MenuHandler.Orbwalker.SetMovement(true);
            }

            if (target.IsValidTarget(Orbwalking.GetRealAutoAttackRange(Player)) && CH.CanAa)
            {
                MenuHandler.Orbwalker.SetAttack(true);
                CH.CanMove = false;
                Player.IssueOrder(GameObjectOrder.AttackUnit, target);
                CH.CanQ = false;
                CH.CanW = false;
                CH.CanE = false;
                CH.CanSr = false;
                CH.LastAa = Environment.TickCount;
            }
        }
    }
}
