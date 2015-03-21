﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using DZAIO.Utility.Helpers;
using LeagueSharp;
using LeagueSharp.Common;

namespace DZAIO.Utility
{
    class ItemManager
    {
        private static float _lastCheckTick;
        //TODO: List of Activator Features here:

        //TODO: Shield Module
        //TODO: Summoners Spells Implementation

        private static readonly List<DzItem> ItemList = new List<DzItem>
        {
            new DzItem
            {
                Id=3144,
                Name = "Bilgewater Cutlass",
                Range = 600f,
                Class = ItemClass.Offensive,
                Mode = ItemMode.Targeted
            },
            new DzItem
            {
                Id= 3153,
                Name = "Blade of the Ruined King",
                Range = 600f,
                Class = ItemClass.Offensive,
                Mode = ItemMode.Targeted
            },
            new DzItem
            {
                Id= 3142,
                Name = "Youmuu",
                Range = 600f,
                Class = ItemClass.Offensive,
                Mode = ItemMode.NoTarget
            }
        };


        public static void OnLoad(Menu menu)
        {
            //Create the menu here.
            var cName = ObjectManager.Player.ChampionName;
            var activatorMenu = new Menu(cName + " - 活化剂", "dzaio.activator");

            //Offensive Menu
            var offensiveMenu = new Menu("活化剂 - 进攻", "dzaio.activator.offensive");
            var offensiveItems = ItemList.FindAll(item => item.Class == ItemClass.Offensive);
            foreach (var item in offensiveItems)
            {
                var itemMenu = new Menu(item.Name, cName + item.Id);
                itemMenu.AddItem(new MenuItem("dzaio.activator." + item.Id + ".always", "总是").SetValue(true));
                itemMenu.AddItem(new MenuItem("dzaio.activator." + item.Id + ".onmyhp", "自己血量 <  %").SetValue(new Slider(30)));
                itemMenu.AddItem(new MenuItem("dzaio.activator." + item.Id + ".ontghpgreater", "目标血量 >  %").SetValue(new Slider(40)));
                itemMenu.AddItem(new MenuItem("dzaio.activator." + item.Id + ".ontghplesser", "目标血量 <  %").SetValue(new Slider(40)));
                itemMenu.AddItem(new MenuItem("dzaio.activator." + item.Id + ".ontgkill", "目标可击杀").SetValue(true));
                itemMenu.AddItem(new MenuItem("dzaio.activator." + item.Id + ".displaydmg", "显示伤害").SetValue(true));
                offensiveMenu.AddSubMenu(itemMenu);
            }
            activatorMenu.AddSubMenu(offensiveMenu);

            //Defensive Menu
            AddHitChanceSelector(activatorMenu);

            activatorMenu.AddItem(new MenuItem("dzaio.activator.activatordelay", "延迟").SetValue(new Slider(80, 0, 300)));
            activatorMenu.AddItem(new MenuItem("dzaio.activator.enabledalways", "启动").SetValue(false));
            activatorMenu.AddItem(new MenuItem("dzaio.activator.enabledcombo", "按键").SetValue(new KeyBind(32, KeyBindType.Press)));
            menu.AddSubMenu(activatorMenu);
            Game.OnUpdate += Game_OnGameUpdate;
        }

        static void Game_OnGameUpdate(EventArgs args)
        {
            if (!MenuHelper.isMenuEnabled("dzaio.activator.enabledalways") &&
                !MenuHelper.getKeybindValue("dzaio.activator.enabledcombo"))
            {
                return;
            }
            if (Environment.TickCount - _lastCheckTick < MenuHelper.getSliderValue("dzaio.activator.activatordelay"))
            {
                return;
            }
            _lastCheckTick = Environment.TickCount;
            UseOffensive();
        }

