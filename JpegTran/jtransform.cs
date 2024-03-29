﻿using System;

using BitMiracle.LibJpeg.Classic;
using BitMiracle.LibJpeg.Classic.Internal;

using JDIMENSION = System.Int32;

namespace BitMiracle.JpegTran
{
    /// <summary>
    /// Image transformation routines and other utility code. These are NOT part of the core 
    /// JPEG library.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles")]
    class jtransform
    {
        private static readonly byte JPEG_APP0 = 0xE0; /* APP0 marker code */

        /* Parse a crop specification (written in X11 geometry style).
         * The routine returns true if the spec string is valid, false if not.
         *
         * The crop spec string should have the format
         *	<width>[{fr}]x<height>[{fr}]{+-}<xoffset>{+-}<yoffset>
         * where width, height, xoffset, and yoffset are unsigned integers.
         * Each of the elements can be omitted to indicate a default value.
         * (A weakness of this style is that it is not possible to omit xoffset
         * while specifying yoffset, since they look alike.)
         *
         * This code is loosely based on XParseGeometry from the X11 distribution.
         */
        public static bool parse_crop_spec(jpeg_transform_info info, string spec)
        {
            throw new NotImplementedException();
        }

        /* Request any required workspace.
         *
         * This routine figures out the size that the output image will be
         * (which implies that all the transform parameters must be set before
         * it is called).
         *
         * We allocate the workspace virtual arrays from the source decompression
         * object, so that all the arrays (both the original data and the workspace)
         * will be taken into account while making memory management decisions.
         * Hence, this routine must be called after jpeg_read_header (which reads
         * the image dimensions) and before jpeg_read_coefficients (which realizes
         * the source's virtual arrays).
         *
         * This function returns false right away if -perfect is given
         * and transformation is not perfect. Otherwise returns true.
         */
        public static bool request_workspace(jpeg_decompress_struct srcinfo, jpeg_transform_info info)
        {
            /* Determine number of components in output image */
            if (info.force_grayscale &&
                (srcinfo.Jpeg_color_space == J_COLOR_SPACE.JCS_YCbCr ||
                srcinfo.Jpeg_color_space == J_COLOR_SPACE.JCS_BG_YCC) &&
                srcinfo.Num_components == 3)
            {
                /* We'll only process the first component */
                info.num_components = 1;
            }
            else
            {
                /* Process all the components */
                info.num_components = srcinfo.Num_components;
            }

            /* Compute output image dimensions and related values. */
            srcinfo.m_inputctl.jpeg_core_output_dimensions();

            /* Return right away if -perfect is given and transformation is not perfect. */
            if (info.perfect)
            {
                if (info.num_components == 1)
                {
                    if (!perfect_transform(
                        srcinfo.Output_width,
                        srcinfo.Output_height,
                        srcinfo.min_DCT_h_scaled_size,
                        srcinfo.min_DCT_v_scaled_size,
                        info.transform))
                    {
                        return false;
                    }
                }
                else
                {
                    if (!perfect_transform(
                        srcinfo.Output_width,
                        srcinfo.Output_height,
                        srcinfo.m_max_h_samp_factor * srcinfo.min_DCT_h_scaled_size,
                        srcinfo.m_max_v_samp_factor * srcinfo.min_DCT_v_scaled_size,
                        info.transform))
                    {
                        return false;
                    }
                }
            }

            /* If there is only one output component, force the iMCU size to be 1;
             * else use the source iMCU size.  (This allows us to do the right thing
             * when reducing color to grayscale, and also provides a handy way of
             * cleaning up "funny" grayscale images whose sampling factors are not 1x1.)
             */
            switch (info.transform)
            {
                case JXFORM_CODE.JXFORM_TRANSPOSE:
                case JXFORM_CODE.JXFORM_TRANSVERSE:
                case JXFORM_CODE.JXFORM_ROT_90:
                case JXFORM_CODE.JXFORM_ROT_270:
                    info.output_width = srcinfo.Output_height;
                    info.output_height = srcinfo.Output_width;
                    if (info.num_components == 1)
                    {
                        info.iMCU_sample_width = srcinfo.min_DCT_v_scaled_size;
                        info.iMCU_sample_height = srcinfo.min_DCT_h_scaled_size;
                    }
                    else
                    {
                        info.iMCU_sample_width = srcinfo.m_max_v_samp_factor * srcinfo.min_DCT_v_scaled_size;
                        info.iMCU_sample_height = srcinfo.m_max_h_samp_factor * srcinfo.min_DCT_h_scaled_size;
                    }
                    break;

                default:
                    info.output_width = srcinfo.Output_width;
                    info.output_height = srcinfo.Output_height;
                    if (info.num_components == 1)
                    {
                        info.iMCU_sample_width = srcinfo.min_DCT_h_scaled_size;
                        info.iMCU_sample_height = srcinfo.min_DCT_v_scaled_size;
                    }
                    else
                    {
                        info.iMCU_sample_width = srcinfo.m_max_h_samp_factor * srcinfo.min_DCT_h_scaled_size;
                        info.iMCU_sample_height = srcinfo.m_max_v_samp_factor * srcinfo.min_DCT_v_scaled_size;
                    }
                    break;
            }

            /* If cropping has been requested, compute the crop area's position and
             * dimensions, ensuring that its upper left corner falls at an iMCU boundary.
             */
            if (info.crop)
            {
                /* Insert default values for unset crop parameters */
                if (info.crop_xoffset_set == JCROP_CODE.JCROP_UNSET)
                    info.crop_xoffset = 0; /* default to +0 */

                if (info.crop_yoffset_set == JCROP_CODE.JCROP_UNSET)
                    info.crop_yoffset = 0; /* default to +0 */

                if (info.crop_width_set == JCROP_CODE.JCROP_UNSET)
                {
                    if (info.crop_xoffset >= info.output_width)
                        srcinfo.ERREXIT(J_MESSAGE_CODE.JERR_BAD_CROP_SPEC);

                    info.crop_width = info.output_width - info.crop_xoffset;
                }
                else
                {
                    /* Check for crop extension */
                    if (info.crop_width > info.output_width)
                    {
                        /* Crop extension does not work when transforming! */
                        if (info.transform != JXFORM_CODE.JXFORM_NONE ||
                            info.crop_xoffset >= info.crop_width ||
                            info.crop_xoffset > info.crop_width - info.output_width)
                        {
                            srcinfo.ERREXIT(J_MESSAGE_CODE.JERR_BAD_CROP_SPEC);
                        }
                    }
                    else
                    {
                        if (info.crop_xoffset >= info.output_width ||
                            info.crop_width <= 0 ||
                            info.crop_xoffset > info.output_width - info.crop_width)
                        {
                            srcinfo.ERREXIT(J_MESSAGE_CODE.JERR_BAD_CROP_SPEC);
                        }
                    }
                }

                if (info.crop_height_set == JCROP_CODE.JCROP_UNSET)
                {
                    if (info.crop_yoffset >= info.output_height)
                        srcinfo.ERREXIT(J_MESSAGE_CODE.JERR_BAD_CROP_SPEC);

                    info.crop_height = info.output_height - info.crop_yoffset;
                }
                else
                {
                    /* Check for crop extension */
                    if (info.crop_height > info.output_height)
                    {
                        /* Crop extension does not work when transforming! */
                        if (info.transform != JXFORM_CODE.JXFORM_NONE ||
                            info.crop_yoffset >= info.crop_height ||
                            info.crop_yoffset > info.crop_height - info.output_height)
                        {
                            srcinfo.ERREXIT(J_MESSAGE_CODE.JERR_BAD_CROP_SPEC);
                        }
                    }
                    else
                    {
                        if (info.crop_yoffset >= info.output_height ||
                            info.crop_height <= 0 ||
                            info.crop_yoffset > info.output_height - info.crop_height)
                        {
                            srcinfo.ERREXIT(J_MESSAGE_CODE.JERR_BAD_CROP_SPEC);
                        }
                    }
                }

                JDIMENSION xoffset;
                /* Convert negative crop offsets into regular offsets */
                if (info.crop_xoffset_set != JCROP_CODE.JCROP_NEG)
                    xoffset = info.crop_xoffset;
                else if (info.crop_width > info.output_width) /* crop extension */
                    xoffset = info.crop_width - info.output_width - info.crop_xoffset;
                else
                    xoffset = info.output_width - info.crop_width - info.crop_xoffset;

                JDIMENSION yoffset;
                if (info.crop_yoffset_set != JCROP_CODE.JCROP_NEG)
                    yoffset = info.crop_yoffset;
                else if (info.crop_height > info.output_height) /* crop extension */
                    yoffset = info.crop_height - info.output_height - info.crop_yoffset;
                else
                    yoffset = info.output_height - info.crop_height - info.crop_yoffset;

                /* Now adjust so that upper left corner falls at an iMCU boundary */
                switch (info.transform)
                {
                    case JXFORM_CODE.JXFORM_DROP:
                        /* Ensure the effective drop region will not exceed the requested */
                        int itemp = info.iMCU_sample_width;
                        JDIMENSION dtemp = itemp - 1 - ((xoffset + itemp - 1) % itemp);
                        xoffset += dtemp;
                        if (info.crop_width <= dtemp)
                            info.drop_width = 0;
                        else if (xoffset + info.crop_width - dtemp == info.output_width)
                            /* Matching right edge: include partial iMCU */
                            info.drop_width = (info.crop_width - dtemp + itemp - 1) / itemp;
                        else
                            info.drop_width = (info.crop_width - dtemp) / itemp;

                        itemp = info.iMCU_sample_height;
                        dtemp = itemp - 1 - ((yoffset + itemp - 1) % itemp);
                        yoffset += dtemp;
                        if (info.crop_height <= dtemp)
                            info.drop_height = 0;
                        else if (yoffset + info.crop_height - dtemp == info.output_height)
                            /* Matching bottom edge: include partial iMCU */
                            info.drop_height = (info.crop_height - dtemp + itemp - 1) / itemp;
                        else
                            info.drop_height = (info.crop_height - dtemp) / itemp;

                        /* Check if sampling factors match for dropping */
                        if (info.drop_width != 0 && info.drop_height != 0)
                        {
                            for (int ci = 0;
                                ci < info.num_components && ci < info.drop_ptr.Num_components;
                                ci++)
                            {
                                if (info.drop_ptr.Comp_info[ci].H_samp_factor *
                                    srcinfo.m_max_h_samp_factor !=
                                    srcinfo.Comp_info[ci].H_samp_factor *
                                    info.drop_ptr.m_max_h_samp_factor)
                                {
                                    srcinfo.ERREXIT(J_MESSAGE_CODE.JERR_BAD_DROP_SAMPLING,
                                        ci,
                                        info.drop_ptr.Comp_info[ci].H_samp_factor,
                                        info.drop_ptr.m_max_h_samp_factor,
                                        srcinfo.Comp_info[ci].H_samp_factor,
                                        srcinfo.m_max_h_samp_factor,
                                        'h');
                                }

                                if (info.drop_ptr.Comp_info[ci].V_samp_factor *
                                    srcinfo.m_max_v_samp_factor !=
                                    srcinfo.Comp_info[ci].V_samp_factor *
                                    info.drop_ptr.m_max_v_samp_factor)
                                {
                                    srcinfo.ERREXIT(J_MESSAGE_CODE.JERR_BAD_DROP_SAMPLING,
                                        ci,
                                        info.drop_ptr.Comp_info[ci].V_samp_factor,
                                        info.drop_ptr.m_max_v_samp_factor,
                                        srcinfo.Comp_info[ci].V_samp_factor,
                                        srcinfo.m_max_v_samp_factor,
                                        'v');
                                }
                            }
                        }
                        break;

                    case JXFORM_CODE.JXFORM_WIPE:
                        /* Ensure the effective wipe region will cover the requested */
                        info.drop_width = (JDIMENSION)JpegUtils.jdiv_round_up(
                            info.crop_width + (xoffset % info.iMCU_sample_width),
                            info.iMCU_sample_width);
                        info.drop_height = (JDIMENSION)JpegUtils.jdiv_round_up(
                            info.crop_height + (yoffset % info.iMCU_sample_height),
                            info.iMCU_sample_height);
                        break;

                    default:
                        /* Ensure the effective crop region will cover the requested */
                        if (info.crop_width_set == JCROP_CODE.JCROP_FORCE ||
                            info.crop_width > info.output_width)
                        {
                            info.output_width = info.crop_width;
                        }
                        else
                        {
                            info.output_width = info.crop_width + (xoffset % info.iMCU_sample_width);
                        }

                        if (info.crop_height_set == JCROP_CODE.JCROP_FORCE ||
                            info.crop_height > info.output_height)
                        {
                            info.output_height = info.crop_height;
                        }
                        else
                        {
                            info.output_height = info.crop_height + (yoffset % info.iMCU_sample_height);
                        }
                        break;
                }

                /* Save x/y offsets measured in iMCUs */
                info.x_crop_offset = xoffset / info.iMCU_sample_width;
                info.y_crop_offset = yoffset / info.iMCU_sample_height;
            }
            else
            {
                info.x_crop_offset = 0;
                info.y_crop_offset = 0;
            }

            /* Figure out whether we need workspace arrays,
             * and if so whether they are transposed relative to the source.
             */
            bool need_workspace = false;
            bool transpose_it = false;
            switch (info.transform)
            {
                case JXFORM_CODE.JXFORM_NONE:
                    if (info.x_crop_offset != 0 || info.y_crop_offset != 0 ||
                    info.output_width > srcinfo.Output_width ||
                    info.output_height > srcinfo.Output_height)
                        need_workspace = true;

                    /* No workspace needed if neither cropping nor transforming */
                    break;

                case JXFORM_CODE.JXFORM_FLIP_H:
                    if (info.trim)
                        trim_right_edge(info, srcinfo.Output_width);

                    if (info.y_crop_offset != 0)
                        need_workspace = true;

                    /* do_flip_h_no_crop doesn't need a workspace array */
                    break;

                case JXFORM_CODE.JXFORM_FLIP_V:
                    if (info.trim)
                        trim_bottom_edge(info, srcinfo.Output_height);

                    /* Need workspace arrays having same dimensions as source image. */
                    need_workspace = true;
                    break;

                case JXFORM_CODE.JXFORM_TRANSPOSE:
                    /* transpose does NOT have to trim anything */
                    /* Need workspace arrays having transposed dimensions. */
                    need_workspace = true;
                    transpose_it = true;
                    break;

                case JXFORM_CODE.JXFORM_TRANSVERSE:
                    if (info.trim)
                    {
                        trim_right_edge(info, srcinfo.Output_height);
                        trim_bottom_edge(info, srcinfo.Output_width);
                    }

                    /* Need workspace arrays having transposed dimensions. */
                    need_workspace = true;
                    transpose_it = true;
                    break;

                case JXFORM_CODE.JXFORM_ROT_90:
                    if (info.trim)
                        trim_right_edge(info, srcinfo.Output_height);
                    /* Need workspace arrays having transposed dimensions. */
                    need_workspace = true;
                    transpose_it = true;
                    break;

                case JXFORM_CODE.JXFORM_ROT_180:
                    if (info.trim)
                    {
                        trim_right_edge(info, srcinfo.Output_width);
                        trim_bottom_edge(info, srcinfo.Output_height);
                    }

                    /* Need workspace arrays having same dimensions as source image. */
                    need_workspace = true;
                    break;

                case JXFORM_CODE.JXFORM_ROT_270:
                    if (info.trim)
                        trim_bottom_edge(info, srcinfo.Output_width);
                    /* Need workspace arrays having transposed dimensions. */
                    need_workspace = true;
                    transpose_it = true;
                    break;

                case JXFORM_CODE.JXFORM_WIPE:
                    break;

                case JXFORM_CODE.JXFORM_DROP:
                    drop_request_from_src(info.drop_ptr, srcinfo);
                    break;
            }

            /* Allocate workspace if needed.
            * Note that we allocate arrays padded out to the next iMCU boundary,
            * so that transform routines need not worry about missing edge blocks.
            */
            if (need_workspace)
            {
                jvirt_array<JBLOCK>[] coef_arrays = new jvirt_array<JBLOCK>[info.num_components];
                var width_in_iMCUs = (JDIMENSION)JpegUtils.jdiv_round_up(info.output_width, info.iMCU_sample_width);
                var height_in_iMCUs = (JDIMENSION)JpegUtils.jdiv_round_up(info.output_height, info.iMCU_sample_height);
                for (JDIMENSION ci = 0; ci < info.num_components; ci++)
                {
                    var compptr = srcinfo.Comp_info[ci];
                    int h_samp_factor;
                    int v_samp_factor;
                    if (info.num_components == 1)
                    {
                        /* we're going to force samp factors to 1x1 in this case */
                        h_samp_factor = 1;
                        v_samp_factor = 1;
                    }
                    else if (transpose_it)
                    {
                        h_samp_factor = compptr.V_samp_factor;
                        v_samp_factor = compptr.H_samp_factor;
                    }
                    else
                    {
                        h_samp_factor = compptr.H_samp_factor;
                        v_samp_factor = compptr.V_samp_factor;
                    }

                    JDIMENSION width_in_blocks = width_in_iMCUs * h_samp_factor;
                    JDIMENSION height_in_blocks = height_in_iMCUs * v_samp_factor;
                    coef_arrays[ci] = jpeg_common_struct.CreateBlocksArray(
                        width_in_blocks, height_in_blocks);
                }
                info.workspace_coef_arrays = coef_arrays;
            }
            else
            {
                info.workspace_coef_arrays = null;
            }

            return true;
        }

