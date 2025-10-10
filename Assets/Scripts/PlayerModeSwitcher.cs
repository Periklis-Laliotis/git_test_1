using UnityEngine;
using UnityEngine.XR;

public class PlayerModeSwitcher : MonoBehaviour
{
    public GameObject xrRig;
    public GameObject desktopRig;

    void Start()
    {
        bool vrConnected = XRSettings.isDeviceActive;
        xrRig.SetActive(vrConnected);
        desktopRig.SetActive(!vrConnected);
    }
}