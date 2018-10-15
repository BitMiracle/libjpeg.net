Here we revisit the JPEG decompression outline given in the [overview](~/articles/KB/typical-usage.html).

The steps of a JPEG decompression operation:

1. Allocate and initialize a JPEG decompression object
------------------------------------------------------

This is just like initialization for compression, as discussed above, except that the object is an instance of <xref:BitMiracle.LibJpeg.Classic.jpeg_decompress_struct>. Error handling is exactly the same.

Typical code:

```cs
jpeg_error_mgr errorManager = new jpeg_error_mgr();
jpeg_decompress_struct cinfo = new jpeg_decompress_struct(errorManager);
```

Both here and in the IJG code, we usually use variable name `cinfo` for both compression and decompression objects.

2. Specify the source of the compressed data (e.g., a file)
-----------------------------------------------------------

As previously mentioned, the JPEG library reads compressed data from a "data source" module. The library includes one data source module which knows how to read from a stdio stream. You can use your own source module if you want to do something else, as discussed later. 

If you use the standard source module, you must open the source stdio stream beforehand. Typical code for this step looks like: 

```cs
Stream input = ...; //initializing of stream for subsequent reading
cinfo.jpeg_stdio_src(input);
```
where the last line invokes the standard source module.

You may not change the data source between calling <xref:BitMiracle.LibJpeg.Classic.jpeg_decompress_struct.jpeg_read_header(System.Boolean)> and <xref:BitMiracle.LibJpeg.Classic.jpeg_decompress_struct.jpeg_finish_decompress>. If you wish to read a series of JPEG images from a single source file, you should repeat the <xref:BitMiracle.LibJpeg.Classic.jpeg_decompress_struct.jpeg_read_header(System.Boolean)> to <xref:BitMiracle.LibJpeg.Classic.jpeg_decompress_struct.jpeg_finish_decompress> sequence without reinitializing either the JPEG object or the data source module; this prevents buffered input data from being discarded. 

3. Call jpeg_read_header to obtain image info
---------------------------------------------

Typical code for this step is just

```cs
cinfo.jpeg_read_header(true);
```

This will read the source datastream header markers, up to the beginning of the compressed data proper. On return, the image dimensions and other info have been stored in the JPEG object. The application may wish to consult this information before selecting decompression parameters. 

It is permissible to stop at this point if you just wanted to find out the image dimensions and other header info for a JPEG file. In that case, call <xref:BitMiracle.LibJpeg.Classic.jpeg_common_struct.jpeg_destroy> when you are done with the JPEG object, or call <xref:BitMiracle.LibJpeg.Classic.jpeg_common_struct.jpeg_abort> to return it to an idle state before selecting a new data source and reading another header. 

4. Set parameters for decompression
-----------------------------------

<xref:BitMiracle.LibJpeg.Classic.jpeg_decompress_struct.jpeg_read_header(System.Boolean)> sets appropriate default decompression parameters based on the properties of the image (in particular, its colorspace). However, you may well want to alter these defaults before beginning the decompression. For example, the default is to produce full color output from a color file. If you want colormapped output you must ask for it. Other options allow the returned image to be scaled and allow various speed/quality tradeoffs to be selected. [Decompression parameter selection](~/articles/KB/decompression-parameter-selection.html) gives details. 

If the defaults are appropriate, nothing need be done at this step.

Note that all default values are set by each call to <xref:BitMiracle.LibJpeg.Classic.jpeg_decompress_struct.jpeg_read_header(System.Boolean)>. If you reuse a decompression object, you cannot expect your parameter settings to be preserved across cycles, as you can for compression. You must set desired parameter values each time. 

5. jpeg_start_decompress
------------------------

Once the parameter values are satisfactory, call <xref:BitMiracle.LibJpeg.Classic.jpeg_decompress_struct.jpeg_start_decompress> to begin decompression. This will initialize internal state, allocate working memory, and prepare for returning data. 

Typical code is just

```cs
cinfo.jpeg_start_decompress();
```

If you have requested a multi-pass operating mode, such as 2-pass color quantization, <xref:BitMiracle.LibJpeg.Classic.jpeg_decompress_struct.jpeg_start_decompress> will do everything needed before data output can begin. In this case <xref:BitMiracle.LibJpeg.Classic.jpeg_decompress_struct.jpeg_start_decompress> may take quite a while to complete. With a single-scan (non progressive) JPEG file and default decompression parameters, this will not happen; <xref:BitMiracle.LibJpeg.Classic.jpeg_decompress_struct.jpeg_start_decompress> will return quickly.

After this call, the final output image dimensions, including any requested scaling, are available in the JPEG object; so is the selected colormap, if colormapped output has been requested. Useful fields include 

|Field|Description|
|---|---|
|Output_width|Image width, as scaled|
|Output_height|Image height, as scaled|
|Out_color_components|Number of color components in out_color_space|
|Output_components|Number of color components returned per pixel|
|Colormap|The selected colormap, if any|
|Actual_number_of_colors|Number of entries in colormap|

