using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using Version = System.Version;

namespace SigmaSeries
{

    public abstract class PluginBase
    {
        public Orbwalking.Orbwalker Orbwalker { set; get; }
        public string ChampName { get; set; }
        public Version Version { get; set; }
        public bool ComboActive { get { return Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo; } }
        public bool HarassActive { get { return Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed; } }
        public bool WaveClearActive { get { return Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear; } }
        public bool FleeActive { get { return false; } }
        public bool FreezeActive { get { return Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LastHit; } }
        public Obj_AI_Hero Player { get { return ObjectManager.Player; } }
        public Spell Q { get; set; }
        public Spell W { get; set; }
        public Spell E { get; set; }
        public Spell R { get; set; }

        public List<Obj_AI_Base> JungleMinions = new List<Obj_AI_Base>();

        public Menu Config { get; set; }
        public Menu ComboConfig { get; set; }
        public Menu HarassConfig { get; set; }
        public Menu DrawConfig { get; set; }
        public Menu FarmConfig { get; set; }
        public Menu BonusConfig { get; set; }
        public Menu ItemConfig { get; set; }
        public Menu SummonerConfig { get; set; }

        public List<Spell> SpellList = new List<Spell>();

        public static Items.Item Hydra;
        public static Items.Item Tiamat;
        public static Items.Item DFG;

        public void castItems(Obj_AI_Base target, bool isMinion = false)
        {
            return;
            if (isMinion)
            {
                if (Player.Distance(target) <= Hydra.Range && Config.Item("hdr").GetValue<bool>())
                {
                    Hydra.Cast(target);
                }
                if (Player.Distance(target) <= Tiamat.Range && Config.Item("tia").GetValue<bool>())
                {
                    Tiamat.Cast(target);
                }
            }
            else
            {
                if (Player.Distance(target) <= Hydra.Range && Config.Item("hdr").GetValue<bool>())
                {
                    Hydra.Cast(target);
                }
                if (Player.Distance(target) <= Tiamat.Range && Config.Item("tia").GetValue<bool>())
                {
                    Tiamat.Cast(target);
                }
                if (target.IsValidTarget(DFG.Range) && Config.Item("dfg").GetValue<bool>())
                {
                    DFG.Cast(target);
                }
            }
        }


        public static SpellSlot IgniteSlot;

        protected PluginBase(Version version)
        {
            ChampName = Player.ChampionName;
            Version = version;

            Game.PrintChat("Sigma" + ChampName + " Loaded. Version: " + Version.ToString() + " - By Fluxy");
            Game.PrintChat("鑺辫竟姹夊寲-Sigma铔囧コ鍗曠嫭鎻愬彇鐗堜辅鍔犺浇鎴愬姛!");
            Game.PrintChat("璇锋敞鎰忚繖涓剼鏈彧鑳戒娇鐢ㄨ泧濂充辅鎻愬彇鍑烘潵鏂逛究浣跨敤!");
            Game.PrintChat("L#杩欎釜椤圭洰鐩墠鏄厤璐圭殑涓ㄦ窐瀹濆崠閽辩殑浣犳槸缂洪挶缁欎綘鑷繁涔版：鏉愪箞,缂洪挶灏卞彨鑺卞摜鎴戞柦鑸嶄竴鐐圭粰浣犱拱鍟婁辅鑽変綘濞樼殑鐪熺壒涔堢殑鏈夌梾");

            Utility.DelayAction.Add(250, () =>
            {
                SpellList.Add(Q);
                SpellList.Add(W);
                SpellList.Add(E);
                SpellList.Add(R);
            });

            Hydra = new Items.Item(3074, 175f);
            Tiamat = new Items.Item(3077, 175f);
            DFG = new Items.Item(3128, 750f);


            IgniteSlot = Player.GetSpellSlot("SummonerDot");

            createConfigs();
            eventsLoad();
            extraEvents();
            addOW();
        }

        private void eventsLoad()
        {
            Game.OnGameUpdate += OnUpdate;
            Drawing.OnDraw += OnDraw;
            Orbwalking.BeforeAttack += BeforeAttack;
            AntiGapcloser.OnEnemyGapcloser += OnEnemyGapcloser;
            Interrupter.OnPossibleToInterrupt +=Interrupter_OnPossibleToInterrupt;
        }

