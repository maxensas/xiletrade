# <img src="https://i.imgur.com/dhWQgtY.png" width="30" height="30" alt="Xiletrade logo"> Xiletrade
[![Release](https://img.shields.io/github/release/maxensas/xiletrade.svg)](https://github.com/maxensas/xiletrade/releases/) 
[![Discord](https://img.shields.io/static/v1?label=Join&message=Discord&color=7289da&logo=discord)](https://discord.gg/AXP5VntYgA) 
[![Github all releases](https://img.shields.io/github/downloads/maxensas/xiletrade/total.svg)](https://GitHub.com/maxensas/xiletrade/releases/) [![Github latest release](https://img.shields.io/github/downloads/maxensas/xiletrade/latest/total.svg)](https://GitHub.com/maxensas/xiletrade/releases/)

**Xiletrade** is an open source **Overlay**, **Price Checker** and **Helper** tool for **POE** (Path Of Exile) series.  

[<img src="https://github.com/user-attachments/assets/7e2ad410-7508-4348-b968-cc0dbbf5b10e" alt="Open official website" />](https://maxensas.github.io/xiletrade/)
[<img src="https://github.com/user-attachments/assets/c3664da6-b66b-49ef-b3c9-992ae7749dd7" alt="Download latest version" />](https://github.com/maxensas/xiletrade/releases/latest/download/Xiletrade_win-x64.7z)

<img width="275" height="332" src="https://github.com/user-attachments/assets/ba015744-ccc2-4bcb-87e1-e07165fcdb33" alt="Xiletrade overview">

# Project quick access
| Project | License |
|---------|---------|
| [WPF UI](https://github.com/maxensas/xiletrade/tree/master/src/Xiletrade) | [GPLv3](https://github.com/maxensas/xiletrade/blob/master/licenses/LICENSE_Xiletrade) |
| [Library](https://github.com/maxensas/xiletrade/tree/master/src/Xiletrade.Library) | [LGPLv3](https://github.com/maxensas/xiletrade/blob/master/licenses/LICENSE_XiletradeLibrary) |
| [Json generator](https://github.com/maxensas/xiletrade/tree/master/src/Xiletrade.Json) | [MIT](https://github.com/maxensas/xiletrade/blob/master/licenses/LICENSE_XiletradeJson) |


# Everyone can contribute
## Star :star:
The easiest way to show your **support** is by **starring** the repository.

## Reporting Bugs :bug:
Here is a **simple guide** to follow : [Reporting guideline](https://github.com/maxensas/xiletrade/issues/48)   
Please check if the **issue** has already been reported. If so, feel free to provide additional information.

# Developer area

**Standalone application** developed using Microsoft .NET [latest SDK version](https://dotnet.microsoft.com/en-us/download) and [Visual Studio 2022 CE](https://visualstudio.microsoft.com/vs/community/).

## Main references

- Microsoft.Extensions.DependencyInjection  
- CommunityToolkit.Mvvm

## Rules to follow for pull requests
* Make sure to follow the **code style** followed throughout the project.
	- The project in its current state combines both **best practices** to follow for many projects but also **legacy code** that gradually disappears with each new refactoring.

### New features

* Must be developed as a **service** and injected, do not use static class. Either by enriching the main library if it does not bring new dependencies or by creating a new one.
* To be pushed, any **big feature** or **change** should be discussed first on **discord** with the maintainer of the project.
* Must follow the [rules set by GGG](https://www.pathofexile.com/developer/docs/index#policy) if that need to interact with the game or will be rejected.

Xiletrade isn't affiliated with or endorsed by Grinding Gear Games in any way.<br>