        static void UseOffensive()
        {
            var offensiveItems = ItemList.FindAll(item => item.Class == ItemClass.Offensive);
            foreach (var item in offensiveItems)
            {
                var selectedTarget = Hud.SelectedUnit as Obj_AI_Base ?? TargetSelector.GetTarget(item.Range, TargetSelector.DamageType.True);
                if (!selectedTarget.IsValidTarget(item.Range))
                {
                    return;
                }
                if (MenuHelper.isMenuEnabled("dzaio.activator." + item.Id + ".always"))
                {
                    UseItem(selectedTarget, item);
                }
                if (ObjectManager.Player.HealthPercentage() < MenuHelper.getSliderValue("dzaio.activator." + item.Id + ".onmyhp"))
                {
                    UseItem(selectedTarget, item);
                }
                if (selectedTarget.HealthPercentage() < MenuHelper.getSliderValue("dzaio.activator." + item.Id + ".ontghplesser") && !MenuHelper.isMenuEnabled("dzaio.activator." + item.Id + ".ontgkill"))
                {
                    UseItem(selectedTarget, item);
                }
                if (selectedTarget.HealthPercentage() > MenuHelper.getSliderValue("dzaio.activator." + item.Id + ".ontghpgreater"))
                {
                    UseItem(selectedTarget, item);
                }
                if (selectedTarget.Health < ObjectManager.Player.GetSpellDamage(selectedTarget, GetItemSpellSlot(item)) && MenuHelper.isMenuEnabled("dzaio.activator." + item.Id + ".ontgkill"))
                {
                    UseItem(selectedTarget, item);
                }
            }
        }


        static void UseItem(Obj_AI_Base target, DzItem item)
        {
            if (!Items.HasItem(item.Id) || !Items.CanUseItem(item.Id))
            {
                return;
            }
            switch (item.Mode)
            {
                case ItemMode.Targeted:
                    Items.UseItem(item.Id, target);
                    break;
                case ItemMode.NoTarget:
                    Items.UseItem(item.Id, ObjectManager.Player);
                    break;
                case ItemMode.Skillshot:
                    if (item.CustomInput == null)
                    {
                        return;
                    }
                    var customPred = Prediction.GetPrediction(item.CustomInput);
                    if (customPred.Hitchance >= GetHitchance())
                    {
                        Items.UseItem(item.Id, customPred.CastPosition);
                    }
                    break;
            }
        }

        static SpellSlot GetItemSpellSlot(DzItem item)
        {
            foreach (var it in ObjectManager.Player.InventoryItems.Where(it => (int)it.Id == item.Id))
            {
                return it.SpellSlot != SpellSlot.Unknown ? it.SpellSlot : SpellSlot.Unknown;
            }
            return SpellSlot.Unknown;
        }

        public static HitChance GetHitchance()
        {
            switch (DZAIO.Config.Item("dzaio.activator.customhitchance").GetValue<StringList>().SelectedIndex)
            {
                case 0:
                    return HitChance.Low;
                case 1:
                    return HitChance.Medium;
                case 2:
                    return HitChance.High;
                case 3:
                    return HitChance.VeryHigh;
                default:
                    return HitChance.Medium;
            }
        }

        public static void AddHitChanceSelector(Menu menu)
        {
            menu.AddItem(new MenuItem("dzaio.activator.customhitchance", "命中率").SetValue(new StringList(new[] { "低", "正常", "高", "非常高" }, 2)));
        }

        internal static float GetItemsDamage(Obj_AI_Hero target)
        {
            var items = ItemList.Where(item => Items.HasItem(item.Id) && Items.CanUseItem(item.Id) && MenuHelper.isMenuEnabled("dzaio.activator." + item.Id + ".displaydmg"));
            return items.Sum(item => (float)ObjectManager.Player.GetSpellDamage(target, GetItemSpellSlot(item)));
        }
    }

    internal class DzItem
    {
        public String Name { get; set; }
        public int Id { get; set; }
        public float Range { get; set; }
        public ItemClass Class { get; set; }
        public ItemMode Mode { get; set; }
        public PredictionInput CustomInput { get; set; }
    }

    enum ItemMode
    {
        Targeted, Skillshot, NoTarget
    }

    enum ItemClass
    {
        Offensive, Defensive
    }
}
