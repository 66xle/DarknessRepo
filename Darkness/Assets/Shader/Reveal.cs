//Shady
using UnityEngine;

[ExecuteInEditMode]
public class Reveal : MonoBehaviour
{
    [SerializeField] Material Mat;
    [SerializeField] Light SpotLight;
    [SerializeField] Torch torchScript;
	
	void Update ()
    {
        if (torchScript.isTorchActive)
        {
            Mat.SetVector("MyLightPosition", SpotLight.transform.position);
            Mat.SetVector("MyLightDirection", -SpotLight.transform.forward);
            Mat.SetFloat("MyLightAngle", SpotLight.spotAngle);
        }
        else
        {
            Mat.SetVector("MyLightPosition", SpotLight.transform.position);
            Mat.SetVector("MyLightDirection", -SpotLight.transform.forward);
            Mat.SetFloat("MyLightAngle", 0f);
        }
        
    }//Update() end
}//class end