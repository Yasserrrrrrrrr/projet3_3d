using UnityEngine;
using TMPro;
using System.Collections;

public class HUDManager : MonoBehaviour
{
    public TextMeshProUGUI txtNiveau, txtScore, txtTemps, txtOuvreurs, txtFleches;
    public GameObject panneauMessage;
    public TextMeshProUGUI txtMessage;

    public void MettreAJour()
    {
        var gm = GameManager.Instance;
        if (txtNiveau)   txtNiveau.text   = gm.niveau + "/10";
        if (txtScore)    txtScore.text    = gm.score.ToString();
        if (txtTemps)    txtTemps.text    = Mathf.Max(0, Mathf.CeilToInt(gm.tempsRestant)) + "s";
        if (txtOuvreurs) txtOuvreurs.text = gm.ouvreurs.ToString();
        if (txtFleches)  txtFleches.text  = gm.fleches.ToString();
    }

    public void AfficherMessage(string msg, float duree)
    {
        if (panneauMessage) panneauMessage.SetActive(true);
        if (txtMessage) txtMessage.text = msg;
        if (duree > 0) StartCoroutine(CacherMessage(duree));
    }

    IEnumerator CacherMessage(float duree)
    {
        yield return new WaitForSeconds(duree);
        if (panneauMessage) panneauMessage.SetActive(false);
    }
}