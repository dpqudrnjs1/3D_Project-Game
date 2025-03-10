using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Condition ���� �ٴ� ���� ������ ����
// �ڵ� ��Ȱ���� ���� ���� ��ũ��Ʈ�� �۾�
public class Condition : MonoBehaviour
{
    public float curValue;
    public float startValue;
    public float maxValue;
    public float passiveValue;
    public Image uiBar;

    void Start()
    {
        curValue = startValue;
    }

    void Update()
    {
        uiBar.fillAmount = GetPercentage();
    }

    float GetPercentage()
    {
        return curValue / maxValue;
    }

    public void Add(float value)
    {
        // �� ���� ���� �� (ex. maxValue���� Ŀ���� maxValue)
        curValue = Mathf.Min(curValue + value, maxValue);
    }

    public void Subtract(float value)
    {
        // �� ���� ū �� (ex. 0���� �۾����� 0)
        curValue = Mathf.Max(curValue - value, 0);
    }
}