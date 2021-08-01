using System.IO;
using System.Text;

namespace Cyberliberty.Socketter.Helpers
{
    /// <summary>
    /// 
    /// </summary>
    public static class Translator
    {

        /// <summary>
        /// Encrypt the string buffer.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static byte[] Encrypt(string message)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (StreamWriter sw = new StreamWriter(ms))
                {
                    byte[] buffer = Encoding.UTF8.GetBytes(message);
                    sw.BaseStream.Write(buffer, 0, buffer.Length);
                    return ms.ToArray();
                }
            }
        }

        /// <summary>
        /// Decrypt the byte buffer
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static string Decrypt(byte[] message)
        {
            using (StreamReader sr = new StreamReader(new MemoryStream(message), Encoding.Default))
            {
                return sr.ReadToEnd();
            }
        }

    }
}
