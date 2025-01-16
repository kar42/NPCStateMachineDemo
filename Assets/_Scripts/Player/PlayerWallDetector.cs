using UnityEngine;
using System.Collections;

public class PlayerWallDetector : MonoBehaviour {

    private int m_ColCount = 0;

    private float m_DisableTimer;

    private bool canPassThrough = false;
    private bool canClimbLadderUp = false;
    private bool canClimbLadderDown = false;
    private GameObject collidingObject;

    private void OnEnable()
    {
        m_ColCount = 0;
    }

    public bool State()
    {
        if (m_DisableTimer > 0)
            return false;
        return m_ColCount > 0;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        //print("CrowWallDetector: " + collision.gameObject.tag);
        if (collision.gameObject.CompareTag("Enviornment"))
        {
            m_ColCount++;
        }
        //allow player to pass through dynamic platform
        if (collision.gameObject.CompareTag("DynamicPlatform"))
        {
            collidingObject = collision.gameObject;
            canPassThrough = true;
        }
        
        if (collision.gameObject.CompareTag("LadderBottom"))
        {
            canClimbLadderUp = true;
        }
        if (collision.gameObject.CompareTag("LadderTop"))
        {
            canClimbLadderDown = true;
        }
        
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Enviornment"))
        {
            m_ColCount--;
        }
        if (collision.gameObject.CompareTag("DynamicPlatform"))
        {
            collidingObject = null;
            canPassThrough = false;
        }
        if (collision.gameObject.CompareTag("LadderBottom")) //&& is not climbing ladder
        {
            canClimbLadderUp = false;
        }
        if (collision.gameObject.CompareTag("LadderTop"))
        {
            canClimbLadderDown = false;
        }
    }

    public bool CanPassThroughPlatform()
    {
        return canPassThrough;
    }
    
    public bool CanClimbUpLadder()
    {
        return canClimbLadderUp;
    }
    
    public bool CanClimbDownLadder()
    {
        return canClimbLadderDown;
    }

    public void PassThroughObject()
    {
        if (collidingObject != null)
        {
            var gameObj = collidingObject;
            StartCoroutine(DisableObjectTimer(gameObj));
        }
    }

    void Update()
    {
        m_DisableTimer -= Time.deltaTime;
    }

    public void Disable(float duration)
    {
        m_DisableTimer = duration;
    }
    
    private IEnumerator DisableObjectTimer(GameObject gameObj)
    {
        
        gameObj.SetActive(false);
        yield return new WaitForSeconds(0.5f);
        
        gameObj.SetActive(true);
        
    }
    
}
