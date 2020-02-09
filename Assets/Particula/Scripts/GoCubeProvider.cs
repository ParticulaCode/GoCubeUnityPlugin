using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Particula;
using Particula.Bluetooth;
using UnityEngine.SceneManagement;
using UnityEngine.Android;
using System.Linq;

public class GoCubeProvider : MonoBehaviour
{
    static Rotation[] allRots = new Rotation[] {    Rotation.R, Rotation.L, Rotation.U, Rotation.D, Rotation.F, Rotation.B, Rotation.RTag, Rotation.LTag, Rotation.UTag,
                                                    Rotation.DTag, Rotation.FTag, Rotation.BTag };

    // This object must be placed in your project which implements the IBluetooth interface
    public BLEWrapper ble;
    public string onlineCubeVmPath;
    public Button connectingToCubeButtonPrefab;
    public RectTransform contentConnectingToCubeButtons; 
    public string nextSceneName;
    public GameObject thinkingIcon;
    public GameObject connectingStr;
    public GameObject connectionScreen;

    // Holds the list of all the available cubes the advertise thier bluetooth
    private Dictionary<Button, ICubeData> availableCubes = new Dictionary<Button, ICubeData>();

    // The object that holds the connected cube and always maintain the cube real state
    private IOnlineCube onlineCube = null;


    public static GoCubeProvider instance;


    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }



    private void Start()
    {
        if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
        {
            Permission.RequestUserPermission(Permission.FineLocation);
        }
    }


    // This function is called from the BLE object of the editor every time the BLE status is changed
    public void BLEStatusChanged(string status)
    {
        switch (status)
        {
            // The BLEWrapper script Awake function call the initialize function of the BLE,
            // Once it finished it should changed it state to "Powered On" with this call back
            case "Powered On":
                Particula.GoCube.Init(ble);
                GetAllAvailableCubes();
                break;

            case "Powered Off":
                Debug.Log("Bluetooth is turned off, please turn bluetooth on");
                break;
            case "Resetting":
                Debug.Log("Bluetooth is resetting");
                break;
            case "Unauthorized":
                Debug.Log("The app is not authorized to use bluetooth");
                break;
            case "Unknown":
                Debug.LogError("Unknown blutooth state change");
                break;
            case "Unsupported":
                Debug.Log("The device has no bluetooth, please use nother device");
                break;
        }
    }

    void GetAllAvailableCubes()
    {
        StartCoroutine(GetCubes(5));
    }

    IEnumerator GetCubes(float time)
    {
        IEnumerable<ICubeData> cubes = null;
        thinkingIcon.SetActive(true);
        contentConnectingToCubeButtons.gameObject.SetActive(false);

        // Get a list of all the available GoCubes in the area, once finished call the callback functions that sets the cubes list with the result
        Particula.GoCube.GetCubes(delegate (IEnumerable<ICubeData> c)
        {
            cubes = c;
        });

        while ((cubes == null) && (time > 0))
        {
            time -= Time.deltaTime;
            yield return null;
        }


        thinkingIcon.SetActive(false);
        contentConnectingToCubeButtons.gameObject.SetActive(true);

        // Create a connect button to each founded GoCube
        foreach (var c in cubes)
        {
            if ((c.name.Contains("GoCube")) && (availableCubes.Where(x=> x.Value.name == c.name)
                                                              .Select(x=> x.Value)
                                                              .ToList().Count == 0))
            {
                var newB = Instantiate(connectingToCubeButtonPrefab, contentConnectingToCubeButtons);
                var text = newB.GetComponentInChildren<Text>();
                text.text = c.name;
                var button = newB.GetComponent<Button>();
                availableCubes.Add(button, c);
                button.onClick.AddListener(() => OnClickButtonFromList(availableCubes[button]));
            }
        }
    }


    // Called when trying to connect to a specific cube
    private void OnClickButtonFromList(ICubeData cube)
    {
        connectingStr.SetActive(true);
        contentConnectingToCubeButtons.gameObject.SetActive(false);
        StopCoroutine(GetCubes(1));
        StartCoroutine(ConnectToCube(cube, 10));
    }

    IEnumerator ConnectToCube(ICubeData cube, float time)
    {

        Debug.Log("Start ConnectToCube");
        IOnlineCube onlineCube = null;

        // Try to connect the cube, once finished, call the callback functions that return the
        // OnlineCube object that holds all the corrisponding phisical GoCube information
        Particula.GoCube.ConnectToCube(cube, delegate (IOnlineCube c)
        {
            onlineCube = c;
        });

        while ((onlineCube == null) && (time > 0))
        {
            time -= Time.deltaTime;
            yield return null;
        }

        connectingStr.SetActive(false);

        if (onlineCube != null)
        {
            // Once holding the online cube object it is possible to call all the cube api functions
            this.onlineCube = onlineCube;

            // Hide connecting screen
            connectionScreen.SetActive(false);

            // Display the state on screen
            SceneManager.LoadScene(nextSceneName);
        }
        else
            contentConnectingToCubeButtons.gameObject.SetActive(true);
    }

    public void ReScan()
    {
        GetAllAvailableCubes();
    }

    public static GoCubeProvider GetProvider()
    {
        return instance;
    }

    public IOnlineCube GetConnectedGoCube()
    {
        return this.onlineCube;
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            SceneManager.LoadScene(nextSceneName);
        }
    }
}
