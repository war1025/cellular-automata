client : libraries
	gmcs -r:ClientUI.dll ./CAClient.cs

server : libraries
	gmcs -r:ICASettings.dll -r:IController.dll -pkg:wcf ./CAServer.cs ./CABoard.cs ./Controller.cs ./CACompiler.cs

libraries :
	gmcs -t:library ./ICASettings.cs
	gmcs -t:library -r:ICASettings.dll -pkg:wcf ./IController.cs
	gmcs -t:library -r:ICASettings.dll -r:IController.dll -pkg:wcf ./ClientController.cs
	gmcs -t:library -r:ICASettings.dll -r:ClientController.dll -r:System.Drawing ./ClientUI.cs ./CAParser.cs 
