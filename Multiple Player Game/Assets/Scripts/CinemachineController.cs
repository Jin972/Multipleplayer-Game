using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CinemachineController : MonoBehaviour
{
    CinemachineVirtualCamera cinemachine;
    [SerializeField]
    float zoom = 40f;
    [SerializeField]
    float zoomOffSet = 300f;
    // Start is called before the first frame update
    void Start()
    {
        cinemachine = GetComponent<CinemachineVirtualCamera>();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        ZoomCamera();
    }

    void ZoomCamera()
    {
        zoom += Input.mouseScrollDelta.y * zoomOffSet * Time.deltaTime;
        zoom = Mathf.Clamp(zoom, 30f, 50f);
        cinemachine.m_Lens.FieldOfView = zoom;
    }
}
