using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class VisorTransition : MonoBehaviour
{
    [SerializeField] Material screenTransitionMaterial;
    [SerializeField] float transitionTime = 1f;
    [SerializeField] string propertyName = "_Progress";

    public UnityEvent OnTransitionDone;
    Image transtionVisorImage;

    private void Start()
    {
        transtionVisorImage = GetComponent<Image>();
    }

    public IEnumerator TransitionCoroutine()
    {
        float currentTime = 0f;
        while (currentTime < transitionTime)
        {
            currentTime += Time.deltaTime;
            screenTransitionMaterial.SetFloat(propertyName, Mathf.Clamp01(currentTime / transitionTime));
            yield return null;
        }

        OnTransitionDone?.Invoke();
    }  
    
    public IEnumerator TransitionFade()
    {
        Color color = transtionVisorImage.color;

        while (color.a > 0f)
        {
            color.a -= Time.deltaTime;

            yield return null;
        }
    }
}
