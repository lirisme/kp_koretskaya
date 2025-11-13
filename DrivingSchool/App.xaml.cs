using System;
using System.Windows;

namespace DrivingSchool
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            // Глобальная обработка исключений
            AppDomain.CurrentDomain.UnhandledException += (s, args) =>
            {
                MessageBox.Show($"Критическая ошибка: {args.ExceptionObject}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            };

            DispatcherUnhandledException += (s, args) =>
            {
                MessageBox.Show($"Ошибка в интерфейсе: {args.Exception.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                args.Handled = true;
            };

            base.OnStartup(e);
        }
    }
}