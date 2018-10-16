When the default error handler is used, any error detected inside the JPEG routines will cause a message to be printed on console, followed by throwing of exception. You can supply your own error handling routines to override this behavior and to control the treatment of nonfatal warnings and trace/debug messages. 

The JPEG library never writes any message directly; it always goes through the error handling routines. Three classes of messages are recognized:

* Fatal errors: the library cannot continue.
* Warnings: the library can continue, but the data is corrupt, and a damaged output image is likely to result.
* Trace/informational messages. These come with a trace level indicating the importance of the message; you can control the verbosity of the program by adjusting the maximum trace level that will be displayed.

All of the error handling routines will receive the JPEG object (a <xref:BitMiracle.LibJpeg.Classic.jpeg_common_struct> which points to either a <xref:BitMiracle.LibJpeg.Classic.jpeg_compress_struct> or a <xref:BitMiracle.LibJpeg.Classic.jpeg_decompress_struct>). This struct includes the error manager struct in its <xref:BitMiracle.LibJpeg.Classic.jpeg_common_struct.Err> property.

The individual methods that you might wish to override are:

|Method|Description|
|---|---|
|<xref:BitMiracle.LibJpeg.Classic.jpeg_error_mgr.error_exit>|Receives control for a fatal error|
|<xref:BitMiracle.LibJpeg.Classic.jpeg_error_mgr.output_message>|Actual output of any JPEG message|
|<xref:BitMiracle.LibJpeg.Classic.jpeg_error_mgr.format_message>|Constructs a readable error message string|
|<xref:BitMiracle.LibJpeg.Classic.jpeg_error_mgr.emit_message(System.Int32)>|Decide whether or not to emit a warning or trace message|

Only <xref:BitMiracle.LibJpeg.Classic.jpeg_error_mgr.error_exit> and <xref:BitMiracle.LibJpeg.Classic.jpeg_error_mgr.emit_message(System.Int32)> are called from the rest of the JPEG library; the other two are internal to the error handler.

You can get the actual message texts using protected virtual method <xref:BitMiracle.LibJpeg.Classic.jpeg_error_mgr.GetMessageText(System.Int32)>. It may be useful for an application to add its own message texts that are handled by the same mechanism. You can override <xref:BitMiracle.LibJpeg.Classic.jpeg_error_mgr.GetMessageText(System.Int32)> for this purpose. If you number the addon messages beginning at 1000 or so, you won't have to worry about conflicts with the library's built-in messages. See the sample applications **cjpeg/djpeg** for an example of using addon messages (class BitMiracle.cdJpeg.cd_jpeg_error_mgr) 

Actual invocation of the error handler is done via methods defined in <xref:BitMiracle.LibJpeg.Classic.jpeg_common_struct>:

|Method|Description|
|---|---|
|<xref:BitMiracle.LibJpeg.Classic.jpeg_common_struct.ERREXIT(BitMiracle.LibJpeg.Classic.J_MESSAGE_CODE)>|For fatal errors|
|<xref:BitMiracle.LibJpeg.Classic.jpeg_common_struct.WARNMS(BitMiracle.LibJpeg.Classic.J_MESSAGE_CODE)>|For corrupt-data warnings|
|<xref:BitMiracle.LibJpeg.Classic.jpeg_common_struct.TRACEMS(System.Int32,BitMiracle.LibJpeg.Classic.J_MESSAGE_CODE)>|For trace and informational messages|

These methods store the message code and any additional parameters into the error manager, then invoke the <xref:BitMiracle.LibJpeg.Classic.jpeg_error_mgr.error_exit> or <xref:BitMiracle.LibJpeg.Classic.jpeg_error_mgr.emit_message(System.Int32)> methods. The variants of each macro are for varying numbers of additional parameters. The additional parameters are inserted into the generated message using standard method string.Format. 
