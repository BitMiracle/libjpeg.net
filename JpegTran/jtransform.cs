using System;

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
            throw new NotImplementedException();
        }

        /* Copy markers saved in the given source object to the destination object.
         * This should be called just after jpeg_start_compress() or
         * jpeg_write_coefficients().
         * Note that those routines will have written the SOI, and also the
         * JFIF APP0 or Adobe APP14 markers if selected.
         */
        public static void jcopy_markers_execute(jpeg_decompress_struct srcinfo, jpeg_compress_struct dstinfo)
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
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
    }
}