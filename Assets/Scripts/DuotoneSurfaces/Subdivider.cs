using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Subdivider 
{

    public static Mesh getSubdividedMesh(Mesh inputMesh, int subdivisionsBeforeColoring, int subdivisionsAfterColoring, bool changePostitionsOfOriginalVertices)
    {
        //Convert Mesh into MeshModel with Lists for Triangles, Edges and Nodes
        TriangularMeshModel tmeshModel = extractModelFromMesh(inputMesh);


        //Subdivide the MeshModel by Catmull-Clark
        MeshModel qmeshModel = subdivideMeshToQuadMesh(tmeshModel, false, changePostitionsOfOriginalVertices);
        for (int i = 0; i < subdivisionsBeforeColoring - 1; i++)
        {
            qmeshModel = subdivideMeshToQuadMesh(qmeshModel, false, changePostitionsOfOriginalVertices);
        }

        //Make two connected colorable areas
        qmeshModel = changeColors(qmeshModel);


        //Subdivide after Coloring
        for (int i = 0; i < subdivisionsAfterColoring; i++)
        {
            qmeshModel = subdivideMeshToQuadMesh(qmeshModel, true, changePostitionsOfOriginalVertices);
        }


        //Convert MeshModel back to Mesh
        return convertQuadMeshModel2Mesh(qmeshModel, inputMesh);
    }




    static Mesh convertQuadMeshModel2Mesh(MeshModel meshModel, Mesh mesh)
    {
        Mesh newMesh = new Mesh();
        List<Vector3> newVertexMap = new List<Vector3>();
        List<Vector2> newUVMap = new List<Vector2>();


        foreach (Quad q in meshModel.shapeList)
        {

            newVertexMap.Add(q.nodes[0].position);
            newVertexMap.Add(q.nodes[1].position);
            newVertexMap.Add(q.nodes[2].position);
            newVertexMap.Add(q.nodes[3].position);

            foreach(Vector2 uv in q.getUVs())
            {
                newUVMap.Add(uv);
            }

        }

        mesh.SetVertices(newVertexMap);
        mesh.SetUVs(0, newUVMap);


        var indices = new int[newVertexMap.Count];
        for (var i = 0; i < indices.Length; i++)
        {
            indices[i] = i;
        }

        mesh.SetIndices(indices, MeshTopology.Quads, 0);

        mesh.RecalculateNormals();
        return mesh;

    }

    static MeshModel changeColors(MeshModel meshModel)
    {
        List<Node> yellowNodes = new List<Node>();
        List<int>[] adj; 

        //Extract Yellow Graph
        foreach(Shape s in meshModel.shapeList)
        {
            yellowNodes.Add(s.getYellowNodes()[0]);
            yellowNodes.Add(s.getYellowNodes()[1]);
            
        }

        adj = new List<int>[yellowNodes.Count];
        for (int i = 0; i < yellowNodes.Count; ++i)
            adj[i] = new List<int>();

        foreach (Shape s in meshModel.shapeList)
        {
            int positionOfNode1 = yellowNodes.IndexOf(s.getYellowNodes()[0]);
            int positionOfNode2 = yellowNodes.IndexOf(s.getYellowNodes()[1]);
            adj[positionOfNode1].Add(positionOfNode2);
            adj[positionOfNode2].Add(positionOfNode1);
        }


        List<int>[] newAdj = new List<int>[yellowNodes.Count];
        for (int i = 0; i < yellowNodes.Count; ++i)
            newAdj[i] = new List<int>();
        List<Node> visited = new List<Node>();
        newAdj = treeify(visited, newAdj, yellowNodes, adj, yellowNodes[0]);

        foreach (Shape s in meshModel.shapeList)
        {

            int positionOfNode1 = yellowNodes.IndexOf(s.getYellowNodes()[0]);
            int positionOfNode2 = yellowNodes.IndexOf(s.getYellowNodes()[1]);
            if (newAdj[positionOfNode1].Contains(positionOfNode2))
            {

                s.UVs.Add(new Vector2(0.5f, 0));
                s.UVs.Add(new Vector2(0.5f, 1));
                s.UVs.Add(new Vector2(1, 1));
                s.UVs.Add(new Vector2(1, 0));
            }
            else
            {
                s.UVs.Add(new Vector2(0, 0));
                s.UVs.Add(new Vector2(0, 1));
                s.UVs.Add(new Vector2(0.5f, 1));
                s.UVs.Add(new Vector2(0.5f, 0));
            }

        }

        return meshModel;
    }

    static List<int>[] treeify(List<Node> visited, List<int>[] newAdj, List<Node> yellowNodes, List<int>[] adj, Node n)
    {

         visited.Add(n);
         foreach (int child in adj[yellowNodes.IndexOf(n)])
            {
                if (!visited.Contains(yellowNodes[child]))
                {
                    visited.Add(yellowNodes[child]);
                    newAdj[child].Add(yellowNodes.IndexOf(n));
                    newAdj[yellowNodes.IndexOf(n)].Add(child);
                    newAdj = treeify(visited, newAdj, yellowNodes, adj, yellowNodes[child]);
                }
            }
        return newAdj;
    }

    static QuadMeshModel subdivideMeshToQuadMesh(MeshModel meshModel, bool calculateUVs, bool changePostitionsOfOriginalVertices)
    {
        //Perform CatmullClark Subdivion to Quads

        //Calculate Edge Midpoints
        foreach(Edge e in meshModel.edgeList)
        {
            List<Shape> shapes_containingEdge = new List<Shape>();
            foreach (Shape s in meshModel.shapeList)
            {
                if (s.containsEdge(e))
                {
                    shapes_containingEdge.Add(s);
                }
            }

            if (changePostitionsOfOriginalVertices) { 
            e.midPoint = new Node((((e.node1.position + e.node2.position) / 2) + ((shapes_containingEdge[0].midPoint.position + shapes_containingEdge[1].midPoint.position) / 2)) / 2, false);
            }
            else
            {
                e.midPoint = new Node((e.node1.position + e.node2.position) / 2, false);
            }
        }

        //Calculate new original Points positions
        foreach(Node n in meshModel.nodeList)
        {
            List<Shape> shapes_containingNode = new List<Shape>();
            foreach (Shape s in meshModel.shapeList)
            {
                if (s.containsNode(n))
                {
                    shapes_containingNode.Add(s);
                }
            }
            Vector3 averageMidPoint = new Vector3(0, 0, 0);
            foreach (Shape s in shapes_containingNode)
            {
                averageMidPoint += s.midPoint.position;
            }
            averageMidPoint = averageMidPoint / shapes_containingNode.Count;

            List<Edge> edges_containingNode = new List<Edge>();
            foreach (Edge e in meshModel.edgeList)
            {
                if (e.containsNode(n))
                {
                    edges_containingNode.Add(e);
                }
            }
            Vector3 averageEdgeMidPoint = new Vector3(0, 0, 0);
            foreach (Edge e in edges_containingNode)
            {
                averageEdgeMidPoint += (e.node1.position + e.node2.position) / 2;
            }
            averageEdgeMidPoint = averageEdgeMidPoint / edges_containingNode.Count;

            n.isyellow = true;
            if (changePostitionsOfOriginalVertices)
            {
                n.position = (averageMidPoint + 2 * averageEdgeMidPoint + (shapes_containingNode.Count - 3) * n.position) / shapes_containingNode.Count;
            }
        }


        return remeshToQuadModel(meshModel.shapeList, calculateUVs);
    }

    static QuadMeshModel remeshToQuadModel(List<Shape> shapes, bool calculateUVs)
    {

        List<Shape> shapeList = new List<Shape>();
        List<Edge> edgeList = new List<Edge>();
        List<Node> nodeList = new List<Node>();


       

        //remesh to quad net
        foreach (Shape s in shapes)
        {

            foreach (Node n in s.getNodes())
            {
                List<Edge> both = new List<Edge>();
                foreach (Edge e in s.edges)
                {
                    if (e.containsNode(n))
                    {
                        both.Add(e);
                    }
                }
                if (both[0] == s.edges[0] && both[1] == s.edges[s.edges.Count - 1])
                {
                    both[0] = s.edges[s.edges.Count - 1];
                    both[1] = s.edges[0];
                }




                Node n1 = addNodeToPool(both[0].midPoint, nodeList);
                Node n2 = addNodeToPool(n, nodeList);
                Node n3 = addNodeToPool(both[1].midPoint, nodeList);
                Node n4 = addNodeToPool(s.midPoint, nodeList);

                Edge e1 = addEdgeToPool(new Edge(n1, n2), edgeList);
                Edge e2 = addEdgeToPool(new Edge(n2, n3), edgeList);
                Edge e3 = addEdgeToPool(new Edge(n3, n4), edgeList);
                Edge e4 = addEdgeToPool(new Edge(n4, n1), edgeList);

                Quad q = new Quad(e1, e2, e3, e4, n1, n2, n3, n4);

                if (calculateUVs)
                {

                    int indexOfNodeN = s.getNodes().IndexOf(n);
                    int indexOfBoth1Point = indexOfNodeN < 3 ? indexOfNodeN + 1 : 0;
                    int indexOfBoth0Point = indexOfNodeN > 0 ? indexOfNodeN - 1 : 3;
                    List<Vector2> oldUVs = s.getUVs();


                    q.UVs.Add((oldUVs[indexOfNodeN] + oldUVs[indexOfBoth0Point]) / 2);
                    q.UVs.Add(oldUVs[indexOfNodeN]);
                    q.UVs.Add((oldUVs[indexOfNodeN] + oldUVs[indexOfBoth1Point]) / 2);
                    q.UVs.Add((oldUVs[indexOfBoth0Point] + oldUVs[indexOfBoth1Point]) / 2);
                }

                shapeList.Add(q);
            }

        }
        return new QuadMeshModel(shapeList, edgeList, nodeList);
    }



    static TriangularMeshModel extractModelFromMesh(Mesh mesh)
    {
        List<Shape> triangleList = new List<Shape>();
        List<Node> nodeList = new List<Node>();
        List<Edge> edgeList = new List<Edge>();


        for (int i = 0; i < mesh.triangles.Length; i += 3)
        {
            Node node1 = addNodeToPool(new Node(mesh.vertices[mesh.triangles[i]], true), nodeList);
            Node node2 = addNodeToPool(new Node(mesh.vertices[mesh.triangles[i+1]], true), nodeList);
            Node node3 = addNodeToPool(new Node(mesh.vertices[mesh.triangles[i+2]], true), nodeList);


            Edge edge1 = addEdgeToPool(new Edge(node1, node2), edgeList);
            Edge edge2 = addEdgeToPool(new Edge(node2, node3), edgeList);
            Edge edge3 = addEdgeToPool(new Edge(node3, node1), edgeList);

            Triangle triangle1 = new Triangle(edge1, edge2, edge3, node1, node2, node3);

            triangleList.Add(triangle1);
        }

        return new TriangularMeshModel(triangleList, edgeList, nodeList);
    }

    static Edge addEdgeToPool(Edge edge, List<Edge> pool)
    {
        if (!pool.Contains(edge))
        {
            pool.Add(edge);
            return edge;
        }
        else
        {
            return pool.Find(e => e.Equals(edge));
        }
    }

    static Node addNodeToPool(Node node, List<Node> pool)
    {
        if (!pool.Contains(node))
        {
            pool.Add(node);
            return node;
        }else
        {
            return pool.Find(n => n.Equals(node));
        }
    }

}






