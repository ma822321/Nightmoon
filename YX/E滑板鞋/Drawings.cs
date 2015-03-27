﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using Color = System.Drawing.Color;

namespace ElKalista
{
    internal class Drawings
    {

        public static void Drawing_OnDraw(EventArgs args)
        {
            var drawOff = ElKalistaMenu._menu.Item("ElKalista.Draw.off").GetValue<bool>();
            var drawQ = ElKalistaMenu._menu.Item("ElKalista.Draw.Q").GetValue<Circle>();
            var drawW = ElKalistaMenu._menu.Item("ElKalista.Draw.W").GetValue<Circle>();
            var drawE = ElKalistaMenu._menu.Item("ElKalista.Draw.E").GetValue<Circle>();
            var drawR = ElKalistaMenu._menu.Item("ElKalista.Draw.R").GetValue<Circle>();
            var drawText = ElKalistaMenu._menu.Item("ElKalista.Draw.Text").GetValue<bool>();
            var rBool = ElKalistaMenu._menu.Item("ElKalista.AutoHarass").GetValue<KeyBind>().Active;

            if (Kalista.Player.IsDead)
                return;

            if (drawOff)
                return;

            var playerPos = Drawing.WorldToScreen(ObjectManager.Player.Position);

            if (drawQ.Active)
                if (Kalista.spells[Spells.Q].Level > 0)
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, Kalista.spells[Spells.Q].Range, Kalista.spells[Spells.Q].IsReady() ? Color.Green : Color.Red);

            if (drawW.Active)
                if (Kalista.spells[Spells.W].Level > 0)
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, Kalista.spells[Spells.W].Range, Kalista.spells[Spells.W].IsReady() ? Color.Green : Color.Red);

            if (drawE.Active)
                if (Kalista.spells[Spells.E].Level > 0)
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, Kalista.spells[Spells.E].Range, Kalista.spells[Spells.E].IsReady() ? Color.Green : Color.Red);

            if (drawR.Active)
                if (Kalista.spells[Spells.R].Level > 0)
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, Kalista.spells[Spells.R].Range, Kalista.spells[Spells.R].IsReady() ? Color.Green : Color.Red);

            if (drawText)
                Drawing.DrawText(playerPos.X - 70, playerPos.Y + 40, (rBool ? Color.Green : Color.Red), "{0}", (rBool ? "Auto harass Enabled" : "Auto harass Disabled"));
        }
    }
}