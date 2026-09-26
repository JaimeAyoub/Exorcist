using UnityEngine;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class MomeventEnemy : MonoBehaviour
{
    public float distanceToFloat;
    public GameObject player;
    private Tween tween;

    public SoundData soundEnemy;

    void Start()
    {
        FloatEffect();
        player = GameObject.FindGameObjectWithTag("PlayerHolder");
        
    }

    void Update()
    {
       LookAtPlayer();
    }

    void FloatEffect()
    {
        
        Vector3 startPos = transform.position;
        
        tween = transform.DOMoveY(startPos.y + distanceToFloat, 1f)   
            .SetLoops(-1, LoopType.Yoyo)      
            .SetEase(Ease.InOutSine);        
    }



    void LookAtPlayer()
    {
        Vector3 targetPosition = player.transform.position;
        targetPosition.y = transform.position.y;

        // En vez de mirar HACIA el jugador, orientamos el forward
        // en dirección OPUESTA al jugador, para que la cara frontal
        // del sprite (normal -Z local) termine apuntando hacia él.
        Vector3 directionAwayFromPlayer = transform.position - targetPosition;

        if (directionAwayFromPlayer.sqrMagnitude > 0.0001f)
        {
            transform.rotation = Quaternion.LookRotation(directionAwayFromPlayer, Vector3.up);
        }
    }

    public void Destroy()
    {
        tween.Kill();
    }
}
