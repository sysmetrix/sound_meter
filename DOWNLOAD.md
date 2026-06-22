# Sound Meter 다운로드 안내

컴퓨터에 익숙하지 않은 분은 아래 링크 하나만 누르면 됩니다.

**[SoundMeterSetup.exe 다운로드](https://github.com/sysmetrix/sound_meter/raw/main/dist/SoundMeterSetup.exe)**

받은 파일을 실행하면 설치가 시작됩니다.

## 다른 프로그램도 설치해야 하나요?

대부분의 Windows 11 PC에서는 필요 없습니다.

`SoundMeterSetup.exe`만 설치하면 됩니다. .NET SDK, Visual Studio, Git, Python 같은 개발 도구는 필요하지 않습니다.

단, Sound Meter는 새 오디오 장치를 만들어주는 프로그램은 아닙니다. 이미 Windows에 보이는 출력 장치 2개를 골라 전환합니다.

VoiceMeeter 장치를 선택해서 쓰려면 VoiceMeeter는 사용자의 PC에 미리 설치되어 있어야 합니다.

## Windows 경고가 뜨면

처음 실행할 때 `Windows의 PC 보호` 또는 `알 수 없는 게시자` 경고가 뜰 수 있습니다.

이유는 설치 파일이 아직 코드 서명 인증서로 서명되어 있지 않기 때문입니다. 다운로드한 주소가 아래 GitHub 저장소가 맞는지 확인하세요.

```text
https://github.com/sysmetrix/sound_meter
```

신뢰할 수 있는 경로에서 받은 파일이 맞다면 `추가 정보`를 누른 뒤 `실행`을 선택하면 됩니다.

불안하면 실행하지 않아도 됩니다.
