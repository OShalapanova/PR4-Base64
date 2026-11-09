using System;
using System.Collections.Generic;
using System.Text;
using Lab.Interfaces;

namespace Lab.Implementations.GenCode2
{
    public class Base64Encoder : IBase64Encoder
    {
        private const string Base64Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";

        public string Encode(string input)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));
            if (input.Length == 0) return string.Empty;
            byte[] bytes = Encoding.UTF8.GetBytes(input);
            StringBuilder result = new StringBuilder((bytes.Length + 2) / 3 * 4);
            for (int i = 0; i < bytes.Length; i += 3)
            {
                int remaining = bytes.Length - i;
                int b0 = bytes[i];
                int b1 = remaining > 1 ? bytes[i + 1] : 0;
                int b2 = remaining > 2 ? bytes[i + 2] : 0;
                result.Append(Base64Alphabet[(b0 >> 2) & 0x3F]);
                result.Append(Base64Alphabet[((b0 & 0x03) << 4) | ((b1 >> 4) & 0x0F)]);
                if (remaining > 1) result.Append(Base64Alphabet[((b1 & 0x0F) << 2) | ((b2 >> 6) & 0x03)]);
                else result.Append("=");
                if (remaining > 2) result.Append(Base64Alphabet[b2 & 0x3F]);
                else result.Append("=");
            }
            return result.ToString();
        }

        public string Decode(string input)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));
            if (input.Length == 0) return string.Empty;
            if (input.Length % 4 != 0) throw new ArgumentException("Invalid Base64 string length.", nameof(input));
            int padding = 0;
            if (input[input.Length - 1] == '=') padding++;
            if (padding == 1 && input.Length >= 2 && input[input.Length - 2] == '=') padding++;
            for (int i = 0; i < input.Length - padding; i++)
            {
                if (input[i] == '=') throw new ArgumentException("Invalid Base64 string.", nameof(input));
            }
            List<byte> bytes = new List<byte>(input.Length / 4 * 3);
            for (int i = 0; i < input.Length; i += 4)
            {
                bool lastGroup = (i + 4 == input.Length);
                char ch2 = input[i + 2];
                char ch3 = input[i + 3];
                if (!lastGroup && (ch2 == '=' || ch3 == '=')) throw new ArgumentException("Invalid Base64 string.", nameof(input));
                if (ch2 == '=' && ch3 != '=') throw new ArgumentException("Invalid Base64 string.", nameof(input));
                int c0 = DecodeChar(input[i]);
                int c1 = DecodeChar(input[i + 1]);
                int c2 = ch2 == '=' ? 0 : DecodeChar(ch2);
                int c3 = ch3 == '=' ? 0 : DecodeChar(ch3);
                bytes.Add((byte)((c0 << 2) | (c1 >> 4)));
                if (ch2 != '=') bytes.Add((byte)(((c1 & 0x0F) << 4) | (c2 >> 2)));
                if (ch3 != '=') bytes.Add((byte)(((c2 & 0x03) << 6) | c3));
            }
            return Encoding.UTF8.GetString(bytes.ToArray());
        }

        private static int DecodeChar(char c)
        {
            if (c >= 'A' && c <= 'Z') return c - 'A';
            if (c >= 'a' && c <= 'z') return c - 'a' + 26;
            if (c >= '0' && c <= '9') return c - '0' + 52;
            if (c == '+') return 62;
            if (c == '/') return 63;
            throw new ArgumentException($"Invalid Base64 character: '{c}'.");
        }
    }
}
