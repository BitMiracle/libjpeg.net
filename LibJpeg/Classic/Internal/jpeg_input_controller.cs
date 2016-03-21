﻿/*
 * This file contains input control logic for the JPEG decompressor.
 * These routines are concerned with controlling the decompressor's input
 * processing (marker reading and coefficient decoding).
 */

using System;

namespace BitMiracle.LibJpeg.Classic.Internal
{
    /// <summary>
    /// Input control module
    /// </summary>
    class jpeg_input_controller
    {
        private jpeg_decompress_struct m_cinfo;
        private bool m_consumeData;
        private int m_inheaders;     /* Nonzero until first SOS is reached */
        private bool m_has_multiple_scans;    /* True if file has multiple scans */
        private bool m_eoi_reached;       /* True when EOI has been consumed */

        /// <summary>
        /// Initialize the input controller module.
        /// This is called only once, when the decompression object is created.
        /// </summary>
        public jpeg_input_controller(jpeg_decompress_struct cinfo)
        {
            m_cinfo = cinfo;

            /* Initialize state: can't use reset_input_controller since we don't
            * want to try to reset other modules yet.
            */
            m_inheaders = 1;
        }

        public ReadResult consume_input()
        {
            if (m_consumeData)
                return m_cinfo.m_coef.consume_data();

            return consume_markers();
        }

        /// <summary>
        /// Reset state to begin a fresh datastream.
        /// </summary>
        public void reset_input_controller()
        {
            m_consumeData = false;
            m_has_multiple_scans = false; /* "unknown" would be better */
            m_eoi_reached = false;
            m_inheaders = 1;

            /* Reset other modules */
            m_cinfo.m_err.reset_error_mgr();
            m_cinfo.m_marker.reset_marker_reader();

            /* Reset progression state -- would be cleaner if entropy decoder did this */
            m_cinfo.m_coef_bits = null;
        }

        /// <summary>
        /// Initialize the input modules to read a scan of compressed data.
        /// The first call to this is done after initializing
        /// the entire decompressor (during jpeg_start_decompress).
        /// Subsequent calls come from consume_markers, below.
        /// </summary>
        public void start_input_pass()
        {
            per_scan_setup();
            latch_quant_tables();
            m_cinfo.m_entropy.start_pass();
            m_cinfo.m_coef.start_input_pass();
            m_consumeData = true;
        }

        /// <summary>
        /// Finish up after inputting a compressed-data scan.
        /// This is called by the coefficient controller after it's read all
        /// the expected data of the scan.
        /// </summary>
        public void finish_input_pass()
        {
            m_cinfo.m_entropy.finish_pass();
            m_consumeData = false;
        }

        public bool HasMultipleScans()
        {
            return m_has_multiple_scans;
        }

        public bool EOIReached()
        {
            return m_eoi_reached;
        }

