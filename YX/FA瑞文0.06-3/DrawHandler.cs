using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;

namespace FuckingAwesomeRiven
{
    class DrawHandler
    {
        public static void Draw(EventArgs args)
        {
            if (MenuHandler.Config.Item("drawCirclesforTest").GetValue<bool>())
            {
                JumpHandler.drawCircles();
            }
            var drawQ = MenuHandler.Config.Item("DQ").GetValue<Circle>();
            var drawW = MenuHandler.Config.Item("DW").GetValue<Circle>();
            var drawE = MenuHandler.Config.Item("DE").GetValue<Circle>();
            var drawR = MenuHandler.Config.Item("DR").GetValue<Circle>();
            var drawBC = MenuHandler.Config.Item("DBC").GetValue<Circle>();
            if (drawQ.Active)
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, SpellHandler.QRange, drawQ.Color);
            }
            if (drawW.Active)
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, SpellHandler.WRange, drawW.Color);
            }
            if (drawE.Active)
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, SpellHandler._spells[SpellSlot.E].Range, drawE.Color);
            }
            if (drawR.Active)
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, 900, drawR.Color);
            }
            if (drawBC.Active)
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, 400 + SpellHandler._spells[SpellSlot.E].Range, drawR.Color);
            }

            if (!MenuHandler.Config.Item("debug").GetValue<bool>())
                return;
            Drawing.DrawText(100, 100 + (20 * 1), Color.White, "鍙互 Q" + ": " + CheckHandler.CanQ);
            Drawing.DrawText(100, 100 + (20 * 2), Color.White, "鍙互 W" + ": " + CheckHandler.CanW);
            Drawing.DrawText(100, 100 + (20 * 3), Color.White, "鍙互 E" + ": " + CheckHandler.CanE);
            Drawing.DrawText(100, 100 + (20 * 4), Color.White, "鍙互 R" + ": " + CheckHandler.CanR);
            Drawing.DrawText(100, 100 + (20 * 5), Color.White, "鍙互 AA" + ": " + CheckHandler.CanAa);
            Drawing.DrawText(100, 100 + (20 * 6), Color.White, "鍙互 绉诲姩" + ": " + CheckHandler.CanMove);
            Drawing.DrawText(100, 100 + (20 * 7), Color.White, "鍙互 SR" + ": " + CheckHandler.CanSr);
            Drawing.DrawText(100, 100 + (20 * 8), Color.White, "閲婃斁 Q" + ": " + CheckHandler.MidQ);
            Drawing.DrawText(100, 100 + (20 * 9), Color.White, "閲婃斁 W" + ": " + CheckHandler.MidW);
            Drawing.DrawText(100, 100 + (20 * 10), Color.White, "閲婃斁 E" + ": " + CheckHandler.MidE);
            Drawing.DrawText(100, 100 + (20 * 11), Color.White, "閲婃斁 AA" + ": " + CheckHandler.MidAa);
            Drawing.DrawText(100, 100 + (20 * 12), Color.White, "鏃堕挓" + ": " + Environment.TickCount);
            Drawing.DrawText(100, 100 + (20 * 13), Color.White, "缁撴潫Q" + ": " + CheckHandler.LastQ);
            Drawing.DrawText(100, 100 + (20 * 14), Color.White, "缁撴潫AA" + ": " + CheckHandler.LastAa);
            Drawing.DrawText(100, 100 + (20 * 15), Color.White, "缁撴潫E" + ": " + CheckHandler.LastE);
        }
    }
}