        /* Adjust output image parameters as needed.
         *
         * This must be called after jpeg_copy_critical_parameters()
         * and before jpeg_write_coefficients().
         *
         * The return value is the set of virtual coefficient arrays to be written
         * (either the ones allocated by jtransform_request_workspace, or the
         * original source data arrays). The caller will need to pass this value
         * to jpeg_write_coefficients().
         */
        public static jvirt_array<JBLOCK>[] adjust_parameters(
            jpeg_decompress_struct srcinfo, jpeg_compress_struct dstinfo,
            jvirt_array<JBLOCK>[] src_coef_arrays, jpeg_transform_info info)
        {
            /* If force-to-grayscale is requested, adjust destination parameters */
            if (info.force_grayscale)
            {
                /* First, ensure we have YCC or grayscale data, and that the source's
                 * Y channel is full resolution.  (No reasonable person would make Y
                 * be less than full resolution, so actually coping with that case
                 * isn't worth extra code space.  But we check it to avoid crashing.)
                 */
                if ((((dstinfo.Jpeg_color_space == J_COLOR_SPACE.JCS_YCbCr ||
                    dstinfo.Jpeg_color_space == J_COLOR_SPACE.JCS_BG_YCC) &&
                    dstinfo.Num_components == 3) ||
                    (dstinfo.Jpeg_color_space == J_COLOR_SPACE.JCS_GRAYSCALE &&
                    dstinfo.Num_components == 1)) &&
                    srcinfo.Comp_info[0].H_samp_factor == srcinfo.m_max_h_samp_factor &&
                    srcinfo.Comp_info[0].V_samp_factor == srcinfo.m_max_v_samp_factor)
                {
                    /* We use jpeg_set_colorspace to make sure subsidiary settings get fixed
                     * properly.  Among other things, it sets the target h_samp_factor &
                     * v_samp_factor to 1, which typically won't match the source.
                     * We have to preserve the source's quantization table number, however.
                     */
                    int sv_quant_tbl_no = dstinfo.Component_info[0].Quant_tbl_no;
                    dstinfo.jpeg_set_colorspace(J_COLOR_SPACE.JCS_GRAYSCALE);
                    dstinfo.Component_info[0].Quant_tbl_no = sv_quant_tbl_no;
                }
                else
                {
                    /* Sorry, can't do it */
                    dstinfo.ERREXIT(J_MESSAGE_CODE.JERR_CONVERSION_NOTIMPL);
                }
            }
            else if (info.num_components == 1)
            {
                /* For a single-component source, we force the destination sampling factors
                 * to 1x1, with or without force_grayscale.  This is useful because some
                 * decoders choke on grayscale images with other sampling factors.
                 */
                dstinfo.Component_info[0].H_samp_factor = 1;
                dstinfo.Component_info[0].V_samp_factor = 1;
            }

            /* Correct the destination's image dimensions as necessary
             * for rotate/flip, resize, and crop operations.
             */
            dstinfo.jpeg_width = info.output_width;
            dstinfo.jpeg_height = info.output_height;

            /* Transpose destination image parameters, adjust quantization */
            switch (info.transform)
            {
                case JXFORM_CODE.JXFORM_TRANSPOSE:
                case JXFORM_CODE.JXFORM_TRANSVERSE:
                case JXFORM_CODE.JXFORM_ROT_90:
                case JXFORM_CODE.JXFORM_ROT_270:
                    transpose_critical_parameters(dstinfo);
                    break;

                case JXFORM_CODE.JXFORM_DROP:
                    if (info.drop_width != 0 && info.drop_height != 0)
                    {
                        adjust_quant(srcinfo, src_coef_arrays, info.drop_ptr,
                            info.drop_coef_arrays, info.trim, dstinfo);
                    }
                    break;

                default:
                    break;
            }

            /* Adjust Exif properties */
            if (srcinfo.Marker_list?.Count > 0 &&
                srcinfo.Marker_list[0].Marker == JPEG_APP0 + 1 &&
                srcinfo.Marker_list[0].Data.Length >= 6 &&
                srcinfo.Marker_list[0].Data[0] == 0x45 &&
                srcinfo.Marker_list[0].Data[1] == 0x78 &&
                srcinfo.Marker_list[0].Data[2] == 0x69 &&
                srcinfo.Marker_list[0].Data[3] == 0x66 &&
                srcinfo.Marker_list[0].Data[4] == 0 &&
                srcinfo.Marker_list[0].Data[5] == 0)
            {
                /* Suppress output of JFIF marker */
                dstinfo.Write_JFIF_header = false;

                /* Adjust Exif image parameters */
                if (dstinfo.jpeg_width != srcinfo.Image_width || dstinfo.jpeg_height != srcinfo.Image_height)
                {
                    /* Align data segment to start of TIFF structure for parsing */
                    adjust_exif_parameters(
                        srcinfo.Marker_list[0].Data, 6, srcinfo.Marker_list[0].Data.Length - 6,
                        dstinfo.jpeg_width, dstinfo.jpeg_height);
                }
            }

            /* Return the appropriate output data set */
            if (info.workspace_coef_arrays != null)
                return info.workspace_coef_arrays;

            return src_coef_arrays;
        }

