using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawn : MonoBehaviour
{
    [Header("Spawn Object")]
    public GameObject obj;
    public Transform spawnposisi;
    public Transform target;

    [Header("Spawn Setting")]
    public float spawninterval = 3f;
    public float spawndelay = 2f;


    private void Start()
    {
        //digunakan untuk looping fucntion
        //3 bagian
        //1. fucntion,2.waktudelay, 3.waktu sebelum dipanggil
        InvokeRepeating(nameof(Spawner), spawndelay, spawninterval);
    }


    void Spawner()
    {
        Vector2 direction = new Vector2(target.position.x + 5, target.position.y);
        //1.object 2.posisi 3. rotasi
        Instantiate(obj, direction, Quaternion.identity);
    }

}

 