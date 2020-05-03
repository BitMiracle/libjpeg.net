using System.IO;
using BitMiracle.LibJpeg.Classic;

namespace LibJpeg.Net.Benchmarks
{
    class VirtualStreamSourceManager : jpeg_source_mgr
    {
        private const int InputBufferSize = 4096;
        private readonly jpeg_decompress_struct m_cinfo;

        private readonly Stream m_infile;       /* source stream */
        private readonly byte[] m_buffer;     /* start of buffer */
        private bool m_start_of_file; /* have we gotten any data yet? */

        public VirtualStreamSourceManager(jpeg_decompress_struct cinfo, Stream src)
        {
            m_cinfo = cinfo;
            m_buffer = new byte[InputBufferSize];

            m_infile = src;
            m_infile.Seek(0, SeekOrigin.Begin);
            initInternalBuffer(null, 0);
        }

        public override void init_source()
        {
            /* We reset the empty-input-file flag for each image,
             * but we don't clear the input buffer.
             * This is correct behavior for reading a series of images from one source.
             */
            m_start_of_file = true;
        }

        /// <summary>
        /// Fill the input buffer - called whenever buffer is emptied.
        /// 
        /// In typical applications, this should read fresh data into the buffer
        /// (ignoring the current state of next_input_byte and bytes_in_buffer),
        /// reset the pointer and count to the start of the buffer, and return true
        /// indicating that the buffer has been reloaded.  It is not necessary to
        /// fill the buffer entirely, only to obtain at least one more byte.
        /// 
        /// There is no such thing as an EOF return.  If the end of the file has been
        /// reached, the routine has a choice of ERREXIT() or inserting fake data into
        /// the buffer.  In most cases, generating a warning message and inserting a
        /// fake EOI marker is the best course of action --- this will allow the
        /// decompressor to output however much of the image is there.  However,
        /// the resulting error message is misleading if the real problem is an empty
        /// input file, so we handle that case specially.
        /// 
        /// In applications that need to be able to suspend compression due to input
        /// not being available yet, a false return indicates that no more data can be
        /// obtained right now, but more may be forthcoming later.  In this situation,
        /// the decompressor will return to its caller (with an indication of the
        /// number of scan lines it has read, if any).  The application should resume
        /// decompression after it has loaded more data into the input buffer.  Note
        /// that there are substantial restrictions on the use of suspension --- see
        /// the documentation.
        /// 
        /// When suspending, the decompressor will back up to a convenient restart point
        /// (typically the start of the current MCU). next_input_byte and bytes_in_buffer
        /// indicate where the restart point will be if the current call returns false.
        /// Data beyond this point must be rescanned after resumption, so move it to
        /// the front of the buffer rather than discarding it.
        /// </summary>
        public override bool fill_input_buffer()
        {
            int nbytes = m_infile.Read(m_buffer, 0, InputBufferSize);
            if (nbytes <= 0)
            {
                if (m_start_of_file) /* Treat empty input file as fatal error */
                    m_cinfo.ERREXIT(J_MESSAGE_CODE.JERR_INPUT_EMPTY);

                m_cinfo.WARNMS(J_MESSAGE_CODE.JWRN_JPEG_EOF);
                /* Insert a fake EOI marker */
                m_buffer[0] = 0xFF;
                m_buffer[1] = (byte)JPEG_MARKER.EOI;
                nbytes = 2;
            }

            initInternalBuffer(m_buffer, nbytes);
            m_start_of_file = false;

            return true;
        }
    }
}
