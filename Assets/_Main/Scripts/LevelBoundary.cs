using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelBoundary : MonoBehaviour
{
    [SerializeField] private Transform spiderMan;
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            spiderMan.transform.position = new Vector3(0, 8, -2.2f);
        }
    }
}
