LibJpeg.NET
The Bit Miracle's JPEG software
===============================
Please send bug reports, offers of help, etc. to support@bitmiracle.com

This package contains C# software to implement JPEG image compression and
decompression.  JPEG (pronounced "jay-peg") is a standardized compression
method for full-color and gray-scale images.  JPEG is intended for compressing
"real-world" scenes; line drawings, cartoons and other non-realistic images
are not its strong suit.  JPEG is lossy, meaning that the output image is not
exactly identical to the input image.  Hence you must not use JPEG if you
have to have identical output bits.  However, on typical photographic images,
very good compression levels can be obtained with no visible change, and
remarkably high compression levels are possible if you can tolerate a
low-quality image.  For more details, see the references, or just experiment
with various compression settings.

This software implements JPEG baseline, extended-sequential, and progressive
compression processes.  Provision is made for supporting all variants of these
processes, although some uncommon parameter settings aren't implemented yet.
For legal reasons, we are not distributing code for the arithmetic-coding
variants of JPEG; see LEGAL ISSUES.  We have made no provision for supporting
the hierarchical or lossless processes defined in the standard.

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

We welcome the use of this software as a component of commercial products.
No royalty is required, but we do ask for an acknowledgement in product
documentation, as described under License.

License
=======
Free, can be used in commercial applications without royalty, with acknowledgement.

In plain English:

1. We don't promise that this software works.  (But if you find any bugs,
   please let us know!)
2. You can use this software for whatever you want.  You don't have to pay us.
3. You may not pretend that you wrote this software.  If you use it in a
   program, you must acknowledge somewhere in your documentation that
   you've used the Bit Miracle code.

In legalese:

The authors make NO WARRANTY or representation, either express or implied,
with respect to this software, its quality, accuracy, merchantability, or
fitness for a particular purpose.  This software is provided "AS IS", and you,
its user, assume the entire risk as to its quality and accuracy.

This software is copyright (C) 2008-2009, Bit Miracle
http://www.bitmiracle.com

This software is based in part on the work of the Independent JPEG Group
Copyright (C) 1991-1998, Thomas G. Lane.
All Rights Reserved except as specified below.

Permission is hereby granted to use, copy, modify, and distribute this
software (or portions thereof) for any purpose, without fee, subject to these
conditions:
(1) If any part of the source code for this software is distributed, then this
README file must be included, with this copyright and no-warranty notice
unaltered; and any additions, deletions, or changes to the original files
must be clearly indicated in accompanying documentation.
(2) If only executable code is distributed, then the accompanying
documentation must state that "this software is based in part on the work of
the Bit Miracle".
(3) Permission for use of this software is granted only if the user accepts
full responsibility for any undesirable consequences; the authors accept
NO LIABILITY for damages of any kind.

These conditions apply to any software derived from or based on the Bit Miracle
code, not just to the unmodified library.  If you use our work, you ought to
acknowledge us.

Permission is NOT granted for the use of any Bit Miracle author's name or company
name in advertising or publicity relating to this software or products derived 
from it. This software may be referred to only as "the Bit Miracle's software".

We specifically permit and encourage the use of this software as the basis of
commercial products, provided that all warranty or liability claims are
assumed by the product vendor.


LEGAL ISSUES
============

It appears that the arithmetic coding option of the JPEG spec is covered by
patents owned by IBM, AT&T, and Mitsubishi.  Hence arithmetic coding cannot
legally be used without obtaining one or more licenses.  For this reason,
support for arithmetic coding has been removed from the free JPEG software.
(Since arithmetic coding provides only a marginal gain over the unpatented
Huffman mode, it is unlikely that very many implementations will support it.)
So far as we are aware, there are no patent restrictions on the remaining
code.
