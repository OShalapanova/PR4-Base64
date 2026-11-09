using System;
using System.Text;
using Lab.Interfaces;

namespace Lab.Implementations.GenCode3
{
    public class Base64Encoder : IBase64Encoder
    {
        private const string Base64Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";
        private static readonly int[] CharToIndexMap = new int[123];

        static Base64Encoder()
        {
            for (int i = 0; i < CharToIndexMap.Length; i++) CharToIndexMap[i] = -1;
            for (int i = 0; i < Base64Chars.Length; i++) CharToIndexMap[Base64Chars[i]] = i;
        }

        public string Encode(string input)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));
            if (input.Length == 0) return string.Empty;
            byte[] bytes = Encoding.UTF8.GetBytes(input);
            var result = new StringBuilder(((bytes.Length + 2) / 3) * 4);
            for (int i = 0; i < bytes.Length - 2; i += 3)
            {
                uint triple = ((uint)bytes[i] << 16) | ((uint)bytes[i + 1] << 8) | bytes[i + 2];
                result.Append(Base64Chars[(int)((triple >> 18) & 0x3F)]);
                result.Append(Base64Chars[(int)((triple >> 12) & 0x3F)]);
                result.Append(Base64Chars[(int)((triple >> 6) & 0x3F)]);
                result.Append(Base64Chars[(int)(triple & 0x3F)]);
            }
            int remaining = bytes.Length % 3;
            if (remaining > 0)
            {
                int startIdx = bytes.Length - remaining;
                uint lastTriple = 0;
                for (int i = 0; i < remaining; i++) lastTriple |= (uint)bytes[startIdx + i] << (16 - (i * 8));
                result.Append(Base64Chars[(int)((lastTriple >> 18) & 0x3F)]);
                result.Append(Base64Chars[(int)((lastTriple >> 12) & 0x3F)]);
                if (remaining == 2)
                {
                    result.Append(Base64Chars[(int)((lastTriple >> 6) & 0x3F)]);
                    result.Append("=");
                }
                else result.Append("==");
            }
            return result.ToString();
        }

        public string Decode(string input)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));
            if (input.Length == 0) return string.Empty;
            if (input.Length % 4 != 0) throw new ArgumentException("Некорректная длина Base64-строки.", nameof(input));
            int paddingCount = 0;
            if (input.EndsWith("==")) paddingCount = 2;
            else if (input.EndsWith("=")) paddingCount = 1;
            if (input.Contains("="))
            {
                for (int i = 0; i < input.Length - paddingCount; i++)
                    if (input[i] == '=') throw new ArgumentException("Символ '=' находится не в конце строки.", nameof(input));
            }
            int outputLength = (input.Length / 4) * 3 - paddingCount;
            var buffer = new byte[outputLength];
            int writePos = 0;
            try
            {
                for (int i = 0; i < input.Length; i += 4)
                {
                    int sextetA = GetCharIndex(input[i]);
                    int sextetB = GetCharIndex(input[i + 1]);
                    int sextetC = (i + 2 < input.Length && input[i + 2] != '=') ? GetCharIndex(input[i + 2]) : 0;
                    int sextetD = (i + 3 < input.Length && input[i + 3] != '=') ? GetCharIndex(input[i + 3]) : 0;
                    uint combined = ((uint)sextetA << 18) | ((uint)sextetB << 12) | ((uint)sextetC << 6) | (uint)sextetD;
                    if (writePos < buffer.Length) buffer[writePos++] = (byte)((combined >> 16) & 0xFF);
                    if (writePos < buffer.Length) buffer[writePos++] = (byte)((combined >> 8) & 0xFF);
                    if (writePos < buffer.Length) buffer[writePos++] = (byte)(combined & 0xFF);
                }
            }
            catch (ArgumentOutOfRangeException) { throw new ArgumentException("Некорректная Base64-строка.", nameof(input)); }
            return Encoding.UTF8.GetString(buffer);
        }
        private int GetCharIndex(char c)
        {
            if (c > 122 || CharToIndexMap[c] == -1) throw new ArgumentException($"Недопустимый символ Base64: '{c}'.", nameof(c));
            return CharToIndexMap[c];
        }
    }
}
