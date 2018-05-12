LibJpeg.NET
The Bit Miracle's JPEG software
===============================
Please send bug reports, offers of help, etc. to support@bitmiracle.com

This package contains C# software to implement JPEG image encoding and decoding.
JPEG (pronounced "jay-peg") is a standardized compression method for full-color
and grayscale images.

This software implements JPEG baseline, extended-sequential, and progressive
compression processes.  Provision is made for supporting all variants of these
processes, although some uncommon parameter settings aren't implemented yet.
We have made no provision for supporting the hierarchical or lossless
processes defined in the standard.

We provide a set of library routines for reading and writing JPEG image files,
plus two sample applications "cjpeg" and "djpeg", which use the library to
perform conversion between JPEG and some other popular image file formats.
The library is intended to be reused in other applications.

In order to support file conversion and viewing software, we have included
considerable functionality beyond the bare JPEG coding/decoding capability;
for example, the color quantization modules are not strictly part of JPEG
decoding, but they are essential for output to colormapped file formats or
colormapped displays.  These extra functions can be compiled out of the
library if not required for a particular application.

The emphasis in designing this software has been on achieving portability and
flexibility, while also making it fast enough to be useful.  In particular,
the software is not intended to be read as a tutorial on JPEG.  Rather, it
is intended to be reliable, portable, industrial-strength code.  We do not
claim to have achieved that goal in every aspect of the software, but we
strive for it.

We welcome the use of this software as a component of commercial products.
No royalty is required, but we do ask for an acknowledgement in product
documentation.

License
=======
Please read License.txt

This software is copyright (C) 2008-2018, Bit Miracle
http://www.bitmiracle.com

This software is based in part on the work of the Independent JPEG Group
Copyright (C) 1991-2016, Thomas G. Lane, Guido Vollbeding.
All Rights Reserved except as specified below.
