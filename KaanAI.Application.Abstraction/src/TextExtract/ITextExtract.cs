using KaanAI.Application.Abstraction.TextExtract.Contracts;

namespace KaanAI.Application.Abstraction;

public interface ITextExtract : IService
{
    public string Extract(string pdfPath);
    public string Normalize(string text);

}
