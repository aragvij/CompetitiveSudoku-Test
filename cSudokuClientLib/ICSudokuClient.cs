using System;
using cSudokuCommonLib;

namespace cSudokuClientLib
{
    /// <summary>
    /// Интерфейс передачи данных из ядра клиента на средство отображения
    /// </summary>
    public interface ICSudokuClient
    {
        /// <summary>
        /// Начало игры
        /// </summary>
        /// <param name="level">Уровень сложности</param>
        void StartNewGame(SudokuLevel level);

        /// <summary>
        /// Отображение состояния игры
        /// </summary>
        /// <param name="state">Информация о состоянии всех полей</param>
        void SetSudokuState(CSudokuState state);

        /// <summary>
        /// Отображение информации об исключительной ошибке
        /// </summary>
        /// <param name="exception">Исключение</param>
        void ShowError(Exception exception);

        /// <summary>
        /// Отображение текстового сообщения
        /// </summary>
        /// <param name="message">Текст сообщения</param>
        void ShowMessage(string message);

        /// <summary>
        /// Показ списка победителей
        /// </summary>
        /// <param name="winnerList">Список победителей</param>
        void ShowWinnerList(string[] winnerList);

        /// <summary>
        /// Окончание игры
        /// </summary>
        void EndOfGame();
    }
}