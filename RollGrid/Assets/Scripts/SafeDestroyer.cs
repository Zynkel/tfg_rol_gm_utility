using System.Collections;
using UnityEngine;

/*
 * Esta clase ayuda a destruir gameobjects que requieren desactivarse previamente por problemas de eventos.
 * Al estar desactivado el gameobject no puede lanzarse con coroutine, por lo que se usa esta clase para lanzarse desde un gameobject
 * activo distinto. 
 */
public class SafeDestroyer : MonoBehaviour
{
    private static SafeDestroyer _instance;

    public static SafeDestroyer Instance
    {
        get
        {
            if (_instance == null)
            {
                var go = new GameObject("SafeDestroyer");
                DontDestroyOnLoad(go);
                _instance = go.AddComponent<SafeDestroyer>();
            }
            return _instance;
        }
    }

    public void DestroySafely(GameObject go)
    {
        StartCoroutine(DestroyNextFrame(go));
    }

    private IEnumerator DestroyNextFrame(GameObject go)
    {
        yield return null;
        Destroy(go);
    }
}
