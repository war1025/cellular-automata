
client : libraries
	dmcs -r:ICASettings.dll -r:IController.dll -r:ClientController.dll -pkg:wcf ./CAClient.cs ./CAParser.cs

server : libraries
	dmcs -r:ICASettings.dll -r:IController.dll -pkg:wcf ./CAServer.cs ./CABoard.cs ./Controller.cs ./CACompiler.cs

libraries :
	dmcs -t:library ./ICASettings.cs
	dmcs -t:library -r:ICASettings.dll -pkg:wcf ./IController.cs
	dmcs -t:library -r:ICASettings.dll -r:IController.dll -pkg:wcf ./ClientController.cs

client3 : libraries3
	gmcs -r:ICASettings.dll -r:IController.dll -r:ClientController.dll -r:System.Drawing -pkg:wcf ./CAClient.cs ./CAParser.cs

server3 : libraries3
	gmcs -r:ICASettings.dll -r:IController.dll -pkg:wcf ./CAServer.cs ./CABoard.cs ./Controller.cs ./CACompiler.cs

libraries3 :
	gmcs -t:library ./ICASettings.cs
	gmcs -t:library -r:ICASettings.dll -pkg:wcf ./IController.cs
	gmcs -t:library -r:ICASettings.dll -r:IController.dll -pkg:wcf ./ClientController.cs
	gmcs -t:library -r:ICASettings.dll -r:ClientController.dll -r:System.Drawing ./ClientUI.cs ./CAParser.cs 
