---
layout: default
title: Xiletrade
lang: ko-KR
minidesc: POE 오버레이 및 가격 확인기
description: Path of Exile 1 및 Path of Exile 2의 아이템 가격 확인을 간편하게 해주는 도구
strings:
  maintained: 프로젝트는 다음에 의해 유지 관리됨
  generated: 이 페이지는 다음에 의해 생성됨
  download: 다운로드
  repository: GitHub에서 보기
  footer: 제대로 작동하려면 지속적인 유지 관리가 필요한 무료 서비스입니다.
  issue: 그리고 사용
  latest: 최신 릴리스
  support: 기부
---
{% include screenshot.html %}
## 한 가지 작업으로 품목 가격 확인

<h2 class="keywords"><i>🔍 즉시 최적 가격대를 찾으세요.</i></h2>
<h2 class="keywords"><i>⚡ 즉시 행동 설정.</i></h2>
<h2 class="keywords"><i>🎮 더 부드럽게 플레이하고, 더 빠르게 거래하세요!</i></h2>

#### {% include youtube.svg %} &nbsp; [개요 보기](https://youtu.be/NygCZvZyUX0)<br>

낮은 CPU 사용량.  
최대 250 MB RAM 사용, 스파이크 없음.  
백그라운드에서 데이터 기록 없음.  

* * *

<br><br><br>

### {% include direction.svg %} 빠른 소개

<h2 class="keywords"><strong>Xiletrade는 Path of Exile 1 및 2와 최적의 통합을 위한 미니멀리스트 인터페이스를 제공합니다.</strong></h2>
<section class="intro">
  <p>기능 덕분에 거래 리그를 플레이할 때 삶의 질을 향상시키며 원활한 경험을 보장합니다.</p>
  <p>오픈 소스로서 코드베이스는 가장 최적화된 도구를 제공하기 위해 세심한 주의를 기울입니다. 
  아래에서 POE에서 아이템 가격 확인과 관련된 모든 사용자 정의 가능한 동작 목록을 확인할 수 있습니다.</p>
</section>

### {% include pokeball.svg %} 주요 기능

- 정의된 단축키 ***CTRL+D*** *기본적으로* 모든 품목에 대한 빠른 **가격 확인기**.
- **CTRL+R**로 **설정**을 빠르게 열어 가격 확인 동작을 수정합니다.
- POE를 통한 빠른 속삭임 거래를 위한 **대량 아이템 교환**.
- 아이템 가격 책정을 돕고 관련 아이템에 대한 추가 정보를 검색하는 **애드온**.
	- **poe.prices**, **poeninja**, **poewiki** 및 **poedb**와 소프트 링크.
