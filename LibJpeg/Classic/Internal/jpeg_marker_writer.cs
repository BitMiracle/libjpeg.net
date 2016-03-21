﻿/*
 * This file contains routines to write JPEG datastream markers.
 */

namespace BitMiracle.LibJpeg.Classic.Internal
{
    /// <summary>
    /// Marker writing
    /// </summary>
    class jpeg_marker_writer
    {
        private jpeg_compress_struct m_cinfo;
        private int m_last_restart_interval; /* last DRI value emitted; 0 after SOI */

        public jpeg_marker_writer(jpeg_compress_struct cinfo)
        {
            m_cinfo = cinfo;
        }

        /// <summary>
        /// Write datastream header.
        /// This consists of an SOI and optional APPn markers.
        /// We recommend use of the JFIF marker, but not the Adobe marker,
        /// when using YCbCr or grayscale data. The JFIF marker is also used
        /// for other standard JPEG colorspaces. The Adobe marker is helpful
        /// to distinguish RGB, CMYK, and YCCK colorspaces.
        /// Note that an application can write additional header markers after
        /// jpeg_start_compress returns.
        /// </summary>
        public void write_file_header()
        {
            emit_marker(JPEG_MARKER.SOI);  /* first the SOI */

            /* SOI is defined to reset restart interval to 0 */
            m_last_restart_interval = 0;

            if (m_cinfo.m_write_JFIF_header)   /* next an optional JFIF APP0 */
                emit_jfif_app0();
            if (m_cinfo.m_write_Adobe_marker) /* next an optional Adobe APP14 */
                emit_adobe_app14();
        }

        /// <summary>
        /// Write frame header.
        /// This consists of DQT and SOFn markers,
        /// a conditional LSE marker and a conditional pseudo SOS marker.
        /// Note that we do not emit the SOF until we have emitted the DQT(s).
        /// This avoids compatibility problems with incorrect implementations that
        /// try to error-check the quant table numbers as soon as they see the SOF.
        /// </summary>
        public void write_frame_header()
        {
            /* Emit DQT for each quantization table.
             * Note that emit_dqt() suppresses any duplicate tables.
             */
            int prec = 0;
            for (int ci = 0; ci < m_cinfo.m_num_components; ci++)
                prec += emit_dqt(m_cinfo.Component_info[ci].Quant_tbl_no);

            /* now prec is nonzero iff there are any 16-bit quant tables. */

            /* Check for a non-baseline specification.
             * Note we assume that Huffman table numbers won't be changed later.
             */
            bool is_baseline;
            if (m_cinfo.arith_code || m_cinfo.m_progressive_mode ||
                m_cinfo.m_data_precision != 8 || m_cinfo.block_size != JpegConstants.DCTSIZE)
            {
                is_baseline = false;
            }
            else
            {
                is_baseline = true;
                for (int ci = 0; ci < m_cinfo.m_num_components; ci++)
                {
                    if (m_cinfo.Component_info[ci].Dc_tbl_no > 1 || m_cinfo.Component_info[ci].Ac_tbl_no > 1)
                        is_baseline = false;
                }

                if (prec != 0 && is_baseline)
                {
                    is_baseline = false;
                    /* If it's baseline except for quantizer size, warn the user */
                    m_cinfo.TRACEMS(0, J_MESSAGE_CODE.JTRC_16BIT_TABLES);
                }
            }

            /* Emit the proper SOF marker */
            if (m_cinfo.arith_code)
            {
                if (m_cinfo.m_progressive_mode)
                    emit_sof(JPEG_MARKER.SOF10); /* SOF code for progressive arithmetic */
                else
                    emit_sof(JPEG_MARKER.SOF9);  /* SOF code for sequential arithmetic */
            }
            else
            {
                if (m_cinfo.m_progressive_mode)
                    emit_sof(JPEG_MARKER.SOF2);    /* SOF code for progressive Huffman */
                else if (is_baseline)
                    emit_sof(JPEG_MARKER.SOF0);    /* SOF code for baseline implementation */
                else
                    emit_sof(JPEG_MARKER.SOF1);    /* SOF code for non-baseline Huffman file */
            }

            /* Check to emit LSE inverse color transform specification marker */
            if (m_cinfo.color_transform != J_COLOR_TRANSFORM.JCT_NONE)
                emit_lse_ict();

            /* Check to emit pseudo SOS marker */
            if (m_cinfo.m_progressive_mode && m_cinfo.block_size != JpegConstants.DCTSIZE)
                emit_pseudo_sos();
        }

