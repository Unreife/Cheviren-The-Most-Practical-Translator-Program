using System;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text.Json;
using System.Timers;
using System.Collections.Generic;
using System.Drawing;
using Microsoft.Win32;



namespace Cheviren
{
    public partial class Form1 : Form
    {
        private NotifyIcon trayIcon;
        private ContextMenuStrip trayMenu;
        private string userLang = "en"; // Varsayılan ana dil İngilizce
        private string lastClipboardText = "";
        private TranslationPopup popup;
        private System.Timers.Timer clipboardResetTimer;

        // ISO kodu ve dilin kendi dilindeki adı
        private readonly Dictionary<string, string> languages = new Dictionary<string, string>
        {
            {"tr", "Türkçe"},
            {"en", "English"},
            {"de", "Deutsch"},
            {"fr", "Français"},
            {"es", "Español"},
            {"it", "Italiano"},
            {"ru", "Русский"},
            {"zh", "中文"},
            {"ar", "العربية"},
            {"ja", "日本語"},
            {"ko", "한국어"},
            {"pt", "Português"},
            {"nl", "Nederlands"},
            {"sv", "Svenska"},
            {"pl", "Polski"},
            {"el", "Ελληνικά"}
        };

        private readonly Dictionary<string, Dictionary<string, string>> uiTexts = new()
        {
            ["en"] = new()
            {
                ["MainLanguage"] = "Main Language",
                ["Exit"] = "Exit",
                ["AppName"] = "Translator",
                ["TranslationFailed"] = "[Translation failed]",
            },
            ["tr"] = new()
            {
                ["MainLanguage"] = "Ana Dil",
                ["Exit"] = "Çıkış",
                ["AppName"] = "Çeviren",
                ["TranslationFailed"] = "[Çeviri başarısız]",
            },
            ["de"] = new()
            {
                ["MainLanguage"] = "Hauptsprache",
                ["Exit"] = "Beenden",
                ["AppName"] = "Übersetzer",
                ["TranslationFailed"] = "[Übersetzung fehlgeschlagen]",
            },
            ["fr"] = new()
            {
                ["MainLanguage"] = "Langue principale",
                ["Exit"] = "Quitter",
                ["AppName"] = "Traducteur",
                ["TranslationFailed"] = "[Échec de la traduction]",
            },
            ["es"] = new()
            {
                ["MainLanguage"] = "Idioma principal",
                ["Exit"] = "Salir",
                ["AppName"] = "Traductor",
                ["TranslationFailed"] = "[Traducción fallida]",
            },
            ["it"] = new()
            {
                ["MainLanguage"] = "Lingua principale",
                ["Exit"] = "Esci",
                ["AppName"] = "Traduttore",
                ["TranslationFailed"] = "[Traduzione fallita]",
            },
            ["ru"] = new()
            {
                ["MainLanguage"] = "Основной язык",
                ["Exit"] = "Выход",
                ["AppName"] = "Переводчик",
                ["TranslationFailed"] = "[Ошибка перевода]",
            },
            ["zh"] = new()
            {
                ["MainLanguage"] = "主要语言",
                ["Exit"] = "退出",
                ["AppName"] = "翻译器",
                ["TranslationFailed"] = "[翻译失败]",
            },
            ["ar"] = new()
            {
                ["MainLanguage"] = "اللغة الرئيسية",
                ["Exit"] = "خروج",
                ["AppName"] = "المترجم",
                ["TranslationFailed"] = "[فشل الترجمة]",
            },
            ["ja"] = new()
            {
                ["MainLanguage"] = "メイン言語",
                ["Exit"] = "終了",
                ["AppName"] = "翻訳者",
                ["TranslationFailed"] = "[翻訳に失敗しました]",
            },
            ["ko"] = new()
            {
                ["MainLanguage"] = "주요 언어",
                ["Exit"] = "종료",
                ["AppName"] = "번역기",
                ["TranslationFailed"] = "[번역 실패]",
            },
            ["pt"] = new()
            {
                ["MainLanguage"] = "Idioma principal",
                ["Exit"] = "Sair",
                ["AppName"] = "Tradutor",
                ["TranslationFailed"] = "[Falha na tradução]",
            },
            ["nl"] = new()
            {
                ["MainLanguage"] = "Hoofdtalen",
                ["Exit"] = "Afsluiten",
                ["AppName"] = "Vertaler",
                ["TranslationFailed"] = "[Vertaling mislukt]",
            },
            ["sv"] = new()
            {
                ["MainLanguage"] = "Huvudspråk",
                ["Exit"] = "Avsluta",
                ["AppName"] = "Översättare",
                ["TranslationFailed"] = "[Översättning misslyckades]",
            },
            ["pl"] = new()
            {
                ["MainLanguage"] = "Język główny",
                ["Exit"] = "Wyjście",
                ["AppName"] = "Tłumacz",
                ["TranslationFailed"] = "[Tłumaczenie nie powiodło się]",
            },
            ["el"] = new()
            {
                ["MainLanguage"] = "Κύρια γλώσσα",
                ["Exit"] = "Έξοδος",
                ["AppName"] = "Μεταφραστής",
                ["TranslationFailed"] = "[Αποτυχία μετάφρασης]",
            },
        };