        /* Copy markers saved in the given source object to the destination object.
         * This should be called just after jpeg_start_compress() or
         * jpeg_write_coefficients().
         * Note that those routines will have written the SOI, and also the
         * JFIF APP0 or Adobe APP14 markers if selected.
         */
        public static void jcopy_markers_execute(jpeg_decompress_struct srcinfo, jpeg_compress_struct dstinfo)
        {
            /* In the current implementation, we don't actually need to examine the
             * option flag here; we just copy everything that got saved.
             * But to avoid confusion, we do not output JFIF and Adobe APP14 markers
             * if the encoder library already wrote one.
             */
            for (int i = 0; i < srcinfo.Marker_list.Count; i++)
            {
                var marker = srcinfo.Marker_list[i];

                if (dstinfo.Write_JFIF_header &&
                    marker.Marker == JPEG_APP0 &&
                    marker.Data.Length >= 5 &&
                    marker.Data[0] == 0x4A &&
                    marker.Data[1] == 0x46 &&
                    marker.Data[2] == 0x49 &&
                    marker.Data[3] == 0x46 &&
                    marker.Data[4] == 0)
                {
                    /* reject duplicate JFIF */
                    continue;
                }

                if (dstinfo.Write_Adobe_marker &&
                    marker.Marker == JPEG_APP0 + 14 &&
                    marker.Data.Length >= 5 &&
                    marker.Data[0] == 0x41 &&
                    marker.Data[1] == 0x64 &&
                    marker.Data[2] == 0x6F &&
                    marker.Data[3] == 0x62 &&
                    marker.Data[4] == 0x65)
                {
                    /* reject duplicate Adobe */
                    continue;
                }

                dstinfo.jpeg_write_marker(marker.Marker, marker.Data);
            }
        }

