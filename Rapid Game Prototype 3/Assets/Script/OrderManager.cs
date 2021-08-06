using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrderManager : MonoBehaviour
{
    public GameObject Customers;
    public GameObject orderMenu;

    // These are the different bits that make up the order
    public List<GameObject> orderPrefabs;

    public GameObject NewOrderObj()
    {
        // Make sure orders has the order template on the zeroth element with an OrderBehaviour script
        GameObject newOrder = Instantiate(orderPrefabs[0]);
        OrderBehaviour newOrderBehaviour = newOrder.GetComponent<OrderBehaviour>();

        int orderSize = Random.Range(1, 5);

        for (int i = 0; i < orderSize; i++)
        {
            GameObject newFood = Instantiate(orderPrefabs[Random.Range(1, orderPrefabs.Count)]);
            newFood.transform.SetParent(newOrderBehaviour.emptySlot.transform);
            newFood.transform.localPosition = Vector3.zero;
            newFood.transform.localScale = Vector3.one;
        }

        return newOrder;
        //return orders[Random.Range(0, orders.Count)];
    }

    private void Update()
    {
        if (orderMenu.transform.childCount > 0)
        {
            for (int i = 0; i < orderMenu.transform.childCount; i++)
            {
                OrderBehaviour order = orderMenu.transform.GetChild(i).GetComponent<OrderBehaviour>();

                if (order != null)
                {
                    if (order.bar.value >= order.bar.maxValue)
                    {
                        RemoveDisplayOrder(i);
                        GameManager.score--;
                    }
                }
            }
        }
    }

    public void RemoveDisplayOrder(int index)
    {
        if (orderMenu.transform.childCount >= index)
        {
            Destroy(orderMenu.transform.GetChild(index).gameObject);

            // Reset pos of orders
            for (int i = 0; i < orderMenu.transform.childCount; i++)
            {
                if (i < index)
                {
                    orderMenu.transform.GetChild(i).localPosition = new Vector3(-1050 + 300 * i, 500, 0);
                }
                else
                {
                    orderMenu.transform.GetChild(i).localPosition = new Vector3(-1050 + 300 * (i - 1), 500, 0);
                }

                if (orderMenu.transform.childCount > 6)
                {
                    if (i < index)
                    {
                        orderMenu.transform.GetChild(i).transform.localPosition = new Vector3(-1050 + (1200.0f / (orderMenu.transform.childCount - 2)) * (i), 500, 0);
                    }
                    else
                    {
                        orderMenu.transform.GetChild(i).transform.localPosition = new Vector3(-1050 + (1200.0f / (orderMenu.transform.childCount - 2)) * (i - 1), 500, 0);
                    }
                }
            }
        }
    }
}
    