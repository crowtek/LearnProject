using UnityEngine;

public class InteractionUIController : MonoBehaviour
{
    [SerializeField] private GameObject bubbleRoot;
    private Transform _mainCameraTransform;

    private void Start()
    {
        if (Camera.main != null)
            _mainCameraTransform = Camera.main.transform;
    }
    private void LateUpdate()
    {
        if (_mainCameraTransform == null) return;

        // Makes the UI face the camera directly
        transform.LookAt(transform.position + _mainCameraTransform.rotation * Vector3.forward,
                         _mainCameraTransform.rotation * Vector3.up);
    }

    public void Show(Transform target)
    {
        bubbleRoot.SetActive(true);
        transform.position = target.position;
    }

    public void Hide() => bubbleRoot.SetActive(false);
}