        private void extraEvents()
        {
            Game.OnGameUpdate += args =>
            {
                JungleMinions = MinionManager.GetMinions(Player.Position, 800, MinionTypes.All, MinionTeam.Neutral,
                    MinionOrderTypes.MaxHealth).ToList();
                var Target = TargetSelector.GetTarget(600, TargetSelector.DamageType.Magical);
                if (Target != null && IgniteSlot != SpellSlot.Unknown 
                    && Player.Spellbook.CanUseSpell(IgniteSlot) == SpellState.Ready 
                    && ObjectManager.Player.GetSummonerSpellDamage(Target, Damage.SummonerSpell.Ignite) > Target.Health
                    && Config.Item("IGNks").GetValue<bool>())
                {
                    Player.Spellbook.CastSpell(IgniteSlot, Target);
                }
            };

            Drawing.OnDraw += args =>
            {
                foreach (var spell in SpellList.Where(s => s != null))
                {
                    var menuItem = Config.Item(spell.Slot + "Range" + ChampName).GetValue<Circle>();
                    if (menuItem.Active && spell.Level > 0 && spell.IsReady())
                        Utility.DrawCircle(Player.Position, spell.Range, menuItem.Color);
                }
            };
        }

        private void createConfigs()
        {
            Config = new Menu("花边汉化-Sigma蛇女提取版", "SigmaSeries - " + Player.ChampionName, true);
            var tsMenu = Config.AddSubMenu(new Menu("目标 选择", "TargetSelector"));
            TargetSelector.AddToMenu(tsMenu);
            ComboConfig = Config.AddSubMenu(new Menu("连   招", "Combo"));
            HarassConfig = Config.AddSubMenu(new Menu("骚   扰", "Harass"));
            FarmConfig = Config.AddSubMenu(new Menu("打   钱", "Farm"));
            BonusConfig = Config.AddSubMenu(new Menu("额外 设置", "Extra"));

            ItemConfig = Config.AddSubMenu(new Menu("物品 设置", "Item Configs"));

            SummonerConfig = Config.AddSubMenu(new Menu("召唤师技能 设置", "Summoner Configs"));
           

            DrawConfig = Config.AddSubMenu(new Menu("Draw", "Draw"));
            DrawConfig.AddItem(new MenuItem("QRange" + ChampName, "Q 范围").SetValue(new Circle(false, System.Drawing.Color.Green)));
            DrawConfig.AddItem(new MenuItem("WRange" + ChampName, "W 范围").SetValue(new Circle(false, System.Drawing.Color.Green)));
            DrawConfig.AddItem(new MenuItem("ERange" + ChampName, "E 范围").SetValue(new Circle(false, System.Drawing.Color.Green)));
            DrawConfig.AddItem(new MenuItem("RRange" + ChampName, "R 范围").SetValue(new Circle(false, System.Drawing.Color.Green)));

            ComboMenu(ComboConfig);
            HarassMenu(HarassConfig);
            FarmMenu(FarmConfig);
            BonusMenu(BonusConfig);
            DrawingMenu(DrawConfig);
            ItemMenu(ItemConfig);
            SummonerMenu(SummonerConfig);
            
            Config.AddToMainMenu();
        }

        public virtual void SummonerMenu(Menu SummonerMenu)
        {
            var subIGNITE = SummonerConfig.AddSubMenu(new Menu("点 燃", "Ignite"));
            subIGNITE.AddItem(new MenuItem("IGNks", "使用点燃丨击杀敌人").SetValue(true));
        }

        public void addOW()
        {
            Orbwalker = new Orbwalking.Orbwalker(Config.AddSubMenu(new Menu("走   砍", "Orbwalking")));
        }
        public virtual void ComboMenu(Menu config)
        {

        }
        public virtual void ItemMenu(Menu config)
        {
            var subAD = ItemConfig.AddSubMenu(new Menu("AD 物品", "AD Items"));

            subAD.AddItem(new MenuItem("hdr", "使用 九头蛇").SetValue(true));
            subAD.AddItem(new MenuItem("tia", "使用 提亚马特").SetValue(true));

            var subAP = ItemConfig.AddSubMenu(new Menu("AP 物品", "AP Items"));

            subAP.AddItem(new MenuItem("dfg", "使用 冥火(已删除)").SetValue(true));

            ItemConfig.AddItem(new MenuItem("UseItems", "使用 物品").SetValue(true));
        }
        public virtual void HarassMenu(Menu config)
        {
        }
        public virtual void FarmMenu(Menu config)
        {
        }
        public virtual void BonusMenu(Menu config)
        {
        }
        public virtual void DrawingMenu(Menu config)
        {
        }
        public virtual void OnDraw(EventArgs args)
        {
        }
        public virtual void OnUpdate(EventArgs args)
        {
        }
        public virtual void BeforeAttack(Orbwalking.BeforeAttackEventArgs args)
        {
        }
        public virtual void AfterAttack(Obj_AI_Base unit, Obj_AI_Base target)
        {
        }
        public virtual void OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
        }
        public virtual void Interrupter_OnPossibleToInterrupt(Obj_AI_Base unit, InterruptableSpell spell)
        {
        }

    }
}