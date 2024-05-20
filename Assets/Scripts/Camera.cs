using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camera : MonoBehaviour
{
    public float sensitivity = 2f;
    public float smoothness = 1f;

    private GameObject player;
    private Vector2 mouseLook;
    private Vector2 smoothV;

    [SerializeField] private bool locked;

    void Start()
    {
        player = transform.parent.gameObject;
        if (locked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    void LateUpdate()
    {
        var input = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));

        input = Vector2.Scale(input, new Vector2(sensitivity * smoothness, sensitivity * smoothness));
        smoothV.x = Mathf.Lerp(smoothV.x, input.x, 1f / smoothness);
        smoothV.y = Mathf.Lerp(smoothV.y, input.y, 1f / smoothness);
        mouseLook += smoothV;

        mouseLook.y = Mathf.Clamp(mouseLook.y, -90f, 90f);

        transform.localRotation = Quaternion.AngleAxis(-mouseLook.y, Vector3.right);
        player.transform.localRotation = Quaternion.AngleAxis(mouseLook.x, player.transform.up);
    }
}//
