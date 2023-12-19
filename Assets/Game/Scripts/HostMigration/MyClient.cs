using Mirror;
using Mirror.Examples.Tanks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyClient : NetworkBehaviour
{
    private Tank _tank;

    private void Start()
    {
        _tank = GetComponent<Tank>();

        if (isServer) 
        {
            //if you are server and disconnect you want to set this true so you dont initiate a rejoining phase
            MyNetworkManager.disconnectGracefully = true;
        }

        if (!isOwned) return;

        RemakeGame();
    }

    [ClientRpc]
    public void StoreNewHostData(HostData hostData) 
    {
        //storing new hostData just incase current host leaves
        MyNetworkManager.backUpHostData = hostData;

        //checking if this player is new host if not set false
        if (hostData.netID == netId && isLocalPlayer)
            MyNetworkManager.isNewHost = true;
        else
            MyNetworkManager.isNewHost = false;
    }

    private void RemakeGame() 
    {
        //Check if there is previous data if so reinitialize states to continue
        if (MyNetworkManager.myPlayerData.health != 0) 
        {
            transform.position = MyNetworkManager.myPlayerData.pos;
            transform.rotation = MyNetworkManager.myPlayerData.rot;
            SetHealth(MyNetworkManager.myPlayerData.health);
        }
    }

    //THIS IS NOT SAFE, BUT FOR EXPERIMENTING PURPOSES WHO CARES
    [Command]
    void SetHealth(int value) 
    {
        _tank.health = value;
    }

    private void OnDestroy()
    {
        //when you are about to be destroyed save your data to be reused on new host
        if (isLocalPlayer) 
        {
            MyNetworkManager.myPlayerData = new PlayerData(transform.position, transform.rotation, GetComponent<Tank>().health);
        }
    }
}
