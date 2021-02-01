# Fictional-Assembly-Parser
 This program parses a language made up by me, inspired by basic assembly.

 If you wanna try it out, fork+clone and open the `.sln` file with Rider or Visual Studio. 

 This requires the .NET SDK v3.1 or above to run.
 You can then Run or Debug it in the IDE.
 
# Commands

#### begin
Denotes the start of the module.

#### end
Denotes the end of the module.

#### def
Defines an alias to a memory address.

#### ld
Loads a value into accumulator.

#### sav
Stores accumulated data into a memory address.

#### print
Logs accumulator value to the terminal.

#### add
Adds a number to accumulator.

#### sub
Subtracts a number from accumulator.

#### mul
Multiplies a number to accumulator.

#### div
Divides the accumulator by a given non-zero number.

#### setf
Sets a specified flag.

#### rstf
Resets a specified flag.

#### proc
Calls a previously declared procedure.

#### endproc
Returns control to parent procedure.