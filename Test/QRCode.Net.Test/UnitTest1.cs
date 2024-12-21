using Newtonsoft.Json;
using QRCode.Net.Codec;
using QRCode.Net.Png;

namespace QRCode.Net.Test;

[TestClass]
public class UnitTest1
{
    [TestMethod]
    public void Draw()
    {
        var random = new Random();
        // ʾ����ά�������� (5x5)
        bool[,] matrix = new bool[100, 100];

        for (int i = 0; i < matrix.GetLength(0); i++)
        {
            for (int j = 0; j < matrix.GetLength(1); j++)
            {
                matrix[i, j] = random.NextDouble() > 0.5;
            }
        }

        var str = "[[true,true,true,true,true,true,true,false,true,false,true,true,true,true,false,true,false,false,true,true,true,true,true,true,true],[true,false,false,false,false,false,true,false,false,false,true,true,true,false,true,false,true,false,true,false,false,false,false,false,true],[true,false,true,true,true,false,true,false,true,true,false,false,false,false,false,true,false,false,true,false,true,true,true,false,true],[true,false,true,true,true,false,true,false,false,true,true,true,true,true,false,true,false,false,true,false,true,true,true,false,true],[true,false,true,true,true,false,true,false,true,true,false,false,false,false,true,true,false,false,true,false,true,true,true,false,true],[true,false,false,false,false,false,true,false,false,false,false,true,true,false,false,false,true,false,true,false,false,false,false,false,true],[true,true,true,true,true,true,true,false,true,false,true,false,true,false,true,false,true,false,true,true,true,true,true,true,true],[false,false,false,false,false,false,false,false,false,true,false,false,true,false,true,true,true,false,false,false,false,false,false,false,false],[false,true,false,false,true,false,true,false,false,true,true,false,false,false,false,true,false,true,false,false,true,false,true,false,true],[true,true,true,false,false,false,false,true,false,true,true,true,false,false,false,true,false,true,true,true,true,true,true,false,true],[true,false,false,false,false,false,true,true,false,true,false,true,false,true,false,false,true,true,true,false,true,false,true,true,false],[true,true,true,true,true,false,false,true,true,false,true,true,true,true,false,true,false,true,true,false,true,true,false,true,false],[false,true,true,false,false,true,true,false,true,false,true,false,false,false,false,false,true,true,true,true,false,false,false,false,true],[false,true,false,true,true,false,false,false,false,true,false,true,false,false,true,false,false,true,false,true,false,false,true,false,false],[true,false,false,false,false,true,true,false,false,false,true,true,true,true,false,false,false,false,true,true,true,true,false,false,false],[false,false,true,false,false,true,false,false,false,false,false,false,false,false,false,false,true,true,true,false,false,true,true,true,false],[false,false,false,false,true,true,true,true,true,true,true,false,false,true,false,true,true,true,true,true,true,true,false,true,true],[false,false,false,false,false,false,false,false,false,false,false,false,true,true,false,true,true,false,false,false,true,false,true,false,true],[true,true,true,true,true,true,true,false,false,true,false,false,true,true,false,false,true,false,true,false,true,false,false,false,true],[true,false,false,false,false,false,true,false,false,false,true,true,true,false,true,false,true,false,false,false,true,true,false,false,true],[true,false,true,true,true,false,true,false,true,false,false,false,false,false,false,true,true,true,true,true,true,true,true,true,false],[true,false,true,true,true,false,true,false,false,false,false,false,true,true,false,false,true,true,true,true,true,true,false,true,false],[true,false,true,true,true,false,true,false,false,false,true,false,false,false,true,false,false,false,false,false,false,true,false,false,false],[true,false,false,false,false,false,true,false,true,false,true,true,true,false,true,true,false,true,true,true,false,false,false,true,true],[true,true,true,true,true,true,true,false,false,true,true,false,true,true,true,false,false,true,true,false,true,false,true,false,true]]";
        matrix = JsonConvert.DeserializeObject<bool[,]>(str);

        var generator = new PngQrCodeGenerator();
        using var ms = generator.GeneratePng(matrix, scale: 1); // ��� PNG �ļ�
        using var fs = new FileStream("test2.png", FileMode.Create);
        ms.CopyTo(fs);
    }

    [TestMethod]
    public void QRCodeEncode()
    {
        var qrCodeEncoder = new QRCodeEncoder();

        //���ñ���ģʽ  
        qrCodeEncoder.QRCodeEncodeMode = QRCodeEncoder.ENCODE_MODE.BYTE;
        //���ñ��������  
        qrCodeEncoder.QRCodeScale = 4;
        //���ñ���汾  
        qrCodeEncoder.QRCodeVersion = 0;
        //���ñ���������  
        qrCodeEncoder.QRCodeErrorCorrect = QRCodeEncoder.ERROR_CORRECTION.M;

        using var ms = qrCodeEncoder.Encode("https://syhu.org");
        using var fs = new FileStream("test3.png", FileMode.Create);
        ms.CopyTo(fs);
    }
}