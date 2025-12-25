// NumberPickUpScript.cs
using UnityEngine;
using UnityEngine.UI;

public class NumberPickUpScript : MonoBehaviour
{
    public int value = 1;

    private void Start()
    {
        
        Text text = GetComponentInChildren<Text>();
        if (text != null)
        {
            text.text = value.ToString();
        }

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            //PlayerControllerScript.instance.AddNumber(value);
            PlayerControllerScript.instance.CollectNumber(value);
            Destroy(gameObject);
        }
    }
}