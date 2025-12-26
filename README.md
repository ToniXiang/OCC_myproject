# 簡易的行控中心
> Simple Operational Control Center

## 專案簡介

模擬鐵路行控中心的基本功能，提供列車與車站管理介面
- 路線地圖可視化顯示：顯示各站點及軌道路段，並以顏色提示軌道狀態
- 多列車模擬：支援多列車同時運行、進站/出站、加減速與煞車距離估算
- 列車操作：
  - 啟動/煞車
  - 臨停/發車
  - 目的地變更（需在進站狀態）
  - 列車優先權調整
- 車站管理：車站優先權調整、查看站台數量、估算列車抵達時間
- 緊急事件處理：選擇事件、指定列車、勾選通報對象、填寫說明後送出
- 介面強化：
  - 自訂 ComboBox 置中繪製
  - 視窗可拖曳
  - 即時日期/時間顯示
## 技術棧
> 放棄維護資料庫代碼，有需要可到先前版本查看資料庫相關實作
- .NET Framework 4.7.2 / C# 7.3
- Windows Forms（WinForms）
- 開發工具：Visual Studio 2026

## 建置與執行
1. 下載或 Clone 此儲存庫
2. 以 Visual Studio 開啟 `簡易的行控中心.csproj`
3. 還原套件：Solution/專案 右鍵 > Restore NuGet Packages
4. 設為啟始專案並執行（F5)

## 資料夾結構

```
OCC_myproject/
│
├── bin/                              # 編譯輸出目錄
│   └── Debug/
│       ├── 簡易的行控中心.exe         # 可執行檔
│       ├── 簡易的行控中心.exe.config  # 應用程式配置檔
│       ├── ...資源
│
├── obj/                              # 編譯中間檔案
│   └── Debug/
│       ├── *.resources               # 資源編譯檔
│       └── TempPE/                   # 臨時 PE 檔案
│
├── Properties/                       # 專案屬性資料夾
│   ├── AssemblyInfo.cs               # 組件資訊
│   ├── Resources.Designer.cs         # 資源設計檔
│   ├── Resources.resx                # 資源檔
│   ├── Settings.Designer.cs          # 設定設計檔
│   └── Settings.settings             # 應用程式設定
│
├── TrafficComponents/                # 交通元件模組
│   └── TrafficNode.cs                # 交通節點類別（車站、列車等）
│
├── src/                              # 資源素材目錄
│
├── App.config                  # 應用程式主配置檔
├── DrawImg.cs                  # 繪圖相關功能
├── Form1.cs                    # 主視窗邏輯
├── Form1.Designer.cs           # 主視窗設計器生成代碼
├── Form1.resx                  # 主視窗資源檔
├── Program.cs                  # 程式進入點
├── README.md                   # 專案說明文件
├── 簡易的行控中心.csproj        # C# 專案檔
├── 簡易的行控中心.csproj.user   # 使用者專案設定
└── 簡易的行控中心.sln           # Visual Studio 解決方案檔
```

