using System;
using System.Collections.Generic;

namespace cSudokuCommonLib
{
    /// <summary>
    /// Класс, представляющий информацию об одной ячейке игры CompetitiveSudoku
    /// </summary>
    [Serializable]
    public class CSudokuCell
    {
        /// <summary>
        /// Метка ячейки о том, что она входит в условие игры ("принадлежит серверу")
        /// </summary>
        public const int ServerId = 0;

        /// <summary>
        /// Значение ячейки. Если она пуста, то 0, иначе - число от 1 до 9
        /// </summary>
        public byte Value;

        /// <summary>
        /// Столбец игрового, в котором находится эта ячейка
        /// </summary>
        public int X;

        /// <summary>
        /// Строка игрового поля, в который находится эта ячейка
        /// </summary>
        public int Y;

        /// <summary>
        /// Поле, определяющее принадлежность этой ячейки. Если null, то ячейка свободна
        /// </summary>
        public int? Owner;

        /// <summary>
        /// Признак того, что эта ячейка ошибочна, то есть имеет то же значение, что и другая ячейка в одной с ней строке, столбце или регионе 3х3
        /// </summary>
        public bool Error;

        /// <summary>
        /// Список индексов ячеек, которые имеют то же значение, и находятся в тех же строке/столбце/регионе, то есть составляют с этой ячейкой взаимную ошибку
        /// </summary>
        [NonSerialized]
        private List<int> errorCellList = new List<int>();

        /// <summary>
        /// Уникальный для игрового поля индекс этой ячейки.
        /// </summary>
        private int Index => X * 9 + Y;

        /// <summary>
        /// Признак того, что эта ячейка является частью условия игры
        /// </summary>
        public bool IsTask => Value != 0 && Owner == ServerId;

        /// <summary>
        /// Пустая ячейка. Нужна для заполнения игрового поля у того игрока, который покинул чемпионат.
        /// </summary>
        /// <param name="x">Координата X пустой ячейки</param>
        /// <param name="y">Координата Y пустой ячейки</param>
        /// <returns>Ячейка с пустым значением</returns>
        public static CSudokuCell NULL(int x, int y) => new CSudokuCell
        {
            Owner = ServerId,
            Value = 0,
            X = x,
            Y = y,
        };

        /// <summary>
        /// Текстовое представление значения ячейки. Удобно для отладки, в игре не используется.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"{Value} {X}:{Y} I={Index} {(Error ? "E" : "")}";
        }

        /// <summary>
        /// Проверка этой ячейки на наличие ошибки по отношению к другой ячейке
        /// </summary>
        /// <param name="testingCell">Та ячейка, ошибочность по отношению к которой нужно установить</param>
        public void TestError(CSudokuCell testingCell)
        {
            if (Value == 0)
            {
                // Пустая ячейка не может быть ошибочной
                return;
            }

            if (0 == testingCell.Value)
            {
                // В проверяемой ячейке стёрто значение. Уберём её из списка ошибочных, если к текущей ячейке относилась ошибка, вызванная предыдущим значением в проверяемой ячейке
                SetError(testingCell, false);
            }
            else
            {
                // Ошибка - когда значения в ячейках одинаковые
                SetError(testingCell, Value == testingCell.Value);
            }

            // Проверка наличия ошибки для этой ячейки. Наличие ошибки может быть вызвано НЕ ТОЛЬКО той ячейкой, по отношению к которой проводиласть проверка в этом методе, но и какой-либо другой.
            CalculateErrorField();
        }

        /// <summary>
        /// Обработка механизма взаимной установки/снятия признака ошибки для двух ячеек
        /// </summary>
        /// <param name="testingCell">Взаимная ячейка для обработки из взаимного состояния</param>
        /// <param name="thisIsError">true, если эта ячейка и проверяемая взаимно ошибочны; false, если нет.</param>
        private void SetError(CSudokuCell testingCell, bool thisIsError)
        {
            // Определение индекса взаимной ячейки
            int testingIndex = testingCell.Index;

            if (thisIsError)
            {
                // Ячейки взаимно ошибочны

                if (!errorCellList.Contains(testingIndex))
                {
                    // Взаимное добавление ячеек в ошибочные списки
                    errorCellList.Add(testingIndex);
                    testingCell.errorCellList.Add(Index);
                }
            }
            else
            {
                // Ячейки взаимно НЕ ошибочны

                if (errorCellList.Contains(testingIndex))
                {
                    // Взаимное убирание из ошибочных списков
                    errorCellList.Remove(testingIndex);
                    testingCell.errorCellList.Remove(Index);
                }
            }

            // Проверка наличия ошибки во взаимной ячейке
            testingCell.CalculateErrorField();
        }

        /// <summary>
        /// Определние наличия ошибки в этой ячейке
        /// </summary>
        private void CalculateErrorField()
        {
            // Ячейка ошибочна, если список взаимно-ошибочных индексов ячеек не пуст
            Error = errorCellList.Count > 0;
        }
    }
}
