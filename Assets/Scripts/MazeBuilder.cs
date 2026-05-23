using UnityEngine;
using System.Collections.Generic;

public class MazeBuilder : MonoBehaviour
{
    public float HAUTEUR_MUR = 1.5f;

    public Material matMurNon;
    public Material matMurOuv;
    public Material matPlancher;
    public Material matPlancherEnc;
    public Material matPlafond;

    private List<GameObject> mursActifs = new List<GameObject>();

    void Start()
    {
        ConstruirePlancher();  
        ConstruireMurs();
    }

    public void ConstruireMurs()
    {
        // Détruire les anciens murs
        foreach (var m in mursActifs) Destroy(m);
        mursActifs.Clear();

        for (int row = 0; row < 31; row++)
        {
            for (int col = 0; col < 31; col++)
            {
                int type = MazeData.DEDALE[row, col];

                if (type == MazeData.MUR_NON || type == MazeData.ENCLOS)
                {
                    bool afficher = type == MazeData.MUR_NON;
                    if (afficher) CreerMur(row, col, matMurNon);
                }
                else if (type == MazeData.MUR_OUV)
                {
                    if (!MazeData.DEDALE_OUVERT[row, col])
                        CreerMur(row, col, matMurOuv);
                }
            }
        }
    }

    void CreerMur(int row, int col, Material mat)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.transform.position = new Vector3(col + 0.5f, HAUTEUR_MUR / 2f, row + 0.5f);

        go.transform.localScale = new Vector3(1.05f, HAUTEUR_MUR, 1.05f);
        
        go.GetComponent<Renderer>().material = mat;
        go.transform.parent = transform;
        mursActifs.Add(go);
    }

    public void ConstruirePlancher()
    {
        for (int row = 0; row < 31; row++)
        {
            for (int col = 0; col < 31; col++)
            {
                var go = GameObject.CreatePrimitive(PrimitiveType.Plane);
                go.transform.position = new Vector3(col + 0.5f, 0f, row + 0.5f);
                go.transform.localScale = new Vector3(0.1f, 1f, 0.1f);
                bool estEnclos = MazeData.DEDALE[row, col] == MazeData.ENCLOS;
                go.GetComponent<Renderer>().material = estEnclos ? matPlancherEnc : matPlancher;
                go.transform.parent = transform;
                Destroy(go.GetComponent<Collider>());
            }
        }

        // Plafond
        var plafond = GameObject.CreatePrimitive(PrimitiveType.Plane);
        plafond.transform.position = new Vector3(15.5f, HAUTEUR_MUR, 15.5f);
        plafond.transform.localScale = new Vector3(3.1f, 1f, 3.1f);
        plafond.transform.rotation = Quaternion.Euler(180, 0, 0);
        plafond.GetComponent<Renderer>().material = matPlafond;
        plafond.transform.parent = transform;
        Destroy(plafond.GetComponent<Collider>());
    }
}