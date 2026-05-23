using UnityEngine;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    [System.Serializable]
    public class Son { public string nom; public AudioClip clip; }

    public Son[] sons;
    private Dictionary<string, AudioSource> sources = new Dictionary<string, AudioSource>();

    void Start()
    {
        foreach (var s in sons)
        {
            var src = gameObject.AddComponent<AudioSource>();
            src.clip = s.clip;
            sources[s.nom] = src;
        }
    }

    public void Jouer(string nom)
    {
        if (sources.ContainsKey(nom)) sources[nom].Play();
    }
}