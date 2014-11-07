REM ********************************************************************************************************************
REM * Hi-Jack the Auto Build Variables by QtAgent since this is injected after it has REM * setup
REM * Open the autogenerated qtREM * setup in the test run location of
REM * C:\Users\IntegrationTester\AppData\Local\VSEQT\QTAgent\...
REM * For example:
REM * set DeploymentDirectory=C:\Users\INTEGR~1\AppData\Local\VSEQT\QTAgent\54371B~1\RSAKLF~1\DEPLOY~1
REM * set TestRunDirectory=C:\Users\INTEGR~1\AppData\Local\VSEQT\QTAgent\54371B~1\RSAKLF~1
REM * set TestRunResultsDirectory=C:\Users\INTEGR~1\AppData\Local\VSEQT\QTAgent\54371B~1\RSAKLF~1\Results\RSAKLF~1
REM * set TotalAgents=5
REM * set AgentWeighting=100
REM * set AgentLoadDistributor=Microsoft.VisualStudio.TestTools.Execution.AgentLoadDistributor
REM * set AgentId=1
REM * set TestDir=C:\Users\INTEGR~1\AppData\Local\VSEQT\QTAgent\54371B~1\RSAKLF~1
REM * set ResultsDirectory=C:\Users\INTEGR~1\AppData\Local\VSEQT\QTAgent\54371B~1\RSAKLF~1\Results
REM * set DataCollectionEnvironmentContext=Microsoft.VisualStudio.TestTools.Execution.DataCollectionEnvironmentContext
REM * set TestLogsDir=C:\Users\INTEGR~1\AppData\Local\VSEQT\QTAgent\54371B~1\RSAKLF~1\Results\RSAKLF~1
REM * set ControllerName=rsaklfsvrtfsbld:6901
REM * set TestDeploymentDir=C:\Users\INTEGR~1\AppData\Local\VSEQT\QTAgent\54371B~1\RSAKLF~1\DEPLOY~1
REM * set AgentName=RSAKLFTST7X64-3
REM ********************************************************************************************************************

taskkill /im "Warewolf Studio.exe"
IF EXIST %TestRunDirectory%\..\..\..\nircmd.exe %TestRunDirectory%\..\..\..\nircmd.exe elevate taskkill /im "Warewolf Studio.exe"
sc STOP "Warewolf Server"
%DeploymentDirectory%\ServerbinDebug\Dev2.Server.exe -x
taskkill /im "Warewolf Server.exe"
IF EXIST %TestRunDirectory%\..\..\..\nircmd.exe %TestRunDirectory%\..\..\..\nircmd.exe elevate taskkill /im "Warewolf Server.exe"
exit 0