- 시작 시 **새로운 업데이트**를 확인하고 사용자 지정/공식 **필터**를 새로 고칩니다.
- [타사 정책](https://www.pathofexile.com/developer/docs#policy)의 **단일 작업** 규칙을 따르는 기능:
	- **은신처**로의 빠른 이동이나 **거래** 동작과 같은 연관된 **단축키**로 미리 정의된 메시지를 보내서 **채팅 콘솔**과 상호 작용합니다.
	- 필요에 따라 **마우스 휠**을 **왼쪽 클릭**과 바인딩합니다.
- **정규 표현식 관리자** 유틸리티.
	- 정의된 **정규 표현식** 목록을 관리합니다.
	- 강조 표시줄에 자동 붙여넣기.

{% include regex.html %}
<br>

### {% include install.svg %} 설치 및 설정

1. 최신 버전을 [**다운로드**]({% include download.html %})하고 원하는 디렉토리에 **`.7zip`** 아카이브를 **추출**합니다.
Xiletrade는 **휴대용 애플리케이션**이며 실행 시 추가로 아무것도 설치하지 않습니다.
2. 시스템 트레이 아이콘을 **마우스 오른쪽 버튼으로 클릭**하여 프로그램을 구성하거나 닫습니다.
3. 시작 또는 설정 창에서 원하는 **게임 버전**, **언어** 및 **리그**를 선택합니다.
4. 각 기능에 **마우스를 올려놓으면 자세한 설명을 제공하는 도구 설명이 나타납니다.**  
<br>
{% include settings.html %}
<br>

* * *
> ### {% include mouse.svg %} 작동 방식
>
> 게임을 **창 모드** 또는 **테두리 없는 전체 화면**에서 실행하여 제대로 작동하도록 합니다.
> Xiletrade **언어**와 **리그**가 해당 게임 설정과 일치하는지 확인합니다.
> Xiletrade가 실행되고 설정되면 이제 다음 프로세스에 따라 **가격 확인**을 할 수 있습니다.
> 1. 게임에서 **아이템** 위에 마우스를 놓고 ***CTRL+D*** *(기본값)*를 누릅니다.
> 2. 게임에서 **아이템 정보 설명**을 복사하여 Xiletrade 창을 엽니다.
> 3. 검색 결과가 일치하는 경우 창에 **예상 가격**이 표시됩니다.
> 4. 표시된 가격은 **공식 거래 웹사이트** [PoE 1](https://www.pathofexile.com/trade/search/) 및 [PoE 2](https://www.pathofexile.com/trade2/search/poe2/)를 기반으로 합니다.
<p class="accordion"><b>가격 확인에 대한 세부 정보 확인</b></p>
<div class="panel"> 게임이 포커스를 가지고 있어야 하며, 그렇지 않으면 키보드 단축키가 작동하지 않습니다.
<br><br> 예를 들어 Xiletrade 창을 열린 상태로 두고 해당 창을 클릭하여 포커스를 Xiletrade로 이동한 후 다시 가격 확인을 시도하면 작동하지 않습니다. 하지만 Xiletrade 창을 클릭하지 않는 한 여러 아이템의 가격을 연속으로 확인할 수 있습니다.
<br><br> PoE가 포커스를 잃을 때마다 키보드 단축키는 자동으로 등록 해제되며, PoE가 다시 포커스를 얻으면 재등록됩니다.
<br><br> 또한 PoE 거래 웹사이트에서 일시적으로 차단되는 것을 방지하기 위해 가격 확인을 너무 연속해서 수행하지 않도록 주의하세요. 이 도구는 제한을 초과하면 경고를 표시하고, 대규모 타임아웃으로부터 사용자를 보호합니다.
</div>

### {% include chip.svg %} 사용자 지정 가능한 동작

* **메인 창**을 화면에서 원하는 곳으로 드래그합니다.
* **현재** 아이템 값 또는 티어 범위의 **최소값**으로 검색합니다.
* **마우스 휠**로 숫자 값을 변경합니다(최소/최대)
* 소수점 값을 입력하려면 **CTRL** 또는 **SHIFT** 키를 누릅니다.
* **불투명도**를 조정하고 창이 초점을 잃었을 때 **자동으로 닫습니다**.
* 메인 창의 **왼쪽 상단 모서리** 아이콘을 클릭합니다.
* 지도에서 **비싼** 및 **위험한 모드**를 **강조 표시**합니다(구성 파일).
* 외부 웹사이트에서 게임 내 **자동 붙여넣기** 귓속말 거래.

```
공식 거래 웹사이트에서 정한 규칙을 준수하도록 설계되어
제한된 데이터 복구 및 시간이 지남에 따른 요청으로 인한 남용을 방지합니다.
```
* * *

### {% include finger.svg %} FAQ

<p class="accordion"><b>애플리케이션이 모든 언어를 지원합니까?</b></p>
<div class="panel">
<b>예</b>, 모든 Path of Exile 클라이언트 언어를 지원하도록 설계되었습니다.
</div>

<p class="accordion"><b>다른 웹사이트에서 애플리케이션을 다운로드하는 것이 안전합니까?</b></p>
<div class="panel">
명백한 보안상의 이유로 Xiletrade는 관련 메인 저장소 또는 github.io 웹사이트에서 다운로드하는 것이 좋습니다.
</div>

<p class="accordion"><b>타사 앱을 사용하면 게임에서 금지될 수 있다는 것을 알고 있습니다.<br>Xiletrade를 사용하는 Path of Exile의 경우도 마찬가지인가요?</b></p>
<div class="panel">
<b>아니요</b>, Xiletrade는 2020년에 처음 출시되었으며 이 사이트에 게시된 버전을 사용하는 한 GGG 정책에서 이러한 도구 사용을 허용하는 한 금지될 위험이 없습니다.
<br>게임 게시자는 타사 도구를 보장하지 않습니다.
제안된 도구가 규칙을 준수하는지 확인하는 것은 프로젝트 관리자의 몫입니다.
<br>정보를 교차 참조하려면 <a target="_blank" rel="noopener noreferrer" href="https://www.pathofexile.com/developer/docs#policy">이용 약관</a>을 읽어보세요.
</div>

<p class="accordion"><b>소프트웨어가 무료인 이유는 무엇인가요?</b></p>
<div class="panel">
Xiletrade는 <b>독립형 소프트웨어</b>이며, 관례적으로 광고를 통합하여 수익을 창출하기 위한 것이 아닙니다. 그러나 귀하의 재량에 따라 <a target="_blank" rel="noopener noreferrer" href="{{ site.github.paypal_url }}">기부</a>를 할 수 있습니다. 프로젝트가 마음에 드신다면 기부를 부탁드립니다. 감사합니다 😊.
</div>