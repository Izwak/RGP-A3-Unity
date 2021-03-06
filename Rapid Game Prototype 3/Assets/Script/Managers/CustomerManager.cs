using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CustomerType
{
    NONE,
    TAKEAWAY,
    DRIVETHRU,
    DINEIN
}
public class CustomerManager : MonoBehaviour
{
    public Transform customerParent;
    public GameObject customerPrefab;
    public List<GameObject> carPrefabs;
    public List<GameObject> pointsOfInterest;
    public List<GameObject> carPointsOfInterest;
    public List<GameObject> registers;


    public int tick = 0;

    // Start is called before the first frame update
    void Start()
    {

    }

    private void FixedUpdate()
    {
        if (GameManager.Instance.isRunning)
        {
            if (customerParent.childCount < GameManager.Instance.maxCustomers && tick % GameManager.Instance.customerSpawnRate == 1)
            {
                if (CountCars() < 5)
                {
                    CreateCustomer(Random.Range(0, 3));
                }
                else
                {
                    CreateCustomer(Random.Range(0, 2));
                }
            }
        }

        tick++;
    }

    int CountCars()
    {
        int count = 0;

        for (int i = 0; i < customerParent.childCount; i++)
        {
            GameObject obj = customerParent.GetChild(i).gameObject;
            CarController car = obj.GetComponent<CarController>();

            if (car != null)
            {
                count++;
            }
        }

        return count;
    }
    

    void CreateCustomer(int registerNum)
    {
        if (registerNum == 2)
        {
            GameObject newCustomer = Instantiate(carPrefabs[Random.Range(0, 5)]);
            newCustomer.transform.position = carPointsOfInterest[0].transform.position; // Set new customer to spawn pos
            newCustomer.GetComponent<CarController>().pointsOfInterest[0] = registers[registerNum];

            for (int i = 0; i < carPointsOfInterest.Count; i++)
            {
                newCustomer.GetComponent<CarController>().pointsOfInterest[i + 1] = carPointsOfInterest[i];
            }

            newCustomer.transform.SetParent(customerParent);
        }
        else
        {
            GameObject newCustomer = Instantiate(customerPrefab);
            newCustomer.transform.position = pointsOfInterest[0].transform.position; // Set new customer to spawn pos
            newCustomer.GetComponent<CustomerController1>().pointsOfInterest[0] = registers[registerNum];

            /*for (int i = 0; i < pointsOfInterest.Count; i++)
            {
                newCustomer.GetComponent<CustomerController1>().pointsOfInterest[i + 1] = pointsOfInterest[i];
            }*/

            newCustomer.GetComponent<CustomerController1>().pointsOfInterest[1] = pointsOfInterest[0];
            newCustomer.GetComponent<CustomerController1>().pointsOfInterest[3] = pointsOfInterest[3];

            bool send2SecondLine = Random.value > 0.5; // Lol i saw this worked somewhere all u need 2 know is that its a random bool

            if (send2SecondLine)
                newCustomer.GetComponent<CustomerController1>().pointsOfInterest[2] = pointsOfInterest[2];
            else
                newCustomer.GetComponent<CustomerController1>().pointsOfInterest[2] = pointsOfInterest[1];

            newCustomer.transform.SetParent(customerParent);
        }
    }
}
