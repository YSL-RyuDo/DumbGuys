using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;

[Serializable]
public class ServerResponse
{
    public string type;
    public bool result;
    public string msg;
    public string nickname; // 서버에서 추가로 보내는 닉네임
}

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager Instance { get; private set; }

    private TcpClient client;
    private NetworkStream stream;

    public event Action<string> OnMessageReceived;  // UI 메시지용
    public event Action OnLoginSuccess;             // 로그인 성공 알림

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        Connect("127.0.0.1", 7777);
    }

    public void Connect(string ip, int port)
    {
        try
        {
            client = new TcpClient();
            client.Connect(ip, port);
            stream = client.GetStream();
            Debug.Log("[Client] Connected to Server.");
            BeginRead();
            OnMessageReceived?.Invoke("Connected to server.");
        }
        catch
        {
            OnMessageReceived?.Invoke("Connection failed.");
        }
    }

    public void SendJson(object data)
    {
        if (client == null || !client.Connected)
        {
            OnMessageReceived?.Invoke("Not connected to server.");
            return;
        }

        string json = JsonUtility.ToJson(data);
        byte[] body = Encoding.UTF8.GetBytes(json);
        byte[] len = BitConverter.GetBytes((uint)body.Length);
        stream.Write(len, 0, 4);
        stream.Write(body, 0, body.Length);
        stream.Flush();

        Debug.Log($"[Client] Sent: {json}");
    }

    private void BeginRead()
    {
        new Thread(() =>
        {
            try
            {
                var br = new BinaryReader(stream);
                while (true)
                {
                    uint len = br.ReadUInt32();
                    byte[] buf = br.ReadBytes((int)len);
                    string json = Encoding.UTF8.GetString(buf);
                    HandleServerResponse(json);
                }
            }
            catch
            {
                UnityMainThreadDispatcher.Instance().Enqueue(() =>
                {
                    OnMessageReceived?.Invoke("Disconnected from server.");
                });
            }
        })
        { IsBackground = true }.Start();
    }

    private void HandleServerResponse(string json)
    {
        try
        {
            var res = JsonUtility.FromJson<ServerResponse>(json);
            if (res == null) return;

            string message = res.msg;

            switch (res.type)
            {
                case "login_result":
                    if (res.result)
                    {
                        message = $"Login successful. Welcome, {res.nickname}!";

                        UnityMainThreadDispatcher.Instance().Enqueue(() =>
                        {
                            ClientSession.Instance.SetLoginData(res.nickname);
                            OnLoginSuccess?.Invoke();
                        });
                    }
                    else
                    {
                        message = $"Login failed: {res.msg}";
                    }
                    break;

                case "signup_result":
                    message = res.result ? "Sign-up successful." : res.msg;
                    break;

                case "player_info":
                    Debug.Log("[Client] Received player info packet.");
                    UnityMainThreadDispatcher.Instance().Enqueue(() =>
                    {
                        OnMessageReceived?.Invoke(json);
                    });
                    return;

                case "appearance_info":
                    Debug.Log("[Client] Received appearance info packet.");
                    UnityMainThreadDispatcher.Instance().Enqueue(() =>
                    {
                        OnMessageReceived?.Invoke(json);
                    });
                    return;

                case "tutorial_info": // 튜토리얼 정보 수신 처리 추가
                    Debug.Log("[Client] Received tutorial info packet.");
                    UnityMainThreadDispatcher.Instance().Enqueue(() =>
                    {
                        OnMessageReceived?.Invoke(json);
                    });
                    return;

                case "enemy_info":
                    Debug.Log("[Client] Received enemy appearance info packet.");
                    UnityMainThreadDispatcher.Instance().Enqueue(() =>
                    {
                        OnMessageReceived?.Invoke(json); // LoadingSceneController가 받음
                    });
                    return;

                case "match_wait":
                    Debug.Log("[LoadingScene] Waiting for opponent...");
                    // TODO: 로딩 UI에 “매칭 중...” 표시
                    break;

                case "match_found":
                    Debug.Log("[LoadingScene] Match found! Requesting enemy appearance...");
                    UnityMainThreadDispatcher.Instance().Enqueue(() =>
                    {
                        OnMessageReceived?.Invoke(json); // 로딩씬이 여기서 적 외형 요청함
                    });
                    return;

                case "match_ready_ack":
                    Debug.Log("[LoadingScene] Match ready acknowledged - waiting for opponent...");
                    UnityMainThreadDispatcher.Instance().Enqueue(() =>
                    {
                        OnMessageReceived?.Invoke("Waiting for opponent to be ready...");
                    });
                    return;
                case "match_start":
                    Debug.Log("[Client] Received match_start, forwarding to scene...");
                    UnityMainThreadDispatcher.Instance().Enqueue(() =>
                    {
                        OnMessageReceived?.Invoke(json);
                    });
                    return;
                case "map_choose_start":
                    Debug.Log("[Client] Received map_choose_start, forwarding to ChooseScene...");
                    UnityMainThreadDispatcher.Instance().Enqueue(() =>
                    {
                        OnMessageReceived?.Invoke(json);
                    });
                    return;
                case "map_wait":
                    Debug.Log("[ChooseScene] Waiting for opponent to select map...");
                    UnityMainThreadDispatcher.Instance().Enqueue(() =>
                    {
                        OnMessageReceived?.Invoke("Waiting for opponent's map selection...");
                    });
                    return;
                case "map_result":
                    Debug.Log("[ChooseScene] Received final map result.");
                    UnityMainThreadDispatcher.Instance().Enqueue(() =>
                    {
                        OnMessageReceived?.Invoke(json); // ChooseSceneController가 받아서 처리함
                    });
                    return;
                case "spawn_info":
                    Debug.Log("[GameScene] Received spawn info packet.");
                    UnityMainThreadDispatcher.Instance().Enqueue(() =>
                    {
                        OnMessageReceived?.Invoke(json); // GameSceneController에서 처리
                    });
                    return;
                case "enemy_move":
                    Debug.Log("[GameScene] Received enemy movement update.");
                    UnityMainThreadDispatcher.Instance().Enqueue(() =>
                    {
                        OnMessageReceived?.Invoke(json); // GameSceneController에서 처리
                    });
                    return;
                case "player_hit":
                    Debug.Log("[Network] Received player_hit packet from server.");
                    UnityMainThreadDispatcher.Instance().Enqueue(() =>
                    {
                        OnMessageReceived?.Invoke(json); // GameSceneController에서 처리
                    });
                    return;
                case "enemy_attack_anim":
                    Debug.Log("[GameScene] Received enemy attack animation packet.");
                    UnityMainThreadDispatcher.Instance().Enqueue(() =>
                    {
                        OnMessageReceived?.Invoke(json); // GameSceneController에서 처리
                    });
                    return;
                case "enemy_die_anim":
                    Debug.Log("[GameScene] Received enemy_die_anim packet from server.");
                    UnityMainThreadDispatcher.Instance().Enqueue(() =>
                    {
                        OnMessageReceived?.Invoke(json); // GameSceneController에서 실제 애니메이션 실행
                    });
                    return;

                case "score_update":
                    Debug.Log("[GameScene] Received score update packet.");
                    UnityMainThreadDispatcher.Instance().Enqueue(() =>
                    {
                        OnMessageReceived?.Invoke(json); // GameSceneController UI 업데이트
                    });
                    return;

                case "round_end":
                    Debug.Log("[GameScene] Round ended → returning to ChooseScene...");
                    UnityMainThreadDispatcher.Instance().Enqueue(() =>
                    {
                        OnMessageReceived?.Invoke(json); // GameSceneController에서 처리
                    });
                    return;

                case "game_end":
                    Debug.Log("[GameScene] Game ended → forwarding to ResultScene...");
                    UnityMainThreadDispatcher.Instance().Enqueue(() =>
                    {
                        OnMessageReceived?.Invoke(json); // GameSceneController에서 처리
                    });
                    return;
                case "game_result":
                    Debug.Log("[ResultScene] Received game result packet.");
                    UnityMainThreadDispatcher.Instance().Enqueue(() =>
                    {
                        OnMessageReceived?.Invoke(json);
                        // ResultSceneController에서 처리
                    });
                    return;

                case "error":
                    message = $"Error: {res.msg}";
                    break;

                default:
                    message = $"Unknown packet: {res.type}";
                    break;
            }

            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                OnMessageReceived?.Invoke(message);
            });
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[Client] Failed to parse response: {e.Message}");
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                OnMessageReceived?.Invoke("Failed to parse server response.");
            });
        }
    }

    private void OnApplicationQuit()
    {
        try { stream?.Close(); } catch { }
        try { client?.Close(); } catch { }
    }
}
