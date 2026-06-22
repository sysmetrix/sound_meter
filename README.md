# Sound Meter

Windows 11용 사운드 출력 장치 전환 유틸리티입니다.

작업 표시줄 트레이에서 원하는 출력 장치 2개를 선택해두고, 클릭 또는 단축키로 빠르게 전환할 수 있습니다. 스피커, USB 헤드셋, HDMI 모니터/TV, VoiceMeeter 출력 장치처럼 Windows에 등록된 출력 장치를 지원합니다.

## 바로 다운로드

아래 링크를 누르면 설치 파일을 바로 받을 수 있습니다.

**[SoundMeterSetup.exe 다운로드](https://github.com/sysmetrix/sound_meter/raw/main/dist/SoundMeterSetup.exe)**

GitHub 화면이 낯선 분은 초록색 `Code` 버튼을 누르지 않아도 됩니다. 위의 다운로드 링크만 누르면 됩니다.

설치 파일 이름:

```text
SoundMeterSetup.exe
```

설치 후 작업 표시줄 오른쪽 아래의 트레이 영역에서 `Sound Meter` 아이콘을 찾으면 됩니다.

## 추가 설치가 필요한가요?

대부분의 Windows 11 사용자는 `SoundMeterSetup.exe`만 설치하면 바로 사용할 수 있습니다.

별도로 설치할 필요가 없는 것:

- .NET SDK
- Visual Studio
- Git
- Python
- 별도 오디오 전환 프로그램

주의할 점:

- Sound Meter는 새 오디오 장치나 가상 오디오 드라이버를 설치하지 않습니다.
- 이미 Windows에 보이는 출력 장치 2개를 골라 전환하는 프로그램입니다.
- VoiceMeeter 장치를 선택해서 쓰려면 VoiceMeeter는 사용자의 PC에 미리 설치되어 있어야 합니다.
- USB 헤드셋, 스피커, HDMI 모니터처럼 이미 연결된 장치는 추가 설치 없이 선택할 수 있습니다.

## Windows 경고가 뜨는 경우

처음 실행할 때 Windows가 아래와 비슷한 경고를 보여줄 수 있습니다.

```text
Windows의 PC 보호
알 수 없는 게시자
```

이 경고는 프로그램이 위험하다는 뜻이라기보다, 아직 코드 서명 인증서로 서명된 유명한 프로그램이 아니라서 Windows가 조심하라고 알려주는 메시지입니다.

안심하고 실행하려면 아래 내용을 먼저 확인하세요.

- 다운로드한 곳이 `https://github.com/sysmetrix/sound_meter`가 맞는지 확인합니다.
- 파일 이름이 `SoundMeterSetup.exe`인지 확인합니다.
- 이 프로그램은 사운드 출력 장치 전환용 트레이 유틸리티입니다.

신뢰할 수 있는 경로에서 받은 파일이 맞다면 Windows 경고 창에서 `추가 정보`를 누른 뒤 `실행`을 선택하면 됩니다.

불안하면 실행하지 않아도 됩니다. 공개 배포 규모가 커질 경우에는 코드 서명 인증서를 적용해 이런 경고를 줄이는 것이 좋습니다.

## 주요 기능

- 트레이 아이콘으로 항상 백그라운드 실행
- Windows 11 스타일의 간단한 출력 전환 팝업
- 사용자가 직접 `Output 1`, `Output 2` 선택 가능
- 기본 단축키 `Ctrl + Alt + A`로 두 출력 장치 전환
- 단축키 변경 가능
- 컴퓨터별 설정 자동 저장
- Windows 시작 시 자동 실행 옵션
- 설치 파일 제공

## 설치

사용자는 아래 파일 하나만 실행하면 됩니다.

```text
SoundMeterSetup.exe
```

개발자용 저장소 안에서는 설치 파일이 아래 위치에 있습니다.

```text
dist\SoundMeterSetup.exe
```

설치 위치:

```text
%LocalAppData%\Programs\SoundMeter
```

설치 과정에서 선택할 수 있는 항목:

- Windows 시작 시 자동 실행
- 바탕화면 바로가기 생성
- 설치 후 바로 실행

관리자 권한 없이 현재 사용자 계정에 설치하는 방식입니다.

## 사용 방법

1. `SoundMeterSetup.exe`로 설치합니다.
2. 작업 표시줄 오른쪽 트레이 영역에서 `Sound Meter` 아이콘을 찾습니다.
3. 트레이 아이콘을 좌클릭하면 빠른 전환 팝업이 열립니다.
4. 원하는 출력 장치를 클릭하면 즉시 기본 출력 장치가 바뀝니다.
5. `Ctrl + Alt + A`를 누르면 설정된 두 장치가 번갈아 전환됩니다.

트레이 아이콘이 보이지 않으면 Windows의 숨겨진 아이콘 영역 `^` 안을 확인하세요.

## 처음 설정하기

다른 컴퓨터에 설치한 사용자는 자기 PC의 오디오 장치를 직접 선택할 수 있습니다.

1. 트레이 아이콘을 우클릭합니다.
2. `Settings`를 클릭합니다.
3. `Output 1`에서 첫 번째 출력 장치를 선택합니다.
4. `Output 2`에서 두 번째 출력 장치를 선택합니다.
5. 필요하면 표시 이름과 단축키를 변경합니다.
6. `Save`를 누릅니다.

설정 예:

```text
Output 1: Speakers / USB Audio Device
Output 2: Voicemeeter Input / HDMI TV / Bluetooth Headset
Hotkey: Ctrl + Alt + A
```

설정 파일은 컴퓨터별로 아래 위치에 저장됩니다.

```text
%AppData%\SoundMeter\settings.xml
```

## 제거

시작 메뉴에 생성된 `Sound Meter 제거`를 실행하면 제거할 수 있습니다.

제거 시 삭제되는 항목:

- 설치된 프로그램 파일
- 바탕화면 바로가기
- 시작 메뉴 바로가기
- Windows 시작 프로그램 등록
- Sound Meter 설정 파일

수동 제거가 필요하면 아래 항목을 삭제하면 됩니다.

```text
%LocalAppData%\Programs\SoundMeter
%AppData%\SoundMeter
```

Windows 시작 프로그램 등록을 해제하려면 작업 관리자 또는 Windows 설정의 시작 앱 목록에서도 끌 수 있습니다.

## 배포 방법

배포할 때는 아래 파일 하나만 전달하면 됩니다.

```text
dist\SoundMeterSetup.exe
```

소스에서 다시 빌드하려면 PowerShell에서 다음 명령을 실행합니다.

```powershell
.\build.ps1
```

빌드 결과:

```text
bin\SoundMeter.exe
bin\AudioProbe.exe
dist\SoundMeterSetup.exe
```

`AudioProbe.exe`는 개발/진단용 도구입니다. 일반 사용자에게는 배포하지 않아도 됩니다.

## 문제 해결

`Hotkey unavailable`이 표시되는 경우:

다른 프로그램이 같은 단축키를 이미 사용 중입니다. 트레이 우클릭 후 `Settings`에서 다른 키를 선택하세요.

출력 장치가 보이지 않는 경우:

Windows 설정에서 해당 장치가 연결되어 있고 활성화되어 있는지 확인하세요. Bluetooth 장치나 HDMI 장치는 연결 상태에 따라 목록에서 사라질 수 있습니다.

VoiceMeeter 장치가 너무 많이 보이는 경우:

`Settings`에서 실제로 전환하고 싶은 두 장치만 선택하면 됩니다. 예를 들어 `Voicemeeter Input`과 `USB Audio Device`만 선택해두면 단축키는 그 두 장치 사이에서만 동작합니다.

설치 파일 실행 시 Windows SmartScreen 경고가 나오는 경우:

현재 설치 파일은 코드 서명 인증서로 서명되어 있지 않습니다. 그래서 Windows가 `알 수 없는 게시자` 또는 `Windows의 PC 보호` 경고를 보여줄 수 있습니다. 다운로드 주소가 이 저장소가 맞는지 확인한 뒤 실행하세요. 공개 배포 규모가 커질 경우 코드 서명 인증서를 적용하는 것을 권장합니다.

## 기술 메모

Windows는 기본 출력 장치를 변경하는 공식 고수준 API를 제공하지 않습니다. Sound Meter는 Windows 오디오 전환 도구들이 흔히 사용하는 `IPolicyConfig` COM 인터페이스를 사용해 기본 출력 장치를 변경합니다.

전환 시 다음 역할의 기본 출력 장치를 함께 변경합니다.

```text
Console
Multimedia
Communications
```

## 지원 범위

지원:

- Windows 11
- Windows 10 대부분의 환경
- 스피커, USB 헤드셋, HDMI/DP 오디오, Bluetooth 오디오
- VoiceMeeter 가상 출력 장치

현재 미지원:

- 3개 이상 출력 장치 순환
- 앱별 출력 장치 라우팅
- 입력 장치 전환
- 5.1/7.1 채널 구성 변경
- 이퀄라이저 또는 볼륨 믹서 기능
