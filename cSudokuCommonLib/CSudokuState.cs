using System;

namespace cSudokuCommonLib
{
    /// <summary>
    /// Уровень сложности игры. Я использовал кириллические идентификаторы, потому что их же вывожу на экран для выбора. Сделал так только потому что C# это позволяет, а список простой...
    /// </summary>
    [Serializable]
    public enum SudokuLevel : byte
    {
        Unknown = 0,
        простейший,
        простой,
        средний,
        сложный,
        сложнейший
    }

    /// <summary>
    /// Текущее состояние игры
    /// </summary>
    [Serializable]
    public class CSudokuState
    {
        /// <summary>
        /// Уровень сложности
        /// </summary>
        public SudokuLevel Level;

        /// <summary>
        /// Массив со значениями ячеек
        /// </summary>
        public CSudokuCell[] Values;

        /// <summary>
        /// Возвращает значение конкретной ячейки
        /// </summary>
        /// <param name="i">Координата X ячейки (номер столбца)</param>
        /// <param name="j">Координата Y ячейки (номер строки)</param>
        /// <returns>Текущее значение ячейки с координатами [i,j]</returns>
        public CSudokuCell GetValue(int i, int j)
        {
            if (i > 9 || j > 9)
            {
                return null;
            }
            return Values[j * 9 + i];
        }
    }
}
