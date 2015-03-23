/// Tools For L# NewBie Level :( Developer BR
/// NewBie :( Developer BR

#region Using
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Color = System.Drawing.Color;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
#endregion
namespace GNomeProject
{
    class Program
    {
        public static Obj_AI_Hero _Player { get { return ObjectManager.Player; } }
        public static List<string> _Hero = new List<string>();
        static void Main(string[] args)
        {
            LoadHeroList();
            if (_Hero.Contains(_Player.BaseSkinName))
            {
                if (_Player.BaseSkinName == "Annie")
                {
                    CustomEvents.Game.OnGameLoad += GNomeProject.Hero.Annie.Game_OnGameLoad;
                }
            }
            else
            {
                return;
            }
           
        }

       
        static void LoadHeroList()
        {
            _Hero.Add("Annie");
        }
    }
}
