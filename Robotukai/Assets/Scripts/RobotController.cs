using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotController : MonoBehaviour
{

    //Koordinaciu logika:
    //Z asis i teigiama puse - NORTH, Vector3(0,0,1) arba Vector3.forward
    //Z asis i neigiama puse - SOUTH, Vector3(0,0,-1) arba Vector3.back
    //X asis i teigiama puse - EAST, Vector3(1,0,0) arba Vector3.right
    //X asis i neigiama puse - WEST, Vector3(-1,0,0) arba vector3.
    //Y koordinate niekada nesikeicia nes robotas nejudes aukstyn/zemyn
    //Praejimo plotis yra 2

    //Dabartines koordinates. 
    float currentX;
    float currentZ;
    float currentY;

    //Iki kur reikia nueiti pries darant nauja aplinkos skenavima
    //Vienu metu bus naudojamas tik vienas is situ
    float targetX = 0;
    float targetZ = 0;

    //I kuria puse ziurim pagal North/South/East/West logika
    //Pagal sita labirinta jis prades ziuredamas north. Perdarius labirinta reiktu sita pakeist.
    Vector3 currentlyFacing = new Vector3(0, 0, 1);
    string currentlyFacingString = "n";

    //iki kokio laipsnou turi buti pasisukes kad butu pilnai atliktas posukis
    float currentRotation = 0;
    float targetRotation;

    //Robotas veiks pagal state machine principa
    string state = "move";

    // {bool, bool, bool} parodo ar galima judet i prieki/kaire/desine. True=laisvas kelias, false=siena.
    bool[] currentSurroundings = new bool[3];

    //Judejimo greitis
    public float moveSpeed = 1f;

    //Pasisukimo greitis
    public float rotateSpeed = 1f;

    //Kazkoks unity dalykas kuris reikalingas roboto judinimui
    public CharacterController controller;

    public Transform robotBody;

    void Start()
    {
        //prisiskiriam koordinaciu reiksmes
        currentX = transform.position.x;
        currentZ = transform.position.z;
        currentY = transform.position.y;

        //Testuojam ar gerai priskyre
        Debug.Log("Pradinis x: " + currentX);
        Debug.Log("Pradinis z: " + currentZ);

        //Gaunam pirma judejimo targeta
        getTarget(out targetX, out targetZ);
        //Debug.Log("tagetX" + targetX);
        //Debug.Log("tagetZ" + targetZ);

    }

    //Update() skaiciuoja frame'us pagal kompo specs, o FixedUpdate() turi preset frame laika. 
    //Naudojam fixed update kad ant visu kompu judetu tokiu paciu greiciu
    void FixedUpdate()
    {
        switch (state)
        {
            case "move":
                Debug.Log("moving");
                MoveForward();
                if (ReachedTheTarget())
                {
                    state = "scanning";
                }
                break;
            case "scanning":
                Debug.Log("scanning");
                currentSurroundings = CheckSurroundings();
                //Debug.Log("Kur galiu eiti? Priekis=" + currentSurroundings[0] + ", Kaire=" + currentSurroundings[1] + ", Desine=" + currentSurroundings[2]+". As esu x"+currentX+"  z"+currentZ+", as ziuriu i "+currentlyFacingString);
                //Cia bus gaunama kryptis, i kuria reiks pasisukt
               // Debug.Log("current rotation" + currentRotation);
                targetRotation = GetTargetRotation(currentRotation);
                //is anksto nustatom koks bus currentFacing kai baigsis pasisukimas
                ChangeCurrentFacing();
                //ChangeCurrentFacingModified();
                state = "rotate";
                break;
            case "rotate":
                Debug.Log("rotating");
                Rotate();
                if (ReachedRotationTarget(out currentRotation))
                {
                    //Debug.Log("DOne rotAtIng");
                    getTarget(out targetX, out targetZ);
                    //Debug.Log("Target x: " + targetX + "Target z" + targetZ);
                    //Debug.Log("CurentlyFacing " + currentlyFacingString);
                    state = "move";
                }
                break;
            default:
                Debug.Log("Nesu jokiame state, kazkas blogai");
                break;
        }
    }

    //Pagal dabartine krypti ir posukiu dydi (laipsniais) nustato kokia bus kryptis baigus posuki
    private void ChangeCurrentFacing()
    {
        switch (currentlyFacingString)
        {
            //Z asis i teigiama puse - NORTH, Vector3(0,0,1) arba Vector3.forward
            //Z asis i neigiama puse - SOUTH, Vector3(0,0,-1) arba Vector3.back
            //X asis i teigiama puse - EAST, Vector3(1,0,0) arba Vector3.right
            //X asis i neigiama puse - WEST, Vector3(-1,0,0) arba vector3.
            case "n":
                if (currentRotation-targetRotation>170||currentRotation-targetRotation<-170)
                {
                    currentlyFacingString = "s";
                    currentlyFacing = new Vector3(0, 0, -1);
                    return;
                }
                else if (Math.Abs(currentRotation)-Math.Abs(targetRotation)>=-1&& Math.Abs(currentRotation) - Math.Abs(targetRotation) <= 1)
                {
                    currentlyFacingString = "n";
                   currentlyFacing = new Vector3(0, 0, 1);
                    return;
                }
                else if (currentRotation>targetRotation)
                {
                    currentlyFacingString = "w";
                    currentlyFacing = new Vector3(-1, 0, 0);
                    return;
                }
                else if (currentRotation<targetRotation)
                {
                    currentlyFacingString = "e";
                    currentlyFacing = new Vector3(1, 0, 0);
                    return;
                }
                break;
            case "s":
                if (currentRotation - targetRotation > 170 || currentRotation - targetRotation < -170)
                {
                    currentlyFacingString = "n";
                    currentlyFacing = new Vector3(0, 0, 1);
                    return;
                }
                else if (Math.Abs(currentRotation) - Math.Abs(targetRotation) >= -1 && Math.Abs(currentRotation) - Math.Abs(targetRotation) <= 1)
                {
                    currentlyFacingString = "s";
                    currentlyFacing = new Vector3(0, 0, -1);
                    return;
                }
                else if (currentRotation > targetRotation)
                {
                    currentlyFacingString = "e";
                    currentlyFacing = new Vector3(1, 0, 0);
                    return;
                }
                else if (currentRotation < targetRotation)
                {
                    currentlyFacingString = "w";
                    currentlyFacing = new Vector3(-1, 0, 0);
                    return;
                }
                break;
            case "e":
                if (currentRotation - targetRotation > 170 || currentRotation - targetRotation < -170)
                {
                    currentlyFacingString = "w";
                    currentlyFacing = new Vector3(-1, 0, 0);
                    return;
                }
                else if (Math.Abs(currentRotation) - Math.Abs(targetRotation) >= -1 && Math.Abs(currentRotation) - Math.Abs(targetRotation) <= 1)
                {
                    currentlyFacingString = "e";
                    currentlyFacing = new Vector3(1, 0, 0);
                    return;
                }
                else if (currentRotation > targetRotation)
                {
                    currentlyFacingString = "n";
                    currentlyFacing = new Vector3(0, 0, 1);
                    return;
                }
                else if (currentRotation < targetRotation)
                {
                    currentlyFacingString = "s";
                    currentlyFacing = new Vector3(0, 0, -1);
                    return;
                }
                break;
            case "w":
                if (currentRotation - targetRotation > 170 || currentRotation - targetRotation < -170)
                {
                    currentlyFacingString = "e";
                    currentlyFacing = new Vector3(1, 0, 0);
                    return;
                }
                else if (Math.Abs(currentRotation) - Math.Abs(targetRotation) >= -1 && Math.Abs(currentRotation) - Math.Abs(targetRotation) <= 1)
                {
                    currentlyFacingString = "w";
                    currentlyFacing = new Vector3(-1, 0, 0);
                    return;
                }
                else if (currentRotation > targetRotation)
                {
                    currentlyFacingString = "s";
                    currentlyFacing = new Vector3(0, 0, -1);
                    return;
                }
                else if (currentRotation < targetRotation)
                {
                    currentlyFacingString = "n";
                    currentlyFacing = new Vector3(0, 0, 1);
                    return;
                }
                break;
            default:
                Debug.Log("Kazkas blogai ChangeCurrentFacing() metode");
                
                break;


        }
    }

    //Tikrina kada baigti sukimosi state
    private bool ReachedRotationTarget(out float currentRotation)
    {
        currentRotation = this.currentRotation;
        //Jeigu nereikia sukiotis arba pasisuko pakankamai
        //turetu buti == bet per 1 frame gali biski per daug pasisukt ir bus kokiu 0.0001 per daug, todel kai lieka maziau nei 1 laipsnis iki target uzskaito kaip true ir manually islygina skirtuma
        if (targetRotation - currentRotation <= 0.5 && targetRotation - currentRotation >= -0.5)
        {
            currentRotation = targetRotation;
            robotBody.eulerAngles = new Vector3(0, targetRotation, 0);

            return true;
        }

        return false;
    }
    //cia vyksta sukimasis
    //skaiciavimai + "animacija"
    private void Rotate()
    {
        
        if (targetRotation>currentRotation)
        {
            currentRotation += rotateSpeed * Time.deltaTime;
            robotBody.eulerAngles = new Vector3(0, currentRotation, 0);
        }
        else
        {
            currentRotation -= rotateSpeed * Time.deltaTime;
            robotBody.eulerAngles = new Vector3(0, currentRotation, 0);
        }
    }
    //Priejes kryzkele issirenka kur nores sukti, pagal suzino kiek laipsniu reiks pasisukt
    private float GetTargetRotation(float currentRotation)
    {
        
        if (currentSurroundings[1])
        {
           //jei bus galima i kaire, ir i desine, suks i kaire
            return currentRotation - 90;
        }
        else if (currentSurroundings[0])
        {
            //jeigu gali toliau judeti tiesiai tai niekur nesuks
            return currentRotation;
        }
        else if (currentSurroundings[2])
        {
            return currentRotation + 90;
        }
        else
        {
            //Jeigu pateko i aklaviete reikia apsisukti
            return currentRotation + 180;
        }
    }

    //Atstumo sensorius
    //Tikrina ar is sonu ir priekio yra kliuciu
    public bool[] CheckSurroundings()
    {
        bool[] surroundings = new bool[3];
        
        //Krypties vektoriai atsizvelgiant i dabartini roboto pasisukima
        Vector3 relativeFront = new Vector3();
        Vector3 relativeLeft = new Vector3();
        Vector3 relativeRight = new Vector3();

        GetRelativeVectors(out relativeFront, out relativeLeft, out relativeRight);
        //TESTAI
        //Debug.Log(relativeFront.x+"  "+ relativeFront.y+"  "+relativeFront.z);
        //Debug.Log(relativeLeft.x+"  "+ relativeLeft.y+"  "+ relativeLeft.z);
        //Debug.Log(relativeRight.x+"  "+ relativeRight.y+"  "+relativeRight.z);

        //Ar galiu judeti i prieki?
        surroundings[0] = !Physics.Raycast(robotBody.position, relativeFront, 2f);

        //Ar galiu judeti i kaire?
        surroundings[1] = !Physics.Raycast(robotBody.position, relativeLeft, 1.1f);

        //Ar galiu judeti i desine?
        surroundings[2] = !Physics.Raycast(robotBody.position, relativeRight, 1.1f);

        return surroundings;
    }

    //Nebeatsimenu nx sito reikejo bet cia pagal tai kas siuo metu robotui yra kaire ar desine pagamina vektorius
    private void GetRelativeVectors(out Vector3 relativeFront, out Vector3 relativeLeft, out Vector3 relativeRight)
    {
        switch (currentlyFacingString)
        {
            //Sita logika reiktu doublecheckinti nes galiu but klaidu padares.
            case "n":
                relativeFront = new Vector3(0, 0, 1);
                relativeLeft = new Vector3(-1, 0, 0);
                relativeRight = new Vector3(1, 0, 0);
                break;
            case "s":
                relativeFront = new Vector3(0, 0, -1);
                relativeLeft = new Vector3(1, 0, 0);
                relativeRight = new Vector3(-1, 0, 0);
                break;
            case "e":
                relativeFront = new Vector3(1, 0, 0);
                relativeLeft = new Vector3(0, 0, 1);
                relativeRight = new Vector3(0, 0, -1);
                break;
            case "w":
                relativeFront = new Vector3(-1, 0, 0);
                relativeLeft = new Vector3(0, 0, -1);
                relativeRight = new Vector3(0, 0, 1);
                break;
            default:
                Debug.Log("Kazkas blogai getRelativeVectors() metode");
                relativeFront = new Vector3(0, 0, 0);
                relativeLeft = new Vector3(0, 0, 0);
                relativeRight = new Vector3(0, 0, 0);
                break;


        }
    }

    //I like to move it move it
    public void MoveForward()
    {
        controller.Move(currentlyFacing * moveSpeed * Time.deltaTime);
    }

    //Pagal tai kokia kryptim judes pasirenka target koordinates
    //Kai pasieks targeta, skenuos is naujo
    public void getTarget(out float targetX, out float targetZ)
    {
        switch (currentlyFacingString)
        {
            case "n":
                targetZ = robotBody.position.z + 2;
                targetX = 0;
                break;
            case "s":
                targetZ = robotBody.position.z - 2;
                targetX = 0;
                break;
            case "e":
                targetX = robotBody.position.x + 2;
                targetZ = 0;
                break;
            case "w":
                targetX = robotBody.position.x -2;
                targetZ = 0;
                break;
            default:
                Debug.Log("Kazkas blogai getTarget() metode!!!");
                targetX = 0;
                targetZ = 0;
                break;
        }
       // Debug.Log("X: " + targetX + ", Z: " + targetZ);
    }

    //Tikrina ar pasieke targeta. Jeigu taip skenuos is naujo
    public bool ReachedTheTarget()
    {
        switch (currentlyFacingString)
        {
            case "n":
                if (robotBody.position.z>=targetZ)
                {
                    robotBody.position.Set(robotBody.position.x, robotBody.position.y, targetZ-targetZ%1);
                    currentX = robotBody.position.x - robotBody.position.x % 1;
                    currentZ = robotBody.position.z - robotBody.position.z % 1;
                    return true;
                }
                break;
            case "s":
                if (robotBody.position.z<=targetZ)
                {
                    robotBody.position.Set(robotBody.position.x, robotBody.position.y, targetZ-targetZ%1);
                    currentX = robotBody.position.x - robotBody.position.x % 1;
                    currentZ = robotBody.position.z - robotBody.position.z % 1;
                    return true;
                }
                break;
            case "e":
                if (robotBody.position.x >= targetX)
                {
                    robotBody.position.Set(targetX-targetX%1, robotBody.position.y, robotBody.position.z);
                    currentX = robotBody.position.x - robotBody.position.x % 1;
                    currentZ = robotBody.position.z - robotBody.position.z % 1;
                    return true;
                }
                break;
            case "w":
                if (robotBody.position.x <= targetX)
                {
                    robotBody.position.Set(targetX-targetX%1, robotBody.position.y, robotBody.position.z);
                    currentX = robotBody.position.x - robotBody.position.x % 1;
                    currentZ = robotBody.position.z - robotBody.position.z % 1;
                    return true;
                }
                break;
            default:
                Debug.Log("Kazkas blogai getTarget() metode!!!");
                targetX = 0;
                targetZ = 0;
                break;
        }
        return false;
    }

   

}
