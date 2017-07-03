﻿using Scripts.Game.Defined.Characters;
using Scripts.Game.Defined.Characters.StartingSpells;
using Scripts.Game.Defined.Serialized.Characters;
using Scripts.Game.Defined.Serialized.Items.Consumables;
using Scripts.Game.Defined.Serialized.Items.Equipment;
using Scripts.Game.Defined.Serialized.Items.Misc;
using Scripts.Game.Defined.Serialized.Spells;
using Scripts.Game.Defined.Serialized.Statistics;
using Scripts.Game.Defined.StartingStats;
using Scripts.Game.Undefined.StartingStats;
using Scripts.Model.Characters;
using Scripts.Model.Items;
using Scripts.Model.Spells;
using Scripts.Model.Stats;

namespace Scripts.Game.Defined.Characters {
    public static class CharacterList {
        #region Character Builder stuff
        private struct ItemCount {
            public readonly Item Item;
            public readonly int Count;
            public ItemCount(Item item, int count) {
                this.Count = count;
                this.Item = item;
            }

            public ItemCount(Item item) {
                this.Item = item;
                this.Count = 1;
            }
        }

        private static Character StandardEnemy(Stats stats, Look look, Brain brain) {
            Character enemy = new Character(stats, look, brain);
            enemy.AddFlag(Model.Characters.Flag.DROPS_ITEMS);
            return enemy;
        }

        private static Character AddStats(this Character c, params Stat[] stats) {
            foreach (Stat stat in stats) {
                c.Stats.AddStat(stat);
            }
            return c;
        }

        private static Character AddSpells(this Character c, params SpellBook[] books) {
            foreach (SpellBook sb in books) {
                c.Spells.AddSpellBook(sb);
            }
            return c;
        }

        private static Character AddFlags(this Character c, params Model.Characters.Flag[] flags) {
            foreach (Model.Characters.Flag flag in flags) {
                c.AddFlag(flag);
            }
            return c;
        }

        private static Character AddItems(this Character c, params ItemCount[] items) {
            foreach (ItemCount itemCount in items) {
                c.Inventory.ForceAdd(itemCount.Item, itemCount.Count);
            }
            return c;
        }

        private static Character AddItem(this Character c, Item item, int count, bool isAdded = true) {
            if (isAdded) {
                c.Inventory.ForceAdd(item, count);
            }
            return c;
        }

        private static Character AddItem(this Character c, Item item, bool isAdded = true) {
            return c.AddItem(item, 1, isAdded);
        }

        private static Character AddEquips(this Character c, params EquippableItem[] equips) {
            foreach (EquippableItem equip in equips) {
                Inventory dummy = new Inventory();
                dummy.ForceAdd(equip);
                c.Equipment.AddEquip(dummy, new Model.Buffs.BuffParams(c.Stats, c.Id), equip);
            }
            return c;
        }

        private static Character AddEquip(this Character c, EquippableItem equip, bool isAdded = true) {
            if (isAdded) {
                Inventory dummy = new Inventory();
                dummy.ForceAdd(equip);
                c.Equipment.AddEquip(dummy, new Model.Buffs.BuffParams(c.Stats, c.Id), equip);
            }
            return c;
        }

        #endregion

        public static Character Hero(string name) {
            return new Character(
                new Stats(0, 1, 1, 1, 5),
                RuinsLooks.Hero(name),
                new Player())
                .AddFlags(Model.Characters.Flag.PLAYER, Model.Characters.Flag.PERSISTS_AFTER_DEFEAT)
                .AddStats(new Experience());
        }

        public static Character NotKitsune() {
            Character c = new Character(new Stats(5, 55, 1, 5, 1), Debug.NotKitsune(), new Player())
                .AddItems(new ItemCount(new Apple(), 7), new ItemCount(new PoisonArmor()), new ItemCount(new Money(), 100))
                .AddFlags(Model.Characters.Flag.DROPS_ITEMS);
            return c;
        }

        public static class Ruins {

            public static Character Villager() {
                return StandardEnemy(
                    new Stats(2, 1, 1, 1, 2),
                    RuinsLooks.Villager(),
                    new RuinsBrains.Villager())
                    .AddItem(new Money(), Util.Range(0, 3));
            }

            public static Character Knight() {
                return StandardEnemy(
                    new Stats(3, 2, 2, 2, 5),
                    RuinsLooks.Knight(),
                    new RuinsBrains.Knight())
                    .AddEquip(new GhostArmor(), Util.IsChance(.50f))
                    .AddEquip(new BrokenSword(), Util.IsChance(.50f))
                    .AddSpells(new SetupCounter());
            }

            public static Character Healer() {
                return StandardEnemy(new Stats(3, 1, 5, 5, 1), RuinsLooks.Healer(), new RuinsBrains.Healer())
                    .AddItem(new Apple(), Util.IsChance(.50f))
                    .AddSpells(new Heal());
            }
        }
    }

}

namespace Scripts.Game.Undefined.Characters {
    public class CreditsDummy : Character {
        public CreditsDummy(
            Breed breed,
            int level,
            string name,
            string spriteLoc,
            string tip)
            : base(new DummyStats(level),
                  new DummyLook(breed, name, Util.GetSprite(spriteLoc), tip),
                  new DebugAI()) {

        }
    }
}