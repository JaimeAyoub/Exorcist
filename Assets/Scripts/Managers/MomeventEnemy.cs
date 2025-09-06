using UnityEngine;
using DG.Tweening;
public class MomeventEnemy : MonoBehaviour
{
    public float distanceToFloat;
    public GameObject player;

    void Start()
    {
        FloatEffect();
    }

    void Update()
    {
        gameObject.transform.LookAt(player.transform.position);
    }

    void FloatEffect()
    {
   
        Vector3 startPos = transform.position;
        
        transform.DOMoveY(startPos.y + distanceToFloat, 1f)   
            .SetLoops(-1, LoopType.Yoyo)      
            .SetEase(Ease.InOutSine);        
    }
}
