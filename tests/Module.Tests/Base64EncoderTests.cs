using System;
using NUnit.Framework;
using Lab.Interfaces;
using Lab.Implementations.GenCode1; // Указываем, что тестируем код Qwen

namespace Tests;

[TestFixture]
public class Base64EncoderTests
{
    private IBase64Encoder _encoder = new Base64Encoder();

    [Test] public void Encode_ValidInput_ReturnsCorrectBase64() => Assert.That(_encoder.Encode("Hello"), Is.EqualTo("SGVsbG8="));
    [Test] public void Encode_EmptyString_ReturnsEmptyString() => Assert.That(_encoder.Encode(""), Is.EqualTo(""));
    [Test] public void Encode_NullInput_ThrowsArgumentNullException() => Assert.Throws<ArgumentNullException>(() => _encoder.Encode(null!));
    [Test] public void Decode_ValidBase64_ReturnsOriginalString() => Assert.That(_encoder.Decode("SGVsbG8="), Is.EqualTo("Hello"));
    [Test] public void Encode_SingleChar_ReturnsCorrectPadding() => Assert.That(_encoder.Encode("A"), Is.EqualTo("QQ=="));
    [Test] public void Encode_TwoChars_ReturnsCorrectPadding() => Assert.That(_encoder.Encode("AB"), Is.EqualTo("QUI="));
    [Test] public void Encode_ThreeChars_NoPadding() => Assert.That(_encoder.Encode("ABC"), Is.EqualTo("QUJD"));
    [Test] public void Encode_UnicodeSymbols_RoundTrip()
    {
        string original = "Привет, мир!";
        string encoded = _encoder.Encode(original);
        string decoded = _encoder.Decode(encoded);
        Assert.That(decoded, Is.EqualTo(original));
    }
}