//Class Node
public class Node
{
    public Vector3 position;
    public bool isyellow;

    public Node(Vector3 pos, bool yellow)
    {
        position = pos;
        isyellow = yellow;
    }

    public bool Equals(Node node)
    {
        if (node.position == this.position && node.isyellow == this.isyellow)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public override bool Equals(object obj)
    {
        if (obj == null) return false;
        Node objAsNode = obj as Node;
        if (objAsNode == null) return false;
        else return Equals(objAsNode);
    }
}





//Class Edge
public class Edge
{
    public Node node1;
    public Node node2;
    public Node midPoint;
    public Edge(Node one, Node two)
    {
        node1 = one;
        node2 = two;
    }

    public bool Equals(Edge other)
    {
        if (other.node1.position == this.node1.position && other.node2.position == this.node2.position)
        {
            return true;
        } else if (other.node2.position == this.node1.position && other.node1.position == this.node2.position)
        {
            return true;
        } else
        {
            return false;
        }
    }

    public override bool Equals(object obj)
    {
        if (obj == null) return false;
        Edge objAsEdge = obj as Edge;
        if (objAsEdge == null) return false;
        else return Equals(objAsEdge);
    }

    public bool containsNode(Node node)
    {
        if (node1.Equals(node)|| node2.Equals(node))
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}




//Class Triangle
public class Triangle : Shape
{

