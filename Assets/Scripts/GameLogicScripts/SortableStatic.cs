using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SortableStatic : MonoBehaviour
{
    void Start()
    {
        SpriteRenderer sprite = gameObject.GetComponent<SpriteRenderer>();
        sprite.sortingOrder = (int)(transform.position.y * -100);
        Destroy(this);
    }
}