        /* Execute the actual transformation, if any.
         *
         * This must be called *after* jpeg_write_coefficients, because it depends
         * on jpeg_write_coefficients to have computed subsidiary values such as
         * the per-component width and height fields in the destination object.
         *
         * Note that some transformations will modify the source data arrays!
         */
        public static void execute_transformation(
            jpeg_decompress_struct srcinfo, jpeg_compress_struct dstinfo,
            jvirt_array<JBLOCK>[] src_coef_arrays, jpeg_transform_info info)
        {
            var dst_coef_arrays = info.workspace_coef_arrays;

            /* Note: conditions tested here should match those in switch statement
             * in jtransform_request_workspace()
             */
            switch (info.transform)
            {
                case JXFORM_CODE.JXFORM_NONE:
                    if (info.output_width > srcinfo.Output_width ||
                        info.output_height > srcinfo.Output_height)
                    {
                        if (info.output_width > srcinfo.Output_width &&
                            info.crop_width_set == JCROP_CODE.JCROP_REFLECT)
                        {
                            do_crop_ext_reflect(srcinfo, dstinfo, info.x_crop_offset,
                                info.y_crop_offset, src_coef_arrays, dst_coef_arrays);
                        }
                        else if (info.output_width > srcinfo.Output_width &&
                            info.crop_width_set == JCROP_CODE.JCROP_FORCE)
                        {
                            do_crop_ext_flat(srcinfo, dstinfo, info.x_crop_offset,
                                info.y_crop_offset, src_coef_arrays, dst_coef_arrays);
                        }
                        else
                        {
                            do_crop_ext_zero(srcinfo, dstinfo, info.x_crop_offset,
                                info.y_crop_offset, src_coef_arrays, dst_coef_arrays);
                        }
                    }
                    else if (info.x_crop_offset != 0 || info.y_crop_offset != 0)
                    {
                        do_crop(srcinfo, dstinfo, info.x_crop_offset, info.y_crop_offset,
                            src_coef_arrays, dst_coef_arrays);
                    }
                    break;

                case JXFORM_CODE.JXFORM_FLIP_H:
                    if (info.y_crop_offset != 0)
                    {
                        do_flip_h(srcinfo, dstinfo, info.x_crop_offset, info.y_crop_offset,
                            src_coef_arrays, dst_coef_arrays);
                    }
                    else
                    {
                        do_flip_h_no_crop(srcinfo, dstinfo, info.x_crop_offset, src_coef_arrays);
                    }
                    break;

                case JXFORM_CODE.JXFORM_FLIP_V:
                    do_flip_v(srcinfo, dstinfo, info.x_crop_offset, info.y_crop_offset,
                        src_coef_arrays, dst_coef_arrays);
                    break;

                case JXFORM_CODE.JXFORM_TRANSPOSE:
                    do_transpose(srcinfo, dstinfo, info.x_crop_offset, info.y_crop_offset,
                        src_coef_arrays, dst_coef_arrays);
                    break;

                case JXFORM_CODE.JXFORM_TRANSVERSE:
                    do_transverse(srcinfo, dstinfo, info.x_crop_offset, info.y_crop_offset,
                        src_coef_arrays, dst_coef_arrays);
                    break;

                case JXFORM_CODE.JXFORM_ROT_90:
                    do_rot_90(srcinfo, dstinfo, info.x_crop_offset, info.y_crop_offset,
                        src_coef_arrays, dst_coef_arrays);
                    break;

                case JXFORM_CODE.JXFORM_ROT_180:
                    do_rot_180(srcinfo, dstinfo, info.x_crop_offset, info.y_crop_offset,
                        src_coef_arrays, dst_coef_arrays);
                    break;

                case JXFORM_CODE.JXFORM_ROT_270:
                    do_rot_270(srcinfo, dstinfo, info.x_crop_offset, info.y_crop_offset,
                        src_coef_arrays, dst_coef_arrays);
                    break;

                case JXFORM_CODE.JXFORM_WIPE:
                    if (info.crop_width_set == JCROP_CODE.JCROP_REFLECT &&
                        info.y_crop_offset == 0 &&
                        info.drop_height ==
                        (JDIMENSION)JpegUtils.jdiv_round_up(info.output_height, info.iMCU_sample_height) &&
                        (info.x_crop_offset == 0 ||
                        info.x_crop_offset + info.drop_width ==
                        (JDIMENSION)JpegUtils.jdiv_round_up(info.output_width, info.iMCU_sample_width)))
                    {
                        do_reflect(srcinfo, dstinfo, info.x_crop_offset, src_coef_arrays,
                            info.drop_width, info.drop_height);
                    }
                    else if (info.crop_width_set == JCROP_CODE.JCROP_FORCE)
                    {
                        do_flatten(srcinfo, dstinfo, info.x_crop_offset, info.y_crop_offset,
                            src_coef_arrays, info.drop_width, info.drop_height);
                    }
                    else
                    {
                        do_wipe(srcinfo, dstinfo, info.x_crop_offset, info.y_crop_offset,
                            src_coef_arrays, info.drop_width, info.drop_height);
                    }
                    break;

                case JXFORM_CODE.JXFORM_DROP:
                    if (info.drop_width != 0 && info.drop_height != 0)
                    {
                        do_drop(srcinfo, dstinfo, info.x_crop_offset, info.y_crop_offset,
                            src_coef_arrays, info.drop_ptr, info.drop_coef_arrays,
                            info.drop_width, info.drop_height);
                    }
                    break;
            }
        }

