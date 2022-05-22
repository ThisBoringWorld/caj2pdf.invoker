# 相关二进制文件构建流程

## 1. 编译 `libjbig2codec.so` `libjbigdec.so`

下载 `jbig2dec` 源码，然后在目录内执行：

```shell
cc -Wall -fPIC --shared -o libjbigdec.so jbigdec.cc JBigDecode.cc
cc -Wall `pkg-config --cflags jbig2dec` -fPIC -shared -o libjbig2codec.so decode_jbig2data_x.cc `pkg-config --libs jbig2dec`
```

## 3. 编译 `mupdf`

下载 `mupdf` 源码后，构建流程参照 https://www.mupdf.com/docs/building.html 
