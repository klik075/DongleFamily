using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NextDongleImage : MonoBehaviour
{
    public Image nextDongleImage;
    public Sprite[] dongleImages;
    public void SetImage(int level)
    {
        if (level >= dongleImages.Length)
            return;

        nextDongleImage.sprite = dongleImages[level];
    }
}
