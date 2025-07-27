namespace 簡易的行控中心
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            SetTheme(Preferences.Get("DarkTheme", true));
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
    }
}