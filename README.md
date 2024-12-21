# 二维码生成工具

### 简介🏃‍♂️
net8 下**不依赖任何库**(也不依赖`System.Drawing`)的二维码生成库

> `ThoughtWorks.QRCode`是C#生成二维码上好的库，但是其绘图部分依赖了`System.Drawing`
> 但是，众所周知 net core 在linux下使用 `System.Drawing` 会有问题
> 于是乎重写了下绘图部分；其中算法依然采用`ThoughtWorks.QRCode`
> 原`ThoughtWorks.QRCode`作者haoersheng没有留联系方式，如有侵权请联系我删除！！！

### 用法🚀
话不多说，请看VCR
```csharp
var qrCodeEncoder = new QRCodeEncoder();

//设置编码模式  
qrCodeEncoder.QRCodeEncodeMode = QRCodeEncoder.ENCODE_MODE.BYTE;
//设置编码测量度  
qrCodeEncoder.QRCodeScale = 4;
//设置编码版本  
qrCodeEncoder.QRCodeVersion = 0;
//设置编码错误纠正  
qrCodeEncoder.QRCodeErrorCorrect = QRCodeEncoder.ERROR_CORRECTION.M;

using var ms = qrCodeEncoder.Encode("https://syhu.org");
using var fs = new FileStream("test3.png", FileMode.Create);
ms.CopyTo(fs);

```

### 效果⚓
![效果](OtherFiles/image/home_page.png)
