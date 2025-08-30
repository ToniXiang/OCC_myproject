<div align="center">
	<h1>OCC_myproject</h1>
	<p>.NET MAUI - 行控中心模擬應用</p>
</div>

---
## 目錄
- [目錄](#目錄)
- [專案簡介](#專案簡介)
- [技術棧](#技術棧)
- [安裝與啟動](#安裝與啟動)
- [資料夾結構](#資料夾結構)
## 專案簡介

這是一個以生活情境為出發點的「簡易行控中心」模擬應用，介面簡潔、操作直覺，讓一般使用者可以觀察並模擬列車與車站的日常運作。你可以：

- 查看列車即時狀態與預估抵達時間
- 模擬發車、停靠與簡單的操作流程
- 提交事件或回報，觀察系統如何記錄與顯示

適合的使用場景：通勤情境示範、車站作業流程演練、課堂或講解交通調度概念，或作為給非技術人員的展示用範例。

## 技術棧

- 開發語言：C#
- UI：.NET MAUI (net9.0)
- 支援平台：Windows、Android、iOS（視目標環境與 SDK）

## 安裝與啟動

先決條件：已安裝 .NET 7/8/9 SDK（與專案目標版本相符），以及 Visual Studio（含 MAUI workload）或等效開發工具。

- 在專案根目錄（含 `簡易的行控中心.sln`）執行：

    ```powershell
    dotnet restore
    dotnet build -c Debug
    ```

- 或使用 Visual Studio：直接開啟 `簡易的行控中心.sln`，選擇目標平台後執行（F5）。
> - Android / iOS：請先啟動對應模擬器或連接實機，並安裝必要平台 SDK。
> - 注意：部分平台需要額外的原生設定（Android manifest、iOS entitlements、Windows appx manifest 等）。

## 資料夾結構

```
ProjectRoot/
│
├── Platforms/              # 各平台啟動程式與平台資源 (Android/iOS/MacCatalyst/Windows)
│   ├── Android/            # Android 主程式、Manifest、Resources
│   ├── iOS/                # iOS AppDelegate、Info.plist
│   ├── MacCatalyst/
│   └── Windows/            # Windows 特定入口與 manifest
│
├── Views/                  # XAML 頁面（UI）
│   ├── MainPage.xaml
│   ├── HomePage.xaml
│   ├── StationPage.xaml
│   ├── TrainPage.xaml
│   └── EventPage.xaml
│
├── ViewModels/             # 對應 View 的邏輯層
│   ├── HomePageViewModel.cs
│   ├── StationPageViewModel.cs
│   └── TrainPageViewModel.cs
│
├── Models/                 # 資料模型
│   └── TrafficNode.cs
│
├── Services/               # 業務/資料服務
│   └── TrafficDataService.cs
│
├── Converters/             # 值轉換器 (IValueConverter)
│   ├── ContactToCheckedConverter.cs
│   ├── PriorityToStringConverter.cs
│   └── StringNotNullOrEmptyConverter.cs
│
└── Messages/               # 輕量訊息類別（用於 MVVM 訊息傳遞）
		└── SimulationUpdatedMessage.cs

Other files:
- `簡易的行控中心.sln`     # Visual Studio solution
- `簡易的行控中心.csproj`  # Project file
```