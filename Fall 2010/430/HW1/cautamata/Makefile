

server : libraries
	dmcs -r:ICASettings.dll -r:IController.dll -pkg:wcf ./CAServer.cs ./CABoard.cs ./Controller.cs ./CACompiler.cs

libraries :
	dmcs -t:library ./ICASettings.cs
	dmcs -t:library -r:ICASettings.dll -pkg:wcf ./IController.cs

server3 : libraries3
	gmcs -r:ICASettings.dll -r:IController.dll -pkg:wcf ./CAServer.cs ./CABoard.cs ./Controller.cs ./CACompiler.cs

libraries3 :
	gmcs -t:library ./ICASettings.cs
	gmcs -t:library -r:ICASettings.dll -pkg:wcf ./IController.cs
