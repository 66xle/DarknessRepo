//Shady
using UnityEngine;

[ExecuteInEditMode]
public class Reveal : MonoBehaviour
{
    [SerializeField] Material Mat;
    [SerializeField] Light SpotLight;
    [SerializeField] Torch torchScript;

    
    public bool canReveal = false;

    private Material newMat;

    private void Start()
    {
        newMat = new Material(Mat);
        GetComponent<MeshRenderer>().material = newMat;
    }

    void Update ()
    {
        if (torchScript.isUVActive && canReveal)
        {
            newMat.SetVector("MyLightPosition", SpotLight.transform.position);
            newMat.SetVector("MyLightDirection", -SpotLight.transform.forward);
            newMat.SetFloat("MyLightAngle", SpotLight.spotAngle);
        }
        else
        {
            newMat.SetVector("MyLightPosition", SpotLight.transform.position);
            newMat.SetVector("MyLightDirection", -SpotLight.transform.forward);
            newMat.SetFloat("MyLightAngle", 0f);
        }
        
    }//Update() end
}//class end