        /*
         * Determine whether lossless transformation is perfectly
         * possible for a specified image and transformation.
         *
         * Inputs:
         *   image_width, image_height: source image dimensions.
         *   MCU_width, MCU_height: pixel dimensions of MCU.
         *   transform: transformation identifier.
         * Parameter sources from initialized jpeg_struct
         * (after reading source header):
         *   image_width = cinfo.image_width
         *   image_height = cinfo.image_height
         *   MCU_width = cinfo.max_h_samp_factor * cinfo.block_size
         *   MCU_height = cinfo.max_v_samp_factor * cinfo.block_size
         * Result:
         *   TRUE = perfect transformation possible
         *   FALSE = perfect transformation not possible
         *           (may use custom action then)
         */
        private static bool perfect_transform(JDIMENSION image_width, JDIMENSION image_height,
            int MCU_width, int MCU_height, JXFORM_CODE transform)
        {
            switch (transform)
            {
                case JXFORM_CODE.JXFORM_FLIP_H:
                case JXFORM_CODE.JXFORM_ROT_270:
                    if (image_width % MCU_width != 0)
                        return false;

                    break;

                case JXFORM_CODE.JXFORM_FLIP_V:
                case JXFORM_CODE.JXFORM_ROT_90:
                    if (image_height % MCU_height != 0)
                        return false;

                    break;

                case JXFORM_CODE.JXFORM_TRANSVERSE:
                case JXFORM_CODE.JXFORM_ROT_180:
                    if (image_width % MCU_width != 0)
                        return false;

                    if (image_height % MCU_height != 0)
                        return false;

                    break;

                default:
                    break;
            }

            return true;
        }

        /* Trim off any partial iMCUs on the indicated destination edge */

        private static void trim_right_edge(jpeg_transform_info info, JDIMENSION full_width)
        {
            JDIMENSION MCU_cols = info.output_width / info.iMCU_sample_width;
            if (MCU_cols > 0 && info.x_crop_offset + MCU_cols == full_width / info.iMCU_sample_width)
                info.output_width = MCU_cols * info.iMCU_sample_width;
        }

        private static void trim_bottom_edge(jpeg_transform_info info, JDIMENSION full_height)
        {
            JDIMENSION MCU_rows = info.output_height / info.iMCU_sample_height;
            if (MCU_rows > 0 && info.y_crop_offset + MCU_rows == full_height / info.iMCU_sample_height)
                info.output_height = MCU_rows * info.iMCU_sample_height;
        }

        private static void drop_request_from_src(jpeg_decompress_struct dropinfo, jpeg_decompress_struct srcinfo)
        {
            throw new NotImplementedException();
        }

        /* Transpose destination image parameters */
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0180:Use tuple to swap values")]
        private static void transpose_critical_parameters(jpeg_compress_struct dstinfo)
        {
            /* Transpose image dimensions */
            var jtemp = dstinfo.Image_width;
            dstinfo.Image_width = dstinfo.Image_height;
            dstinfo.Image_height = jtemp;

            var itemp = dstinfo.min_DCT_h_scaled_size;
            dstinfo.min_DCT_h_scaled_size = dstinfo.min_DCT_v_scaled_size;
            dstinfo.min_DCT_v_scaled_size = itemp;

            /* Transpose sampling factors */
            for (int ci = 0; ci < dstinfo.Num_components; ci++)
            {
                var compptr = dstinfo.Component_info[ci];
                itemp = compptr.H_samp_factor;
                compptr.H_samp_factor = compptr.V_samp_factor;
                compptr.V_samp_factor = itemp;
            }

            /* Transpose quantization tables */
            for (int tblno = 0; tblno < JpegConstants.NUM_QUANT_TBLS; tblno++)
            {
                var qtblptr = dstinfo.Quant_tbl_ptrs[tblno];
                if (qtblptr == null)
                    continue;

                for (int i = 0; i < JpegConstants.DCTSIZE; i++)
                {
                    for (int j = 0; j < i; j++)
                    {
                        var qtemp = qtblptr.quantval[i * JpegConstants.DCTSIZE + j];
                        qtblptr.quantval[i * JpegConstants.DCTSIZE + j] = qtblptr.quantval[j * JpegConstants.DCTSIZE + i];
                        qtblptr.quantval[j * JpegConstants.DCTSIZE + i] = qtemp;
                    }
                }
            }
        }

        private static void adjust_quant(jpeg_decompress_struct srcinfo,
            jvirt_array<JBLOCK>[] src_coef_arrays, jpeg_decompress_struct dropinfo,
            jvirt_array<JBLOCK>[] drop_coef_arrays, bool trim, jpeg_compress_struct dstinfo)
        {
            throw new NotImplementedException();
        }

        /* Adjust Exif image parameters.
         *
         * We try to adjust the Tags ExifImageWidth and ExifImageHeight if possible.
         */
        private static void adjust_exif_parameters(
            byte[] data, int offset, int length, JDIMENSION new_width, JDIMENSION new_height)
        {
            throw new NotImplementedException();
        }

        /* Crop.  This is only used when no rotate/flip is requested with the crop.
         * Extension: The destination width is larger than the source and we fill in
         * the extra area with repeated reflections of the source region.  Note we
         * also have to fill partial iMCUs at the right and bottom edge of the source
         * image area in this case.
         */
        private static void do_crop_ext_reflect(
            jpeg_decompress_struct srcinfo, jpeg_compress_struct dstinfo, JDIMENSION x_crop_offset,
            JDIMENSION y_crop_offset, jvirt_array<JBLOCK>[] src_coef_arrays,
            jvirt_array<JBLOCK>[] dst_coef_arrays)
        {
            throw new NotImplementedException();
        }

        /* Crop.  This is only used when no rotate/flip is requested with the crop.
         * Extension: The destination width is larger than the source and we fill in
         * the extra area with the DC of the adjacent block.  Note we also have to
         * fill partial iMCUs at the right and bottom edge of the source image area
         * in this case.
         */
        private static void do_crop_ext_flat(
            jpeg_decompress_struct srcinfo, jpeg_compress_struct dstinfo, JDIMENSION x_crop_offset,
            JDIMENSION y_crop_offset, jvirt_array<JBLOCK>[] src_coef_arrays,
            jvirt_array<JBLOCK>[] dst_coef_arrays)
        {
            throw new NotImplementedException();
        }

        /* Crop.  This is only used when no rotate/flip is requested with the crop.
         * Extension: If the destination size is larger than the source, we fill in
         * the extra area with zero (neutral gray).  Note we also have to zero partial
         * iMCUs at the right and bottom edge of the source image area in this case.
         */
        private static void do_crop_ext_zero(
            jpeg_decompress_struct srcinfo, jpeg_compress_struct dstinfo, JDIMENSION x_crop_offset,
            JDIMENSION y_crop_offset, jvirt_array<JBLOCK>[] src_coef_arrays,
            jvirt_array<JBLOCK>[] dst_coef_arrays)
        {
            throw new NotImplementedException();
        }

        /* Crop.  This is only used when no rotate/flip is requested with the crop. */
        private static void do_crop(
            jpeg_decompress_struct srcinfo, jpeg_compress_struct dstinfo, JDIMENSION x_crop_offset,
            JDIMENSION y_crop_offset, jvirt_array<JBLOCK>[] src_coef_arrays,
            jvirt_array<JBLOCK>[] dst_coef_arrays)
        {
            throw new NotImplementedException();
        }

        /* Horizontal flip in general cropping case */
        private static void do_flip_h(
            jpeg_decompress_struct srcinfo, jpeg_compress_struct dstinfo, JDIMENSION x_crop_offset,
            JDIMENSION y_crop_offset, jvirt_array<JBLOCK>[] src_coef_arrays,
            jvirt_array<JBLOCK>[] dst_coef_arrays)
        {
            throw new NotImplementedException();
        }

