using UnityEngine;

public class ImpactEffect : MonoBehaviour
{
    public void selfDestroy()
    {
        Destroy(gameObject);
    }
}