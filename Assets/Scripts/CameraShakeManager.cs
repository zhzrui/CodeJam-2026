using UnityEngine;
using Unity.Cinemachine; 


public class CameraShakeManager : MonoBehaviour
{
    public static CameraShakeManager Instance;
    [SerializeField] private float globalShakeForce = 1f;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    public void CameraShake(CinemachineImpulseSource impulseSource, float shakeForce)
    {
        if (impulseSource != null)
        {
            impulseSource.GenerateImpulse(globalShakeForce);
        }
    }
}