    public Triangle(Edge one, Edge two, Edge three, Node none, Node ntwo, Node nthree)
    {
        edges = new List<Edge>();
        edges.Add(one);
        edges.Add(two);
        edges.Add(three);

        nodes = new List<Node>();
        nodes.Add(none);
        nodes.Add(ntwo);
        nodes.Add(nthree);


        Node n0 = this.getNodes()[0];
        Node n1 = this.getNodes()[1];
        Node n2 = this.getNodes()[2];

        midPoint = new Node((n0.position + n1.position + n2.position) / 3,  true);

        UVs = new List<Vector2>();
    }   

    public List<Edge> getEdges()
    {
        return this.edges;
    }

}


//Class Quad
public class Quad : Shape
{

    public Quad(Edge one, Edge two, Edge three, Edge four, Node none, Node ntwo, Node nthree, Node nfour)
    {
        edges = new List<Edge>();
        this.edges.Add(one);
        this.edges.Add(two);
        this.edges.Add(three);
        this.edges.Add(four);

        nodes = new List<Node>();
        this.nodes.Add(none);
        this.nodes.Add(ntwo);
        this.nodes.Add(nthree);
        this.nodes.Add(nfour);

        this.midPoint = new Node((none.position + ntwo.position + nthree.position + nfour.position) / 4, true);

        UVs = new List<Vector2>();
    }

}


//Parent Class Shape
public class Shape
{
    public List<Node> nodes;
    public List<Edge> edges;
    public Node midPoint;
    public List<Vector2> UVs;


