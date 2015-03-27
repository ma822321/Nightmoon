using System;
using System.Reflection;
using KaiHelper.Activator;
using KaiHelper.Misc;
using KaiHelper.Timer;
using KaiHelper.Tracker;
using LeagueSharp;
using LeagueSharp.Common;

namespace KaiHelper
{
    internal class Program
    {
        public static Menu MainMenu;

        private static void Main(string[] args)
        {
            MainMenu = new Menu("花边-Kai助手", "KaiHelp", true);
            //Menu Tracker = MainMenu.AddSubMenu(new Menu("Tracker", "Tracker"));
            new SkillBar(MainMenu);
            new JungleTimer(MainMenu);
            new GankDetector(MainMenu);
            //new LastPosition(MainMenu);
            new WayPoint(MainMenu);
            new WardDetector(MainMenu);
            new HealthTurret(MainMenu);
            //Menu Timer = MainMenu.AddSubMenu(new Menu("Timer", "Timer"));
            //Menu Range = MainMenu.AddSubMenu(new Menu("Range", "Range"));
            new Vision(MainMenu);
            //Menu ActivatorMenu = MainMenu.AddSubMenu(new Menu("Activator", "Activator"));
            new AutoPot(MainMenu);
            MainMenu.AddToMainMenu();
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            bool hasUpdate = Helper.HasNewVersion(Assembly.GetExecutingAssembly().GetName().Name);
            if (hasUpdate)
            {
                Game.PrintChat(
                    "<font color = \"#ff002b\">鎵綡uaBian绱㈠彇鏈€鏂扮増鐨凨ai鍔╂墜婕㈠寲鐗堟湰锛佹鐗堟湰宸查亷鏈燂紒</font>");
            }
            Game.PrintChat("<font color = \"#00FF2B\">HuaBian婕㈠寲 Kai鍔╂墜</font> - <font color = \"#FD00FF\">鍔犺級鎴愬姛</font>");
            Game.PrintChat("<font color = \"#0092FF\">Feel free to donate via Paypal to:</font> <font color = \"#F0FF00\">ntanphat2406@gmail.com</font>");
        }
    }
}