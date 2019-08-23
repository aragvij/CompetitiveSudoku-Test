using cSudokuCommonLib;
using System;
using System.Diagnostics;
using System.Security.Principal;
using System.Windows.Forms;

namespace cSudokuServer
{
    static class Program
    {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main()
        {
            WindowsPrincipal pricipal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
            bool hasAdministrativeRight = pricipal.IsInRole(WindowsBuiltInRole.Administrator);

            // Проверка на наличие прав администратора у приложения при запуске
            if (hasAdministrativeRight == false)
            {
                ProcessStartInfo processInfo = new ProcessStartInfo
                {
                    // Для выполнения команды netsh, необходимой для привязывания прослушиваемых URL к этому приложению, ему необходимы права администратора
                    Verb = "runas",
                    FileName = Application.ExecutablePath
                };
                try
                {
                    // Запуск сервера
                    Process.Start(processInfo);
                }
                catch (Exception ex)
                {
                    // Пользователь, возможно, нажал кнопку "Нет" в ответ на вопрос о запуске программы с правами администратора
                    MessageBox.Show("Программа должна быть запущена с правами администратора. Ошибка: " + ex.AllMessages());
                }
                Application.Exit();
            }
            else
            {
                //имеем права администратора, значит, стартуем
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new FormCSudokuServer());
            }
        }
    }
}
