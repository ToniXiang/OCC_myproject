using System.Reflection;

namespace 簡易的行控中心;

public partial class SettingPage : ContentPage
{
    public SettingPage()
	{
		InitializeComponent();
        ThemeSwitch.IsToggled = Preferences.Get("DarkTheme", true);
        IntroductionSwitch.IsToggled = Preferences.Get("ShowIntro", true);
        LanguagePicker.SelectedIndex = Preferences.Get("LanguageIndex", 0);
        IntroText.IsVisible = IntroductionSwitch.IsToggled;
    }

    protected override async void OnAppearing()
    {
        if (!IntroText.IsVisible) return;
        base.OnAppearing();

        string message = "簡易的行控中心(Simple operational control center)\r\n\r\n" +
            "1. 使用者可以自行操作列車\r\n" +
            "2. 時刻掌握當前時間\r\n" +
            "3. 使用模擬計算來預估列車的當前狀態\r\n" +
            "5. 路線地圖可視化展示\r\n" +
            "6. 緊急事件處理\r\n" +
            "7. 用戶回饋連結資料庫\r\n" +
            "8. 更多功能日後開發(維護和檢修計劃、能源管理連接資料庫)\r\n\r\n" +
            $"© 2025 陳國翔\r\n\r\nhttps://chenguoxiang940.github.io/project.html\r\n\r\n" +
            $"功能介绍 - 版本 {Assembly.GetExecutingAssembly().GetName().Version}";

        await DisplayAlert("簡介", message, "OK");
    }
    private void ThemeSwitch_Toggled(object sender, ToggledEventArgs e)
    {
        bool isDark = e.Value;
        Preferences.Set("DarkTheme", isDark);
        SetTheme(isDark);
        ShowStatus("主題已切換");
    }

    private void IntroductionSwitch_Toggled(object sender, ToggledEventArgs e)
    {
        bool show = e.Value;
        Preferences.Set("ShowIntro", show);
        IntroText.IsVisible = show;
        ShowStatus("說明已" + (show ? "顯示" : "隱藏"));
    }

    private void LanguagePicker_SelectedIndexChanged(object sender, EventArgs e)
    {
        Preferences.Set("LanguageIndex", LanguagePicker.SelectedIndex);
        ShowStatus("語言已選擇：" + (string)LanguagePicker.SelectedItem);
    }

    private void ClearSettings_Clicked(object sender, EventArgs e)
    {
        Preferences.Clear();
        ThemeSwitch.IsToggled = true;
        IntroductionSwitch.IsToggled = true;
        LanguagePicker.SelectedIndex = 0;
        IntroText.IsVisible = true;
        SetTheme(ThemeSwitch.IsToggled);
        ShowStatus("設定已重置");
    }
    public void SetTheme(bool isDarkTheme)
    {
        var dictionaries = Application.Current.Resources.MergedDictionaries;
        var themeDictionary = dictionaries
            .FirstOrDefault(d => d.GetType().Name.Contains("Theme"));

        if (themeDictionary != null)
            dictionaries.Remove(themeDictionary);

        ResourceDictionary newTheme;
        if (isDarkTheme)
            newTheme = new 簡易的行控中心.Resources.Styles.DarkTheme();
        else
            newTheme = new 簡易的行控中心.Resources.Styles.LightTheme();

        dictionaries.Add(newTheme);
    }
    private void OnFeedbackClicked(object sender, EventArgs e)
    {
        Launcher.OpenAsync(new Uri("https://github.com/ChenGuoXiang940/OCC_myproject/issues"));
    }
    private CancellationTokenSource _statusCts;
    private async void ShowStatus(string message)
    {
        StatusLabel.Text = message;
        _statusCts?.Cancel();
        _statusCts = new CancellationTokenSource();
        var token = _statusCts.Token;

        try
        {
            await Task.Delay(3000, token);
            if (!token.IsCancellationRequested)
                StatusLabel.Text = "";
        }
        catch (TaskCanceledException)
        {
            // Ignore cancellation
        }
    }
}