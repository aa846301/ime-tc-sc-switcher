using System;
using System.Globalization;
using System.Threading;
using ChineseInputSwitcher.Models;
using Avalonia.Threading;

namespace ChineseInputSwitcher.Services
{
    public class LocalizationService
    {
        private readonly AppSettings _settings;

        public LocalizationService(AppSettings settings)
        {
            _settings = settings;
        }

        public void InitializeLanguage()
        {
            try
            {
                // 顯式強制打印出當前的系統文化信息，有助於診斷
                var sysCulture = CultureInfo.CurrentUICulture;
                Console.WriteLine($"系統文化: {sysCulture.Name}, 父文化: {sysCulture.Parent.Name}");
                
                // 檢測是否處於不變模式
                bool isInvariantMode = false;
                try
                {
                    _ = new CultureInfo("en");
                }
                catch
                {
                    isInvariantMode = true;
                }
                
                if (isInvariantMode)
                {
                    Console.WriteLine("運行在不變文化模式，將使用默認資源");
                    return; // 不要設置任何文化，使用默認資源
                }
                
                // 正常處理語言設置
                string languageSetting = _settings.Language;
                Console.WriteLine($"配置的語言設置: {languageSetting}");
                
                // 確保選擇正確的語言
                CultureInfo culture;
                if (languageSetting == "system")
                {
                    string mappedLanguage = GetSystemLanguage();
                    Console.WriteLine($"映射的系統語言: {mappedLanguage}");
                    culture = new CultureInfo(mappedLanguage);
                }
                else
                {
                    try
                    {
                        culture = new CultureInfo(languageSetting);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"文化創建失敗: {ex.Message}");
                        culture = new CultureInfo("zh-Hant"); // 默認使用繁體中文
                    }
                }
                
                // 設置當前線程的文化信息
                Thread.CurrentThread.CurrentUICulture = culture;
                Thread.CurrentThread.CurrentCulture = culture;
                
                Console.WriteLine($"設置當前文化為: {culture.Name}");
                
                // 觸發語言變更事件
                App.NotifyLanguageChanged();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"初始化語言時發生錯誤: {ex}");
            }
        }
        
        private string GetSystemLanguage()
        {
            try
            {
                // 強制清除緩存並重新檢測
                CultureInfo.CurrentUICulture.ClearCachedData();
                
                // 獲取當前系統UI語言
                var currentCulture = CultureInfo.CurrentUICulture;
                Console.WriteLine($"重新檢測系統UI語言: {currentCulture.Name}, 父文化: {currentCulture.Parent.Name}");
                
                // 嘗試通過Windows API獲取更精確的語言設置 (如果在Windows上運行)
                if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
                {
                    try
                    {
                        var windowsLang = System.Globalization.CultureInfo.InstalledUICulture;
                        Console.WriteLine($"Windows API獲取的UI文化: {windowsLang.Name}");
                        currentCulture = windowsLang;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"獲取Windows語言失敗: {ex.Message}");
                    }
                }
                
                // 更詳細的語言映射邏輯
                if (currentCulture.Name.StartsWith("zh-TW") || 
                    currentCulture.Name.StartsWith("zh-HK") || 
                    currentCulture.Name.StartsWith("zh-MO") || 
                    currentCulture.Name.Equals("zh-Hant") ||
                    (currentCulture.Name.StartsWith("zh") && currentCulture.Name.Contains("Hant")))
                {
                    return "zh-Hant"; // 繁體中文
                }
                else if (currentCulture.Name.StartsWith("zh-CN") || 
                        currentCulture.Name.StartsWith("zh-SG") || 
                        currentCulture.Name.Equals("zh-Hans") ||
                        (currentCulture.Name.StartsWith("zh") && currentCulture.Name.Contains("Hans")))
                {
                    return "zh-Hans"; // 簡體中文
                }
                else if (currentCulture.Name.StartsWith("en"))
                {
                    return "en"; // 英文
                }
                else if (currentCulture.Name.StartsWith("ja"))
                {
                    return "ja"; // 日文
                }
                else if (currentCulture.Name.StartsWith("zh"))
                {
                    // 對於其他中文區域，嘗試判斷是繁體還是簡體
                    return "zh-Hant"; // 默認使用繁體中文
                }
                
                // 如果沒有匹配的語言，默認使用繁體中文
                return "zh-Hant";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"獲取系統語言時出錯: {ex.Message}");
                return "zh-Hant"; // 出錯時使用默認值
            }
        }

        // 添加動態切換語言的方法
        public void SwitchLanguage(string languageCode)
        {
            // 設置當前 UI 文化
            if (languageCode == "system")
            {
                // 獲取系統語言
                var systemLanguage = GetSystemLanguage();
                Thread.CurrentThread.CurrentUICulture = new CultureInfo(systemLanguage);
            }
            else
            {
                // 使用指定語言
                Thread.CurrentThread.CurrentUICulture = new CultureInfo(languageCode);
            }
            
            // 儲存設置
            _settings.Language = languageCode;
            _settings.Save();
            
            // 通知應用程序語言已變更
            App.NotifyLanguageChanged();
            
            Console.WriteLine($"語言已切換到: {Thread.CurrentThread.CurrentUICulture.Name}");
        }
    }
} 