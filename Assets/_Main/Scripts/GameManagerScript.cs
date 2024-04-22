
using UnityEngine;

public class GameManagerScript : MonoBehaviour
{
    [SerializeField] private Transform spiderMan;
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Application.targetFrameRate = 60;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            spiderMan.transform.position = new Vector3(0, 8, -2.2f);
        }
    }
}