        /* Horizontal flip; done in-place, so no separate dest array is required.
         * NB: this only works when y_crop_offset is zero.
         */
        private static void do_flip_h_no_crop(
            jpeg_decompress_struct srcinfo, jpeg_compress_struct dstinfo, JDIMENSION x_crop_offset,
            jvirt_array<JBLOCK>[] src_coef_arrays)
        {
            /* Horizontal mirroring of DCT blocks is accomplished by swapping
             * pairs of blocks in-place.  Within a DCT block, we perform horizontal
             * mirroring by changing the signs of odd-numbered columns.
             * Partial iMCUs at the right edge are left untouched.
             */
            JDIMENSION MCU_cols = srcinfo.Output_width / 
                (dstinfo.m_max_h_samp_factor * dstinfo.min_DCT_h_scaled_size);

            for (int ci = 0; ci < dstinfo.Num_components; ci++)
            {
                var compptr = dstinfo.Component_info[ci];
                JDIMENSION comp_width = MCU_cols * compptr.H_samp_factor;
                JDIMENSION x_crop_blocks = x_crop_offset * compptr.H_samp_factor;
                for (JDIMENSION blk_y = 0;
                    blk_y < compptr.height_in_blocks;
                    blk_y += compptr.V_samp_factor)
                {
                    var buffer = src_coef_arrays[ci].Access(blk_y, compptr.V_samp_factor);
                    for (int offset_y = 0; offset_y < compptr.V_samp_factor; offset_y++)
                    {
                        /* Do the mirroring */
                        for (JDIMENSION blk_x = 0; blk_x * 2 < comp_width; blk_x++)
                        {
                            var ptr1 = buffer[offset_y][blk_x];
                            var ptr1Offset = 0;

                            var ptr2 = buffer[offset_y][comp_width - blk_x - 1];
                            var ptr2Offset = 0;

                            /* this unrolled loop doesn't need to know which row it's on... */
                            for (int k = 0; k < JpegConstants.DCTSIZE2; k += 2)
                            {
                                /* swap even column */
                                short temp1 = ptr1[ptr1Offset];
                                short temp2 = ptr2[ptr2Offset];
                                ptr1[ptr1Offset++] = temp2;
                                ptr2[ptr2Offset++] = temp1;

                                /* swap odd column with sign change */
                                temp1 = ptr1[ptr1Offset];
                                temp2 = ptr2[ptr2Offset];
                                ptr1[ptr1Offset++] = (short)-temp2;
                                ptr2[ptr2Offset++] = (short)-temp1;
                            }
                        }

                        if (x_crop_blocks > 0)
                        {
                            /* Now left-justify the portion of the data to be kept.
                             * We can't use a single jcopy_block_row() call because that routine
                             * depends on memcpy(), whose behavior is unspecified for overlapping
                             * source and destination areas.  Sigh.
                             */
                            for (JDIMENSION blk_x = 0; blk_x < compptr.Width_in_blocks; blk_x++)
                            {
                                jcopy_block_row(buffer[offset_y], blk_x + x_crop_blocks,
                                    buffer[offset_y], blk_x, 1);
                            }
                        }
                    }
                }
            }
        }

        /* Vertical flip */
        private static void do_flip_v(
            jpeg_decompress_struct srcinfo, jpeg_compress_struct dstinfo, JDIMENSION x_crop_offset,
            JDIMENSION y_crop_offset, jvirt_array<JBLOCK>[] src_coef_arrays,
            jvirt_array<JBLOCK>[] dst_coef_arrays)
        {
            /* We output into a separate array because we can't touch different
             * rows of the source virtual array simultaneously.  Otherwise, this
             * is a pretty straightforward analog of horizontal flip.
             * Within a DCT block, vertical mirroring is done by changing the signs
             * of odd-numbered rows.
             * Partial iMCUs at the bottom edge are copied verbatim.
             */
            JDIMENSION MCU_rows = srcinfo.Output_height /
                (dstinfo.m_max_v_samp_factor * dstinfo.min_DCT_v_scaled_size);

            for (int ci = 0; ci < dstinfo.Num_components; ci++)
            {
                var compptr = dstinfo.Component_info[ci];
                JDIMENSION comp_height = MCU_rows * compptr.V_samp_factor;
                JDIMENSION x_crop_blocks = x_crop_offset * compptr.H_samp_factor;
                JDIMENSION y_crop_blocks = y_crop_offset * compptr.V_samp_factor;
                for (JDIMENSION dst_blk_y = 0;
                    dst_blk_y < compptr.height_in_blocks;
                    dst_blk_y += compptr.V_samp_factor)
                {
                    var dst_buffer = dst_coef_arrays[ci].Access(dst_blk_y, compptr.V_samp_factor);
                    JBLOCK[][] src_buffer;
                    if (y_crop_blocks + dst_blk_y < comp_height)
                    {
                        /* Row is within the mirrorable area. */
                        src_buffer = src_coef_arrays[ci].Access(
                            comp_height - y_crop_blocks - dst_blk_y - compptr.V_samp_factor,
                            compptr.V_samp_factor);
                    }
                    else
                    {
                        /* Bottom-edge blocks will be copied verbatim. */
                        src_buffer = src_coef_arrays[ci].Access(
                            dst_blk_y + y_crop_blocks, compptr.V_samp_factor);
                    }

                    for (int offset_y = 0; offset_y < compptr.V_samp_factor; offset_y++)
                    {
                        if (y_crop_blocks + dst_blk_y < comp_height)
                        {
                            /* Row is within the mirrorable area. */
                            var dst_row_ptr = dst_buffer[offset_y];
                            var src_row_ptr = src_buffer[compptr.V_samp_factor - offset_y - 1];
                            for (JDIMENSION dst_blk_x = 0;
                                dst_blk_x < compptr.Width_in_blocks;
                                dst_blk_x++)
                            {
                                var dst_ptr = dst_row_ptr[dst_blk_x];
                                var dstOffset = 0;

                                var src_ptr = src_row_ptr[x_crop_blocks + dst_blk_x];
                                var srcOffset = 0;

                                for (int i = 0; i < JpegConstants.DCTSIZE; i += 2)
                                {
                                    /* copy even row */
                                    for (int j = 0; j < JpegConstants.DCTSIZE; j++)
                                        dst_ptr[dstOffset++] = src_ptr[srcOffset++];

                                    /* copy odd row with sign change */
                                    for (int j = 0; j < JpegConstants.DCTSIZE; j++)
                                        dst_ptr[dstOffset++] = (short)-src_ptr[srcOffset++];
                                }
                            }
                        }
                        else
                        {
                            /* Just copy row verbatim. */
                            jcopy_block_row(src_buffer[offset_y], x_crop_blocks,
                                dst_buffer[offset_y], 0, compptr.Width_in_blocks);
                        }
                    }
                }
            }
        }

        /* Transpose source into destination */
        private static void do_transpose(
            jpeg_decompress_struct srcinfo, jpeg_compress_struct dstinfo, JDIMENSION x_crop_offset,
            JDIMENSION y_crop_offset, jvirt_array<JBLOCK>[] src_coef_arrays,
            jvirt_array<JBLOCK>[] dst_coef_arrays)
        {
            throw new NotImplementedException();
        }

        /* Transverse transpose is equivalent to
         *   1. 180 degree rotation;
         *   2. Transposition;
         * or
         *   1. Horizontal mirroring;
         *   2. Transposition;
         *   3. Horizontal mirroring.
         * These steps are merged into a single processing routine.
         */
        private static void do_transverse(
            jpeg_decompress_struct srcinfo, jpeg_compress_struct dstinfo, JDIMENSION x_crop_offset,
            JDIMENSION y_crop_offset, jvirt_array<JBLOCK>[] src_coef_arrays,
            jvirt_array<JBLOCK>[] dst_coef_arrays)
        {
            throw new NotImplementedException();
        }

