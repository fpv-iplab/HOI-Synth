using Unity.CV.SyntheticHumans;
using Unity.CV.SyntheticHumans.Tags;
using UnityEditor;
using UnityEngine;
using UnityEngine.Perception.Randomization.Parameters;
using UnityEngine.Perception.Randomization.Samplers;

public static class AssignFaceBodyEyeMaterials
{
    static readonly int k_Rotation = Shader.PropertyToID("rotation");
    static readonly int k_Subtone = Shader.PropertyToID("subtone");
    static readonly int k_Tone = Shader.PropertyToID("tone");

    [MenuItem("CONTEXT/SkinnedMeshRenderer/AssignMaterials...")]
    public static void AssignMaterials(UnityEditor.MenuCommand menuCommand)
    {
        SkinnedMeshRenderer humanRenderer = menuCommand.context as SkinnedMeshRenderer;
        SingleHumanSpecification humanSpecs =
            humanRenderer.GetComponent<SingleHumanSpecification>();
        Debug.Log(humanRenderer.sharedMaterials[0].shader.name);

        FloatParameter s_RandomGenerator = new() { value = new UniformSampler(0, 1) };

        var skinToneMin = 0.0f;
        var skinToneMax = 10.0f;
        switch (humanSpecs.ethnicity)
        {
            case SyntheticHumanEthnicity.African:
                skinToneMin = 5.0f;
                break;
            case SyntheticHumanEthnicity.Asian:
                skinToneMax = 5.0f;
                break;
            case SyntheticHumanEthnicity.Caucasian:
                skinToneMax = 5.0f;
                break;
            case SyntheticHumanEthnicity.LatinAmerican:
                skinToneMin = 4.0f;
                skinToneMax = 7.0f;
                break;
            case SyntheticHumanEthnicity.MiddleEastern:
                skinToneMin = 4.0f;
                skinToneMax = 7.0f;
                break;
        }

        var skinTone = s_RandomGenerator.Sample() * (skinToneMax - skinToneMin) + skinToneMin;
        var subTone = s_RandomGenerator.Sample() * 10.0f;

        Debug.Log(humanRenderer.sharedMaterials[2].shader.GetPropertyName(0));

        humanRenderer.sharedMaterials[0].SetFloat("tone", skinTone);
        humanRenderer.sharedMaterials[0].SetFloat("subtone", subTone);
        humanRenderer.sharedMaterials[1].SetFloat("tone", skinTone);
        humanRenderer.sharedMaterials[1].SetFloat("subtone", subTone);
        humanRenderer.sharedMaterials[2].SetFloat("rotation", s_RandomGenerator.Sample() * 360);
    }

    public static void AssignMaterials(SkinnedMeshRenderer humanRenderer)
    {
        SingleHumanSpecification humanSpecs =
            humanRenderer.GetComponent<SingleHumanSpecification>();
        FloatParameter s_RandomGenerator = new() { value = new UniformSampler(0, 1) };
        var skinToneMin = 0.0f;
        var skinToneMax = 10.0f;
        switch (humanSpecs.ethnicity)
        {
            case SyntheticHumanEthnicity.African:
                skinToneMin = 5.0f;
                break;
            case SyntheticHumanEthnicity.Asian:
                skinToneMax = 5.0f;
                break;
            case SyntheticHumanEthnicity.Caucasian:
                skinToneMax = 5.0f;
                break;
            case SyntheticHumanEthnicity.LatinAmerican:
                skinToneMin = 4.0f;
                skinToneMax = 7.0f;
                break;
            case SyntheticHumanEthnicity.MiddleEastern:
                skinToneMin = 4.0f;
                skinToneMax = 7.0f;
                break;
        }

        var skinTone = s_RandomGenerator.Sample() * (skinToneMax - skinToneMin) + skinToneMin;
        var subTone = s_RandomGenerator.Sample() * 10.0f;

        humanRenderer.sharedMaterials[0].SetFloat("tone", skinTone);
        humanRenderer.sharedMaterials[0].SetFloat("subtone", subTone);
        humanRenderer.sharedMaterials[1].SetFloat("tone", skinTone);
        humanRenderer.sharedMaterials[1].SetFloat("subtone", subTone);
        humanRenderer.sharedMaterials[2].SetFloat("rotation", s_RandomGenerator.Sample() * 360);
    }
}