        /// <summary>
        /// Write scan header.
        /// This consists of DHT or DAC markers, optional DRI, and SOS.
        /// Compressed data will be written following the SOS.
        /// </summary>
        public void write_scan_header()
        {
            if (m_cinfo.arith_code)
            {
                /* Emit arith conditioning info.  We may have some duplication
                 * if the file has multiple scans, but it's so small it's hardly
                 * worth worrying about.
                 */
                emit_dac();
            }
            else
            {
                /* Emit Huffman tables.
                 * Note that emit_dht() suppresses any duplicate tables.
                 */
                for (int i = 0; i < m_cinfo.m_comps_in_scan; i++)
                {
                    jpeg_component_info compptr = m_cinfo.Component_info[m_cinfo.m_cur_comp_info[i]];

                    /* DC needs no table for refinement scan */
                    if (m_cinfo.m_Ss == 0 && m_cinfo.m_Ah == 0)
                        emit_dht(compptr.Dc_tbl_no, false);

                    /* AC needs no table when not present */
                    if (m_cinfo.m_Se != 0)
                        emit_dht(compptr.Ac_tbl_no, true);
                }

                /* Emit DRI if required --- note that DRI value could change for each scan.
                 * We avoid wasting space with unnecessary DRIs, however.
                 */
                if (m_cinfo.m_restart_interval != m_last_restart_interval)
                {
                    emit_dri();
                    m_last_restart_interval = m_cinfo.m_restart_interval;
                }

                emit_sos();
            }
        }

        /// <summary>
        /// Write datastream trailer.
        /// </summary>
        public void write_file_trailer()
        {
            emit_marker(JPEG_MARKER.EOI);
        }

        /// <summary>
        /// Write an abbreviated table-specification datastream.
        /// This consists of SOI, DQT and DHT tables, and EOI.
        /// Any table that is defined and not marked sent_table = true will be
        /// emitted.  Note that all tables will be marked sent_table = true at exit.
        /// </summary>
        public void write_tables_only()
        {
            emit_marker(JPEG_MARKER.SOI);

            for (int i = 0; i < JpegConstants.NUM_QUANT_TBLS; i++)
            {
                if (m_cinfo.m_quant_tbl_ptrs[i] != null)
                    emit_dqt(i);
            }

            for (int i = 0; i < JpegConstants.NUM_HUFF_TBLS; i++)
            {
                if (m_cinfo.m_dc_huff_tbl_ptrs[i] != null)
                    emit_dht(i, false);
                if (m_cinfo.m_ac_huff_tbl_ptrs[i] != null)
                    emit_dht(i, true);
            }

            emit_marker(JPEG_MARKER.EOI);
        }

        //////////////////////////////////////////////////////////////////////////
        // These routines allow writing an arbitrary marker with parameters.
        // The only intended use is to emit COM or APPn markers after calling
        // write_file_header and before calling write_frame_header.
        // Other uses are not guaranteed to produce desirable results.
        // Counting the parameter bytes properly is the caller's responsibility.

        /// <summary>
        /// Emit an arbitrary marker header
        /// </summary>
        public void write_marker_header(int marker, int datalen)
        {
            if (datalen > 65533)     /* safety check */
                m_cinfo.ERREXIT(J_MESSAGE_CODE.JERR_BAD_LENGTH);

            emit_marker((JPEG_MARKER) marker);

            emit_2bytes(datalen + 2);    /* total length */
        }

        /// <summary>
        /// Emit one byte of marker parameters following write_marker_header
        /// </summary>
        public void write_marker_byte(byte val)
        {
            emit_byte(val);
        }

        //////////////////////////////////////////////////////////////////////////
        // Routines to write specific marker types.
        //

