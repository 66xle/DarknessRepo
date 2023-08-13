using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Experimental.AI;
using UnityEngine.UI;

public class Visor : MonoBehaviour
{
    [Header("Visor Transition")]
    [SerializeField] Material screenTransitionMaterial;
    [SerializeField] float transitionTime = 1f;
    [SerializeField] string propertyName = "_Progress";

    [Header("References")]
    [SerializeField] GameObject visor;
    [SerializeField] GameObject visorTransitionPanel;
    [SerializeField] Color matColor;
    [SerializeField] GameManager gameManager;

    public UnityEvent OnTransitionDone;
    public bool isVisorActive = false;

    private Image visorTransitionPanelImage;

    private bool isTransitionInProgress = false;



    private void Start()
    {
        visorTransitionPanelImage = visorTransitionPanel.GetComponent<Image>();

        screenTransitionMaterial.SetFloat(propertyName, 1f);
        screenTransitionMaterial.SetColor("_Color", matColor);
    }

    private void Update()
    {
        ToggleVisor();
    }

    void ToggleVisor()
    {
        if (Input.GetKeyDown(KeyCode.Q) && !isTransitionInProgress)
        {
            if (isVisorActive)
            {
                isVisorActive = false;
                StartCoroutine(TransitionCoroutine());

            }
            else
            {
                isVisorActive = true;
                // Start visor transition
                StartCoroutine(TransitionCoroutine());
            }
        }
    }

    public IEnumerator TransitionCoroutine()
    {
        isTransitionInProgress = true;

        float currentTime = transitionTime;
        while (currentTime > 0f)
        {
            currentTime -= Time.deltaTime;
            screenTransitionMaterial.SetFloat(propertyName, Mathf.Clamp01(currentTime / transitionTime));
            yield return null;
        }

        OnTransitionDone?.Invoke();
    }

    public void VisorTransitionEnd()
    {
        // Activate visor
        if (isVisorActive)
            visor.SetActive(true);
        else
            visor.SetActive(false);

        // Turn on/off hornets
        gameManager.ToggleMeshRenderer(isVisorActive);

        // Remove shader from panel
        visorTransitionPanelImage.material = null;

        StartCoroutine(TransitionFade());
    }

    
    public IEnumerator TransitionFade()
    {
        Color color = matColor;


        float currentTime = transitionTime;

        while (currentTime > 0f)
        {
            currentTime -= Time.deltaTime;

            color.a = Mathf.Clamp01(currentTime / transitionTime);
            visorTransitionPanelImage.color = color;

            yield return null;
        }

        isTransitionInProgress = false;

        // Reset visor panel
        visorTransitionPanelImage.color = matColor;

        screenTransitionMaterial.SetFloat(propertyName, 1f);
        visorTransitionPanelImage.material = screenTransitionMaterial;
    }
}
