﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/**
 * This class represents the text on the top of the screen
 * You can set/show/hide the location, chapter, and main quest blurb here
 */
public class HeaderView : MonoBehaviour {
    public static HeaderView Instance { get; private set; }

    Text location;
    Text chapter;
    Text mainQuestBlurb;

    // Use this for initialization
    void Start() {
        Instance = this;
        location = GameObject.Find("Location").GetComponent<Text>();
        chapter = GameObject.Find("Chapter").GetComponent<Text>();
        mainQuestBlurb = GameObject.Find("MainQuestBlurb").GetComponent<Text>();
    }

    // Update is called once per frame
    void Update() {

    }

    public void SetLocation(string loc) {
        location.text = loc;
    }

    public void EnableLocation(bool enable) {
        location.enabled = enable;
    }

    public void SetChapter(string chapterText) {
        chapter.text = chapterText;
    }

    public void EnableChapter(bool enable) {
        chapter.enabled = enable;
    }

    public void SetBlurb(string blurb) {
        mainQuestBlurb.text = blurb;
    }

    public void EnableBlurb(bool enable) {
        mainQuestBlurb.enabled = enable;
    }
}