        /// <summary>
        /// Emit a SOS marker
        /// </summary>
        private void emit_sos()
        {
            emit_marker(JPEG_MARKER.SOS);

            emit_2bytes(2 * m_cinfo.m_comps_in_scan + 2 + 1 + 3); /* length */

            emit_byte(m_cinfo.m_comps_in_scan);

            for (int i = 0; i < m_cinfo.m_comps_in_scan; i++)
            {
                int componentIndex = m_cinfo.m_cur_comp_info[i];
                jpeg_component_info compptr = m_cinfo.Component_info[componentIndex];
                emit_byte(compptr.Component_id);

                /* We emit 0 for unused field(s); this is recommended by the P&M text
                 * but does not seem to be specified in the standard.
                 */

                /* DC needs no table for refinement scan */
                int td = (m_cinfo.m_Ss == 0 && m_cinfo.m_Ah == 0) ? compptr.Dc_tbl_no : 0;

                /* AC needs no table when not present */
                int ta = (m_cinfo.m_Se != 0) ? compptr.Ac_tbl_no : 0;

                emit_byte((td << 4) + ta);
            }

            emit_byte(m_cinfo.m_Ss);
            emit_byte(m_cinfo.m_Se);
            emit_byte((m_cinfo.m_Ah << 4) + m_cinfo.m_Al);
        }

        /// <summary>
        /// Emit an LSE inverse color transform specification marker
        /// </summary>
        private void emit_lse_ict()
        {
            /* Support only 1 transform */
            if (m_cinfo.color_transform != J_COLOR_TRANSFORM.JCT_SUBTRACT_GREEN || m_cinfo.Num_components < 3)
                m_cinfo.ERREXIT(J_MESSAGE_CODE.JERR_CONVERSION_NOTIMPL);

            emit_marker(JPEG_MARKER.JPG8);

            emit_2bytes(24); /* fixed length */

            emit_byte(0x0D); /* ID inverse transform specification */
            emit_2bytes(JpegConstants.MAXJSAMPLE); /* MAXTRANS */
            emit_byte(3);        /* Nt=3 */
            emit_byte(m_cinfo.Component_info[1].Component_id);
            emit_byte(m_cinfo.Component_info[0].Component_id);
            emit_byte(m_cinfo.Component_info[2].Component_id);
            emit_byte(0x80); /* F1: CENTER1=1, NORM1=0 */
            emit_2bytes(0);  /* A(1,1)=0 */
            emit_2bytes(0);  /* A(1,2)=0 */
            emit_byte(0);    /* F2: CENTER2=0, NORM2=0 */
            emit_2bytes(1);  /* A(2,1)=1 */
            emit_2bytes(0);  /* A(2,2)=0 */
            emit_byte(0);    /* F3: CENTER3=0, NORM3=0 */
            emit_2bytes(1);  /* A(3,1)=1 */
            emit_2bytes(0);  /* A(3,2)=0 */
        }

        /// <summary>
        /// Emit a SOF marker
        /// </summary>
        private void emit_sof(JPEG_MARKER code)
        {
            emit_marker(code);

            emit_2bytes(3 * m_cinfo.m_num_components + 2 + 5 + 1); /* length */

            /* Make sure image isn't bigger than SOF field can handle */
            if (m_cinfo.jpeg_height > 65535 || m_cinfo.jpeg_width > 65535)
                m_cinfo.ERREXIT(J_MESSAGE_CODE.JERR_IMAGE_TOO_BIG, 65535);

            emit_byte(m_cinfo.m_data_precision);
            emit_2bytes(m_cinfo.jpeg_height);
            emit_2bytes(m_cinfo.jpeg_width);

            emit_byte(m_cinfo.m_num_components);

            for (int ci = 0; ci < m_cinfo.m_num_components; ci++)
            {
                jpeg_component_info componentInfo = m_cinfo.Component_info[ci];
                emit_byte(componentInfo.Component_id);
                emit_byte((componentInfo.H_samp_factor << 4) + componentInfo.V_samp_factor);
                emit_byte(componentInfo.Quant_tbl_no);
            }
        }