        /*
         * Compute output image dimensions and related values.
         * NOTE: this is exported for possible use by application.
         * Hence it mustn't do anything that can't be done twice.
         */
        /* Do computations that are needed before master selection phase.
        * This function is used for transcoding and full decompression.
        */
        public void jpeg_core_output_dimensions()
        {
            /* Compute actual output image dimensions and DCT scaling choices. */
            if (m_cinfo.m_scale_num * m_cinfo.block_size <= m_cinfo.m_scale_denom)
            {
                /* Provide 1/block_size scaling */
                m_cinfo.m_output_width = (int)
                  JpegUtils.jdiv_round_up((long)m_cinfo.m_image_width, (long)m_cinfo.block_size);
                m_cinfo.m_output_height = (int)
                  JpegUtils.jdiv_round_up((long)m_cinfo.m_image_height, (long)m_cinfo.block_size);
                m_cinfo.min_DCT_h_scaled_size = 1;
                m_cinfo.min_DCT_v_scaled_size = 1;
            }
            else if (m_cinfo.m_scale_num * m_cinfo.block_size <= m_cinfo.m_scale_denom * 2)
            {
                /* Provide 2/block_size scaling */
                m_cinfo.m_output_width = (int)
                  JpegUtils.jdiv_round_up((long)m_cinfo.m_image_width * 2L, (long)m_cinfo.block_size);
                m_cinfo.m_output_height = (int)
                  JpegUtils.jdiv_round_up((long)m_cinfo.m_image_height * 2L, (long)m_cinfo.block_size);
                m_cinfo.min_DCT_h_scaled_size = 2;
                m_cinfo.min_DCT_v_scaled_size = 2;
            }
            else if (m_cinfo.m_scale_num * m_cinfo.block_size <= m_cinfo.m_scale_denom * 3)
            {
                /* Provide 3/block_size scaling */
                m_cinfo.m_output_width = (int)
                  JpegUtils.jdiv_round_up((long)m_cinfo.m_image_width * 3L, (long)m_cinfo.block_size);
                m_cinfo.m_output_height = (int)
                  JpegUtils.jdiv_round_up((long)m_cinfo.m_image_height * 3L, (long)m_cinfo.block_size);
                m_cinfo.min_DCT_h_scaled_size = 3;
                m_cinfo.min_DCT_v_scaled_size = 3;
            }
            else if (m_cinfo.m_scale_num * m_cinfo.block_size <= m_cinfo.m_scale_denom * 4)
            {
                /* Provide 4/block_size scaling */
                m_cinfo.m_output_width = (int)
                  JpegUtils.jdiv_round_up((long)m_cinfo.m_image_width * 4L, (long)m_cinfo.block_size);
                m_cinfo.m_output_height = (int)
                  JpegUtils.jdiv_round_up((long)m_cinfo.m_image_height * 4L, (long)m_cinfo.block_size);
                m_cinfo.min_DCT_h_scaled_size = 4;
                m_cinfo.min_DCT_v_scaled_size = 4;
            }
            else if (m_cinfo.m_scale_num * m_cinfo.block_size <= m_cinfo.m_scale_denom * 5)
            {
                /* Provide 5/block_size scaling */
                m_cinfo.m_output_width = (int)
                  JpegUtils.jdiv_round_up((long)m_cinfo.m_image_width * 5L, (long)m_cinfo.block_size);
                m_cinfo.m_output_height = (int)
                  JpegUtils.jdiv_round_up((long)m_cinfo.m_image_height * 5L, (long)m_cinfo.block_size);
                m_cinfo.min_DCT_h_scaled_size = 5;
                m_cinfo.min_DCT_v_scaled_size = 5;
            }
            else if (m_cinfo.m_scale_num * m_cinfo.block_size <= m_cinfo.m_scale_denom * 6)
            {
                /* Provide 6/block_size scaling */
                m_cinfo.m_output_width = (int)
                  JpegUtils.jdiv_round_up((long)m_cinfo.m_image_width * 6L, (long)m_cinfo.block_size);
                m_cinfo.m_output_height = (int)
                  JpegUtils.jdiv_round_up((long)m_cinfo.m_image_height * 6L, (long)m_cinfo.block_size);
                m_cinfo.min_DCT_h_scaled_size = 6;
                m_cinfo.min_DCT_v_scaled_size = 6;
            }
            else if (m_cinfo.m_scale_num * m_cinfo.block_size <= m_cinfo.m_scale_denom * 7)
            {
                /* Provide 7/block_size scaling */
                m_cinfo.m_output_width = (int)
                  JpegUtils.jdiv_round_up((long)m_cinfo.m_image_width * 7L, (long)m_cinfo.block_size);
                m_cinfo.m_output_height = (int)
                  JpegUtils.jdiv_round_up((long)m_cinfo.m_image_height * 7L, (long)m_cinfo.block_size);
                m_cinfo.min_DCT_h_scaled_size = 7;
                m_cinfo.min_DCT_v_scaled_size = 7;
            }
            else if (m_cinfo.m_scale_num * m_cinfo.block_size <= m_cinfo.m_scale_denom * 8)
            {
                /* Provide 8/block_size scaling */
                m_cinfo.m_output_width = (int)
                  JpegUtils.jdiv_round_up((long)m_cinfo.m_image_width * 8L, (long)m_cinfo.block_size);
                m_cinfo.m_output_height = (int)
                  JpegUtils.jdiv_round_up((long)m_cinfo.m_image_height * 8L, (long)m_cinfo.block_size);
                m_cinfo.min_DCT_h_scaled_size = 8;
                m_cinfo.min_DCT_v_scaled_size = 8;
            }
            else if (m_cinfo.m_scale_num * m_cinfo.block_size <= m_cinfo.m_scale_denom * 9)
            {
                /* Provide 9/block_size scaling */
                m_cinfo.m_output_width = (int)
                  JpegUtils.jdiv_round_up((long)m_cinfo.m_image_width * 9L, (long)m_cinfo.block_size);
                m_cinfo.m_output_height = (int)
                  JpegUtils.jdiv_round_up((long)m_cinfo.m_image_height * 9L, (long)m_cinfo.block_size);
                m_cinfo.min_DCT_h_scaled_size = 9;
                m_cinfo.min_DCT_v_scaled_size = 9;
            }
            else if (m_cinfo.m_scale_num * m_cinfo.block_size <= m_cinfo.m_scale_denom * 10)
            {
                /* Provide 10/block_size scaling */
                m_cinfo.m_output_width = (int)
                  JpegUtils.jdiv_round_up((long)m_cinfo.m_image_width * 10L, (long)m_cinfo.block_size);
                m_cinfo.m_output_height = (int)
                  JpegUtils.jdiv_round_up((long)m_cinfo.m_image_height * 10L, (long)m_cinfo.block_size);
                m_cinfo.min_DCT_h_scaled_size = 10;
                m_cinfo.min_DCT_v_scaled_size = 10;
            }
            else if (m_cinfo.m_scale_num * m_cinfo.block_size <= m_cinfo.m_scale_denom * 11)
            {
                /* Provide 11/block_size scaling */
                m_cinfo.m_output_width = (int)
                  JpegUtils.jdiv_round_up((long)m_cinfo.m_image_width * 11L, (long)m_cinfo.block_size);
                m_cinfo.m_output_height = (int)
                  JpegUtils.jdiv_round_up((long)m_cinfo.m_image_height * 11L, (long)m_cinfo.block_size);
                m_cinfo.min_DCT_h_scaled_size = 11;
                m_cinfo.min_DCT_v_scaled_size = 11;
            }
            else if (m_cinfo.m_scale_num * m_cinfo.block_size <= m_cinfo.m_scale_denom * 12)
            {
                /* Provide 12/block_size scaling */
                m_cinfo.m_output_width = (int)
                  JpegUtils.jdiv_round_up((long)m_cinfo.m_image_width * 12L, (long)m_cinfo.block_size);
                m_cinfo.m_output_height = (int)
                  JpegUtils.jdiv_round_up((long)m_cinfo.m_image_height * 12L, (long)m_cinfo.block_size);
                m_cinfo.min_DCT_h_scaled_size = 12;
                m_cinfo.min_DCT_v_scaled_size = 12;
            }
            else if (m_cinfo.m_scale_num * m_cinfo.block_size <= m_cinfo.m_scale_denom * 13)
            {
                /* Provide 13/block_size scaling */
                m_cinfo.m_output_width = (int)
                  JpegUtils.jdiv_round_up((long)m_cinfo.m_image_width * 13L, (long)m_cinfo.block_size);
                m_cinfo.m_output_height = (int)
                  JpegUtils.jdiv_round_up((long)m_cinfo.m_image_height * 13L, (long)m_cinfo.block_size);
                m_cinfo.min_DCT_h_scaled_size = 13;
                m_cinfo.min_DCT_v_scaled_size = 13;
            }
            else if (m_cinfo.m_scale_num * m_cinfo.block_size <= m_cinfo.m_scale_denom * 14)
            {
                /* Provide 14/block_size scaling */
                m_cinfo.m_output_width = (int)
                  JpegUtils.jdiv_round_up((long)m_cinfo.m_image_width * 14L, (long)m_cinfo.block_size);
                m_cinfo.m_output_height = (int)
                  JpegUtils.jdiv_round_up((long)m_cinfo.m_image_height * 14L, (long)m_cinfo.block_size);
                m_cinfo.min_DCT_h_scaled_size = 14;
                m_cinfo.min_DCT_v_scaled_size = 14;
            }
            else if (m_cinfo.m_scale_num * m_cinfo.block_size <= m_cinfo.m_scale_denom * 15)
            {
                /* Provide 15/block_size scaling */
                m_cinfo.m_output_width = (int)
                  JpegUtils.jdiv_round_up((long)m_cinfo.m_image_width * 15L, (long)m_cinfo.block_size);
                m_cinfo.m_output_height = (int)
                  JpegUtils.jdiv_round_up((long)m_cinfo.m_image_height * 15L, (long)m_cinfo.block_size);
                m_cinfo.min_DCT_h_scaled_size = 15;
                m_cinfo.min_DCT_v_scaled_size = 15;
            }
            else {
                /* Provide 16/block_size scaling */
                m_cinfo.m_output_width = (int)
                  JpegUtils.jdiv_round_up((long)m_cinfo.m_image_width * 16L, (long)m_cinfo.block_size);
                m_cinfo.m_output_height = (int)
                  JpegUtils.jdiv_round_up((long)m_cinfo.m_image_height * 16L, (long)m_cinfo.block_size);
                m_cinfo.min_DCT_h_scaled_size = 16;
                m_cinfo.min_DCT_v_scaled_size = 16;
            }

            /* Recompute dimensions of components */
            for (int ci = 0; ci < m_cinfo.m_num_components; ci++)
            {
                jpeg_component_info compptr = m_cinfo.Comp_info[ci];
                compptr.DCT_h_scaled_size = m_cinfo.min_DCT_h_scaled_size;
                compptr.DCT_v_scaled_size = m_cinfo.min_DCT_v_scaled_size;
            }
        }


