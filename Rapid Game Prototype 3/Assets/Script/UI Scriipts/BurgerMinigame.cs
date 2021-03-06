using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BurgerMinigame : MonoBehaviour
{
    //public GameManager gameManager;

    public static DragWindow selected;

    public List<Transform> DObjects; // No clue y i called it a d object

    public bool[] correctPos;
        

    // Start is called before the first frame update
    void Start()
    {
        selected = DObjects[1].GetComponent<DragWindow>();
        selected.border.GetComponent<Image>().color = new Color(0, 1, 0, 1);

        for (int i = 1; i < 6; i++)
        {
            correctPos[i] = false;

            // First point stays where it is
            // Starting rand pos
            DObjects[i].transform.localPosition = new Vector3(Random.Range(100, 1820) - Screen.width / 2, Random.Range(100, 920) - Screen.width / 2, 0);
        }


    }

    // Update is called once per frame
    void Update()
    {
        // When we snap it needs to be
        //  - Obj 1 just dropped 
        //  - connect obj1 to nearest obj2's snapping point within closestDis
        //  - obj 1 is connecting 2 obj2's snapping point
        //  - obj 1 is becomes child of obj 2
        //      - this is so when move obj2 obj1 moves 2
        //      - this is can be broken by moving obj 1
        //  - obj2 can only have 1 child
        //  - obj1 can still have child
        //  - obj2's snapping point becomes obj1's 1
        //      - this can be broken by moving obj1
        //      - if obj1 has no snapping point then so does obj2
        //  - obj1 cannot snap to a child of itself

        JustMoved();

        Snap();

        if (Won())
        {
            ResetBoard();

            if (GameManager.Instance.isTouchEnabled)
                GameManager.Instance.screen.touchUI.gameObject.SetActive(true);
        }

        // Draw dem kewl lines
        for (int i = 0; i < DObjects.Count; i++)
        {

            Debug.DrawLine(selected.transform.position, DObjects[i].position, Color.black);
        }
    }

    void JustMoved()
    {
        for (int i = 0; i < 7; i++)
        {
            Transform obj1 = DObjects[i];
            DragWindow dragRect1 = DObjects[i].GetComponent<DragWindow>();

            if (obj1 != null && dragRect1 != null && dragRect1.justMoved)
            {
                //print(obj1.name);

                dragRect1.justMoved = false;
                correctPos[i - 1] = false;

                Stackable stackable = obj1.parent.GetComponent<Stackable>();

                if (stackable != null)
                {
                    //print(obj1.parent.name);
                    stackable.isActive1 = true;
                }

                obj1.SetParent(this.transform);
            }
        }
    }


    void Snap()
    {
        // Check for has drops
        for (int i = 0; i < 7; i++)
        {
            DragWindow obj1 = DObjects[i].GetComponent<DragWindow>();


            // Only runs on has drops
            if (obj1 != null && obj1.hasDropped)
            {
                obj1.hasDropped = false; // Immediately switch it off again


                float closestDis = 1f;
                int closestElemnt = -1;

                GameObject stackTop = obj1.gameObject;

                // Find the ingredient on the top of the stack
                do
                {
                    // If theres no children theres no where left to go, break
                    if (stackTop.transform.childCount == 0)
                    {
                        break;
                    }

                    // The Last Ingedint is thr next ingredient
                    DragWindow nextInredient = stackTop.transform.GetChild(stackTop.transform.childCount - 1).GetComponent<DragWindow>();

                    // If its not an ingredient, break
                    if (nextInredient == null)
                    {
                        break;
                    }

                    stackTop = nextInredient.gameObject;

                } while (true);

                // Check Obj 2
                for (int j = 0; j < 7; j++)
                {
                    Stackable stackable2 = DObjects[j].GetComponent<Stackable>();

                    // check distances
                    if (stackable2 != null)
                    {
                        float distance = Vector3.Distance(DObjects[i].position, stackable2.snappingPoint.position);

                        // Dont try to stack on itself
                        if (distance < closestDis && i != j && stackable2.isActive1 && DObjects[j].gameObject != stackTop)
                        {
                            closestDis = distance;
                            closestElemnt = j;
                        }
                    }
                }

                // If a closest element candidate is found
                if (closestElemnt != -1)
                {
                    Stackable closeStack = DObjects[closestElemnt].GetComponent<Stackable>();

                    if (closeStack != null)
                    {

                        DObjects[i].SetParent(DObjects[closestElemnt]);
                        DObjects[i].position = closeStack.snappingPoint.position;

                        closeStack.isActive1 = false;
                    }

                    //print("elements " + i + " " + closestElemnt);

                    if (i == closestElemnt + 1)
                    {
                        correctPos[i - 1] = true;
                    }
                }

            }
        }
    }

    void SelectNext()
    {
        float[] distanceX = new float[DObjects.Count];
        float[] distanceY = new float[DObjects.Count];

        for (int i = 0; i < DObjects.Count; i++)
        {
            distanceX[i] = selected.transform.position.x - DObjects[i].position.x;
            distanceY[i] = selected.transform.position.y - DObjects[i].position.y;

        }

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            DragWindow closest;

            for (int i = 0; i < DObjects.Count; i++)
            {
                //if (DObjects[i].)
            }
        }
    }

    bool Won()
    {
        bool result = true;

        for (int i = 0; i < 6; i++)
        {
            if (!correctPos[i])
            {
                result = false;
            }
        }

        return result;
    }
    void ResetBoard()
    {

        for (int i = 0; i < 7; i++)
        {
            DObjects[i].SetParent(this.transform);

            if (i > 0) // Dont Rand first object
                DObjects[i].transform.localPosition = new Vector3(Random.Range(100, 1820) - Screen.width / 2, Random.Range(100, 920) - Screen.width / 2, 0);

            // Reset Stack
            Stackable stackable = DObjects[i].GetComponent<Stackable>();

            if (stackable != null)
                stackable.isActive1 = true;

            // Reset Borders
            DragWindow dragWindow = DObjects[i].GetComponent<DragWindow>();

            if (dragWindow != null)
                dragWindow.border.SetParent(dragWindow.transform);
        }

        for (int i = 0; i < 6; i++)
        {
            correctPos[i] = false;
        }

        //gameManager.gameState = GameState.GAMEPLAY;
        GameManager.Instance.player1.isRunning = true;
        this.gameObject.SetActive(false);
    }
}
