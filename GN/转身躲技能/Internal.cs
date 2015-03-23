#region

using System;

#endregion

namespace Advanced_Turn_Around
{
    internal class Internal
    {
        public static void AddChampions()
        {
            Variable.ExistingChampions.Add(
                new ChampionInfo
                {
                    CharName = "卡西奥佩娜",
                    Key = "CassiopeiaPetrifyingGaze",
                    Range = 750,
                    SpellName = "R技能",
                    Movement = Variable.MovementDirection.Backward
                });

            Variable.ExistingChampions.Add(
                new ChampionInfo
                {
                    CharName = "哨兵之殇",
                    Key = "TwoShivPoison",
                    Range = 625,
                    SpellName = "E技能",
                    Movement = Variable.MovementDirection.Forward
                });

            Variable.ExistingChampions.Add(
                new ChampionInfo
                {
                    CharName = "泰达米尔",
                    Key = "MockingShout",
                    Range = 850,
                    SpellName = "W技能",
                    Movement = Variable.MovementDirection.Forward
                });
        }

        public static int MoveTo(Variable.MovementDirection direction)
        {
            switch (direction)
            {
                case Variable.MovementDirection.Forward:
                    return 100;
                case Variable.MovementDirection.Backward:
                    return -100;
                default:
                    throw new ArgumentOutOfRangeException("direction");
            }
        }

        public class ChampionInfo
        {
            public string CharName { get; set; }
            public string Key { get; set; }
            public float Range { get; set; }
            public string SpellName { get; set; }
            public Variable.MovementDirection Movement { get; set; }
        }
    }
}