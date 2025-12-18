using AutoIt;
using CupMarker.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Media.Media3D;


namespace CupMarker.AutoItScript
{
    public static class EzCadAutoItScript
    {
     
        public static IntPtr WinActiveByExe(string exeName)
        {

            // Normalize name (remove ".exe" if present)
            if (exeName.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                exeName = exeName.Substring(0, exeName.Length - 4);

            var processes = Process.GetProcessesByName(exeName);
            if (processes.Length == 0)
                return 0; // same as SetError(1, 0, 0)

            foreach (var proc in processes)
            {
                // Get all top-level windows owned by this process
                IntPtr mainWindow = proc.MainWindowHandle;
                if (mainWindow == IntPtr.Zero)
                    continue;


               
                AutoItX.WinActivate(mainWindow);
                return mainWindow;
                
                
            }

            return 0; 
        }




        public static void DoTheJob(AutoScriptParam parameters)
        {
            try
            {
                IntPtr ezCadWindow = 0;


                if (AutoItX.ProcessExists("EzCad2.exe") == 0)
                {
                    try
                    {
                        AutoItX.Run(ConfigurationManager.AppSettings["EzCadExePath"], "", AutoItX.SW_SHOW);
                        Thread.Sleep(1000);
                        AutoItX.Send("{ENTER}");


                        var start = DateTime.Now;
                        var textWindow = "";
                        // the text length condition is a hack to make sure we point to the right window, the full size window, there is probably a better way but I didn't find it
                        while ((ezCadWindow == 0 || textWindow.Length < 200) && (DateTime.Now - start).TotalSeconds < 15)
                        {
                            Thread.Sleep(500);  // Small delay to avoid CPU burn
                            ezCadWindow = WinActiveByExe("EzCad2.exe");
                            textWindow = AutoItX.WinGetText(ezCadWindow);

                        }
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show("Couldn't launch EzCad is EzCadExePath correct in the config ?");
                    }


                }
                else
                {
                    ezCadWindow = WinActiveByExe("EzCad2.exe");
                }

                if (ezCadWindow == 0 || AutoItX.WinWaitActive(ezCadWindow, 2) == 0)
                {
                    var titi = AutoItX.WinGetTitle(ezCadWindow);
                    MessageBox.Show("Couldn't put the focus on ezCad");
                    return;
                }
                var rectEzCadWindow = AutoItX.WinGetPos(ezCadWindow);

                //Close rotary mark window if open
                AutoItX.ControlClick("RotaryMark", "", "Button1");

                // Open new  and say no to do you want to save
                AutoItX.Send("^n");
                AutoItX.WinWaitActive("EzCad", "Yes", 2);
                AutoItX.Send("{TAB}");
                AutoItX.Send("{ENTER}");

                //open svg import with shortcut
                AutoItX.Send("^b");
                AutoItX.WinWaitActive("Open", "", 2);
                AutoItX.Send(parameters.DxfFilePath);
                AutoItX.Send("{ENTER}");
                AutoItX.WinWaitActive(ezCadWindow, 2);

                // Center the svg


                AutoItX.ControlClick(ezCadWindow, AutoItX.ControlGetHandle(ezCadWindow, "Button201"));


                //open transform menu

                int xModifyMenuItem = Int32.Parse(ConfigurationManager.AppSettings["xModifyMenuItem"]);
                int yModifyMenuItem = Int32.Parse(ConfigurationManager.AppSettings["yModifyMenuItem"]);
                int xTransformMenuItem = Int32.Parse(ConfigurationManager.AppSettings["xTransformMenuItem"]);
                int yTransformMenuItem = Int32.Parse(ConfigurationManager.AppSettings["yTransformMenuItem"]);
                AutoItX.MouseClick("left", rectEzCadWindow.X + xModifyMenuItem, rectEzCadWindow.Y + yModifyMenuItem + 10);
                AutoItX.MouseClick("left", rectEzCadWindow.X + xTransformMenuItem, rectEzCadWindow.Y + yTransformMenuItem + 10);
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


                AutoItX.WinWaitActive(ezCadWindow, 2);
                // Center the svg again
                AutoItX.ControlClick(ezCadWindow, AutoItX.ControlClick(ezCadWindow, AutoItX.ControlGetHandle(ezCadWindow, "Button201")));


                //open transform menu again
                AutoItX.MouseClick("left", rectEzCadWindow.X + xModifyMenuItem, rectEzCadWindow.Y + yModifyMenuItem + 10);
                AutoItX.MouseClick("left", rectEzCadWindow.X + xTransformMenuItem, rectEzCadWindow.Y + yTransformMenuItem + 10);
                AutoItX.WinWaitActive("Transform", "", 2);


                //click on position button
                AutoItX.ControlClick("Transform", "", "Button1");

                //position by center button


                // This is a hack because it cannot click on the center button, not sure why
                // so we find the position of transform window and then click at the right coordinate 
                var rectangleWindow = AutoItX.WinGetPos("Transform");
                AutoItX.MouseClick("left", rectangleWindow.X + 59, rectangleWindow.Y + 277);

                //set the position x
                AutoItX.ControlSetText("Transform", "", "Edit1", $"{parameters.CenterY:F1}");
                //set the position y
                AutoItX.ControlSetText("Transform", "", "Edit2", (parameters.PartDiameter* 1.57).ToString("F1"));
                //Apply button
                AutoItX.ControlClick("Transform", "", "Button18");


                //go to size tab
                AutoItX.ControlClick("Transform", "", "Button4");

                //size by center button
                AutoItX.ControlClick("Transform", "", "Button48");


                //set the size x, this a hack because using ControlSetText directly doesn't recalculate the size y 
                // we need to use Send, we empty the input before because the double click doesn't manage to select the former value
                AutoItX.ControlSetText("Transform", "", "Edit8", "");
                AutoItX.ControlClick("Transform", "", "Edit8");

                AutoItX.Send($"{parameters.Height:F1}");
                //Apply button
                AutoItX.ControlClick("Transform", "", "Button58");


                //quit the transform window
                AutoItX.Send("!{F4}");


                AutoItX.WinWaitActive(ezCadWindow, 2);

                // Open hatch and validate
                int xHatchButton = Int32.Parse(ConfigurationManager.AppSettings["xHatchButton"]);
                int yHatchButton = Int32.Parse(ConfigurationManager.AppSettings["yHatchButton"]);
                AutoItX.MouseClick("left", rectEzCadWindow.X + xHatchButton, rectEzCadWindow.Y + yHatchButton + 10);
                AutoItX.WinWaitActive("Hatch", "", 2);
                AutoItX.ControlClick("Hatch", "", "Button1");

                Thread.Sleep(1000);

                //Open RotaryMark
                int xLazerMenuItem = Int32.Parse(ConfigurationManager.AppSettings["xLazerMenuItem"]);
                int yLazerMenuItem = Int32.Parse(ConfigurationManager.AppSettings["yLazerMenuItem"]);
                AutoItX.MouseClick("left", rectEzCadWindow.X + xLazerMenuItem, rectEzCadWindow.Y + yLazerMenuItem + 10);

                int xRotaryMarkMenuItem = Int32.Parse(ConfigurationManager.AppSettings["xRotaryMarkMenuItem"]);
                int yRotaryMarkMenuItem = Int32.Parse(ConfigurationManager.AppSettings["yRotaryMarkMenuItem"]);

                AutoItX.MouseClick("left", rectEzCadWindow.X + xRotaryMarkMenuItem, rectEzCadWindow.Y + yRotaryMarkMenuItem + 10);

                AutoItX.WinWaitActive("RotaryMark", "", 2);

                AutoItX.ControlSetText("RotaryMark", "", "Edit3", "0.100");

                bool continousIsCkecked  = AutoItX.ControlCommand("RotaryMark", "", "Button2", "IsChecked", "").Equals("1");
                if (continousIsCkecked)
                {
                    AutoItX.ControlCommand("RotaryMark", "", "Button2", "UnCheck", "");
                }

                bool markSelectedIsCkecked = AutoItX.ControlCommand("RotaryMark", "", "Button5", "IsChecked", "").Equals("1");
                if (markSelectedIsCkecked)
                {
                    AutoItX.ControlCommand("RotaryMark", "", "Button5", "UnCheck", "");
                }

                bool forceAllSplitIsCkecked = AutoItX.ControlCommand("RotaryMark", "", "Button10", "IsChecked", "").Equals("1");
                if (!forceAllSplitIsCkecked)
                {
                    AutoItX.ControlCommand("RotaryMark", "", "Button10", "Check", "");
                }

                bool markBySplitLineIsCkecked = AutoItX.ControlCommand("RotaryMark", "", "Button12", "IsChecked", "").Equals("1");
                if (markBySplitLineIsCkecked)
                {
                    AutoItX.ControlCommand("RotaryMark", "", "Button12", "UnCheck", "");
                }

                AutoItX.ControlClick("RotaryMark", "", "Button11");
                AutoItX.WinWaitActive("Part Diameter", "", 2);
                AutoItX.ControlSetText("Part Diameter", "", "Edit1", parameters.PartDiameter.ToString("F1"));
                AutoItX.ControlClick("Part Diameter", "", "Button1");

                


            }
            catch (Exception e)
            {
                MessageBox.Show("An error occurred while automating EzCad: " + e.Message);

            }
        }

    }
}
