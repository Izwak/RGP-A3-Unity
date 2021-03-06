using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerBehaviours1 : MonoBehaviour
{
    public bool isRunning;
    public Joystick joystick;

    public GameObject empltySlot;

    public Transform target;

    Rigidbody body;

    float speed = 5;
    float angle;

    float holdCoolDown = 0;
    bool rewritethisbtr = false;

    AudioSource tempSound;

    RaycastHit hit;

    TouchPhase touchInteract = TouchPhase.Canceled;

    Vector2 tartgetPoint = new Vector2(0, -1);

    // Start is called before the first frame update
    void Start()
    {
        body = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        // If not running exit
        if (!isRunning)
            return;

        int test = Input.touchCount;


        if (Input.touches.Length > 0)
        {
            for (int i = 0; i < Input.touches.Length; i++)
            {
                Vector2 touchPos = Input.touches[i].position;

                //print("X: " + touchPos.x + ", Y: " + touchPos.y);


                if (touchPos.x > 1000)
                {
                    touchInteract = Input.touches[i].phase;
                    //isTouchInteract = true;
                    //print("Interact");
                    break;
                }
            }
        }

        tartgetPoint += new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        tartgetPoint += new Vector2(joystick.Horizontal, joystick.Vertical);

        angle = Mathf.Atan2(tartgetPoint.x, tartgetPoint.y) * Mathf.Rad2Deg;

        // Sets a position for the player to be looking at makes turning less jerky
        if (tartgetPoint.magnitude > speed / 2)
        {
            tartgetPoint = tartgetPoint.normalized * speed / 2;
        }

        body.velocity = Vector3.zero;
        body.velocity += new Vector3(Input.GetAxis("Horizontal") * speed, body.velocity.y, Input.GetAxis("Vertical") * speed);
        body.velocity += new Vector3(joystick.Horizontal * speed, body.velocity.y, joystick.Vertical * speed);

        transform.rotation = Quaternion.Euler(0, angle, 0);

        //print("Phase" + touchInteract);

        if (Input.GetButtonDown("Fire3") || touchInteract == TouchPhase.Began)
        {
            speed = 8;
        }

        if (Input.GetButtonUp("Fire3") || touchInteract == TouchPhase.Ended)
        {
            speed = 5;
        }

        LookingAtObjects();

        //if ((Input.GetButton("Interact")) && gameManager.isRunning)
        if ((Input.GetButton("Interact") || touchInteract == TouchPhase.Moved || touchInteract == TouchPhase.Stationary) && GameManager.Instance.isRunning)
        {
            holdCoolDown += Time.deltaTime;

            if (holdCoolDown >= 0.3 && !rewritethisbtr)
            {
                //print("Held");
                HoldInteractions();

                rewritethisbtr = true;
            }
        }
        //if ((Input.GetButtonUp("Interact")) && gameManager.isRunning)
        if ((Input.GetButtonUp("Interact") || touchInteract == TouchPhase.Ended) && GameManager.Instance.isRunning)
        {
            if (holdCoolDown < 0.3 && holdCoolDown != 0)
            {
                Interactions();
            }

            holdCoolDown = 0;
            rewritethisbtr = false;
        }
    }

    void RenderOutline(RaycastHit newhit)
    {
        if (hit.transform != null)
        {
            if (!newhit.Equals(hit))
            {
                Outline oldOutline = hit.transform.GetComponentInParent<Outline>();
                if (oldOutline != null)
                {
                    oldOutline.enabled = false;
                }
            }
        }

        hit = newhit;


        Outline outline = hit.transform.GetComponentInParent<Outline>();
        if (outline != null)
        {
            if (hit.distance < 2)
            //if (Vector3.Distance(transform.position, newhit.transform.position) <= 2)
            {
                outline.enabled = true;
            }
            else
            {
                outline.enabled = false;
            }
        }
    }

    void DisableOutline(RaycastHit newhit)
    {
        Outline outline = hit.transform.GetComponentInParent<Outline>();
        if (outline != null)
        {
            outline.enabled = false;
        }
    }

    void LookingAtObjects()
    {
        RaycastHit newhit;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out newhit, 1000))
        {
            target.position = newhit.point;
        }

        if (Physics.Raycast(transform.position - new Vector3(0, 0.7f, 0), transform.forward, out newhit))
        
        //if (Physics.Raycast(ray, out newhit, 1000))
        {

            // Deselect old Object if look new object is different
            if (hit.transform != null)
            {
                GameObject oldObj = hit.transform.parent.gameObject;

                if (!newhit.transform.Equals(hit.transform) && oldObj != null)
                {
                    Outline oldOutline = oldObj.GetComponent<Outline>();
                    Pointer pointer = oldObj.GetComponent<Pointer>();
                    DisplayIcon displayIcon = oldObj.GetComponent<DisplayIcon>();

                    if (oldOutline != null)
                        oldOutline.enabled = false;
                    if (pointer != null)
                        pointer.pointer.gameObject.SetActive(false);
                    if (displayIcon != null)
                        displayIcon.icon.gameObject.SetActive(false);

                    GameManager.Instance.screen.overlay.hideTip();
                }
            }

            Interact obj = newhit.transform.GetComponentInParent<Interact>();

            if (obj != null)
            {
                int holdingNum = empltySlot.transform.childCount;
                int counterHoldingNum = obj.emptySlot.transform.childCount;

                Pointer pointer = newhit.transform.GetComponentInParent<Pointer>();
                DisplayIcon displayIcon = newhit.transform.GetComponentInParent<DisplayIcon>();

                RenderOutline(newhit);

                // If objects have a icon whether or not it's displays
                if (displayIcon != null && newhit.distance < 2)
                //if (displayIcon != null && Vector3.Distance(transform.position, newhit.transform.position) <= 2)
                {
                    displayIcon.icon.gameObject.SetActive(true);

                    if (holdingNum > 0 || counterHoldingNum > 0)
                    {
                        displayIcon.icon.gameObject.SetActive(false);
                    }
                }

                // Interactable types
                if (obj.type == Interactables.NOTHING)
                {
                    DisableOutline(newhit);
                }
                if (obj.type == Interactables.COUNTER)
                {
                    if (holdingNum > 0 && !empltySlot.transform.GetChild(0).CompareTag("Food") && !empltySlot.transform.GetChild(0).CompareTag("Extinguisher") && !empltySlot.transform.GetChild(0).CompareTag("Tray"))
                    {
                        DisableOutline(newhit);
                    }

                    if (newhit.distance < 2)
                    {
                        // When u can pick up tray display take tray tip
                        if (counterHoldingNum > 0 && holdingNum == 0)
                        {
                            Tray tray = obj.emptySlot.transform.GetChild(0).GetComponent<Tray>();

                            if (tray != null)
                            {
                                GameManager.Instance.screen.overlay.waitThenDisplay("Hold Space to take tray");
                            }
                        }
                        // When u can pick up tray display take tray tip
                        else if (counterHoldingNum == 0 && holdingNum > 0)
                        {
                            Tray tray = empltySlot.transform.GetChild(0).GetComponent<Tray>();

                            if (tray != null)
                            {
                                GameManager.Instance.screen.overlay.waitThenDisplay("Hold Space to place tray");
                            }
                        }
                        else if (counterHoldingNum > 0 && holdingNum > 0)
                        {

                            Tray playerTray = empltySlot.transform.GetChild(0).GetComponent<Tray>();
                            Tray counterTray = obj.emptySlot.transform.GetChild(0).GetComponent<Tray>();

                            if (playerTray != null && counterTray != null)
                            {
                                GameOverlay overlay = GameManager.Instance.screen.overlay;

                                Dispencer dispencer = obj.GetComponent<Dispencer>();

                                if (dispencer != null && playerTray.emptySlot.transform.childCount + counterTray.emptySlot.transform.childCount == 0)
                                    overlay.waitThenDisplay("Hold Space to put back");
                                else if (dispencer != null && playerTray.emptySlot.transform.childCount > 0 && counterTray.emptySlot.transform.childCount == 0)
                                    overlay.waitThenDisplay("Hold Space to put back");
                                else
                                    overlay.waitThenDisplay("Hold Space to swap tray");
                            }
                        }
                    }
                }
                if (obj.type == Interactables.HOTPLATE)
                {
                    if (holdingNum > 0)
                    {
                        if (counterHoldingNum > 0 && obj.emptySlot.transform.GetChild(0).CompareTag("Raw Paddies"))
                        {
                            DisableOutline(newhit);
                        }
                        if (!empltySlot.transform.GetChild(0).CompareTag("Raw Paddies") && !empltySlot.transform.GetChild(0).CompareTag("Cooked Paddies") &&
                            !empltySlot.transform.GetChild(0).CompareTag("Burnt Paddies") && !empltySlot.transform.GetChild(0).CompareTag("Extinguisher"))
                        {
                            DisableOutline(newhit);
                        }
                        if (empltySlot.transform.GetChild(0).CompareTag("Raw Paddies") || empltySlot.transform.GetChild(0).CompareTag("Cooked Paddies") ||
                            empltySlot.transform.GetChild(0).CompareTag("Burnt Paddies"))
                        {
                            if (counterHoldingNum > 0)
                            {
                                if (empltySlot.transform.GetChild(0).tag != obj.emptySlot.transform.GetChild(0).tag)
                                {
                                    DisableOutline(newhit);
                                }
                                else if (holdingNum >= 2)
                                {
                                    DisableOutline(newhit);
                                }
                            }
                            else if (!empltySlot.transform.GetChild(0).CompareTag("Raw Paddies"))
                            {
                                DisableOutline(newhit);
                            }
                        }
                        if (empltySlot.transform.GetChild(0).CompareTag("Extinguisher")) {

                        }
                    }
                }
                if (obj.type == Interactables.FRIDGE)
                {
                    if (holdingNum > 0)
                    {
                        GameObject playerObj = empltySlot.transform.GetChild(0).gameObject;
                        Tray tray = playerObj.GetComponent<Tray>();

                        if (obj.createObject.CompareTag("Raw Paddies"))
                        {
                            if (playerObj.CompareTag("Raw Paddies"))
                            {
                                if (holdingNum >= 2)
                                {
                                    DisableOutline(newhit);
                                }
                            }
                            else
                            {
                                DisableOutline(newhit);
                            }
                        }
                        else if (obj.createObject.CompareTag("Food"))
                        {
                            if (tray != null)
                            {
                                if (tray.emptySlot.transform.childCount >= 4)
                                {
                                    DisableOutline(newhit);
                                }
                            }
                            else
                            {
                                DisableOutline(newhit);
                            }
                        }
                        else
                        {
                            DisableOutline(newhit);
                        }
                    }
                }
                if (obj.type == Interactables.SERVICECOUNTER)
                {
                    if (holdingNum > 0)
                    {
                        DisableOutline(newhit);
                    }
                }
                if (obj.type == Interactables.BURGERSTATION)
                {
                    if (holdingNum > 0)
                    {
                        Tray tray = empltySlot.transform.GetChild(0).GetComponent<Tray>();

                        if (tray != null)
                        {
                            if (tray.emptySlot.transform.childCount >= 4)
                            {
                                DisableOutline(newhit);
                            }
                        }
                        else
                        {
                            DisableOutline(newhit);
                        }
                    }
                }
                if (obj.type == Interactables.FRYSTATION)
                {
                    FryStation fryStation = newhit.transform.GetComponentInParent<FryStation>();

                    if (fryStation != null && holdingNum > 0)
                    {
                        GameObject playerObject = empltySlot.transform.GetChild(0).gameObject;

                        Tray tray = playerObject.GetComponent<Tray>();

                        if (playerObject.CompareTag("Cooked Fries"))
                        {
                            if (fryStation.fryLvl > 1)
                            {
                                DisableOutline(newhit);
                            }
                        }
                        else if (tray != null)
                        {
                            if (tray.emptySlot.transform.childCount >= 4)
                            {
                                DisableOutline(newhit);
                            }
                        }
                        else
                        {
                            DisableOutline(newhit);
                        }
                    }

                }
                if (obj.type == Interactables.FRIER)
                {
                    Frier frier = newhit.transform.GetComponentInParent<Frier>();

                    if (frier != null)
                    {
                        if (frier.isFull() && holdingNum > 0)
                        {
                            DisableOutline(newhit);
                        }
                    }

                    if (holdingNum > 0 && !empltySlot.transform.GetChild(0).CompareTag("Raw Fries"))
                    {
                        DisableOutline(newhit);
                    }
                }
                if (obj.type == Interactables.HEATER)
                {
                    if (counterHoldingNum >= 3)
                    {
                        DisableOutline(newhit);
                    }
                    if (holdingNum > 0 && !empltySlot.transform.GetChild(0).CompareTag("Cooked Paddies"))
                    {
                        DisableOutline(newhit);
                    }
                }
            }
        }
        // If i do see anything deselect last object
        else
        {
            // Deselect old Object if not looking at any object
            if (hit.transform != null)
            {
                Outline oldOutline = hit.transform.GetComponentInParent<Outline>();
                if (oldOutline != null)
                {
                    oldOutline.enabled = false;
                }
            }
        }
    }

    void Interactions()
    {
        // Swap items In and out of counters


        // Check if looking at an object
        ///Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        //if (Physics.Raycast(ray, out hit, 1000))


        // Exit clauses

        RaycastHit hit;

        // If raycast hits nothing exit
        if (!Physics.Raycast(body.position - new Vector3(0, 0.7f, 0), transform.forward, out hit))
            return;

        // If distance of raycast is > 2 exit
        if (hit.distance > 2)
            return;

        Interact obj = hit.transform.GetComponentInParent<Interact>();

        // If not interactable object
        if (obj == null)
            return;

        // Setup number of items being held and on the counter
        int holdingNum = empltySlot.transform.childCount;
        int counterHoldingNum = obj.emptySlot.transform.childCount;


        // Pointers
        Pointer pointer = obj.GetComponent<Pointer>();

        if (pointer != null)
        {
            if (holdingNum == 0 && counterHoldingNum == 0)
            {
                pointer.pointer.gameObject.SetActive(true);
            }
            else if (counterHoldingNum == 0 && holdingNum > 0)
            {
                Tray tray = empltySlot.transform.GetChild(0).GetComponent<Tray>();

                if (tray != null && tray.emptySlot.transform.childCount < 4 && obj.createObject != null && obj.createObject.CompareTag("Food"))
                {
                    pointer.pointer.gameObject.SetActive(true);
                }
            }
        }


        // Interactions Past here

        if (obj.type == Interactables.COUNTER)
        {
            // Put object on counter
            if (holdingNum > 0 && counterHoldingNum == 0)
            {
                print("EMPTY");

                GameObject playersObject = empltySlot.transform.GetChild(0).gameObject;

                // Can put objects on counters if they're a food
                if (playersObject.CompareTag("Food") || playersObject.CompareTag("Extinguisher"))
                {
                    //print("swap hand with counter");
                    playersObject.transform.SetParent(obj.emptySlot.transform);
                    playersObject.transform.localPosition = Vector3.zero;
                    playersObject.transform.localRotation = Quaternion.identity;
                }
                // Put food from hand tray onto counter
                if (playersObject.CompareTag("Tray"))
                {
                    Tray tray = playersObject.GetComponent<Tray>();

                    if (tray != null && tray.emptySlot.transform.childCount > 0)
                    {
                        //print("Put food from hand tray onto counter");

                        GameObject food = tray.emptySlot.transform.GetChild(tray.emptySlot.transform.childCount - 1).gameObject;

                        food.transform.SetParent(obj.emptySlot.transform);
                        food.transform.localPosition = Vector3.zero;
                        food.transform.localRotation = Quaternion.identity;
                    }
                }
            }

            // Put object onto tray
            else if (holdingNum > 0 && counterHoldingNum > 1)
            {

                print("COUNTER");


                GameObject playersObject = empltySlot.transform.GetChild(0).gameObject;
                GameObject counterObject = obj.emptySlot.transform.GetChild(0).gameObject;

                if (playersObject != null && counterObject != null)
                {
                    Tray counterTray = counterObject.GetComponent<Tray>();
                    Tray handTray = playersObject.GetComponent<Tray>();

                    // Put food from Counter Tray into hand
                    if (counterTray != null && counterTray.emptySlot.transform.childCount < 4 && playersObject.CompareTag("Food"))
                    {
                        //print("Put food from Counter Tray into hand");

                        playersObject.transform.SetParent(counterTray.emptySlot.transform);
                        playersObject.transform.localPosition = Vector3.zero;
                        playersObject.transform.localRotation = Quaternion.identity;
                    }

                    // Put food from hand tray onto counter
                    if (handTray != null && handTray.emptySlot.transform.childCount < 4 && counterObject.CompareTag("Food"))
                    {
                        //print("Put food from hand tray onto counter");

                        counterObject.transform.SetParent(handTray.emptySlot.transform);
                        counterObject.transform.localPosition = Vector3.zero;
                        counterObject.transform.localRotation = Quaternion.identity;
                    }
                }
            }

            // Take object from Counter
            else if (counterHoldingNum > 0 && holdingNum == 0)
            {

                print("PLAYER SLOT EMPTY");


                GameObject counterObject = obj.emptySlot.transform.GetChild(0).gameObject;

                // Pick up Food
                if (counterObject.CompareTag("Food") || counterObject.CompareTag("Extinguisher"))
                {
                    counterObject.transform.SetParent(empltySlot.transform);
                    counterObject.transform.localPosition = Vector3.zero;
                    counterObject.transform.localRotation = Quaternion.identity;
                }
                // Pick up Food from Counter tray
                else if (counterObject.CompareTag("Tray"))
                {
                    Tray tray = counterObject.GetComponent<Tray>();

                    if (tray != null && tray.emptySlot.transform.childCount > 0)
                    {
                        //print("Pick up Food from Counter tray");

                        GameObject trayObject = tray.emptySlot.transform.GetChild(tray.emptySlot.transform.childCount - 1).gameObject;

                        trayObject.transform.SetParent(empltySlot.transform);
                        trayObject.transform.localPosition = Vector3.zero;
                        trayObject.transform.localRotation = Quaternion.identity;
                    }
                }
            }
            else
            {

                print("ERROR LOL");
            }


            PickUp pickUp = obj.transform.GetComponent<PickUp>();

            // Check if customers can pickup from this counter
            if (pickUp != null && obj.emptySlot.transform.childCount == 1)
            {
                GameObject counterObject = obj.emptySlot.transform.GetChild(0).gameObject;

                // Check if order matcheds
                if (pickUp.DoesOrderMatch(counterObject) != -1)
                {

                    // Get closest avalible customer
                    for (int j = 0; j < pickUp.CustomerParent.childCount; j++)
                    {
                        GameObject customer = pickUp.CustomerParent.GetChild(j).gameObject;
                        CustomerController1 customerController = customer.GetComponent<CustomerController1>();
                        CarController carController = customer.GetComponent<CarController>();


                        if (pickUp.type == CustomerType.TAKEAWAY)
                        {
                            // Check Customer State
                            if (customerController != null && customerController.stage == CustomerStage.WAITING)
                            {
                                // Order Completed, Customer Picks up order and removes it from display
                                pickUp.RemoveDisplayOrder(pickUp.DoesOrderMatch(counterObject));

                                customerController.stage = CustomerStage.PICKUP;
                                customerController.pointsOfInterest.Add(obj.gameObject);

                                Destroy(counterObject);

                                GameManager.Instance.numCustomersServed++;

                                // Turn order into a happy meal
                                GameObject happyMeal = Instantiate(pickUp.HappyMeal, obj.emptySlot.transform);
                                happyMeal.transform.localPosition = Vector3.zero;
                                happyMeal.transform.localRotation = Quaternion.identity;

                                break;
                            }
                        }

                        else if (pickUp.type == CustomerType.DRIVETHRU)
                        {
                            // Check Car Customer State
                            if (carController != null && carController.stage == CustomerStage.WAITING)
                            {
                                // Order Completed, Customer Picks up order and removes it from display
                                pickUp.RemoveDisplayOrder(pickUp.DoesOrderMatch(counterObject));

                                carController.stage = CustomerStage.PICKUP;
                                carController.pointsOfInterest.Add(obj.gameObject);

                                Destroy(counterObject);

                                GameManager.Instance.numCustomersServed++;

                                // Turn order into a happy meal
                                GameObject happyMeal = Instantiate(pickUp.HappyMeal, obj.emptySlot.transform);
                                happyMeal.transform.localPosition = Vector3.zero;
                                happyMeal.transform.localRotation = Quaternion.identity;

                                break;
                            }
                        }
                    }
                }
            }
        }

        else if (obj.type == Interactables.BIN)
        {
            // If not holding anything exit
            if (holdingNum <= 0)
                return;

            tempSound = obj.GetComponent<AudioSource>();
            tempSound.Play();
            GameObject playerObj = empltySlot.transform.GetChild(holdingNum - 1).gameObject;
            Tray tray = playerObj.GetComponent<Tray>();

            // If holding tray dont throw in bin
            if (tray != null)
            {
                int trayNum = tray.emptySlot.transform.childCount;

                // If the tray has food on it throw that in bin
                if (trayNum > 0)
                {
                    GameManager.Instance.score -= 1;
                    GameManager.Instance.numFoodWasted++;

                    Destroy(tray.emptySlot.transform.GetChild(trayNum - 1).gameObject);
                }
            }
            // If not a tray throw in bin
            else
            {
                GameManager.Instance.score -= 1;
                GameManager.Instance.numFoodWasted++;

                Destroy(playerObj);
            }

        }

        else if (obj.type == Interactables.FRIER)
        {

            Frier frier = obj.GetComponent<Frier>();

            if (frier != null)
            {
                // Take Fries out of frier
                if (holdingNum == 0)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        if (frier.traySlots[i].transform.childCount > 0 && !frier.traySlots[i].transform.GetChild(0).CompareTag("Raw Fries"))
                        {
                            GameObject friedFries = frier.traySlots[i].transform.GetChild(0).gameObject;

                            friedFries.transform.SetParent(empltySlot.transform);
                            friedFries.transform.localPosition = Vector3.zero;
                            friedFries.transform.localRotation = Quaternion.identity;

                            Cooking cooking = friedFries.GetComponent<Cooking>();

                            if (friedFries.CompareTag("Cooked Fries") && cooking != null)
                            {
                                cooking.beingCooked = false;
                            }

                            if (pointer != null)
                            {
                                pointer.pointer.gameObject.SetActive(false);
                            }
                            break;
                        }
                    }
                }
                // Puting in the frier
                else if (holdingNum > 0)
                {
                    GameObject playersObject = empltySlot.transform.GetChild(0).gameObject;
                    Cooking cooking = empltySlot.transform.GetChild(0).GetComponent<Cooking>();

                    if (playersObject.CompareTag("Raw Fries") && cooking != null)
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            if (frier.traySlots[i].transform.childCount == 0)
                            {
                                playersObject.transform.SetParent(frier.traySlots[i].transform);
                                playersObject.transform.localPosition = Vector3.zero;
                                playersObject.transform.localRotation = Quaternion.identity;

                                cooking.beingCooked = true;

                                break;
                            }
                        }
                    }
                }
            }

            if (pointer != null)
            {
                if (!frier.isEmpty())
                {
                    pointer.pointer.gameObject.SetActive(false);
                }
            }
        }

        else if (obj.type == Interactables.FRYSTATION)
        {
            FryStation fryStation = obj.GetComponent<FryStation>();

            // No fry station exit
            if (fryStation == null)
                return;

            // Put raw fries in station
            if (holdingNum > 0)
            {
                GameObject playersObject = empltySlot.transform.GetChild(0).gameObject;
                Tray tray = playersObject.GetComponent<Tray>();

                if (playersObject.CompareTag("Cooked Fries"))
                {
                    if (fryStation.fryLvl <= 1)
                    {
                        fryStation.fryLvl++;
                        Destroy(playersObject);
                    }
                }
                // Put fries on tray
                if (tray != null && tray.emptySlot.transform.childCount < 4)
                {
                    if (fryStation.fryLvl > 0)
                    {
                        GameObject newFries = Instantiate(obj.createObject);
                        newFries.transform.SetParent(tray.emptySlot.transform);
                        newFries.transform.localPosition = Vector3.zero;
                        newFries.transform.forward = transform.forward;


                        fryStation.fryLvl -= 0.25f;

                        if (pointer != null)
                        {
                            pointer.pointer.gameObject.SetActive(false);
                        }
                    }
                }
            }
            // Take cooked fries in station
            else
            {
                if (fryStation.fryLvl > 0)
                {
                    GameObject newFries = Instantiate(obj.createObject);
                    newFries.transform.SetParent(empltySlot.transform);
                    newFries.transform.localPosition = Vector3.zero;
                    newFries.transform.forward = transform.forward;


                    fryStation.fryLvl -= 0.25f;

                    if (pointer != null)
                    {
                        pointer.pointer.gameObject.SetActive(false);
                    }
                }
            }
        }

        else if (obj.type == Interactables.FRIDGE)
        {
            if (holdingNum == 0)
            {
                GameObject food = Instantiate(obj.createObject);
                tempSound = obj.GetComponent<AudioSource>();
                tempSound.Play();
                food.transform.SetParent(empltySlot.transform);

                if (food.CompareTag("Raw Fries"))
                    food.transform.localPosition = Vector3.zero;
                else
                    food.transform.localPosition = Vector3.zero;

                food.transform.forward = transform.forward;
            }
            else if (holdingNum == 1)
            {
                GameObject playerObject = empltySlot.transform.GetChild(0).gameObject;
                Tray tray = playerObject.GetComponent<Tray>();

                if (playerObject.CompareTag("Raw Paddies") && obj.createObject.CompareTag("Raw Paddies"))
                {
                    GameObject food = Instantiate(obj.createObject);
                    food.transform.SetParent(empltySlot.transform);
                    food.transform.localPosition = new Vector3(0, 0.1f, 0);
                    food.transform.localRotation = Quaternion.identity;
                }
                else if (tray != null && tray.emptySlot.transform.childCount < 4 && obj.createObject.CompareTag("Food"))
                {
                    GameObject food = Instantiate(obj.createObject);
                    food.transform.SetParent(tray.emptySlot.transform);
                    food.transform.localPosition = new Vector3(0, 0.1f, 0);
                    food.transform.localRotation = Quaternion.identity;
                }
            }

        }

        else if (obj.type == Interactables.BURGERSTATION)
        {
            BurgerStation burgerStation = obj.GetComponent<BurgerStation>();


            // If no burger station parts and no paddies left to put in the burgers then exit
            if (burgerStation == null || burgerStation.paddyHeater.childCount <= 0)
                return;

            GameManager.Instance.screen.touchUI.gameObject.SetActive(false);

            // Set how many burgers are created with each interaction
            int maxBurgersAllowed;

            if (GameManager.mode == GameMode.BABY)
                maxBurgersAllowed = 4;
            else
                maxBurgersAllowed = 2;

            // If there are burgers to pick up
            if (counterHoldingNum > 0)
            {
                GameObject food = obj.emptySlot.transform.GetChild(0).gameObject;

                // If food not food exit
                if (!food.CompareTag("Food"))
                    return;

                // If not holding anything pick it up food
                if (holdingNum == 0)
                {
                    food.transform.SetParent(empltySlot.transform);
                    food.transform.localPosition = Vector3.zero;
                    food.transform.localRotation = Quaternion.identity;
                }
                // If holding something 
                else
                {
                    GameObject playersObject = empltySlot.transform.GetChild(0).gameObject;

                    // If holding the tray
                    if (playersObject.CompareTag("Tray"))
                    {
                        Tray tray = playersObject.GetComponent<Tray>();

                        // If not actual tray exit
                        if (tray == null)
                            return;
                        // Tray is full exit
                        if (tray.transform.childCount < 4)
                            return;

                        // Put food from Counter Tray into hand
                        if (tray.emptySlot.transform.childCount < 4)
                        {
                            food.transform.SetParent(tray.emptySlot.transform);
                            food.transform.localPosition = Vector3.zero;
                            food.transform.localRotation = Quaternion.identity;
                        }
                    }
                    // Else players hands are full and nothing happens
                }
            }

            // If not trying to pick up burgers create the burgers
            else
            {
                // Not holding anything
                if (holdingNum == 0)
                {
                    // Hold Burger
                    GameObject newBurger = Instantiate(obj.createObject);
                    newBurger.transform.SetParent(empltySlot.transform);
                    newBurger.transform.localPosition = Vector3.zero;
                    newBurger.transform.forward = transform.forward;
                    burgerStation.TakePaddy();


                    // Put Burgers on counter
                    for (int i = 0; i < maxBurgersAllowed - 1; i++)
                    {
                        if (burgerStation.CanTakePaddy())
                        {
                            GameObject newerBurger = Instantiate(obj.createObject);
                            newerBurger.transform.SetParent(obj.emptySlot.transform);
                            newerBurger.transform.localPosition = Vector3.zero;
                            newerBurger.transform.forward = transform.forward;
                            burgerStation.TakePaddy();
                        }
                    }
                    //
                    isRunning = false;

                    if (pointer != null)
                    {
                        pointer.pointer.gameObject.SetActive(false);
                    }
                }
                // Holding a tray
                else
                {
                    Tray tray = empltySlot.transform.GetChild(0).GetComponent<Tray>();

                    // Check holding a tray or exit
                    if (tray == null)
                        return;

                    int amountOnTray = tray.emptySlot.transform.childCount;

                    // Make sure tray isnt full or leave
                    if (amountOnTray > 4)
                        return;

                    int burgersTaken = 0;

                    for (int i = 0; i < 4 - amountOnTray; i++)
                    {

                        burgersTaken++;

                        // Make sure theres paddies to put on bun
                        if (burgerStation.CanTakePaddy() && burgersTaken <= maxBurgersAllowed)
                        {
                            GameObject newBurger = Instantiate(obj.createObject);
                            newBurger.transform.SetParent(tray.emptySlot.transform);
                            newBurger.transform.localPosition = Vector3.zero;
                            newBurger.transform.forward = transform.forward;

                            burgerStation.TakePaddy();
                            isRunning = false;
                        }
                        else
                        {
                            break;
                        }
                    }

                    if (pointer != null)
                    {
                        pointer.pointer.gameObject.SetActive(false);
                    }
                }
            }
        }

        else if (obj.type == Interactables.HOTPLATE)
        {
            // Put paddys on hotplate
            if (holdingNum > 0 && counterHoldingNum == 0)
            {
                GameObject paddy = empltySlot.transform.GetChild(holdingNum - 1).gameObject;
                if (paddy.CompareTag("Raw Paddies"))
                {
                    paddy.transform.SetParent(obj.emptySlot.transform);
                    paddy.transform.localPosition = Vector3.zero;
                    paddy.transform.localRotation = Quaternion.identity;

                    Cooking cooking = paddy.GetComponent<Cooking>();

                    if (cooking != null)
                    {
                        cooking.beingCooked = true;
                    }
                }
                if (paddy.CompareTag("Extinguisher"))
                {
                    obj.GetComponent<FireParticleController>().onfire = false;
                }
            }

            // Take paddys off hotplate
            else if (counterHoldingNum > 0 && holdingNum == 0 && !obj.emptySlot.transform.GetChild(0).CompareTag("Raw Paddies"))
            {
                Transform cookedPaddy = obj.emptySlot.transform.GetChild(0).transform;
                cookedPaddy.SetParent(empltySlot.transform);
                cookedPaddy.localPosition = Vector3.zero;
                cookedPaddy.localRotation = Quaternion.identity;

                Cooking cooking = cookedPaddy.GetComponent<Cooking>();

                if (cooking != null)
                {
                    cooking.beingCooked = false;
                }
            }

            // Can grab 2 paddies at a time
            else if (counterHoldingNum > 0 && holdingNum > 0)
            {
                // Only take off what you are already holding
                if ((empltySlot.transform.GetChild(0).CompareTag("Cooked Paddies") && obj.emptySlot.transform.GetChild(0).CompareTag("Cooked Paddies"))
                    || (empltySlot.transform.GetChild(0).CompareTag("Burnt Paddies") && obj.emptySlot.transform.GetChild(0).CompareTag("Burnt Paddies"))
                    || (empltySlot.transform.GetChild(0).CompareTag("Fire Paddies") && obj.emptySlot.transform.GetChild(0).CompareTag("Fire Paddies")))
                {
                    Transform cookedPaddy = obj.emptySlot.transform.GetChild(0).transform;
                    cookedPaddy.SetParent(empltySlot.transform);
                    cookedPaddy.localPosition = new Vector3(0, 0.1f, 0);
                    cookedPaddy.localRotation = Quaternion.identity;

                    Cooking cooking = cookedPaddy.GetComponent<Cooking>();

                    if (cooking != null)
                    {
                        cooking.beingCooked = false;
                    }
                }
            }
        }

        else if (obj.type == Interactables.HEATER)
        {
            // if not holding anything or 
            if (holdingNum <= 0 || counterHoldingNum >= 3)
                return;

            GameObject paddy = empltySlot.transform.GetChild(empltySlot.transform.childCount - 1).gameObject;

            // if oby isnt a cooked paddy exit
            if (!paddy.CompareTag("Cooked Paddies"))
                return;

            paddy.transform.SetParent(obj.emptySlot.transform);
            paddy.transform.SetAsFirstSibling();
            paddy.transform.localRotation = Quaternion.identity;

            // Reset paddies Pos
            for (int i = 0; i < obj.HoldingNum(); i++)
            {
                obj.emptySlot.transform.GetChild(i).transform.localPosition = new Vector3(0, i * 0.3f, 0);
            }
        }

        else if (obj.type == Interactables.SERVICECOUNTER)
        {
            ServiceCounter service = obj.GetComponent<ServiceCounter>();

            if (service != null && holdingNum == 0 && service.customerAtRegister != CustomerType.NONE)
            {
                service.AddOrdersToScreen(false);
            }
        }

        else if (obj.type == Interactables.TRAYDISPENCER)
        {
            // Make sure theres items on the counter or exit
            if (counterHoldingNum <= 0)
                return;

            Tray tray = obj.emptySlot.transform.GetChild(0).GetComponent<Tray>();

            // If tray dispencer doesnt have tray exit
            if (tray == null)
                return;

            int trayNum = tray.emptySlot.transform.childCount;

            // Put item onto tray
            if (holdingNum > 0 && trayNum < 4)
            {
                GameObject playersObj = empltySlot.transform.GetChild(0).gameObject;

                // Can put objects on tray if they're a food
                if (playersObj.CompareTag("Food"))
                {
                    //print("Food put on tray");
                    playersObj.transform.SetParent(tray.emptySlot.transform);
                    playersObj.transform.localPosition = Vector3.zero;
                    playersObj.transform.localRotation = Quaternion.identity;
                }
            }

            // Take item from tray
            else if (trayNum > 0 && holdingNum == 0)
            {
                //print("Food taken from tray");

                GameObject trayObj = tray.emptySlot.transform.GetChild(trayNum - 1).gameObject;
                trayObj.transform.SetParent(empltySlot.transform);
                trayObj.transform.localPosition = Vector3.zero;
                trayObj.transform.localRotation = Quaternion.identity;
            }
        }

        else if (obj.type == Interactables.ICECREAMMACHINE)
        {
            print("Interacted with ice crea");

            IceCreamMachine iceCreamMachine = obj.GetComponent<IceCreamMachine>();

            if (!GameManager.IceCreamMachineWorking)
            {
                StartCoroutine(GameManager.Instance.screen.overlay.waitThenDisplay2("Ice Cream Machine is Broken LOL", 0));
            }
            else if (holdingNum == 0 && iceCreamMachine != null)
            {
                if (iceCreamMachine.tick > 60)
                {
                    GameObject iceCream = Instantiate(obj.createObject, empltySlot.transform);
                    iceCream.transform.localPosition = Vector3.zero;
                    iceCream.transform.localRotation = Quaternion.identity;
                    iceCreamMachine.tick = 0;
                }
                else
                {
                    StartCoroutine(GameManager.Instance.screen.overlay.waitThenDisplay2("Chill out dude wait " + (60 - (int)iceCreamMachine.tick) + " seconds", 0));
                }

            }
        }
    }

    void HoldInteractions()
    {
        RaycastHit hit;

        // If your looking at nothing exit
        if (!Physics.Raycast(body.position - new Vector3(0, 0.7f, 0), transform.forward, out hit))
            return;

        // If hit isnt within 2 exit
        if (hit.distance > 2)
            return;

        Interact obj = hit.transform.GetComponentInParent<Interact>();

        // If you cant interact with the object your looking at exit
        if (obj == null)
            return;

        int holdingNum = empltySlot.transform.childCount;
        int counterHoldingNum = obj.emptySlot.transform.childCount;

        // Interactions Past here

        if (obj.type == Interactables.COUNTER)
        {
            // Put tray on counter
            if (holdingNum > 0 && counterHoldingNum == 0)
            {
                GameObject playersObject = empltySlot.transform.GetChild(0).gameObject;
                Tray tray = playersObject.GetComponent<Tray>();


                // Can put objects on counters if they're a food
                if (tray != null)
                {
                    playersObject.transform.SetParent(obj.emptySlot.transform);
                    playersObject.transform.localPosition = Vector3.zero;
                    playersObject.transform.localRotation = Quaternion.identity;

                    GameManager.Instance.screen.overlay.hideTip();
                }
            }
            // Take tray from counter
            else if (holdingNum == 0 && counterHoldingNum > 0)
            {

                GameObject playersObject = obj.emptySlot.transform.GetChild(0).gameObject;
                Tray tray = playersObject.GetComponent<Tray>();

                if (tray != null)
                {
                    playersObject.transform.SetParent(empltySlot.transform);
                    playersObject.transform.localPosition = Vector3.zero;
                    playersObject.transform.localRotation = Quaternion.identity;

                    GameManager.Instance.screen.overlay.hideTip();
                }
            }
            // Both player and counter have Trays
            else if (holdingNum > 0 && counterHoldingNum > 0)
            {
                GameObject playersObject = empltySlot.transform.GetChild(0).gameObject;
                GameObject counterObject = obj.emptySlot.transform.GetChild(0).gameObject;

                Tray playersTray = playersObject.GetComponent<Tray>();
                Tray counterTray = counterObject.GetComponent<Tray>();
                Dispencer dispencer = obj.GetComponent<Dispencer>();

                if (playersTray != null && counterTray != null)
                {
                    // If its a dispence and neither trays have food on them put back in stack
                    if (dispencer != null && playersTray.emptySlot.transform.childCount + counterTray.emptySlot.transform.childCount == 0)
                    {
                        Destroy(playersTray.gameObject);
                    }
                    // Put player tray on top of stack
                    else if (dispencer != null && playersTray.emptySlot.transform.childCount > 0 && counterTray.emptySlot.transform.childCount == 0)
                    {
                        playersObject.transform.SetParent(obj.emptySlot.transform);
                        playersObject.transform.localPosition = Vector3.zero;
                        playersObject.transform.localRotation = Quaternion.identity;

                        Destroy(counterTray.gameObject);
                    }
                    // Swap Trays (Put hand tray on counter put counter tray in hands)
                    else
                    {
                        playersObject.transform.SetParent(obj.emptySlot.transform);
                        playersObject.transform.localPosition = Vector3.zero;
                        playersObject.transform.localRotation = Quaternion.identity;
                        counterTray.transform.SetParent(empltySlot.transform);
                        counterTray.transform.localPosition = Vector3.zero;
                        counterTray.transform.localRotation = Quaternion.identity;
                    }

                    GameManager.Instance.screen.overlay.hideTip();
                }
            }

            PickUp pickUp = obj.transform.GetComponent<PickUp>();

            // Have Customers pick up tray
            if (pickUp != null && obj.emptySlot.transform.childCount > 0)
            {
                GameObject counterObject = obj.emptySlot.transform.GetChild(0).gameObject;

                // Check if order matcheds
                if (pickUp.DoesOrderMatch(counterObject) != -1)
                {
                    // Get closest avalible customer
                    for (int j = 0; j < pickUp.CustomerParent.childCount; j++)
                    {
                        GameObject customer = pickUp.CustomerParent.GetChild(j).gameObject;
                        CustomerController1 customerController = customer.GetComponent<CustomerController1>();
                        CarController carController = customer.GetComponent<CarController>();


                        if (pickUp.type == CustomerType.TAKEAWAY)
                        {
                            // Check Customer State
                            if (customerController != null && customerController.stage == CustomerStage.WAITING)
                            {
                                // Order Completed, Customer Picks up order and removes it from display
                                pickUp.RemoveDisplayOrder(pickUp.DoesOrderMatch(counterObject));

                                customerController.stage = CustomerStage.PICKUP;
                                customerController.pointsOfInterest.Add(obj.gameObject);

                                Destroy(counterObject);

                                GameManager.Instance.numCustomersServed++;

                                // Turn order into a happy meal
                                GameObject happyMeal = Instantiate(pickUp.HappyMeal, obj.emptySlot.transform);
                                happyMeal.transform.localPosition = Vector3.zero;
                                happyMeal.transform.localRotation = Quaternion.identity;

                                break;
                            }
                        }
                        else
                        {
                            // Check Car Customer State
                            if (carController != null && carController.stage == CustomerStage.WAITING)
                            {
                                // Order Completed, Customer Picks up order and removes it from display
                                pickUp.RemoveDisplayOrder(pickUp.DoesOrderMatch(counterObject));

                                carController.stage = CustomerStage.PICKUP;
                                carController.pointsOfInterest.Add(obj.gameObject);

                                Destroy(counterObject);

                                GameManager.Instance.numCustomersServed++;

                                // Turn order into a happy meal
                                GameObject happyMeal = Instantiate(pickUp.HappyMeal, obj.emptySlot.transform);
                                happyMeal.transform.localPosition = Vector3.zero;
                                happyMeal.transform.localRotation = Quaternion.identity;

                                break;
                            }
                        }
                    }
                }
            }
        }

        else if (obj.type == Interactables.BIN)
        {
            // Put tray in bin
            if (holdingNum > 0)
            {
                tempSound = obj.GetComponent<AudioSource>();
                tempSound.Play();
                GameObject playersObject = empltySlot.transform.GetChild(0).gameObject;

                Tray tray = playersObject.GetComponent<Tray>();

                // For every object in the tray
                if (tray != null)
                {
                    GameManager.Instance.score -= tray.emptySlot.transform.childCount;
                    GameManager.Instance.numFoodWasted += tray.emptySlot.transform.childCount;
                }

                // For thee tray itself
                GameManager.Instance.score--;
                GameManager.Instance.numFoodWasted++;

                Destroy(playersObject);
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        GameObject CollidedObj = collision.collider.gameObject;
        CarController car = CollidedObj.GetComponent<CarController>();
        Rigidbody bodyObj = CollidedObj.GetComponent<Rigidbody>();

        if (car != null && bodyObj != null && bodyObj.velocity.magnitude > 5)
        {
            print("Mummy Look at Me");

            body.constraints = RigidbodyConstraints.None;
            //body.transform.forward = bodyObj.transform.forward;
            body.velocity = bodyObj.velocity * 2;
            enabled = false;

            GameManager.Instance.ending = GameEnding.DEAD;
            //this.gameObject.GetComponent<PlayerBehaviours1>().enabled = false;
            //enabled = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "Gold")
        {
            GameManager.Instance.isRunning = true;
            //GameManager.state = GameState.GAMEPLAY;
            GameManager.Instance.ending = GameEnding.GOLD;
            GameManager.IceCreamMachineWorking = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.name == "HitBox")
        {
            GameManager.Instance.ending = GameEnding.QUIT;
        }
    }
}
