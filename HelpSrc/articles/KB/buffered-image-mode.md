In buffered-image mode, the library stores the partially decoded image in a coefficient buffer, from which it can be read out as many times as desired. This mode is typically used for incremental display of progressive JPEG files, but it can be used with any JPEG file. Each scan of a progressive JPEG file adds more data (more detail) to the buffered image. The application can display in lockstep with the source file (one display pass per input scan), or it can allow input processing to outrun display processing. By making input and display processing run independently, it is possible for the application to adapt progressive display to a wide range of data transmission rates. 

The basic control flow for buffered-image decoding is:

```cs
Allocate and initialize a JPEG decompression object
Set data source
jpeg_read_header();
Set overall decompression parameters
cinfo.Buffered_image = true; /* select buffered-image mode */
jpeg_start_decompress();
for (each output pass)
{
    adjust output decompression parameters if required
    jpeg_start_output(); /* start a new output pass */
    for (all scanlines in image)
    {
        jpeg_read_scanlines();
        display scanlines
    }
    jpeg_finish_output(); /* terminate output pass */
}
jpeg_finish_decompress();
```

This differs from ordinary unbuffered decoding in that there is an additional level of looping. The application can choose how many output passes to make and how to display each pass. 

The simplest approach to displaying progressive images is to do one display pass for each scan appearing in the input file. In this case the start-output call should look as: 

```cs
cinfo.jpeg_start_output(cinfo.Input_scan_number);
```

and the outer loop condition is typically:

```cs
while (!cinfo.jpeg_input_complete())
```

Alternative solution - you can use a loop counter starting at 1 if you like. The library automatically reads data as necessary to complete each requested scan, and <xref:BitMiracle.LibJpeg.Classic.jpeg_decompress_struct.jpeg_finish_output> advances to the next scan or end-of-image marker (hence <xref:BitMiracle.LibJpeg.Classic.jpeg_decompress_struct.Input_scan_number> will be incremented by the time control arrives back at <xref:BitMiracle.LibJpeg.Classic.jpeg_decompress_struct.jpeg_start_output(System.Int32)>). With this technique, data is read from the input file only as needed, and input and output processing run in lockstep. 

After reading the final scan and reaching the end of the input file, the buffered image remains available; it can be read additional times by repeating the <xref:BitMiracle.LibJpeg.Classic.jpeg_decompress_struct.jpeg_start_output(System.Int32)>/<xref:BitMiracle.LibJpeg.Classic.jpeg_decompress_struct.jpeg_read_scanlines(System.Byte[][],System.Int32)>/<xref:BitMiracle.LibJpeg.Classic.jpeg_decompress_struct.jpeg_finish_output> sequence. For example, a useful technique is to use fast one-pass color quantization for display passes made while the image is arriving, followed by a final display pass using two-pass quantization for highest quality. This is done by changing the library parameters before the final output pass. Changing parameters between passes is discussed in detail below. 

In general the last scan of a progressive file cannot be recognized as such until after it is read, so a post-input display pass is the best approach if you want special processing in the final pass. 

When done with the image, be sure to call <xref:BitMiracle.LibJpeg.Classic.jpeg_decompress_struct.jpeg_finish_decompress> to release the buffered image. 

If input data arrives faster than it can be displayed, the application can cause the library to decode input data in advance of what's needed to produce output. This is done by calling the routine <xref:BitMiracle.LibJpeg.Classic.jpeg_decompress_struct.jpeg_consume_input>. This routine reads some additional data and returns when one of the indicated significant events occurs.

The library's output processing will automatically call <xref:BitMiracle.LibJpeg.Classic.jpeg_decompress_struct.jpeg_consume_input> whenever the output processing overtakes the input; thus, simple lockstep display requires no direct calls to <xref:BitMiracle.LibJpeg.Classic.jpeg_decompress_struct.jpeg_consume_input>. But by adding calls to <xref:BitMiracle.LibJpeg.Classic.jpeg_decompress_struct.jpeg_consume_input>, you can absorb data in advance of what is being displayed. This has two benefits: 
* You can limit buildup of unprocessed data in your input buffer.
* You can eliminate extra display passes by paying attention to the state of the library's input processing.

The first of these benefits only requires interspersing calls to <xref:BitMiracle.LibJpeg.Classic.jpeg_decompress_struct.jpeg_consume_input> with your display operations and any other processing you may be doing. To avoid wasting cycles due to backtracking, it's best to call <xref:BitMiracle.LibJpeg.Classic.jpeg_decompress_struct.jpeg_consume_input> only after a hundred or so new bytes have arrived. 

