using UnityEngine;
using System.Collections.Generic;

public class TreasureSpawner : MonoBehaviour
{
    public GameObject prefabTresor;
    public GameObject prefabFleche;
    public GameObject prefabTeleTrans;
    public GameObject prefabTeleRecep;

    public Vector2Int? tresorPos;
    public List<Vector2Int> teleTrans = new List<Vector2Int>();
    public List<Vector2Int> teleRecep = new List<Vector2Int>();

    private List<GameObject> objetsSpawned = new List<GameObject>();

    public void PlacerObjets(int niveau)
    {
        // Nettoyer les anciens objets
        foreach (var o in objetsSpawned) Destroy(o);
        objetsSpawned.Clear();
        teleTrans.Clear();
        teleRecep.Clear();

        // Collecter les couloirs disponibles
        var couloirs = new List<Vector2Int>();
        for (int r = 0; r < 31; r++)
            for (int c = 0; c < 31; c++)
                if (MazeData.DEDALE[r, c] == MazeData.COULOIR && !(r == 13 && c == 15))
                    couloirs.Add(new Vector2Int(r, c));

        // Mélanger
        for (int i = couloirs.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            var tmp = couloirs[i]; couloirs[i] = couloirs[j]; couloirs[j] = tmp;
        }

        var occupees = new HashSet<Vector2Int>();
        int idx = 0;

        // Trésor
        tresorPos = couloirs[idx++];
        occupees.Add(tresorPos.Value);
        SpawnObjet(prefabTresor, tresorPos.Value, 0.35f);

        // Télé-récepteurs
        int nbRecep = GameManager.Instance.TeleRecepPourNiveau(niveau);
        for (int i = 0; i < nbRecep; i++)
        {
            while (idx < couloirs.Count)
            {
                var pos = couloirs[idx++];
                if (!occupees.Contains(pos))
                {
                    occupees.Add(pos);
                    teleRecep.Add(pos);
                    SpawnObjet(prefabTeleRecep, pos, 0f);
                    break;
                }
            }
        }

        // Télé-transporteurs
        int nbTrans = GameManager.Instance.TeleTransPourNiveau(niveau);
        for (int i = 0; i < nbTrans; i++)
        {
            while (idx < couloirs.Count)
            {
                var pos = couloirs[idx++];
                if (!occupees.Contains(pos))
                {
                    occupees.Add(pos);
                    teleTrans.Add(pos);
                    SpawnObjet(prefabTeleTrans, pos, 0f);
                    break;
                }
            }
        }

        // Flèches
        int nbFl = GameManager.Instance.FlechesPourNiveau(niveau);
        for (int i = 0; i < nbFl; i++)
        {
            while (idx < couloirs.Count)
            {
                var pos = couloirs[idx++];
                if (!occupees.Contains(pos))
                {
                    occupees.Add(pos);
                    SpawnFleche(pos, tresorPos.Value);
                    break;
                }
            }
        }
    }

    void SpawnObjet(GameObject prefab, Vector2Int pos, float hauteur)
    {
        if (prefab == null) return;
        var go = Instantiate(prefab,
            new Vector3(pos.y + 0.5f, hauteur, pos.x + 0.5f),
            Quaternion.identity);
        go.transform.parent = transform;
        objetsSpawned.Add(go);
    }

    void SpawnFleche(Vector2Int pos, Vector2Int tresor)
    {
        if (prefabFleche == null) return;
        float dx = (tresor.y + 0.5f) - (pos.y + 0.5f);
        float dz = (tresor.x + 0.5f) - (pos.x + 0.5f);
        float angle = Mathf.Atan2(dx, dz) * Mathf.Rad2Deg;

        float HAUTEUR_MUR = 1.5f;
        var go = Instantiate(prefabFleche,
            new Vector3(pos.y + 0.5f, HAUTEUR_MUR - 0.15f, pos.x + 0.5f),
            Quaternion.Euler(0, angle, 0));
            go.transform.parent = transform;
            objetsSpawned.Add(go);

        foreach (var col in go.GetComponentsInChildren<Collider>())
            col.enabled = false;
    }
}