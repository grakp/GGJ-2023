using System.Collections;
using System.Collections.Generic;
using System;


public static class IListExtensions {
    /// <summary>
    /// Shuffles the element order of the specified list.
    /// </summary>
    //public static void Shuffle<T>(this IList<T> ts, System.Random pseudoRandom = null) {

    // Make sure psueudoRandom seed to ensure that we use world seed
    public static void Shuffle<T>(this IList<T> ts, System.Random pseudoRandom) {
        int count = ts.Count;
        int last = count - 1;
        for (int i = 0; i < last; ++i) {
            int r;
            if (pseudoRandom != null)
            {
                r = pseudoRandom.Next(i, count);
            }
            else
            {
                r = UnityEngine.Random.Range(i, count);
            }
            
            var tmp = ts[i];
            ts[i] = ts[r];
            ts[r] = tmp;
        }
    }
}