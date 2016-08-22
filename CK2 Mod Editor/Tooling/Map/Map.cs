using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CK2_Mod_Editor.Tooling.Map
{
    class Map
    {

        public int v_pixels,h_pixels;
        public Pixel[,] pixelMap;
        public Dictionary<string, Dictionary<string, MapEntity>> entityDict = new Dictionary<string, Dictionary<string, MapEntity>>();

        public Map(string mode)
        {
            string map_path = @"D:\Documents\backups\Documents\mod_victarion\vicatuprov.jpg";
            constructMap(map_path);
            outputParsedMap();
        }

        public void constructMap(string imagePath)
        {

            Bitmap mapImage = new Bitmap(imagePath);

            pixelMap = getMapPixels(mapImage);

            Map_Scripting.GetProvinces gpinst = new Map_Scripting.GetProvinces(this);
            List<MapEntity> provinces = gpinst.provinces;

            Debug.WriteLine(provinces.Count);
            Debug.WriteLine(this.entityDict["Province"].Count);
        }

        public bool IsInMapRange(int x,int y)
        {
            //Debug.WriteLine("x: "+x + ",y: " + y);
            return (x >= 0 && x < h_pixels && y >= 0 && y < v_pixels);
        }

        public Pixel[,] getMapPixels(Bitmap mapImage)
        {

            Debug.WriteLine("Getting map pixels");

            var data = mapImage.LockBits(
                    new Rectangle(Point.Empty, mapImage.Size),
                    ImageLockMode.ReadWrite, mapImage.PixelFormat);
            var pixelSize = data.PixelFormat == PixelFormat.Format32bppArgb ? 4 : 3; // only works with 32 or 24 pixel-size bitmap!
            var padding = data.Stride - (data.Width * pixelSize);
            var bytes = new byte[data.Height * data.Stride];

            // copy the bytes from bitmap to array
            Marshal.Copy(data.Scan0, bytes, 0, bytes.Length);

            var index = 0;
            //var builder = new StringBuilder();
            v_pixels = data.Height;
            h_pixels = data.Width;
            //Debug.WriteLine("Vertical: " + v_pixels + "Horizontal: " + h_pixels);
            Pixel[,] mapPixels = new Pixel[h_pixels,v_pixels];
            int ctr = 0;
            for (var y = 0; y < v_pixels; y++)
            {
                for (var x = 0; x < h_pixels; x++)
                {
                    Color pixelColor = Color.FromArgb(
                        pixelSize == 3 ? 255 : bytes[index + 3], // A component if present
                        bytes[index + 2], // R component
                        bytes[index + 1], // G component
                        bytes[index]      // B component
                        );

                    mapPixels[x,y]=new Pixel(x, y, pixelColor.R, pixelColor.G, pixelColor.B, pixelColor.A);
                        

                    index += pixelSize;
                    ctr++;
                    if (ctr==((h_pixels*v_pixels)/2) || ctr == ((h_pixels * v_pixels) / 4) || ctr == (3*(h_pixels * v_pixels) / 4))
                    {
                        Debug.Write(String.Format("Getting pixels:{0}%\r", (((float)ctr / (float)(v_pixels * h_pixels)) * 100).ToString("#.##")));
                    }
                    


                }

                index += padding;
                
            }

            // copy back the bytes from array to the bitmap
            Marshal.Copy(bytes, 0, data.Scan0, bytes.Length);
            return mapPixels;

        }

        public void addEntities(List<MapEntity> entities)
        {
            foreach(MapEntity entity in entities)
            {
                if (!entityDict.ContainsKey(entity.category))
                {
                    Dictionary<string, MapEntity> catDict = new Dictionary<string, MapEntity>();
                    entityDict.Add(entity.category, catDict);
                }
                entityDict[entity.category].Add(entity.name, entity);
            }
        }

        public class Pixel
        {
            public int x_coord, y_coord;
            public int r, g, b,a;
            public bool isBlack;
            public List<int> coord = new List<int>();
            public List<int> color = new List<int>();
            public PixelBlock assignedPixelBlock = null;

            public Pixel(int _x, int _y, int _r, int _g, int _b,int _a)
            {
                x_coord = _x;
                y_coord = _y;
                r = _r;
                g = _g;
                b = _b;
                a = _a;
                coord.Add(x_coord);
                coord.Add(y_coord);
                color.Add(r);
                color.Add(g);
                color.Add(b);
                color.Add(a);
                isBlack = _isBlack(r, g, b);


            }

            bool _isBlack(int r,int g,int b)
            { 
                return (r < 50 && g < 50 && b < 50);
            }

            public void assignToPixelBlock(PixelBlock pixelblock)
            {
                assignedPixelBlock = pixelblock;
            }
        }

        public class PixelBlock
        {
            public List<Pixel> edgeblock = new List<Pixel>();
            public List<Pixel> blocks = new List<Pixel>();
            public MapEntity provinceEntity;

            public PixelBlock()
            {

            }

            public PixelBlock(Pixel origPixel)
            {
                origPixel.assignToPixelBlock(this);
                //block.Add(origPixel);
                
            }

            public void addPixel(Pixel newPixel)
            {
                blocks.Add(newPixel);
            }
            public void removePixel(Pixel pixelToRemove)
            {
                blocks.Remove(pixelToRemove);
            }

            public void addEdgePixel(Pixel edgePixel)
            {
                edgeblock.Add(edgePixel);
            }


        }

        public class MapEntity
        {
            public string name;
            public string category;
            public int[] color;
            public PixelBlock associatedBlock;
            public Dictionary<string, Dictionary<string, MapEntity>> connectedEntities = new Dictionary<string, Dictionary<string, MapEntity>>();

            public MapEntity(string _name,string _category, PixelBlock _associatedBlock)
            {
                name = _name;
                category = _category;
                associatedBlock = _associatedBlock;
            }
            public MapEntity(string _name, string _category, int[] _color, PixelBlock _associatedBlock)
            {
                name = _name;
                category = _category;
                color = _color;
                associatedBlock = _associatedBlock;
            }
            public void connectEntity(MapEntity entityToConnect)
            {
                if (connectedEntities.ContainsKey(entityToConnect.category))
                {
                    if (!connectedEntities[entityToConnect.category].ContainsKey(entityToConnect.name))
                    {
                        connectedEntities[entityToConnect.category].Add(entityToConnect.name, entityToConnect);
                    }
                } else
                {
                    Dictionary<string, MapEntity> innerDictionary = new Dictionary<string, MapEntity>();
                    innerDictionary.Add(entityToConnect.name, entityToConnect);
                    connectedEntities.Add(entityToConnect.category, innerDictionary);
                }

                entityToConnect.connectEntity(this);
                
            }
        }

        public void outputParsedMap()
        {
            var b = new Bitmap(h_pixels, v_pixels);
            for (int x = 0; x < h_pixels; x++)
            {
                for (int y = 0; y < v_pixels; y++)
                {
                    if (pixelMap[x, y].assignedPixelBlock != null)
                    {
                        if(pixelMap[x, y].assignedPixelBlock.provinceEntity != null)
                        {
                            b.SetPixel(x, y, createPixel(pixelMap[x, y].assignedPixelBlock.provinceEntity.color));
                        }
                    }
                    
                }
            }
            b.Save("C:/Users/Joe/parsedmap.png");

        }

        Color createPixel(int[] pixArray)
        {
            return Color.FromArgb(pixArray[0], pixArray[1], pixArray[2]);
        }
    }

}
