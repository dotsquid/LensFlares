using UnityEngine;

public class MapFrame : MonoBehaviour
{
    [SerializeField]
    private float _scale = 1.1f;
    [SerializeField]
    private float _frequency = 1.0f;

    private Transform _transform;
    private Vector2 _initScale;

    private void Awake()
    {
        _transform = transform;
        _initScale = _transform.localScale;
    }

    private void Update()
    {
        float scale = (Mathf.RoundToInt(Mathf.Sin(Time.time * _frequency) * 0.5f + 0.5f)) * (_scale - 1.0f) + 1.0f;
        _transform.localScale = _initScale * scale;
    }
}
