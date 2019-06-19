using UnityEngine;

[ExecuteInEditMode]
public class OcclusionCamera : MonoBehaviour
{
    private const string kReplacementTag = "RenderType";

    [SerializeField]
    private Camera _camera;
    [SerializeField]
    private Shader _shader;
    [SerializeField]
    private Renderer _target;

    private void Awake()
    {
#if UNITY_EDITOR
        if (_camera != null)
#endif
        {
            _camera.enabled = false;
        }
    }

    private void Update()
    {
#if UNITY_EDITOR
        if (_camera != null &&
            _shader != null &&
            _target != null)
#endif
        {
            var bounds = _target.bounds;
            var size = bounds.size;
            var maxSize = Mathf.Max(size.x, size.y);
            var orthoSize = maxSize * 0.5f;
            _camera.orthographicSize = orthoSize;
            _camera.RenderWithShader(_shader, kReplacementTag);
        }
    }
}
