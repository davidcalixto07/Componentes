using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Vuforia;

public class TargetIDGenerator : MonoBehaviour
{
    public void GenerateNewIds(ImageTargetBehaviour targetBehaviour)
    {
        var id = targetBehaviour.TargetName;

        // Callback 
        Debug.Log("Image:" + id);

        // probablemente necesite una bandera para que mientras consulte
        // obvie algunos ids - para q no llame 200 veces x segundo al server
        // o algo asi !!!!!! - hay que evaluarlo y quitar el comentario

        SceneLoader.findScene4ImageId(id, (bOk) =>
        {
            // termina de consultar al server por la esccena asociada a la imagen id
            // con resultado bOk (true/false)
            if (bOk)
            {
                Debug.Log("ENCONTRÓ!");
                // Acciones al haber encontrado
            }
            else
            {
                Debug.Log("NO ENCONTRÓ!");
                //Acciones al no haber encontrado
            }
        });

    }
}
