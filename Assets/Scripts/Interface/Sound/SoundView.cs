﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SoundView : MonoBehaviour {
    public static SoundView Instance { get; private set; }
    public IDictionary<string, AudioSource> Sound { get; private set; }

    void Awake() {
        Instance = this;
        this.Sound = new Dictionary<string, AudioSource>();
    }

    public void Play(string resourceLocation) {
        Util.Assert(Resources.Load(resourceLocation) != null);
        if (!Sound.ContainsKey(resourceLocation)) {
            AudioSource audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.clip = Resources.Load<AudioClip>(resourceLocation);
            Sound.Add(resourceLocation, audioSource);
        }
        Sound[resourceLocation].Play();
    }
}