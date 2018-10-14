This topic describes the outline of typical usage LibJpeg.Net for JPEG compression and decompression.

JPEG Compression
----------------
The rough outline of a JPEG compression operation is:

1. Allocate and initialize a JPEG compression object
2. Specify the destination for the compressed data (e.g., a file)
3. Set parameters for compression, including image size and colorspace
4. <xref:BitMiracle.LibJpeg.Classic.jpeg_compress_struct.jpeg_start_compress(System.Boolean)>
5. while (scan lines remain to be written)
6. <xref:BitMiracle.LibJpeg.Classic.jpeg_compress_struct.jpeg_write_scanlines(System.Byte[][],System.Int32)>
7. <xref:BitMiracle.LibJpeg.Classic.jpeg_compress_struct.jpeg_finish_compress>

A JPEG compression object holds parameters and working state for the JPEG library. We make creation/destruction of the object separate from starting or finishing compression of an image; the same object can be re-used for a series of image compression operations. This makes it easy to re-use the same parameter settings for a sequence of images. Re-use of a JPEG object also has important implications for processing abbreviated JPEG datastreams, as discussed later.

The image data to be compressed is supplied to <xref:BitMiracle.LibJpeg.Classic.jpeg_compress_struct.jpeg_write_scanlines(System.Byte[][],System.Int32)> from in-memory buffers. If the application is doing file-to-file compression, reading image data from the source file is the application's responsibility.

The library emits compressed data by calling a "data destination manager", which typically will write the data into a file; but the application can provide its own destination manager to do something else.

For further information see the [Compression details](~/articles/KB/compression-details.html).

JPEG Decompression
------------------
Similarly, the rough outline of a JPEG decompression operation is:

1. Allocate and initialize a JPEG decompression object
2. Specify the source of the compressed data (e.g., a file)
3. Call <xref:BitMiracle.LibJpeg.Classic.jpeg_decompress_struct.jpeg_read_header(System.Boolean)> to obtain image info
4. Set parameters for decompression
5. <xref:BitMiracle.LibJpeg.Classic.jpeg_decompress_struct.jpeg_start_decompress>
6. while (scan lines remain to be read)
7. <xref:BitMiracle.LibJpeg.Classic.jpeg_decompress_struct.jpeg_read_scanlines(System.Byte[][],System.Int32)>
8. <xref:BitMiracle.LibJpeg.Classic.jpeg_decompress_struct.jpeg_finish_decompress>

This is comparable to the compression outline except that reading the datastream header is a separate step. This is helpful because information about the image's size, colorspace, etc is available when the application selects decompression parameters. For example, the application can choose an output scaling ratio that will fit the image into the available screen size.

The decompression library obtains compressed data by calling a data source manager, which typically will read the data from a file; but other behaviors can be obtained with a custom source manager. Decompressed data is delivered into in-memory buffers passed to <xref:BitMiracle.LibJpeg.Classic.jpeg_decompress_struct.jpeg_read_scanlines(System.Byte[][],System.Int32)>.

It is possible to abort an incomplete compression or decompression operation by calling <xref:BitMiracle.LibJpeg.Classic.jpeg_common_struct.jpeg_abort> or, if you do not need to retain the JPEG object, simply release it by calling <xref:BitMiracle.LibJpeg.Classic.jpeg_common_struct.jpeg_destroy>.

JPEG compression and decompression objects are two separate struct types. However, they share some common fields, and certain routines such as <xref:BitMiracle.LibJpeg.Classic.jpeg_common_struct.jpeg_destroy> can work on either type of object.

The JPEG library has no static variables: all state is in the compression or decompression object. Therefore it is possible to process multiple compression and decompression operations concurrently, using multiple JPEG objects.

For further information see the [Decompression details](~/articles/KB/decompression-details.html).
