﻿using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Serialization;

namespace XTransmit.Utility
{
    /**
     * Updated: 2019-09-24
     */
    public static class TextUtil
    {
        /**
         * Source: https://stackoverflow.com/questions/281640/how-do-i-get-a-human-readable-file-size-in-bytes-abbreviation-using-net
         * 
         * <summary>
         * Returns the human-readable file size for an arbitrary.
         * The default format is "0.### XB", e.g. "4.2 KB" or "1.434 GB"
         * </summary>
         */
        public static string GetBytesReadable(long i)
        {
            // Get absolute value
            var absolute_i = (i < 0 ? -i : i);

            // Determine the suffix and readable value
            string suffix;
            double readable;

            if (absolute_i >= 0x1000000000000000) // Exabyte
            {
                suffix = "EB";
                readable = (i >> 50);
            }
            else if (absolute_i >= 0x4000000000000) // Petabyte
            {
                suffix = "PB";
                readable = (i >> 40);
            }
            else if (absolute_i >= 0x10000000000) // Terabyte
            {
                suffix = "TB";
                readable = (i >> 30);
            }
            else if (absolute_i >= 0x40000000) // Gigabyte
            {
                suffix = "GB";
                readable = (i >> 20);
            }
            else if (absolute_i >= 0x100000) // Megabyte
            {
                suffix = "MB";
                readable = (i >> 10);
            }
            else if (absolute_i >= 0x400) // Kilobyte
            {
                suffix = "KB";
                readable = i;
            }
            else
            {
                return i.ToString("0 B"); // Byte
            }

            // Divide by 1024 to get fractional value
            readable = (readable / 1024);

            // Return formatted number with suffix
            return readable.ToString("0.## ") + suffix;
        }

        /**<summary>
         * Serialize the objectFrom to xml then deserialize the xml to objectTo and return it.
         * Time-consuming 21ms, 19ms, 20ms
         * </summary>
         */
        public static object CopyBySerializer(object objectFrom)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(objectFrom.GetType());
            MemoryStream memoryStream = new MemoryStream();

            xmlSerializer.Serialize(memoryStream, objectFrom);
            memoryStream.Position = 0;
            object objectTo = xmlSerializer.Deserialize(memoryStream);

            memoryStream.Close();
            memoryStream.Dispose();
            return objectTo;
        }

        public static byte[] GetXmlMD5(object objectUtf8)
        {
            byte[] md5Value = null;
            using (MemoryStream memObj = new MemoryStream())
            using (StreamWriter swMem = new StreamWriter(memObj, new UTF8Encoding(false)))
            using (MD5 md5 = MD5.Create())
            {
                new XmlSerializer(objectUtf8.GetType()).Serialize(swMem, objectUtf8);
                swMem.Flush(); 
                
                /**Any data written to a MemoryStream object is written into RAM, 
                 * MemoryStream.Flush() method is redundant.
                 */
                memObj.Position = 0;
                md5Value = md5.ComputeHash(memObj);
            }

            return md5Value;
        }
    }
}
