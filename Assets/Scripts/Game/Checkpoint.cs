using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Checkpoint : MonoBehaviour
{
    [Tooltip("Unique ID for this checkpoint. Must be 0..(N-1) and assigned in the editor.")]
    public int checkpointId;

    private void Reset()
    {
        // ensure it's a trigger for 2D collisions
        var col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }
}
