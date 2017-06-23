﻿using Scripts.Model.Characters;
using Scripts.Model.Interfaces;
using Scripts.Model.Stats;
using Scripts.View.Portraits;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scripts.Model.SaveLoad.SaveObjects;
using Scripts.Model.SaveLoad;
using System.Collections.ObjectModel;
using System.Linq;

namespace Scripts.Model.Spells {

    public struct SpellParams {
        public int CharacterId;
        public Characters.Stats Stats;
        public Characters.SpellBooks Spells;
        public Characters.Buffs Buffs;
        public Characters.Inventory Inventory;
        public Characters.Equipment Equipment;

        public SpellParams(Character c) {
            this.CharacterId = c.Id;
            this.Stats = c.Stats;
            this.Spells = c.Spells;
            this.Buffs = c.Buffs;
            this.Inventory = c.Inventory;
            this.Equipment = c.Equipment;
        }
    }

    public abstract class SpellBook : ISpellable, ISaveable<SpellBookSave> {

        public readonly string Name;
        public readonly Sprite Icon;
        public readonly TargetType TargetType;
        public readonly SpellType SpellType;
        public readonly IDictionary<StatType, int> Costs;
        public readonly int Priority;

        public readonly int CastTime;
        public readonly int Cooldown;
        public readonly bool IsSilenced; // TODO SAVE THIS

        protected readonly HashSet<Flag> flags;

        public SpellBook(string spellName, Sprite sprite, TargetType target, SpellType spell, int castTime, int cooldown) {
            this.Name = spellName;
            this.Icon = sprite;
            this.Costs = new Dictionary<StatType, int>();
            this.TargetType = target;
            this.SpellType = spell;
            this.CastTime = castTime;
            this.Cooldown = cooldown;
            this.flags = new HashSet<Flag>() { Flag.CASTER_REQUIRES_SPELL };
        }

        public SpellBook(string spellName, string spriteLoc, TargetType target, SpellType spell, int castTime, int cooldown)
            : this(spellName, Util.GetSprite(spriteLoc), target, spell, castTime, cooldown) { }

        public override bool Equals(object obj) {
            var item = obj as SpellBook;

            if (item == null) {
                return false;
            }

            return this.GetType().Equals(item.GetType())
                && this.IsSilenced.Equals(item.IsSilenced);
        }

        public override int GetHashCode() {
            return GetType().GetHashCode();
        }

        public bool HasFlag(Flag f) {
            return flags.Contains(f);
        }

        public virtual string CreateDescription(SpellParams caster) {
            return string.Format("{0}{3}\n\nTarget: {1}\nCost: {2}",
                CasterHasResources(caster.Stats) ? string.Empty : Util.ColorString("Insufficient resource.\n", Color.red),
                TargetType.Name,
                Costs.Count == 0 ? "None" : GetCommaSeparatedCosts(caster.Stats),
                CreateDescriptionHelper(caster)
                );
        }

        public string CreateTargetDescription(Character caster, Character target) {
            return string.Format("{0} will use {1} on {2}.\n\n{3}", caster.Look.DisplayName, this.Name, target.Look.DisplayName, CreateDescription(new SpellParams(caster)));
        }

        public abstract string CreateDescriptionHelper(SpellParams caster);

        public bool CasterHasResource(StatType stat, Characters.Stats caster) {
            return caster.GetStatCount(stat) >= Costs[stat];
        }

        public bool CasterHasResources(Characters.Stats caster) {
            foreach (KeyValuePair<StatType, int> stat in Costs) {
                if (caster.GetStatCount(Value.MOD, stat.Key) < stat.Value) {
                    return false;
                }
            }
            return true;
        }

        public bool IsCastable(SpellParams caster, SpellParams target) {
            return !IsSilenced && CasterHasResources(caster.Stats) && IsMeetOtherCastRequirements(caster, target) && (caster.Spells.HasSpellBook(this) || !flags.Contains(Flag.CASTER_REQUIRES_SPELL));
        }

        public Spell BuildSpell(Character caster, Character target) {
            Util.Assert(
                IsCastable(new SpellParams(caster), new SpellParams(target)),
                string.Format(
                "Attempted to cast spell without requirements fulfilled. Resources={0}, OtherRequirements={1}."
                , CasterHasResources(caster.Stats),
                IsMeetOtherCastRequirements(new SpellParams(caster), new SpellParams(target))
                ));

            // Consume resources
            foreach (KeyValuePair<StatType, int> pair in Costs) {
                caster.Stats.AddToStat(pair.Key, -pair.Value);
            }

            return ForceSpell(caster, target);
        }

        public Spell ForceSpell(Character caster, Character target) {
            Result res = new Result();
            if (IsHit(new SpellParams(caster), new SpellParams(target))) {
                res.AddSFX(GetHitSFX(caster.Presenter.PortraitView, target.Presenter.PortraitView));
                if (IsCritical(new SpellParams(caster), new SpellParams(target))) {
                    res.Type = ResultType.CRITICAL;
                    res.Effects = GetCriticalEffects(new SpellParams(caster), new SpellParams(target));
                } else {
                    res.Type = ResultType.HIT;
                    res.Effects = GetHitEffects(new SpellParams(caster), new SpellParams(target));
                }
            } else {
                res.AddSFX(GetMissSFX(caster.Presenter.PortraitView, target.Presenter.PortraitView));
                res.Type = ResultType.MISS;
                res.Effects = GetMissEffects(new SpellParams(caster), new SpellParams(target));
            }

            return new Spell(this, res, caster, target);
        }

        public override string ToString() {
            return base.ToString();
        }

        protected virtual bool IsMeetOtherCastRequirements(SpellParams caster, SpellParams target) {
            return true;
        }

        protected virtual bool IsHit(SpellParams caster, SpellParams target) {
            return true;
        }

        protected abstract IList<SpellEffect> GetHitEffects(SpellParams caster, SpellParams target);

        protected virtual bool IsCritical(SpellParams caster, SpellParams target) {
            return false;
        }

        protected virtual IList<SpellEffect> GetCriticalEffects(SpellParams caster, SpellParams target) {
            return new SpellEffect[0];
        }

        protected virtual IList<SpellEffect> GetMissEffects(SpellParams caster, SpellParams target) {
            return new SpellEffect[0];
        }

        protected virtual IList<IEnumerator> GetHitSFX(PortraitView caster, PortraitView target) {
            return new IEnumerator[0];
        }

        protected virtual IList<IEnumerator> GetMissSFX(PortraitView caster, PortraitView target) {
            return new IEnumerator[0];
        }

        private string GetCommaSeparatedCosts(Characters.Stats caster) {
            string[] arr = new string[Costs.Count];

            int index = 0;
            foreach (KeyValuePair<StatType, int> pair in Costs) {
                arr[index++] = string.Format("{0} {1}",
                    Util.ColorString(pair.Value.ToString(), CasterHasResource(pair.Key, caster)),
                    Util.ColorString(pair.Key.Name, pair.Key.Color));
            }
            return string.Join(", ", arr);
        }

        SpellBook ISpellable.GetSpellBook() {
            return this;
        }

        public virtual string GetDetailedName(SpellParams caster) {
            return Util.ColorString(Name, CasterHasResources(caster.Stats));
        }

        public SpellBookSave GetSaveObject() {
            return new SpellBookSave(GetType());
        }

        public void InitFromSaveObject(SpellBookSave saveObject) {
            // Spellbook doesn't need anything restored!
        }
    }
}