using System.Collections.Generic;
using UnityEngine;

public class JoustickSettings : MonoBehaviour
{
    public bool mobile = false;

    // Start is called before the first frame update
    public GameObject variableJoystick;
    void Start()
    {
        if (mobile == true) LoadJoustick();
    }

    private void LoadJoustick() //загружаем настройки джостика
    {
        List<Transform> c;
        OutChild(variableJoystick.gameObject.transform, out c);
        foreach (Transform copy in c)
        {
            RectTransform rt = copy.GetComponent<RectTransform>();
            copy.GetComponent<RectTransform>().sizeDelta = new Vector2(rt.rect.width + 5 * PlayerPrefs.GetFloat(copy.gameObject.name), rt.rect.height + 5 * PlayerPrefs.GetFloat(copy.gameObject.name));
            /*for (int i = 0; i < attributes.GetLength(0); i++) //Магия
            {
                for (int ii = 0; ii < attributes.GetLength(1); ii++)
                {
                    if (attributes[i, ii] == copy.gameObject.name)
                    {
                        attributes[i + 1, ii] = (rt.rect.width + 5 * PlayerPrefs.GetFloat(copy.gameObject.name)).ToString();
                        print("Value " + ii.ToString() + ": " + (rt.rect.width + 5 * PlayerPrefs.GetFloat(copy.gameObject.name)).ToString());
                    }
                }
            }*/
        }
    }
    private void OutChild(Transform p, out List<Transform> c)
    {
        //print("Parent: " + p.gameObject.name);
        c = new List<Transform>();
        for (int i = 0; i < p.childCount; i++)
        {
            c.Add(p.GetChild(i));
            //print("Child" + p.gameObject.name + ": " + c[i].gameObject.name);
        }
    }
}