        /// <summary>
        /// Read JPEG markers before, between, or after compressed-data scans.
        /// Change state as necessary when a new scan is reached.
        /// Return value is JPEG_SUSPENDED, JPEG_REACHED_SOS, or JPEG_REACHED_EOI.
        /// 
        /// The consume_input method pointer points either here or to the
        /// coefficient controller's consume_data routine, depending on whether
        /// we are reading a compressed data segment or inter-segment markers.
        /// 
        /// Note: This function should NOT return a pseudo SOS marker(with zero
        /// component number) to the caller.A pseudo marker received by
        /// read_markers is processed and then skipped for other markers.
        /// </summary>
        private ReadResult consume_markers()
        {
            ReadResult val;

            if (m_eoi_reached) /* After hitting EOI, read no further */
                return ReadResult.JPEG_REACHED_EOI;

            /* Loop to pass pseudo SOS marker */
            for (;;)
            {
                val = m_cinfo.m_marker.read_markers();

                switch (val)
                {
                    case ReadResult.JPEG_REACHED_SOS:
                        /* Found SOS */
                        if (m_inheaders != 0)
                        {
                            /* 1st SOS */
                            if (m_inheaders == 1)
                                initial_setup();

                            if (m_cinfo.m_comps_in_scan == 0)
                            {
                                /* pseudo SOS marker */
                                m_inheaders = 2;
                                break;
                            }

                            m_inheaders = 0;

                            /* Note: start_input_pass must be called by jpeg_decomp_master
                             * before any more input can be consumed.
                             */
                        }
                        else
                        {
                            /* 2nd or later SOS marker */
                            if (!m_has_multiple_scans)
                            {
                                /* Oops, I wasn't expecting this! */
                                m_cinfo.ERREXIT(J_MESSAGE_CODE.JERR_EOI_EXPECTED);
                            }

                            if (m_cinfo.m_comps_in_scan == 0)
                            {
                                /* unexpected pseudo SOS marker */
                                break;
                            }

                            m_cinfo.m_inputctl.start_input_pass();
                        }
                        return val;

                    case ReadResult.JPEG_REACHED_EOI:
                        /* Found EOI */
                        m_eoi_reached = true;
                        if (m_inheaders != 0)
                        {
                            /* Tables-only datastream, apparently */
                            if (m_cinfo.m_marker.SawSOF())
                                m_cinfo.ERREXIT(J_MESSAGE_CODE.JERR_SOF_NO_SOS);
                        }
                        else
                        {
                            /* Prevent infinite loop in coef ctlr's decompress_data routine
                             * if user set output_scan_number larger than number of scans.
                             */
                            if (m_cinfo.m_output_scan_number > m_cinfo.m_input_scan_number)
                                m_cinfo.m_output_scan_number = m_cinfo.m_input_scan_number;
                        }

                        return val;

                    case ReadResult.JPEG_SUSPENDED:
                    default:
                        return val;
                }
            }
        }