        /// <summary>
        /// Emit an Adobe APP14 marker
        /// </summary>
        private void emit_adobe_app14()
        {
            /*
             * Length of APP14 block    (2 bytes)
             * Block ID         (5 bytes - ASCII "Adobe")
             * Version Number       (2 bytes - currently 100)
             * Flags0           (2 bytes - currently 0)
             * Flags1           (2 bytes - currently 0)
             * Color transform      (1 byte)
             *
             * Although Adobe TN 5116 mentions Version = 101, all the Adobe files
             * now in circulation seem to use Version = 100, so that's what we write.
             *
             * We write the color transform byte as 1 if the JPEG color space is
             * YCbCr, 2 if it's YCCK, 0 otherwise.  Adobe's definition has to do with
             * whether the encoder performed a transformation, which is pretty useless.
             */

            emit_marker(JPEG_MARKER.APP14);

            emit_2bytes(2 + 5 + 2 + 2 + 2 + 1); /* length */

            emit_byte(0x41); /* Identifier: ASCII "Adobe" */
            emit_byte(0x64);
            emit_byte(0x6F);
            emit_byte(0x62);
            emit_byte(0x65);
            emit_2bytes(100);    /* Version */
            emit_2bytes(0);  /* Flags0 */
            emit_2bytes(0);  /* Flags1 */
            switch (m_cinfo.m_jpeg_color_space)
            {
                case J_COLOR_SPACE.JCS_YCbCr:
                    emit_byte(1);    /* Color transform = 1 */
                    break;
                case J_COLOR_SPACE.JCS_YCCK:
                    emit_byte(2);    /* Color transform = 2 */
                    break;
                default:
                    emit_byte(0);    /* Color transform = 0 */
                    break;
            }
        }

        /// <summary>
        /// Emit a DRI marker
        /// </summary>
        private void emit_dri()
        {
            emit_marker(JPEG_MARKER.DRI);

            emit_2bytes(4);  /* fixed length */

            emit_2bytes(m_cinfo.m_restart_interval);
        }
        
        /// <summary>
        /// Emit a DHT marker
        /// </summary>
        private void emit_dht(int index, bool is_ac)
        {
            JHUFF_TBL htbl = m_cinfo.m_dc_huff_tbl_ptrs[index];
            if (is_ac)
            {
                htbl = m_cinfo.m_ac_huff_tbl_ptrs[index];
                index += 0x10; /* output index has AC bit set */
            }

            if (htbl == null)
                m_cinfo.ERREXIT(J_MESSAGE_CODE.JERR_NO_HUFF_TABLE, index);

            if (!htbl.Sent_table)
            {
                emit_marker(JPEG_MARKER.DHT);

                int length = 0;
                for (int i = 1; i <= 16; i++)
                    length += htbl.Bits[i];

                emit_2bytes(length + 2 + 1 + 16);
                emit_byte(index);

                for (int i = 1; i <= 16; i++)
                    emit_byte(htbl.Bits[i]);

                for (int i = 0; i < length; i++)
                    emit_byte(htbl.Huffval[i]);

                htbl.Sent_table = true;
            }
        }
        
        /// <summary>
        /// Emit a DQT marker
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>the precision used (0 = 8bits, 1 = 16bits) for baseline checking</returns>
        private int emit_dqt(int index)
        {
            JQUANT_TBL qtbl = m_cinfo.m_quant_tbl_ptrs[index];
            if (qtbl == null)
                m_cinfo.ERREXIT(J_MESSAGE_CODE.JERR_NO_QUANT_TABLE, index);

            int prec = 0;
            for (int i = 0; i <= m_cinfo.lim_Se; i++)
            {
                if (qtbl.quantval[m_cinfo.natural_order[i]] > 255)
                    prec = 1;
            }

            if (!qtbl.Sent_table)
            {
                emit_marker(JPEG_MARKER.DQT);

                emit_2bytes(prec != 0 ?
                    m_cinfo.lim_Se * 2 + 2 + 1 + 2 : m_cinfo.lim_Se + 1 + 1 + 2);

                emit_byte(index + (prec << 4));

                for (int i = 0; i <= m_cinfo.lim_Se; i++)
                {
                    /* The table entries must be emitted in zigzag order. */
                    int qval = qtbl.quantval[m_cinfo.natural_order[i]];

                    if (prec != 0)
                        emit_byte(qval >> 8);

                    emit_byte(qval & 0xFF);
                }

                qtbl.Sent_table = true;
            }

            return prec;
        }