        /* 90 degree rotation is equivalent to
         *   1. Transposing the image;
         *   2. Horizontal mirroring.
         * These two steps are merged into a single processing routine.
         */
        private static void do_rot_90(
            jpeg_decompress_struct srcinfo, jpeg_compress_struct dstinfo, JDIMENSION x_crop_offset,
            JDIMENSION y_crop_offset, jvirt_array<JBLOCK>[] src_coef_arrays,
            jvirt_array<JBLOCK>[] dst_coef_arrays)
        {
            /* Because of the horizontal mirror step, we can't process partial iMCUs
             * at the (output) right edge properly.  They just get transposed and
             * not mirrored.
             */
            JDIMENSION MCU_cols = srcinfo.Output_height / 
                (dstinfo.m_max_h_samp_factor * dstinfo.min_DCT_h_scaled_size);

            for (int ci = 0; ci < dstinfo.Num_components; ci++)
            {
                var compptr = dstinfo.Component_info[ci];
                JDIMENSION comp_width = MCU_cols * compptr.H_samp_factor;
                JDIMENSION x_crop_blocks = x_crop_offset * compptr.H_samp_factor;
                JDIMENSION y_crop_blocks = y_crop_offset * compptr.V_samp_factor;
                for (JDIMENSION dst_blk_y = 0;
                    dst_blk_y < compptr.height_in_blocks;
                    dst_blk_y += compptr.V_samp_factor)
                {
                    var dst_buffer = dst_coef_arrays[ci].Access(dst_blk_y, compptr.V_samp_factor);
                    for (int offset_y = 0; offset_y < compptr.V_samp_factor; offset_y++)
                    {
                        for (JDIMENSION dst_blk_x = 0;
                            dst_blk_x < compptr.Width_in_blocks;
                            dst_blk_x += compptr.H_samp_factor)
                        {
                            JBLOCK[][] src_buffer;
                            if (x_crop_blocks + dst_blk_x < comp_width)
                            {
                                /* Block is within the mirrorable area. */
                                src_buffer = src_coef_arrays[ci].Access(
                                    comp_width - x_crop_blocks - dst_blk_x - compptr.H_samp_factor,
                                    compptr.H_samp_factor);
                            }
                            else
                            {
                                /* Edge blocks are transposed but not mirrored. */
                                src_buffer = src_coef_arrays[ci].Access(
                                    dst_blk_x + x_crop_blocks, compptr.H_samp_factor);
                            }

                            for (int offset_x = 0; offset_x < compptr.H_samp_factor; offset_x++)
                            {
                                var dst_ptr = dst_buffer[offset_y][dst_blk_x + offset_x];
                                if (x_crop_blocks + dst_blk_x < comp_width)
                                {
                                    /* Block is within the mirrorable area. */
                                    var src_ptr = src_buffer[compptr.H_samp_factor - offset_x - 1][dst_blk_y + offset_y + y_crop_blocks];
                                    for (int i = 0; i < JpegConstants.DCTSIZE; i++)
                                    {
                                        for (int j = 0; j < JpegConstants.DCTSIZE; j++)
                                        {
                                            dst_ptr[j * JpegConstants.DCTSIZE + i] =
                                                  src_ptr[i * JpegConstants.DCTSIZE + j];
                                        }

                                        i++;

                                        for (int j = 0; j < JpegConstants.DCTSIZE; j++)
                                        {
                                            dst_ptr[j * JpegConstants.DCTSIZE + i] =
                                                (short)-src_ptr[i * JpegConstants.DCTSIZE + j];
                                        }
                                    }
                                }
                                else
                                {
                                    /* Edge blocks are transposed but not mirrored. */
                                    var src_ptr = src_buffer[offset_x][dst_blk_y + offset_y + y_crop_blocks];
                                    for (int i = 0; i < JpegConstants.DCTSIZE; i++)
                                    {
                                        for (int j = 0; j < JpegConstants.DCTSIZE; j++)
                                        {
                                            dst_ptr[j * JpegConstants.DCTSIZE + i] =
                                                src_ptr[i * JpegConstants.DCTSIZE + j];
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        /* 180 degree rotation is equivalent to
         *   1. Vertical mirroring;
         *   2. Horizontal mirroring.
         * These two steps are merged into a single processing routine.
         */
        private static void do_rot_180(
            jpeg_decompress_struct srcinfo, jpeg_compress_struct dstinfo, JDIMENSION x_crop_offset,
            JDIMENSION y_crop_offset, jvirt_array<JBLOCK>[] src_coef_arrays,
            jvirt_array<JBLOCK>[] dst_coef_arrays)
        {
            JDIMENSION MCU_cols = srcinfo.Output_width /
                (dstinfo.m_max_h_samp_factor * dstinfo.min_DCT_h_scaled_size);
            JDIMENSION MCU_rows = srcinfo.Output_height /
                (dstinfo.m_max_v_samp_factor * dstinfo.min_DCT_v_scaled_size);

            for (int ci = 0; ci < dstinfo.Num_components; ci++)
            {
                var compptr = dstinfo.Component_info[ci];
                JDIMENSION comp_width = MCU_cols * compptr.H_samp_factor;
                JDIMENSION comp_height = MCU_rows * compptr.V_samp_factor;
                JDIMENSION x_crop_blocks = x_crop_offset * compptr.H_samp_factor;
                JDIMENSION y_crop_blocks = y_crop_offset * compptr.V_samp_factor;
                for (JDIMENSION dst_blk_y = 0;
                    dst_blk_y < compptr.height_in_blocks;
                    dst_blk_y += compptr.V_samp_factor)
                {
                    var dst_buffer = dst_coef_arrays[ci].Access(dst_blk_y, compptr.V_samp_factor);
                    JBLOCK[][] src_buffer;
                    if (y_crop_blocks + dst_blk_y < comp_height)
                    {
                        /* Row is within the vertically mirrorable area. */
                        src_buffer = src_coef_arrays[ci].Access(
                            comp_height - y_crop_blocks - dst_blk_y - compptr.V_samp_factor,
                            compptr.V_samp_factor);
                    }
                    else
                    {
                        /* Bottom-edge rows are only mirrored horizontally. */
                        src_buffer = src_coef_arrays[ci].Access(
                            dst_blk_y + y_crop_blocks, compptr.V_samp_factor);
                    }

                    for (int offset_y = 0; offset_y < compptr.V_samp_factor; offset_y++)
                    {
                        var dst_row_ptr = dst_buffer[offset_y];
                        if (y_crop_blocks + dst_blk_y < comp_height)
                        {
                            /* Row is within the mirrorable area. */
                            var src_row_ptr = src_buffer[compptr.V_samp_factor - offset_y - 1];
                            for (JDIMENSION dst_blk_x = 0; dst_blk_x < compptr.Width_in_blocks; dst_blk_x++)
                            {
                                var dst_ptr = dst_row_ptr[dst_blk_x];
                                var dstOffset = 0;

                                if (x_crop_blocks + dst_blk_x < comp_width)
                                {
                                    /* Process the blocks that can be mirrored both ways. */
                                    var src_ptr = src_row_ptr[comp_width - x_crop_blocks - dst_blk_x - 1];
                                    var srcOffset = 0;

                                    for (int i = 0; i < JpegConstants.DCTSIZE; i += 2)
                                    {
                                        /* For even row, negate every odd column. */
                                        for (int j = 0; j < JpegConstants.DCTSIZE; j += 2)
                                        {
                                            dst_ptr[dstOffset++] = src_ptr[srcOffset++];
                                            dst_ptr[dstOffset++] = (short)-src_ptr[srcOffset++];
                                        }

                                        /* For odd row, negate every even column. */
                                        for (int j = 0; j < JpegConstants.DCTSIZE; j += 2)
                                        {
                                            dst_ptr[dstOffset++] = (short)-src_ptr[srcOffset++];
                                            dst_ptr[dstOffset++] = src_ptr[srcOffset++];
                                        }
                                    }
                                }
                                else
                                {
                                    /* Any remaining right-edge blocks are only mirrored vertically. */
                                    var src_ptr = src_row_ptr[x_crop_blocks + dst_blk_x];
                                    var srcOffset = 0;
                                    for (int i = 0; i < JpegConstants.DCTSIZE; i += 2)
                                    {
                                        for (int j = 0; j < JpegConstants.DCTSIZE; j++)
                                            dst_ptr[dstOffset++] = src_ptr[srcOffset++];

                                        for (int j = 0; j < JpegConstants.DCTSIZE; j++)
                                            dst_ptr[dstOffset++] = (short)-src_ptr[srcOffset++];
                                    }
                                }
                            }
                        }
                        else
                        {
                            /* Remaining rows are just mirrored horizontally. */
                            var src_row_ptr = src_buffer[offset_y];
                            for (JDIMENSION dst_blk_x = 0; dst_blk_x < compptr.Width_in_blocks; dst_blk_x++)
                            {
                                if (x_crop_blocks + dst_blk_x < comp_width)
                                {
                                    /* Process the blocks that can be mirrored. */
                                    var dst_ptr = dst_row_ptr[dst_blk_x];
                                    var dstOffset = 0;

                                    var src_ptr = src_row_ptr[comp_width - x_crop_blocks - dst_blk_x - 1];
                                    var srcOffset = 0;

                                    for (int i = 0; i < JpegConstants.DCTSIZE2; i += 2)
                                    {
                                        dst_ptr[dstOffset++] = src_ptr[srcOffset++];
                                        dst_ptr[dstOffset++] = (short)-src_ptr[srcOffset++];
                                    }
                                }
                                else
                                {
                                    /* Any remaining right-edge blocks are only copied. */
                                    jcopy_block_row(
                                        src_row_ptr, dst_blk_x + x_crop_blocks, dst_row_ptr, dst_blk_x, 1);
                                }
                            }
                        }
                    }
                }
            }
        }

        /* 270 degree rotation is equivalent to
         *   1. Horizontal mirroring;
         *   2. Transposing the image.
         * These two steps are merged into a single processing routine.
         */
        private static void do_rot_270(
            jpeg_decompress_struct srcinfo, jpeg_compress_struct dstinfo, JDIMENSION x_crop_offset,
            JDIMENSION y_crop_offset, jvirt_array<JBLOCK>[] src_coef_arrays,
            jvirt_array<JBLOCK>[] dst_coef_arrays)
        {
            /* Because of the horizontal mirror step, we can't process partial iMCUs
             * at the (output) bottom edge properly.  They just get transposed and
             * not mirrored.
             */
            JDIMENSION MCU_rows = srcinfo.Output_width /
                (dstinfo.m_max_v_samp_factor * dstinfo.min_DCT_v_scaled_size);

            for (int ci = 0; ci < dstinfo.Num_components; ci++)
            {
                var compptr = dstinfo.Component_info[ci];
                JDIMENSION comp_height = MCU_rows * compptr.V_samp_factor;
                JDIMENSION x_crop_blocks = x_crop_offset * compptr.H_samp_factor;
                JDIMENSION y_crop_blocks = y_crop_offset * compptr.V_samp_factor;
                for (JDIMENSION dst_blk_y = 0;
                    dst_blk_y < compptr.height_in_blocks;
                    dst_blk_y += compptr.V_samp_factor)
                {
                    var dst_buffer = dst_coef_arrays[ci].Access(dst_blk_y, compptr.V_samp_factor);
                    for (int offset_y = 0; offset_y < compptr.V_samp_factor; offset_y++)
                    {
                        for (JDIMENSION dst_blk_x = 0;
                            dst_blk_x < compptr.Width_in_blocks;
                            dst_blk_x += compptr.H_samp_factor)
                        {
                            var src_buffer = src_coef_arrays[ci].Access(
                                dst_blk_x + x_crop_blocks, compptr.H_samp_factor);
                            for (int offset_x = 0; offset_x < compptr.H_samp_factor; offset_x++)
                            {
                                var dst_ptr = dst_buffer[offset_y][dst_blk_x + offset_x];
                                if (y_crop_blocks + dst_blk_y < comp_height)
                                {
                                    /* Block is within the mirrorable area. */
                                    var src_ptr = src_buffer[offset_x][comp_height - y_crop_blocks - dst_blk_y - offset_y - 1];
                                    for (int i = 0; i < JpegConstants.DCTSIZE; i++)
                                    {
                                        for (int j = 0; j < JpegConstants.DCTSIZE; j++)
                                        {
                                            dst_ptr[j * JpegConstants.DCTSIZE + i] =
                                                src_ptr[i * JpegConstants.DCTSIZE + j];

                                            j++;
                                            
                                            dst_ptr[j * JpegConstants.DCTSIZE + i] =
                                                (short)-src_ptr[i * JpegConstants.DCTSIZE + j];
                                        }
                                    }
                                }
                                else
                                {
                                    /* Edge blocks are transposed but not mirrored. */
                                    var src_ptr = src_buffer[offset_x][dst_blk_y + offset_y + y_crop_blocks];
                                    for (int i = 0; i < JpegConstants.DCTSIZE; i++)
                                    {
                                        for (int j = 0; j < JpegConstants.DCTSIZE; j++)
                                        {
                                            dst_ptr[j * JpegConstants.DCTSIZE + i] =
                                                src_ptr[i * JpegConstants.DCTSIZE + j];
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        /* Reflect - drop content of specified area, similar to wipe, but
         * fill with repeated reflections of the outside area, instead of zero.
         * NB: y_crop_offset is assumed to be zero.
         */
        private static void do_reflect(
            jpeg_decompress_struct srcinfo, jpeg_compress_struct dstinfo, JDIMENSION x_crop_offset,
            jvirt_array<JBLOCK>[] src_coef_arrays, JDIMENSION drop_width, JDIMENSION drop_height)
        {
            throw new NotImplementedException();
        }

        /* Flatten - drop content of specified area, similar to wipe,
         * but fill with average of adjacent blocks, instead of zero.
         */
        private static void do_flatten(
            jpeg_decompress_struct srcinfo, jpeg_compress_struct dstinfo, JDIMENSION x_crop_offset,
            JDIMENSION y_crop_offset, jvirt_array<JBLOCK>[] src_coef_arrays, JDIMENSION drop_width,
            JDIMENSION drop_height)
        {
            throw new NotImplementedException();
        }

        /* Wipe - drop content of specified area, fill with zero (neutral gray) */
        private static void do_wipe(
            jpeg_decompress_struct srcinfo, jpeg_compress_struct dstinfo, JDIMENSION x_crop_offset,
            JDIMENSION y_crop_offset, jvirt_array<JBLOCK>[] src_coef_arrays, JDIMENSION drop_width,
            JDIMENSION drop_height)
        {
            throw new NotImplementedException();
        }

        /* Drop.  If the dropinfo component number is smaller than the destination's,
         * we fill in the remaining components with zero.  This provides the feature
         * of dropping grayscale into (arbitrarily sampled) color images.
         */
        private static void do_drop(
            jpeg_decompress_struct srcinfo, jpeg_compress_struct dstinfo, JDIMENSION x_crop_offset,
            JDIMENSION y_crop_offset, jvirt_array<JBLOCK>[] src_coef_arrays,
            jpeg_decompress_struct dropinfo, jvirt_array<JBLOCK>[] drop_coef_arrays,
            JDIMENSION drop_width, JDIMENSION drop_height)
        {
            throw new NotImplementedException();
        }

        /* Copy a row of coefficient blocks from one place to another. */
        private static void jcopy_block_row(
            JBLOCK[] input_row, int inputOffset, JBLOCK[] output_row, int outputOffset, JDIMENSION num_blocks)
        {
            for (int i = 0; i < num_blocks; i++)
            {
                var input = input_row[inputOffset + i];
                var output = output_row[outputOffset + i];
                Array.Copy(input.data, output.data, JpegConstants.DCTSIZE2);
            }
        }
    }
}