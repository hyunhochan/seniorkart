## '서라운드라이버'

### 1. 프로젝트 소개
#### [Unity3D를 사용한 생생한 사운드를 느낄 수 있는 실시간 멀티플레이 레이싱 게임]

‘서라운드라이버’는 실시간 멀티플레잉 레이싱 게임으로서, 플레이어는 맵에 적용된 생생한 사운드를 직접 체감하며 가장 먼저 목적지에 도착하게 되면 승리하는 게임이다. 사용자는 두 종류의 맵 중 하나를 선택하여 맵마다 다른 체험을 경험할 수 있다.

본 프로젝트는 Unity를 기반으로 진행되며, Unity와 C#을 사용하여 카트바디의 주행, 다양한 상호작용 등을 구현한다. 본 프로젝트는 기본적으로 다른 사용자와의 멀티플레이를 상정하여 개발되었다. 멀티플레이의 경우에는 실시간 네트워크 대전을 구현했으며 이를 위해 Unity Gaming Services에서 제공되는 Relay와 Lobby를 사용하여 MatchMaking 및 P2P 연결을 가능하게 한다.  또한 Google Firebase를 이용해 서버와 사용자간의 정보와 기록을 저장, 연결한다.

#### 목표
  1. 자동차 물리법칙을 직접 구현함으로서 사용자에게 원활한 조작감과 플레이 경험을 구현 - 완료
  2. Unity Gaming Services를 활용하여 다른 사용자와의 멀티플레이 기능을 구현 - 완료
  3. 사용자의 주행기록을 저장하여 스스로와의 대결을 해볼 수 있는 고스트드라이버 기능을 구현 - 미완료
  4. 기존 레이싱게임과의 차별요소를 제작하기 위한 다양하고 실감나는 사운드를 구현 - 완료

위의 4가지의 목표를 달성함을 목적으로 진행되는 프로젝트이다.

클라이언트 제작에서는 Unity/C#을 사용하며, 필요한 에셋(카트 디자인, 맵, 소리 등)은 Blender를 사용하여 제작한다.

멀티플레이를 위한 서버의 경우에는 유저의 데이터를 관리하기 위해 Firebase를 사용하며, 멀티플레이 진행에는 Unity Relay+Robby 서비스를 활용해 매치메이킹 시스템을 제작한다.


#### 사용기술
![image](https://github.com/hyunhochan/seniorkart/assets/162757631/7c946b82-15d8-4b14-a91e-1d7e4a86a15e)


### 2. 시연영상

### 3. 팀소개
|이름|학번|이메일|역할|
|------|---|---|---|
|유현호|1891248|homnbvcx12@gmail.com|카트 물리엔진 및 UI 제작, 기능 부여|
|유현우|1891247|robindalnim@naver.com|로그인/멀티플레이 구현, 네트워크 클라이언트 동기화, 게임시스템 구현|
|이동규|1971183|leedonggyu1290@gmail.com|네트워크 클라이언트 동기화, 3D 사운드 제작|
|최민수|1771441|chl6863@gmail.com|3D 이펙트, 그래픽, 사운드 제작|

### 4. 사용방법
#### 실행방법
실행 후 MultiPlay 또는 SinglePlay를 눌러 방을 생성하신 후 게임을 시작하시면 됩니다. 현재는 멀티플레이의 경우 1명 이상의 인원이 있어야 실행이 가능해야 하지만 테스트를 진행중이므로 이러한 제한이 없는 상황입니다.

#### 에디터 파일

 - 압축 해재 후 UnityHub 실행
 - 유니티 버전을 2022.3.20f1버전으로 설정

### 5. 포스터
![image](https://github.com/hyunhochan/seniorkart/assets/162757631/ab0ae006-bb57-4c94-90d3-48b3a7005092)

### 6. 상장
![Adobe-Scan-2024년-6월-3일](https://github.com/hyunhochan/seniorkart/assets/39771617/af8cb5fd-8e97-48fb-b5f9-9fffa11d71d2)



