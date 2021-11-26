using System;
using UnityEngine;

[ExecuteInEditMode]
public class MaterialInstancing : MonoBehaviour {

    [Range(0, 2f)]
    public float albedo = 1.0f;
    [Range(0.5f, 3f)]
    public float gamma = 1.0f;
    
    MaterialPropertyBlock props;
    MeshRenderer renderer;
    void OnValidate() {
        renderer = this.GetComponent<MeshRenderer>();
        props = new MaterialPropertyBlock();
        setGPUINstancingProps();
    }

    private void Start() {
        renderer = this.GetComponent<MeshRenderer>();
        props = new MaterialPropertyBlock();
        setGPUINstancingProps();
    }

    private void Update() {
        setGPUINstancingProps();
    }

    private void setGPUINstancingProps() {
        props.SetFloat("_Albedo", albedo);
        props.SetFloat("_Gamma", gamma);
        renderer.SetPropertyBlock(props);
    }
}
