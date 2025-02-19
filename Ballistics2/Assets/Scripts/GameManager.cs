using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;

public class GameManager : MonoBehaviour
{
    public Ballistic ballistic;
    public void OnReset()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void OnNewTarget(GameObject newTarget)
    {
        ballistic.target = newTarget;
    }
    public void OnSetTimeType()
    {
        ballistic.useMaxTime = !ballistic.useMaxTime;
    }
}
