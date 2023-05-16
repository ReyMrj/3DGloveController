using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using WebSocketSharp;

public class SensorDataVisualizer : MonoBehaviour
{
    // Object references
    public Transform thumbJoint;
    public Transform indexJoint;
    public Transform middleJoint;
    public Transform ringJoint;
    public Transform pinkyJoint;
    public GameObject gloveModel;

    // WebSocket variables
    private WebSocket ws;
    public string serverURL = "ws://localhost:3000";

    // Variables to hold sensor values
    private float piezoValue;
    private float[] flexValues = new float[5];
    private float[] mpuValues = new float[3];

    // Function to parse incoming sensor data
    private void ParseSensorData(string jsonData)
    {
        // Deserialize JSON string into object
        SensorData data = JsonUtility.FromJson<SensorData>(jsonData);

        // Extract values for each sensor
        piezoValue = data.piezo_sensor_value;
        flexValues = data.flex_sensor_values;
        mpuValues = new float[] { data.mpu6050_x_axis, data.mpu6050_y_axis, data.mpu6050_z_axis };
    }

    // Function to calculate joint angles based on sensor data
    private Vector3 CalculateJointAngles(float[] flexValues, float[] mpuValues)
    {
        // Implement your joint angle calculation logic here
        // Return Euler angles as a Vector3 object
        
    // Calculate finger bend angles based on flex sensor values
    float thumbBend = flexValues[0];
    float indexBend = flexValues[1];
    float middleBend = flexValues[2];
    float ringBend = flexValues[3];
    float pinkyBend = flexValues[4];

    // Calculate hand rotation based on MPU6050 values
    float handRoll = mpuValues[0];
    float handPitch = mpuValues[1];
    float handYaw = mpuValues[2];

    // Calculate joint angles based on finger bend and hand rotation values
    Vector3 jointAngles = new Vector3(
        -thumbBend,
        indexBend,
        middleBend
    );

    // Add hand rotation to joint angles
    jointAngles += new Vector3(
        handPitch,
        -handYaw,
        handRoll
    );

    return jointAngles;
}

    

    // Start WebSocket connection
    private void StartWebSocket()
    {
        ws = new WebSocket(serverURL);
        ws.OnMessage += (sender, e) => {
            ParseSensorData(e.Data);
        };
        ws.Connect();
    }

    // Close WebSocket connection
    private void CloseWebSocket()
    {
        if (ws != null)
        {
            ws.Close();
            ws = null;
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Calculate joint angles based on sensor data
        Vector3 jointAngles = CalculateJointAngles(flexValues, mpuValues);

        // Set rotation of glove model joints
        thumbJoint.localRotation = Quaternion.Euler(jointAngles.x, jointAngles.y, 0f);
        indexJoint.localRotation = Quaternion.Euler(jointAngles.x, jointAngles.y, jointAngles.z);
        middleJoint.localRotation = Quaternion.Euler(jointAngles.x, jointAngles.y, jointAngles.z);
        ringJoint.localRotation = Quaternion.Euler(jointAngles.x, jointAngles.y, jointAngles.z);
        pinkyJoint.localRotation = Quaternion.Euler(jointAngles.x, jointAngles.y, jointAngles.z);
    }

    // Clean up WebSocket connection when the script is destroyed
    private void OnDestroy()
    {
        CloseWebSocket();
    }
}

// Class to hold sensor data
[System.Serializable]
public class SensorData
{
    public float piezo_sensor_value;
    public float[] flex_sensor_values;
    public float mpu6050_x_axis;
    public float mpu6050_y_axis;
    public float mpu6050_z_axis;
}
