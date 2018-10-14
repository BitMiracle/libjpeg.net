Before diving into procedural details, it is helpful to understand the image data format that the JPEG library expects or returns.

The standard input image format is a rectangular array of pixels (or "samples"), with each pixel having the same number of "component" values (color channels). You must specify how many components there are and the colorspace interpretation of the components. Most applications will use RGB data (three components per pixel) or grayscale data (one component per pixel).

There is no provision for colormapped input. JPEG files are always full-color or full grayscale (or sometimes another colorspace such as CMYK). You can feed in a colormapped image by expanding it to full-color format. However JPEG often doesn't work very well with source data that has been colormapped, because of dithering noise.

Samples are stored by scanlines, with each scanline running from left to right. The component values for each pixel are adjacent in the row; for example, R,G,B,R,G,B,R,G,B,... for 24-bit RGB color. Each scanline is an array of bytes (unsigned chars)

The library accepts or supplies one or more complete scanlines per call. It is not possible to process part of a row at a time. Scanlines are always processed top-to-bottom. You can process an entire image in one call if you have it all in memory, but usually it's simplest to process one scanline at a time.

For best results, source data values should have the precision specified by <xref:BitMiracle.LibJpeg.Classic.JpegConstants.BITS_IN_JSAMPLE> (normally 8 bits). For instance, if you choose to compress data that's only 6 bits/channel, you should left-justify each value in a byte before passing it to the compressor. If you need to compress data that has more than 8 bits/channel, compile with <xref:BitMiracle.LibJpeg.Classic.JpegConstants.BITS_IN_JSAMPLE> = 12. 

The data format returned by the decompressor is the same in all details, except that colormapped output is supported. (Again, a JPEG file is never colormapped. But you can ask the decompressor to perform on-the-fly color quantization to deliver colormapped output.) If you request colormapped output then the returned data array contains a single byte per pixel; its value is an index into a color map. The color map is represented as a 2-D array of bytes in which each row holds the values of one color component, that is, colormap[i][j] is the value of the i'th color component for pixel value (map index) j. Note that since the colormap indexes are stored in bytes, the maximum number of colors is limited by the size of byte (at most 256 colors for an 8-bit JPEG library) 

