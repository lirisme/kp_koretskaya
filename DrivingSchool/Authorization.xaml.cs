using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.TextFormatting;
using System.Xml;

namespace DrivingSchool
{
    public partial class Authorization : Window
    {
        private class UserData
        {
            public string Salt { get; set; }
            public string Hash { get; set; }
            public string Role { get; set; }
        }

        private Dictionary<string, UserData> users = new Dictionary<string, UserData>();
        private const string filePath = "users.json";
        private const int SaltSize = 16;
        private int failedLoginAttempts = 0;
        private string currentCaptchaText = "";

        public Authorization()
        {
            InitializeComponent();
            LoadUsers();
        }

        private void LoadUsers()
        {
            if (File.Exists(filePath))
            {
                try
                {
                    string json = File.ReadAllText(filePath);
                    users = JsonConvert.DeserializeObject<Dictionary<string, UserData>>(json) ?? new Dictionary<string, UserData>();
                }
                catch
                {
                    users = new Dictionary<string, UserData>();
                }
            }
        }

        private void SaveUsers()
        {
            string json = JsonConvert.SerializeObject(users, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText(filePath, json);
        }

        private string GenerateSalt()
        {
            byte[] salt = new byte[SaltSize];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }
            return Convert.ToBase64String(salt);
        }

        private string HashPassword(string password, string salt)
        {
            byte[] saltBytes = Convert.FromBase64String(salt);
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
            byte[] combined = new byte[saltBytes.Length + passwordBytes.Length];

            Buffer.BlockCopy(saltBytes, 0, combined, 0, saltBytes.Length);
            Buffer.BlockCopy(passwordBytes, 0, combined, saltBytes.Length, passwordBytes.Length);

            using (var sha256 = SHA256.Create())
            {
                return Convert.ToBase64String(sha256.ComputeHash(combined));
            }
        }

        private void Register_Click(object sender, RoutedEventArgs e)
        {
            string login = RegLoginBox.Text.Trim();
            string password = RegPasswordBox.Password;
            string role = ((ComboBoxItem)RegRoleComboBox.SelectedItem).Content.ToString();
            string adminPassword = AdminPasswordBox.Password.Trim();

            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(role))
            {
                MessageBox.Show("Заполните все поля!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (adminPassword != "56")
            {
                MessageBox.Show("Неверный пароль администратора. Регистрация запрещена.", "Доступ запрещён", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Введите логин и пароль");
                return;
            }

            if (users.ContainsKey(login))
            {
                MessageBox.Show("Пользователь с таким логином уже существует");
                return;
            }

            string salt = GenerateSalt();
            string hash = HashPassword(password, salt);

            users[login] = new UserData { Salt = salt, Hash = hash, Role = role };
            SaveUsers();

            MessageBox.Show("Регистрация прошла успешно");
            RegLoginBox.Text = "";
            RegPasswordBox.Password = "";
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            string login = LoginBox.Text.Trim();
            string password = PasswordBox.Password;
            string selectedRole = ((ComboBoxItem)RoleComboBox.SelectedItem).Content.ToString();

            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Введите логин и пароль");
                return;
            }

            if (failedLoginAttempts >= 3)
            {
                if (!CheckCaptcha())
                {
                    MessageBox.Show("Неверная капча");
                    GenerateCaptcha();
                    return;
                }
            }

            if (users.TryGetValue(login, out var userData))
            {
                string hashOfInput = HashPassword(password, userData.Salt);
                if (userData.Hash == hashOfInput && userData.Role == selectedRole)
                {
                    MessageBox.Show("Успешный вход");

                    failedLoginAttempts = 0;
                    CaptchaPanel.Visibility = Visibility.Collapsed;

                    MainWindow mainWindow = new MainWindow();
                    mainWindow.Show();
                    this.Close();
                    return;
                }
            }

            failedLoginAttempts++;
            MessageBox.Show("Неверный логин, пароль или роль");

            if (failedLoginAttempts >= 3)
            {
                CaptchaPanel.Visibility = Visibility.Visible;
                GenerateCaptcha();
            }
        }

        private void GenerateCaptcha()
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
            Random rnd = new Random();
            int width = 200, height = 70;
            DrawingVisual dv = new DrawingVisual();
            currentCaptchaText = "";

            using (DrawingContext dc = dv.RenderOpen())
            {
                dc.DrawRectangle(Brushes.Lavender, null, new Rect(0, 0, width, height));
                for (int i = 0; i < 150; i++)
                    dc.DrawEllipse(Brushes.Gray, null, new Point(rnd.Next(width), rnd.Next(height)), 1, 1);

                for (int i = 0; i < 6; i++)
                {
                    dc.DrawLine(new Pen(Brushes.SlateBlue, 1),
                        new Point(rnd.Next(width), rnd.Next(height)),
                        new Point(rnd.Next(width), rnd.Next(height)));
                }

                double x = 10;
                for (int i = 0; i < 5; i++)
                {
                    char ch = chars[rnd.Next(chars.Length)];
                    currentCaptchaText += ch;
                    double size = rnd.Next(28, 38);
                    var formattedText = new FormattedText(
                        ch.ToString(),
                        System.Globalization.CultureInfo.InvariantCulture,
                        FlowDirection.LeftToRight,
                        new Typeface("Arial Black"),
                        size,
                        Brushes.DarkSlateBlue,
                        1.0);

                    dc.PushTransform(new TranslateTransform(x, 15));
                    dc.PushTransform(new RotateTransform(rnd.Next(-20, 20), 0, 0));
                    dc.DrawText(formattedText, new Point(0, 0));
                    dc.Pop();
                    dc.Pop();

                    x += size;
                }
            }

            RenderTargetBitmap bmp = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
            bmp.Render(dv);
            CaptchaImage.Source = bmp;
            CaptchaInputBox.Text = "";
            CaptchaPlaceholder.Visibility = Visibility.Visible;
        }

        private bool CheckCaptcha()
        {
            return CaptchaInputBox.Text.Trim().Equals(currentCaptchaText, StringComparison.OrdinalIgnoreCase);
        }

        private void CaptchaInputBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            CaptchaPlaceholder.Visibility = string.IsNullOrWhiteSpace(CaptchaInputBox.Text) ? Visibility.Visible : Visibility.Collapsed;
        }

        private void CheckCaptcha_Click(object sender, RoutedEventArgs e)
        {
            if (CaptchaInputBox.Text == currentCaptchaText)
            {
                MessageBox.Show("Капча введена верно!");
            }
            else
            {
                MessageBox.Show("Неверная капча, попробуйте ещё раз.");
                GenerateCaptcha();
            }
        }

    }
}
