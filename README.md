# DumbGuys

[English](README.md), [Korean](README.ko.md), [Japanese](README.ja.md)

A multiplayer team game with connected account, lobby, match and appearance flows.

| Project | Details |
|---|---|
| Status | Development archive |
| Stack | C++, C#, Unity |

[Explore the case](https://ysl-ryudo.github.io/en/work/dumbguys/), [Portfolio](https://ysl-ryudo.github.io/en/)

[![DumbGuys](https://i.ytimg.com/vi/Z4tffAvRjJs/hqdefault.jpg)](https://www.youtube.com/watch?v=Z4tffAvRjJs)

[DumbGuys full play](https://www.youtube.com/watch?v=Z4tffAvRjJs)

## My contribution

Worked on client networking and the login, lobby, loading, game and result flows.

Implemented movement/input separation and gameplay interactions such as moving platforms, pushing, falling and curved pipe transport.

Continued integration and sound work on restore-1121; this branch carries later work not represented by the default-branch snapshot.

## Contribution evidence

[Development branch / 작업 브랜치 / 作業ブランチ](https://github.com/YSL-RyuDo/DumbGuys/tree/restore-1121)

[Pipe transport / 파이프 이동 / パイプ移動](https://github.com/YSL-RyuDo/DumbGuys/commit/359f27621811e1cdc667328cfb55d799340cb3e6)

[Later snapshot / 후속 버전 / 後続版](https://github.com/YSL-RyuDo/DumbGuys/commit/6c30b670a68da9a89d8ad0445de376cd620a21a7)

Commits are evidence of the stated changes, not a percentage-of-authorship score. Third-party assets and teammates' work remain separately credited.

## Implementation and design

Accounts, rooms and player appearance need to carry through the lobby and match flow. A client and server must also be reviewed at compatible revisions.

I worked on account/connection handling, room and player state, store/appearance data and client-side lobby/match integration.

| Area | Implementation |
|---|---|
| Identity | Account and connection handling |
| Lobby to match | Room and player state linked to the Unity client |
| Player data | Store and appearance state |

## How the code works and why it is structured this way

These notes connect the inspected implementation to the constraints it addresses and its tradeoffs. Explanations of design consequences are code analysis, not a claim that every alternative was prototyped. Contribution links identify my changes within team projects; source links retain their original access permissions.

### Match criteria change with time spent waiting

**Implementation**

MatchManager compares queued players by rank, score and level, then widens selected thresholds as waiting time grows. On a match it creates a room and sends the participant/spawn information. Client and server also exchange owned items and appearance data for the surrounding lobby flow.

**Design rationale**

Strict similarity and short waiting time can conflict when few compatible players are queued. Time-dependent thresholds make that tradeoff explicit instead of accepting any opponent immediately or waiting indefinitely for an exact match. Keeping ownership and appearance distinct also separates what a player has from what they currently display.

**Alternatives and the current tradeoff**

First-come pairing favors quick matches while ignoring skill similarity. Fixed similarity limits are predictable but can leave outliers waiting. Widening thresholds over time balances those aims explicitly, but choosing the thresholds still requires population and match-quality evidence.

**Tradeoffs and verification**

The thresholds are implementation policy, not proof of fair matchmaking or shorter observed queues. Player population and cancellation/disconnect behavior need separate evaluation. The client reference below is the restore-1121 branch, so its integration state should not be assumed to equal the default branch.

[DumbGuysServer/MatchManager.cpp](https://github.com/YSL-RyuDo/DumbGuysServer/blob/6c9f9da948008ace439ebfc7a1aeb4b130da12f0/DumbGuysServer/MatchManager.cpp), [DumbGuysServer/OwnedItemManager.cpp](https://github.com/YSL-RyuDo/DumbGuysServer/blob/6c9f9da948008ace439ebfc7a1aeb4b130da12f0/DumbGuysServer/OwnedItemManager.cpp), [DumbGuys/ClientSession.cs](https://github.com/YSL-RyuDo/DumbGuys/blob/6c30b670a68da9a89d8ad0445de376cd620a21a7/Assets/Scripts/Network/ClientSession.cs)

## Open and inspect

Open the project with the Unity version recorded in [ProjectVersion.txt](ProjectSettings/ProjectVersion.txt). Keep its package lock and serialized assets intact. Select the documented branch before opening Unity.

Unity scene: [Assets/Scenes/LoginScene.unity](Assets/Scenes/LoginScene.unity)

[DumbGuysServer](https://github.com/YSL-RyuDo/DumbGuysServer)

## Development and verification scope

Use restore-1121 when inspecting the later client implementation; it has diverged from main. A commit message saying 'complete' is not a substitute for a current end-to-end playtest. Assets and dependencies retain their original credits.

## Related repositories

[DumbGuys](https://github.com/YSL-RyuDo/DumbGuys), Private, access required

[DumbGuysServer](https://github.com/YSL-RyuDo/DumbGuysServer), Private, access required
