using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace cSudokuCommonLib
{
    public static class MyException
    {
        public static string AllMessages(this Exception ex)
        {
            if (null == ex)
                return null;

            StringBuilder stringBuilder = new StringBuilder();
            while (ex != null)
            {
                stringBuilder.Append(ex.Message);
                ex = ex.InnerException;
                if (ex != null)
                    stringBuilder.Append("; ");
            }
            return stringBuilder.ToString();
        }
    }

    public enum MessageType
    {
        None,
        Error,
        StartGame,
        PlayGame,
        WinnerList,
        SetValue,
        OutOfGame,
    }

    [Serializable]
    public class SocketMessage
    {
        public MessageType messageType;
        public string[] winnerList;
        public string text;
        public SudokuLevel level;
        public CSudokuState gameState;
        public CSudokuCell cell;
        public int playerId;

        private static readonly BinaryFormatter binaryFormatter = new BinaryFormatter();

        public byte[] Serialize()
        {
            using (MemoryStream stream = new MemoryStream())
            {
                binaryFormatter.Serialize(stream, this);
                return stream.ToArray();
            }
        }

        public static SocketMessage Deserialize(byte[] bytes)
        {
            using (MemoryStream stream = new MemoryStream(bytes))
            {
                try
                {
                    return (SocketMessage)binaryFormatter.Deserialize(stream);
                }
                catch (Exception ex)
                {
                    return new SocketMessage
                    {
                        messageType = MessageType.Error,
                        text = "SocketMessage.Deserialize: " + ex.AllMessages()
                    };
                }
            }
        }
    }
}
