using System;
using Neutron;
using Riptide;
using UnityEngine;

public class NeutronUnityClient : NeutronClient
{
    public void SpawnObject(string resourcePath, Vector3 position, Quaternion rotation)
    {
        if (String.IsNullOrEmpty(LocalPlayer.InRoomId))
        {
            Debug.LogError("Spawn cannot be sent if the player is not in a room");
            return;
        }

        GameObject toSpawn = Resources.Load<GameObject>(resourcePath);
        var spawned = GameObject.Instantiate(toSpawn, position, rotation);
        var networkObject = spawned.GetComponent<NetworkObject>();
        if (networkObject != null)
        {
            networkObject.player = LocalPlayer;
            networkObject.IsOwner = true;
        }
            
        Message message = Message.Create(MessageSendMode.Reliable, ClientToServerMessageId.SpawnObject);
        message.AddString(resourcePath);
        message.AddVector3(position);
        message.AddQuaternion(rotation);

        _client.Send(message);
    }

    protected void RemoteSpawnObject(Message message)
    {
        if (String.IsNullOrEmpty(LocalPlayer.InRoomId))
        {
            Debug.LogError("Spawn cannot be sent if the player is not in a room");
            return;
        }
            
        var resourcePath = message.GetString();
        var position = message.GetVector3();
        var rotation = message.GetQuaternion();

        GameObject toSpawn = Resources.Load<GameObject>(resourcePath);
        var spawned = GameObject.Instantiate(toSpawn, position, rotation);
        var networkObject = spawned.GetComponent<NetworkObject>();
        if (networkObject != null)
        {
            //TODO player reference
            networkObject.IsOwner = false;
        }
    }
}
