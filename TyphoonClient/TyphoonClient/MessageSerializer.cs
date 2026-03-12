using System.Text;
using System;
using Google.Protobuf;

namespace Waters.Control.Client
{
    /// <summary>
    /// Message serialize class
    /// </summary>
    public static class MessageSerializer
    {
        /// <summary>
        /// Empty message
        /// </summary>
        public static byte[] EmptyMessage { get { return new byte[] { }; } }

        /// <summary>
        /// Serialize the message
        /// </summary>
        /// <typeparam name="T">The message type</typeparam>
        /// <param name="t">The message</param>
        /// <returns></returns>
        public static byte[] Serialize<T>(T t)
        {
            byte[] data = null;

            if (typeof(T) == typeof(string))
            {
                // Annoying type dancing to convert a T as string
                string asString = (string)Convert.ChangeType(t, typeof(string));
                data = Encoding.GetEncoding("ISO-8859-1").GetBytes(asString);
            }
            else if (typeof(T) == typeof(byte[]))
            {
                data = (byte[])Convert.ChangeType(t, typeof(byte[]));
            }
            else
            {
                data = ((IMessage) t).ToByteArray();
            }
            return data;
        }

        /// <summary>
        /// De-serialize the message
        /// </summary>
        /// <typeparam name="T">The message type</typeparam>
        /// <param name="data">The data</param>
        /// <returns></returns>
        public static T Deserialize<T>(byte[] data) where T : new()
        {
            if (typeof(T) == typeof(string))
            {
                var asString = Encoding.GetEncoding("ISO-8859-1").GetString(data);
                // Annoying type dancing to return a string as T
                return (T)Convert.ChangeType(asString, typeof(T));
            }

            if (typeof(T) == typeof(byte[]))
            {
                return (T)Convert.ChangeType(data, typeof(byte[]));
            }

            T message = new T();
            ((IMessage)message).MergeFrom(data);
            return message;
        }

        /// <summary>
        /// Deserialize object from string representing binary data
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dataString"></param>
        /// <returns>An instance of the requsted type</returns>
        public static T Deserialize<T>(string dataString) where T : class, new()
        {
            if (typeof(T) == typeof(string))
            {
                // Annoying type dancing to return a string as T
                return (T)Convert.ChangeType(dataString, typeof(T));
            }
            return Deserialize<T>(Encoding.GetEncoding("ISO-8859-1").GetBytes(dataString));
        }
    }
}
