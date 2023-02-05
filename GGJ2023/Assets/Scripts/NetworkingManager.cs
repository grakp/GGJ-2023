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
};

[System.Serializable]
public class TestPacket : PacketBase
{
    public static byte id = 1;
    public override byte packetId { get { return id; } protected set { packetId = value; }}

    private int magicNumber = 123;
    private string testString = "Asd";

    public static byte[] Serialize(object customType)
    {
        /*
        TestPacket data = (TestPacket)customType;
        
        List<byte[]> byteArrays = new List<byte[]>();

        {
            byte[] bytes = BitConverter.GetBytes(data.magicNumber);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }

            byteArrays.Add(bytes);
        }

        {
            byte[] bytes = BitConverter.GetBytes(data.testString.Length);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }

            byteArrays.Add(bytes);
        }

        {
            byte[] bytes = System.Text.Encoding.ASCII.GetBytes(data.testString);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }

            byteArrays.Add(bytes);
        }

        byte [] result = NetworkingManager.JoinBytes(byteArrays.ToArray());
        return result;
        */
        return NetworkingManager.ObjectToByteArray(customType);
    }

    public static object Deserialize(byte[] data)
    {
        /*
        TestPacket packet = new TestPacket();
        int runningOffset = 0;
        NetworkingManager.Deserialize_Int(data, ref runningOffset, ref packet.magicNumber);
        NetworkingManager.Deserialize_String(data, ref runningOffset, ref packet.testString);

        return packet;
        */
        return NetworkingManager.ByteArrayToObject(data);
    }
};

public class NetworkingManager : MonoBehaviour
{

    public static void Deserialize_Int(byte[] data, ref int offset, ref int result)
    {
        int size = sizeof(int);
        byte[] bytes = new byte[size];
        Array.Copy(data, offset, bytes, 0, bytes.Length);
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(bytes);
        }

        result = BitConverter.ToInt32(bytes, 0);
        offset += size;
    }

    public static void Deserialize_String(byte[] data, ref int offset, ref string result)
    {
        int size = 0;
        NetworkingManager.Deserialize_Int(data, ref offset, ref size);

        byte[] bytes = new byte[size];
        Array.Copy(data, offset, bytes, 0, bytes.Length);
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(bytes);
        }

        result = System.Text.Encoding.UTF8.GetString(bytes);
        offset += size;
    }


    public static int worldSeed = -1;
    public static System.Random worldSeedRandom;

    private Photon.Realtime.RaiseEventOptions sendToMasterClientOptions = new Photon.Realtime.RaiseEventOptions();
    private Photon.Realtime.RaiseEventOptions sendToAllOptions = new Photon.Realtime.RaiseEventOptions();
    private Photon.Realtime.RaiseEventOptions sendToOtherOptions = new Photon.Realtime.RaiseEventOptions();

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

        if (PhotonNetwork.IsConnected)
        {
            sendToMasterClientOptions.Receivers = Photon.Realtime.ReceiverGroup.MasterClient;
            sendToAllOptions.Receivers = Photon.Realtime.ReceiverGroup.All;
            sendToOtherOptions.Receivers = Photon.Realtime.ReceiverGroup.Others;

            PhotonPeer.RegisterType(typeof(TestPacket), TestPacket.id, TestPacket.Serialize, TestPacket.Deserialize);

            PhotonNetwork.NetworkingClient.EventReceived += Network_OnEventReceived;

            TestPacket testPacket = new TestPacket();
            SendPacket(testPacket, Photon.Realtime.ReceiverGroup.Others);
        }
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
        Debug.Log("Data type: " + dataType);

        if (obj.Code == TestPacket.id)
        {
            HandlePacket((TestPacket)obj.CustomData);
        }
        else if (dataType.IsSubclassOf(typeof(PacketBase)))
        {
            Debug.LogWarning("Unhandled event: " + obj.Code + " " + dataType);
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
        Debug.Log("Hey, a packet!!!");
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


}