        /// <summary>
        /// Routines to calculate various quantities related to the size of the image.
        /// Called once, when first SOS marker is reached
        /// </summary>
        private void initial_setup()
        {
            /* Make sure image isn't bigger than I can handle */
            if (m_cinfo.m_image_height > JpegConstants.JPEG_MAX_DIMENSION ||
                m_cinfo.m_image_width > JpegConstants.JPEG_MAX_DIMENSION)
            {
                m_cinfo.ERREXIT(J_MESSAGE_CODE.JERR_IMAGE_TOO_BIG, (int)JpegConstants.JPEG_MAX_DIMENSION);
            }

            /* Only 8 to 12 bits data precision are supported for DCT based JPEG */
            if (m_cinfo.m_data_precision < 8 || m_cinfo.m_data_precision > 12)
                m_cinfo.ERREXIT(J_MESSAGE_CODE.JERR_BAD_PRECISION, m_cinfo.m_data_precision);

            /* Check that number of components won't exceed internal array sizes */
            if (m_cinfo.m_num_components > JpegConstants.MAX_COMPONENTS)
                m_cinfo.ERREXIT(J_MESSAGE_CODE.JERR_COMPONENT_COUNT, m_cinfo.m_num_components, JpegConstants.MAX_COMPONENTS);

            /* Compute maximum sampling factors; check factor validity */
            m_cinfo.m_max_h_samp_factor = 1;
            m_cinfo.m_max_v_samp_factor = 1;

            for (int ci = 0; ci < m_cinfo.m_num_components; ci++)
            {
                if (m_cinfo.Comp_info[ci].H_samp_factor <= 0 || m_cinfo.Comp_info[ci].H_samp_factor > JpegConstants.MAX_SAMP_FACTOR ||
                    m_cinfo.Comp_info[ci].V_samp_factor <= 0 || m_cinfo.Comp_info[ci].V_samp_factor > JpegConstants.MAX_SAMP_FACTOR)
                {
                    m_cinfo.ERREXIT(J_MESSAGE_CODE.JERR_BAD_SAMPLING);
                }

                m_cinfo.m_max_h_samp_factor = Math.Max(m_cinfo.m_max_h_samp_factor, m_cinfo.Comp_info[ci].H_samp_factor);
                m_cinfo.m_max_v_samp_factor = Math.Max(m_cinfo.m_max_v_samp_factor, m_cinfo.Comp_info[ci].V_samp_factor);
            }

            /* Derive block_size, natural_order, and lim_Se */
            if (m_cinfo.is_baseline || (m_cinfo.m_progressive_mode && m_cinfo.m_comps_in_scan != 0))
            {
                /* no pseudo SOS marker */
                m_cinfo.block_size = JpegConstants.DCTSIZE;
                m_cinfo.natural_order = JpegUtils.jpeg_natural_order;
                m_cinfo.lim_Se = JpegConstants.DCTSIZE2 - 1;
            }
            else
            {
                switch (m_cinfo.m_Se)
                {
                    case (1 * 1 - 1):
                        m_cinfo.block_size = 1;
                        m_cinfo.natural_order = JpegUtils.jpeg_natural_order; /* not needed */
                        m_cinfo.lim_Se = m_cinfo.m_Se;
                        break;
                    case (2 * 2 - 1):
                        m_cinfo.block_size = 2;
                        m_cinfo.natural_order = JpegUtils.jpeg_natural_order2;
                        m_cinfo.lim_Se = m_cinfo.m_Se;
                        break;
                    case (3 * 3 - 1):
                        m_cinfo.block_size = 3;
                        m_cinfo.natural_order = JpegUtils.jpeg_natural_order3;
                        m_cinfo.lim_Se = m_cinfo.m_Se;
                        break;
                    case (4 * 4 - 1):
                        m_cinfo.block_size = 4;
                        m_cinfo.natural_order = JpegUtils.jpeg_natural_order4;
                        m_cinfo.lim_Se = m_cinfo.m_Se;
                        break;
                    case (5 * 5 - 1):
                        m_cinfo.block_size = 5;
                        m_cinfo.natural_order = JpegUtils.jpeg_natural_order5;
                        m_cinfo.lim_Se = m_cinfo.m_Se;
                        break;
                    case (6 * 6 - 1):
                        m_cinfo.block_size = 6;
                        m_cinfo.natural_order = JpegUtils.jpeg_natural_order6;
                        m_cinfo.lim_Se = m_cinfo.m_Se;
                        break;
                    case (7 * 7 - 1):
                        m_cinfo.block_size = 7;
                        m_cinfo.natural_order = JpegUtils.jpeg_natural_order7;
                        m_cinfo.lim_Se = m_cinfo.m_Se;
                        break;
                    case (8 * 8 - 1):
                        m_cinfo.block_size = 8;
                        m_cinfo.natural_order = JpegUtils.jpeg_natural_order;
                        m_cinfo.lim_Se = JpegConstants.DCTSIZE2 - 1;
                        break;
                    case (9 * 9 - 1):
                        m_cinfo.block_size = 9;
                        m_cinfo.natural_order = JpegUtils.jpeg_natural_order;
                        m_cinfo.lim_Se = JpegConstants.DCTSIZE2 - 1;
                        break;
                    case (10 * 10 - 1):
                        m_cinfo.block_size = 10;
                        m_cinfo.natural_order = JpegUtils.jpeg_natural_order;
                        m_cinfo.lim_Se = JpegConstants.DCTSIZE2 - 1;
                        break;
                    case (11 * 11 - 1):
                        m_cinfo.block_size = 11;
                        m_cinfo.natural_order = JpegUtils.jpeg_natural_order;
                        m_cinfo.lim_Se = JpegConstants.DCTSIZE2 - 1;
                        break;
                    case (12 * 12 - 1):
                        m_cinfo.block_size = 12;
                        m_cinfo.natural_order = JpegUtils.jpeg_natural_order;
                        m_cinfo.lim_Se = JpegConstants.DCTSIZE2 - 1;
                        break;
                    case (13 * 13 - 1):
                        m_cinfo.block_size = 13;
                        m_cinfo.natural_order = JpegUtils.jpeg_natural_order;
                        m_cinfo.lim_Se = JpegConstants.DCTSIZE2 - 1;
                        break;
                    case (14 * 14 - 1):
                        m_cinfo.block_size = 14;
                        m_cinfo.natural_order = JpegUtils.jpeg_natural_order;
                        m_cinfo.lim_Se = JpegConstants.DCTSIZE2 - 1;
                        break;
                    case (15 * 15 - 1):
                        m_cinfo.block_size = 15;
                        m_cinfo.natural_order = JpegUtils.jpeg_natural_order;
                        m_cinfo.lim_Se = JpegConstants.DCTSIZE2 - 1;
                        break;
                    case (16 * 16 - 1):
                        m_cinfo.block_size = 16;
                        m_cinfo.natural_order = JpegUtils.jpeg_natural_order;
                        m_cinfo.lim_Se = JpegConstants.DCTSIZE2 - 1;
                        break;
                    default:
                        m_cinfo.ERREXIT(J_MESSAGE_CODE.JERR_BAD_PROGRESSION,
                             m_cinfo.m_Ss, m_cinfo.m_Se, m_cinfo.m_Ah, m_cinfo.m_Al);
                        break;
                }
            }

            /* We initialize DCT_scaled_size and min_DCT_scaled_size to block_size.
             * In the full decompressor,
             * this will be overridden by jpeg_calc_output_dimensions in jdmaster.c;
             * but in the transcoder,
             * jpeg_calc_output_dimensions is not used, so we must do it here.
             */
            m_cinfo.min_DCT_h_scaled_size = m_cinfo.block_size;
            m_cinfo.min_DCT_v_scaled_size = m_cinfo.block_size;

            /* Compute dimensions of components */
            for (int ci = 0; ci < m_cinfo.m_num_components; ci++)
            {
                jpeg_component_info compptr = m_cinfo.Comp_info[ci];
                compptr.DCT_h_scaled_size = m_cinfo.block_size;
                compptr.DCT_v_scaled_size = m_cinfo.block_size;

                /* Size in DCT blocks */
                compptr.Width_in_blocks = (int)JpegUtils.jdiv_round_up(
                    m_cinfo.m_image_width * compptr.H_samp_factor,
                    m_cinfo.m_max_h_samp_factor * m_cinfo.block_size);

                compptr.height_in_blocks = (int)JpegUtils.jdiv_round_up(
                    m_cinfo.m_image_height * compptr.V_samp_factor,
                    m_cinfo.m_max_v_samp_factor * m_cinfo.block_size);

                /* downsampled_width and downsampled_height will also be overridden by
                 * jpeg_decomp_master if we are doing full decompression.  The transcoder library
                 * doesn't use these values, but the calling application might.
                 */
                /* Size in samples */
                compptr.downsampled_width = (int)JpegUtils.jdiv_round_up(
                    m_cinfo.m_image_width * compptr.H_samp_factor,
                    m_cinfo.m_max_h_samp_factor);

                compptr.downsampled_height = (int)JpegUtils.jdiv_round_up(
                    m_cinfo.m_image_height * compptr.V_samp_factor,
                    m_cinfo.m_max_v_samp_factor);

                /* Mark component needed, until color conversion says otherwise */
                compptr.component_needed = true;

                /* Mark no quantization table yet saved for component */
                compptr.quant_table = null;
            }

            /* Compute number of fully interleaved MCU rows. */
            m_cinfo.m_total_iMCU_rows = (int)JpegUtils.jdiv_round_up(
                m_cinfo.m_image_height, m_cinfo.m_max_v_samp_factor * m_cinfo.block_size);

            /* Decide whether file contains multiple scans */
            if (m_cinfo.m_comps_in_scan < m_cinfo.m_num_components || m_cinfo.m_progressive_mode)
                m_cinfo.m_inputctl.m_has_multiple_scans = true;
            else
                m_cinfo.m_inputctl.m_has_multiple_scans = false;
        }

