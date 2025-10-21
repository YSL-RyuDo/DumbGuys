using System;

public static class EventBus
{
    // UI 이벤트
    public static event Action OnLoginRequest;
    public static event Action OnChangeModeRequest;
    public static event Action OnQuitRequest;

    // 이벤트 발행 메서드
    public static void RaiseLogin() => OnLoginRequest?.Invoke();
    public static void RaiseChangeMode() => OnChangeModeRequest?.Invoke();
    public static void RaiseQuit() => OnQuitRequest?.Invoke();
}
