namespace Lab.Interfaces;
public interface IBase64Encoder
{
    string Encode(string input);
    string Decode(string input);
}
