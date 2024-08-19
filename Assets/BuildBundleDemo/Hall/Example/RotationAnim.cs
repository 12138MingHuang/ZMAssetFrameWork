using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotationAnim : MonoBehaviour
{
    public float rotationSpeed;

    private void Update()
    {
        Vector3 angle = transform.localEulerAngles;
        angle.z += Time.deltaTime * rotationSpeed;
        transform.localEulerAngles = angle;
    }
}
