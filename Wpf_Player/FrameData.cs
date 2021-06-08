using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wpf_Player
{
    class FrameData : ICloneable
    {
        public string ID;
        public int width;
        public int height;
        public int chunkSize;
        public ulong dataLength;
        int headerLen = 5;
        int sendSize = 500;

        public byte[][] imageData;

        public FrameData() { }
        public FrameData(byte[] chunk)
        {
            // Prepare info for new header.
            ID = BitConverter.ToString(new byte[] { chunk[1], chunk[2] }).Replace("-", string.Empty);
            chunkSize = (chunk[4] * 255) + chunk[3];
            width = (chunk[9] * 255) + chunk[8];
            height = (chunk[11] * 255) + chunk[10];
            dataLength = (ulong)(((chunk[7] * 255) * 255) + (chunk[6] * 255) + chunk[5]);
            // Prepare for new image data.
            imageData = new byte[chunkSize][];
        }
        public object Clone()
        {
            return new FrameData()
            {
                ID = this.ID,
                chunkSize = this.chunkSize,
                width = this.width,
                height = this.height,
                dataLength = this.dataLength,
                // Prepare for new image data.
                imageData = this.imageData
            };
        }

        public void AddChunk(byte[] chunk)
        {
            int index = (chunk[4] * 255) + chunk[3];
            // Define and copy the data to that part.
            imageData[index] = chunk;
        }

        // TODO: Check
        public byte[] BuildBitmapRaw()
        {
            // Build a new array, for the combined parts.
            byte[] compressSize = new byte[dataLength];
            foreach (byte[] bytePart in this.imageData)
            {
                if (bytePart != null)
                {
                    int index = (bytePart[4] * 255) + bytePart[3];
                    // Copy the usefull data to the tmp array.
                    byte[] tmpArr = new byte[bytePart.Length - headerLen]; //byte[10000];
                    Array.Copy(bytePart, headerLen, tmpArr, 0, bytePart.Length - headerLen);
                    //tmpArr = DecompressBitmap(tmpArr);
                    Array.Copy(tmpArr, 0, compressSize, index * sendSize, tmpArr.Length);
                    //Array.Copy(bytePart, headerLen, compressSize, index * 1000, bytePart.Length - headerLen);
                }
            }
            return compressSize;
        }
    }
}
