using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundPlane : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        if (transform.position.y > 5)
        {
            gameObject.SetActive(false);
        }
    }

}
