---
layout: default
title: Xiletrade
lang: zh-CN
locale: zh_CN
minidesc: PoE 和 PoE2 物品价格查询与游戏覆盖层软件
description: 一款简化《流亡之路》和《流亡之路2》物品价格查询的工具
strings:
  maintained: 项目由
  generated: 此页面由
  download: 下载
  portable: 便携版 (.7z)
  installer: 安装程序 (.exe)
  alt_customsearch: Xiletrade 自定义搜索窗口，包含搜索表单、预设、物品类别以及带建议的属性过滤器
  alt_history: Xiletrade 货币历史窗口，显示所选货币的交易量和汇率图表
  alt_detailed: Xiletrade 详细价格检查，显示物品的上架列表和已上架物品的提示
  alt_searchresult: Xiletrade 自定义搜索结果，显示价格、物品等级、上架时间、卖家以及物品提示
  alt_settings_common: Xiletrade 设置的常用按键标签页，包含各功能的快捷键
  alt_settings_additional: Xiletrade 设置的附加按键标签页，包含聊天命令和私聊快捷键
  repository: 在 GitHub 上查看
  footer: 免费服务需要持续维护才能正常运行。
  issue: 并使用
  latest: 最新版本
  support: 捐赠
---
{% include screenshot.html %}
## 只需一个动作即可检查您的商品价格

<h2 class="keywords"><i>🔍 立即找到最佳价格区间。</i></h2>
<h2 class="keywords"><i>⚡ 即时设置行为。</i></h2>
<h2 class="keywords"><i>🎮 玩得更顺畅，交易更快速！</i></h2>

#### {% include youtube.svg %} &nbsp; [观看快速概览](https://youtu.be/NygCZvZyUX0)<br>

极低的 CPU 使用率。  
平均使用 170 MB 内存，无峰值。  
后台不写入任何数据。  

* * *

<br><br><br>

### {% include direction.svg %} 快速介绍

<h2 class="keywords"><strong>Xiletrade 提供简约界面，以实现与 Path of Exile 和 Path of Exile 2 的最佳整合。</strong></h2>
<section class="intro">
  <p>借助其功能，它将在贸易联赛中提升您的游戏体验，确保流畅的操作。</p>
  <p>作为开源软件，其代码库经过精心打磨，为您提供尽可能优化的工具。 
  下面您将找到关于 POE 物品价格检查的完整可自定义行为列表。</p>
</section>

### {% include knife.svg %} 主要信息

- 平台： **Windows** 64 位，**便携版**压缩包或**网络安装程序**（无需管理员权限）。
- 费用与许可证： 免费且开源（GPLv3），[GitHub 上的源代码]({{ site.github.repository_url }})。
- 支持的游戏： Path of Exile 和 Path of Exile 2。

### {% include pokeball.svg %} 主要功能

- 使用定义的快捷键 ***CTRL+D*** *默认情况下*，快速**检查所有商品的价格**。
- 使用 ***CTRL+R*** 快速打开 **设置** 以修改价格检查行为。
- 通过 POE 进行 **批量物品交换** 以进行快速私下交易。
- **附加组件** 帮助定价物品并检索有关相关物品的更多信息。
	- 与 **poe.prices**、**poeninja**、**poewiki** 和 **poedb** 软链接。
