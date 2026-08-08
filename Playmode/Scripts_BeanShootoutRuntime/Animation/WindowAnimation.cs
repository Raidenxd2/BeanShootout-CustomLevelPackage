using Cysharp.Threading.Tasks;
using UnityEngine;
using LitMotion;
using LitMotion.Extensions;

namespace KillItMyself.Runtime
{
    public class WindowAnimation : MonoBehaviour
    {
        [SerializeField] private Ease UIEase1;
        [SerializeField] private Ease UIEase2;

        [SerializeField] private float duration;

        [SerializeField] private CanvasGroup canvasGroup;

        [SerializeField] private bool DoAnimationOnEnable = true;
        [SerializeField] private bool DoAnimationOnDisable;

        [SerializeField] private GameObject Canvas;
        
        private void OnEnable()
        {
            if (!DoAnimationOnEnable)
            {
                return;
            }
            
            OpenAnimationInternal().Forget();
        }

        public async UniTask OpenAnimationAsync()
        {
            await OpenAnimationInternal();
        }

        private async UniTask OpenAnimationInternal()
        {
            transform.localScale = new Vector3(0, 0, 0);
            
#pragma warning disable CS4014
            LMotion.Create(Vector3.zero, Vector3.one, duration)
                .WithEase(UIEase1)
                .WithScheduler(MotionScheduler.TimeUpdateIgnoreTimeScale)
                .BindToLocalScale(transform);
#pragma warning restore CS4014

            await LMotion.Create(0f, 1f, duration)
                .WithEase(UIEase1)
                .WithScheduler(MotionScheduler.TimeUpdateIgnoreTimeScale)
                .Bind(x => canvasGroup.alpha = x);
        }

        private void OnDisable()
        {
            if (DoAnimationOnDisable)
            {
                OnDisableAsync().Forget();
            }
        }

        private async UniTaskVoid OnDisableAsync()
        {
            await UniTask.WaitForEndOfFrame();
            if (this != null)
            {
                gameObject.SetActive(true);
                Close();
            }
        }

        public void Close()
        {
            CloseAnimationInternal().Forget();
        }

        public async UniTask CloseAsync()
        {
            await CloseAnimationInternal();
        }

        private async UniTask CloseAnimationInternal()
        {
            bool reEnabledaod = false;
            if (DoAnimationOnDisable)
            {
                reEnabledaod = true;
                DoAnimationOnDisable = false;
            }
            
#pragma warning disable CS4014
            LMotion.Create(Vector3.one, Vector3.zero, duration)
                .WithEase(UIEase2)
                .WithScheduler(MotionScheduler.TimeUpdateIgnoreTimeScale)
                .WithOnComplete(() => gameObject.SetActive(false))
                .BindToLocalScale(transform);
#pragma warning restore CS4014

            await LMotion.Create(1f, 0f, duration)
                .WithEase(UIEase2)
                .WithScheduler(MotionScheduler.TimeUpdateIgnoreTimeScale)
                .WithOnComplete(() => gameObject.SetActive(false))
                .Bind(x => canvasGroup.alpha = x);

            if (Canvas)
            {
                Canvas.SetActive(false);
            }

            if (reEnabledaod)
            {
                DoAnimationOnDisable = true;
            }
        }
    }
}