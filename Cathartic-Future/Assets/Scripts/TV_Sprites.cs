using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controlador de los sprites de las pantallas de los edificios.
/// </summary>
public class TV_Sprites : MonoBehaviour
{
    [Tooltip("Hoja de sprites 1")]
    [SerializeField] SpriteRenderer spriteSheet;
    [Tooltip("Hoja de sprites 2")]
    [SerializeField] SpriteRenderer spriteSheet2;

    private int colorIndex; // Índice que determina el color de los sprites

    public void UpdateColor()
    {
        colorIndex = Random.Range(0,5); // Actualizamos el valor del índice

        // En función del índice, se escoge un color entre las 5 opciones disponiles
        switch (colorIndex)
        {
            case 0:
                spriteSheet.color = new Color(0, 255, 255, 1);
                if (spriteSheet2)
                {
                    spriteSheet2.color = new Color(0, 255, 255, 1);
                }
                break;
            case 1:
                spriteSheet.color = new Color(255, 0, 255, 1);
                if (spriteSheet2)
                {
                    spriteSheet2.color = new Color(255, 0, 255, 1);
                }
                break;
            case 2:
                spriteSheet.color = new Color(255, 255, 0, 1);
                if (spriteSheet2)
                {
                    spriteSheet2.color = new Color(255, 255, 0, 1);
                }
                break;
            case 3:
                spriteSheet.color = new Color(255, 0.5f, 0, 1);
                if (spriteSheet2)
                {
                    spriteSheet2.color = new Color(255, 0.5f, 0, 1);
                }
                break;
            case 4:
                spriteSheet.color = new Color(0.5f, 255, 0, 1);
                if (spriteSheet2)
                {
                    spriteSheet2.color = new Color(0.5f, 255, 0, 1);
                }
                break;
        }
    }
}
