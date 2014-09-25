﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BitMiracle.LibJpeg
{
    class DecompressorToJpegImage : IDecompressDestination
    {
        private JpegImage m_jpegImage;

        internal DecompressorToJpegImage(JpegImage jpegImage)
        {
            m_jpegImage = jpegImage;
        }

        public Stream Output
        {
            get
            {
                return null;
            }
        }

        public void SetImageAttributes(LoadedImageAttributes parameters)
        {
            m_jpegImage.Width = parameters.Width;
            m_jpegImage.Height = parameters.Height;
            m_jpegImage.BitsPerComponent = 8;
            m_jpegImage.ComponentsPerSample = (byte)parameters.ComponentsPerSample;
            m_jpegImage.Colorspace = parameters.Colorspace;
        }

        public void BeginWrite()
        {
        }

        public void ProcessPixelsRow(byte[] row)
        {
            SampleRow samplesRow = new SampleRow(row, m_jpegImage.Width, m_jpegImage.BitsPerComponent, m_jpegImage.ComponentsPerSample);
            m_jpegImage.addSampleRow(samplesRow);
        }

        public void EndWrite()
        {
        }
    }
}
