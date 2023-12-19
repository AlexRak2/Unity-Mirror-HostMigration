using Mirror;
using Mirror.Examples.Tanks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// this is the host data each player will store, so the address, which for us i hardcoded as localhost, but for steam would probably be the players steamid
[System.Serializable]
public struct HostData
{
    public string address;
    public uint netID;
    public HostData(string address, uint netID)
    {
        this.address = address;
        this.netID = netID;
    }
}

//This will be any data you want to synchronize during host migration, so for us we want to restore positions, rotations and players health.
[System.Serializable]
public struct PlayerData 
{
    public Vector3 pos;
    public Quaternion rot;
    public int health;

    public PlayerData(Vector3 pos, Quaternion rot, int health)
    {
        this.pos = pos;
        this.rot = rot;
        this.health = health;
    }
}

public class MyNetworkManager : NetworkManager
{
    //Disconnect only if you are host, if not this will be false and you will join next lobby
    public static bool disconnectGracefully = false;
    //If new host, so you start new lobby
    public static bool isNewHost = false;

    //this is set by localclient, so once leaving this will be stored
    public static PlayerData myPlayerData;
    //also stored by localclient everytime a new client joins
    public static HostData backUpHostData;

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        base.OnServerAddPlayer(conn);
        SetBackUpHost();
    }

    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        base.OnServerDisconnect(conn);
    }

    public override void OnClientDisconnect()
    {
        if (!disconnectGracefully)
        {
            StartCoroutine(HostMigrate());
        }
        else 
        {
            //clear the data if not rejoining a game
            myPlayerData = new PlayerData();
        }

        base.OnClientDisconnect();
    }

    void SetBackUpHost()
    {
        //this is a proof of concept, you will have to check which is the best host, but for now im just getting a random one.
        NetworkConnectionToClient randomHost = GetNextHost();

        if (randomHost == null) return;

        //once found send to each client to store
        HostData newHost = new HostData("localhost", randomHost.identity.netId);
        randomHost.identity.GetComponent<MyClient>().StoreNewHostData(newHost);
    }

    IEnumerator HostMigrate()
    {
        //if new host, start host
        if (isNewHost)
        {
            //these delays can be played with, i was told we have to wait x amount of frames before attempting to start
            yield return new WaitForSeconds(0.3f);
            StartHost();
            Debug.Log("new host");

        }
        else 
        {
            //these delays can be played with, i was told we have to wait x amount of frames before attempting to start
            yield return new WaitForSeconds(0.6f);

            //i not new host, set hostaddress to backup and initialize joiining
            networkAddress = backUpHostData.address;
            StartClient();
            Debug.Log("new client");

        }

        yield return null;
    }


    NetworkConnectionToClient GetNextHost()
    {
        foreach (NetworkConnectionToClient conn in NetworkServer.connections.Values)
        {
            if (conn.identity.isLocalPlayer) continue;

            return conn;
        }

        return null;
    }
}
