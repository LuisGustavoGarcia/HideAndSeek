using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SortableDynamic : MonoBehaviour
{
    private SpriteRenderer m_sprite;

    void Start()
    {
        m_sprite = gameObject.GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        m_sprite.sortingOrder = (int)(transform.position.y * -110);
    }
}
