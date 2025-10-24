using System;
using JetBrains.Annotations;

//
// === 공용 패킷 구조 ===
//

[Serializable]
public class PlayerInfoRequest
{
    public string cmd = "get_player_info";
    public string nickname;

    public PlayerInfoRequest(string nickname)
    {
        this.nickname = nickname;
    }
}

[Serializable]
public class PlayerInfoResponse
{
    public string type;
    public string nickname;
    public int level;
    public int exp;
    public int coin0;
    public int coin1;
    public int icon;
    public int iconBorder;
    public int rank;
    public int rankScore;
}

[Serializable]
public class AppearanceRequest
{
    public string cmd = "get_appearance_info";
    public string nickname;
}

[Serializable]
public class AppearanceResponse
{
    public string type = "appearance_info";
    public string nickname;
    public int FullBody;
    public int Hat;
    public int Accessories;
    public int Faces;
    public int Glasses;
    public int Gloves;
    public int Hairstyle;
    public int Outerwear;
    public int Pants;
    public int Shoes;
}

[Serializable]
public class TutorialInfoRequest
{
    public string cmd = "get_tutorial_info"; // 서버로 보낼 명령 이름
    public string nickname;                  // 요청 대상 플레이어
}


[Serializable]
public class TutorialInfoResponse
{
    public string type = "tutorial_info";  // 서버에서 식별용
    public string nickname;
    public bool tutorialDone;              // 튜토리얼 완료 여부
}

[System.Serializable]
public class TutorialCompleteRequest
{
    public string cmd = "set_tutorial_done";
    public string nickname;
}


[Serializable]
public class MatchRequest
{
    public string cmd = "request_match";
    public string nickname;

    public MatchRequest(string nickname)
    {
        this.nickname = nickname;
    }
}


[Serializable]
public class MatchReadyRequest
{
    public string cmd = "match_ready";
    public string nickname;

    public MatchReadyRequest(string nickname)
    {
        this.nickname = nickname;
    }
}

[Serializable]
public class MapSelectPacket
{
    public string cmd = "select_map";
    public string nickname;
    public int mapIndex; // -1, 0, 1, 2
}

[System.Serializable]
public class MapChooseStartRequest
{
    public string cmd = "map_choose_start_request";
    public string nickname;

    public MapChooseStartRequest(string nickname)
    {
        this.nickname = nickname;
    }
}


// === 패킷 정의 ===
[Serializable]
public class MapChooseStartPacket
{
    public string type;
    public string playerNickname;
    public string enemyNickname;
    public int playerScore;
    public int enemyScore;
    public int[] usedMaps;
}


[Serializable]
public class MapResultPacket
{
    public string type = "map_result";
    public int selectedMap; // 0,1,2
}

[System.Serializable]
public class MapWaitPacket
{
    public string type;
    public string msg;
}


[System.Serializable]
public class SpawnInfoResponse
{
    public string type;          // "spawn_info"
    public string playerNickname;
    public string enemyNickname;
    public string playerPosition; // "A" or "B"
}

[System.Serializable]
public class SpawnInfoRequest
{
    public string cmd = "get_spawn_info";
    public string nickname;
}

[Serializable]
public class MovePacket
{
    public string cmd = "player_move";
    public string nickname;
    public float px, py, pz;
    public float rx, ry, rz;
}

[System.Serializable]
public class EnemyMovePacket
{
    public string type;       // "enemy_move"
    public string nickname;   // 이동한 상대 닉네임
    public float px, py, pz;  // 위치
    public float rx, ry, rz;  // 회전
}

[Serializable]
public class PlayerWinPacket
{
    public string cmd = "player_win";
    public string nickname;
}

[System.Serializable]
public class ScoreUpdatePacket
{
    public string type;
    public string player1;
    public string player2;
    public int score1;
    public int score2;
}

[System.Serializable]
public class GameEndPacket
{
    public string type;
    public string winner;
}

[Serializable]
public class GameResultRequest
{
    public string cmd = "get_game_result";
    public string nickname;
}

[Serializable]
public class GameResultPacket
{
    public string type;     // 항상 "game_result"
    public string winner;   // 승자 닉네임
    public string enemy;    // 상대 닉네임
}


[Serializable]
public class LeaveRoomPacket
{
    public string cmd = "leave_room";
    public string nickname;
}


