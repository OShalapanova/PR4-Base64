using System;
using System.Text;
using Lab.Interfaces;

namespace Lab.Implementations.GenCode1
{
    public class Base64Encoder : IBase64Encoder
    {
        private const string Base64Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";
        private static readonly int[] DecodeTable = BuildDecodeTable();

        private static int[] BuildDecodeTable()
        {
            int[] table = new int[128];
            for (int i = 0; i < table.Length; i++) table[i] = -1;
            for (int i = 0; i < 64; i++) table[Base64Chars[i]] = i;
            return table;
        }

        public string Encode(string input)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));
            if (input.Length == 0) return string.Empty;
            byte[] bytes = Encoding.UTF8.GetBytes(input);
            StringBuilder sb = new StringBuilder((bytes.Length + 2) / 3 * 4);
            int i = 0;
            while (i < bytes.Length)
            {
                byte b1 = bytes[i];
                byte b2 = (i + 1 < bytes.Length) ? bytes[i + 1] : (byte)0;
                byte b3 = (i + 2 < bytes.Length) ? bytes[i + 2] : (byte)0;
                int idx1 = b1 >> 2;
                int idx2 = ((b1 & 0x03) << 4) | (b2 >> 4);
                int idx3 = ((b2 & 0x0F) << 2) | (b3 >> 6);
                int idx4 = b3 & 0x3F;
                sb.Append(Base64Chars[idx1]);
                sb.Append(Base64Chars[idx2]);
                if (i + 1 < bytes.Length) sb.Append(Base64Chars[idx3]); else sb.Append("=");
                if (i + 2 < bytes.Length) sb.Append(Base64Chars[idx4]); else sb.Append("=");
                i += 3;
            }
            return sb.ToString();
        }

        public string Decode(string input)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));
            if (input.Length == 0) return string.Empty;
            if (input.Length % 4 != 0) throw new ArgumentException("Некорректная длина Base64 строки.");
            int padCount = 0;
            if (input[input.Length - 1] == '=') padCount++;
            if (input.Length > 1 && input[input.Length - 2] == '=') padCount++;
            for (int i = 0; i < input.Length - padCount; i++)
            {
                char c = input[i];
                if (c >= 128 || DecodeTable[c] == -1) throw new ArgumentException("Некорректные символы.");
            }
            byte[] bytes = new byte[input.Length / 4 * 3 - padCount];
            int byteIndex = 0;
            for (int i = 0; i < input.Length; i += 4)
            {
                int c1 = DecodeTable[input[i]];
                int c2 = DecodeTable[input[i + 1]];
                int c3 = (input[i + 2] == '=') ? 0 : DecodeTable[input[i + 2]];
                int c4 = (input[i + 3] == '=') ? 0 : DecodeTable[input[i + 3]];
                int combined = (c1 << 18) | (c2 << 12) | (c3 << 6) | c4;
                if (byteIndex < bytes.Length) bytes[byteIndex++] = (byte)((combined >> 16) & 0xFF);
                if (byteIndex < bytes.Length) bytes[byteIndex++] = (byte)((combined >> 8) & 0xFF);
                if (byteIndex < bytes.Length) bytes[byteIndex++] = (byte)(combined & 0xFF);
            }
            return Encoding.UTF8.GetString(bytes);
        }
    }
}
