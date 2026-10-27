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
                              Description                .cs/.xaml (2026)
src/
│
├── Xiletrade.Library/        # Project Library          ~ 40k
├── Xiletrade.UI.WPF/         # Main WPF App             ~ 14k
├── Xiletrade.UI.Avalonia/    # Avalonia App             ~  2k
├── Xiletrade.Updater/        # Avalonia App             <  1k
├── Xiletrade.DatToJson/      # Console App              ~  3k
├── Xiletrade.Benchmark/      # Benchmarks Project       ~  1k
├── Xiletrade.Test/           # Unit Tests Project       ~  2k
├── Xiletrade.sln             # Visual Studio Solution
```
## 🧪 Load and Unload projects

Project Library need to be loaded in all cases. 

- To work on main app:

Load the main WPF application and unload other projects if you just want to build your own release.

- To run unit tests:

Load Test project (with Library and DatToJson, both referenced by the tests). Via Visual Studio: Test menu > Run All Tests.
From the command line, the same tests as the automated ones (see [Automated tests](#-automated-tests)):
```bash
cd src
dotnet test -c Release --filter-not-namespace "Xiletrade.Test.Exploratory*"
```
Add `-p:SkipOozBuild=true` if CMake is not installed: the tests do not need the ooz decompressor of DatToJson.

- To run benchmarks:

Load Test and Benchmark projects. Select 'Release' mode then run Benchmark.

- To run json generator:

Load Xiletrade.DatToJson project. Its first build also builds the ooz decompressor (CMake and a C++ compiler needed, on Windows: Visual Studio with the C++ workload).
Run it with a command, from the repository root: `fullupdate` for the json files generated from the game, `trade` for the ones of the trade website.
See [Xiletrade.DatToJson README](https://github.com/maxensas/xiletrade/blob/master/src/Xiletrade.DatToJson/README.md).

## 🔄 Create a contribution

Here are the steps to contribute:

- Read this file entirely and agree with the rules.
- Fork this repository
- Create a new branch (feature or fix):
```bash
git checkout -b feature/NewFeature
```
- Make your changes

Run and validate your code in your personal repo, **including the unit tests** (see above): they will run again on your pull request.

- Commit properly:
```bash
git commit -m "Adding fix or feature XYZ"
```
- Push to your fork:
```bash
git push origin feature/NewFeature
```
- Open a Pull Request to the **dev** branch

And now make yourself comfortable and wait for the code review.

## ✅ Automated tests

Unit tests are a **safeguard**: they run automatically on GitHub ([CI workflow](https://github.com/maxensas/xiletrade/blob/master/.github/workflows/CI.yml)) before any pull request can be merged.

- They run when a pull request is **opened**, **reopened**, and on **every new push** to its branch, whatever the target branch.
- Every test of `Xiletrade.Test` runs, on Windows with the .NET SDK of the project, except the `Xiletrade.Test.Exploratory` namespace (studies and comparisons, run by hand).
- The result is shown in the **Checks** of the pull request. A pull request whose tests fail is **not merged**: fix the code (or the test, if the expected behavior changed on purpose) and push again, the tests run again.
- Tests run once more before each release, when dev is merged into master.

Write tests for your changes when possible:
- in `Xiletrade.Test`, outside of `Exploratory` (not run by the automated tests), e.g. `Xiletrade.Test/CI/<feature>`;
- named like the existing ones: `_01_What_Should_Happen`, `_02_...`, with the *Arrange / Act / Assert* steps.

## 🔁 Data updates

The json files of `Xiletrade.Library/Data/Lang` and `Xiletrade/Data/Lang` are **generated**: do not edit them by hand (except `ParsingRules.json`).
They are updated by GitHub workflows using [Xiletrade.DatToJson](https://github.com/maxensas/xiletrade/blob/master/src/Xiletrade.DatToJson/README.md),
which run the tests too, then open a pull request to dev, validated by the maintainer:
- `dat-to-json.yml`: files generated from the game (bases, mods, monsters, words, gems, item classes), after a game update;
- `trade-data.yml`: files of the trade website (leagues, filters, currency, item types).

## 🧼 Code Style

This project uses C#, an object-oriented programming language with advanced technical concepts commonly used in 
**IT services** (industrial and tertiary sectors) to enhance projects **readability**, **maintainability** and improve collaborative work.

Please **respect** general C# conventions and adapt your work to the code style used.

## 🔧 Limitations and Improvements

The **automated tests** only cover part of the base code, which limits its flexibility and is therefore a constraint for new eyes wishing to contribute to the repository: 
new tests are welcome. Don't be afraid when submitting new idea.

Furthermore, regarding the **price check functionality** :  
Info descriptions of PoE1/2 items are not automatically tested as they evolve over time at the publisher's discretion, which represents a considerable maintenance burden.

## 📚 .NET References

Using **DI** implementation and **MVVM** pattern with **XAML** frameworks:
- [Microsoft.Extensions.DependencyInjection](https://github.com/dotnet/runtime/tree/main/src/libraries/Microsoft.Extensions.DependencyInjection)
- [CommunityToolkit.Mvvm](https://github.com/CommunityToolkit/MVVM-Samples)

Using **archive** utility:
- [SharpCompress](https://github.com/adamhathcock/sharpcompress)

Using **parsing** algorithm:
- [Raffinert.FuzzySharp](https://github.com/Raffinert/FuzzySharp)

Using **game archives** (json generator):
- [LibGGPK3](https://github.com/aianlinb/LibGGPK3)

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