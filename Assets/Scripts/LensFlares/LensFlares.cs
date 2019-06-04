using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
//using SC.Math;
//using SC.Utilities;

using Random = UnityEngine.Random;

public class LensFlares : MonoBehaviour
{
    private class Flare
    {
        private const string kMeshName = "Flare Quad";
        private const string kObjectName = "Flare #{0}";

        private static readonly int kMainTexPropId = Shader.PropertyToID("_MainTex");
        private static readonly Vector3 kForward = Vector3.forward;

        private Mesh _mesh;
        private MeshFilter _filter;
        private MeshRenderer _renderer;
        private MaterialPropertyBlock _propBlock;
        private GameObject _self;
        private Transform _transform;
        private Transform _ownerTransform;
        private LensFlares _owner;
        private FlarePreset _preset;

        private Vector3 localScale
        {
            get
            {
                float size = _preset.size;
                return new Vector3(size, size, 1.0f);
            }
        }

        public Flare(LensFlares owner, int index)
        {
            var flares = owner._presets.presets;
            _preset = flares[index];
            _owner = owner;
            _ownerTransform = owner.transform;
            CreateObject(index);
            ApplyParams();
            GenerateMesh();
            Update();
        }

        public void Refresh()
        {
            ApplyParams();
            GenerateMesh();
        }

        public void Clear()
        {
            if (_self != null)
            {
                Destroy(_self);
            }
        }

        public void Update()
        {
            Vector2 lightPosition = _ownerTransform.position;
            Vector2 cameraPosition = _owner._cameraTransform.position;
            cameraPosition += _owner._cameraOffset;
            Vector2 position = Vector2.LerpUnclamped(lightPosition, cameraPosition, _preset.position);

            //if (_preset.align)
            //{
            //    var direction = position - lightPosition;
            //    direction = direction.Rotate(_preset.startAngle);
            //    var rotation = Quaternion.LookRotation(kForward, direction);
            //    _transform.SetPositionAndRotation(position, rotation);
            //}
            //else
            {
                _transform.position = position;
            }
        }

        private void CreateObject(int index)
        {
            string name = string.Format(kObjectName, index);
            _self = new GameObject(name, typeof(MeshFilter), typeof(MeshRenderer));
            _transform = _self.transform;
            _transform.SetParent(_ownerTransform);
            _filter = _self.GetComponent<MeshFilter>();
            _renderer = _self.GetComponent<MeshRenderer>();
            _renderer.receiveShadows = false;
            _renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            _renderer.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
            _renderer.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
        }

        private void ApplyParams()
        {
            _transform.localScale = localScale;
            _renderer.sharedMaterial = _owner._material;
            _renderer.sortingOrder = _owner._sortingOrder;
            UpdateMaterialPropertyBlock();
        }

        private void GenerateMesh()
        {
            if (null == _mesh)
            {
                _mesh = new Mesh();
                _mesh.name = kMeshName;
                _filter.sharedMesh = _mesh;
            }

            var color = _preset.color;
            var sprite = _preset.sprite;
            if (null != sprite)
            {
                ushort[] spriteTriangles = sprite.triangles;
                int[] meshTriangles = Array.ConvertAll(spriteTriangles, (i) => (int)i);
                Vector2[] spriteUVs = sprite.uv;
                Vector2[] spriteVertices = sprite.vertices;
                Vector3[] meshVertices = Array.ConvertAll(spriteVertices, (pos) => (Vector3)pos);

                Vector2 center = Vector2.zero;
                int vertexCount = spriteVertices.Length;
                if (vertexCount > 0)
                {
                    for (int i = 0; i < vertexCount; ++i)
                    {
                        var v = spriteVertices[i];
                        center += v;
                    }
                    center /= vertexCount;
                }

                float randomSeed = Random.value;
                Vector4[] offsets = Array.ConvertAll(spriteVertices, (pos) =>
                {
                    Vector2 lossyScale = _transform.lossyScale;
                    Vector2 scaledPos = Vector2.Scale(pos, lossyScale);
                    Vector4 offset = scaledPos - center;
                    offset.z = _preset.align ? 0.0f : 1.0f;
                    offset.w = randomSeed;
                    return offset;
                });
                Color[] colors = Array.ConvertAll(spriteUVs, (uv) => color);

                _mesh.vertices = meshVertices;
                _mesh.triangles = meshTriangles;
                _mesh.uv = spriteUVs;
                _mesh.SetUVs(1, new List<Vector4>(offsets));
                _mesh.colors = colors;
                _mesh.RecalculateBounds();
            }
        }

        private void UpdateMaterialPropertyBlock()
        {
            if (_propBlock == null)
                _propBlock = new MaterialPropertyBlock();

            if (_renderer != null && _preset != null && _preset.sprite !=null)
            {
                _renderer.GetPropertyBlock(_propBlock);
                _propBlock.SetTexture(kMainTexPropId, _preset.sprite.texture);
                _renderer.SetPropertyBlock(_propBlock);
            }
        }
    }

    [Serializable]
    public class FlarePreset
    {
        public Sprite sprite;
        public Color color = Color.white;
        public float size = 1.0f;
        public float position = 0.0f;
        public bool align = true;
        public float startAngle = 0.0f;
    }

    //[Serializable]
    //public class FlarePresetList : CustomPropertyArray<FlarePreset> { }

    [SerializeField]
    private Transform _cameraTransform;
    [SerializeField]
    private Vector2 _cameraOffset = Vector2.zero;
    [SerializeField]
    private Material _material;
    [SerializeField]
    private int _sortingOrder;
    [SerializeField]
    private LensFlaresPreset _presets;

    private List<Flare> _flares;

    private void Awake()
    {
        CreateFlares();
        SubscribePresets();
    }

    private void OnDestroy()
    {
        UnsubscribePresets();
    }

    private void OnValidate()
    {
#if UNITY_EDITOR
        if (EditorApplication.isPlaying)
        {
            RecreateFlares();
        }
#endif
    }

    private void LateUpdate()
    {
        UpdateFlares();
    }

    private void CreateFlares()
    {
        int count = _presets.presets.Length;

        if (_flares != null)
        {
            for (int i = 0, flareCount = _flares.Count; i < flareCount; ++i)
            {
                _flares[i].Clear();
            }
            _flares.Clear();
        }
        else
        {
            _flares = new List<Flare>(count);
        }

        for (int i = 0; i < count; ++i)
        {
            var flare = new Flare(this, i);
            _flares.Add(flare);
        }
    }

    private void RefreshFlares()
    {
        if (_flares == null)
            return;

        for (int i = 0, count = _flares.Count; i < count; ++i)
        {
            _flares[i].Refresh();
        }
    }

    private void RecreateFlares()
    {
        if (_flares == null)
            return;

        CreateFlares();
    }

    private void UpdateFlares()
    {
        for (int i = 0, count = _flares.Count; i < count; ++i)
        {
            _flares[i].Update();
        }
    }

    private void SubscribePresets()
    {
        if (_presets)
            _presets.onValidate += OnValidate;
    }

    private void UnsubscribePresets()
    {
        if (_presets)
            _presets.onValidate -= OnValidate;
    }
}
