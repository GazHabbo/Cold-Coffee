using System;
using System.Collections;

namespace Warlord
{
    class Pathfinder
    {
        internal string[] GetPath(string Start, string End, int[,] Mapzy)
        {
            MapClass Map = new MapClass(Mapzy);
            ArrayList SolutionPathList = new ArrayList();
            int Startxlol = int.Parse(Start.Split(",".ToCharArray())[0]);
            int startylol = int.Parse(Start.Split(",".ToCharArray())[1]);
            int endxlol = int.Parse(End.Split(",".ToCharArray())[0]);
            int endylol = int.Parse(End.Split(",".ToCharArray())[1]);
            Node node_goal = new Node(null, null, 1, endxlol, endylol,Map);
            Node node_start = new Node(null, node_goal, 1, Startxlol, startylol,Map);
            SortedCostNodeList OPEN = new SortedCostNodeList();
            SortedCostNodeList CLOSED = new SortedCostNodeList();
            int Failures = 0;
            OPEN.push(node_start);

            while (OPEN.Count > 0)
            {
                Node node_current = OPEN.pop();
                if (node_current.isMatch(node_goal))
                {
                    node_goal.parentNode = node_current.parentNode;
                    break;
                }

                ArrayList successors = node_current.GetSuccessors();

                foreach (Node node_successor in successors)
                {
                    int oFound = OPEN.IndexOf(node_successor);
                    if (oFound > 0)
                    {
                        Node existing_node = OPEN.NodeAt(oFound);
                        if (existing_node.CompareTo(node_current) <= 0)
                            continue;
                    }
                    int cFound = CLOSED.IndexOf(node_successor);
                    if (cFound > 0)
                    {
                        Node existing_node = CLOSED.NodeAt(cFound);
                        if (existing_node.CompareTo(node_current) <= 0)
                            continue;
                    }

                    if (oFound != -1)
                    {
                        OPEN.RemoveAt(oFound);
                        if (Failures > 10000)
                        {
                            string[] FailureCords;
                           FailureCords =  new string[1];
                            return FailureCords;
                        }
                        Failures++;
                    }
                    if (cFound != -1)
                        CLOSED.RemoveAt(cFound);
                    OPEN.push(node_successor);
                    if (Failures > 10000)
                    {
                        string[] FailureCords;
                        FailureCords = new string[1];
                        return FailureCords;
                    }
                    Failures++;

                }
                CLOSED.push(node_current);
            }
            Node p = node_goal;

            int y = 0;

            Node b = p;
            while (b != null)
            {
                b = b.parentNode;
                y++;
            }
            string[] Movements = new string[y];
            y = 0;
            while (p != null)
            {

                SolutionPathList.Insert(0, p);
                Movements[y] = p.x + "," + p.y;
                p = p.parentNode;
                y++;
            }
            return Movements;
        }
    }
}