Note: the JPEG library currently is not thread-safe. You must not call <xref:BitMiracle.LibJpeg.Classic.jpeg_decompress_struct.jpeg_consume_input> from one thread of control if a different library routine is working on the same JPEG object in another thread. 

When input arrives fast enough that more than one new scan is available before you start a new output pass, you may as well skip the output pass corresponding to the completed scan. This occurs for free if you pass cinfo.Input_scan_number as the target scan number to <xref:BitMiracle.LibJpeg.Classic.jpeg_decompress_struct.jpeg_start_output(System.Int32)>. The <xref:BitMiracle.LibJpeg.Classic.jpeg_decompress_struct.Input_scan_number> property is simply the index of the scan currently being consumed by the input processor. You can ensure that this is up-to-date by emptying the input buffer just before calling <xref:BitMiracle.LibJpeg.Classic.jpeg_decompress_struct.jpeg_start_output(System.Int32)>: call <xref:BitMiracle.LibJpeg.Classic.jpeg_decompress_struct.jpeg_consume_input> repeatedly until it returns <xref:BitMiracle.LibJpeg.Classic.ReadResult.JPEG_SUSPENDED> or <xref:BitMiracle.LibJpeg.Classic.ReadResult.JPEG_REACHED_EOI>. 

The target scan number passed to <xref:BitMiracle.LibJpeg.Classic.jpeg_decompress_struct.jpeg_start_output(System.Int32)> is saved in the cinfo.Output_scan_number property. The library's output processing calls <xref:BitMiracle.LibJpeg.Classic.jpeg_decompress_struct.jpeg_consume_input> whenever the current input scan number and row within that scan is less than or equal to the current output scan number and row. Thus, input processing can "get ahead" of the output processing but is not allowed to "fall behind". You can achieve several different effects by manipulating this interlock rule. For example, if you pass a target scan number greater than the current input scan number, the output processor will wait until that scan starts to arrive before producing any output. (To avoid an infinite loop, the target scan number is automatically reset to the last scan number when the end of image is reached. Thus, if you specify a large target scan number, the library will just absorb the entire input file and then perform an output pass. This is effectively the same as what <xref:BitMiracle.LibJpeg.Classic.jpeg_decompress_struct.jpeg_start_decompress> does when you don't select buffered-image mode.) When you pass a target scan number equal to the current input scan number, the image is displayed no faster than the current input scan arrives. The final possibility is to pass a target scan number less than the current input scan number; this disables the input/output interlock and causes the output processor to simply display whatever it finds in the image buffer, without waiting for input. (However, the library will not accept a target scan number less than one, so you can't avoid waiting for the first scan.) 

When data is arriving faster than the output display processing can advance through the image, <xref:BitMiracle.LibJpeg.Classic.jpeg_decompress_struct.jpeg_consume_input> will store data into the buffered image beyond the point at which the output processing is reading data out again. If the input arrives fast enough, it may "wrap around" the buffer to the point where the input is more than one whole scan ahead of the output. If the output processing simply proceeds through its display pass without paying attention to the input, the effect seen on-screen is that the lower part of the image is one or more scans better in quality than the upper part. Then, when the next output scan is started, you have a choice of what target scan number to use. The recommended choice is to use the current input scan number at that time, which implies that you've skipped the output scans corresponding to the input scans that were completed while you processed the previous output scan. In this way, the decoder automatically adapts its speed to the arriving data, by skipping output scans as necessary to keep up with the arriving data. 

When using this strategy, you'll want to be sure that you perform a final output pass after receiving all the data; otherwise your last display may not be full quality across the whole screen. So the right outer loop logic is something like this: 

```cs
do
{
    Absorb any waiting input by calling jpeg_consume_input()
    final_pass = cinfo.jpeg_input_complete();
    Adjust output decompression parameters if required
    cinfo.jpeg_start_output(cinfo.Input_scan_number);
    ...
    jpeg_finish_output();
} while (! final_pass);
```
rather than quitting as soon as <xref:BitMiracle.LibJpeg.Classic.jpeg_decompress_struct.jpeg_input_complete> returns `true`. This arrangement makes it simple to use higher-quality decoding parameters for the final pass. But if you don't want to use special parameters for the final pass, the right loop logic is like this: 

```cs
for (;;)
{
    Absorb any waiting input by calling jpeg_consume_input()
    cinfo.jpeg_start_output(cinfo.Input_scan_number);
    ...
    jpeg_finish_output()
    if (cinfo.jpeg_input_complete() && cinfo.Input_scan_number == cinfo.Output_scan_number)
        break;
}
```

In this case you don't need to know in advance whether an output pass is to be the last one, so it's not necessary to have reached EOF before starting the final output pass; rather, what you want to test is whether the output pass was performed in sync with the final input scan. This form of the loop will avoid an extra output pass whenever the decoder is able (or nearly able) to keep up with the incoming data. 

When the data transmission speed is high, you might begin a display pass, then find that much or all of the file has arrived before you can complete the pass. (You can detect this by noting the <xref:BitMiracle.LibJpeg.Classic.ReadResult.JPEG_REACHED_EOI> return code from <xref:BitMiracle.LibJpeg.Classic.jpeg_decompress_struct.jpeg_consume_input>, or equivalently by testing <xref:BitMiracle.LibJpeg.Classic.jpeg_decompress_struct.jpeg_input_complete>) In this situation you may wish to abort the current display pass and start a new one using the newly arrived information. To do so, just call <xref:BitMiracle.LibJpeg.Classic.jpeg_decompress_struct.jpeg_finish_output> and then start a new pass with <xref:BitMiracle.LibJpeg.Classic.jpeg_decompress_struct.jpeg_start_output(System.Int32)>.

A variant strategy is to abort and restart display if more than one complete scan arrives during an output pass; this can be detected by noting <xref:BitMiracle.LibJpeg.Classic.ReadResult.JPEG_REACHED_SOS> returns and/or examining cinfo.Input_scan_number. This idea should be employed with caution, however, since the display process might never get to the bottom of the image before being aborted, resulting in the lower part of the screen being several passes worse than the upper. In most cases it's probably best to abort an output pass only if the whole file has arrived and you want to begin the final output pass immediately. 

When receiving data across a communication link, we recommend always using the current input scan number for the output target scan number; if a higher-quality final pass is to be done, it should be started (aborting any incomplete output pass) as soon as the end of file is received. However, many other strategies are possible. For example, the application can examine the parameters of the current input scan and decide whether to display it or not. If the scan contains only chroma data, one might choose not to use it as the target scan, expecting that the scan will be small and will arrive quickly. To skip to the next scan, call <xref:BitMiracle.LibJpeg.Classic.jpeg_decompress_struct.jpeg_consume_input> until it returns <xref:BitMiracle.LibJpeg.Classic.ReadResult.JPEG_REACHED_SOS> or <xref:BitMiracle.LibJpeg.Classic.ReadResult.JPEG_REACHED_EOI>. Or just use the next higher number as the target scan for <xref:BitMiracle.LibJpeg.Classic.jpeg_decompress_struct.jpeg_start_output(System.Int32)>; but that method doesn't let you inspect the next scan's parameters before deciding to display it.

In buffered-image mode, <xref:BitMiracle.LibJpeg.Classic.jpeg_decompress_struct.jpeg_start_decompress> never performs input and thus never suspends. An application that uses input suspension with buffered-image mode must be prepared for suspension returns from these routines: 

* <xref:BitMiracle.LibJpeg.Classic.jpeg_decompress_struct.jpeg_start_output(System.Int32)> performs input only if you request 2-pass quantization and the target scan isn't fully read yet. (This is discussed below.) 
* <xref:BitMiracle.LibJpeg.Classic.jpeg_decompress_struct.jpeg_read_scanlines(System.Byte[][],System.Int32)>, as always, returns the number of scanlines that it was able to produce before suspending. 
* <xref:BitMiracle.LibJpeg.Classic.jpeg_decompress_struct.jpeg_finish_output> will read any markers following the target scan, up to the end of the file or the SOS marker that begins another scan. (but it reads no input if <xref:BitMiracle.LibJpeg.Classic.jpeg_decompress_struct.jpeg_consume_input> has already reached the end of the file or a SOS marker beyond the target output scan) 
* <xref:BitMiracle.LibJpeg.Classic.jpeg_decompress_struct.jpeg_finish_decompress> will read until the end of file, and thus can suspend if the end hasn't already been reached (as can be tested by calling <xref:BitMiracle.LibJpeg.Classic.jpeg_decompress_struct.jpeg_input_complete>).

<xref:BitMiracle.LibJpeg.Classic.jpeg_decompress_struct.jpeg_start_output(System.Int32)>, <xref:BitMiracle.LibJpeg.Classic.jpeg_decompress_struct.jpeg_finish_output>, and <xref:BitMiracle.LibJpeg.Classic.jpeg_decompress_struct.jpeg_finish_decompress> all return `true` if they completed their tasks, `false` if they had to suspend. In the event of a `false` return, the application must load more input data and repeat the call. Applications that use non-suspending data sources need not check the return values of these three routines. 

It is possible to change decoding parameters between output passes in the buffered-image mode. The decoder library currently supports only very limited changes of parameters. Only the following parameters changes are allowed after <xref:BitMiracle.LibJpeg.Classic.jpeg_decompress_struct.jpeg_start_decompress> is called:

* <xref:BitMiracle.LibJpeg.Classic.jpeg_decompress_struct.Dct_method> can be changed before each call to <xref:BitMiracle.LibJpeg.Classic.jpeg_decompress_struct.jpeg_start_output(System.Int32)>. For example, one could use a fast DCT method for early scans, changing to a higher quality method for the final scan. 
* <xref:BitMiracle.LibJpeg.Classic.jpeg_decompress_struct.Dither_mode> can be changed before each call to <xref:BitMiracle.LibJpeg.Classic.jpeg_decompress_struct.jpeg_start_output(System.Int32)>; of course this has no impact if not using color quantization. Typically one would use ordered dither for initial passes, then switch to Floyd-Steinberg dither for the final pass. Caution: changing dither mode can cause more memory to be allocated by the library, but the amount of memory involved is not large (a scanline or so). 
* <xref:BitMiracle.LibJpeg.Classic.jpeg_decompress_struct.Do_block_smoothing> can be changed before each call to <xref:BitMiracle.LibJpeg.Classic.jpeg_decompress_struct.jpeg_start_output(System.Int32)>. This setting is relevant only when decoding a progressive JPEG image. During the first DC-only scan, block smoothing provides a very "fuzzy" look instead of the very "blocky" look seen without it; which is better seems a matter of personal taste. But block smoothing is nearly always a win during later stages, especially when decoding a successive-approximation image: smoothing helps to hide the slight blockiness that otherwise shows up on smooth gradients until the lowest coefficient bits are sent. 
* Color quantization mode (see <xref:BitMiracle.LibJpeg.Classic.jpeg_decompress_struct.Quantize_colors>) can be changed under the rules described below. You cannot change between full-color and quantized output (because that would alter the required I/O buffer sizes), but you can change which quantization method is used.

When generating color-quantized output, changing quantization method is a very useful way of switching between high-speed and high-quality display. The library allows you to change among its three quantization methods: 

1. Single-pass quantization to a fixed color cube.

    Selected by `cinfo.Two_pass_quantize = false` and `cinfo.Colormap = null`.
    
2. Single-pass quantization to an application-supplied colormap.

    Selected by setting `cinfo.Colormap` to point to the colormap (the value of <xref:BitMiracle.LibJpeg.Classic.jpeg_decompress_struct.Two_pass_quantize> is ignored); also set `cinfo.Actual_number_of_colors`. 
    
3. Two-pass quantization to a colormap chosen specifically for the image.

    Selected by `cinfo.Two_pass_quantize = true` and `cinfo.Colormap = null`. (This is the default setting selected by <xref:BitMiracle.LibJpeg.Classic.jpeg_decompress_struct.jpeg_read_header(System.Boolean)>, but it is probably NOT what you want for the first pass of progressive display!) 

These methods offer successively better quality and lesser speed. However, only the first method is available for quantizing in non-RGB color spaces. 

IMPORTANT: because the different quantizer methods have very different working-storage requirements, the library requires you to indicate which one(s) you intend to use before you call <xref:BitMiracle.LibJpeg.Classic.jpeg_decompress_struct.jpeg_start_decompress>. You do this by setting one or more of these three cinfo properties to `true`: 

|Property|Description|
|---|---|
|<xref:BitMiracle.LibJpeg.Classic.jpeg_decompress_struct.Enable_1pass_quant>|Fixed color cube colormap|
|<xref:BitMiracle.LibJpeg.Classic.jpeg_decompress_struct.Enable_external_quant>|Externally-supplied colormap|
|<xref:BitMiracle.LibJpeg.Classic.jpeg_decompress_struct.Enable_2pass_quant>|Two-pass custom colormap|

All three are initialized `false` by <xref:BitMiracle.LibJpeg.Classic.jpeg_decompress_struct.jpeg_read_header(System.Boolean)>. But <xref:BitMiracle.LibJpeg.Classic.jpeg_decompress_struct.jpeg_start_decompress> automatically sets `true` the one selected by the current <xref:BitMiracle.LibJpeg.Classic.jpeg_decompress_struct.Two_pass_quantize> and Colormap settings, so you only need to set the enable flags for any other quantization methods you plan to change to later. 

After setting the enable flags correctly at <xref:BitMiracle.LibJpeg.Classic.jpeg_decompress_struct.jpeg_start_decompress> time, you can change to any enabled quantization method by setting <xref:BitMiracle.LibJpeg.Classic.jpeg_decompress_struct.Two_pass_quantize> and Colormap properly just before calling <xref:BitMiracle.LibJpeg.Classic.jpeg_decompress_struct.jpeg_start_output(System.Int32)>. You must explicitly set `cinfo.Colormap = null` when switching to 1-pass or 2-pass mode from a different mode, or when you want the 2-pass quantizer to be re-run to generate a new colormap. 

Note that in buffered-image mode, the library generates any requested colormap during <xref:BitMiracle.LibJpeg.Classic.jpeg_decompress_struct.jpeg_start_output(System.Int32)>, not during <xref:BitMiracle.LibJpeg.Classic.jpeg_decompress_struct.jpeg_start_decompress>. 

When using two-pass quantization, <xref:BitMiracle.LibJpeg.Classic.jpeg_decompress_struct.jpeg_start_output(System.Int32)> makes a pass over the buffered image to determine the optimum color map; it therefore may take a significant amount of time, whereas ordinarily it does little work. The progress monitor hook is called during this pass, if defined. It is also important to realize that if the specified target scan number is greater than or equal to the current input scan number, <xref:BitMiracle.LibJpeg.Classic.jpeg_decompress_struct.jpeg_start_output(System.Int32)> will attempt to consume input as it makes this pass. If you use a suspending data source, you need to check for a false return from <xref:BitMiracle.LibJpeg.Classic.jpeg_decompress_struct.jpeg_start_output(System.Int32)> under these conditions. The combination of 2-pass quantization and a not-yet-fully-read target scan is the only case in which <xref:BitMiracle.LibJpeg.Classic.jpeg_decompress_struct.jpeg_start_output(System.Int32)> will consume input.

Application authors who support buffered-image mode may be tempted to use it for all JPEG images, even single-scan ones. This will work, but it is inefficient: there is no need to create an image-sized coefficient buffer for single-scan images. Requesting buffered-image mode for such an image wastes memory. Worse, it can cost time on large images, since the buffered data has to be swapped out or written to a temporary file. If you are concerned about maximum performance on baseline JPEG files, you should use buffered-image mode only when the incoming file actually has multiple scans. This can be tested by calling jpeg_has_multiple_scans(), which will return a correct result at any time after <xref:BitMiracle.LibJpeg.Classic.jpeg_decompress_struct.jpeg_read_header(System.Boolean)> completes.

In some applications it may be convenient to use <xref:BitMiracle.LibJpeg.Classic.jpeg_decompress_struct.jpeg_consume_input> for all input processing, including reading the initial markers; that is, you may wish to call <xref:BitMiracle.LibJpeg.Classic.jpeg_decompress_struct.jpeg_consume_input> instead of <xref:BitMiracle.LibJpeg.Classic.jpeg_decompress_struct.jpeg_read_header(System.Boolean)> during startup. This works, but note that you must check for <xref:BitMiracle.LibJpeg.Classic.ReadResult.JPEG_REACHED_SOS> and <xref:BitMiracle.LibJpeg.Classic.ReadResult.JPEG_REACHED_EOI> return codes as the equivalent of <xref:BitMiracle.LibJpeg.Classic.jpeg_decompress_struct.jpeg_read_header(System.Boolean)>'s codes. Once the first SOS marker has been reached, you must call <xref:BitMiracle.LibJpeg.Classic.jpeg_decompress_struct.jpeg_start_decompress> before <xref:BitMiracle.LibJpeg.Classic.jpeg_decompress_struct.jpeg_consume_input> will consume more input; it'll just keep returning <xref:BitMiracle.LibJpeg.Classic.ReadResult.JPEG_REACHED_SOS> until you do. If you read a tables-only file this way, <xref:BitMiracle.LibJpeg.Classic.jpeg_decompress_struct.jpeg_consume_input> will return <xref:BitMiracle.LibJpeg.Classic.ReadResult.JPEG_REACHED_EOI> without ever returning <xref:BitMiracle.LibJpeg.Classic.ReadResult.JPEG_REACHED_SOS>; be sure to check for this case. If this happens, the decompressor will not read any more input until you call jpeg_abort()  to reset it. It is OK to call <xref:BitMiracle.LibJpeg.Classic.jpeg_decompress_struct.jpeg_consume_input> even when not using buffered-image mode, but in that case it's basically a no-op after the initial markers have been read: it will just return <xref:BitMiracle.LibJpeg.Classic.ReadResult.JPEG_SUSPENDED>. 
