# 簡易的行控中心 (Simple Operational Control Center)

一個以 Windows Forms 製作的列車運行模擬與控管工具。提供路線地圖可視化、列車進出站模擬、優先權與目的地調整、緊急事件處理等功能，適合作為基礎行控邏輯與 UI 的教學/示範專案

## 功能特色
> 我已經放棄學習 .NET MAIU 了，這個專案純粹是為了複習 WinForms 而已
- 路線地圖可視化顯示：顯示各站點及軌道路段，並以顏色提示軌道狀態。
- 多列車模擬：支援多列車同時運行、進站/出站、加減速與煞車距離估算。
- 列車操作：
  - 啟動/煞車
  - 臨停/發車
  - 目的地變更（需在進站狀態）
  - 列車優先權調整
- 車站管理：車站優先權調整、查看站台數量、估算列車抵達時間。
- 緊急事件處理：選擇事件、指定列車、勾選通報對象、填寫說明後送出。
- 介面強化：
  - 自訂 ComboBox 置中繪製
  - 視窗可拖曳
  - 即時日期/時間顯示

## 系統需求
- 作業系統：Windows 11
- .NET Framework：4.7.2（需安裝 Developer Pack）
- 開發工具：Visual Studio 2022（.NET 桌面開發工作負載）

## 建置與執行
1. 下載或 Clone 此儲存庫
2. 以 Visual Studio 開啟 `簡易的行控中心.csproj`
3. 還原套件：Solution/專案 右鍵 > Restore NuGet Packages
4. 設為啟始專案並執行（F5

## 常見錯誤與排解
- 錯誤訊息：`Your project does not reference ".NETFramework,Version=v4.7.2" framework...`
  - 請確認已安裝「.NET Framework 4.7.2 Developer Pack」。
  - 使用 Visual Studio 的還原動作，或在命令列使用 `nuget restore`，避免使用 `dotnet restore`。
  - 專案檔已設定 `<TargetFrameworkVersion>v4.7.2</TargetFrameworkVersion>`；若 CI 工具強制要求，可額外加入 `<TargetFrameworks>net472</TargetFrameworks>`（保持傳統專案設定）。

## 專案結構
- `Form1.cs`：主視窗與主要控制流程（計時器、事件處理、列車控制）。
- `Form1.Designer.cs`：主視窗 UI 版面配置。
- `TrafficComponents/TrafficNode.cs`：核心模型（例如 `Train`、`Track`、`Station` 及相關邏輯）。
- `DrawImg.cs`：載入與繪製路線圖資產。
- `Properties/*`：專案組態、資源、自動產生程式碼。

## 操作說明（重點）
- 進入程式後，按「開始」啟動模擬；再次按下切換為「暫停」。
- 從下拉選單選擇列車：
  - 「啟動/煞車」控制列車速度。
  - 「停靠站/發車」讓列車臨停或離站。
  - 更改目的地（列車需在進站狀態）。
  - 調整列車優先權（影響進站順序與權重）。
- 選擇站點可查看該站預估到達時間與狀態，並可調整車站優先權。
- 緊急事件：選擇事件與列車、勾選通報對象、填寫說明後送出。

## 技術重點
- .NET Framework 4.7.2 / C# 7.3
- Windows Forms（WinForms）
- 計時器驅動模擬（`timer1` 時鐘、`timer2` 模擬迴圈）
- 以簡化物理參數模擬列車加減速與煞車距離估算
- 多執行緒/非同步控制（`CancellationTokenSource`、`async/await`）

## 貢獻與回饋
- 歡迎提交 Issue/PR，或提出功能建議
- 若有錯誤或改進建議，請描述步驟與環境，以利重現與修正
