Decompression parameter selection is somewhat simpler than [compression parameter selection](~/articles/KB/compression-parameter-selection.html), since all of the JPEG internal parameters are recorded in the source file and need not be supplied by the application. Decompression parameters control the postprocessing done on the image to deliver it in a format suitable for the application's use. Many of the parameters control speed/quality tradeoffs, in which faster decompression may be obtained at the price of a poorer-quality image. The defaults select the highest quality (slowest) processing. 

The following properties in the JPEG object are set by <xref:BitMiracle.LibJpeg.Classic.jpeg_decompress_struct.jpeg_read_header(System.Boolean)> and may be useful to the application in choosing decompression parameters: 

|Property|Description|
|---|---|
|<xref:BitMiracle.LibJpeg.Classic.jpeg_decompress_struct.Image_width>, <xref:BitMiracle.LibJpeg.Classic.jpeg_decompress_struct.Image_height>|Width and height of image|
|<xref:BitMiracle.LibJpeg.Classic.jpeg_decompress_struct.Num_components>|Number of color components|
|<xref:BitMiracle.LibJpeg.Classic.jpeg_decompress_struct.Jpeg_color_space>|Colorspace of image|
|<xref:BitMiracle.LibJpeg.Classic.jpeg_decompress_struct.Density_unit>, <xref:BitMiracle.LibJpeg.Classic.jpeg_decompress_struct.X_density>, <xref:BitMiracle.LibJpeg.Classic.jpeg_decompress_struct.Y_density>|Resolution data from JFIF marker|

The JPEG color space, unfortunately, is something of a guess since the JPEG standard proper does not provide a way to record it. In practice most files adhere to the JFIF or Adobe conventions, and the decoder will recognize these correctly. See [Special color spaces](~/articles/KB/special-color-spaces.html) for more info. 

The decompression parameters that determine the basic properties of the returned image are:

|Parameter|Description|
|---|---|
|<xref:BitMiracle.LibJpeg.Classic.jpeg_decompress_struct.Out_color_space*>|Output color space|
|<xref:BitMiracle.LibJpeg.Classic.jpeg_decompress_struct.Scale_num>, <xref:BitMiracle.LibJpeg.Classic.jpeg_decompress_struct.Scale_denom>|Scale the image by the fraction `Scale_num/Scale_denom`|
|<xref:BitMiracle.LibJpeg.Classic.jpeg_decompress_struct.Quantize_colors>|Chooses colormapped or full-color output|

The next three parameters are relevant only if <xref:BitMiracle.LibJpeg.Classic.jpeg_decompress_struct.Quantize_colors> is `true`.

|Property|Description|
|---|---|
|<xref:BitMiracle.LibJpeg.Classic.jpeg_decompress_struct.Desired_number_of_colors>|Maximum number of colors to use in generating a library-supplied color map|
|<xref:BitMiracle.LibJpeg.Classic.jpeg_decompress_struct.Two_pass_quantize>|If `true`, an extra pass over the image is made to select a custom color map for the image|
|<xref:BitMiracle.LibJpeg.Classic.jpeg_decompress_struct.Dither_mode>|Selects color dithering method|

When <xref:BitMiracle.LibJpeg.Classic.jpeg_decompress_struct.Quantize_colors> is `true`, the target color map is described by the next two properties. <xref:BitMiracle.LibJpeg.Classic.jpeg_decompress_struct.Colormap> is set to `null` by <xref:BitMiracle.LibJpeg.Classic.jpeg_decompress_struct.jpeg_read_header(System.Boolean)>. The application can supply a color map by setting <xref:BitMiracle.LibJpeg.Classic.jpeg_decompress_struct.Colormap> non-null and setting <xref:BitMiracle.LibJpeg.Classic.jpeg_decompress_struct.Actual_number_of_colors> to the map size. Otherwise, <xref:BitMiracle.LibJpeg.Classic.jpeg_decompress_struct.jpeg_start_decompress> selects a suitable color map and sets these two properties itself.

|Property|Description|
|---|---|
|<xref:BitMiracle.LibJpeg.Classic.jpeg_decompress_struct.Colormap>|The color map, represented as a 2-D pixel array of <xref:BitMiracle.LibJpeg.Classic.jpeg_decompress_struct.Out_color_components> rows and <xref:BitMiracle.LibJpeg.Classic.jpeg_decompress_struct.Actual_number_of_colors> columns|
|<xref:BitMiracle.LibJpeg.Classic.jpeg_decompress_struct.Actual_number_of_colors>|The number of colors in the color map|

Additional decompression parameters that the application may set include:

|Parameter|Description|
|---|---|
|<xref:BitMiracle.LibJpeg.Classic.jpeg_decompress_struct.Dct_method>|Selects the algorithm used for the DCT step|
|<xref:BitMiracle.LibJpeg.Classic.jpeg_decompress_struct.Do_fancy_upsampling>|Upsampling of chroma components|
|<xref:BitMiracle.LibJpeg.Classic.jpeg_decompress_struct.Do_block_smoothing>|Apply interblock smoothing in early stages of decoding progressive JPEG files|
|<xref:BitMiracle.LibJpeg.Classic.jpeg_decompress_struct.Enable_1pass_quant>, <xref:BitMiracle.LibJpeg.Classic.jpeg_decompress_struct.Enable_external_quant>, <xref:BitMiracle.LibJpeg.Classic.jpeg_decompress_struct.Enable_2pass_quant>|These are significant only in [buffered-image mode](~/articles/KB/buffered-image-mode.html)|

The output image dimensions are given by the following properties. These are computed from the source image dimensions and the decompression parameters by <xref:BitMiracle.LibJpeg.Classic.jpeg_decompress_struct.jpeg_start_decompress>. You can also call <xref:BitMiracle.LibJpeg.Classic.jpeg_decompress_struct.jpeg_calc_output_dimensions> to obtain the values that will result from the current parameter settings. This can be useful if you are trying to pick a scaling ratio that will get close to a desired target size. 

|Property|Description|
|---|---|
|<xref:BitMiracle.LibJpeg.Classic.jpeg_decompress_struct.Output_width>, <xref:BitMiracle.LibJpeg.Classic.jpeg_decompress_struct.Output_height>|Actual dimensions of output image|
|<xref:BitMiracle.LibJpeg.Classic.jpeg_decompress_struct.Out_color_components>|Number of color components in <xref:BitMiracle.LibJpeg.Classic.jpeg_decompress_struct.Out_color_space*>|
|<xref:BitMiracle.LibJpeg.Classic.jpeg_decompress_struct.Output_components>|Number of color components returned|
|<xref:BitMiracle.LibJpeg.Classic.jpeg_decompress_struct.Rec_outbuf_height>|Recommended height of scanline buffer|

The output arrays are required to be <xref:BitMiracle.LibJpeg.Classic.jpeg_decompress_struct.Output_width> * <xref:BitMiracle.LibJpeg.Classic.jpeg_decompress_struct.Output_components> bytes wide.
