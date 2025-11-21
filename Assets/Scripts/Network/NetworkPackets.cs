using System;
using JetBrains.Annotations;
using static Unity.Burst.Intrinsics.X86.Avx;

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


[System.Serializable]
public class TutorialInfoResponse
{
    public string type;
    public string nickname;
    public bool tutorialDone;
    public int tutorialStage;
}

[System.Serializable]
public class TutorialProgressRequest
{
    public string cmd;            // "set_tutorial_stage"
    public string nickname;       // 플레이어 닉네임
    public int tutorialStage;     // 현재 완료된 튜토리얼 스테이지 (예: 0 -> 1)
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


[System.Serializable]
public class AttackRequest
{
    public string cmd = "attack";
    public string attacker;
    public string target;
    public int damage;
}

[System.Serializable]
public class PlayerHitPacket
{
    public string type;
    public string attacker;
    public string target;
    public int damage;
}

[System.Serializable]
public class AttackAnimRequest
{
    public string cmd = "attack_anim";
    public string attacker;
}

[System.Serializable]
public class EnemyAttackAnimPacket
{
    public string type;
    public string attacker;
}

[System.Serializable]
public class DeathAnimRequest
{
    public string cmd = "death_anim";  // 서버에서 구분용 키워드
    public string attacker;            // 죽은 플레이어 닉네임
}

[System.Serializable]
public class EnemyDieAnimPacket
{
    public string type;      // "enemy_die_anim"
    public string attacker;  // 죽은 플레이어 닉네임
}

[System.Serializable]
public class ChatRequest
{
    public string cmd = "chat";
    public string sender;
    public string message;
}

<<<<<<< Updated upstream
[System.Serializable]
public class ChatMessagePacket
{
    public string type;
    public string sender;
    public string message;
}


[System.Serializable]
public class PushRequest
{
    public string cmd;      // "push"
    public string attacker;
    public string target;
}

[System.Serializable]
public class PushHitPacket
{
    public string type;
    public string attacker;
    public string target;
}

[System.Serializable]
public class LogoutRequest
{
    public string cmd;
    public string nickname;
}
=======
public class CoinInfoRequest
{
    public string cmd = "get_coin_info";
    public string nickname;

    public CoinInfoRequest(string nickname)
    {
        this.nickname = nickname;
    }
}


[System.Serializable]
public class CoinInfoResponse
{
    public string type;   // "coin_info"
    public int coin0;     // 예: 골드
    public int coin1;     // 예: 젬
}


[Serializable]
public class StoreItemData
{
    public int id;
    public string name;
    public int price;
}

[System.Serializable]
public class StoreItemRequest
{
    public string cmd = "get_store_items";
    public string category; // Accessories, Hat, Shoes 등등
}

[Serializable]
public class OwnedItemRequest
{
    public string cmd = "get_owned_items";
    public string nickname;
    public string category;
}

[Serializable]
public class StoreItemResponse
{
    public string type;
    public string category;
    public StoreItemData[] items;
}

[Serializable]
public class OwnedItemsResponse
{
    public string type;         // "owned_items"
    public string category;     // ex: "Accessories"
    public int[] owned;         // ex: [1,0,1,0,0,0]
}

[System.Serializable]
public class BuyItemRequest
{
    public string cmd = "buy_item";
    public string nickname;
    public string category;
    public int id;
}

[System.Serializable]
public class BuyItemResult
{
    public string type;
    public bool result;
    public string category;
    public int id;
    public int coin1;
    public string msg;
}
>>>>>>> Stashed changes