        /// <summary>
        /// Save away a copy of the Q-table referenced by each component present
        /// in the current scan, unless already saved during a prior scan.
        /// 
        /// In a multiple-scan JPEG file, the encoder could assign different components
        /// the same Q-table slot number, but change table definitions between scans
        /// so that each component uses a different Q-table.  (The IJG encoder is not
        /// currently capable of doing this, but other encoders might.)  Since we want
        /// to be able to dequantize all the components at the end of the file, this
        /// means that we have to save away the table actually used for each component.
        /// We do this by copying the table at the start of the first scan containing
        /// the component.
        /// The JPEG spec prohibits the encoder from changing the contents of a Q-table
        /// slot between scans of a component using that slot.  If the encoder does so
        /// anyway, this decoder will simply use the Q-table values that were current
        /// at the start of the first scan for the component.
        /// 
        /// The decompressor output side looks only at the saved quant tables,
        /// not at the current Q-table slots.
        /// </summary>
        private void latch_quant_tables()
        {
            for (int ci = 0; ci < m_cinfo.m_comps_in_scan; ci++)
            {
                jpeg_component_info componentInfo = m_cinfo.Comp_info[m_cinfo.m_cur_comp_info[ci]];

                /* No work if we already saved Q-table for this component */
                if (componentInfo.quant_table != null)
                    continue;

                /* Make sure specified quantization table is present */
                int qtblno = componentInfo.Quant_tbl_no;
                if (qtblno < 0 || qtblno >= JpegConstants.NUM_QUANT_TBLS || m_cinfo.m_quant_tbl_ptrs[qtblno] == null)
                    m_cinfo.ERREXIT(J_MESSAGE_CODE.JERR_NO_QUANT_TABLE, qtblno);

                /* OK, save away the quantization table */
                JQUANT_TBL qtbl = new JQUANT_TBL();
                Buffer.BlockCopy(m_cinfo.m_quant_tbl_ptrs[qtblno].quantval, 0,
                    qtbl.quantval, 0, qtbl.quantval.Length * sizeof(short));
                qtbl.Sent_table = m_cinfo.m_quant_tbl_ptrs[qtblno].Sent_table;
                componentInfo.quant_table = qtbl;
                m_cinfo.Comp_info[m_cinfo.m_cur_comp_info[ci]] = componentInfo;
            }
        }

