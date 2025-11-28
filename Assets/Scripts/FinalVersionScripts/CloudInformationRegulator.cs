using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CloudInformationRegulator : MonoBehaviour
{
    [SerializeField] PointCloudControls pointCloudControls;
    
    // all text objects in list: 
    // name, class, instanceID, coordinates, diameter, last inspection date, Inspector, Status 
    [SerializeField] List<TextMeshProUGUI> cloudInfoText = new List<TextMeshProUGUI>();

    public void ChangeInformation()
    {
        // Terrain
        if (pointCloudControls.classSelected[0] == true && pointCloudControls.classInstanceSelected[0] == true)
        {
            //name
            cloudInfoText[0].text = "Terrain";
            //class
            cloudInfoText[1].text = "1";
            //Instance ID
            cloudInfoText[2].text = "I77Ø11";
            //coordinates
            cloudInfoText[3].text = "(X0,Y0,Z0)";
            //Diameter
            cloudInfoText[4].text = "1000m";
            //Last Inspected
            cloudInfoText[5].text = "---";
            //Inspector
            cloudInfoText[6].text = "---";
            //Status
            cloudInfoText[7].text = "---";

            return;
        }
        // Top
        if (pointCloudControls.classSelected[1] == true && pointCloudControls.classInstanceSelected[0] == true)
        {
            //name
            cloudInfoText[0].text = "Structure Top";
            //class
            cloudInfoText[1].text = "2";
            //Instance ID
            cloudInfoText[2].text = "I89Å45";
            //coordinates
            cloudInfoText[3].text = "(X0,Y0,Z0)";
            //Diameter
            cloudInfoText[4].text = "10m";
            //Last Inspected
            cloudInfoText[5].text = "---";
            //Inspector
            cloudInfoText[6].text = "---";
            //Status
            cloudInfoText[7].text = "---";

            return;
        }
        // Walls
        if (pointCloudControls.classSelected[2] == true && pointCloudControls.classInstanceSelected[0] == true)
        {
            //name
            cloudInfoText[0].text = "Structure Walls";
            //class
            cloudInfoText[1].text = "3";
            //Instance ID
            cloudInfoText[2].text = "I22Ø99";
            //coordinates
            cloudInfoText[3].text = "(X0,Y0,Z0)";
            //Diameter
            cloudInfoText[4].text = "10m";
            //Last Inspected
            cloudInfoText[5].text = "---";
            //Inspector
            cloudInfoText[6].text = "---";
            //Status
            cloudInfoText[7].text = "---";

            return;
        }
        // Internal Tech - 1
        if (pointCloudControls.classSelected[3] == true && pointCloudControls.classInstanceSelected[0] == true)
        {
            //name
            cloudInfoText[0].text = "Debris Screen";
            //class
            cloudInfoText[1].text = "4";
            //Instance ID
            cloudInfoText[2].text = "I35Ø46";
            //coordinates
            cloudInfoText[3].text = "(X0,Y0,Z0)";
            //Diameter
            cloudInfoText[4].text = "3.55m";
            //Last Inspected
            cloudInfoText[5].text = "01-05-2025";
            //Inspector
            cloudInfoText[6].text = "Kim Kallestrup";
            //Status
            cloudInfoText[7].text = "Active";

            return;
        }
        // Internal Tech - 2
        if (pointCloudControls.classSelected[3] == true && pointCloudControls.classInstanceSelected[1] == true)
        {
            //name
            cloudInfoText[0].text = "Flow Control Gate";
            //class
            cloudInfoText[1].text = "4";
            //Instance ID
            cloudInfoText[2].text = "I36Ø56";
            //coordinates
            cloudInfoText[3].text = "(X0,Y0,Z0)";
            //Diameter
            cloudInfoText[4].text = "5m";
            //Last Inspected
            cloudInfoText[5].text = "01-05-2025";
            //Inspector
            cloudInfoText[6].text = "Kim Kallestrup";
            //Status
            cloudInfoText[7].text = "Active";

            return;
        }

        // Pipes - 1
        if (pointCloudControls.classSelected[4] == true && pointCloudControls.classInstanceSelected[0] == true)
        {
            //name
            cloudInfoText[0].text = "Inlet Outlet Pipes";
            //class
            cloudInfoText[1].text = "5";
            //Instance ID
            cloudInfoText[2].text = "O77V50";
            //coordinates
            cloudInfoText[3].text = "(X0,Y0,Z0)";
            //Diameter
            cloudInfoText[4].text = "1.25m - 4.22m";
            //Last Inspected
            cloudInfoText[5].text = "22-03-2019";
            //Inspector
            cloudInfoText[6].text = "Kent Andersen";
            //Status
            cloudInfoText[7].text = "Active";

            return;
        }
        // Pipes - 2
        if (pointCloudControls.classSelected[4] == true && pointCloudControls.classInstanceSelected[1] == true)
        {
            //name
            cloudInfoText[0].text = "Conduit Pipes";
            //class
            cloudInfoText[1].text = "5";
            //Instance ID
            cloudInfoText[2].text = "O88V56";
            //coordinates
            cloudInfoText[3].text = "(X0,Y0,Z0)";
            //Diameter
            cloudInfoText[4].text = "0.75m - 0.83m";
            //Last Inspected
            cloudInfoText[5].text = "22-03-2019";
            //Inspector
            cloudInfoText[6].text = "Kent Andersen";
            //Status
            cloudInfoText[7].text = "Active";

            return;
        }
        // Bottom - 1
        if (pointCloudControls.classSelected[5] == true && pointCloudControls.classInstanceSelected[0] == true)
        {
            //name
            cloudInfoText[0].text = "Retention Bassin";
            //class
            cloudInfoText[1].text = "6";
            //Instance ID
            cloudInfoText[2].text = "D36P10";
            //coordinates
            cloudInfoText[3].text = "(X0,Y0,Z0)";
            //Diameter
            cloudInfoText[4].text = "9.5m";
            //Last Inspected
            cloudInfoText[5].text = "01-07-2022";
            //Inspector
            cloudInfoText[6].text = "Gert Nielsen";
            //Status
            cloudInfoText[7].text = "Active";

            return;
        }
        // Bottom - 2
        if (pointCloudControls.classSelected[5] == true && pointCloudControls.classInstanceSelected[1] == true)
        {
            //name
            cloudInfoText[0].text = "Spillway";
            //class
            cloudInfoText[1].text = "6";
            //Instance ID
            cloudInfoText[2].text = "D90K44";
            //coordinates
            cloudInfoText[3].text = "(X0,Y0,Z0)";
            //Diameter
            cloudInfoText[4].text = "9.5m";
            //Last Inspected
            cloudInfoText[5].text = "01-07-2022";
            //Inspector
            cloudInfoText[6].text = "Gert Nielsen";
            //Status
            cloudInfoText[7].text = "Inactive";

            return;
        }
    }
}
