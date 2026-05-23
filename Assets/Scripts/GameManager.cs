using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Constantes")]
    public float DUREE_NIVEAU = 60f;
    public float VITESSE_AVANCE = 0.085f;
    public float VITESSE_ROTATION = 2.0f;

    [Header("État du jeu")]
    public int niveau = 1;
    public int score = 300;
    public float tempsRestant;
    public int ouvreurs = 4;
    public int fleches = 18;
    public bool modeVueAer = false;
    public bool triche = false;
    public bool gameOver = false;
    public bool jeuTermine = false;
    public bool aSortiEnclos = false;

    [Header("Références — glisse dans l'Inspector")]
    public MazeBuilder mazeBuilder;
    public TreasureSpawner spawner;
    public PlayerController player;
    public HUDManager hud;
    public AudioManager audioManager;

    private float timerVueAer = 0f;
    private bool dejaAppeleTresor = false; 

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (mazeBuilder  == null) mazeBuilder  = FindObjectOfType<MazeBuilder>();
        if (spawner      == null) spawner       = FindObjectOfType<TreasureSpawner>();
        if (player       == null) player        = FindObjectOfType<PlayerController>();
        if (hud          == null) hud           = FindObjectOfType<HUDManager>();
        if (audioManager == null) audioManager  = FindObjectOfType<AudioManager>();

        if (mazeBuilder  == null) { Debug.LogError("MazeBuilder introuvable!");  return; }
        if (spawner      == null) { Debug.LogError("TreasureSpawner introuvable!"); return; }
        if (player       == null) { Debug.LogError("PlayerController introuvable!"); return; }
        if (hud          == null) { Debug.LogError("HUDManager introuvable!");    return; }
        if (audioManager == null) { Debug.LogError("AudioManager introuvable!");  return; }

        InitialiserNiveau(1);
    }

    void Update()
    {
        if (gameOver || jeuTermine) return;

        // Timer
        tempsRestant -= Time.deltaTime;
        if (tempsRestant <= 0)
        {
            tempsRestant = 0;
            TempsEcoule();
            return;
        }

        // Pénalité vue aérienne
        if (modeVueAer)
        {
            timerVueAer += Time.deltaTime;
            if (timerVueAer >= 1f)
            {
                timerVueAer = 0f;
                score -= 10;
                if (score < 0) score = 0;
                if (score < 10) DesactiverVueAerienne();
            }
        }

        if (hud != null) hud.MettreAJour();

        // Touches
        if (Input.GetKeyDown(KeyCode.PageUp) && !modeVueAer && score >= 10)
            ActiverVueAerienne();

        if (Input.GetKeyDown(KeyCode.PageDown) && modeVueAer)
            DesactiverVueAerienne();

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftShift))
                triche = !triche;
            else
                player.OuvrirMurDevant();
        }
    }

    // ---------- NIVEAUX ----------

    public int OuvreursPourNiveau(int n)
    {
        if (n <= 2) return 4;
        if (n <= 4) return 3;
        if (n <= 6) return 2;
        if (n <= 8) return 1;
        return 0;
    }

    public int FlechesPourNiveau(int n)   => Mathf.Max(0, 20 - n * 2);
    public int TeleTransPourNiveau(int n) => n <= 1 ? 0 : n / 2;
    public int TeleRecepPourNiveau(int n) => n <= 1 ? 0 : n - 1;

    public void InitialiserNiveau(int n)
    {
        niveau        = n;
        ouvreurs      = OuvreursPourNiveau(n);
        fleches       = FlechesPourNiveau(n);
        tempsRestant  = DUREE_NIVEAU;
        aSortiEnclos  = false;
        dejaAppeleTresor = false; // reset le guard

        MazeData.ResetOuvert();
        MazeData.DEDALE[13, 15] = MazeData.COULOIR;

        mazeBuilder.ConstruireMurs();
        spawner.PlacerObjets(n);
        player.ResetPosition();
        if (audioManager != null) audioManager.Jouer("debutNiveau");
        if (hud != null) hud.MettreAJour();
    }

    public void RecommencerNiveau()
    {
        score        -= 200;
        ouvreurs      = OuvreursPourNiveau(niveau);
        tempsRestant  = DUREE_NIVEAU;
        aSortiEnclos  = false;
        dejaAppeleTresor = false;

        MazeData.ResetOuvert();
        MazeData.DEDALE[13, 15] = MazeData.COULOIR;

        mazeBuilder.ConstruireMurs();
        player.ResetPosition();
        if (audioManager != null) audioManager.Jouer("debutNiveau");
        if (hud != null) hud.MettreAJour();
    }

    void TempsEcoule()
    {
        if (score < 200)
        {
            DeclencherGameOver();
            return;
        }
        if (audioManager != null) audioManager.Jouer("tempsEcoule");
        RecommencerNiveau();
    }

    public void TrouveTresor()
    {
        if (dejaAppeleTresor) return; 
        dejaAppeleTresor = true;

        int bonus = Mathf.RoundToInt(tempsRestant) * 10;
        score += bonus;

        if (niveau >= 10)
        {
            FinJeu();
            return;
        }

        if (audioManager != null) audioManager.Jouer("tresor");
        niveau++;
        InitialiserNiveau(niveau);
        if (hud != null) hud.AfficherMessage("NIVEAU " + niveau + " !", 2f);
    }

    void DeclencherGameOver()
    {
        if (gameOver) return;
        gameOver = true;
        if (audioManager != null) audioManager.Jouer("gameOver");
        if (hud != null) hud.AfficherMessage("GAME OVER", 0f);
    }

    void FinJeu()
    {
        if (jeuTermine) return;
        jeuTermine = true;
        if (audioManager != null) audioManager.Jouer("finJeu");
        if (hud != null) hud.AfficherMessage("VICTOIRE !", 0f);
    }

    public void ActiverVueAerienne()
    {
        modeVueAer  = true;
        timerVueAer = 0f;
        if (player != null)
            player.SetVueAerienne(true);
    }

    public void DesactiverVueAerienne()
    {
        modeVueAer = false;
        if (player != null)
            player.SetVueAerienne(false);
    }

    public void OuvrirMur(int row, int col)
    {
        if (ouvreurs <= 0 || score < 50) return;
        if (row < 0 || row > 30 || col < 0 || col > 30) return;
        if (MazeData.DEDALE[row, col] != MazeData.MUR_OUV) return;
        if (MazeData.DEDALE_OUVERT[row, col]) return;

        MazeData.DEDALE_OUVERT[row, col] = true;
        ouvreurs--;
        score -= 50;
        mazeBuilder.ConstruireMurs();
        if (audioManager != null) audioManager.Jouer("ouvrirMur");
        if (hud != null) hud.MettreAJour();
    }
}