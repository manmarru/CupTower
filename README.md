# CupTower 프로젝트
[시연 영상](https://youtu.be/LJoUJH3Uyo0)  
[프로젝트 개발 기록서](https://drive.google.com/file/d/1L8gsk_rO1SqeiQrtgzJS18Z3tA8TThwx/view?usp=drive_link)
- 보드게임 '펭귄 타워'를 모작했습니다.
- 3명의 플레이어가 참가하는 턴제 게임입니다.
- 각 플레이어는 활성화된 자리에 카드를 냅니다.  
1층을 제외하고 카드를 놓을 때는 바로 아래 놓인 카드들 중 같은 모양의 카드가 있어야 합니다.
- 카드를 가장 많이 낸 플레이어가 라운드를 승리를 가져갑니다.
- 라운드 승리를 가장 많이 한 플레이어가 우승합니다.

# 프로젝트 개요
- 제작 기간: 3주
- 개발 환경  
  언어 : C#, C++(테스트 툴)  
  엔진 : Unity  
  버전관리 : Github Desktop  

# 주요 코드
- [CSocket.cs](https://github.com/manmarru/CupTower/blob/main/Assets/Script/CSocket.cs) : 클라이언트 소켓 코드입니다.  
- [CupManager.cs](https://github.com/manmarru/CupTower/blob/main/Assets/Script/CupManager.cs) : 멀티쓰레딩을 통해 데이터를 처리하고 메인 쓰레드에서 유니티 기능을 처리합니다.  
- [TextManager.cs](https://github.com/manmarru/CupTower/blob/main/Assets/Script/TextManager.cs) : 타이머와 턴 알림 TMP UI 제어를 담당합니다.
