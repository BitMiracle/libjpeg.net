Here we revisit the JPEG compression outline given in the [overview](~/articles/KB/typical-usage.html).

The steps of a JPEG compression operation:

1. Allocate and initialize a JPEG compression object
----------------------------------------------------

A JPEG compression object is an instance of <xref:BitMiracle.LibJpeg.Classic.jpeg_compress_struct>. You will also need a class representing a JPEG error handler. The part of this that the library cares about is an instance of <xref:BitMiracle.LibJpeg.Classic.jpeg_error_mgr>. 

If you are providing your own error handler, you'll typically need to inherit the <xref:BitMiracle.LibJpeg.Classic.jpeg_error_mgr> class; this is discussed later under [Error handling](~/articles/KB/error-handling.html). For now we'll assume you are just using the default error handler. The default error handler will print JPEG error/warning messages on console, and it will throw an exception if a fatal error occurs.

Typical code for this step, if you are using the default error handler, is

```cs
jpeg_error_mgr errorManager = new jpeg_error_mgr();
jpeg_compress_struct cinfo = new jpeg_compress_struct(errorManager);
```

2. Specify the destination for the compressed data (e.g., a file)
-----------------------------------------------------------------

As previously mentioned, the JPEG library delivers compressed data to a "data destination" module. The library includes one data destination module which knows how to write to a System.IO.Stream. You can use your own destination module if you want to do something else, as discussed later.

If you use the standard destination module, you must open the target System.IO.Stream beforehand. Typical code for this step looks like:

```cs
Stream output = ...; //initializing of stream for subsequent writing
cinfo.jpeg_stdio_dest(output);
```
where the last line invokes the standard destination module.

You can select the data destination after setting other parameters (step 3), if that's more convenient. You may not change the destination between calling <xref:BitMiracle.LibJpeg.Classic.jpeg_compress_struct.jpeg_start_compress(System.Boolean)> and <xref:BitMiracle.LibJpeg.Classic.jpeg_compress_struct.jpeg_finish_compress*>.

3. Set parameters for compression, including image size and colorspace
----------------------------------------------------------------------

You must supply information about the source image by setting the following properties in the JPEG object (<xref:BitMiracle.LibJpeg.Classic.jpeg_compress_struct>): 

|Property|Description|
|---|---|
|Image_width|Width of image, in pixels|
|Image_height|Height of image, in pixels|
|Input_components|Number of color channels (components per pixel)|
|In_color_space|Color space of source image|

The image dimensions are, hopefully, obvious. JPEG supports image dimensions of 1 to 64K pixels in either direction. The input color space is typically RGB or grayscale, and input_components is 3 or 1 accordingly. See [Special color spaces](~/articles/KB/special-color-spaces.html) for more info. The <xref:BitMiracle.LibJpeg.Classic.jpeg_compress_struct.In_color_space> property must be assigned one of the <xref:BitMiracle.LibJpeg.Classic.J_COLOR_SPACE> enum constants, typically JCS_RGB or JCS_GRAYSCALE. 

JPEG has a large number of compression parameters that determine how the image is encoded. Most applications don't need or want to know about all these parameters. You can set all the parameters to reasonable defaults by calling <xref:BitMiracle.LibJpeg.Classic.jpeg_compress_struct.jpeg_set_defaults> then, if there are particular values you want to change, you can do so after that. The [Compression parameter selection](~/articles/KB/compression-parameter-selection.html) tells about all the parameters.

You must set <xref:BitMiracle.LibJpeg.Classic.jpeg_compress_struct.In_color_space> correctly before calling <xref:BitMiracle.LibJpeg.Classic.jpeg_compress_struct.jpeg_set_defaults>, because the defaults depend on the source image colorspace. However the other three source image parameters need not be valid until you call <xref:BitMiracle.LibJpeg.Classic.jpeg_compress_struct.jpeg_start_compress(System.Boolean)>. There's no harm in calling <xref:BitMiracle.LibJpeg.Classic.jpeg_compress_struct.jpeg_set_defaults> more than once, if that happens to be convenient.

Typical code for a 24-bit RGB source image is

```cs
cinfo.Image_width = Width;
cinfo.Image_height = Height;
cinfo.Input_components = 3;
cinfo.In_color_space = J_COLOR_SPACE.JCS_RGB;

cinfo.jpeg_set_defaults();
//Make optional parameter settings here...
```

4. jpeg_start_compress
----------------------

The `true` parameter ensures that a complete JPEG interchange datastream will be written. This is appropriate in most cases. If you think you might want to use an abbreviated datastream, read the section on abbreviated datastreams.

Once you have called <xref:BitMiracle.LibJpeg.Classic.jpeg_compress_struct.jpeg_start_compress(System.Boolean)>, you may not alter any JPEG parameters or other fields of the JPEG object until you have completed the compression cycle.

