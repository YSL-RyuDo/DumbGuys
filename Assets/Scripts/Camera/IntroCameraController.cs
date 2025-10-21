using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;

public class IntroCameraController : MonoBehaviour
{
    [Header("Cameras")]
    [SerializeField] private Camera introCamera;   // 오프닝용 카메라
    [SerializeField] private Camera mainCamera;    // 플레이용 카메라

    [Header("Path Points")]
    [SerializeField] private Transform pointA;     // 시작 위치
    [SerializeField] private Transform pointB;     // 도착 위치

    [Header("UI Elements")]
    [SerializeField] private GameObject backgroundImage; // 처음에 3초 동안 보여줄 이미지
    [SerializeField] private Image fadeImage;            // 페이드용 이미지 (검은색)

    [Header("Settings")]
    [SerializeField] private float backgroundDuration = 3f; // 배경 보여주는 시간
    [SerializeField] private float moveDuration = 2f;       // 카메라 이동 시간
    [SerializeField] private float delayBeforeSwitch = 0.3f; // 이동 후 전환 대기 시간
    [SerializeField] private float fadeDuration = 1f;        // 페이드 시간

    private void Start()
    {
        // 초기 설정
        mainCamera.enabled = false;
        introCamera.enabled = true;

        // 페이드 이미지 완전히 투명하게 설정
        Color c = fadeImage.color;
        c.a = 0f;
        fadeImage.color = c;

        // 인트로 시퀀스 시작
        StartCoroutine(IntroSequence());
    }

    private IEnumerator IntroSequence()
    {
        // 1. 배경 이미지 3초 동안 보여주기
        backgroundImage.SetActive(true);
        yield return new WaitForSeconds(backgroundDuration);
        backgroundImage.SetActive(false);

        // 2. 카메라 이동 시작 (A -> B)
        introCamera.transform.position = pointA.position;
        introCamera.transform.rotation = pointA.rotation;

        introCamera.transform.DOMove(pointB.position, moveDuration).SetEase(Ease.InOutSine);
        introCamera.transform.DORotateQuaternion(pointB.rotation, moveDuration).SetEase(Ease.InOutSine);

        // 이동 완료까지 대기
        yield return new WaitForSeconds(moveDuration + delayBeforeSwitch);

        // 3. 화면 가리기 (페이드 인)
        fadeImage.gameObject.SetActive(true);
        fadeImage.DOFade(1f, fadeDuration);
        yield return new WaitForSeconds(fadeDuration);

        // 4. 메인 카메라로 전환
        introCamera.enabled = false;
        mainCamera.enabled = true;

        // 5. 화면 밝아지기 (페이드 아웃)
        fadeImage.DOFade(0f, fadeDuration);
        yield return new WaitForSeconds(fadeDuration);

        fadeImage.gameObject.SetActive(false);

        Debug.Log("[IntroCameraController] Intro finished, switched to main camera!");
    }
}
