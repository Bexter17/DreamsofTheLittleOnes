using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class ChangeStatusUI : MonoBehaviour
{
    public Image m_image;

    public Sprite bjorn_100_percent;
    public Sprite bjorn_50_percent;
    public Sprite bjorn_25_percent;

    public Slider healthbar;

    // Start is called before the first frame update
    void Start()
    {
        m_image = GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
     
      
    }

    private void FixedUpdate()
    {
        if (healthbar.value >= 51)
        {
            m_image.sprite = bjorn_100_percent;
        }
        else if (healthbar.value <= 50)
        {
            m_image.sprite = bjorn_50_percent;
        }
       
    }
}
