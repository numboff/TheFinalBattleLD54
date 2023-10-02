using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEndCheck : MonoBehaviour
{
    public GameObject player;
    public GameObject enemy;
    public GameObject screen;

    // Update is called once per frame
    void Update()
    {

        if(player.GetComponent<PlayerControls>().isDead || enemy.GetComponent<Enemy>().isDead)
        {
            screen.SetActive(true);
        }
    }
}
