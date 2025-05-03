using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;


public class HUD : MonoBehaviour
{
    public enum InfoType {Time, Health, Kill};
    public InfoType type;

    Slider mySlider;
    Text myText;

    private void Awake()
    {
        myText = GetComponent<Text>();
        mySlider = GetComponent<Slider>();        
    }

    private void LateUpdate()
    {
        switch (type)
        {
            case InfoType.Time:
                int min = Mathf.FloorToInt(GameManager.instance.gameTime / 60);
                int sec = Mathf.FloorToInt(GameManager.instance.gameTime % 60);
                myText.text = string.Format("{0:D2}:{1:D2}", min, sec);
                break;

            case InfoType.Health:
                float curHp = StatManager.instance.vitalStats.maxHealth - GameManager.instance.player.curDamage;
                mySlider.value = curHp/StatManager.instance.vitalStats.maxHealth;
                break;

            case InfoType.Kill:
                myText.text = string.Format("{0:F0}", GameManager.instance.killCount);
                break;
        }
    }
}
