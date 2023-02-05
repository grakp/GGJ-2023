using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using ExitGames.Client.Photon;
using Photon.Pun;
using System.Linq;

using UnityEngine;

[System.Serializable]
public abstract class PacketBase
{
    public abstract byte packetId { get; protected set; }

    // Convert an object to a byte array
    public static byte[] ObjectToByteArray(object obj)
    {
        BinaryFormatter bf = new BinaryFormatter();
        using (var ms = new MemoryStream())
        {
            bf.Serialize(ms, obj);
            return ms.ToArray();
        }
    }

    // Convert a byte array to an Object
    public static object ByteArrayToObject(byte[] arrBytes)
    {
        using (var memStream = new MemoryStream())
        {
            var binForm = new BinaryFormatter();
            memStream.Write(arrBytes, 0, arrBytes.Length);
            memStream.Seek(0, SeekOrigin.Begin);
            var obj = binForm.Deserialize(memStream);
            return obj;
        }
    }

    public static byte[] Serialize(object customType)
    {
        return ObjectToByteArray(customType);
    }

    public static object Deserialize(byte[] data)
    {
        return ByteArrayToObject(data);
    }
};

[System.Serializable]
public class TestPacket : PacketBase
{
    public const byte id = 1;
    public override byte packetId { get { return id; } protected set { packetId = value; }}

    public int magicNumber = 123;
    public string testString = "Asd";
};

[System.Serializable]
public class SetWorldSeedPacket : PacketBase
{
    public const byte id = 2;
    public override byte packetId { get { return id; } protected set { packetId = value; }}
    public int worldSeed;
};

public class NetworkingManager : MonoBehaviour
{
    public static int worldSeed = -1;
    public static System.Random worldSeedRandom = null;

    private Photon.Realtime.RaiseEventOptions sendToMasterClientOptions = new Photon.Realtime.RaiseEventOptions();
    private Photon.Realtime.RaiseEventOptions sendToAllOptions = new Photon.Realtime.RaiseEventOptions();
    private Photon.Realtime.RaiseEventOptions sendToOtherOptions = new Photon.Realtime.RaiseEventOptions();

    public bool IsDebuggingMode {get; private set;}

    private bool hasInitialized = false;

    public void Initialize()
    {
        if (hasInitialized)
        {
            return;
        }

        if (PhotonNetwork.IsMasterClient)
        {
            worldSeed = Time.time.ToString().GetHashCode();
            worldSeedRandom = new System.Random(worldSeed.GetHashCode());
        }

        sendToMasterClientOptions.Receivers = Photon.Realtime.ReceiverGroup.MasterClient;
        sendToAllOptions.Receivers = Photon.Realtime.ReceiverGroup.All;
        sendToOtherOptions.Receivers = Photon.Realtime.ReceiverGroup.Others;

        PhotonPeer.RegisterType(typeof(TestPacket), TestPacket.id, TestPacket.Serialize, TestPacket.Deserialize);
        PhotonPeer.RegisterType(typeof(SetWorldSeedPacket), SetWorldSeedPacket.id, SetWorldSeedPacket.Serialize, SetWorldSeedPacket.Deserialize);

        PhotonNetwork.NetworkingClient.EventReceived += Network_OnEventReceived;

    
        TestPacket testPacket = new TestPacket();
        SendPacket(testPacket, Photon.Realtime.ReceiverGroup.All);

        hasInitialized = true;
    }

    public void SetDebuggingMode(bool set)
    {
        IsDebuggingMode = set;
        if (set == true && !hasInitialized)
        {
            Debug.Log("Testing game without network. World seed will not be replicated");
            Initialize();
        }
    }

    public bool HasSetWorldSeed()
    {
        return worldSeed != -1 && worldSeedRandom != null;
    }


    public void OnDestroy()
    {
        PhotonNetwork.NetworkingClient.EventReceived -= Network_OnEventReceived;
    }

    public void Network_OnEventReceived(EventData obj)
    {
        if (obj == null || obj.CustomData == null)
        {
            return;
        }

        System.Type dataType = obj.CustomData.GetType();
        switch (obj.Code)
        {
            case TestPacket.id:
                HandlePacket((TestPacket)obj.CustomData);
                Debug.Log("Sender: " + obj.Sender.ToString());
                break;
            case SetWorldSeedPacket.id:
                HandlePacket((SetWorldSeedPacket)obj.CustomData);
                break;
            default:
                if (dataType.IsSubclassOf(typeof(PacketBase)))
                {
                    Debug.LogError("Unhandled event: " + obj.Code + " " + dataType);
                }
                break;
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


    private Photon.Realtime.RaiseEventOptions GetReceiverOptions(Photon.Realtime.ReceiverGroup receiver)
    {
        switch (receiver)
        {
            case Photon.Realtime.ReceiverGroup.All:
                return sendToAllOptions;
            case Photon.Realtime.ReceiverGroup.MasterClient:
                return sendToMasterClientOptions;
            case Photon.Realtime.ReceiverGroup.Others:
                return sendToOtherOptions;
        }

        return sendToOtherOptions;
    }

    public void SendPacket(PacketBase packet, Photon.Realtime.ReceiverGroup receiver)
    {
        Photon.Realtime.RaiseEventOptions option = GetReceiverOptions(receiver);
        PhotonNetwork.RaiseEvent(packet.packetId, packet, option, ExitGames.Client.Photon.SendOptions.SendReliable);
    }

    public void HandlePacket(TestPacket packet)
    {
        Debug.Log("Hey, a packet!!!: " + packet.magicNumber + " " + packet.testString);
    }

    public void HandlePacket(SetWorldSeedPacket packet)
    {
        CheckClientOnly(packet);
        worldSeed = packet.worldSeed;
        worldSeedRandom = new System.Random(worldSeed.GetHashCode());
        Debug.Log("Set world seed packet: " + worldSeed);
    }


    public void CheckServerOnly(PacketBase packet)
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            Debug.LogError("Should not be called on the client: " + packet.GetType().Name);
        }
    }

    public void CheckClientOnly(PacketBase packet)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            Debug.LogError("Should not be called on the server: " + packet.GetType().Name);
        }
    }

    public static byte[] JoinBytes(params byte[][] arrays)
    {
        byte[] rv = new byte[arrays.Sum(arrays => arrays.Length)];
        int offset = 0;
        foreach (byte[] array in arrays)
        {
           System.Buffer.BlockCopy(array, 0, rv, offset, array.Length);
           offset += array.Length;
        }

        return rv;
    }



}
