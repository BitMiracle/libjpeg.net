The JPEG standard itself is "color blind" and doesn't specify any particular color space. It is customary to convert color data to a luminance/chrominance color space before compressing, since this permits greater compression. The existing de-facto JPEG file format standards specify YCbCr or grayscale data (JFIF), or grayscale, RGB, YCbCr, CMYK, or YCCK (Adobe). For special applications such as multispectral images, other color spaces can be used, but it must be understood that such files will be unportable. 

The JPEG library can handle the most common colorspace conversions (namely RGB <=> YCbCr and CMYK <=> YCCK). It can also deal with data of an unknown color space, passing it through without conversion. If you deal extensively with an unusual color space, you can easily extend the library to understand additional color spaces and perform appropriate conversions. 

For compression, the source data's color space is specified by property <xref:BitMiracle.LibJpeg.Classic.jpeg_compress_struct.In_color_space>. This is transformed to the JPEG file's color space given by <xref:BitMiracle.LibJpeg.Classic.jpeg_compress_struct.Jpeg_color_space>. <xref:BitMiracle.LibJpeg.Classic.jpeg_compress_struct.jpeg_set_defaults> chooses a reasonable JPEG color space depending on <xref:BitMiracle.LibJpeg.Classic.jpeg_compress_struct.In_color_space>, but you can override this by calling <xref:BitMiracle.LibJpeg.Classic.jpeg_compress_struct.jpeg_set_colorspace(BitMiracle.LibJpeg.Classic.J_COLOR_SPACE)>. Of course you may select a supported transformation. 

The following transformations are currently supported:

* RGB => YCbCr
* RGB => GRAYSCALE
* YCbCr => GRAYSCALE
* CMYK => YCCK
plus the null transforms: GRAYSCALE => GRAYSCALE, RGB => RGB, YCbCr => YCbCr, CMYK => CMYK, YCCK => YCCK, and UNKNOWN => UNKNOWN. 

The de-facto file format standards (JFIF and Adobe) specify APPn markers that indicate the color space of the JPEG file. It is important to ensure that these are written correctly, or omitted if the JPEG file's color space is not one of the ones supported by the de-facto standards. <xref:BitMiracle.LibJpeg.Classic.jpeg_compress_struct.jpeg_set_colorspace(BitMiracle.LibJpeg.Classic.J_COLOR_SPACE)> will set the compression parameters to include or omit the APPn markers properly, so long as it is told the truth about the JPEG color space. For example, if you are writing some random 3-component color space without conversion, don't try to fake out the library by setting <xref:BitMiracle.LibJpeg.Classic.jpeg_compress_struct.In_color_space> and <xref:BitMiracle.LibJpeg.Classic.jpeg_compress_struct.Jpeg_color_space> to <xref:BitMiracle.LibJpeg.Classic.J_COLOR_SPACE.JCS_YCbCr>; use <xref:BitMiracle.LibJpeg.Classic.J_COLOR_SPACE.JCS_UNKNOWN>. You may want to write an APPn marker of your own devising to identify the colorspace - see [Special markers](~/articles/KB/special-markers.html). 

For decompression, the JPEG file's color space is given in <xref:BitMiracle.LibJpeg.Classic.jpeg_compress_struct.Jpeg_color_space>, and this is transformed to the output color space <xref:BitMiracle.LibJpeg.Classic.jpeg_decompress_struct.Out_color_space>. <xref:BitMiracle.LibJpeg.Classic.jpeg_decompress_struct.jpeg_read_header(System.Boolean)>'s setting of <xref:BitMiracle.LibJpeg.Classic.jpeg_compress_struct.Jpeg_color_space> can be relied on if the file conforms to JFIF or Adobe conventions, but otherwise it is no better than a guess. If you know the JPEG file's color space for certain, you can override <xref:BitMiracle.LibJpeg.Classic.jpeg_decompress_struct.jpeg_read_header(System.Boolean)>'s guess by setting <xref:BitMiracle.LibJpeg.Classic.jpeg_compress_struct.Jpeg_color_space>. <xref:BitMiracle.LibJpeg.Classic.jpeg_decompress_struct.jpeg_read_header(System.Boolean)> also selects a default output color space based on (its guess of) <xref:BitMiracle.LibJpeg.Classic.jpeg_compress_struct.Jpeg_color_space>; set <xref:BitMiracle.LibJpeg.Classic.jpeg_decompress_struct.Out_color_space> to override this. Again, you must select a supported transformation. 

The following transformations are currently supported:

* YCbCr => GRAYSCALE
* YCbCr => RGB
* GRAYSCALE => RGB
* YCCK => CMYK
as well as the null transforms. (Since GRAYSCALE => RGB is provided, an application can force grayscale JPEGs to look like color JPEGs if it only wants to handle one case.) 

The two-pass color quantizer is specialized to handle RGB data (it weights distances appropriately for RGB colors). You'll need to modify the code if you want to use it for non-RGB output color spaces. 
