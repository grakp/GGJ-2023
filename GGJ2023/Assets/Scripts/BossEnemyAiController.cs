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
    public float damageDealt = 1;

    void Start()
    {
        currentSize = minSize;
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
            currentSize += speed * Time.deltaTime;
        }
        else
        {
            currentSize -= speed * Time.deltaTime;
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

        if (isIncreasing && currentSize >= maxSize)
        {
            isIncreasing = false;
        }
        else if (!isIncreasing && currentSize <= minSize)
        {
            isIncreasing = true;
        }

        currentHitboxInterval += Time.deltaTime;

        if (currentHitboxInterval >= hitboxInterval)
        {
            currentHitboxInterval = 0;
            foreach (BaseUnit unit in magicCircle.collidedUnits)
            {
                unit.DoTakeDamage(1, selfUnit);
            }
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
