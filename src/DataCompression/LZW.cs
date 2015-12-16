using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace DataCompression
{
    
    public class LZW : ICompress
    {
        public const int MAX_NUMBER = 255;
        public const int CODE_ERROR = -1;
        public const int BITS_IN_BYTE = 8;
      //  public const int BIT_FOR_NUMBER = 10;
        public List<FrequencyTableItem> FrequencyTable { get; protected set; }
        public byte BitForNumber { get; protected set; }
        public LZW()
        {
            FrequencyTable = new List<FrequencyTableItem>();       
        }
        /// <summary>
        /// Сжатие данных
        /// </summary>
        /// <param name="message">Данные</param>
        /// <returns></returns>
        public byte[] Compress(byte[] message)
        {
            if (message == null)
                throw new NullReferenceException();
            CreateFrequencyTable(message);
            BitForNumber = GetNumberOfBitsForSymbol();
            return CompressMessageFromFrequencyTable();             
        }
        public byte[] Expand(byte[] messageToExpand)
        {
            if (messageToExpand == null)
                throw new NullReferenceException();
            BitForNumber = messageToExpand[0];
            byte[] messageWOSize = GetArrayPart(messageToExpand, messageToExpand.Length - 1, 1);           
            
            int[] messageInt = ByteArrayToIntArray(messageWOSize);
            RestoreFrequencyTable(messageInt);
            List<byte> byteList = ExpandMessageFromIntArray(messageInt);
            return ByteListToByteArray(byteList);
        }
        /// <summary>
        /// Заполнение таблицы для сжатия
        /// </summary>
        /// <param name="message">Сообщение</param>
        private void CreateFrequencyTable(byte[] message)
        {
            if (message == null)
                throw new NullReferenceException();
            int previousSymbolCode = message[0];
            int index = 1;
            int rowNumber = MAX_NUMBER + 1;
            while (index < message.Length + 1)
            {
                int currentSymbolCode = index >= message.Length ? -1 : message[index];
                FrequencyTableItem tableItem = new FrequencyTableItem(rowNumber, previousSymbolCode, currentSymbolCode);
                SearchIfItemExists(tableItem, message, ref index, ref currentSymbolCode);
                FrequencyTable.Add(tableItem);
                ++index;
                ++rowNumber;
                previousSymbolCode = currentSymbolCode;
            }       
        }
        /// <summary>
        /// Проверка на то, существует ли заданный элемент в матрице
        /// </summary>
        /// <param name="tableItem">Элемент, который необходимо найти</param>
        /// <param name="message">Сообщение</param>
        /// <param name="index">Номер текущего символа сообщения</param>
        /// <param name="currentSymbolCode">Значение текущего символа сообщения</param>
        private void SearchIfItemExists(FrequencyTableItem tableItem, byte[] message, ref int index, ref int currentSymbolCode)
        {
            if (message == null)
                throw new NullReferenceException();
            int rowSearchNumber = 0;
            while (rowSearchNumber != CODE_ERROR)
            {
                rowSearchNumber = FindRowInFrequencyTable(tableItem);
                if (rowSearchNumber != CODE_ERROR)
                {
                    tableItem.Prefics = rowSearchNumber;
                    ++index;
                    if (index < message.Length)
                    {
                        currentSymbolCode = message[index];
                        tableItem.Suffics = currentSymbolCode;
                    }
                    else
                        tableItem.Suffics = CODE_ERROR;
                }
            }
        }
        /// <summary>
        /// Поиск номера строки матрицы по элменту 
        /// </summary>
        /// <param name="tableItem">Элемент матрицы</param>
        /// <returns>Номер строки матрицы, соответствующий заданному элементу</returns>
        private int FindRowInFrequencyTable(FrequencyTableItem tableItem)
        {
            if (tableItem == null)
                throw new NullReferenceException();
            int rowBumber = CODE_ERROR;
            bool notFound = true;
            for (int i = 0; i < FrequencyTable.Count && notFound; ++i)
            {
                if (FrequencyTable[i].Prefics == tableItem.Prefics && FrequencyTable[i].Suffics == tableItem.Suffics)
                {
                    notFound = false;
                    rowBumber = FrequencyTable[i].SymbolCode;
                }
            }
            return rowBumber;            
        } 
        /// <summary>
        /// Поиск элемента матрицы по номеру строки
        /// </summary>
        /// <param name="symbolCode">Номер строки</param>
        /// <returns>Элемент матрицы, стоящий в заданной строке</returns>
        private FrequencyTableItem GetTableItemBySymbolCode(int symbolCode)
        {
            bool notFound = true;
            FrequencyTableItem tableItem = null;
            for (int i = 0; i < FrequencyTable.Count && notFound; ++i)
            {
                if (FrequencyTable[i].SymbolCode == symbolCode)
                {
                    tableItem = new FrequencyTableItem(FrequencyTable[i].SymbolCode, FrequencyTable[i].Prefics, 
                                                        FrequencyTable[i].Suffics);                  
                    notFound = false;
                }
            }
            return tableItem;
        } 

        /// <summary>
        /// Записывает подряд в массив байт элементы типа int из расчета по 10 бит на каждый элемент
        /// </summary>
        /// <returns>Массив байт</returns>
        private byte[] CompressMessageFromFrequencyTable()
        {
            int bytesNumber = FrequencyTable.Count * BitForNumber / BITS_IN_BYTE + 2; // 1
            byte[] compressedMessage = new byte[bytesNumber];
            compressedMessage[0] = BitForNumber;

            int bitCounter = BITS_IN_BYTE - 1;
            for (int i = 0, currentByteNumber = 1; i < FrequencyTable.Count; i++)
            {
                int messageElement = FrequencyTable[i].Prefics;
                for (int j = BitForNumber - 1; j >= 0; --j, --bitCounter)
                {
                    uint currentBit = (uint)(messageElement >> j);
                    uint setBit = (uint)currentBit << bitCounter;
                    compressedMessage[currentByteNumber] |= (byte)setBit;
                    if (bitCounter == 0)
                    {
                        bitCounter = BITS_IN_BYTE;
                        ++currentByteNumber;
                    }
                }
            }
            return compressedMessage;
        }

        /// <summary>
        /// Переводит массив байтов в массив int из расчета по 10 бит на элемент типа int
        /// </summary>
        /// <param name="messageToExpand">Массив байтов</param>
        /// <returns>Массив типа int</returns>
        private int[] ByteArrayToIntArray(byte[] messageToExpand)
        {
            if (messageToExpand == null)
                throw new NullReferenceException();
            int messageSize = messageToExpand.Length * BITS_IN_BYTE / BitForNumber;
            int[] message = new int[messageSize];
            int bitCounter = BitForNumber - 1;

            int currentNumber = 0;
            for (int i = 0, currentByteNumber = 0; i < messageToExpand.Length; i++)
            {
                int messageElement = messageToExpand[i];

                for (int j = BITS_IN_BYTE - 1; j >= 0; --j, --bitCounter)
                {
                    int currentBit = (int)(messageElement >> j) & 1;
                    int setBit = (int)currentBit << bitCounter;
                    currentNumber |= (int)setBit;
                    if (bitCounter == 0)
                    {
                        bitCounter = BitForNumber;
                        message[currentByteNumber] = currentNumber;
                        currentNumber = 0;
                        ++currentByteNumber;
                    }
                }
            }
            return message;
        }

        /// <summary>
        /// Расширение данных
        /// </summary>
        /// <param name="message">Сообщение, которое необходимо расширить</param>
        /// <returns>Расширенные данные</returns>
        private List<byte> ExpandMessageFromIntArray(int[] message)
        {
            if (message == null)
                throw new NullReferenceException();
            List<byte> byteList = new List<byte>();
            for (int i = 0; i < message.Length; ++i)
            {
                int symbolCode = message[i];
                if (symbolCode <= MAX_NUMBER)
                    byteList.Add((byte)symbolCode);
                else
                {
                    FrequencyTableItem tableItem = null;
                    List<byte> tmpLst = new List<byte>();
                    while (symbolCode > MAX_NUMBER)
                    {
                        tableItem = GetTableItemBySymbolCode(symbolCode);
                        symbolCode = tableItem.Prefics;
                        tmpLst.Insert(0, (byte)tableItem.Suffics);
                    }
                    tmpLst.Insert(0, (byte)tableItem.Prefics);
                    byteList.AddRange(tmpLst);
                }

            }
            return byteList;
        }

        /// <summary>
        /// Перевод списка байтов в массив байтов
        /// </summary>
        /// <param name="byteList">Список байтов</param>
        /// <returns>Массив байтов</returns>
        private byte[] ByteListToByteArray(List<byte> byteList)
        {
            byte[] decompressedMessage = new byte[byteList.Count];
            for (int i = 0; i < byteList.Count; ++i)
                decompressedMessage[i] = byteList[i];
            return decompressedMessage;      
        }

        /// <summary>
        /// Получить количество бит, необходимых для кодирования числа
        /// </summary>
        /// <returns>Число бит, необходимых для кодирония символа</returns>
        private byte GetNumberOfBitsForSymbol()
        {
            int maxPrefics = FrequencyTable[0].Prefics;
            for (int i = 0; i < FrequencyTable.Count; ++i)
                if (FrequencyTable[i].Prefics > maxPrefics)
                    maxPrefics = FrequencyTable[i].Prefics;
            return (byte) (Math.Log(maxPrefics, 2) + 1);
        }

        /// <summary>
        /// Выделить из массива часть
        /// </summary>
        /// <param name="message">Массив</param>
        /// <param name="numberOfElements">Сколько элементов выделить</param>
        /// <param name="index">Начиная с какого индекса выделить</param>
        /// <returns>Часть массива</returns>
        private byte[] GetArrayPart(byte[] message, int numberOfElements, int index)
        {
            if (message == null)
                throw new NullReferenceException();
            byte[] partMessage = new byte[numberOfElements];
            for (int i = index, j = 0; j < partMessage.Length; ++i, ++j)
                partMessage[j] = message[i];
            return partMessage;
        }

        /// <summary>
        /// Выделить префикс из таблицы по номеру строки таблицы
        /// </summary>
        /// <param name="index">Номер строки</param>
        /// <returns>Префикс</returns>
        private int GetPrefics(int index)
        {
            while (index > MAX_NUMBER)
                index = FrequencyTable[index - MAX_NUMBER - 1].Prefics;
            return index;
        }

        /// <summary>
        /// Восстановить таблицу частот по входному сообщению
        /// </summary>
        /// <param name="message">Сообщение</param>
        private void RestoreFrequencyTable(int[] message)
        {
            if (message == null)
                throw new NullReferenceException();
            FrequencyTable.Clear();
            int rowNumber = MAX_NUMBER + 1;
            for (int i = 0; i < message.Length - 1; ++i, ++rowNumber)
            {
                int currentElement = message[i];
                int nextElement = message[i + 1];
                if (nextElement > FrequencyTable.Count + MAX_NUMBER)
                    nextElement = GetPrefics(currentElement);
                else
                    nextElement = GetPrefics(nextElement);
                FrequencyTableItem tableItem = new FrequencyTableItem(rowNumber, currentElement, nextElement);
                FrequencyTable.Add(tableItem);                
            }
            FrequencyTableItem lastTableItem = new FrequencyTableItem(rowNumber, message[message.Length - 1], -1);
            FrequencyTable.Add(lastTableItem);  
        }
    }
}