    public bool containsEdge(Edge edge)
    {
        foreach(Edge e in edges)
        {
            if (e.Equals(edge))
            {
                return true;
            }
        }
         return false;
    }

    public bool containsNode(Node node)
    {
        return nodes.Contains(node);
    }

    public List<Node> getYellowNodes()
    {
        List<Node> yellowNodes = new List<Node>();

            foreach (Node n in this.nodes)
            {
                if (n.isyellow)
                {
                    yellowNodes.Add(n);
                }
            }
        return yellowNodes;
    }


    public List<Vector2> getUVs()
    {
        return this.UVs;
    }

    public List<Node> getNodes()
        {
            return this.nodes;
        }
    }


//Class TriangularMeshModel
public class TriangularMeshModel : MeshModel
{

    public TriangularMeshModel(List<Shape> triangleList, List<Edge> edgeList, List<Node> nodeList)
    {
        this.shapeList = triangleList;
        this.edgeList = edgeList;
        this.nodeList = nodeList;
    }
}



//Class QuadMeshModel
public class QuadMeshModel : MeshModel
{

    public QuadMeshModel(List<Shape> quadList, List<Edge> edgeList, List<Node> nodeList)
    {
        this.shapeList = quadList;
        this.edgeList = edgeList;
        this.nodeList = nodeList;
    }
}


//Parent Class MeshModel
public class MeshModel
{
    public List<Shape> shapeList;
    public List<Edge> edgeList;
    public List<Node> nodeList;
}
