using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class BossEnemyAiController : AiControllerBase
{
    public BaseUnit selfUnit;
    public BossMagicCircle magicCircle;
    public Transform MAGIC_CIRCLE_OF_DARKNESS;

    public float speed = 1;
    private float currentSpeed = 1;

    public float minSize = 20.0f;
    public float maxSize = 50.0f;

    public Transform MAGIC_CIRCLE_STARS1;
    public Transform MAGIC_CIRCLE_STARS2;

    public Transform MAGIC_CIRCLE_STARS3;
    public Transform MAGIC_CIRCLE_STARS4;

    public float rotationSpeed;

    float lastSize = 0;


    float currentSize = 0;

    bool isIncreasing = true;


    public float hitboxInterval = 0.1f;
    private float currentHitboxInterval = 0;
    private int damageDealt = 1;

    int phase = 0;

    int originalHealth;

    float maxSizeToUse = 50.0f;
    private float currentMinSize = 10.0f;

    void Start()
    {
        currentSpeed = speed;
        currentSize = minSize;
        currentMinSize = minSize;
        originalHealth = selfUnit.health;
        maxSizeToUse = maxSize;
    }

    void Update()
    {
        /*
        float size = Mathf.Sin(Time.time * speed) * currentAmp;
        MAGIC_CIRCLE_OF_DARKNESS.transform.localScale = new Vector3(size, size, MAGIC_CIRCLE_OF_DARKNESS.transform.localScale.z);

      

        if (size > lastSize && !hasIncreasedAmp)
        {
            currentAmp += ampIncreaseRate;
            currentAmp = Mathf.Clamp(currentAmp, initialAmp, maxAmp);
            Debug.Log("Increase size: " + currentAmp);

            hasIncreasedAmp = true;
        }
        else if (size < lastSize && hasIncreasedAmp)
        {
            hasIncreasedAmp = false;
        }

        lastSize = size;
*/

        if (isIncreasing)
        {
            currentSize += currentSpeed * Time.deltaTime;
        }
        else
        {
            currentSize -= currentSpeed * Time.deltaTime;
        }

        MAGIC_CIRCLE_OF_DARKNESS.transform.localScale = new Vector3(currentSize, currentSize, MAGIC_CIRCLE_OF_DARKNESS.transform.localScale.z);

        Vector3 eulerRotation1 = MAGIC_CIRCLE_STARS1.transform.rotation.eulerAngles;
        eulerRotation1.z += rotationSpeed * Time.deltaTime;
        MAGIC_CIRCLE_STARS1.transform.rotation = Quaternion.Euler(eulerRotation1);


        Vector3 eulerRotation2 = MAGIC_CIRCLE_STARS2.transform.rotation.eulerAngles;
        eulerRotation2.z += -rotationSpeed * Time.deltaTime;
        MAGIC_CIRCLE_STARS2.transform.rotation = Quaternion.Euler(eulerRotation2);

        Vector3 eulerRotation3 = MAGIC_CIRCLE_STARS3.transform.rotation.eulerAngles;
        eulerRotation3.z += -rotationSpeed * Time.deltaTime;
        MAGIC_CIRCLE_STARS3.transform.rotation = Quaternion.Euler(eulerRotation3);

        Vector3 eulerRotation4 = MAGIC_CIRCLE_STARS4.transform.rotation.eulerAngles;
        eulerRotation4.z += -rotationSpeed * Time.deltaTime;
        MAGIC_CIRCLE_STARS4.transform.rotation = Quaternion.Euler(eulerRotation3);

        if (isIncreasing && currentSize >= maxSizeToUse)
        {
            isIncreasing = false;
        }
        else if (!isIncreasing && currentSize <= currentMinSize)
        {
            isIncreasing = true;
        }

        currentHitboxInterval += Time.deltaTime;

        if (currentHitboxInterval >= hitboxInterval)
        {
            currentHitboxInterval = 0;
            foreach (BaseUnit unit in magicCircle.collidedUnits)
            {
                unit.RequestTakeDamage(damageDealt, selfUnit);
            }
        }

        int health = selfUnit.health;
        float healthPercent = (float)health / (float)originalHealth;
        
        if (healthPercent < 0.75f && phase == 0)
        {
            currentSpeed = speed * 2.0f;
            currentMinSize = minSize + minSize * 0.5f;
            maxSizeToUse = maxSize + maxSize * 0.5f;
            phase++;
        }
        else if (healthPercent < 0.5f && phase == 1)
        {
            currentSpeed = speed * 5.0f;
            currentMinSize = minSize + minSize * 1f;
            maxSizeToUse = maxSize + maxSize * 1f;
            phase++;
        }
        else if (healthPercent < 0.25f && phase == 2)
        {
            damageDealt = 2;
            currentSpeed = speed * 20.0f;
            maxSizeToUse = maxSize + maxSize * 2f;
            currentMinSize = minSize + minSize * 2f;

            phase++;
        }

    }

    public override void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        // Probably don't need to do anything here since it's static :)
        //throw new System.NotImplementedException();
    }

    void DoRotations()
    {


    }

}
