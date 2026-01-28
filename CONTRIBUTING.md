# 🤝 Contribute as a developer to Xiletrade

**Thank you** for your interest in contributing to **Xiletrade**, an application developed in C# with XAML Frameworks from .NET environement. Whether you're a beginner or an experienced developer, any help is welcome 🙌

---

## 🧰 Prerequisites

Before you begin, make sure you have:

- Windows 10 or higher
- The [**Latest .NET SDK Version**](https://dotnet.microsoft.com/en-us/download)
- [Visual Studio 2022+](https://visualstudio.microsoft.com/vs/community/) (VS Community Edition) or compatible IDE of your choice.
- GitHub account

**Components to install:**
- .NET Desktop Development from SDK (include WPF apps)
- Git for Windows

**For VS users:**
- You will be prompted to install all needed addons after opening the solution.
- Install Avalonia addon only if you want to work on Updater project or make your own UI.
---

## 🚀 Clone and launch the project


### 1. Fork this repository, then clone it locally:
```bash
git clone https://github.com/maxensas/Xiletrade.git
cd Xiletrade
```
### 2. Open The .sln file with your IDE

## 📁 Project Structure
```bash
src/
│
├── Xiletrade.Library/        # Project Library
├── Xiletrade.UI.WPF/         # Main WPF App
├── Xiletrade.UI.Avalonia/    # Avalonia App
├── Xiletrade.Updater/        # Avalonia App
├── Xiletrade.Json/           # Console App
├── Xiletrade.Benchmark/      # Benchmarks Project
├── Xiletrade.Test/           # Unit Tests Project
├── Xiletrade.sln             # Visual Studio Solution
```
## 🧪 Load and Unload projects

Project Library need to be loaded in all cases. 

- To work on main app:

Load the main WPF application and unload other projects if you just want to build your own release.

- To run unit tests:

Load Test and Benchmark projects. Via Visual Studio: Test menu > Run All Tests

- To run benchmarks:

Load Test and Benchmark projects. Select 'Release' mode then run Benchmark.

- To run json generator:

Load Json projects. Just run the console App.

## 🔄 Create a contribution

Here are the steps to contribute:

- Read this file entirely and agree with the rules.
- Fork this repository
- Create a new branch (feature or fix):
```bash
git checkout -b feature/NewFeature
```
- Make your changes

Run and validate your code in your personal repo

- Commit properly:
```bash
git commit -m "Adding fix or feature XYZ"
```
- Push to your fork:
```bash
git push origin feature/NewFeature
```
- Open a Pull Request to the master or dev branch

And now make yourself comfortable and wait for the code review.

## 🧼 Code Style

This project uses C#, an object-oriented programming language with advanced technical concepts commonly used in 
**IT services** (industrial and tertiary sectors) to enhance projects **readability**, **maintainability** and improve collaborative work.

Please **respect** general C# conventions and adapt your work to the code style used.

## 📚 .NET References

Using **DI** implementation and **MVVM** pattern with **XAML** frameworks:
- [Microsoft.Extensions.DependencyInjection](https://github.com/dotnet/runtime/tree/main/src/libraries/Microsoft.Extensions.DependencyInjection)
- [CommunityToolkit.Mvvm](https://github.com/CommunityToolkit/MVVM-Samples)

Using **archive** utility:
- [SharpCompress](https://github.com/adamhathcock/sharpcompress)

Using **parsing** algorithm:
- [Raffinert.FuzzySharp](https://github.com/Raffinert/FuzzySharp)

**Stick to the goal:** make the application with a low level of dependency on third-party references but also avoid reinventing the wheel when implementing new ideas.

## 🛠️ New features

- Must be developed as a **service** and injected. Either by enriching the main library if it does not bring new dependencies or by creating a new one.
- To be pushed, any **big feature** or **change** should be discussed first on **discord** with the maintainer of the project.
- Must follow the [rules set by GGG](https://www.pathofexile.com/developer/docs/index#policy) if that need to interact with the game or will be rejected.

## 📜 Code of Conduct

We ask all contributors to read and **respect** our [Code of Conduct](https://github.com/maxensas/xiletrade/blob/master/CODE_OF_CONDUCT.md).

## 📣 Need help?

**You can:**

- [Open an issue](https://github.com/maxensas/xiletrade/issues)

- [Join the discord](https://discord.gg/AXP5VntYgA)

- [Contact the maintainer](mailto:xiletrade@gmail.com)  

# 🎉 Thank you!

**We appreciate every contribution.** Your names will be added to the project and release acknowledgments!