5. while (scan lines remain to be written)
------------------------------------------

Now write all the required image data by calling <xref:BitMiracle.LibJpeg.Classic.jpeg_compress_struct.jpeg_write_scanlines(System.Byte[][],System.Int32)> one or more times. You can pass one or more scanlines in each call, up to the total image height. In most applications it is convenient to pass just one or a few scanlines at a time. The expected format for the passed data is discussed under [Data formats](~/articles/KB/data-formats.html) topic. 

Image data should be written in top-to-bottom scanline order. The JPEG spec contains some weasel wording about how top and bottom are application-defined terms (a curious interpretation of the English language...) but if you want your files to be compatible with everyone else's, you WILL use top-to-bottom order. If the source data must be read in bottom-to-top order, you can use the JPEG library's virtual array mechanism to invert the data efficiently. Examples of this can be found in the sample application **cJpeg**.

The library maintains a count of the number of scanlines written so far in the <xref:BitMiracle.LibJpeg.Classic.jpeg_compress_struct.Next_scanline> property of the JPEG object. Usually you can just use this variable as the loop counter, so that the loop test looks like `while (cinfo.Next_scanline < cinfo.Image_height)`

Code for this step depends heavily on the way that you store the source data. Here is the example for the case of a full-size 2-D source array containing 3-byte RGB pixels (`byte[][] image_buffer`): 

```cs
byte[][] rowData = new byte[1][]; // single row
int row_stride = cinfo.Image_width * 3; // physical row width in buffer

while (cinfo.Next_scanline < cinfo.Image_height)
{
    rowData[0] = image_buffer[cinfo.Next_scanline];
    cinfo.jpeg_write_scanlines(rowData, 1);
}
```

<xref:BitMiracle.LibJpeg.Classic.jpeg_compress_struct.jpeg_write_scanlines(System.Byte[][],System.Int32)> returns the number of scanlines actually written. This will normally be equal to the number passed in, so you can usually ignore the return value. It is different if you try to write more scanlines than the declared image height, the additional scanlines are ignored. 

6. jpeg_finish_compress
-----------------------

After all the image data has been written, call <xref:BitMiracle.LibJpeg.Classic.jpeg_compress_struct.jpeg_finish_compress> to complete the compression cycle. This step is **essential** to ensure that the last bufferload of data is written to the data destination.

<xref:BitMiracle.LibJpeg.Classic.jpeg_compress_struct.jpeg_finish_compress> also releases working memory associated with the JPEG object.

Typical code:
```cs
cinfo.jpeg_finish_compress();
```

If using the standard destination manager, don't forget to close the output stream (if necessary) afterwards.

If you have requested a multi-pass operating mode, such as Huffman code optimization, <xref:BitMiracle.LibJpeg.Classic.jpeg_compress_struct.jpeg_finish_compress> will perform the additional passes using data buffered by the first pass. In this case <xref:BitMiracle.LibJpeg.Classic.jpeg_compress_struct.jpeg_finish_compress> may take quite a while to complete. With the default compression parameters, this will not happen.

It is an error to call <xref:BitMiracle.LibJpeg.Classic.jpeg_compress_struct.jpeg_finish_compress> before writing the necessary total number of scanlines. If you wish to abort compression, call <xref:BitMiracle.LibJpeg.Classic.jpeg_common_struct.jpeg_abort> as discussed below.

After completing a compression cycle you may use it to compress another image. In that case return to step 2, 3, or 4 as appropriate. If you do not change the destination manager, the new datastream will be written to the same target. If you do not change any JPEG parameters, the new datastream will be written with the same parameters as before. Note that you can change the input image dimensions freely between cycles, but if you change the input colorspace, you should call <xref:BitMiracle.LibJpeg.Classic.jpeg_compress_struct.jpeg_set_defaults> to adjust for the new colorspace; and then you'll need to repeat all of step 3.

7. Aborting
-----------

If you decide to abort a compression cycle before finishing, you can call <xref:BitMiracle.LibJpeg.Classic.jpeg_compress_struct.jpeg_abort_compress>, or call <xref:BitMiracle.LibJpeg.Classic.jpeg_common_struct.jpeg_abort> which works on both compression and decompression objects. This will return the object to an idle state, releasing any working memory. <xref:BitMiracle.LibJpeg.Classic.jpeg_common_struct.jpeg_abort> is allowed at any time after successful object creation.

<xref:BitMiracle.LibJpeg.Classic.jpeg_common_struct.jpeg_abort> is the only safe calls to make on a JPEG object that has reported an error by calling error_exit.  See [Error handling](~/articles/KB/error-handling.html) for more info. The internal state of such an object is likely to be out of whack. Either of these two routines will return the object to a known state.
