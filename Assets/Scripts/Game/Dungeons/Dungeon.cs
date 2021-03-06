﻿using Scripts.Game.Defined.Characters;
using Scripts.Game.Serialized;
using Scripts.Model.Characters;
using Scripts.Model.Interfaces;
using Scripts.Model.Pages;
using Scripts.Model.Processes;
using Scripts.Model.TextBoxes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Scripts.Game.Dungeons {
    /// <summary>
    /// Dungeons are pagegroups with multiple battle encounters.
    /// </summary>
    /// <seealso cref="Scripts.Model.Pages.PageGroup" />
    /// <seealso cref="Scripts.Model.Interfaces.IButtonable" />
    /// <seealso cref="Scripts.Game.Dungeons.IStageable" />
    public class Dungeon : PageGroup, IButtonable, IStageable {
        /// <summary>
        /// The encounter generator. Creates the encounters for the dungeon.
        /// </summary>
        private readonly Func<Encounter[]> encounterGenerator;

        /// <summary>
        /// Action to be performed on a dungeon clear.
        /// </summary>
        private readonly Action onClear;

        /// <summary>
        /// Initializes a new instance of the <see cref="Dungeon" /> class.
        /// </summary>
        /// <param name="party">The party to enter the dungeon.</param>
        /// <param name="defeat">The page to go to if the party is defeated.</param>
        /// <param name="previous">The previous page, used to back out of the dungeon.</param>
        /// <param name="destination">The destination, where finishing the dungeon will lead to.</param>
        /// <param name="name">The name of the dungeon.</param>
        /// <param name="description">The description of the dungeon.</param>
        /// <param name="encounterGenerator">The encounter generator, used to generate dungeon encounters.</param>
        /// <param name="onClear">The on clear.</param>
        public Dungeon(
                    IEnumerable<Character> party,
                    Page defeat,
                    Page previous,
                    Page destination,
                    string name,
                    string description,
                    Func<Encounter[]> encounterGenerator,
                    Action onClear) : base(new Page(name)) {
            Root.Body = description;
            this.encounterGenerator = encounterGenerator;
            this.onClear = onClear;
            Root.AddCharacters(Side.LEFT, party);

            Root.Actions = new IButtonable[] {
                PageUtil.GenerateBack(previous),
                GetDungeonEnterProcess(party, defeat, destination, onClear)
            };
        }

        /// <summary>
        /// Gets the dungeon enter process, which is the final confirmation button before delving
        /// into the dungeon.
        /// </summary>
        /// <param name="party">The party entering the dungeon.</param>
        /// <param name="defeat">The defeat page.</param>
        /// <param name="destination">The destination.</param>
        /// <param name="onClear">What to be called if the dungeon is cleared.</param>
        /// <returns></returns>
        private Process GetDungeonEnterProcess(IEnumerable<Character> party, Page defeat, Page destination, Action onClear) {
            Encounter[] encounters = encounterGenerator();
            Battle[] battles = new Battle[encounters.Length];

            for (int i = battles.Length - 1; i >= 0; i--) {
                Encounter encounter = encounters[i];

                encounter.Enemies.Shuffle();
                Page victoryDestination = null;
                if (i == battles.Length - 1) {
                    Page results = GetResults(destination, battles);
                    results.OnEnter += onClear;
                    victoryDestination = results;
                } else {
                    victoryDestination = battles[i + 1];
                }
                battles[i] = new Battle(defeat, victoryDestination, encounter.Music, string.Format("{0} - {1}", Root.Location, i), party, encounter.Enemies);
            }
            return new Process(
                    "Enter",
                    Util.GetSprite("dungeon-gate"),
                    "Enter this stage.",
                    () => {
                        battles[0].Invoke();
                    }
                );
        }

        /// <summary>
        /// Gets the results of the dungeon.
        /// </summary>
        /// <param name="destination">The destination to go to after leaving the results page.</param>
        /// <param name="battles">The battles in the dungeon to track.</param>
        /// <returns></returns>
        private Page GetResults(Page destination, Battle[] battles) {
            Page results = new Page(string.Format("{0} - {1}", Root.Location, "Results"));
            results.Actions = new IButtonable[] { destination };
            results.OnEnter = () => {
                results.AddText(new TextBox(string.Format(
                    "{0} was cleared in {1} turns.\nTotal experience gained: {1}.",
                      Root.Location,
                      battles.Sum(b => b.TurnCount + 1),
                      battles.Sum(b => b.ExperienceGiven)
                      )));
            };
            return results;
        }

        /// <summary>
        /// Gets a dungeon from this class.
        /// </summary>
        /// <param name="dungeonIndex">Index of the dungeon.</param>
        /// <param name="areaDungeonCount">The number of dungeons in the area.</param>
        /// <param name="flags">The current game's flags.</param>
        /// <param name="party">The party who will traverse the dungeon.</param>
        /// <param name="camp">The camp to return to on defeat and victory.</param>
        /// <param name="quests">The quests page to back out of the dungeon.</param>
        /// <param name="area">The are this dungeon is in.</param>
        /// <returns></returns>
        Dungeon IStageable.GetStageDungeon(int dungeonIndex, int areaDungeonCount, Flags flags, IEnumerable<Character> party, Page camp, Page quests, AreaType area) {
            return this;
        }
    }
}
