using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
    [Range(0.0f, 1.0f)]
    public float time; // ���� �ð� (0.0f ~ 1.0f ������ ��, �Ϸ�: 1)
    public float fullDayLength; // �Ϸ��� ���� (�� ����)
    public float startTime = 0.4f; // ���� �ð� (0.4�� �Ϸ��� 40% ����, ��ħ�̳� ���ķ� ���� ����)
    private float timeRate; // �ð� ���� �ӵ�
    public Vector3 noon; // �¾�� ���� ����(�ְ���)�� ��ġ�� ����

    [Header("Sun")]
    public Light sun; // �¾� ������ ��
    public Gradient sunColor; // �¾� �� ��ȭ (�ð��� ���� ����)
    public AnimationCurve sunIntensity; // �¾� ��� ��ȭ (�ð��� ���� ����)

    [Header("Moon")]
    public Light moon; // �� ������ ��
    public Gradient moonColor; // �� �� ��ȭ (�ð��� ���� ����)
    public AnimationCurve moonIntensity; // �� ��� ��ȭ (�ð��� ���� ����)

    [Header("Other Lighting")]
    public AnimationCurve lightingIntensityMultiplier; // ��ü ȯ�汤 ���� ��ȭ (�ð��� ���� ����)
    public AnimationCurve reflectionIntensityMultiplier; // �ݻ籤 ��ȭ (�ð��� ���� ����)

    private void Start()
    {
        timeRate = 1.0f / fullDayLength; // �ð� ���� �ӵ� ��� (1�ʸ��� �Ϸ��� �Ϻ� ����)
        time = startTime; // ���� �ð��� ����
    }

    private void Update()
    {
        // �ð� ���� (1�� ������ �ٽ� 0���� ��ȯ)
        time = (time + timeRate * Time.deltaTime) % 1.0f;

        // �¾�� �� ���� ������Ʈ
        UpdateLighting(sun, sunColor, sunIntensity);
        UpdateLighting(moon, moonColor, moonIntensity);

        // ȯ�汤�� �ݻ籤 ������Ʈ
        RenderSettings.ambientIntensity = lightingIntensityMultiplier.Evaluate(time);
        RenderSettings.reflectionIntensity = reflectionIntensityMultiplier.Evaluate(time);
    }

    void UpdateLighting(Light lightSource, Gradient colorGradiant, AnimationCurve intensityCurve)
    {
        float intensity = intensityCurve.Evaluate(time); // ���� �ð��� ���� ���� ���� ���

        // ���� ȸ�� �� ���� (�¾�� ���� �Ϸ� ���� �����̴� ����)
        lightSource.transform.eulerAngles = (time - (lightSource == sun ? 0.25f : 0.75f)) * noon * 4.0f;  
        //30 ������ 0.5 = 180�̴� 0.25�� ���� ������ �ȴ�. �ݴ�� ���� �ؿ� �ٸ��� ��Ȯ�ϰ� 180�� ���� ������ 0.5�� ���� 0.75�� ����. 
        //90�� 0.25�� ���ϸ� 90�� �ȳ����� 4�� ���Ѵ�.

        lightSource.color = colorGradiant.Evaluate(time); // ���� ������Ʈ
        lightSource.intensity = intensity; // ��� ������Ʈ

        // ���� ������ ������� ��Ȱ��ȭ, ������� Ȱ��ȭ
        GameObject go = lightSource.gameObject;
        if (lightSource.intensity == 0 && go.activeInHierarchy)
            go.SetActive(false);
        else if (lightSource.intensity > 0 && !go.activeInHierarchy)
            go.SetActive(true);
    }
}
