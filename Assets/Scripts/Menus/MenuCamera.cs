using UnityEngine;

public class MenuCamera : MonoBehaviour
{
    GameObject eye;

    GameObject menuAnim;
    GameObject menuCamera;

    public float smoothTimer = 0.0f;
    float smoothDelay = 2.0f;
    float smoothAlpha = 0.0f;

    public Animation Animation
    {
        get { return menuAnim.animation; }
    }

    void Start()
    {
        menuAnim = (GameObject)Instantiate(Resources.Load("Menu/Menu"));
        menuAnim.animation.clip = null;
        menuAnim.animation.playAutomatically = false;
        menuAnim.animation.Stop();

        menuCamera = GameObject.Find("Camera_Menu");

        SetEye(menuCamera);
        transform.position = eye.transform.position;
        transform.rotation = eye.transform.rotation * Quaternion.Euler(0.0f, 180.0f, 0.0f);

        smoothTimer = smoothDelay;
    }

    public void ResetEye()
    {
        eye = menuCamera;
    }

    public void SetEye(GameObject eye)
    {
        this.eye = eye;
    }
    
    void OnDrawGizmos()
    {
        if (eye != null)
        {
            Gizmos.DrawSphere(eye.transform.position, 1.0f);
        }
    }

    void Update()
    {
        if (Main.Instance.MenuController.state == MenuController.State.Pause)
        {
            transform.position = Main.Instance.PlayerStart.transform.position;
            transform.rotation = Main.Instance.PlayerStart.transform.rotation;
        }
        else
        {
            Quaternion fix = Quaternion.identity;
            if (eye == null)
                ResetEye();
            if (eye == menuCamera)
                fix = Quaternion.Euler(0.0f, 180.0f, 0.0f);

            smoothTimer += Time.deltaTime;
            smoothTimer = Mathf.Clamp(smoothTimer, 0.0f, smoothDelay);
            smoothAlpha = smoothTimer / smoothDelay;

            transform.position += (eye.transform.position - transform.position) * (0.01f + smoothAlpha * 0.5f);
            transform.rotation = Quaternion.Slerp(transform.rotation, eye.transform.rotation * fix, 0.2f);
        }
    }
}
