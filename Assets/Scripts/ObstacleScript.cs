using System.Collections;
using UnityEngine;

public class ObstacleScript : MonoBehaviour
{
    [Header("Damage Settings")]
    public int damage = 1;          // How many numbers to remove
    public float slowAmount = 0.5f; // Speed multiplier (0.5 = 50% speed)
    public float slowTime = 2f;     // How long slow lasts

    [Header("Visual Effects")]
    public GameObject hitEffect;    // Particle effect on collision
    public bool flashOnHit = true;  // Flash obstacle color

    public Renderer obstacleRenderer;

    private void OnTriggerEnter(Collider other)
    {
        // Check if player hit the obstacle
        if (other.CompareTag("Player"))
        {



            PlayerControllerScript.instance.RemoveNumbers(damage);

            // Apply slow effect if set
            if (slowAmount < 1f && slowTime > 0f)
                {
                    //PlayerControllerScript.instance.moveSpeed *= slowAmount;
                    Invoke(nameof(ResetSpeed), slowTime);
                }
            

            // Show collision effects
            ShowEffects();
        }
    }

    void ResetSpeed()
    {
        //PlayerControllerScript.instance.moveSpeed /= slowAmount;
    }

    void ShowEffects()
    {
        StartCoroutine(FlashObstacle());
    }

    IEnumerator FlashObstacle()
    {
        
        if (obstacleRenderer == null) yield break;

        Color originalColor = obstacleRenderer.material.color;
        obstacleRenderer.material.color = Color.red;
        hitEffect.SetActive(true);
        yield return new WaitForSeconds(0.3f);
        hitEffect.SetActive(false);

        obstacleRenderer.material.color = originalColor;
    }

}