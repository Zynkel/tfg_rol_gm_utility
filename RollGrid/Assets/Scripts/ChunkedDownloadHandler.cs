using UnityEngine;
using UnityEngine.Networking;
using System.Text;

public class ChunkedDownloadHandler : DownloadHandlerScript
{
    private StringBuilder stringBuilder;

    public ChunkedDownloadHandler()
    {
        stringBuilder = new StringBuilder();
    }

    protected override bool ReceiveData(byte[] data, int dataLength)
    {
        if (data == null || dataLength == 0)
            return false;

        // Convertimos los bytes a texto y lo vamos acumulando
        string chunk = Encoding.UTF8.GetString(data, 0, dataLength);
        stringBuilder.Append(chunk);
        return true;
    }

    protected override void CompleteContent()
    {
        Debug.Log("Descarga completada. Longitud final: " + stringBuilder.Length);
    }

    public string GetText()
    {
        return stringBuilder.ToString();
    }
}
