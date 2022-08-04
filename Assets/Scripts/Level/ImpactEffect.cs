using UnityEngine;

public class ImpactEffect : MonoBehaviour
{
    public void SelfDestroy()
    {
        Destroy(gameObject);
    }
}