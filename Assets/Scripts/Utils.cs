using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utils
{
    public static int IntIncDec(int i, int val, int max, int min = 0) {
        val += i;
        if (val < min) { val = max; } else if (val > max) { val = min; }
        return val;
    }
}
