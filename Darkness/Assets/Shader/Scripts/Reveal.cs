//Shady
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class Reveal : MonoBehaviour
{
    [SerializeField] Material Mat;
    [SerializeField] Light SpotLight;
    [SerializeField] Visor visorScript;
    [SerializeField] List<SkinnedMeshRenderer> meshList = new List<SkinnedMeshRenderer>();
    private List<Material> matList = new List<Material>();


    public bool canReveal = false;

    private Material newMat;

    private void Start()
    {
        foreach (SkinnedMeshRenderer rend in meshList)
        {
            newMat = new Material(Mat);
            matList.Add(newMat);

            rend.material = newMat;
        }
    }

    void Update ()
    {
        if (visorScript.isVisorActive && canReveal)
        {
            foreach (Material mat in matList)
            {
                mat.SetVector("MyLightPosition", SpotLight.transform.position);
                mat.SetVector("MyLightDirection", -SpotLight.transform.forward);
                mat.SetFloat("MyLightAngle", SpotLight.spotAngle);
            }
        }
        else
        {
            foreach (Material mat in matList)
            {
                mat.SetVector("MyLightPosition", SpotLight.transform.position);
                mat.SetVector("MyLightDirection", -SpotLight.transform.forward);
                mat.SetFloat("MyLightAngle", 0f);
            }
        }
        
    }//Update() end
}//class end