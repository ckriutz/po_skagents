using System.ComponentModel;
using Microsoft.SemanticKernel;

namespace SKPurchaseOrderClient.Plugins;

public class ImageProcessingPlugin
{
    [KernelFunction]
    [Description("Extracts the image bytes from a purchase order file for processing.")]
    public byte[] ExtractImageBytes(string filePath, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Console.WriteLine($"\nExtracting image bytes from file: {filePath}");
        using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        using var memoryStream = new MemoryStream();
        stream.CopyTo(memoryStream);
        return memoryStream.ToArray();
    }
}