        /// <summary>
        /// Do computations that are needed before processing a JPEG scan
        /// cinfo.comps_in_scan and cinfo.cur_comp_info[] were set from SOS marker
        /// </summary>
        private void per_scan_setup()
        {
            if (m_cinfo.m_comps_in_scan == 1)
            {
                /* Noninterleaved (single-component) scan */
                jpeg_component_info componentInfo = m_cinfo.Comp_info[m_cinfo.m_cur_comp_info[0]];

                /* Overall image size in MCUs */
                m_cinfo.m_MCUs_per_row = componentInfo.Width_in_blocks;
                m_cinfo.m_MCU_rows_in_scan = componentInfo.height_in_blocks;

                /* For noninterleaved scan, always one block per MCU */
                componentInfo.MCU_width = 1;
                componentInfo.MCU_height = 1;
                componentInfo.MCU_blocks = 1;
                componentInfo.MCU_sample_width = componentInfo.DCT_h_scaled_size;
                componentInfo.last_col_width = 1;

                /* For noninterleaved scans, it is convenient to define last_row_height
                 * as the number of block rows present in the last iMCU row.
                 */
                int tmp = componentInfo.height_in_blocks % componentInfo.V_samp_factor;
                if (tmp == 0)
                    tmp = componentInfo.V_samp_factor;
                componentInfo.last_row_height = tmp;
                m_cinfo.Comp_info[m_cinfo.m_cur_comp_info[0]] = componentInfo;

                /* Prepare array describing MCU composition */
                m_cinfo.m_blocks_in_MCU = 1;
                m_cinfo.m_MCU_membership[0] = 0;
            }
            else
            {
                /* Interleaved (multi-component) scan */
                if (m_cinfo.m_comps_in_scan <= 0 || m_cinfo.m_comps_in_scan > JpegConstants.MAX_COMPS_IN_SCAN)
                    m_cinfo.ERREXIT(J_MESSAGE_CODE.JERR_COMPONENT_COUNT, m_cinfo.m_comps_in_scan, JpegConstants.MAX_COMPS_IN_SCAN);

                /* Overall image size in MCUs */
                m_cinfo.m_MCUs_per_row = (int)JpegUtils.jdiv_round_up(
                    m_cinfo.m_image_width, m_cinfo.m_max_h_samp_factor * m_cinfo.block_size);

                m_cinfo.m_MCU_rows_in_scan = (int)JpegUtils.jdiv_round_up(
                    m_cinfo.m_image_height, m_cinfo.m_max_v_samp_factor * m_cinfo.block_size);

                m_cinfo.m_blocks_in_MCU = 0;

                for (int ci = 0; ci < m_cinfo.m_comps_in_scan; ci++)
                {
                    jpeg_component_info componentInfo = m_cinfo.Comp_info[m_cinfo.m_cur_comp_info[ci]];

                    /* Sampling factors give # of blocks of component in each MCU */
                    componentInfo.MCU_width = componentInfo.H_samp_factor;
                    componentInfo.MCU_height = componentInfo.V_samp_factor;
                    componentInfo.MCU_blocks = componentInfo.MCU_width * componentInfo.MCU_height;
                    componentInfo.MCU_sample_width = componentInfo.MCU_width * componentInfo.DCT_h_scaled_size;

                    /* Figure number of non-dummy blocks in last MCU column & row */
                    int tmp = componentInfo.Width_in_blocks % componentInfo.MCU_width;
                    if (tmp == 0)
                        tmp = componentInfo.MCU_width;
                    componentInfo.last_col_width = tmp;

                    tmp = componentInfo.height_in_blocks % componentInfo.MCU_height;
                    if (tmp == 0)
                        tmp = componentInfo.MCU_height;
                    componentInfo.last_row_height = tmp;

                    /* Prepare array describing MCU composition */
                    int mcublks = componentInfo.MCU_blocks;
                    if (m_cinfo.m_blocks_in_MCU + mcublks > JpegConstants.D_MAX_BLOCKS_IN_MCU)
                        m_cinfo.ERREXIT(J_MESSAGE_CODE.JERR_BAD_MCU_SIZE);

                    m_cinfo.Comp_info[m_cinfo.m_cur_comp_info[ci]] = componentInfo;

                    while (mcublks-- > 0)
                        m_cinfo.m_MCU_membership[m_cinfo.m_blocks_in_MCU++] = ci;
                }
            }
        }
    }
}
