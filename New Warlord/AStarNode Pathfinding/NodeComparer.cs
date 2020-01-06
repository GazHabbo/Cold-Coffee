using System;
using System.Collections ;
namespace Warlord
{
	
	public class NodeComparer:IComparer
	{
		public NodeComparer()
		{
			
		}
	
		public int Compare(object x, object y)
		{
			return ((Node)x).totalCost -  ((Node) y).totalCost ;
		}
	}

    public class MapClass
    {
        public int[,] Mapdata;

        public MapClass(int[,] ThMap)
        {
            Mapdata = ThMap;
        }

        

        public int getMap(int x, int y)
        {
            int yMax = Mapdata.GetUpperBound(0);
            int xMax = Mapdata.GetUpperBound(1);

            if (x < 0 || x > xMax)
                return -1;
            else if (y < 0 || y > yMax)
                return -1;
            else
                return Mapdata[x, y];
        }
    }
}
