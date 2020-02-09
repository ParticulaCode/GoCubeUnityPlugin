using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Particula;
using Particula.Cube;
using Polymorph.Unity.MVVM;

public class GoCubeView : MonoBehaviour
{

    public string onlineCubeVmPath;
    public Text textOfCubeData;

    private CubeViewModel onlineVm;


    private void Start()
    {

        
        // Register to the GoCube rotations event
        if (!GoCubeProvider.GetProvider())
            return;

        /***** API example of how to register to the connected cube rotation events *****/
        GoCubeProvider.GetProvider().GetConnectedGoCube().afterRotation += RotationEvent;


        /***** Example of how to create a virtual cube on screen that mirroring the physical cube *****/
        // The CubeViewModel use the OnlineCube data and events to show the cube on screen
        onlineVm = new CubeViewModel(GoCubeProvider.GetProvider().GetConnectedGoCube());

        // Create a virtual cube view that is a mirror of the phisical cube with the view model
        ViewModelRegistry.DeclareProvider(onlineCubeVmPath, onlineVm);
    }

    private void OnDestroy()
    {
        if (onlineVm != null)
        {
            ViewModelRegistry.ClearProvider(onlineCubeVmPath, onlineVm);
        }
    }

    IEnumerator WaitFor(float time, Action cb)
    {
        while (time > 0)
        {
            time -= Time.deltaTime;
            yield return null;
        }
        cb();
    }



    /************ Samples for cube API functions (Using the UI buttons) ***********/

    // Open led different led patterns
    public void OpenLed(int ledPattern)
    {
        switch (ledPattern)
        {
            case 1:
                GoCubeProvider.GetProvider().GetConnectedGoCube().PlayLedPattern(LedPattern.Pattern1);
                break;
            case 2:
                GoCubeProvider.GetProvider().GetConnectedGoCube().PlayLedPattern(LedPattern.Pattern2);
                break;
            case 3:
                GoCubeProvider.GetProvider().GetConnectedGoCube().PlayLedPattern(LedPattern.Pattern3);
                break;
            case 4:
                GoCubeProvider.GetProvider().GetConnectedGoCube().PlayLedPattern(LedPattern.Pattern4);
                break;
            default:
                break;
        }
    }

    // Get the battery of the cube
    public void DisplayBatteryPercent()
    {
        // Get battery percentage
        float batteryPerc = GoCubeProvider.GetProvider().GetConnectedGoCube().batteryPercent;

        // Display to screen
        textOfCubeData.text = Mathf.RoundToInt((batteryPerc * 100)).ToString() + "%";
        DisplayTextOfCubeData();
    }

    // Check if the cube is solved
    public void DisplayIsSolved()
    {
        // Get if cube is on a sloved state
        var isSolved = GoCubeProvider.GetProvider().GetConnectedGoCube().IsSolved();

        // Display to screen
        if (isSolved)
        {
            textOfCubeData.text = "The cube is solved!";
        }
        else
        {
            textOfCubeData.text = "The cube is not solved!";
        }

        DisplayTextOfCubeData();
    }

    // Enable/Disable the IMU of the cube
    public void ChangeImuState(bool onImu)
    {
        if (onImu)
        {
            GoCubeProvider.GetProvider().GetConnectedGoCube().IMUState = true;
        }
        else
        {
            GoCubeProvider.GetProvider().GetConnectedGoCube().IMUState = false;
        }
    }

    // Check if the cube is an Edge cube
    public void GetCurrentQuaternionData()
    {
        // Example how to get the current GoCube IMU data (quaternion)
        Quat q = GoCubeProvider.GetProvider().GetConnectedGoCube().orientation;

        // Display the current cube quaternion value (x,y,z,w) on screen
        textOfCubeData.text = "x = " + q.x + ",\n y = " + q.y + ",\n z = " + q.z + ",\nw = " + q.w;
        DisplayTextOfCubeData();
    }

    private void DisplayTextOfCubeData()
    {
        textOfCubeData.gameObject.SetActive(true);
        StartCoroutine(WaitFor(3, DisableTextOfCubeData));
    }

    private void DisableTextOfCubeData()
    {
        textOfCubeData.gameObject.SetActive(false);
    }

    // When one of the cube faces will be rotated this function will be called
    // "rot" parameter is the current rotation of the physical cube
    private void RotationEvent(Rotation rot)
    {
        // Display the current rotation on the screen
        textOfCubeData.text = rot.ToString();
        DisplayTextOfCubeData();

        // You can do your stuff here....

    }
}
