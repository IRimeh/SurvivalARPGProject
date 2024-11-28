using System;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

public class GrassGenerator : MonoBehaviour
{
    [SerializeField] 
    private MeshRenderer _refGrassPlane;
    [SerializeField] 
    private List<MeshRenderer> _grassLayers;

    [Space] 
    [SerializeField] private int _grassLayerCount = 8;
    [SerializeField] private float _grassHeight = 1.0f;

    private float GrassOffset => _grassHeight / _grassLayerCount;

    private void Awake()
    {
        UpdateGrassCutoffValues();
    }

    private void OnValidate()
    {
        UpdateGrassCutoffValues();
    }

    private void UpdateGrassCutoffValues()
    {
        for (int i = 0; i < _grassLayers.Count; i++)
        {
            float cutOff = ((float)(i + 1) / (float)_grassLayerCount);
            MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
            propertyBlock.SetFloat("_GrassHeight0To1", cutOff);
            _grassLayers[i].SetPropertyBlock(propertyBlock);
        }
    }

    [Button]
    private void SpawnGrassLayers()
    {
        DestroyGrassLayers();

        for (int i = 0; i < _grassLayerCount; i++)
        {
            Vector3 pos = transform.position + (Vector3.up * GrassOffset * (i + 1));
            MeshRenderer grassLayer = Instantiate(_refGrassPlane, pos, _refGrassPlane.transform.rotation, transform);

            float cutOff = ((float)(i + 1) / (float)_grassLayerCount);
            MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
            propertyBlock.SetFloat("_GrassHeight0To1", cutOff);
            grassLayer.SetPropertyBlock(propertyBlock);

            _grassLayers.Add(grassLayer);
            grassLayer.gameObject.SetActive(true);
        }
        
        _refGrassPlane.gameObject.SetActive(false);
    }

    [Button]
    private void DestroyGrassLayers()
    {
        for (int i = _grassLayers.Count - 1; i >= 0; i--)
        {
            if(Application.isPlaying)
                Destroy(_grassLayers[i].gameObject);
            else
                DestroyImmediate(_grassLayers[i].gameObject);
        }
        _grassLayers.Clear();
    }
}