- 检查 **新更新** 并在启动时刷新自定义/官方 **过滤器**。
- 遵守 [第三方政策](https://www.pathofexile.com/developer/docs#policy) 中的 **单一操作** 规则的功能：
	- 通过发送带有关联 **热键** 的预定义消息与 **聊天控制台** 交互
例如快速前往 **藏身处** 或 **交易** 操作。
	- 根据需要将 **鼠标滚轮** 与 **左键单击** 绑定。
- **正则表达式管理器**实用程序。
	- 管理已定义的**正则表达式**列表。
	- 自动粘贴到高亮栏中。  
- **货币历史**，可查看所选货币当前的市场趋势。
- 带有**预设**和**实时搜索**通知的**自定义搜索**工具。

{% include history.html %}

{% include regex.html %}
<br>

### {% include adjust.svg %} 自定义搜索工具

<h2 class="keywords"><i>🧭 交易搜索更简单，直接叠加在游戏之上。</i></h2>

包含一切所需功能的**极简界面**，*默认* 使用 ***CTRL+E*** 打开。

- 带结果列表的**搜索表单**。
	- 像在交易网站或游戏中一样进行简单搜索，让您倍感熟悉。
	- 物品名称或基础类型、类别、稀有度、过滤器、一口价以及**属性组**。
- **预设管理**。
	- 保存您的搜索，**导出**和**导入**以便与好友分享。
	- 确保与**未来赛季**兼容：交易过滤器变化时，会检查并修复预设。
- **实时搜索**，仅面向**高级用户**，边玩边用。
	- 支持**多个实时搜索**，即使关闭窗口也可暂停、恢复或停止。
	- 新物品以**通知**形式显示，提供丰富的操作：  
前往卖家的藏身处、关闭，或**固定**无法立即回复的物品。

{% include customsearch.html %}

实时搜索**在设计上就很安全**，让您始终遵守 GGG 的规则：

- **安全的请求**：遵守交易 API 返回的速率限制，绝不超出其规则进行查询，从而**避免超时**。
- **获取冷却**：新物品的获取在请求之间设有冷却时间，只有最新的结果才会出现在通知中。
- **针对价格检查优化**：实时搜索只占用部分请求额度，**价格检查始终优先**。
- **自动暂停**：结果过多的搜索会被暂停，请**缩小**搜索条件后再恢复。
- **没有任何自动操作**：前往卖家的藏身处始终需要点击通知。

<br>


### {% include install.svg %} 安装和设置

1. [**下载**]({% include installer.html %})并运行最新的**网络安装程序** *（无需管理员权限）*，  
或[**下载**]({% include download.html %})**便携版**并将 **`.7z`** 压缩包**提取**到所需目录。  
2. **右键单击**系统托盘图标以配置或关闭程序。
3. 在启动或设置窗口下选择所需的**游戏版本**、**语言**和**联赛**。
4. **将鼠标悬停**在每个功能上，直到出现工具提示以获得深入解释。  
<br>
{% include settings.html %}
<br>

* * *
> ### {% include mouse.svg %} 工作原理
>
> 以**窗口**或**无边框全屏**模式运行游戏，以便正常工作。
> 确保 Xiletrade **语言**和**联盟**与相应的游戏设置相匹配。
> 一旦 Xiletrade 启动并设置完毕，您现在可以按照以下步骤**检查价格**：
>   1. 将鼠标**放在游戏中的某个物品**上，然后按 ***CTRL+D*** *（默认）*
>   2. 它会从游戏中复制**物品信息描述**并打开 Xiletrade 窗口。
>   3. 如果搜索返回匹配结果，窗口将显示**估计价格**。
>   4. 显示的价格基于**官方交易网站** [PoE 1](https://www.pathofexile.com/trade/search/) 和 [PoE 2](https://www.pathofexile.com/trade2/search/poe2/)。
<p class="accordion"><b>查看价格查询的详细信息</b></p>
<div class="panel"> 请注意，游戏必须处于焦点状态，否则快捷键将无法使用。
<br><br> 例如，如果你保持 Xiletrade 窗口打开，并点击该窗口（使 Xiletrade 获得焦点），然后再尝试进行价格查询，将不会生效。不过，只要你不点击 Xiletrade 窗口，就可以连续查询多个物品的价格。
<br><br> 每当 PoE 失去焦点时，快捷键都会自动注销，并在 PoE 重新获得焦点时重新注册。
<br><br> 此外，请注意不要在短时间内进行过多的价格查询，以免被 PoE 交易网站暂时限制。该工具会在你超出限制时发出警告，并保护你避免遭受大规模超时。
</div>

### {% include chip.svg %} 可自定义行为

* **将主窗口**拖到屏幕上的任意位置。
* 按**当前**项目值或层级范围内的**最小值**进行搜索。
* 使用**鼠标滚轮**更改数值（最小/最大）
* 按住**CTRL**或**SHIFT**键可显示小数值。
* 调整**不透明度**并在窗口失去焦点时**自动关闭**。
* 单击主窗口的**左上角**图标。
* 在地图（配置文件中）中**突出显示****昂贵**和**危险模组**。
* **自动粘贴**来自外部网站的游戏内私聊交易。

```
旨在遵守官方交易网站设定的规则
以避免因数据恢复受限和随时间推移的请求而造成的滥用。
```

* * *

### {% include finger.svg %} 常见问题解答

<p class="accordion"><b>该应用程序是否支持所有语言？</b></p>
<div class="panel">
<b>是的</b>，它被设计为支持所有 Path of Exile 客户端语言。
</div>

<p class="accordion"><b>从其他网站下载该应用程序是否安全？</b></p>
<div class="panel">
出于明显的安全原因，强烈建议从主存储库或关联的 github.io 网站下载 Xiletrade。
</div>

<p class="accordion"><b>我知道使用第三方应用可能会导致游戏被禁。<br>Path of Exile 与 Xiletrade 是否也存在这种情况？</b></p>
<div class="panel">
<b>不</b>，Xiletrade 于 2020 年首次发布，只要您使用本网站上发布的版本，只要 GGG 政策允许使用此类工具，您就永远不会面临被禁的风险。
<br>请注意，游戏发行商永远不会保证第三方工具。
项目维护者有责任确保所提议的工具符合规则。
<br>我邀请您阅读<a target="_blank" rel="noopener noreferrer" href="https://www.pathofexile.com/developer/docs#policy">使用条款</a>，以便交叉引用信息。
</div>

<p class="accordion"><b>为什么该软件是免费的？</b></p>
<div class="panel">
Xiletrade 是一款<b>独立软件</b>，并非旨在通过集成广告来盈利。但是，您可以自行决定<a target="_blank" rel="noopener noreferrer" href="{{ site.github.paypal_url }}">捐赠</a>。如果您喜欢该项目，我们将不胜感激您的贡献，谢谢😊。
</div>