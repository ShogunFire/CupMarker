using AutoIt;
using CupMarker.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;


namespace CupMarker.AutoItScript
{
    public static class EzCadAutoItScript
    {
 
        public static void DoTheJob(AutoScriptParam parameters)
        {
            // Activate the window
            AutoItX.WinActivate("EzCad");

            // if it's not active execute ezcad
            if (AutoItX.WinActive("EzCad") != 1)
            {
                AutoItX.Run(ConfigurationManager.AppSettings["EzCadExePath"], "", AutoItX.SW_SHOW);
                AutoItX.WinWait("EzCad", "", 5); // 10 sec timeout
                Thread.Sleep(4000);
                AutoItX.WinActivate("EzCad");
                if (AutoItX.WinActive("EzCad") != 1)
                {
                    return;
                }
            }
            // Open new  and say no to do you want to save
            AutoItX.Send("^n");
            AutoItX.Send("{TAB}");
            AutoItX.Send("{ENTER}");

            //open svg import with shortcut
            AutoItX.Send("^b");
            AutoItX.WinWaitActive("Ouvrir","",2);
            AutoItX.Send(parameters.SvgfilePath);
            AutoItX.Send("{ENTER}");
            AutoItX.WinWaitActive("EzCad","",2);

            // Center the svg
            AutoItX.ControlClick("EzCad", "", "Button201");


            //open transform menu

            AutoItX.MouseClick("left", 131, 29);
            AutoItX.MouseClick("left", 170, 100);
            AutoItX.WinWaitActive("Transform", "", 2);

            // rotate button
            AutoItX.ControlClick("Transform", "", "Button2");

            //rotate by center button
            AutoItX.ControlClick("Transform", "", "Button20");

            //set the angle
            AutoItX.ControlSetText("Transform", "", "Edit3", "-90");

            //Apply button
            AutoItX.ControlClick("Transform", "", "Button31");
            //quit the transform window
            AutoItX.Send("!{F4}");

            // Center the svg again
            AutoItX.ControlClick("EzCad", "", "Button201");

            //double click to select the text in x coordinate and put centerYInIt
            AutoItX.MouseClick("left", 50, 450);
            AutoItX.MouseClick("left", 50, 450);
            AutoItX.Send($"{parameters.CenterY:F1}");

            //Go to y coordinate en set 0
            AutoItX.Send("{TAB}");
            AutoItX.Send("0");

            //Go to size x and put height in it
            AutoItX.Send("{TAB}");
            AutoItX.Send($"{parameters.Height:F1}");

            // Open hatch and validate
            AutoItX.MouseClick("left", 428, 61);
            AutoItX.WinWaitActive("Hatch", "", 2);
            AutoItX.ControlClick("Hatch", "", "Button1");





        }

    }
}
