#Models

InputStreamstring,($binary)
application/octet_stream body support

reason{
description:	
reason for failure

reason*	string
reason for failure

}
scenes[
definition: Scene list
minItems: 1
uniqueItems: truestring]
scene_definition{
description:	
Scene Payload Body Structure

prj*	{...}
marker	{...}
list	[...]
remove	[...]
}
job{
description:	
Prepare scene assets

input*	{...}
output*	{...}
}
job_state{
description:	
dd

result*	string
default: created
reporting success status

Enum:
Array [ 2 ]
urn*	string
the urn identifier of the source file

acceptedJobs"	{...}
}