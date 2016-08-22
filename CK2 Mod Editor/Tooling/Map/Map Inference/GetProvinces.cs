using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK2_Mod_Editor.Tooling.Map.Map_Scripting
{
    class GetProvinces
    {


        public List<Map.PixelBlock> pixelblocks = new List<Map.PixelBlock>();
        public List<Map.MapEntity> provinces = new List<Map.MapEntity>();
        public Map connectedMap;

        public GetProvinces(Map map)
        {
            Debug.WriteLine("Beginning construction of provinces");
            connectedMap = map;
            getPixelBlocks();
            createEntities();
            attachEntities();
        }

        bool isInMapRange(int x,int y)
        {
            return (x >= 0 && x < connectedMap.h_pixels && y >= 0 && y < connectedMap.v_pixels);
        }

        void getPixelBlocks() {

            Debug.WriteLine("Constructing Provinces");
            int ctr = 0;
            for (int x = 1; x < connectedMap.h_pixels; x++)
            {
                for (int y = 1; y < connectedMap.v_pixels; y++)
                {
                    if (connectedMap.pixelMap[x, y].assignedPixelBlock == null && (!connectedMap.pixelMap[x, y].isBlack))
                    {

                        //connectedMap.pixelMap[x, y]
                        Map.PixelBlock currentPixelBlock = new Map.PixelBlock();
                        currentPixelBlock = getPixelBlock(connectedMap.pixelMap[x, y], currentPixelBlock);
                        if (currentPixelBlock.edgeblock.Count > 16)
                        {
                            pixelblocks.Add(currentPixelBlock);
                        }
                    }
                    ctr++;
                    if (ctr == ((connectedMap.h_pixels * connectedMap.v_pixels) / 2) || ctr == ((connectedMap.h_pixels * connectedMap.v_pixels) / 4) || ctr == (3 * (connectedMap.h_pixels * connectedMap.v_pixels) / 4))
                    {
                        Debug.Write(String.Format("Constructing Provinces:{0}%   \r", ((float)ctr / (float)(connectedMap.h_pixels * connectedMap.v_pixels) * 100)));
                    }
                        
                }
            }
        }

        Map.PixelBlock getPixelBlock(Map.Pixel pixel,Map.PixelBlock pixelBlock)
        {

            //pixelBlock.addPixel(pixel);
            //pixel.assignToPixelBlock(pixelBlock);

            Queue<Map.Pixel> queue = new Queue<Map.Pixel>();
            queue.Enqueue(pixel);

            while (queue.Count > 0)
            {
                Map.Pixel curPixel = queue.Dequeue();
                curPixel.assignToPixelBlock(pixelBlock);

                for (int y = curPixel.y_coord - 1; y <= curPixel.y_coord + 1; y++)  
                {
                    for (int x = curPixel.x_coord - 1; x <= curPixel.x_coord + 1; x++)
                    {
                        if(connectedMap.IsInMapRange(x, y))
                        {
                            if (connectedMap.pixelMap[x, y].assignedPixelBlock == null && !connectedMap.pixelMap[x, y].isBlack)
                            {
                                //Debug.WriteLine("Connected Pixel Found");
                                queue.Enqueue(connectedMap.pixelMap[x, y]);
                                connectedMap.pixelMap[x, y].assignToPixelBlock(pixelBlock);
                                //pixelBlock.addPixel(pixel);

                                //

                            }
                            else if (connectedMap.pixelMap[x, y].assignedPixelBlock == null && connectedMap.pixelMap[x, y].isBlack)
                            {
                                //Debug.WriteLine("Edge Pixel Found");
                                pixelBlock.addEdgePixel(connectedMap.pixelMap[x, y]);
                                //curPixel.assignToPixelBlock(pixelBlock);
                            }
                        }
                        //Debug.WriteLine(curPixel.assignedPixelBlock);
                        //Debug.WriteLine(connectedMap.IsInMapRange(x,y)+" "+curPixel.isBlack);
                        

                    }
                }
            }
            return pixelBlock;
        }

        void createEntities()
        {
            Dictionary<int, int[]> colorDict = new Dictionary<int, int[]>();
            int colorCtr=0;
            for (int r = 84; r <= 255; r += 57)
            {
                colorDict.Add(colorCtr, new int[3] { r, 0, 0 });
                colorCtr++;
            }
            for (int g = 84; g <= 255; g += 57)
            {
                colorDict.Add(colorCtr, new int[3] { 0, g, 0 });
                colorCtr++;
            }
            for (int b = 84; b <= 255; b += 57)
            {
                colorDict.Add(colorCtr, new int[3] { 0, 0, b });
                colorCtr++;
            }

                        

            int blockColor;
            Debug.WriteLine("Creating entities");
            int ctr = 0;
            foreach(Map.PixelBlock pixelBlock in pixelblocks)
            {
                
                string name = String.Format("{0}", ctr);
                //Debug.WriteLine(name + ", size:" + pixelBlock.edgeblock.Count());
                Map.MapEntity provinceEntity = new Map.MapEntity(name, "Province", colorDict[ctr % 10], pixelBlock);
                pixelBlock.provinceEntity = provinceEntity;
                provinces.Add(provinceEntity);
                Debug.Write(String.Format("{0}%\r",(float)((float)ctr/(float)pixelblocks.Count)*100));
                ctr++;

            }
        }
        void attachEntities()
        {
            connectedMap.addEntities(provinces);
        }

    }
}
