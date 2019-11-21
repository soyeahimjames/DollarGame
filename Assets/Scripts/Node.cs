using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class Node : MonoBehaviour, IPointerClickHandler
{
    public int score { get { return _score; } set { _score = value; UpdateUI(); } }
    int _score;
    public TextMeshProUGUI txValue;

    public List<Node> Connections;

    public GameObject transferIcon;

    public GameManager gameManager;

    //Give this node a random amount of currency
    public void RandomScore()
    {
        score = Random.Range(-3, 4);
    }

    //Handle left and right mouse clicks
    public void OnPointerClick(PointerEventData eventData)
    {
        //Left Click will send currency to all connected nodes
        if (eventData.button == PointerEventData.InputButton.Left)
            {Send();}
        //Right Click will take currency from all connected nodes
        else if (eventData.button == PointerEventData.InputButton.Right)
            {Receive();}
    }

    //Update its txValue to show its current currancy to the user
    void UpdateUI()
    {
        txValue.text = score.ToString();
    }

    //Send money to all connected nodes
    public void Send()
    {
        foreach (Node node in Connections)
        {
            //Remove a piece of currency
            score--;
            //Visually show the currency and set its start position as this nodes central point
            GameObject g = Instantiate(transferIcon,transform.parent);
            g.GetComponent<RectTransform>().localPosition = GetComponent<RectTransform>().localPosition;
            StartCoroutine(Transfer(g,node));
        }
    }

    //Take money from all connected nodes
    public void Receive()
    {
        foreach (Node node in Connections)
        {
            //Remove a piece of currency from the other nodes
            node.score--;
            //Visually show the currency and set its start position as the nodes central point
            GameObject g = Instantiate(transferIcon, transform.parent);
            g.GetComponent<RectTransform>().localPosition = node.GetComponent<RectTransform>().localPosition;
            StartCoroutine(Transfer(g,this));
        }
    }
    
    //Move money across the board from one node to another
    IEnumerator Transfer(GameObject g, Node destinationNode)
    {
        Vector2 destination = destinationNode.transform.localPosition;

        float percent = 0;

        while (percent <= 1)
        {
            percent += Time.deltaTime / 1.5f;
            g.transform.localPosition = Vector2.MoveTowards(g.transform.localPosition, destination,Vector2.Distance(g.transform.localPosition,destination)/20f);
            yield return null;
        }
        Destroy(g);
        destinationNode.score++;
        CheckForCompletion();
    }

    void CheckForCompletion()
    {
        gameManager.CheckForCompletion();
    }

    //Reset the node - used when resetting the game
    public void ClearNode()
    {
        score = 0;
        Connections.Clear();
    }
}

