using System;
using System.IO;
using UnityEngine;

namespace PNMtoBuffer
{
    public class PNMtoBufferedIntArray
    {

        public PNMIntArrayObject Compile(string filePath)
        {
            TextAsset file = Resources.Load(filePath) as TextAsset;
            Stream s = new MemoryStream(file.bytes);
            using (BinaryReader reader = new BinaryReader(s))
            {
                if (reader.ReadChar() == 'P')
                {
                    char c = reader.ReadChar();

                    switch (c)
                    {
                        case '1':
                            return ReadTextBitmapImage(reader);
                        case '2':
                            return ReadTextGreyscaleImage(reader);
                        case '3':
                            return ReadTextPixelImage(reader);
                        case '4':
                            return ReadBinaryBitmapImage(reader);
                        case '5':
                            return ReadBinaryGreyscaleImage(reader);
                        case '6':
                            return ReadBinaryPixelImage(reader);
                        default:
                            throw new FormatException("The PNM file is not formated as expected");
                    }
                }
            }
            return null;
        }

        private PNMIntArrayObject ReadBinaryPixelImage(BinaryReader reader)
        {
            throw new NotImplementedException();
        }

        private PNMIntArrayObject ReadBinaryGreyscaleImage(BinaryReader reader)
        {
            // create the object to be sent back
            PNMIntArrayObject output = new PNMIntArrayObject
            {
                // grab the two headers 
                Width = GetNextHeaderValue(reader),
                Height = GetNextHeaderValue(reader),

                // ensure the scalse is set to an invalid number
                Scale = GetNextHeaderValue(reader)
            };
            
            // Initalize the array
            output.Pixels = new int[output.Height * output.Width-1];

            for (int index = 0; index < (output.Height * output.Width) -1; index++)
            {
                output.Pixels[index] = reader.ReadByte() * 255 / output.Scale;
            }

            return output;
        }

        private PNMIntArrayObject ReadBinaryBitmapImage(BinaryReader reader)
        {
            throw new NotImplementedException();
        }

        private PNMIntArrayObject ReadTextPixelImage(BinaryReader reader)
        {
            throw new NotImplementedException();
        }

        private PNMIntArrayObject ReadTextGreyscaleImage(BinaryReader reader)
        {
            throw new NotImplementedException();
        }

        private PNMIntArrayObject ReadTextBitmapImage(BinaryReader reader)
        {
            throw new NotImplementedException();
        }


        private int GetNextHeaderValue(BinaryReader reader)
        {
            bool hasValue = false;
            string value = string.Empty;
            char c;
            bool comment = false;

            do
            {
                c = reader.ReadChar();

                if (c == '#')
                {
                    comment = true;
                }

                if (comment)
                {
                    if (c == '\n')
                    {
                        comment = false;
                    }

                    continue;
                }

                if (!hasValue)
                {
                    if ((c == '\n' || c == ' ' || c == '\t') && value.Length != 0)
                    {
                        hasValue = true;
                    }
                    else if (c >= '0' && c <= '9')
                    {
                        value += c;
                    }
                }

            } while (!hasValue);

            return int.Parse(value);
        }

        private int GetNextTextValue(BinaryReader reader)
        {
            string value = string.Empty;
            char c = reader.ReadChar();

            do
            {
                value += c;

                c = reader.ReadChar();

            } while (!(c == '\n' || c == ' ' || c == '\t') || value.Length == 0);

            try
            {
                return int.Parse(value);
            }
            catch (Exception ex)
            {
                return -1;
            }
        }
    }
}
