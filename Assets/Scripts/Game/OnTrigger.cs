using UnityEngine;
using UnityEngine.Rendering;
public class OnTrigger : MonoBehaviour
{
    public bool isTrigger = false;
    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
            isTrigger = true;
    }
}