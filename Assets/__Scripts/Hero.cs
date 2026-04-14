using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hero : MonoBehaviour
{
    static public Hero S { get; private set; }

    [Header("Inscribed")]
    public float speed = 30;
    public float rollMult = -45;
    public float pitchMult = 30;
    public GameObject projectilePrefab;
    public float projectileSpeed = 40;
    public Weapon[] weapons;

    [Header("Missile Weapon")]
    public GameObject missilePrefab;
    public float missileCooldown = 1.5f;
    private float lastMissileTime = -10f;

    [Header("Dynamic")]
    [Range(0, 4)]
    [SerializeField]
    private float _shieldLevel = 1;

    private GameObject lastTriggerGo = null;

    public delegate void WeaponFireDelegate();
    public event WeaponFireDelegate fireEvent;

    void Awake()
    {
        if (S == null)
        {
            S = this;
        }
        else
        {
            Debug.LogError("Hero.Awake() - Attempted to assign second Hero.S!");
        }

        ClearWeapons();
        weapons[0].SetType(eWeaponType.blaster);
    }

    void Update()
    {
        float hAxis = Input.GetAxis("Horizontal");
        float vAxis = Input.GetAxis("Vertical");

        Vector3 pos = transform.position;
        pos.x += hAxis * speed * Time.deltaTime;
        pos.y += vAxis * speed * Time.deltaTime;
        transform.position = pos;

        transform.rotation = Quaternion.Euler(vAxis * pitchMult, hAxis * rollMult, 0);

        // Normal weapon (spacebar)
        if (Input.GetAxis("Jump") == 1 && fireEvent != null)
        {
            fireEvent();
        }

        // MISSILE (press M)
        if (Input.GetKeyDown(KeyCode.M))
        {
            TryFireMissile();
        }
    }

    void TryFireMissile()
    {
        if (Time.time - lastMissileTime < missileCooldown) return;

        Enemy[] enemies = FindObjectsOfType<Enemy>();
        if (enemies.Length == 0) return;

        Enemy nearestEnemy = null;
        float bestDist = Mathf.Infinity;

        foreach (Enemy e in enemies)
        {
            float d = Vector3.Distance(transform.position, e.transform.position);
            if (d < bestDist)
            {
                bestDist = d;
                nearestEnemy = e;
            }
        }

        if (nearestEnemy == null) return;

        GameObject missile = Instantiate(missilePrefab, transform.position, Quaternion.identity);
        ProjectileMissile pm = missile.GetComponent<ProjectileMissile>();

        if (pm != null)
        {
            pm.target = nearestEnemy.transform;
            lastMissileTime = Time.time;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        Transform rootT = other.gameObject.transform.root;
        GameObject go = rootT.gameObject;

        if (go == lastTriggerGo) return;
        lastTriggerGo = go;

        Enemy enemy = go.GetComponent<Enemy>();
        PowerUp pUp = go.GetComponent<PowerUp>();

        if (enemy != null)
        {
            shieldLevel--;
            Destroy(go);
        }
        else if (pUp != null)
        {
            AbsorbPowerUp(pUp);
        }
        else
        {
            Debug.LogWarning("Shield trigger hit by non-Enemy: " + go.name);
        }
    }

    public float shieldLevel
    {
        get { return (_shieldLevel); }
        private set
        {
            _shieldLevel = Mathf.Min(value, 4);
            if (value < 0)
            {
                Destroy(this.gameObject);
                Main.HERO_DIED();
            }
        }
    }

    Weapon GetEmptyWeaponSlot()
    {
        for (int i = 0; i < weapons.Length; i++)
        {
            if (weapons[i].type == eWeaponType.none)
            {
                return (weapons[i]);
            }
        }
        return (null);
    }

    void ClearWeapons()
    {
        foreach (Weapon w in weapons)
        {
            w.SetType(eWeaponType.none);
        }
    }

    public void AbsorbPowerUp(PowerUp pUp)
    {
        Debug.Log("Absorbed PowerUp: " + pUp.type);
        switch (pUp.type)
        {
            case eWeaponType.shield:
                shieldLevel++;
                break;

            default:
                if (pUp.type == weapons[0].type)
                {
                    Weapon weap = GetEmptyWeaponSlot();
                    if (weap != null)
                    {
                        weap.SetType(pUp.type);
                    }
                }
                else
                {
                    ClearWeapons();
                    weapons[0].SetType(pUp.type);
                }
                break;
        }
        pUp.AbsorbedBy(this.gameObject);
    }
}