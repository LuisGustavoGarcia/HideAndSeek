using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformSprites : MonoBehaviour
{
    public static TransformSprites Singleton = null;

    [SerializeField] private List<Sprite> m_sprites;
    [SerializeField] private List<string> m_names;
    public Dictionary<string, Sprite> m_spriteDictionary;

    private void Awake()
    {
        m_spriteDictionary = new Dictionary<string, Sprite>();
        if (Singleton == null)
        {
            Singleton = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else if (Singleton != this)
        {
            Destroy(gameObject);
        }

        // Initialize sprite dictionary.
        if (m_sprites.Count != m_names.Count)
        {
            Debug.Log("Error: Sprites + SpriteNames are not of the same length");
        }
        else
        {
            m_spriteDictionary.Add("PLAYER", null);
            for (int i = 0; i < m_names.Count; i++)
            {
                m_spriteDictionary.Add(m_names[i], m_sprites[i]);
            }
        }
    }

    public Sprite GetObjectSprite(string name)
    {
        return m_spriteDictionary[name];
    }

    public List<string> GetAllTransformSpriteNames()
    {
        return m_names;
    }
}
