# caj2pdf.invoker
针对 https://github.com/caj2pdf/caj2pdf 的简易封装，以使C#能够较方便的使用，依赖python3，详情参见源项目

使用时需要安装 caj2pdf.invoker 包和 runtime 包（当前只做了windowsx64和ubuntux64的包）

项目运行依赖 `Python 3.3+`

- 原始项目地址： https://github.com/caj2pdf/caj2pdf
- jbig2dec地址： https://github.com/ArtifexSoftware/jbig2dec
- PyPDF2地址： https://github.com/py-pdf/PyPDF2
- mupdf 下载地址： https://www.mupdf.com/releases/index.html

### 如何使用

```C#
await Caj2PdfConverter.ConvertAsync("files/1.caj", "./out/1.pdf", default);
```