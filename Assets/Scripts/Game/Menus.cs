﻿using Scripts.Model.Interfaces;
using Scripts.Model.Pages;
using Scripts.Model.Processes;
using Scripts.Model.Characters;
using Scripts.Presenter;
using UnityEngine;
using Scripts.View.Effects;
using Scripts.View.ObjectPool;
using Scripts.Game.Defined.Characters;
using System;
using Scripts.Game.Defined.Spells;

namespace Scripts.Game.Pages {

    public class Menus {
        public Page Start;
        public Page Debug;

        public Menus() {
            Start = new Page("Start Menu");
            Debug = new Page("Debug");

            // Start page
            Start.Actions = Util.GetArray(
                new Tuple(1, Debug),
                new Tuple(11, new Process("Exit", () => Application.Quit()))
                );

            // Debug page
            Grid submenu = new Grid("Go to submenu");
            Grid mainDebug = new Grid("Return to main menu");

            Character kitsune = new Kitsune();

            mainDebug.Array = new IButtonable[] {
                Start,
                new Process("Say", "Hello", () => Presenter.Main.Instance.TextBoxes.AddTextBox(new Model.TextBoxes.TextBox("Hello"))),
                new Process("AttDisb", () => Presenter.Main.Instance.TextBoxes.AddTextBox(new Model.TextBoxes.TextBox(kitsune.Stats.AttributeDistribution))),
                new Process("hitsplat test", () => {
                    HitsplatView hpv = ObjectPoolManager.Instance.Get(EffectsManager.Instance.Hitsplat);
                    Util.Parent(hpv.gameObject, kitsune.Presenter.PortraitView.EffectsHolder);
                    Presenter.Main.Instance.StartCoroutine(hpv.Animation("Test", Color.cyan, Util.GetSprite("fox-head")));
                }),
                new Battle("Battle Test", new Character[] { new Kitsune(), new Kitsune()  }, new Character[] { new Kitsune(), new Kitsune() }),
                new Process("UNcastables", () => {

                    }),
                submenu
            };

            submenu.Array = new IButtonable[] {
                new Process("Say hello", () => Presenter.Main.Instance.TextBoxes.AddTextBox(new Model.TextBoxes.TextBox("Hello"))),
                mainDebug
            };

            Debug.AddCharacters(false, kitsune);
            Debug.Actions = mainDebug.Array;
        }
    }

}