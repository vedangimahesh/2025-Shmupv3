using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_Homing : Enemy
{
    public float homingSpeed = 8f;
    public float downwardSpeed = 3f;
    public float rotateSpeed = 5f;

    public override void Move()
    {
        // If hero doesn't exist, just move normally
        if (Hero.S == null)
        {
            base.Move();
            return;
        }

        // Direction toward hero
        Vector3 dir = (Hero.S.transform.position - transform.position).normalized;

        // Move toward hero + downward
        Vector3 move = dir * homingSpeed * Time.deltaTime;
        move += Vector3.down * downwardSpeed * Time.deltaTime;

        pos += move;

        // Rotate toward hero
        Quaternion targetRot = Quaternion.LookRotation(dir, Vector3.back);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotateSpeed * Time.deltaTime);
    }
}