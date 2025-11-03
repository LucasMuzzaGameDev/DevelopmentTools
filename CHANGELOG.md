## [1.0.0] - 03/11/2025

### First Release

- Inserted a Console System that utilizes an Attribute "[Command]" to acess static and non-static commands
- Inserted a Querier Interface;
- Inserted a Command Querier;
- Inserted a base Command Attribute class;
- Inserted a Command Executor to execute all the commands within the console;
- Inserted a Command Parameter Parser to identify all the parameters relevants for an specific command;
- Inserted a Debug.Log Handler within the console, making it display the Debug.Log that are anywhere in the project;
- Inserted an MonobehaviorTargetType that will later be used to better target non-static methods;
- Inserted an EditorConsoleWindow script to handle all ui interactions, including actions like:
    - Suggestion Boxes;
    - Auto-Completing Commands;
    - Submission of Commands;
    - Click on Suggestion -> Auto-Completing Command;
    

### UI

- Inserted an Editor Window that used UI Documents to be created;
- Inserted StyleSheets to all texts and commands;
- Inserted suggestion boxes;
- Inserted input field;
- Inserted output view list;