        /* Emit a DAC marker */
        /* Since the useful info is so small, we want to emit all the tables in */
        /* one DAC marker.  Therefore this routine does its own scan of the table. */
        private void emit_dac()
        {
            byte[] dc_in_use = new byte[JpegConstants.NUM_ARITH_TBLS];
            byte[] ac_in_use = new byte[JpegConstants.NUM_ARITH_TBLS];

            for (int i = 0; i < m_cinfo.m_comps_in_scan; i++)
            {
                jpeg_component_info compptr = m_cinfo.Component_info[m_cinfo.m_cur_comp_info[i]];
                /* DC needs no table for refinement scan */
                if (m_cinfo.m_Ss == 0 && m_cinfo.m_Ah == 0)
                    dc_in_use[compptr.Dc_tbl_no] = 1;

                /* AC needs no table when not present */
                if (m_cinfo.m_Se != 0)
                    ac_in_use[compptr.Ac_tbl_no] = 1;
            }

            int length = 0;
            for (int i = 0; i < JpegConstants.NUM_ARITH_TBLS; i++)
                length += dc_in_use[i] + ac_in_use[i];

            if (length != 0)
            {
                emit_marker(JPEG_MARKER.DAC);

                emit_2bytes(length * 2 + 2);

                for (int i = 0; i < JpegConstants.NUM_ARITH_TBLS; i++)
                {
                    if (dc_in_use[i] != 0)
                    {
                        emit_byte(i);
                        emit_byte(m_cinfo.arith_dc_L[i] + (m_cinfo.arith_dc_U[i] << 4));
                    }
                    if (ac_in_use[i] != 0)
                    {
                        emit_byte(i + 0x10);
                        emit_byte(m_cinfo.arith_ac_K[i]);
                    }
                }
            }
        }

        /// <summary>
        /// Emit a pseudo SOS marker
        /// </summary>
        private void emit_pseudo_sos()
        {
            emit_marker(JPEG_MARKER.SOS);

            emit_2bytes(2 + 1 + 3); /* length */

            emit_byte(0); /* Ns */

            emit_byte(0); /* Ss */
            emit_byte(m_cinfo.block_size * m_cinfo.block_size - 1); /* Se */
            emit_byte(0); /* Ah/Al */
        }

        /// <summary>
        /// Emit a JFIF-compliant APP0 marker
        /// </summary>
        private void emit_jfif_app0()
        {
            /*
             * Length of APP0 block (2 bytes)
             * Block ID         (4 bytes - ASCII "JFIF")
             * Zero byte            (1 byte to terminate the ID string)
             * Version Major, Minor (2 bytes - major first)
             * Units            (1 byte - 0x00 = none, 0x01 = inch, 0x02 = cm)
             * Xdpu         (2 bytes - dots per unit horizontal)
             * Ydpu         (2 bytes - dots per unit vertical)
             * Thumbnail X size     (1 byte)
             * Thumbnail Y size     (1 byte)
             */

            emit_marker(JPEG_MARKER.APP0);

            emit_2bytes(2 + 4 + 1 + 2 + 1 + 2 + 2 + 1 + 1); /* length */

            emit_byte(0x4A); /* Identifier: ASCII "JFIF" */
            emit_byte(0x46);
            emit_byte(0x49);
            emit_byte(0x46);
            emit_byte(0);
            emit_byte(m_cinfo.m_JFIF_major_version); /* Version fields */
            emit_byte(m_cinfo.m_JFIF_minor_version);
            emit_byte((int)m_cinfo.m_density_unit); /* Pixel size information */
            emit_2bytes(m_cinfo.m_X_density);
            emit_2bytes(m_cinfo.m_Y_density);
            emit_byte(0);        /* No thumbnail image */
            emit_byte(0);
        }

        //////////////////////////////////////////////////////////////////////////
        // Basic output routines.
        // 
        // Note that we do not support suspension while writing a marker.
        // Therefore, an application using suspension must ensure that there is
        // enough buffer space for the initial markers (typ. 600-700 bytes) before
        // calling jpeg_start_compress, and enough space to write the trailing EOI
        // (a few bytes) before calling jpeg_finish_compress.  Multipass compression
        // modes are not supported at all with suspension, so those two are the only
        // points where markers will be written.


        /// <summary>
        /// Emit a marker code
        /// </summary>
        private void emit_marker(JPEG_MARKER mark)
        {
            emit_byte(0xFF);
            emit_byte((int)mark);
        }

        /// <summary>
        /// Emit a 2-byte integer; these are always MSB first in JPEG files
        /// </summary>
        private void emit_2bytes(int value)
        {
            emit_byte((value >> 8) & 0xFF);
            emit_byte(value & 0xFF);
        }

        /// <summary>
        /// Emit a byte
        /// </summary>
        private void emit_byte(int val)
        {
            if (!m_cinfo.m_dest.emit_byte(val))
                m_cinfo.ERREXIT(J_MESSAGE_CODE.JERR_CANT_SUSPEND);
        }
    }
}
