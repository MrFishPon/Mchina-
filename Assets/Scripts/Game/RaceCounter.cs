using UnityEngine;
using TMPro;
public class RaceCounter : MonoBehaviour
{
    [SerializeField] private TMP_Text text;

    [SerializeField] private int counter = 0;

    [SerializeField] OnTrigger[] triggers;

    private void OnTriggerEnter2D(Collider2D collision)
    {

        bool completed = true;

        foreach(var trg in triggers)
        {
            if(trg.isTrigger == false)
            {
                completed = false;

                break;
            }
        }
        if(completed == true)
        {
            counter++;

            text.text = counter.ToString();

            foreach (var trg in triggers)
            {
                 trg.isTrigger = false;
            }
        }
        

    }

}
