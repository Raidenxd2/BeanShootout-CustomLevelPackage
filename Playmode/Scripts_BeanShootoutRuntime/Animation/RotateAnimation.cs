using UnityEngine;

namespace KillItMyself.Runtime
{
    public class RotateAnimation : MonoBehaviour
    {
        [SerializeField] private TransformType transformType;
        [SerializeField] private Vector3 RotateAngle;

        private RectTransform rt;

        private void Start()
        {
            if (transformType == TransformType.RectTransform)
            {
                rt = GetComponent<RectTransform>();
            }
        }

        private void Update()
        {
            switch (transformType)
            {
                case TransformType.Transform:
                    transform.Rotate(RotateAngle * Time.unscaledDeltaTime);
                    break;

                case TransformType.RectTransform:
                    rt.Rotate(RotateAngle * Time.unscaledDeltaTime);
                    break;
            }
        }
    }

    public enum TransformType
    {
        Transform,
        RectTransform
    }
}