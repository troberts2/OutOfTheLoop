using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiplierPointer : MonoBehaviour
{
    public Transform target;

    private SpriteRenderer pointer;

    private void Start()
    {
        pointer = transform.GetChild(0).GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        GameManager.OnGameReset += OnGameReset;
    }

    private void OnDisable()
    {
        GameManager.OnGameReset -= OnGameReset;
    }

    private void OnGameReset()
    {
        target = null;
        if(pointer != null)
        {
            pointer.enabled = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (pointer == null) return;

        if (target == null || !target.GetComponentInChildren<SpriteRenderer>().enabled)
        {
            pointer.enabled = false;
            return;
        }
        else
        {
            float dist = Vector2.Distance(target.position, transform.position);
            if(dist > 2f)
            {
                pointer.enabled = true;
            }
            else
            {
                pointer.enabled = false;
            }     
        }

        // Get direction from this object to the target
        Vector2 dir = target.position - transform.position;

        // Calculate the angle (atan2 gives radians, convert to degrees)
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        // Rotate so the RIGHT vector (local X) points at the target
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }
}
