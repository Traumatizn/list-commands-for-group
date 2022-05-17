using System;
using System.IO;
using System.Windows;
using System.Diagnostics;
using System.Threading;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

/** 
* Created by Traumatizn
*
* This script was created for Streamer.bot version "0.1.7"
*/
public class CPHInline
{	

	/** 
	* Instructions:
	* 
	* 1. Locate & copy the Streamer.bot directory's full dir path.
	* 2. Replace "{PARENT_DIRECTORY_ABSOLUTE_PATH}" within the "fullPathToSettingsJson" variable's value with the copied directory path from Step 1.
	* 3. Change the "groupTagName" variable's value to match the Command Groups that contain the commands to list. 
	* 4. If you get errors while compiling, make sure you have all the necessary References:
	*		- Microsoft.NET\Framework\{version}\mscorlib.dll
	*		- Microsoft.NET\Framework\{version}\System.dll
	*		- Microsoft.NET\Framework\{version}\WPF\PresentationCore.dll
	*/
	
	// TODO: Replace {PARENT_DIRECTORY_ABSOLUTE_PATH} with the absolute/full path to the parent directory that contains the Streamer.bot
	// 		 For example: C:\Users\Traumatizn\Desktop\Streamer.bot-0.1.7\data\settings.json
	// PROTIP: If your Streamer.bot directory has the version number in its name, you will need to update this code every time you update Streamer.bot. 
	// 		   I recommend making the directory name generic to avoid this additional work.
	private string fullPathToSettingsJson = @"{PARENT_DIRECTORY_ABSOLUTE_PATH}\data\settings.json";
	
	// TODO: Change the name of this value to match the Command Groups (not Action Groups in the "Actions" tab!) that contain the commands to list. 
	//		 This script will check the beginning of all Group names for this value.
	// 		 For example: If this value is "Mod Commands" and you have Command Groups named "Viewer Commands", "Mod Commands 1" and "Mod Commands 2", 
	//		 only the commands within "Mod Commands 1" and "Mod Commands 2" will be displayed. 
	private string groupTagName = "Commands";
	
	// OPTIONAL: Change this string value to whatever you want to call the list of commands to be displayed in chat.
	//			 For example: "Here is a list of all the commands:"
	private string beginningMessage = "";
	
	// Max character length for each individual message listing commands (also called a "page" for pagination). 
	// This can be increased/decreased to fit your needs.
	private int MAX_CHAR_LENGTH = 500;
	
	// OPTIONAL: Increase or decrease this value to increase/decrease the delay between messages sent (aka "pages")
	private int PAGINATION_DELAY = 1000; // In milliseconds

	public bool Execute()
	{
		CPH.LogInfo("Start");
		doAction();
		CPH.LogInfo("End");
		return true;
	}

	private void doAction()
	{
		string output = "";
		JObject jsonObject = JObject.Parse(File.ReadAllText(fullPathToSettingsJson));
		
		JArray commandArray = jsonObject["commandSettings"]["commands"] as JArray;
		Dictionary<int, string> pages = new Dictionary<int, string>();
		Dictionary<int, string> soundClips = new Dictionary<int, string>();
		int currentPage = 0;
		int currentSCPage = 0;
		
		pages[0] = beginningMessage + " ";
		foreach (JToken item in commandArray)
		{
			bool isEnabled = ((bool) (item["enabled"] as JValue).Value) == true;
			bool isGroupSupported = item["group"].ToString().StartsWith(groupTagName);
			if (isEnabled)
			{
				string command = extractCommands(item["command"].ToString()).Trim();
				
				if (isGroupSupported)
				{
					if ((pages[currentPage].Length + command.Length + 2) > MAX_CHAR_LENGTH)
					{
						output = output + command + ", ";
						currentPage++;
						pages[currentPage] = "";
					} 
					
					pages[currentPage] = pages[currentPage] + command + ", ";
				} 
			}
		}
		CPH.LogInfo("All Commands: " + output);

		for (int index = 0; index < pages.Count; index++)
		{
			// Removing the last comma and space from the last page
			CPH.SendMessage(pages[index].Substring(0, pages[index].Length - 2));
			Thread.Sleep(PAGINATION_DELAY);
		}
	}

	private string extractCommands(string command)
	{
		if (command.Contains("\r\n"))
		{
			return command.Replace("\r\n", "/");
		} else {
			return command;
		}
	}
}