<xref:BitMiracle.LibJpeg.Classic.jpeg_decompress_struct.Output_components> is 1 (a colormap index) when quantizing colors; otherwise it equals <xref:BitMiracle.LibJpeg.Classic.jpeg_decompress_struct.Out_color_components>. It is the number of bytes that will be emitted per pixel in the output arrays. 

Typically you will need to allocate data buffers to hold the incoming image. You will need <xref:BitMiracle.LibJpeg.Classic.jpeg_decompress_struct.Output_width*> * <xref:BitMiracle.LibJpeg.Classic.jpeg_decompress_struct.Output_components> bytes per scanline in your output buffer, and a total of <xref:BitMiracle.LibJpeg.Classic.jpeg_decompress_struct.Output_height*> scanlines will be returned. 

**Note:** if you are using the JPEG library's internal memory manager to allocate data buffers (as dJpeg does), then the manager's protocol requires that you request large buffers before calling <xref:BitMiracle.LibJpeg.Classic.jpeg_decompress_struct.jpeg_start_decompress>. This is a little tricky since the Output_XXX fields are not normally valid then. You can make them valid by calling <xref:BitMiracle.LibJpeg.Classic.jpeg_decompress_struct.jpeg_calc_output_dimensions*> after setting the relevant parameters (scaling, output color space and quantization flag).

6. while (scan lines remain to be read)
---------------------------------------

Now you can read the decompressed image data by calling <xref:BitMiracle.LibJpeg.Classic.jpeg_decompress_struct.jpeg_read_scanlines(System.Byte[][],System.Int32)> one or more times. At each call, you pass in the maximum number of scanlines to be read (i.e., the height of your working buffer); <xref:BitMiracle.LibJpeg.Classic.jpeg_decompress_struct.jpeg_read_scanlines(System.Byte[][],System.Int32)> will return up to that many lines. The return value is the number of lines actually read. The format of the returned data is discussed under [Data formats](~/articles/KB/data-formats.html). Don't forget that grayscale and color JPEGs will return different data formats! 

Image data is returned in top-to-bottom scanline order. If you must write out the image in bottom-to-top order, you can use the JPEG library's virtual array mechanism to invert the data efficiently. Examples of this can be found in the sample application **dJpeg**.

The library maintains a count of the number of scanlines returned so far in the Output_scanline property of the JPEG object. Usually you can just use this variable as the loop counter, so that the loop test looks like `while (cinfo.Output_scanline < cinfo.Output_height)`. Note that the test should NOT be against <xref:BitMiracle.LibJpeg.Classic.jpeg_decompress_struct.Image_height>, unless you never use scaling. The <xref:BitMiracle.LibJpeg.Classic.jpeg_decompress_struct.Image_height> property is the height of the original unscaled image. The return value always equals the change in the value of <xref:BitMiracle.LibJpeg.Classic.jpeg_decompress_struct.Output_scanline>. 

If you don't use a suspending data source, it is safe to assume that <xref:BitMiracle.LibJpeg.Classic.jpeg_decompress_struct.jpeg_read_scanlines(System.Byte[][],System.Int32)> reads at least one scanline per call, until the bottom of the image has been reached. 

If you use a buffer larger than one scanline, it is NOT safe to assume that <xref:BitMiracle.LibJpeg.Classic.jpeg_decompress_struct.jpeg_read_scanlines(System.Byte[][],System.Int32)> fills it. (The current implementation returns only a few scanlines per call, no matter how large a buffer you pass.) So you must always provide a loop that calls <xref:BitMiracle.LibJpeg.Classic.jpeg_decompress_struct.jpeg_read_scanlines(System.Byte[][],System.Int32)> repeatedly until the whole image has been read. 

7. jpeg_finish_decompress
-------------------------

After all the image data has been read, call <xref:BitMiracle.LibJpeg.Classic.jpeg_decompress_struct.jpeg_finish_decompress> to complete the decompression cycle. This causes working memory associated with the JPEG object to be released. 

Typical code:

```cs
cinfo.jpeg_finish_decompress();
```

If using the standard source manager, don't forget to close the source stream if necessary. 

It is an error to call <xref:BitMiracle.LibJpeg.Classic.jpeg_decompress_struct.jpeg_finish_decompress> before reading the correct total number of scanlines. If you wish to abort decompression, call <xref:BitMiracle.LibJpeg.Classic.jpeg_common_struct.jpeg_abort> as discussed below. 

After completing a decompression cycle you may use it to decompress another image. In that case return to step 2 or 3 as appropriate. If you do not change the source manager, the next image will be read from the same source. 

8. Aborting
-----------

You can abort a decompression cycle by <xref:BitMiracle.LibJpeg.Classic.jpeg_decompress_struct.jpeg_abort_decompress> or <xref:BitMiracle.LibJpeg.Classic.jpeg_common_struct.jpeg_abort>. The previous discussion of aborting compression cycles applies here too. 
