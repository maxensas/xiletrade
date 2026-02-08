---
layout: default
title: Xiletrade
lang: zh-TW
minidesc: 《流亡黯道Path of Exile》價格查詢
description: 開源工具，可以輕鬆查看《流亡黯道PoE 1和PoE 2》中的物品價格。
strings:
  maintained: 專案由
  generated: 此頁面由
  download: 下載
  repository: 在 GitHub 上查看
  footer: 免費服務需要持續維護才能正常運作。
  issue: 並使用
  latest: 最新版本
  support: 捐贈
---
{% include screenshot.html %}
## 一個動作可以檢查您的商品價格

<h2 class="keywords"><i>🔍 立即找到最佳價格範圍。</i></h2>
<h2 class="keywords"><i>⚡ 即時設置行為。</i></h2>
<h2 class="keywords"><i>🎮 玩得更順暢，交易更快速！</i></h2>

#### {% include youtube.svg %} &nbsp; [觀看快速概覽](https://youtu.be/NygCZvZyUX0)<br>

低 CPU 使用率。  
使用最多 250 MB 記憶體，無峰值。  
後台不寫入任何資料。  

* * *

<br><br><br>

### {% include direction.svg %} 快速介紹

<h2 class="keywords"><strong>Xiletrade 提供簡約介面，以實現與 Path of Exile 1 和 2 的最佳整合。</strong></h2>
<section class="intro">
  <p>借助其功能，它將在貿易聯賽中提升您的遊戲體驗，確保操作流暢。</p>
  <p>作為開源軟體，其程式碼庫經過精心打磨，為您提供盡可能優化的工具。 
  下面您將找到關於 POE 物品價格檢查的完整可自訂行為列表。</p>
</section>

### {% include pokeball.svg %} 主要功能

- 使用定義的快速鍵 ***CTRL+D*** *預設* 對所有商品進行快速**價格檢查**。
- 使用 ***CTRL+R*** 快速開啟 **設定** 來修改價格檢查行為。
- **大量物品交換**透過 POE 進行快速耳語交易。
- **附加組件** 協助定價物品並檢索有關相關物品的更多資訊。
	- 與 **poe.prices**、**poeninja**、**poewiki** 和 **poedb** 軟連結。
- 啟動時檢查**新更新**並刷新自訂/官方**過濾器**。
- 遵守 [第三方政策](https://www.pathofexile.com/developer/docs#policy) 中的 **單一操作** 規則的功能： 
	- 通过发送带有相关**热键**的预定义消息与**聊天控制台**进行交互，例如快速前往**藏身处**或**交易**操作。
	- 依需要將**滑鼠滾輪**與**左鍵點選**綁定。
- **正規表示式管理器**實用程式。
	- 管理定義的**正規表示式**清單。
	- 自動貼上到高亮欄。  

{% include regex.html %}
<br>

### {% include install.svg %} 安裝與設定

1. [**下載**]({% include download.html %})最新版本並**提取** **`.7zip`** 檔案至所需目錄。
Xiletrade 是一款**便攜式應用程式**，啟動時不需要安裝任何其他東西。
2. **右鍵單擊**系統托盤圖示來配置或關閉程式。
3. 在啟動或設定視窗下選擇所需的**遊戲版本**、**語言**和**聯盟**。
4. 將**滑鼠懸停**在每個功能上，直到出現工具提示以獲得深入的解釋。  
<br>
{% include settings.html %}
<br>
* * *

> ### {% include mouse.svg %} 工作原理
> 
> 請在**視窗**或**無邊框全螢幕**模式下執行遊戲，以確保正常運作。
> 確保 Xiletrade **語言**和**聯盟**與相應的遊戲設定相符。
> 一旦 Xiletrade 啟動並設定完畢，您現在就可以按照以下步驟進行**價格檢查**： 
> 1. 將滑鼠**放在遊戲中的某個物品**上，然後按 ***CTRL+D*** *（預設）* 
> 2. 它會從遊戲中複製**物品資訊描述**並開啟 Xiletrade 視窗。
> 3. 如果搜尋傳回符合的結果，視窗將顯示**預估價格**。
> 4. 顯示的價格是根據**官方貿易網站** [PoE 1](https://www.pathofexile.com/trade/search/) 和 [PoE 2](https://www.pathofexile.com/trade2/search/poe2/)。
<p class="accordion"><b>查看價格查詢的詳細資訊</b></p>
<div class="panel"> 請注意，遊戲必須處於焦點狀態，否則快捷鍵將無法運作。
<br><br> 例如，如果你讓 Xiletrade 視窗保持開啟，並點擊該視窗（使 Xiletrade 取得焦點），然後再嘗試進行價格查詢，將不會成功。不過，只要你不點擊 Xiletrade 視窗，就可以連續查詢多個物品的價格。
<br><br> 每當 PoE 失去焦點時，快捷鍵會自動解除註冊，並在 PoE 重新取得焦點時再次註冊。
<br><br> 此外，請注意不要在短時間內進行過多的價格查詢，以避免被 PoE 交易網站暫時限制。工具會在你超出限制時提醒你，並保護你避免遭遇大規模的逾時。
</div>


### {% include chip.svg %} 可自訂的行為

* **將主視窗**拖曳到螢幕上的任意位置。
* 按**目前**項目值或層級範圍內的**最小值**進行搜尋。
* 使用**滑鼠滾輪**變更數值（最小/最大） 
	* 按住**CTRL**或**SHIFT**鍵可調整小數值。
* 調整**不透明度**和視窗失去焦點時的**自動關閉**。
	* 點選主視窗的**左上角**圖示。
* 地圖中（設定檔中）**反白**昂貴**和**危險的模組**。
* **自動貼上** 來自外部網站的遊戲內耳語交易。

```
旨在遵守官方贸易网站设定的规则
以避免因数据恢复受限和请求时间延长而造成的滥用。
```
* * *

### {% include finger.svg %} 常見問題 

<p class="accordion"><b>該應用程式是否支援所有語言？ </b></p>
<div class="panel"> <b>是的</b>，它旨在支援所有Path Of Exile用戶端語言。</div>

<p class="accordion"><b>從其他網站下載該應用程式安全嗎？</b></p>
<div class="panel">出於明顯的安全原因，強烈建議從主儲存庫或相關的 github.io 網站下載 Xiletrade。</div>

<p class="accordion"><b>我知道使用第三方應用程式可能會導致遊戲被禁。 <br>Xiletrade 與 Path Of Exile 的情況也是這樣嗎？</b></p>
<div class="panel"> <b>否</b>，Xiletrade 於 2020 年首次發布，只要您使用此網站上發布的版本，只要 GGG 政策允許使用此類工具，您就永遠不會有被禁止的風險。
<br>請注意，遊戲發行商永遠不會保證第三方工具。
專案維護者有責任確保所提議的工具符合規則。
<br>我邀請您閱讀<a target="_blank" rel="noopener noreferrer" href="https://www.pathofexile.com/developer/docs#policy">使用條款</a>，以便交叉引用資訊。
</div>

<p class="accordion"><b>為什麼該軟體是免費的？ </b></p>
<div class="panel"> Xiletrade 是一款<b>獨立軟體</b>，並不旨在像慣例那樣透過整合廣告來獲取利潤。然而，您可以根據自己的意願進行<a target="_blank" rel="noopener noreferrer" href="{{ site.github.paypal_url }}">捐贈</a>。如果您喜歡這個項目，我們將非常感激您的貢獻，謝謝您😊。
</div>

