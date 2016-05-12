﻿using UnityEngine;
using System.Collections;

/**
 * This class represents a pair of integer values
 */
public class PairedInt {
    float _false;
    int _true;

    public virtual float False { get { return _false; } set { _false = value; } }
    public virtual int True { get { return _true; } set { _true = value; } }

    //Alternate naming scheme
    public virtual float Percent { get { return False; } set { False = value; } }
    public virtual int Flat { get { return True; } set { True = value; } }

    public PairedInt(int initial) {
        Set(initial);
    }

    public PairedInt(int trueValue, int falseValue) {
        _true = trueValue;
        _false = falseValue;
    }

    void Set(int both) {
        True = both;
        False = both;
    }
}
