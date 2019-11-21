using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI.Extensions;

public class GameManager : MonoBehaviour
{
    //A UILineRenderer stored on a Prefab - used to draw connection lines
    public GameObject Connection;

    //The Nodes on the board
    public List<Node> Nodes;

    //The Maximum additional vertices we allow the game to generate
    public int MaxAdditionalConnections = 3;

    //How many connections there are
    int Vertices;

    //The total amount of money on the board
    int TotalCurrency;

    public Animator anim;

    void Start()
    {
        FindNodes();
        MainConnection();

        int AdditionalConnections = Random.Range(0, MaxAdditionalConnections);

        for (int i = 0; i < AdditionalConnections; i++)
        {
            AdditionalConnection();
        }

        CheckCurrency();        
    }
    
    /// <summary>
    /// Finds all nodes Objects in the scene, adds them Nodes List and then assigns them a random score
    /// </summary>
    void FindNodes()
    {
        Node[] nodes = GameObject.FindObjectsOfType<Node>();
        foreach (Node node in nodes)
        {
            Nodes.Add(node);
            node.RandomScore();
            node.gameManager = this;
        }
    }

    /// <summary>
    /// Goes through each node and creates connections ensuring a path runs between the first and last node
    /// </summary>
    void MainConnection()
    {
        //Duplicates the Nodes List
        List<Node> ConnectNodes = new List<Node>(Nodes);
        //Then shuffles the list
        ConnectNodes.Shuffle();

        //Creates a connection gameObject
        GameObject connection = Instantiate(Connection, transform);
        //Sets the number of points in the line to be equal to the number of nodes
        connection.GetComponent<UILineRenderer>().Points = new Vector2[ConnectNodes.Count];

        for (int i = 0; i < ConnectNodes.Count; i++)
        {
            //Set the next point in the line to be the central point of the next node.
            connection.GetComponent<UILineRenderer>().Points[i].Set(ConnectNodes[i].GetComponent<RectTransform>().localPosition.x, ConnectNodes[i].GetComponent<RectTransform>().localPosition.y);

            //This runs only after the first node in the sequence has done its thing - It grabs the previous node and says "hey we are connected!"
            if (i > 0)
                {ConnectNodes[i].Connections.Add(ConnectNodes[i - 1]);}

            //If its the not the last node in the sequence - it grabs the next node and says "hey friend, we are connected!"
            if (i < ConnectNodes.Count-1)
                {ConnectNodes[i].Connections.Add(ConnectNodes[i+1]);}
        }

        //Counts the numbers of current connections
        Vertices = Nodes.Count-1;
    }

    /// <summary>
    /// Adds additional connections between the nodes
    /// </summary>
    void AdditionalConnection()
    {
        //Create a new connection from the prefab
        GameObject connection = Instantiate(Connection, transform);
        //This connection only ever joins two nodes together
        connection.GetComponent<UILineRenderer>().Points = new Vector2[2];

        //Select a random node from the Node List
        Node randomNode = Nodes[Random.Range(0, Nodes.Count)];
        //Set the connections first point to be at this nodes centre
        connection.GetComponent<UILineRenderer>().Points[0].Set(randomNode.GetComponent<RectTransform>().localPosition.x, randomNode.GetComponent<RectTransform>().localPosition.y);

        //Selects a random node, which is not already connected to this node
        Node choosenNode = ChooseConnection(randomNode);
        //Set the connections second point to be at this nodes centre
        connection.GetComponent<UILineRenderer>().Points[1].Set(choosenNode.GetComponent<RectTransform>().localPosition.x, choosenNode.GetComponent<RectTransform>().localPosition.y);

        //Tell each node that they are now connected to one another
        randomNode.Connections.Add(choosenNode);
        choosenNode.Connections.Add(randomNode);

        //Theres now an extra connection/vertices on the board
        Vertices++;
    }

    /// <summary>
    /// Check there is enough currency to solve the board
    /// </summary>
    void CheckCurrency()
    {
        //Count the current total currency on the board - remember this has been assigned randomly during the main connection stage
        foreach (Node node in Nodes)
            {TotalCurrency += node.score;}

        //If the TotalCurrency is smaller than the Genus the game remains unsolvable - therefore we need to allocate additional currency
        if (TotalCurrency < Genus())
        {
            //First we calculate how much additional currency we need to add
           int Allocate = (Genus()-TotalCurrency);

            //Then we randomly distribute the money to the nodes
            for (int i = 0; i < Allocate; i++)
            {
                Nodes[Random.Range(0, Nodes.Count)].score += 1;
                TotalCurrency++;
            }
        }
    }

    //The graphs genus
    int Genus()
    {
        //Genus = Edges - Vertices + 1
        return Nodes.Count - Vertices + 1;
    }

    /// <summary>
    /// Picks a random node to connect to
    /// </summary>
    Node ChooseConnection(Node selected)
    {
        Node choosen = null;

        //Ensures it hasn't randomly selected itself or a node its already connected too
        while ( choosen == null || choosen == selected || selected.Connections.Contains(choosen))
        {
            choosen = (Nodes[Random.Range(0, Nodes.Count)]);
        }

        //Return the random node
        return choosen;
    }

    //Check if the board has been completed
    public void CheckForCompletion()
    {
        bool complete = true;

        foreach (Node node in Nodes)
        {
            if (node.score < 0)
                {complete = false; }
        }

        if (complete)
            {
                anim.SetTrigger("Victory");
                Invoke("ClearGame", 2f);
            }
    }

    /// <summary>
    /// Clears the board and starts a new game
    /// </summary>
    public void ClearGame()
    {
        foreach (Node node in Nodes)
            {node.ClearNode();}

        foreach (Transform child in transform)
            {Destroy(child.gameObject);}

        Nodes.Clear();

        Vertices = TotalCurrency = 0;
        Start();
    }
}

public static class IListExtensions
{
    /// <summary>
    /// Shuffles the element order of the specified list.
    /// </summary>
    public static void Shuffle<T>(this IList<T> ts)
    {
        var count = ts.Count;
        var last = count - 1;
        for (var i = 0; i < last; ++i)
        {
            var r = UnityEngine.Random.Range(i, count);
            var tmp = ts[i];
            ts[i] = ts[r];
            ts[r] = tmp;
        }
    }
}