using System.Collections;
using System.Collections.Generic;
using Photon.Pun;

using UnityEngine;


public class NetworkingManager : MonoBehaviour
{

    public static int worldSeed = -1;
    public static System.Random worldSeedRandom;

    public void Initialize()
    {
        if (PhotonNetwork.IsMasterClient || !PhotonNetwork.IsConnected)
        {
            worldSeed = Time.time.ToString().GetHashCode();
            worldSeedRandom = new System.Random(worldSeed.GetHashCode());

            if (!PhotonNetwork.IsConnected)
            {
                Debug.Log("Network is not connected. World seed will not be replicated");
            }
        }
        
    }

    public bool WorldSeedIsSet()
    {
        return worldSeedRandom != null;
    }

    public static int RandomRangeUsingWorldSeed(int min, int max)
    {
        return worldSeedRandom.Next(min, max);
    }

    public static float RandomRangeUsingWorldSeed(float min, float max)
    {
        return NextFloat(worldSeedRandom) * (max - min) + min;
    }

    // https://stackoverflow.com/questions/3365337/best-way-to-generate-a-random-float-in-c-sharp
    public static float NextFloat(System.Random random)
    {
        return (float)(float.MaxValue * 2.0 * (random.NextDouble()-0.5));
    }

}
