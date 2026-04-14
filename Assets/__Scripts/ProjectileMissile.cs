using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileMissile : MonoBehaviour
{
    public float speed = 20f;
    public float rotateSpeed = 8f;
    public float lifeTime = 5f;

    public Transform target;

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        Vector3 dir = (target.position - transform.position).normalized;

        transform.position += dir * speed * Time.deltaTime;

        Quaternion targetRot = Quaternion.LookRotation(dir, Vector3.back);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotateSpeed * Time.deltaTime);
    }

    void OnCollisionEnter(Collision coll)
    {
        if (target != null && coll.gameObject == target.gameObject)
        {
            Destroy(coll.gameObject);
            Destroy(gameObject);
        }
    }
}