
#region
using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
#endregion

namespace MasterYiQx
{
    internal class Program
    {
        public const string ChampionName = "MasterYi";
        private static readonly Obj_AI_Hero Player = ObjectManager.Player;

        //Orbwalker instance
        public static Orbwalking.Orbwalker Orbwalker;

        //Spells
        public static List<Spell> SpellList = new List<Spell>();
        public static Spell Q;
        public static Spell E;
        public static Spell W;
        public static Spell R;

        private static readonly SpellSlot IgniteSlot = Player.GetSpellSlot("SummonerDot");

        public static Items.Item Tiamat = new Items.Item(3077, 375);
        public static Items.Item Hydra = new Items.Item(3074, 375);

        public static int DelayTick = 0;
        //Menu
        public static Menu Config;
        public static Menu MenuExtras;
        public static Menu MenuTargetedItems;
        public static Menu MenuNonTargetedItems;

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            if (Player.BaseSkinName != "MasterYi")
                return;
            if (Player.IsDead)
                return;

            Q = new Spell(SpellSlot.Q, 600f);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E, 200f);
            R = new Spell(SpellSlot.R);

            Q.SetTargetted(0.50f, 75f);

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);


            //Create the menu
            Config = new Menu("花边汉化-xQx易大师", "MasterYi", true);

            var targetSelectorMenu = new Menu("目标 选择", "Target Selector");
            TargetSelector.AddToMenu(targetSelectorMenu);
            Config.AddSubMenu(targetSelectorMenu);

            new AssassinManager();
            Config.AddSubMenu(new Menu("走 砍", "Orbwalking"));
            Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalking"));
            Orbwalker.SetAttack(true);

            // Combo
            Config.AddSubMenu(new Menu("连 招", "Combo"));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseQCombo", "使用 Q").SetValue(true));
            Config.SubMenu("Combo")
                .AddItem(new MenuItem("UseQComboDontUnderTurret", "禁止塔下 Q").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseECombo", "使用 E").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseRCombo", "使用 R").SetValue(true));
            Config.SubMenu("Combo")
                .AddItem(
                    new MenuItem("ComboActive", "连招 按键!").SetValue(
                        new KeyBind(Config.Item("Orbwalk").GetValue<KeyBind>().Key, KeyBindType.Press)));

            // Harass
            Config.AddSubMenu(new Menu("骚 扰", "Harass"));
            Config.SubMenu("Harass").AddItem(new MenuItem("UseQHarass", "使用 Q").SetValue(true));
            Config.SubMenu("Harass")
                .AddItem(new MenuItem("UseQHarassDontUnderTurret", "禁止塔下 Q").SetValue(true));
            Config.SubMenu("Harass").AddItem(new MenuItem("UseEHarass", "使用 E").SetValue(true));
            Config.SubMenu("Harass")
                .AddItem(
                    new MenuItem("HarassMode", "骚扰 模式: ").SetValue(
                        new StringList(new[] { "Q+W", "Q+E", "默认" })));
            Config.SubMenu("Harass")
                .AddItem(new MenuItem("HarassMana", "最小. Mana %: ").SetValue(new Slider(50, 100, 0)));
            Config.SubMenu("Harass")
                .AddItem(
                    new MenuItem("HarassActive", "骚扰 按键").SetValue(
                        new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));

            // Lane Clear
            Config.AddSubMenu(new Menu("清 线", "LaneClear"));
            Config.SubMenu("LaneClear").AddItem(new MenuItem("UseQLaneClear", "使用 Q").SetValue(false));
            Config.SubMenu("LaneClear")
                .AddItem(new MenuItem("UseQLaneClearDontUnderTurret", "禁止塔下 Q").SetValue(true));
            Config.SubMenu("LaneClear").AddItem(new MenuItem("UseELaneClear", "使用 E").SetValue(false));
            Config.SubMenu("LaneClear")
                .AddItem(new MenuItem("LaneClearMana", "最小. Mana %: ").SetValue(new Slider(50, 100, 0)));
            Config.SubMenu("LaneClear")
                .AddItem(
                    new MenuItem("LaneClearActive", "清线 按键").SetValue(
                        new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));

            // Jungling Farm
            Config.AddSubMenu(new Menu("清 野", "JungleFarm"));
            Config.SubMenu("JungleFarm").AddItem(new MenuItem("UseQJungleFarm", "使用 Q").SetValue(true));
            Config.SubMenu("JungleFarm").AddItem(new MenuItem("UseEJungleFarm", "使用 E").SetValue(false));
            Config.SubMenu("JungleFarm")
                .AddItem(new MenuItem("JungleFarmMana", "最小. mp %: ").SetValue(new Slider(50, 100, 0)));
            Config.SubMenu("JungleFarm")
                .AddItem(
                    new MenuItem("AutoSmite", "自动 惩戒").SetValue<KeyBind>(new KeyBind('N', KeyBindType.Toggle)));
            Config.SubMenu("JungleFarm")
                .AddItem(
                    new MenuItem("JungleFarmActive", "清野 按键").SetValue(
                        new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));

            Config.AddSubMenu(new Menu("W 设置", "HealSettings"));
            Config.SubMenu("HealSettings").AddItem(new MenuItem("HealUseW", "使用 W").SetValue(true));
            Config.SubMenu("HealSettings")
                .AddItem(new MenuItem("HealPercent", "最小. HP %").SetValue(new Slider(50, 100, 0)));

            Config.SubMenu("HealSettings")
                .AddItem(new MenuItem("JungleFarmMana", "最小. MP %:").SetValue(new Slider(50, 100, 0)));
            Config.SubMenu("HealSettings")
                .AddItem(
                    new MenuItem("AutoSmite", "自动 惩戒").SetValue<KeyBind>(new KeyBind('N', KeyBindType.Toggle)));

            // Extras
            MenuExtras = new Menu("额 外", "Extras");
            Config.AddSubMenu(MenuExtras);

            // Extras -> Use Items 
            var menuUseItems = new Menu("使用 物品", "menuUseItems");
            MenuExtras.AddSubMenu(menuUseItems);

            // Extras -> Use Items -> Targeted Items
            MenuTargetedItems = new Menu("目标 物品", "menuTargetItems");
            menuUseItems.AddSubMenu(MenuTargetedItems);
            MenuTargetedItems.AddItem(new MenuItem("item3153", "破败").SetValue(true));
            MenuTargetedItems.AddItem(new MenuItem("item3143", "蓝盾").SetValue(true));
            MenuTargetedItems.AddItem(new MenuItem("item3144", "水银刀").SetValue(true));
            MenuTargetedItems.AddItem(new MenuItem("item3146", "科技枪").SetValue(true));
            MenuTargetedItems.AddItem(new MenuItem("item3184", "冰霜战锤 ").SetValue(true));

            // Extras -> Use Items -> AOE Items
            MenuNonTargetedItems = new Menu("AOE 物品", "menuNonTargetedItems");
            menuUseItems.AddSubMenu(MenuNonTargetedItems);
            MenuNonTargetedItems.AddItem(new MenuItem("item3180", "奥丁面纱").SetValue(true));
            MenuNonTargetedItems.AddItem(new MenuItem("item3131", "神圣之剑").SetValue(true));
            MenuNonTargetedItems.AddItem(new MenuItem("item3074", "贪欲九头蛇").SetValue(true));
            MenuNonTargetedItems.AddItem(new MenuItem("item3142", "幽梦之灵").SetValue(true));

            // Drawing
            Config.AddSubMenu(new Menu("范 围", "Drawings"));
            Config.SubMenu("Drawings")
                .AddItem(
                    new MenuItem("DrawQRange", "Q 范围").SetValue(
                        new Circle(true, System.Drawing.Color.FromArgb(255, 255, 255, 255))));
            Config.SubMenu("Drawings")
                .AddItem(
                    new MenuItem("DrawSmite", "惩戒 范围").SetValue(
                        new Circle(false, System.Drawing.Color.FromArgb(255, 255, 255, 255))));

            new PotionManager();

            Config.AddToMainMenu();


            Game.OnGameUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Game.PrintChat("鑺辫竟姹夊寲-xQx鏄撳ぇ甯堜辅鍔犺浇鎴愬姛!");
            Game.PrintChat(
                String.Format(
                    "<font color='#70DBDB'>xQx | </font> <font color='#FFFFFF'>{0}" +
                    "</font> <font color='#70DBDB'> Loaded!</font>", ChampionName));
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            var drawQRange = Config.Item("DrawQRange").GetValue<Circle>();
            if (drawQRange.Active)
            {
                Render.Circle.DrawCircle(Player.Position, Q.Range, drawQRange.Color);
            }
        }

        private static Obj_AI_Hero GetEnemy(float vDefaultRange = 0,
            TargetSelector.DamageType vDefaultDamageType = TargetSelector.DamageType.Physical)
        {
            if (Math.Abs(vDefaultRange) < 0.00001)
                vDefaultRange = Q.Range;

            if (!Config.Item("AssassinActive").GetValue<bool>())
                return TargetSelector.GetTarget(vDefaultRange, vDefaultDamageType);

            var assassinRange = Config.Item("AssassinSearchRange").GetValue<Slider>().Value;

            var vEnemy =
                ObjectManager.Get<Obj_AI_Hero>()
                    .Where(
                        enemy =>
                            enemy.Team != ObjectManager.Player.Team && !enemy.IsDead && enemy.IsVisible &&
                            Config.Item("Assassin" + enemy.ChampionName) != null &&
                            Config.Item("Assassin" + enemy.ChampionName).GetValue<bool>() &&
                            ObjectManager.Player.Distance(enemy) < assassinRange);

            if (Config.Item("AssassinSelectOption").GetValue<StringList>().SelectedIndex == 1)
            {
                vEnemy = (from vEn in vEnemy select vEn).OrderByDescending(vEn => vEn.MaxHealth);
            }

            Obj_AI_Hero[] objAiHeroes = vEnemy as Obj_AI_Hero[] ?? vEnemy.ToArray();

            Obj_AI_Hero t = !objAiHeroes.Any()
                ? TargetSelector.GetTarget(vDefaultRange, vDefaultDamageType)
                : objAiHeroes[0];

            return t;
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            if (!Orbwalking.CanMove(100))
                return;

            if (Config.Item("ComboActive").GetValue<KeyBind>().Active)
            {
                Combo();
            }

            if (Config.Item("HarassActive").GetValue<KeyBind>().Active)
            {
                var existsMana = Player.MaxMana / 100 * Config.Item("HarassMana").GetValue<Slider>().Value;
                if (Player.Mana >= existsMana)
                    Harass();
            }

            if (Config.Item("LaneClearActive").GetValue<KeyBind>().Active)
            {
                var existsMana = Player.MaxMana / 100 * Config.Item("LaneClearMana").GetValue<Slider>().Value;
                if (Player.Mana >= existsMana)
                    LaneClear();
            }

            if (Config.Item("JungleFarmActive").GetValue<KeyBind>().Active)
            {
                var existsMana = Player.MaxMana / 100 * Config.Item("JungleFarmMana").GetValue<Slider>().Value;
                if (Player.Mana >= existsMana)
                    JungleFarm();
            }

            if (Config.Item("HealUseW").GetValue<KeyBind>().Active)
            {
                foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => !enemy.IsAlly))
                {
                    if (Player.Health > enemy.Health && Player.Level >= enemy.Level)
                        return;
                    if (Player.Distance(enemy) > Q.Range + 200 && enemy.IsValid && enemy.IsVisible)
                        return;
                }

                var existsHp = Player.MaxMana / 100 * Config.Item("HealPercent").GetValue<Slider>().Value;
                if (Player.Health <= existsHp)
                    W.Cast(Player, true);
            }
        }

        private static void Combo()
        {
            var t = GetEnemy(Q.Range, TargetSelector.DamageType.Physical);

            var useQ = Config.Item("UseQCombo").GetValue<bool>();
            var useE = Config.Item("UseECombo").GetValue<bool>();
            var useR = Config.Item("UseRCombo").GetValue<bool>();
            var useQDontUnderTurret = Config.Item("UseQComboDontUnderTurret").GetValue<bool>();

            if (Q.IsReady() && useQ && t != null)
            {
                if (useQDontUnderTurret)
                {
                    if (!t.UnderTurret())
                        Q.CastOnUnit(t);
                }
                else
                {
                    Q.CastOnUnit(t);
                }
            }

            if (Player.Distance(t) <= E.Range + 50)
                UseItems(t, true);

            if (t != null)
                UseItems(t);

            if (E.IsReady() && useE && Player.Distance(t) <= E.Range)
                E.Cast();

            if (IgniteSlot != SpellSlot.Unknown && Player.Spellbook.CanUseSpell(IgniteSlot) == SpellState.Ready)
            {
                if (t != null && Player.GetSummonerSpellDamage(t, Damage.SummonerSpell.Ignite) > t.Health)
                {
                    Player.Spellbook.CastSpell(IgniteSlot, t);
                }
            }

            if (R.IsReady() && useR && t != null)
            {
                if (ObjectManager.Player.CountEnemysInRange((int) Q.Range) >= 2)
                {
                    R.CastOnUnit(Player);
                }
            }

            if (Tiamat.IsReady() && Player.Distance(t) <= Tiamat.Range)
                Tiamat.Cast();

            if (Hydra.IsReady() && Player.Distance(t) <= Hydra.Range)
                Tiamat.Cast();

        }

        private static void Harass() {}

        private static void LaneClear()
        {
            var useQ = Config.Item("UseQLaneClear").GetValue<bool>();
            var useE = Config.Item("UseELaneClear").GetValue<bool>();
            var useQDontUnderTurret = Config.Item("UseQLaneClearDontUnderTurret").GetValue<bool>();

            var allMinions = MinionManager.GetMinions(
                ObjectManager.Player.ServerPosition, Q.Range, MinionTypes.All, MinionTeam.NotAlly);


            if (allMinions.Count >= 2)
            {
                if (Tiamat.IsReady())
                    Tiamat.Cast();

                if (Hydra.IsReady())
                    Hydra.Cast();
            }

            if (Q.IsReady() && useQ)
            {
                var closestMinion = new Obj_AI_Base();
                if (allMinions.Any())
                {
                    foreach (var minion in allMinions)
                    {
                        if (allMinions.IndexOf(minion) == 0)
                        {
                            closestMinion = minion;
                        }
                        else if (Player.Distance(minion.Position) < Player.Distance(closestMinion.Position))
                        {
                            closestMinion = minion;
                        }
                    }
                    if (!closestMinion.IsValidTarget())
                        return;

                    Q.Cast(closestMinion);
                }
            }
        }

        private static void JungleFarm()
        {
            if (!Config.Item("JungleFarmActive").GetValue<KeyBind>().Active)
                return;

            var useQ = Config.Item("UseQJungleFarm").GetValue<bool>();
            var useE = Config.Item("UseEJungleFarm").GetValue<bool>();

            var mobs = MinionManager.GetMinions(
                ObjectManager.Player.ServerPosition, Q.Range, MinionTypes.All, MinionTeam.Neutral,
                MinionOrderTypes.MaxHealth);

            if (mobs.Count <= 0)
                return;

            var mob = mobs[0];
            if (useE && E.IsReady() && Player.Distance(mob) < Orbwalking.GetRealAutoAttackRange(Player))
            {
                E.Cast(mob);
            }

            if (useQ && Q.IsReady())
            {
                Q.Cast(mob);
            }

            if (mobs.Count >= 2)
            {
                if (Tiamat.IsReady())
                    Tiamat.Cast();

                if (Hydra.IsReady())
                    Hydra.Cast();
            }
        }

        private static InventorySlot GetInventorySlot(int ID)
        {
            return
                ObjectManager.Player.InventoryItems.FirstOrDefault(
                    item =>
                        (item.Id == (ItemId) ID && item.Stacks >= 1) || (item.Id == (ItemId) ID && item.Charges >= 1));
        }

        public static void UseItems(Obj_AI_Hero vTarget, bool useNonTargetedItems = false)
        {
            if (vTarget == null)
                return;
            foreach (var itemID in from menuItem in MenuTargetedItems.Items
                let useItem = MenuTargetedItems.Item(menuItem.Name).GetValue<bool>()
                where useItem
                select Convert.ToInt16(menuItem.Name.ToString().Substring(4, 4))
                into itemID
                where Items.HasItem(itemID) && Items.CanUseItem(itemID) && GetInventorySlot(itemID) != null
                select itemID)
            {
                Items.UseItem(itemID, vTarget);
            }

            if (!useNonTargetedItems)
                return;

            foreach (var itemID in from menuItem in MenuNonTargetedItems.Items
                let useItem = MenuNonTargetedItems.Item(menuItem.Name).GetValue<bool>()
                where useItem
                select Convert.ToInt16(menuItem.Name.ToString().Substring(4, 4))
                into itemID
                where Items.HasItem(itemID) && Items.CanUseItem(itemID) && GetInventorySlot(itemID) != null
                select itemID)
            {
                Items.UseItem(itemID);
            }
        }
    }
}