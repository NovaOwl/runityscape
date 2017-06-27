﻿using Scripts.Game.Defined.Serialized.Spells;
using Scripts.Model.Characters;

namespace Scripts.Game.Defined.Characters.StartingSpells {
    public class HeroSpells : SpellBooks {
        public HeroSpells() {
            AddSpellBook(new Attack());
        }
    }
    public class DebugSpells : SpellBooks {
        public DebugSpells() {
            AddSpellBook(new Attack());
            AddSpellBook(new InflictPoison());
            AddSpellBook(new Wait());
        }
    }
}