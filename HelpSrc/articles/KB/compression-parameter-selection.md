This section describes all the optional parameters you can set for JPEG compression, as well as the "helper" methods provided to assist in this task. Proper setting of some parameters requires detailed understanding of the JPEG standard; if you don't know what a parameter is for, it's best not to mess with it!

It's a good idea to call <xref:BitMiracle.LibJpeg.Classic.jpeg_compress_struct.jpeg_set_defaults> first, even if you plan to set all the parameters; that way your code is more likely to work with future JPEG libraries that have additional parameters. For the same reason, we recommend you use a helper method where one is provided, in preference to twiddling <xref:BitMiracle.LibJpeg.Classic.jpeg_compress_struct> properties directly.

The helper methods are:

|Method|Description|
|---|---|
|<xref:BitMiracle.LibJpeg.Classic.jpeg_compress_struct.jpeg_set_defaults>|Sets all JPEG parameters to reasonable defaults, using only the input image's color space (In_color_space)|
|<xref:BitMiracle.LibJpeg.Classic.jpeg_compress_struct.jpeg_set_colorspace(BitMiracle.LibJpeg.Classic.J_COLOR_SPACE)>|Sets the JPEG file's colorspace as specified, and sets other colorspace-dependent parameters appropriately|
|<xref:BitMiracle.LibJpeg.Classic.jpeg_compress_struct.jpeg_default_colorspace>|Selects an appropriate JPEG colorspace based on In_color_space, and calls <xref:BitMiracle.LibJpeg.Classic.jpeg_compress_struct.jpeg_set_colorspace(BitMiracle.LibJpeg.Classic.J_COLOR_SPACE)>|
|<xref:BitMiracle.LibJpeg.Classic.jpeg_compress_struct.jpeg_set_quality(System.Int32,System.Boolean)>|Constructs JPEG quantization tables appropriate for the indicated quality setting|
|<xref:BitMiracle.LibJpeg.Classic.jpeg_compress_struct.jpeg_set_linear_quality(System.Int32,System.Boolean)>|Same as <xref:BitMiracle.LibJpeg.Classic.jpeg_compress_struct.jpeg_set_quality(System.Int32,System.Boolean)> except that the generated tables are the sample tables given in the JPEG spec section K.1, multiplied by the specified scale factor|
|<xref:BitMiracle.LibJpeg.Classic.jpeg_compress_struct.jpeg_quality_scaling(System.Int32)>|Converts a value on the IJG-recommended quality scale to a linear scaling percentage|
|<xref:BitMiracle.LibJpeg.Classic.jpeg_compress_struct.jpeg_add_quant_table(System.Int32,System.Int32[],System.Int32,System.Boolean)>|Allows an arbitrary quantization table to be created|
|<xref:BitMiracle.LibJpeg.Classic.jpeg_compress_struct.jpeg_simple_progression>|Generates a default scan script for writing a progressive-JPEG file|

Compression parameters (properties) include:

|Property|Description|
|---|---|
|<xref:BitMiracle.LibJpeg.Classic.jpeg_compress_struct.Dct_method>|Selects the algorithm used for the DCT step|
|<xref:BitMiracle.LibJpeg.Classic.jpeg_compress_struct.Jpeg_color_space>|The JPEG color space|
|<xref:BitMiracle.LibJpeg.Classic.jpeg_compress_struct.Num_components>|The number of color components for JPEG color space|
|<xref:BitMiracle.LibJpeg.Classic.jpeg_compress_struct.Optimize_coding>|The way of using Huffman coding tables|
|<xref:BitMiracle.LibJpeg.Classic.jpeg_compress_struct.Restart_interval>, <xref:BitMiracle.LibJpeg.Classic.jpeg_compress_struct.Restart_in_rows>|To emit restart markers in the JPEG file, set one of these nonzero|
|<xref:BitMiracle.LibJpeg.Classic.jpeg_compress_struct.Smoothing_factor>|Gets/sets smoothing level|
|<xref:BitMiracle.LibJpeg.Classic.jpeg_compress_struct.Write_JFIF_header>|Emits JFIF APP0 marker|
|<xref:BitMiracle.LibJpeg.Classic.jpeg_compress_struct.JFIF_major_version>, <xref:BitMiracle.LibJpeg.Classic.jpeg_compress_struct.JFIF_minor_version>|The version number to be written into the JFIF marker|
|<xref:BitMiracle.LibJpeg.Classic.jpeg_compress_struct.Density_unit>, <xref:BitMiracle.LibJpeg.Classic.jpeg_compress_struct.X_density>, <xref:BitMiracle.LibJpeg.Classic.jpeg_compress_struct.Y_density>|The resolution information to be written into the JFIF marker|
|<xref:BitMiracle.LibJpeg.Classic.jpeg_compress_struct.Write_Adobe_marker>|Emits Adobe APP14 marker|
