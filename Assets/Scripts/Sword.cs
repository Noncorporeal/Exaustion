using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sword : MonoBehaviour
{
    [System.NonSerialized]
    public Quaternion UpDownRot;
    [System.NonSerialized]
    public Quaternion SideRot;
    [System.NonSerialized]
    public Vector3 RightPos;
    [System.NonSerialized]
    public Vector3 LeftPos;
    [System.NonSerialized]
    public Vector3 UpPos;
    [System.NonSerialized]
    public Vector3 DownPos;

    void Start()
    {
        UpDownRot = new Quaternion(0, 0, 0, 0);
        SideRot = new Quaternion(0, 0, 90, 0);
        RightPos = new Vector3(-0.15f, -0.05f, 0.1f);
        LeftPos = new Vector3(0.15f, -0.05f, 0.1f);
        UpPos = new Vector3(0, -0.2f, -0.1f);
        DownPos = new Vector3(0, 0.1f, 0.1f);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        collision.transform.GetComponent<Zombie>().Damage();
    }
}
