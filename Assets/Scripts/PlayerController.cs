using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    public float hauteurYeux = 0.75f;
    public float rayonJoueur = 0.22f;

    private CharacterController cc;
    private Camera cam;
    private Camera camAerienne;
    private float angleVue = 0f;
    private float anglePitch = 0f;
    private const float PITCH_MAX = 80f;
    private bool modeSouris = false;

    void Start()
    {
        cc = GetComponent<CharacterController>();
        cam = Camera.main;


        GameObject aerGO = GameObject.Find("CameraAerienne");
        if (aerGO != null) camAerienne = aerGO.GetComponent<Camera>();

        if (cam != null)
            cam.nearClipPlane = 0.05f;

        ResetPosition();
    }

    void Update()
    {
        var gm = GameManager.Instance;
        if (gm == null || gm.gameOver || gm.jeuTermine) return;

        if (gm.modeVueAer) return;


        if (Input.GetKeyDown(KeyCode.Alpha0) || Input.GetKeyDown(KeyCode.Keypad0))
        {
            modeSouris = !modeSouris;
            Cursor.lockState = modeSouris ? CursorLockMode.Locked : CursorLockMode.None;
        }

        float vitesse = gm.VITESSE_AVANCE;
        float rotation = gm.VITESSE_ROTATION;

        if (modeSouris)
        {
            float mx = Input.GetAxis("Mouse X");
            float my = Input.GetAxis("Mouse Y");
            angleVue += mx * rotation * 8f;
            anglePitch -= my * rotation * 5f;
            anglePitch = Mathf.Clamp(anglePitch, -PITCH_MAX, PITCH_MAX);

            if (Input.GetKey(KeyCode.W)) Avancer(vitesse);
            if (Input.GetKey(KeyCode.S)) Avancer(-vitesse);
            if (Input.GetKey(KeyCode.A)) Strafer(-vitesse);
            if (Input.GetKey(KeyCode.D)) Strafer(vitesse);
        }
        else
        {
            if (Input.GetKey(KeyCode.UpArrow))    Avancer(vitesse);
            if (Input.GetKey(KeyCode.DownArrow))  Avancer(-vitesse);
            if (Input.GetKey(KeyCode.LeftArrow))  angleVue -= rotation;
            if (Input.GetKey(KeyCode.RightArrow)) angleVue += rotation;
        }

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0) Avancer(scroll * vitesse * 15f);


        transform.rotation = Quaternion.Euler(0, angleVue, 0);
        if (cam != null)
            cam.transform.localRotation = Quaternion.Euler(anglePitch, 0, 0);
    }

    void Avancer(float baseSpeed)
    {
        Vector3 forward = transform.forward;
        forward.y = 0;
        forward.Normalize();
        float actualSpeed = baseSpeed * 60f * Time.deltaTime;
        cc.Move(forward * actualSpeed);
    }

    void Strafer(float baseSpeed)
    {
        Vector3 right = transform.right;
        right.y = 0;
        right.Normalize();
        float actualSpeed = baseSpeed * 60f * Time.deltaTime;
        cc.Move(right * actualSpeed);
    }

    public void ResetPosition()
    {
        if (cc != null)
        {
            cc.enabled = false;                    // ← IMPORTANT
            transform.position = new Vector3(15.5f, hauteurYeux, 15.5f);
            cc.enabled = true;                     // ← IMPORTANT
        }
        else
        {
            transform.position = new Vector3(15.5f, hauteurYeux, 15.5f);
        }

        angleVue = 0f;
        anglePitch = 0f;

        if (GameManager.Instance != null)
            GameManager.Instance.aSortiEnclos = false;
    }

    public void SetVueAerienne(bool actif)
    {
        if (cam != null)
            cam.gameObject.SetActive(!actif);

        if (camAerienne != null)
            camAerienne.gameObject.SetActive(actif);
    }

    
    public void OuvrirMurDevant()
    {
        float rad = angleVue * Mathf.Deg2Rad;
        float fx = transform.position.x + Mathf.Sin(rad) * 0.65f;
        float fz = transform.position.z + Mathf.Cos(rad) * 0.65f;
        int col = Mathf.FloorToInt(fx);
        int row = Mathf.FloorToInt(fz);
        GameManager.Instance.OuvrirMur(row, col);
    }


}