        private const string RegistryPath = @"SOFTWARE\Çeviren";
        private const string RegistryLangKey = "UserLanguage";

        public Form1()
        {
            InitializeComponent();

            var savedLang = LoadUserLanguage();
            if (!string.IsNullOrEmpty(savedLang) && languages.ContainsKey(savedLang))
                userLang = savedLang;

            InitializeTray();
            trayIcon.MouseUp += TrayIcon_MouseUp; // Doğru yer
            this.WindowState = FormWindowState.Minimized;
            this.ShowInTaskbar = false;
            this.Visible = false;

            this.Icon = new Icon("translate.ico");

            ClipboardNotification.ClipboardUpdate += ClipboardChanged;
        }

        private void TrayIcon_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                trayMenu.Show(Cursor.Position);
            }
        }

        private void InitializeTray()
        {
            trayMenu = new ContextMenuStrip();
            trayIcon = new NotifyIcon
            {
                Icon = new Icon("translate.ico"),
                ContextMenuStrip = trayMenu,
                Visible = true
            };
            UpdateUiLanguage();
        }

        private void UpdateUiLanguage()
        {
            trayMenu.Items.Clear();

            var userLangMenu = new ToolStripMenuItem(uiTexts[userLang]["MainLanguage"]);
            foreach (var lang in languages)
            {
                var item = new ToolStripMenuItem(lang.Value)
                {
                    Checked = lang.Key == userLang
                };
                string langCode = lang.Key;
                item.Click += (s, e) =>
                {
                    userLang = langCode;
                    SaveUserLanguage(userLang); // <-- Burada kaydediyoruz
                    foreach (ToolStripMenuItem mi in userLangMenu.DropDownItems)
                        mi.Checked = false;
                    item.Checked = true;
                    UpdateUiLanguage();
                };
                userLangMenu.DropDownItems.Add(item);
            }
            trayMenu.Items.Add(userLangMenu);

            trayMenu.Items.Add(uiTexts[userLang]["Exit"], null, (s, e) => Application.Exit());

            trayIcon.Text = uiTexts[userLang]["AppName"];
        }

        private async void ClipboardChanged(object sender, EventArgs e)
        {
            if (Clipboard.ContainsText())
            {
                string text = Clipboard.GetText();
                if (text != lastClipboardText && !string.IsNullOrWhiteSpace(text))
                {
                    lastClipboardText = text;
                    // Önce dil algıla
                    string detectedLang = await DetectLanguage(text);
                    if (detectedLang == userLang)
                    {
                        // Ana dildeyse çeviri gösterme
                        return;
                    }
                    string translated = await TranslateWithAI(text, detectedLang, userLang);
                    if (!string.IsNullOrEmpty(translated))
                    {
                        ShowTranslationPopup(translated);

                        clipboardResetTimer?.Stop();
                        clipboardResetTimer = new System.Timers.Timer(1000);
                        clipboardResetTimer.Elapsed += (s, args) =>
                        {
                            lastClipboardText = "";
                            clipboardResetTimer.Stop();
                        };
                        clipboardResetTimer.Start();
                    }
                }
            }
        }

        // Google Translate ile dil algıla
        private async Task<string> DetectLanguage(string text)
        {
            try
            {
                string url = $"https://translate.googleapis.com/translate_a/single?client=gtx&sl=auto&tl={userLang}&dt=t&q={Uri.EscapeDataString(text)}";
                using var client = new HttpClient();
                var response = await client.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                    return userLang; // Hata olursa ana dili döndür

                var responseString = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(responseString);
                // Yanıt örneği: [[[ÇEVİRİ,"orijinal",null,null,1]],null,"en",null,null,null,null,[]]
                var detectedLang = doc.RootElement[2].GetString();
                return detectedLang ?? userLang;
            }
            catch
            {
                return userLang;
            }
        }

        // Google Translate ile çeviri
        private async Task<string> TranslateWithAI(string text, string from, string to)
        {
            try
            {
                string url = $"https://translate.googleapis.com/translate_a/single?client=gtx&sl={from}&tl={to}&dt=t&q={Uri.EscapeDataString(text)}";
                using var client = new HttpClient();
                var response = await client.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return $"{uiTexts[userLang]["TranslationFailed"]}\n{response.StatusCode}\n{errorContent}";
                }

                var responseString = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(responseString);
                var sentences = doc.RootElement[0];
                string result = "";
                foreach (var sentence in sentences.EnumerateArray())
                {
                    result += sentence[0].GetString();
                }
                return result;
            }
            catch (Exception ex)
            {
                return $"[Çeviri başarısız]\nAğ veya bağlantı hatası:\n{ex.Message}";
            }
        }

        private void ShowTranslationPopup(string translatedText)
        {
            popup?.Close();

            var mousePos = Cursor.Position;
            popup = new TranslationPopup(translatedText, 3000); // 3 saniye sonra otomatik kapanır
            popup.StartPosition = FormStartPosition.Manual;
            popup.Location = new Point(mousePos.X + 10, mousePos.Y + 10);
            popup.Show();

            popup.Deactivate += (s, e) =>
            {
                popup.Close();
                lastClipboardText = "";
            };
        }

        private void SaveUserLanguage(string lang)
        {
            try
            {
                using (var key = Registry.CurrentUser.CreateSubKey(RegistryPath))
                {
                    key.SetValue(RegistryLangKey, lang);
                }
            }
            catch
            {
                // Hata yönetimi: Gerekirse kullanıcıya bilgi verilebilir
            }
        }

        private string LoadUserLanguage()
        {
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(RegistryPath))
                {
                    return key?.GetValue(RegistryLangKey) as string;
                }
            }
            catch
            {
                return null;
            }
        }
    }

    // Clipboard değişikliklerini dinlemek için yardımcı sınıf
    public static class ClipboardNotification
    {
        public static event EventHandler ClipboardUpdate;

        private static NotificationForm _form = new NotificationForm();

        private class NotificationForm : Form
        {
            public NotificationForm()
            {
                NativeMethods.SetParent(Handle, NativeMethods.HWND_MESSAGE);
                NativeMethods.AddClipboardFormatListener(Handle);
            }

            protected override void WndProc(ref Message m)
            {
                if (m.Msg == NativeMethods.WM_CLIPBOARDUPDATE)
                    ClipboardUpdate?.Invoke(null, EventArgs.Empty);
                base.WndProc(ref m);
            }
        }

        private static class NativeMethods
        {
            public const int WM_CLIPBOARDUPDATE = 0x031D;
            public static IntPtr HWND_MESSAGE = new IntPtr(-3);

            [System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = true)]
            public static extern bool AddClipboardFormatListener(IntPtr hwnd);

            [System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = true)]
            public static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);
        }
    }
}
